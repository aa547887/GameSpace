using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using GameSpace.Models;
using GameSpace.Areas.social_hub.Services;

namespace GameSpace.Areas.social_hub.Hubs
{
	public class ChatHub : Hub
	{
		private static readonly ConcurrentDictionary<string, string> _connName = new();
		private readonly GameSpacedatabaseContext _db;
		private readonly IMuteFilter _mutes;

		public ChatHub(GameSpacedatabaseContext db, IMuteFilter mutes)
		{
			_db = db;
			_mutes = mutes;
		}

		private static string UG(int uid) => $"u:{uid}";

		public override async Task OnConnectedAsync()
		{
			var http = Context.GetHttpContext();
			if (http?.Request.Cookies.TryGetValue("sh_uid", out var uidStr) == true
				&& int.TryParse(uidStr, out var uid) && uid > 0)
			{
				await Groups.AddToGroupAsync(Context.ConnectionId, UG(uid));
			}
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			_connName.TryRemove(Context.ConnectionId, out _);
			var http = Context.GetHttpContext();
			if (http?.Request.Cookies.TryGetValue("sh_uid", out var uidStr) == true
				&& int.TryParse(uidStr, out var uid) && uid > 0)
			{
				await Groups.RemoveFromGroupAsync(Context.ConnectionId, UG(uid));
			}
			await base.OnDisconnectedAsync(exception);
		}

		public Task RegisterUser(string displayName)
		{
			_connName[Context.ConnectionId] = string.IsNullOrWhiteSpace(displayName) ? "匿名" : displayName.Trim();
			return Task.CompletedTask;
		}

		public async Task SendMessageTo(int receiverId, string message)
		{
			var http = Context.GetHttpContext();

			if (!(http?.Request.Cookies.TryGetValue("sh_uid", out var uidStr) == true
				&& int.TryParse(uidStr, out var senderId) && senderId > 0))
			{
				await Clients.Caller.SendAsync("Error", "NOT_LOGGED_IN");
				return;
			}
			if (receiverId <= 0)
			{
				await Clients.Caller.SendAsync("Error", "NO_PEER");
				return;
			}

			var senderName = _connName.TryGetValue(Context.ConnectionId, out var n) ? n : "匿名";
			var content = (message ?? "").Trim();
			if (string.IsNullOrEmpty(content)) return;
			if (content.Length > 100) content = content[..100];

			// ★ 關鍵：過濾敏感詞（從 Mutes 表）
			content = await _mutes.FilterAsync(content);

			var now = DateTime.UtcNow;
			var row = new ChatMessage
			{
				SenderId = senderId,
				ReceiverId = receiverId,
				ChatContent = content,
				SentAt = now,
				IsRead = false,
				IsSent = true
			};
			_db.ChatMessages.Add(row);
			await _db.SaveChangesAsync();

			var forSender = new
			{
				id = row.MessageId,
				fromId = senderId,
				fromName = senderName,
				toId = receiverId,
				content,
				timeIso = now.ToString("o"),
				isMine = true,
				isRead = false
			};
			var forReceiver = new
			{
				id = row.MessageId,
				fromId = senderId,
				fromName = senderName,
				toId = receiverId,
				content,
				timeIso = now.ToString("o"),
				isMine = false,
				isRead = false
			};

			await Clients.Group(UG(senderId)).SendAsync("ReceiveDirect", forSender);
			await Clients.Group(UG(receiverId)).SendAsync("ReceiveDirect", forReceiver);
		}

		public async Task NotifyRead(int partnerId, string upToIso)
		{
			var http = Context.GetHttpContext();
			if (http?.Request.Cookies.TryGetValue("sh_uid", out var meStr) != true
				|| !int.TryParse(meStr, out var me) || me <= 0)
				return;

			await Clients.Group(UG(partnerId)).SendAsync("ReadReceipt", new { fromUserId = me, upToIso });
		}

		public async Task SendDirect(int receiverId, string content)
		{
			var http = Context.GetHttpContext();
			if (http?.Request.Cookies.TryGetValue("sh_uid", out var meStr) != true
				|| !int.TryParse(meStr, out var me) || me <= 0)
				return;

			var text = content ?? string.Empty;

			// ✅ 先過濾
			var filtered = await _mutes.FilterAsync(text);

			// ✅ 入庫用「已過濾」內容
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

			var payload = new
			{
				messageId = msg.MessageId,
				senderId = me,
				receiverId,
				content = filtered,
				sentAtIso = msg.SentAt.ToString("o")
			};

			// ✅ 廣播也是用「已過濾」內容（連自己回顯也一樣）
			await Clients.Group(UG(me)).SendAsync("ReceiveDirect", payload);
			await Clients.Group(UG(receiverId)).SendAsync("ReceiveDirect", payload);
		}

	}
}
