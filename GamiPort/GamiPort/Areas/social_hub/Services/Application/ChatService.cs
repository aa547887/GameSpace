using GamiPort.Areas.social_hub.Services.Abstractions;      // IChatService, IProfanityFilter
using GamiPort.Areas.social_hub.SignalR;
using GamiPort.Areas.social_hub.SignalR.Contracts;           // DirectMessagePayload, ReadReceiptPayload, UnreadUpdatePayload
using GamiPort.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GamiPort.Areas.social_hub.Services.Application
{
	/// <summary>
	/// DM 服務（EF Core 操作集中）
	/// 重點：
	/// 1) 「已讀」：先以 ExecuteUpdateAsync 對 message_id IN (...) 批次更新（快），若為 0 行再逐筆 SaveChanges（穩）。
	/// 2) 「穢語遮蔽」：資料庫永遠保留原文；回給前端的 payload.Content 一律經 IProfanityFilter.Censor(...)。
	/// </summary>
	public sealed class ChatService : IChatService
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly MessagePolicyOptions _policy;
		private readonly ILogger<ChatService> _logger;
		private readonly IProfanityFilter _profanity; // 依據 dbo.Mutes 的遮蔽服務（DB 保留原文）

		public ChatService(
			GameSpacedatabaseContext db,
			IOptions<MessagePolicyOptions> policy,
			ILogger<ChatService> logger,
			IProfanityFilter profanity)
		{
			_db = db;
			_policy = policy.Value ?? new MessagePolicyOptions();
			_logger = logger;
			_profanity = profanity;
		}

		/// <summary>
		/// 傳送一則一對一訊息：DB 存原文，但回傳給前端的內容會先遮蔽。
		/// </summary>
		public async Task<DirectMessagePayload> SendDirectAsync(int senderId, int recipientId, string content, CancellationToken ct = default)
		{
			if (senderId <= 0) throw new InvalidOperationException(ErrorCodes.NotLoggedIn);
			if (recipientId <= 0 || recipientId == senderId) throw new InvalidOperationException(ErrorCodes.NoPeer);

			await EnsureUserExistsAsync(senderId, ct);
			await EnsureUserExistsAsync(recipientId, ct);

			if (string.IsNullOrWhiteSpace(content)) throw new InvalidOperationException(ErrorCodes.BadText);
			content = content.Trim();
			if (content.Length > _policy.MaxContentLength)
				content = content[.._policy.MaxContentLength];

			var conv = await GetOrCreateConversationAsync(senderId, recipientId, ct);
			var iAmP1 = senderId == conv.Party1Id;
			var now = DateTime.UtcNow;

			// ⚠ DB 永遠保存「原文」
			var msg = new DmMessage
			{
				ConversationId = conv.ConversationId,
				SenderIsParty1 = iAmP1,
				MessageText = content, // 原文
				IsRead = false,
				EditedAt = now         // 你目前以 EditedAt 當訊息時間
			};
			_db.DmMessages.Add(msg);
			await _db.SaveChangesAsync(ct);

			// ✅ 回給前端的顯示內容一律遮蔽（保持前後端一致）
			return new DirectMessagePayload
			{
				MessageId = msg.MessageId,
				SenderId = senderId,
				ReceiverId = recipientId,
				Content = _profanity.Censor(content),
				SentAtIso = now.ToString("o", CultureInfo.InvariantCulture)
			};
		}

		/// <summary>
		/// 把「對方傳給我」的未讀訊息批次標記為已讀；回傳影響列數。
		/// 本版忽略 upTo（Hub 也傳 null），一次將此會話「對方→我」的未讀全部設為已讀，
		/// 以避免 upTo 的 Kind/精度/觸發器差異造成 0-row 的不確定性。
		/// </summary>
		public async Task<int> MarkReadUpToAsync(int meUserId, int otherUserId, DateTime? upToUtc = null, CancellationToken ct = default)
		{
			if (meUserId <= 0 || otherUserId <= 0 || meUserId == otherUserId) return 0;

			await EnsureUserExistsAsync(meUserId, ct);
			await EnsureUserExistsAsync(otherUserId, ct);

			var p1 = Math.Min(meUserId, otherUserId);
			var p2 = Math.Max(meUserId, otherUserId);
			var conv = await _db.DmConversations.AsNoTracking()
				.FirstOrDefaultAsync(c => !c.IsManagerDm && c.Party1Id == p1 && c.Party2Id == p2, ct);
			if (conv is null) return 0;

			var iAmP1 = (meUserId == conv.Party1Id);

			// 目前版本不加時間條件；若未來要支援 upTo，這裡加 m.EditedAt <= upTo.Value 即可
			var ids = await _db.DmMessages
				.Where(m => m.ConversationId == conv.ConversationId
						 && m.SenderIsParty1 != iAmP1
						 && !m.IsRead)
				.Select(m => m.MessageId)
				.ToListAsync(ct);

			if (ids.Count == 0) return 0;

			var now = DateTime.UtcNow;

			// 3A) 批次更新（快）
			var rows = await _db.DmMessages
				.Where(m => ids.Contains(m.MessageId))
				.ExecuteUpdateAsync(s => s
					.SetProperty(m => m.IsRead, true)
					.SetProperty(m => m.ReadAt, now), ct);

			if (rows > 0)
			{
				_logger.LogDebug("[ChatService] MarkReadUpTo: batch updated {Count} rows. me={Me} other={Other}", rows, meUserId, otherUserId);
				return rows;
			}

			// 3B) 逐筆更新（穩）
			var entities = await _db.DmMessages
				.Where(m => ids.Contains(m.MessageId) && !m.IsRead)
				.ToListAsync(ct);

			foreach (var m in entities)
			{
				m.IsRead = true;
				m.ReadAt = now;
			}

			rows = await _db.SaveChangesAsync(ct);
			_logger.LogDebug("[ChatService] MarkReadUpTo: per-entity updated {Count} rows. me={Me} other={Other}", rows, meUserId, otherUserId);
			return rows;
		}

		/// <summary>相容保留（忽略回傳值，導到新方法）。</summary>
		[Obsolete("Use MarkReadUpToAsync instead, which returns rowsAffected.")]
		public async Task MarkReadAsync(int meUserId, int otherUserId, DateTime upToUtc, CancellationToken ct = default)
			=> _ = await MarkReadUpToAsync(meUserId, otherUserId, upToUtc, ct);

		/// <summary>
		/// 讀歷史訊息：未帶 afterUtc 時，回傳最新 pageSize 筆（由新到舊再正序輸出）。
		/// DB 讀出仍是原文；在組 payload 前一律先遮蔽。
		/// </summary>
		public async Task<IReadOnlyList<DirectMessagePayload>> GetHistoryAsync(int userId, int otherId, DateTime? afterUtc, int pageSize, CancellationToken ct = default)
		{
			if (userId <= 0) throw new InvalidOperationException(ErrorCodes.NotLoggedIn);
			if (otherId <= 0 || otherId == userId) throw new InvalidOperationException(ErrorCodes.NoPeer);

			await EnsureUserExistsAsync(userId, ct);
			await EnsureUserExistsAsync(otherId, ct);

			var p1 = Math.Min(userId, otherId);
			var p2 = Math.Max(userId, otherId);

			var conv = await _db.DmConversations.AsNoTracking()
				.FirstOrDefaultAsync(c => !c.IsManagerDm && c.Party1Id == p1 && c.Party2Id == p2, ct);

			if (conv == null) return Array.Empty<DirectMessagePayload>();

			IQueryable<DmMessage> q = _db.DmMessages
				.AsNoTracking()
				.Where(m => m.ConversationId == conv.ConversationId);

			if (afterUtc.HasValue)
				q = q.Where(m => m.EditedAt > afterUtc.Value).OrderBy(m => m.EditedAt);
			else
				q = q.OrderByDescending(m => m.EditedAt).Take(pageSize).OrderBy(m => m.EditedAt);

			var raw = await q.Select(m => new
			{
				m.MessageId,
				m.SenderIsParty1,
				m.MessageText,   // 原文
				m.EditedAt
			}).ToListAsync(ct);

			// ✅ 輸出前遮蔽
			return raw.Select(m =>
			{
				var utc = DateTime.SpecifyKind(m.EditedAt, DateTimeKind.Utc);
				return new DirectMessagePayload
				{
					MessageId = m.MessageId,
					SenderId = m.SenderIsParty1 ? conv.Party1Id : conv.Party2Id,
					ReceiverId = m.SenderIsParty1 ? conv.Party2Id : conv.Party1Id,
					Content = _profanity.Censor(m.MessageText),
					SentAtIso = utc.ToString("o", CultureInfo.InvariantCulture)
				};
			}).ToList();
		}

		public async Task<(int total, int peer)> ComputeUnreadAsync(int userId, int peerId, CancellationToken ct = default)
		{
			var conv = await GetOrCreateConversationAsync(userId, peerId, ct);
			var iAmP1 = userId == conv.Party1Id;

			var peerUnread = await _db.DmMessages
				.Where(m => m.ConversationId == conv.ConversationId && !m.IsRead && m.SenderIsParty1 != iAmP1)
				.CountAsync(ct);

			var totalUnread = await (
				from m in _db.DmMessages
				join c in _db.DmConversations on m.ConversationId equals c.ConversationId
				where !c.IsManagerDm && !m.IsRead &&
					  ((c.Party1Id == userId && m.SenderIsParty1 == false) || (c.Party2Id == userId && m.SenderIsParty1 == true))
				select m
			).CountAsync(ct);

			return (totalUnread, peerUnread);
		}

		public async Task<int> ComputeTotalUnreadAsync(int userId, CancellationToken ct = default)
		{
			return await (
				from m in _db.DmMessages
				join c in _db.DmConversations on m.ConversationId equals c.ConversationId
				where !c.IsManagerDm && !m.IsRead &&
					  ((c.Party1Id == userId && m.SenderIsParty1 == false) || (c.Party2Id == userId && m.SenderIsParty1 == true))
				select m
			).CountAsync(ct);
		}

		// ---------- EF helpers ----------
		private async Task EnsureUserExistsAsync(int userId, CancellationToken ct)
		{
			var exists = await _db.Users.AnyAsync(u => u.UserId == userId, ct);
			if (!exists) throw new InvalidOperationException(ErrorCodes.UserNotFound);
		}

		private async Task<DmConversation> GetOrCreateConversationAsync(int a, int b, CancellationToken ct)
		{
			var p1 = Math.Min(a, b);
			var p2 = Math.Max(a, b);
			var conv = await _db.DmConversations
				.FirstOrDefaultAsync(c => !c.IsManagerDm && c.Party1Id == p1 && c.Party2Id == p2, ct);

			if (conv != null) return conv;

			// 若尚未有對話紀錄，傳訊/算未讀時會自動建立
			conv = new DmConversation
			{
				IsManagerDm = false,
				Party1Id = p1,
				Party2Id = p2,
				CreatedAt = DateTime.UtcNow
			};
			_db.DmConversations.Add(conv);
			await _db.SaveChangesAsync(ct);
			return conv;
		}
	}
}
