// Areas/social_hub/Services/MuteFilter.cs
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GameSpace.Areas.social_hub.Services
{
	public sealed class MuteFilter : IMuteFilter
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IMemoryCache _cache;
		private readonly MuteFilterOptions _opt;

		private const string CK_REGEX = "social_hub.mutes.regex";
		private const string CK_WORDS = "social_hub.mutes.words";
		private static readonly TimeSpan CACHE_TTL = TimeSpan.FromMinutes(30);

		private Regex? _compiled;
		private string[] _words = Array.Empty<string>();

		public MuteFilter(GameSpacedatabaseContext db, IMemoryCache cache, IOptions<MuteFilterOptions> opt)
		{
			_db = db;
			_cache = cache;
			_opt = opt.Value;

			_compiled = _cache.Get<Regex>(CK_REGEX);
			_words = _cache.Get<string[]>(CK_WORDS) ?? Array.Empty<string>();
		}

		public async Task RefreshAsync(CancellationToken ct = default)
		{
			var list = await _db.Mutes.AsNoTracking()
				.Where(m => m.IsActive == true && m.MuteName != null && m.MuteName != "")
				.OrderByDescending(m => m.MuteId)
				.Select(m => m.MuteName!)
				.Distinct()
				.ToListAsync(ct);

			_words = list.ToArray();
			_compiled = BuildRegex(_words, _opt);

			_cache.Set(CK_WORDS, _words, CACHE_TTL);
			if (_compiled != null) _cache.Set(CK_REGEX, _compiled, CACHE_TTL);
		}

		public string Apply(string input)
		{
			if (string.IsNullOrEmpty(input)) return input;
			var rx = _compiled ?? BuildFromCacheOrDb();
			if (rx == null) return input;
			return rx.Replace(input, Mask);
		}

		public Task<string> ApplyAsync(string input, CancellationToken ct = default)
			=> Task.FromResult(Apply(input));

		// === 舊名稱相容 ===
		public string Filter(string input) => Apply(input);
		public Task<string> FilterAsync(string input, CancellationToken ct = default)
			=> ApplyAsync(input, ct);

		// --- 內部 ---

		private Regex? BuildFromCacheOrDb()
		{
			if (_compiled != null) return _compiled;

			var cached = _cache.Get<string[]>(CK_WORDS);
			if (cached == null || cached.Length == 0)
			{
				var loaded = _db.Mutes.AsNoTracking()
					.Where(m => m.IsActive == true && m.MuteName != null && m.MuteName != "")
					.Select(m => m.MuteName!)
					.Distinct()
					.ToList();
				_words = loaded.ToArray();
			}
			else
			{
				_words = cached;
			}

			_compiled = BuildRegex(_words, _opt);
			if (_compiled != null) _cache.Set(CK_REGEX, _compiled, CACHE_TTL);
			return _compiled;
		}

		private static Regex? BuildRegex(System.Collections.Generic.IEnumerable<string> words, MuteFilterOptions opt)
		{
			var list = words.Where(w => !string.IsNullOrWhiteSpace(w))
							.Select(w => w.Trim())
							.Distinct()
							.ToList();
			if (list.Count == 0) return null;

			string BuildPattern(string w)
			{
				if (!opt.FuzzyBetweenCjkChars || w.Length <= 1)
					return Regex.Escape(w);

				var sb = new StringBuilder();
				for (int i = 0; i < w.Length; i++)
				{
					sb.Append(Regex.Escape(w[i].ToString()));
					if (i < w.Length - 1)
						// 允許的干擾字元集合
						sb.Append(@"[\s\p{P}\p{Sk}\p{Mn}\u200B\u200C\u200D\u2060_\-]*");

				}
				return sb.ToString();
			}

			var union = "(" + string.Join("|", list.Select(BuildPattern)) + ")";
			return new Regex(union, RegexOptions.Compiled | RegexOptions.IgnoreCase);
		}

		private string Mask(Match m)
			=> _opt.MaskStyle == MaskStyle.FixedLabel ? _opt.FixedLabel : new string('＊', m.Value.Length);
	}
}
