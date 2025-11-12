using GamiPort.Areas.social_hub.SignalR.Contracts;

namespace GamiPort.Areas.social_hub.Services.Abstractions
{
	/// <summary>
	/// 【聊天推播抽象介面】
	/// 目的：
	///  - 讓 Hub 不直接操作 SignalR IHubContext/Clients，改由介面封裝事件名與目標群組邏輯。
	///  - 集中管理推播行為（事件名稱、群組命名規則），避免魔術字串散落各處。
	///  - 之後若要更換底層（例如改用另一種推播、或加上審計/記錄），只需改實作，不必碰 Hub。
	///
	/// 事件名建議統一放在 ChatEvents（GamiPort.Areas.social_hub.SignalR.ChatEvents）
	/// 群組命名建議統一放在 GroupNames（GamiPort.Areas.social_hub.SignalR.GroupNames）
	/// </summary>
	public interface IChatNotifier
	{
		/// <summary>
		/// 【新訊息推播】點對點訊息抵達時，對「雙方」推播。
		/// 為什麼要雙向？使用者可能一人多裝置/多分頁，自己端也需要同步顯示（例如另一個分頁的對話框）。
		/// - 推播目標群組：GroupNames.User(userAId) 與 GroupNames.User(userBId)
		/// - 事件名稱：ChatEvents.ReceiveDirect
		/// </summary>
		/// <param name="userAId">一方使用者 Id（通常是發送者）</param>
		/// <param name="userBId">另一方使用者 Id（通常是接收者）</param>
		/// <param name="payload">訊息內容（DirectMessagePayload）</param>
		/// <param name="ct">取消權杖：外部要求取消時，中止 SignalR 傳送</param>
		Task BroadcastReceiveDirectAsync(int userAId, int userBId, DirectMessagePayload payload, CancellationToken ct = default);

		/// <summary>
		/// 【已讀回執推播】當一方回報「已讀到某時間點」，通常需要讓兩邊 UI 同步。
		/// - 推播目標群組：GroupNames.User(userAId) 與 GroupNames.User(userBId)
		/// - 事件名稱：ChatEvents.ReadReceipt
		/// - ReadReceiptPayload.RowsAffected：服務層已保證 rowsAffected &gt; 0 才會廣播，此欄位可做除錯或追蹤
		/// </summary>
		/// <param name="userAId">一方使用者 Id（發出回執者）</param>
		/// <param name="userBId">另一方使用者 Id（回執接收者）</param>
		/// <param name="payload">回執內容（ReadReceiptPayload）</param>
		/// <param name="ct">取消權杖</param>
		Task BroadcastReadReceiptAsync(int userAId, int userBId, ReadReceiptPayload payload, CancellationToken ct = default);

		/// <summary>
		/// 【未讀統計推播】更新單一使用者的未讀資訊（含全站總未讀與特定對象的未讀）。
		/// - 推播目標群組：GroupNames.User(userId)
		/// - 事件名稱：ChatEvents.UnreadUpdate
		/// 依你的 UI 設計，PeerId=0 可代表「僅更新全站總未讀」。
		/// </summary>
		/// <param name="userId">要接收未讀統計的目標使用者</param>
		/// <param name="payload">未讀統計（UnreadUpdatePayload）</param>
		/// <param name="ct">取消權杖</param>
		Task BroadcastUnreadAsync(int userId, UnreadUpdatePayload payload, CancellationToken ct = default);
	}
}
