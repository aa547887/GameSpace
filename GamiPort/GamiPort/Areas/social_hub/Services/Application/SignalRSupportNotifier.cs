using GamiPort.Areas.social_hub.Hubs;
using GamiPort.Areas.social_hub.Services.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace GamiPort.Areas.social_hub.Services.Application
{
	/// <summary>
	/// 用 IHubContext 廣播到 "ticket:{id}" 群組
	/// </summary>
	public sealed class SignalRSupportNotifier : ISupportNotifier
	{
		private readonly IHubContext<SupportHub> _hub;

		public SignalRSupportNotifier(IHubContext<SupportHub> hub) => _hub = hub;

		public Task BroadcastMessageAsync(SupportMessageDto msg, CancellationToken ct = default)
		{
			var group = $"ticket:{msg.TicketId}";
			// client 端用 connection.on('msg', handler) 接收
			return _hub.Clients.Group(group).SendAsync("msg", msg, ct);
		}
	}
}
