using GamiPort.Areas.social_hub.Services.Abstractions;  // ← 只保留這個介面
using GamiPort.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace GamiPort.Areas.social_hub.Services.Application
{
	/// <summary>
	/// 穢語遮蔽服務（Singleton）。
	/// 重要：不直接吃 DbContext（Scoped），改吃 IServiceScopeFactory 建 Scope 取得 DbContext。
	/// 這樣就能在 Singleton 中安全使用 EF Core。
	/// </summary>
	public sealed class ProfanityFilter : IProfanityFilter
	{
		private readonly IServiceScopeFactory _scopeFactory;

		// 編譯後規則（伺服器端）
		private sealed class CompiledRule
		{
			public Regex Re { get; init; } = default!;
			public string Replacement { get; init; } = "**";
		}

		// 不可變快照；用 Interlocked.Exchange 切換
		private sealed class Snapshot
		{
			public long Version { get; }
			public IReadOnlyList<CompiledRule> Rules { get; }
			public Snapshot(long v, IReadOnlyList<CompiledRule> r) { Version = v; Rules = r; }
		}

		private Snapshot _snap = new Snapshot(0, Array.Empty<CompiledRule>());

		public ProfanityFilter(IServiceScopeFactory scopeFactory)
		{
			_scopeFactory = scopeFactory;
		}

		/// <summary>
		/// 重新載入規則：讀 dbo.Mutes（IsActive=1），把 Word 轉為 literal 正則，replacement 預設 **。
		/// Version 用 UtcNow.Ticks，讓前端可用來判斷是否新版。
		/// </summary>
		public async Task ReloadAsync(CancellationToken ct = default)
		{
			// ★ 在 Singleton 內建立「短命」Scope，從 Scope 解析 DbContext（Scoped）
			using var scope = _scopeFactory.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<GameSpacedatabaseContext>();

			var rows = await db.Mutes
				.AsNoTracking()
				.Where(m => m.IsActive == true)
				.Select(m => new { m.Word, m.Replacement })
				.ToListAsync(ct);

			var compiled = new List<CompiledRule>(rows.Count);
			foreach (var r in rows)
			{
				if (string.IsNullOrWhiteSpace(r.Word)) continue;

				// 將字串詞彙轉為 literal 的正則；旗標：IgnoreCase + CultureInvariant
				var pattern = Regex.Escape(r.Word.Trim());
				var rx = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

				compiled.Add(new CompiledRule
				{
					Re = rx,
					Replacement = string.IsNullOrWhiteSpace(r.Replacement) ? "**" : r.Replacement!.Trim()
				});
			}

			var newSnap = new Snapshot(DateTime.UtcNow.Ticks, compiled);
			Interlocked.Exchange(ref _snap, newSnap);
		}

		/// <summary>
		/// 回給前端做本地遮蔽的規則（正則字串 + flags + replacement）。
		/// pattern 為 literal（Regex.Escape 過），flags 固定 "gi"。
		/// </summary>
		public (long version, IReadOnlyList<(string pattern, string? replacement, string flags)> rules) GetClientRules()
		{
			var s = _snap;
			var list = s.Rules
				.Select(r => (pattern: r.Re.ToString(), replacement: (string?)r.Replacement, flags: "gi"))
				.ToList()
				.AsReadOnly();

			return (s.Version, list);
		}

		/// <summary>
		/// 僅改顯示不改 DB：依目前快照將文字做替換。
		/// </summary>
		public string Censor(string? text)
		{
			if (string.IsNullOrEmpty(text)) return text ?? string.Empty;
			var s = _snap;
			var output = text;
			foreach (var r in s.Rules)
				output = r.Re.Replace(output, r.Replacement);
			return output;
		}
	}
}
