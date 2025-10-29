using System.Threading;
using System.Threading.Tasks;
using GamiPort.Areas.social_hub.Hubs;
using GamiPort.Areas.social_hub.Services.Abstractions;
using GamiPort.Areas.social_hub.SignalR;              // ChatEvents / GroupNames
using GamiPort.Areas.social_hub.SignalR.Contracts;     // Payload 型別
using Microsoft.AspNetCore.SignalR;

namespace GamiPort.Areas.social_hub.Services.Application
{
	/// <summary>
	/// SignalR 推播實作：
	/// - 透過 IHubContext&lt;ChatHub&gt; 發送到「使用者個人群組」(GroupNames.User(userId))。
	/// - 不更動 payload 內容；payload 來自 ChatService，已是「遮蔽後」。
	/// </summary>
	public sealed class SignalRChatNotifier : IChatNotifier
	{
		private readonly IHubContext<ChatHub> _hub;
		public SignalRChatNotifier(IHubContext<ChatHub> hub) => _hub = hub;

		/// <summary>【新訊息推播】雙向（A 與 B 各自的個人群組都會收到）。</summary>
		public async Task BroadcastReceiveDirectAsync(int userAId, int userBId, DirectMessagePayload payload, CancellationToken ct = default)
		{
			await _hub.Clients.Group(GroupNames.User(userBId))
				.SendAsync(ChatEvents.ReceiveDirect, payload, ct);

			await _hub.Clients.Group(GroupNames.User(userAId))
				.SendAsync(ChatEvents.ReceiveDirect, payload, ct);
		}

		/// <summary>【已讀回執】雙向（A 與 B 各自的個人群組都會收到）。</summary>
		public async Task BroadcastReadReceiptAsync(int userAId, int userBId, ReadReceiptPayload payload, CancellationToken ct = default)
		{
			await _hub.Clients.Group(GroupNames.User(userBId))
				.SendAsync(ChatEvents.ReadReceipt, payload, ct);

			await _hub.Clients.Group(GroupNames.User(userAId))
				.SendAsync(ChatEvents.ReadReceipt, payload, ct);
		}

		/// <summary>【未讀統計】單向（只對目標 userId 推播）。</summary>
		public async Task BroadcastUnreadAsync(int userId, UnreadUpdatePayload payload, CancellationToken ct = default)
		{
			await _hub.Clients.Group(GroupNames.User(userId))
				.SendAsync(ChatEvents.UnreadUpdate, payload, ct);
		}
	}
}
