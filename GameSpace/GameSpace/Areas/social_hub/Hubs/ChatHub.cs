using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace GameSpace.Areas.social_hub.Hubs
{
	public class ChatHub : Hub
	{
		// 保存 ConnectionId 對應使用者名稱
		private static ConcurrentDictionary<string, string> _connections = new();

		// 使用者連線時設定名字
		public Task RegisterUser(string userName)
		{
			_connections[Context.ConnectionId] = userName;
			return Task.CompletedTask;
		}

		// 發送訊息
		public async Task SendMessage(string message)
		{
			if (_connections.TryGetValue(Context.ConnectionId, out var userName))
			{
				await Clients.All.SendAsync("ReceiveMessage", userName, message, DateTime.Now.ToString("HH:mm"));
			}
		}

		// 斷線時移除
		public override Task OnDisconnectedAsync(Exception? exception)
		{
			_connections.TryRemove(Context.ConnectionId, out _);
			return base.OnDisconnectedAsync(exception);
		}
	}
}
