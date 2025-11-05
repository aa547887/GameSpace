using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GamiPort.Areas.social_hub.Services.Abstractions; // RelationCommand / RelationResult / IRelationService
using GamiPort.Models;                                  // GameSpacedatabaseContext / Relation / RelationStatus / User
using Microsoft.EntityFrameworkCore;

using GamiPort.Areas.social_hub.Hubs;
using GamiPort.Areas.social_hub.SignalR.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace GamiPort.Areas.social_hub.Services.Application
{
	/// <summary>
	/// 交友服務（集中驗證 + 狀態轉換；通過才寫入 DB）
	///
	/// 設計原則：
	///  1) 「以 StatusId 為主」判斷與寫入（穩定且直觀）；對前端則回 DB 的 StatusCode（如：PENDING/ACCEPTED…）。
	///  2) 關係唯一化：以 (UserIdSmall, UserIdLarge) = (min, max) 正規化，並依據資料庫唯一鍵 UQ_Relation_UserPair。
	///  3) No-Op：沒有實際變更就回 NoOp=true，避免重複通知/寫入。
	///  4) RequestedBy 僅在 PENDING 有意義；離開 PENDING 一律清空。
	///  5) 競態防護：新增/更新時若撞唯一鍵或狀態被他人改動，重讀現況並回合理結果。
	///
	/// 狀態機（依你的資料表記錄，ID 從資料庫動態載入）：
	///   NONE：無關係（解除好友/解除封鎖後的最終）
	///   PENDING ← friend_request（從 NONE/REJECTED/REMOVED）
	///   PENDING → ACCEPTED ：accept（僅受邀方）
	///   PENDING → REJECTED ：reject（僅受邀方）	
	///   PENDING → REMOVED  ：cancel_request（僅邀請方；歷史註記「已取消邀請」）
	///   *         → BLOCKED   ：block（任一狀態可封鎖）
	///   BLOCKED → NONE     ：unblock（解除封鎖）
	///   ACCEPTED → NONE    ：unfriend（解除好友）
	///   ACCEPTED → set_nickname（不改變狀態；僅更新暱稱）
	///
	/// 安全提醒：
	///   - 請於 Controller/Hub 以登入身分覆蓋 ActorUserId，勿信任前端送來的 ActorUserId。
	/// </summary>
	public sealed class RelationService : IRelationService
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IHubContext<ChatHub, IChatClient> _hubContext;

		public RelationService(GameSpacedatabaseContext db, IHubContext<ChatHub, IChatClient> hubContext)
		{
			_db = db;
			_hubContext = hubContext;
		}


		// ===== 狀態 Id（請與資料庫 Relation_Status 對應）=============================
		// 這些將從資料庫動態載入
		private static System.Collections.Generic.Dictionary<string, int>? _statusIdMap;

		private static int StatusPending => _statusIdMap!["PENDING"];
		private static int StatusAccepted => _statusIdMap!["ACCEPTED"];
		private static int StatusBlocked => _statusIdMap!["BLOCKED"];
		private static int StatusRemoved => _statusIdMap!["REMOVED"];
		private static int StatusRejected => _statusIdMap!["REJECTED"];
		private static int StatusNone => _statusIdMap!["NONE"];

		// 暱稱長度限制（與 OnModelCreating HasMaxLength(10) 對應）
		private const int NICKNAME_MAXLEN = 10;

		private async Task<FriendPayload> GetUserPayloadAsync(int userId, CancellationToken ct)
		{
			var user = await _db.Users.AsNoTracking()
				.Where(u => u.UserId == userId)
				.Select(u => new { u.UserId, u.UserName })
				.FirstOrDefaultAsync(ct);

			if (user == null) return new FriendPayload(userId, "Unknown User");

			// TODO: 實際的頭像 URL 邏輯
			return new FriendPayload(user.UserId, user.UserName, AvatarUrl: "/img/default-avatar.png");
		}
		// ===== 一次性快取：StatusId -> StatusCode（用 DB 的代碼回傳給前端）=============
		private static volatile bool _statusMapLoaded = false;
		private static readonly object _statusMapLock = new();
		private static System.Collections.Generic.Dictionary<int, string>? _statusCodeMap;

		/// <summary>第一次呼叫時載入 Relation_Status 的代碼映射。</summary>
		private async Task EnsureStatusMapAsync(CancellationToken ct)
		{
			if (_statusMapLoaded && _statusCodeMap is not null) return;

			lock (_statusMapLock)
			{
				if (_statusMapLoaded && _statusCodeMap is not null) return;
			}

			var list = await _db.Set<RelationStatus>()
				.AsNoTracking()
				.Select(s => new { s.StatusId, s.StatusCode })
				.ToListAsync(ct);

			var statusCodeDict = list.ToDictionary(s => s.StatusId, s => s.StatusCode);
			var statusIdDict = list.ToDictionary(s => s.StatusCode, s => s.StatusId);

			lock (_statusMapLock)
			{
				_statusCodeMap = statusCodeDict;
				_statusIdMap = statusIdDict;
				_statusMapLoaded = true;
			}
		}

		/// <summary>取得 DB 定義的狀態代碼；若未載入或查無，退回 Id.ToString()（不致拋例外）。</summary>
		private static string CodeOf(int statusId)
			=> (_statusCodeMap is not null && _statusCodeMap.TryGetValue(statusId, out var code))
				? code
				: statusId.ToString();

		// ============================================================================
		// 入口：集中驗證 + 路由到各動作
		// ============================================================================
		public async Task<RelationResult> ExecuteAsync(RelationCommand cmd, CancellationToken ct = default)
		{
			// 先確保代碼映射已載入（每個請求只會首次載入一次）
			await EnsureStatusMapAsync(ct);

			// ---- 0) 基本輸入驗證（Actor 請在 Controller/Hub 以登入者覆蓋） ----
			var actor = cmd.ActorUserId;
			var target = cmd.TargetUserId;

			if (actor <= 0 || target <= 0) return Fail("UserId 不合法。");
			if (actor == target) return Fail("不可對自己操作。");
			if (string.IsNullOrWhiteSpace(cmd.ActionCode)) return Fail("ActionCode 必填。");

			// 使用者存在性（一次往返檢查兩人；也可省略讓 FK 代勞）
			var count = await _db.Users.AsNoTracking()
				.CountAsync(u => u.UserId == actor || u.UserId == target, ct);
			if (count != 2) return Fail("使用者不存在。");

			// ---- 1) 對稱 pair 正規化 ----
			var small = Math.Min(actor, target);
			var large = Math.Max(actor, target);

			// 當前關係（可能為 null）
			var row = await _db.Relations
				.SingleOrDefaultAsync(r => r.UserIdSmall == small && r.UserIdLarge == large, ct);

			// ---- 2) 動作路由 ----
			var action = cmd.ActionCode.Trim().ToLowerInvariant();
			return action switch
			{
				RelationActionCodes.FriendRequest => await DoFriendRequest(row, small, large, actor, ct),
				RelationActionCodes.Accept => await DoAccept(row, actor, ct),
				RelationActionCodes.Reject => await DoReject(row, actor, ct),
				RelationActionCodes.CancelRequest => await DoCancel(row, actor, ct),
				RelationActionCodes.Block => await DoBlock(row, small, large, actor, ct),
				RelationActionCodes.Unblock => await DoUnblock(row, actor, ct),  // BLOCKED → NONE
				RelationActionCodes.Unfriend => await DoUnfriend(row, actor, ct), // ACCEPTED → NONE
				RelationActionCodes.SetNickname => await DoSetNickname(row, actor, cmd.Nickname, ct),
				_ => Fail("未知的 ActionCode。")
			};
		}

		// ============================================================================
		// 各動作實作（微優化：統一 UTC 時戳；所有非 PENDING 都清空 RequestedBy）
		// ============================================================================

		private async Task<RelationResult> DoFriendRequest(
	Relation? row, int small, int large, int actor, CancellationToken ct)
		{
			// A) 關係不存在 → 嘗試建立 PENDING（唯一鍵保護）
			if (row is null)
			{
				row = new Relation
				{
					UserIdSmall = small,
					UserIdLarge = large,
					StatusId = StatusPending,
					RequestedBy = actor,
					CreatedAt = DateTime.UtcNow
				};
				_db.Relations.Add(row);

				try
				{
					await _db.SaveChangesAsync(ct);
					return Ok(row.RelationId, StatusPending);
				}
				catch (DbUpdateException)
				{
					// 可能是兩邊同時發請求撞唯一鍵 → 重讀現況（NoOp/Changed）
					row = await _db.Relations.SingleAsync(
						r => r.UserIdSmall == small && r.UserIdLarge == large, ct);
					return NoOp(row.RelationId, row.StatusId, "關係已由其他操作建立/更新。");
				}
			}

			// B) 已存在 → 依狀態處理
			if (row.StatusId == StatusBlocked)
			{
				return Fail("目前為封鎖狀態，無法送出邀請。");
			}
			else if (row.StatusId == StatusAccepted)
			{
				return NoOp(row.RelationId, StatusAccepted, "已是好友。");
			}
			else if (row.StatusId == StatusPending)
			{
				// 我自己送過邀請 → 不再重複
				if (row.RequestedBy == actor)
					return NoOp(row.RelationId, StatusPending, "邀請已送出。");

				// 對方已邀我 → 維持 PENDING，提示前端顯示「去按接受」
				return NoOp(row.RelationId, StatusPending, "對方已發出邀請，請改用 accept。");
			}
			else if (row.StatusId == StatusRejected || row.StatusId == StatusRemoved || row.StatusId == StatusNone)
			{
				return await UpdateAndReturnOk(row, StatusPending, ct, requestedBy: actor, clearRequestedBy: false);
			}

			return Fail("目前狀態不允許送出邀請。");
		}


		private async Task<RelationResult> DoAccept(Relation? row, int actor, CancellationToken ct)
		{
			if (row is null) return Fail("尚無此關係。");
			if (row.StatusId != StatusPending) return Fail("關係狀態已變更，請重新整理後再試。");
			if (row.RequestedBy == actor) return Fail("邀請方不得自行接受。");

			row.StatusId = StatusAccepted;

			row.UpdatedAt = DateTime.UtcNow;

			await _db.SaveChangesAsync(ct);



			// Broadcast FriendAdded event to both users

			var actorPayload = await GetUserPayloadAsync(actor, ct);

			var targetPayload = await GetUserPayloadAsync(row.RequestedBy!.Value, ct);



			await _hubContext.Clients.User(actor.ToString()).FriendAdded(targetPayload);

			await _hubContext.Clients.User(row.RequestedBy.Value.ToString()).FriendAdded(actorPayload);



			return Ok(row.RelationId, StatusAccepted);


		}

		private async Task<RelationResult> DoReject(Relation? row, int actor, CancellationToken ct)
		{
			if (row is null) return Fail("尚無此關係。");
			if (row.StatusId != StatusPending) return Fail("關係狀態已變更，請重新整理後再試。");
			if (row.RequestedBy == actor) return Fail("邀請方不得拒絕。");

			return await UpdateAndReturnOk(row, StatusRejected, ct);
		}

		private async Task<RelationResult> DoCancel(Relation? row, int actor, CancellationToken ct)
		{
			if (row is null) return Fail("尚無此關係。");
			if (row.StatusId != StatusPending) return Fail("關係狀態已變更，請重新整理後再試。");
			if (row.RequestedBy != actor) return Fail("僅邀請方可取消邀請。");

			return await UpdateAndReturnOk(row, StatusRemoved, ct);
		}

		private async Task<RelationResult> DoBlock(Relation? row, int small, int large, int actor, CancellationToken ct)
		{
			if (row is null)
			{
				row = new Relation
				{
					UserIdSmall = small,
					UserIdLarge = large,
					StatusId = StatusBlocked,
					RequestedBy = null,
					CreatedAt = DateTime.UtcNow
				};
				_db.Relations.Add(row);

				try
				{
					await _db.SaveChangesAsync(ct);
					return Ok(row.RelationId, StatusBlocked);
				}
				catch (DbUpdateException)
				{
					// 撞唯一鍵 → 讀現況
					row = await _db.Relations.SingleAsync(r => r.UserIdSmall == small && r.UserIdLarge == large, ct);
					if (row.StatusId == StatusBlocked)
						return NoOp(row.RelationId, StatusBlocked, "已在封鎖狀態。");

					// 改為封鎖
					return await UpdateAndReturnOk(row, StatusBlocked, ct);
				}
			}

			if (row.StatusId == StatusBlocked)
				return NoOp(row.RelationId, StatusBlocked, "已在封鎖狀態。");

			var result = await UpdateAndReturnOk(row, StatusBlocked, ct);

			// Broadcast FriendRemoved event to both users
			var actorId = actor;
			var targetId = (actor == row.UserIdSmall) ? row.UserIdLarge : row.UserIdSmall;

			await _hubContext.Clients.User(actorId.ToString()).FriendRemoved(targetId);
			await _hubContext.Clients.User(targetId.ToString()).FriendRemoved(actorId);

			return result;
		}

		private async Task<RelationResult> DoUnblock(Relation? row, int actor, CancellationToken ct)

		{

			if (row is null) return Fail("尚無此關係。");

			if (row.StatusId != StatusBlocked) return Fail("不是封鎖狀態。");



			return await UpdateAndReturnOk(row, StatusNone, ct);

		}

		private async Task<RelationResult> DoUnfriend(Relation? row, int actor, CancellationToken ct)
		{
			if (row is null) return Fail("尚無此關係。");
			if (row.StatusId != StatusAccepted) return Fail("僅在好友狀態可解除好友。");

			var result = await UpdateAndReturnOk(row, StatusNone, ct);

			// Broadcast FriendRemoved event to both users
			var actorId = actor;
			var targetId = (actor == row.UserIdSmall) ? row.UserIdLarge : row.UserIdSmall;

			await _hubContext.Clients.User(actorId.ToString()).FriendRemoved(targetId);
			await _hubContext.Clients.User(targetId.ToString()).FriendRemoved(actorId);

			return result;
		}

		private async Task<RelationResult> DoSetNickname(Relation? row, int actor, string? nickname, CancellationToken ct)
		{
			if (row is null) return Fail("尚無此關係。");
			if (row.StatusId != StatusAccepted) return Fail("非好友狀態不可設定暱稱。");

			var newName = string.IsNullOrWhiteSpace(nickname) ? null : nickname.Trim();
			if (newName is not null && newName.Length > NICKNAME_MAXLEN)
				return Fail($"暱稱長度上限 {NICKNAME_MAXLEN}。");

			if (string.Equals(row.FriendNickname, newName, StringComparison.Ordinal))
				return NoOp(row.RelationId, StatusAccepted, "暱稱未變更。");

			return await UpdateAndReturnOk(row, StatusAccepted, ct, clearRequestedBy: false);
		}

		private async Task<RelationResult> UpdateAndReturnOk(
			Relation row, int newStatusId, CancellationToken ct, int? requestedBy = null, bool clearRequestedBy = true)
		{
			row.StatusId = newStatusId;
			if (clearRequestedBy)
			{
				row.RequestedBy = requestedBy; // Use the provided requestedBy, which can be null
			}
			else if (requestedBy.HasValue)
			{
				row.RequestedBy = requestedBy.Value; // Set to specific value if provided and not clearing
			}
			// If clearRequestedBy is false and requestedBy is null, keep existing value

			row.UpdatedAt = DateTime.UtcNow;

			await _db.SaveChangesAsync(ct);
			return Ok(row.RelationId, newStatusId);
		}
		// ============================================================================
		// 統一回傳封裝（newStatusCode 一律用 DB 的 StatusCode）
		// ============================================================================

		private static RelationResult Ok(int relationId, int newStatusId, string? reason = null)
			=> new(true, false, relationId, CodeOf(newStatusId), newStatusId, reason);

		private static RelationResult NoOp(int relationId, int newStatusId, string? reason = null)
			=> new(true, true, relationId, CodeOf(newStatusId), newStatusId, reason);

		private static RelationResult Fail(string reason)
			=> new(false, false, null, null, null, reason);
	}
}
