using GamiPort.Areas.social_hub.Hubs;
using GamiPort.Areas.social_hub.Services.Abstractions;
using Microsoft.AspNetCore.SignalR;
using System.Threading;

namespace GamiPort.Areas.social_hub.Services.Application
{
	/// <summary>
	/// 用 IHubContext 廣播到 "ticket:{id}" 群組。
	/// 同時發出兩種事件：
	/// - "msg"：輕量事件，部分頁面只用它觸發刷新
	/// - "ticket.message"：完整 payload，能直接 append 泡泡
	/// </summary>
	public sealed class SignalRSupportNotifier : ISupportNotifier
	{
		private readonly IHubContext<SupportHub> _hub;
		public SignalRSupportNotifier(IHubContext<SupportHub> hub) => _hub = hub;

		public Task BroadcastMessageAsync(SupportMessageDto msg, CancellationToken ct = default)
		{
			var group = $"ticket:{msg.TicketId}";
			// 建議兩個事件都發，提升相容性
			var t1 = _hub.Clients.Group(group).SendAsync("msg", new { ticketId = msg.TicketId }, ct);
			var t2 = _hub.Clients.Group(group).SendAsync("ticket.message", msg, ct);
			return Task.WhenAll(t1, t2);
		}
	}
}
