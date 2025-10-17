using System.Threading;
using System.Threading.Tasks;

namespace GamiPort.Areas.social_hub.Services.Abstractions
{
	/// <summary>
	/// 交友關係的單一入口服務（集中驗證＋寫入；不通過就拒絕寫 DB）。
	/// 設計原則：
	///  - 狀態不綁數字：以 status_code（字串）查 DB → 取得對應 status_id（有快取）
	///  - No-Op 最佳化：狀態未改變不寫 DB，回傳 Succeeded=true, NoOp=true
	///  - 對稱關係：以 (UserIdSmall, UserIdLarge) 唯一化一筆紀錄
	///  - 單一服務層：所有動作（friend_request/accept/reject/cancel_request/block/unblock/set_nickname）走同一入口
	/// </summary>
	public interface IRelationService
	{
		Task<RelationResult> ExecuteAsync(RelationCommand cmd, CancellationToken ct = default);
	}

	/// <summary>前端（按鈕 data-*）丟進來的參數</summary>
	public sealed record RelationCommand(
		int ActorUserId,          // 操作者（誰按了按鈕）
		int TargetUserId,         // 目標對象
		string ActionCode,        // friend_request / accept / reject / cancel_request / block / unblock / set_nickname
		string? Nickname = null   // 只有 set_nickname 用得到（上限 10）
	);

	/// <summary>統一回傳結果（成功/失敗＋NoOp＋新狀態）</summary>
	public sealed record RelationResult(
		bool Succeeded,
		bool NoOp = false,
		int? RelationId = null,
		string? NewStatusCode = null,
		string? Reason = null
	);

	public enum RelationError
	{
		None = 0,
		SelfRelationNotAllowed,
		UserNotFound,
		TargetNotFound,
		InvalidAction,
		InvalidTransition,
		NicknameTooLong,
		DbError
	}
}
