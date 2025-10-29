using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GamiPort.Areas.social_hub.Services.Abstractions
{
	/// <summary>
	/// 穢語遮蔽服務：規則來自 dbo.Mutes；DB 保留原文，僅改顯示。
	/// </summary>
	public interface IProfanityFilter
	{
		/// <summary>從資料庫重新載入規則並重建快取（執行緒安全）。</summary>
		Task ReloadAsync(CancellationToken ct = default);

		/// <summary>僅改顯示文字，不改 DB；可傳入 null。</summary>
		string Censor(string? text);

		/// <summary>
		/// 提供給前端做本地遮蔽用的規則（正則字串 + flags + replacement）。
		/// 回傳： (版本, 規則清單)，版本可用於快取控制。
		/// </summary>
		(long version, IReadOnlyList<(string pattern, string? replacement, string flags)> rules) GetClientRules();
	}
}
