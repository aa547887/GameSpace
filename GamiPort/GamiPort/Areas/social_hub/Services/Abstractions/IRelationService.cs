using System.Threading;
using System.Threading.Tasks;

namespace GamiPort.Areas.social_hub.Services.Abstractions
{
    /// <summary>
    /// 定義了 <see cref="IRelationService"/> 中所有支援的交友動作代碼。
    /// 這些常數用於 <see cref="RelationCommand.ActionCode"/> 欄位，以指定要執行的操作。
    /// </summary>
    public static class RelationActionCodes
    {
        /// <summary>
        /// 表示發送好友請求的操作代碼。
        /// </summary>
        public const string FriendRequest = "friend_request";

        /// <summary>
        /// 表示接受好友請求的操作代碼。
        /// </summary>
        public const string Accept = "accept";

        /// <summary>
        /// 表示拒絕好友請求的操作代碼。
        /// </summary>
        public const string Reject = "reject";

        /// <summary>
        /// 表示取消已發送的好友請求的操作代碼。
        /// </summary>
        public const string CancelRequest = "cancel_request";

        /// <summary>
        /// 表示封鎖其他使用者的操作代碼。
        /// </summary>
        public const string Block = "block";

        /// <summary>
        /// 表示解除封鎖其他使用者的操作代碼。
        /// </summary>
        public const string Unblock = "unblock";

        /// <summary>
        /// 表示解除好友關係的操作代碼。
        /// </summary>
        public const string Unfriend = "unfriend";

        /// <summary>
        /// 表示設定好友暱稱的操作代碼。
        /// </summary>
        public const string SetNickname = "set_nickname";
    }

	/// <summary>
	/// 交友動作命令。用於向 <see cref="IRelationService"/> 傳遞執行特定交友操作所需的資訊。
	/// </summary>
	/// <param name="ActorUserId">執行操作的使用者 ID。</param>
	/// <param name="TargetUserId">操作的目標使用者 ID。</param>
	/// <param name="ActionCode">要執行的操作代碼，應使用 <see cref="RelationActionCodes"/> 中定義的常數。</param>
	/// <param name="Nickname">可選。僅在 <see cref="RelationActionCodes.SetNickname"/> 操作中使用，表示要設定的新暱稱。</param>
	public sealed record RelationCommand(
		int ActorUserId,          // 操作者
		int TargetUserId,         // 目標對象
		string ActionCode,        // friend_request / accept / reject / cancel_request / block / unblock / unfriend / set_nickname
		string? Nickname = null   // 僅 set_nickname 使用
	);

	/// <summary>
	/// 交友動作結果。包含操作是否成功、是否造成狀態變更以及新的關係狀態等資訊。
	/// </summary>
	/// <param name="Succeeded">表示操作是否成功完成（不包括驗證或資料庫錯誤）。</param>
	/// <param name="NoOp">表示本次操作是否沒有造成實際的狀態變更。</param>
	/// <param name="RelationId">可選。如果操作涉及現有關係，則為該關係的主鍵 ID。</param>
	/// <param name="NewStatusCode">可選。操作完成後關係的新狀態代碼（例如 "PENDING", "ACCEPTED", "BLOCKED"）。</param>
	/// <param name="NewStatusId">可選。操作完成後關係的新狀態 ID（對應資料庫中的狀態 ID）。</param>
	/// <param name="Reason">可選。如果操作失敗或被拒絕，則為失敗的理由。</param>
	public sealed record RelationResult(
		bool Succeeded,           // 是否成功（非驗證/DB 錯誤）
		bool NoOp,                // 本次是否沒有造成狀態變更
		int? RelationId,          // 關係主鍵
		string? NewStatusCode,    // 新狀態碼（"pending" / "accepted" / "blocked" / "removed" / "rejected" / "none"...）
		int? NewStatusId,         // 新狀態 ID（1=pending, 2=accepted, 3=blocked, 4=removed, 5=rejected, 6=none）
		string? Reason            // 失敗/拒絕時的理由（BadRequest 用）
	);

	/// <summary>
	/// 定義了處理使用者之間交友關係操作的服務介面。
	/// </summary>
	public interface IRelationService
	{
		/// <summary>
		/// 執行一個交友關係操作。
		/// </summary>
		/// <param name="cmd">包含操作細節的 <see cref="RelationCommand"/> 物件。</param>
		/// <param name="ct">用於取消操作的 <see cref="CancellationToken"/>。</param>
		/// <returns>表示操作結果的 <see cref="RelationResult"/> 物件。</returns>
		Task<RelationResult> ExecuteAsync(RelationCommand cmd, CancellationToken ct = default);
	}
}
