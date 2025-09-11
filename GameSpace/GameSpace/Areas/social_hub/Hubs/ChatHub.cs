using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameSpace.Models;
using GameSpace.Areas.social_hub.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.social_hub.Hubs
{
	public class ChatHub : Hub
	{
		private static readonly ConcurrentDictionary<string, string> _connName = new();
		private static readonly ConcurrentDictionary<int, Queue<DateTime>> _rate = new();

		private readonly GameSpacedatabaseContext _db;
		private readonly IMuteFilter _mutes;

		public ChatHub(GameSpacedatabaseContext db, IMuteFilter mutes)
		{
			_db = db;
			_mutes = mutes;
		}

		private static string UG(int uid) => $"U_{uid}";

		public override async Task OnConnectedAsync()
		{
			if (TryGetUserIdFromCookies(Context.GetHttpContext(), out var me) && me > 0)
				await Groups.AddToGroupAsync(Context.ConnectionId, UG(me));
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			if (TryGetUserIdFromCookies(Context.GetHttpContext(), out var me) && me > 0)
				await Groups.RemoveFromGroupAsync(Context.ConnectionId, UG(me));
			await base.OnDisconnectedAsync(exception);
		}

		// ========= Helpers =========
		private static bool TryGetUserIdFromCookies(Microsoft.AspNetCore.Http.HttpContext? http, out int uid)
		{
			uid = 0;
			if (http == null) return false;

			// 先舊測試 cookie，再正式 cookie
			if (http.Request.Cookies.TryGetValue("sh_uid", out var v1) && int.TryParse(v1, out uid) && uid > 0)
				return true;
			if (http.Request.Cookies.TryGetValue("gs_id", out var v2) && int.TryParse(v2, out uid) && uid > 0)
				return true;
			return false;
		}

		private static bool HitRateLimit(int uid, int limit = 8, int windowSec = 10)
		{
			var q = _rate.GetOrAdd(uid, _ => new Queue<DateTime>());
			var now = DateTime.UtcNow;
			lock (q)
			{
				while (q.Count > 0 && (now - q.Peek()).TotalSeconds > windowSec) q.Dequeue();
				if (q.Count >= limit) return true;
				q.Enqueue(now);
				return false;
			}
		}

		private async Task<DmConversation> GetOrCreateConversationAsync(int me, int other)
		{
			// 目前僅處理使用者-使用者（is_manager_dm=false）
			var p1 = Math.Min(me, other);
			var p2 = Math.Max(me, other);

			var conv = await _db.DmConversations
				.SingleOrDefaultAsync(c => c.IsManagerDm == false && c.Party1Id == p1 && c.Party2Id == p2);

			if (conv == null)
			{
				conv = new DmConversation
				{
					IsManagerDm = false,
					Party1Id = p1,
					Party2Id = p2,
					CreatedAt = DateTime.UtcNow
				};
				_db.DmConversations.Add(conv);
				await _db.SaveChangesAsync();
			}
			return conv;
		}

		private static bool AmIParty1(int me, DmConversation conv) => me == conv.Party1Id;

		// ========= Public Hub APIs =========

		// 只是暱稱顯示（非權限）
		public Task RegisterUser(string displayName)
		{
			_connName[Context.ConnectionId] = string.IsNullOrWhiteSpace(displayName) ? "匿名" : displayName.Trim();
			return Task.CompletedTask;
		}

		// 已讀回執：更新 DB 並通知對方
		public async Task NotifyRead(int partnerId, string upToIso)
		{
			if (!TryGetUserIdFromCookies(Context.GetHttpContext(), out var me) || me <= 0) return;

			if (!DateTime.TryParse(upToIso, out var upTo)) upTo = DateTime.UtcNow;

			var conv = await GetOrCreateConversationAsync(me, partnerId);
			var iAmP1 = AmIParty1(me, conv);
			var now = DateTime.UtcNow;

			var toUpdate = await _db.DmMessages
				.Where(m => m.ConversationId == conv.ConversationId
						 && m.ReadAt == null
						 && m.SenderIsParty1 != iAmP1
						 && m.EditedAt <= upTo)
				.ToListAsync();

			foreach (var m in toUpdate)
			{
				m.ReadAt = now;     // 以時間戳為「已讀」判斷依據
				m.IsRead = true;    // 若模型有此欄位就同步設置
			}
			if (toUpdate.Count > 0) await _db.SaveChangesAsync();

			await Clients.Group(UG(partnerId)).SendAsync("ReadReceipt", new { fromUserId = me, upToIso = upTo.ToString("o") });
		}

		// 主要發送 API（取代 ChatMessages）
		public async Task SendMessageTo(int receiverId, string content)
		{
			if (!TryGetUserIdFromCookies(Context.GetHttpContext(), out var me) || me <= 0)
			{
				await Clients.Caller.SendAsync("Error", "NOT_LOGGED_IN");
				return;
			}
			if (receiverId <= 0)
			{
				await Clients.Caller.SendAsync("Error", "NO_PEER");
				return;
			}
			if (HitRateLimit(me))
			{
				await Clients.Caller.SendAsync("Error", "RATE_LIMIT");
				return;
			}

			var text = (content ?? string.Empty).Trim();
			if (text.Length > 255) text = text[..255]; // DM_Messages.message_text = nvarchar(255)

			// 穢語過濾
			var filtered = await _mutes.FilterAsync(text);

			// 找/建 1對1 對話
			var conv = await GetOrCreateConversationAsync(me, receiverId);
			var iAmP1 = AmIParty1(me, conv);

			// 入庫（DM_Messages）
			var msg = new DmMessage
			{
				ConversationId = conv.ConversationId,
				SenderIsParty1 = iAmP1,
				MessageText = filtered,
				EditedAt = DateTime.UtcNow,
				IsRead = false // 若模型有欄位就保留；已讀以 ReadAt 判斷
			};
			_db.DmMessages.Add(msg);
			await _db.SaveChangesAsync();

			// 廣播（群組：自己與對方）
			var payload = new
			{
				messageId = msg.MessageId,
				conversationId = conv.ConversationId,
				senderId = me,
				receiverId = receiverId,
				content = filtered,
				sentAtIso = msg.EditedAt.ToString("o")
			};

			await Clients.Group(UG(me)).SendAsync("ReceiveDirect", payload);
			await Clients.Group(UG(receiverId)).SendAsync("ReceiveDirect", payload);
		}

		// 保持相容
		public Task SendDirect(int receiverId, string content) => SendMessageTo(receiverId, content);
	}
}
