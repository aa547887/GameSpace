using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameSpace.Areas.social_hub.Services
{
	public class MuteConfig
	{
		public Regex? Pattern { get; set; }
		public DateTime BuiltAt { get; set; } = DateTime.UtcNow;
	}

	public interface IMuteFilter
	{
		Task<string> FilterAsync(string input);
		Task RefreshAsync();
	}

	public class MuteFilter : IMuteFilter
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IMemoryCache _cache;
		private const string CacheKey = "MuteFilter.Pattern";
		private const int CacheMinutes = 5;

		public static readonly string DefaultReplacement = "█";

		private static readonly Regex ZeroWidth = new Regex(
			"[\\u200B-\\u200F\\u202A-\\u202E\\u2060\\uFEFF]",
			RegexOptions.Compiled
		);

		public MuteFilter(GameSpacedatabaseContext db, IMemoryCache cache)
		{
			_db = db;
			_cache = cache;
		}

		private static string Fuzzy(string word)
		{
			if (string.IsNullOrWhiteSpace(word)) return string.Empty;
			var src = word.Normalize(NormalizationForm.FormKC);
			var parts = src.Select(ch => Regex.Escape(ch.ToString()));
			return string.Join(@"(?:\W|_|\p{Zs}|\p{Cf}){0,2}", parts);
		}

		private async Task<MuteConfig> BuildAsync()
		{
			var words = await _db.Mutes
				.AsNoTracking()
				.Where(m => m.IsActive)
				.Select(m => m.MuteName)
				.ToListAsync();

			if (words == null || words.Count == 0)
				return new MuteConfig { Pattern = null };

			var patterns = words.Select(Fuzzy).Where(p => !string.IsNullOrEmpty(p));
			var patternJoined = string.Join("|", patterns);

			var regex = new Regex(
				patternJoined,
				RegexOptions.Compiled | RegexOptions.CultureInvariant |
				RegexOptions.IgnoreCase | RegexOptions.Singleline
			);

			return new MuteConfig { Pattern = regex, BuiltAt = DateTime.UtcNow };
		}

		private Task<MuteConfig> GetOrBuildAsync()
		{
			if (_cache.TryGetValue(CacheKey, out MuteConfig cfg) && cfg != null)
				return Task.FromResult(cfg);

			return Task.Run(async () =>
			{
				var built = await BuildAsync();
				_cache.Set(CacheKey, built, TimeSpan.FromMinutes(CacheMinutes));
				return built;
			});
		}

		public async Task<string> FilterAsync(string input)
		{
			if (string.IsNullOrEmpty(input)) return input ?? string.Empty;
			var cfg = await GetOrBuildAsync();
			if (cfg.Pattern == null) return input;

			var normalized = input.Normalize(NormalizationForm.FormKC);
			normalized = ZeroWidth.Replace(normalized, "");
			return cfg.Pattern.Replace(normalized, _ => DefaultReplacement);
		}

		public async Task RefreshAsync()
		{
			var cfg = await BuildAsync();
			_cache.Set(CacheKey, cfg, TimeSpan.FromMinutes(CacheMinutes));
		}
	}
}
