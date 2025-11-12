using GamiPort.Areas.social_hub.SignalR.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GamiPort.Areas.social_hub.Services.Abstractions
{
	/// <summary>
	/// DM 業務邏輯（EF 讀寫集中在這）
	/// </summary>
	public interface IChatService
	{
		/// <summary>
		/// 傳送一則一對一訊息（會自動建立對話、裁切文字長度、回傳前端要用的 payload）。
		/// </summary>
		Task<DirectMessagePayload> SendDirectAsync(int senderId, int recipientId, string content, CancellationToken ct = default);

		/// <summary>
		/// 讀取歷史訊息（afterUtc 之後；或未帶 afterUtc 時回傳最新 N 筆）。
		/// </summary>
		Task<IReadOnlyList<DirectMessagePayload>> GetHistoryAsync(int userId, int otherId, DateTime? afterUtc, int pageSize, CancellationToken ct = default);

		/// <summary>
		/// 回傳 (全站未讀總數, 與指定對象的未讀數)。
		/// </summary>
		Task<(int total, int peer)> ComputeUnreadAsync(int userId, int peerId, CancellationToken ct = default);

		/// <summary>
		/// 僅回傳全站未讀總數。
		/// </summary>
		Task<int> ComputeTotalUnreadAsync(int userId, CancellationToken ct = default);

		/// <summary>
		/// [新] 標記「我(meUserId) 與 對方(otherUserId)」該對話中，
		///     對方傳給我的所有【未讀】訊息為已讀。回傳實際更新行數。
		///     【說明】為了避免欄位差異，本版 upTo 參數為可選；若要精準到某時間點，服務會以 EditedAt <= upToUtc 套用條件。
		/// </summary>
		Task<int> MarkReadUpToAsync(int meUserId, int otherUserId, DateTime? upToUtc = null, CancellationToken ct = default);

		/// <summary>
		/// [舊] 舊版供相容呼叫（忽略回傳）。建議改用 MarkReadUpToAsync。
		/// </summary>
		[Obsolete("Use MarkReadUpToAsync instead, which returns rowsAffected.")]
		Task MarkReadAsync(int meUserId, int otherUserId, DateTime upToUtc, CancellationToken ct = default);
	}
}
