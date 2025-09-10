using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameSpace.Models;
using GameSpace.Areas.social_hub.Services;
using Microsoft.AspNetCore.SignalR;

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
			{
				await Groups.AddToGroupAsync(Context.ConnectionId, UG(me));
			}
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			if (TryGetUserIdFromCookies(Context.GetHttpContext(), out var me) && me > 0)
			{
				await Groups.RemoveFromGroupAsync(Context.ConnectionId, UG(me));
			}
			await base.OnDisconnectedAsync(exception);
		}

		// ========== Helpers ==========
		private static bool TryGetUserIdFromCookies(Microsoft.AspNetCore.Http.HttpContext? http, out int uid)
		{
			uid = 0;
			if (http == null) return false;

			// 先 sh_uid（相容舊測試），再 gs_id（正式）
			if (http.Request.Cookies.TryGetValue("sh_uid", out var v1) &&
				int.TryParse(v1, out uid) && uid > 0)
				return true;

			if (http.Request.Cookies.TryGetValue("gs_id", out var v2) &&
				int.TryParse(v2, out uid) && uid > 0)
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

		// ========== Public Hub APIs ==========

		// 只是暱稱顯示（非權限）—客端可傳 displayName
		public Task RegisterUser(string displayName)
		{
			_connName[Context.ConnectionId] = string.IsNullOrWhiteSpace(displayName)
				? "匿名"
				: displayName.Trim();
			return Task.CompletedTask;
		}

		// 讀取回執：客户端呼叫 → 廣播給對方群組
		public async Task NotifyRead(int partnerId, string upToIso)
		{
			if (!TryGetUserIdFromCookies(Context.GetHttpContext(), out var me) || me <= 0) return;
			await Clients.Group(UG(partnerId)).SendAsync("ReadReceipt", new { fromUserId = me, upToIso });
		}

		// 主要發送 API（沿用你前端的 SendMessageTo）
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
			if (text.Length > 100) text = text[..100];

			// 先過濾
			var filtered = await _mutes.FilterAsync(text);

			// 入庫（存已過濾內容）
			var msg = new ChatMessage
			{
				SenderId = me,
				ReceiverId = receiverId,
				ChatContent = filtered,
				SentAt = DateTime.UtcNow,
				IsRead = false,
				IsSent = true
			};
			_db.ChatMessages.Add(msg);
			await _db.SaveChangesAsync();

			// 廣播（群組：自己與對方）
			var payload = new
			{
				messageId = msg.MessageId,
				senderId = me,
				receiverId = receiverId,
				content = filtered,
				sentAtIso = msg.SentAt.ToString("o")
			};

			await Clients.Group(UG(me)).SendAsync("ReceiveDirect", payload);
			await Clients.Group(UG(receiverId)).SendAsync("ReceiveDirect", payload);
		}

		// 若客端還有人呼叫 SendDirect，就轉用 SendMessageTo，保持相容
		public Task SendDirect(int receiverId, string content) => SendMessageTo(receiverId, content);
	}
}
