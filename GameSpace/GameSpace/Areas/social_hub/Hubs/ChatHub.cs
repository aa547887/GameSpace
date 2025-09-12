using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// ⬇️ 加上這行：用別名避免命名衝突（與 Program.cs 一致）
using IMuteFilterAlias = GameSpace.Areas.social_hub.Services.IMuteFilter;

namespace GameSpace.Areas.social_hub.Hubs
{
	/// <summary>
	/// 1對1聊天 Hub（user↔user）
	/// 事件：
	///   - ReceiveDirect({ messageId, senderId, receiverId, content, sentAtIso })
	///   - ReadReceipt({ fromUserId, upToIso })
	///   - UnreadUpdate({ total, peerId, unread })
	///   - Error("NOT_LOGGED_IN" | "NO_PEER" | "BAD_TEXT")
	/// </summary>
	public class ChatHub : Hub
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly ILogger<ChatHub> _logger;
		// ⬇️ 新增：顯示端遮罩服務
		private readonly IMuteFilterAlias _mute;

		// ⬇️ 建構子注入 _mute
		public ChatHub(GameSpacedatabaseContext db, ILogger<ChatHub> logger, IMuteFilterAlias mute)
		{
			_db = db;
			_logger = logger;
			_mute = mute;
		}

		public override async Task OnConnectedAsync()
		{
			var (uid, _) = GetIdentity();
			if (uid > 0)
			{
				await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(uid));
				_logger.LogInformation("User {Uid} connected to ChatHub.", uid);
			}
			else
			{
				await Clients.Caller.SendAsync("Error", "NOT_LOGGED_IN");
			}
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			var (uid, _) = GetIdentity();
			if (uid > 0)
				await Groups.RemoveFromGroupAsync(Context.ConnectionId, UserGroup(uid));
			await base.OnDisconnectedAsync(exception);
		}

		/// <summary>送訊息 → 存 DB(原文) → 推給雙方(遮罩後) → 推對方未讀</summary>
		public async Task<object> SendMessageTo(int otherId, string text)
		{
			var (meId, _) = GetIdentity();
			if (meId <= 0) throw new HubException("NOT_LOGGED_IN");
			if (otherId <= 0 || otherId == meId) throw new HubException("NO_PEER");

			if (string.IsNullOrWhiteSpace(text)) throw new HubException("BAD_TEXT");
			text = text.Trim();
			if (text.Length > 255) text = text[..255];

			var conv = await GetOrCreateConversationAsync(meId, otherId);
			var iAmP1 = (meId == conv.Party1Id);

			var now = DateTime.UtcNow;
			var msg = new DmMessage
			{
				ConversationId = conv.ConversationId,
				SenderIsParty1 = iAmP1,
				MessageText = text,   // ✅ DB 永遠存原文
				IsRead = false,
				EditedAt = now
			};

			_db.DmMessages.Add(msg);
			await _db.SaveChangesAsync(); // 你的 Trigger 會更新 last_message_at

			// ✅ 廣播前遮罩（只影響顯示）
			var display = _mute.Apply(text);

			var payload = new
			{
				messageId = msg.MessageId,
				senderId = meId,
				receiverId = otherId,
				content = display, // ✅ 用遮罩後文字
				sentAtIso = msg.EditedAt.ToString("o")
			};

			await Clients.Group(UserGroup(meId)).SendAsync("ReceiveDirect", payload);
			await Clients.Group(UserGroup(otherId)).SendAsync("ReceiveDirect", payload);

			// 對方未讀更新
			await BroadcastUnreadAsync(otherId, meId);

			return new { ok = true, messageId = msg.MessageId, atIso = msg.EditedAt.ToString("o") };
		}

		/// <summary>只做讀回執推播；實際清未讀由 HTTP/With 處理</summary>
		public async Task NotifyRead(int otherId, string? upToIso)
		{
			var (meId, _) = GetIdentity();
			if (meId <= 0 || otherId <= 0) return;

			if (!string.IsNullOrWhiteSpace(upToIso) &&
				DateTime.TryParse(upToIso, null, DateTimeStyles.RoundtripKind, out _))
			{
				await Clients.Group(UserGroup(otherId)).SendAsync("ReadReceipt", new { fromUserId = meId, upToIso });
			}
			else
			{
				await Clients.Group(UserGroup(otherId)).SendAsync("ReadReceipt", new { fromUserId = meId, upToIso = (string?)null });
			}
		}

		public Task RegisterUser(string displayName)
		{
			var (meId, _) = GetIdentity();
			if (meId > 0 && !string.IsNullOrWhiteSpace(displayName))
				_logger.LogInformation("User {Uid} name: {Name}", meId, displayName.Trim());
			return Task.CompletedTask;
		}

		// ===== helpers =====
		private (int meId, bool isManager) GetIdentity()
		{
			var http = Context.GetHttpContext();
			var idStr = http?.Request.Cookies["gs_id"] ?? http?.Request.Cookies["sh_uid"] ?? "0";
			int.TryParse(idStr, out var id);
			var kind = (http?.Request.Cookies["gs_kind"] ?? "user").ToLowerInvariant();
			return (id, kind == "manager");
		}

		private static string UserGroup(int userId) => $"U_{userId}";

		private async Task<DmConversation> GetOrCreateConversationAsync(int a, int b)
		{
			var conv = await _db.DmConversations
				.FirstOrDefaultAsync(c => !c.IsManagerDm &&
					((c.Party1Id == a && c.Party2Id == b) || (c.Party1Id == b && c.Party2Id == a)));
			if (conv != null) return conv;

			var p1 = Math.Min(a, b);
			var p2 = Math.Max(a, b);
			conv = new DmConversation
			{
				IsManagerDm = false,
				Party1Id = p1,
				Party2Id = p2,
				CreatedAt = DateTime.UtcNow
			};
			_db.DmConversations.Add(conv);
			try
			{
				await _db.SaveChangesAsync();
				return conv;
			}
			catch
			{
				return await _db.DmConversations
					.FirstAsync(c => !c.IsManagerDm && c.Party1Id == p1 && c.Party2Id == p2);
			}
		}

		private async Task<(int total, int peerUnread)> ComputeUnreadAsync(int userId, int peerId)
		{
			var total = await _db.DmMessages
				.Where(m => !m.IsRead &&
					!m.Conversation.IsManagerDm &&
					(
						(m.Conversation.Party1Id == userId && !m.SenderIsParty1) ||
						(m.Conversation.Party2Id == userId && m.SenderIsParty1)
					))
				.CountAsync();

			var conv = await _db.DmConversations
				.FirstOrDefaultAsync(c => !c.IsManagerDm &&
					((c.Party1Id == userId && c.Party2Id == peerId) ||
					 (c.Party1Id == peerId && c.Party2Id == userId)));

			var peerUnread = 0;
			if (conv != null)
			{
				var iAmP1 = (conv.Party1Id == userId);
				peerUnread = await _db.DmMessages
					.Where(m => m.ConversationId == conv.ConversationId && !m.IsRead && m.SenderIsParty1 != iAmP1)
					.CountAsync();
			}
			return (total, peerUnread);
		}

		private async Task BroadcastUnreadAsync(int userId, int peerId)
		{
			var (total, peerUnread) = await ComputeUnreadAsync(userId, peerId);
			await Clients.Group(UserGroup(userId))
				.SendAsync("UnreadUpdate", new { total, peerId, unread = peerUnread });
		}
	}
}
