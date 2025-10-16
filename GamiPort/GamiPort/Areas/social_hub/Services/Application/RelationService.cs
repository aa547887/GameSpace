using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GamiPort.Areas.social_hub.Services.Abstractions;
using GamiPort.Models;                    // GameSpacedatabaseContext + EF 實體
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GamiPort.Areas.social_hub.Services.Application
{
	/// <summary>
	/// RelationService：集中處理所有交友動作，狀態以 status_code 解譯，不硬塞數字 ID。
	/// 重要：本實作不假設「RequestedBy」欄位一定存在，若你 DB 有此欄位，可再加上「只有被邀請者可 accept」等更嚴格驗證。
	/// </summary>
	public sealed class RelationService : IRelationService
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IMemoryCache _cache;

		// 狀態代碼（字串）；請確認 RelationStatuses 表有對應資料
		private const string STATUS_NONE = "none";
		private const string STATUS_PENDING = "pending";
		private const string STATUS_ACCEPTED = "accepted";
		private const string STATUS_BLOCKED = "blocked";
		private const string STATUS_REJECTED = "rejected";

		public RelationService(GameSpacedatabaseContext db, IMemoryCache cache)
		{
			_db = db;
			_cache = cache;
		}

		public async Task<RelationResult> ExecuteAsync(RelationCommand cmd, CancellationToken ct = default)
		{
			// 0) 基本檢查：不得對自己、雙方得存在
			if (cmd.ActorUserId == cmd.TargetUserId)
				return Fail(RelationError.SelfRelationNotAllowed, "不能對自己建立關係。");

			var actorExists = await _db.Users.AsNoTracking().AnyAsync(u => u.UserId == cmd.ActorUserId, ct);
			if (!actorExists) return Fail(RelationError.UserNotFound, $"找不到使用者 UserId={cmd.ActorUserId}。");

			var targetExists = await _db.Users.AsNoTracking().AnyAsync(u => u.UserId == cmd.TargetUserId, ct);
			if (!targetExists) return Fail(RelationError.TargetNotFound, $"找不到使用者 UserId={cmd.TargetUserId}。");

			// 1) 對稱化：唯一鍵採用 (small, large)
			var (small, large) = cmd.ActorUserId < cmd.TargetUserId
				? (cmd.ActorUserId, cmd.TargetUserId)
				: (cmd.TargetUserId, cmd.ActorUserId);

			// 2) 取得現有關係（含 Status 導覽屬性）
			var rel = await _db.Relations
				.Include(r => r.Status)
				.FirstOrDefaultAsync(r => r.UserIdSmall == small && r.UserIdLarge == large, ct);

			var currentCode = rel?.Status?.StatusCode ?? STATUS_NONE;

			// 3) 動作分派
			switch (cmd.ActionCode)
			{
				case "friend_request":
					return await DoFriendRequestAsync(rel, small, large, currentCode, ct);

				case "accept":
					return await TransitionAsync(rel, currentCode, mustBeCurrent: STATUS_PENDING, nextCode: STATUS_ACCEPTED, ct);

				case "reject":
					return await TransitionAsync(rel, currentCode, mustBeCurrent: STATUS_PENDING, nextCode: STATUS_REJECTED, ct);

				case "cancel_request":
					// 取消邀請：只允許 pending → 刪除（視為 none）
					return await CancelIfAsync(rel, currentCode, mustBeCurrent: STATUS_PENDING, ct);

				case "block":
					return await SetBlockedAsync(rel, small, large, currentCode, ct);

				case "unblock":
					// 解除封鎖：只允許 blocked → 刪除（視為 none）
					return await CancelIfAsync(rel, currentCode, mustBeCurrent: STATUS_BLOCKED, ct);

				case "set_nickname":
					return await SetNicknameAsync(cmd, rel, small, large, currentCode, ct);

				default:
					return Fail(RelationError.InvalidAction, $"未知的 ActionCode：{cmd.ActionCode}");
			}
		}

		// ====== 具體動作 ======

		private async Task<RelationResult> DoFriendRequestAsync(
			Relation? rel, int small, int large, string currentCode, CancellationToken ct)
		{
			// 已 pending/accepted/blocked → 不重送
			if (currentCode is STATUS_PENDING or STATUS_ACCEPTED or STATUS_BLOCKED)
				return Noop(currentCode, "目前狀態不允許重複送出交友邀請。");

			var pendingId = await GetStatusIdAsync(STATUS_PENDING, ct);

			if (rel is null)
			{
				rel = new Relation
				{
					UserIdSmall = small,
					UserIdLarge = large,
					StatusId = pendingId,
					CreatedAt = DateTime.UtcNow
				};
				_db.Relations.Add(rel);
			}
			else
			{
				rel.StatusId = pendingId;
				rel.UpdatedAt = DateTime.UtcNow;
				_db.Relations.Update(rel);
			}

			await _db.SaveChangesAsync(ct);
			return Success(rel.RelationId, STATUS_PENDING);
		}

		private async Task<RelationResult> SetBlockedAsync(
			Relation? rel, int small, int large, string currentCode, CancellationToken ct)
		{
			if (currentCode == STATUS_BLOCKED) return Noop(currentCode, "已是封鎖狀態。");

			var blockedId = await GetStatusIdAsync(STATUS_BLOCKED, ct);

			if (rel is null)
			{
				rel = new Relation
				{
					UserIdSmall = small,
					UserIdLarge = large,
					StatusId = blockedId,
					CreatedAt = DateTime.UtcNow
				};
				_db.Relations.Add(rel);
			}
			else
			{
				rel.StatusId = blockedId;
				rel.UpdatedAt = DateTime.UtcNow;
				_db.Relations.Update(rel);
			}

			await _db.SaveChangesAsync(ct);
			return Success(rel.RelationId, STATUS_BLOCKED);
		}

		private async Task<RelationResult> SetNicknameAsync(
			RelationCommand cmd, Relation? rel, int small, int large, string currentCode, CancellationToken ct)
		{
			// 暱稱（可空則視為清除；若你想限制不可空，就把下面第一行改成直接 Fail）
			var nickname = cmd.Nickname?.Trim();
			if (nickname is { Length: > 10 }) return Fail(RelationError.NicknameTooLong, "暱稱長度上限 10。");

			if (rel is null)
			{
				// 尚未建立任何關係 → 建一筆「none」殼，僅存暱稱
				rel = new Relation
				{
					UserIdSmall = small,
					UserIdLarge = large,
					StatusId = await GetStatusIdAsync(STATUS_NONE, ct),
					FriendNickname = nickname,
					CreatedAt = DateTime.UtcNow
				};
				_db.Relations.Add(rel);
			}
			else
			{
				rel.FriendNickname = nickname;
				rel.UpdatedAt = DateTime.UtcNow;
				_db.Relations.Update(rel);
			}

			await _db.SaveChangesAsync(ct);
			return Success(rel.RelationId, currentCode);
		}

		private async Task<RelationResult> TransitionAsync(
			Relation? rel,
			string currentCode,
			string mustBeCurrent,
			string nextCode,
			CancellationToken ct)
		{
			if (rel is null) return Fail(RelationError.InvalidTransition, "沒有可轉換的關係紀錄。");
			if (currentCode != mustBeCurrent) return Noop(currentCode, $"僅允許由 {mustBeCurrent} 轉為 {nextCode}。");

			if (nextCode == STATUS_NONE)
			{
				_db.Relations.Remove(rel);
				await _db.SaveChangesAsync(ct);
				return Success(null, STATUS_NONE);
			}

			rel.StatusId = await GetStatusIdAsync(nextCode, ct);
			rel.UpdatedAt = DateTime.UtcNow;
			_db.Relations.Update(rel);
			await _db.SaveChangesAsync(ct);
			return Success(rel.RelationId, nextCode);
		}

		private async Task<RelationResult> CancelIfAsync(
			Relation? rel, string currentCode, string mustBeCurrent, CancellationToken ct)
		{
			if (rel is null) return Noop(currentCode, "已無關係可取消。");
			if (currentCode != mustBeCurrent) return Noop(currentCode, $"僅允許由 {mustBeCurrent} 取消。");

			_db.Relations.Remove(rel);
			await _db.SaveChangesAsync(ct);
			return Success(null, STATUS_NONE);
		}

		// ====== 小工具 ======

		private async Task<int> GetStatusIdAsync(string statusCode, CancellationToken ct)
		{
			var key = $"rel:status:{statusCode}";
			if (_cache.TryGetValue(key, out int id)) return id;

			// ★ 注意：如果你的表名是 RelationStatus（單數），把 RelationStatuses 改掉
			var found = await _db.RelationStatuses
				.AsNoTracking()
				.Where(s => s.StatusCode == statusCode)
				.Select(s => new { s.StatusId })
				.FirstOrDefaultAsync(ct);

			if (found is null)
				throw new InvalidOperationException($"RelationStatuses 找不到 status_code='{statusCode}'，請先種資料。");

			_cache.Set(key, found.StatusId, TimeSpan.FromMinutes(30));
			return found.StatusId;
		}

		private static RelationResult Success(int? relationId, string? newCode) =>
			new(true, NoOp: false, RelationId: relationId, NewStatusCode: newCode, Reason: null);

		private static RelationResult Noop(string currentCode, string message) =>
			new(true, NoOp: true, RelationId: null, NewStatusCode: currentCode, Reason: message);

		private static RelationResult Fail(RelationError _, string message) =>
			new(false, NoOp: false, RelationId: null, NewStatusCode: null, Reason: message);
	}
}
