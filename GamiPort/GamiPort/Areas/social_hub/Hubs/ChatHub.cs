using System.Globalization;
using GamiPort.Areas.social_hub.Services.Abstractions;
using GamiPort.Areas.social_hub.SignalR;
using GamiPort.Areas.social_hub.SignalR.Contracts;
using GamiPort.Infrastructure.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GamiPort.Areas.social_hub.Hubs
{
	[Authorize]
	public sealed class ChatHub : Hub
	{
		private readonly ILoginIdentity _login;
		private readonly IChatService _svc;
		private readonly IChatNotifier _notify;

		public ChatHub(ILoginIdentity login, IChatService svc, IChatNotifier notify)
		{ _login = login; _svc = svc; _notify = notify; }

		private async Task<int> GetMeAsync()
		{
			var id = await _login.GetAsync();
			return id.IsAuthenticated && id.UserId is > 0 ? id.UserId.Value : 0;
		}

		public override async Task OnConnectedAsync()
		{
			var me = await GetMeAsync();
			if (me > 0) await Groups.AddToGroupAsync(Context.ConnectionId, GroupNames.User(me));
			await base.OnConnectedAsync();
		}
		public override async Task OnDisconnectedAsync(Exception? ex)
		{
			var me = await GetMeAsync();
			if (me > 0) await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupNames.User(me));
			await base.OnDisconnectedAsync(ex);
		}

		public async Task<DirectMessagePayload> SendMessageTo(int otherId, string text)
		{
			var me = await GetMeAsync();
			if (me <= 0) throw new HubException(ErrorCodes.NotLoggedIn);
			if (otherId <= 0 || otherId == me) throw new HubException(ErrorCodes.NoPeer);

			var payload = await _svc.SendDirectAsync(me, otherId, text);
			await _notify.BroadcastReceiveDirectAsync(me, otherId, payload);

			// 未讀更新：雙方
			var (totalMe, peerMe) = await _svc.ComputeUnreadAsync(me, otherId);
			await _notify.BroadcastUnreadAsync(me, new UnreadUpdatePayload { PeerId = otherId, Unread = peerMe, Total = totalMe });
			var (totalOt, peerOt) = await _svc.ComputeUnreadAsync(otherId, me);
			await _notify.BroadcastUnreadAsync(otherId, new UnreadUpdatePayload { PeerId = me, Unread = peerOt, Total = totalOt });

			return payload;
		}

		public async Task<ReadReceiptPayload> NotifyRead(int otherId, string upToIso)
		{
			var me = await GetMeAsync();
			if (me <= 0) throw new HubException(ErrorCodes.NotLoggedIn);
			if (otherId <= 0 || otherId == me) throw new HubException(ErrorCodes.NoPeer);

			if (!DateTime.TryParse(upToIso, null, DateTimeStyles.RoundtripKind, out var upToUtc))
				upToUtc = DateTime.UtcNow;

			var receipt = await _svc.MarkReadAsync(me, otherId, upToUtc);
			await _notify.BroadcastReadReceiptAsync(me, otherId, receipt);

			// 未讀更新：雙方
			var (totalMe, peerMe) = await _svc.ComputeUnreadAsync(me, otherId);
			await _notify.BroadcastUnreadAsync(me, new UnreadUpdatePayload { PeerId = otherId, Unread = peerMe, Total = totalMe });
			var (totalOt, peerOt) = await _svc.ComputeUnreadAsync(otherId, me);
			await _notify.BroadcastUnreadAsync(otherId, new UnreadUpdatePayload { PeerId = me, Unread = peerOt, Total = totalOt });

			return receipt;
		}

		/// <summary>前端連線完成，主動請求目前「全站未讀總數」</summary>
		public async Task RefreshUnread()
		{
			var me = await GetMeAsync();
			if (me <= 0) throw new HubException(ErrorCodes.NotLoggedIn);
			var total = await _svc.ComputeTotalUnreadAsync(me);
			await _notify.BroadcastUnreadAsync(me, new UnreadUpdatePayload { PeerId = 0, Unread = 0, Total = total });
		}

		/// <summary>一次回傳多位好友的未讀統計（以及全站總未讀）。</summary>
		public async Task<object> GetUnreadForPeers(int[] peerIds)
		{
			var me = await GetMeAsync();
			if (me <= 0) throw new HubException(ErrorCodes.NotLoggedIn);

			var result = new List<object>();
			var uniq = (peerIds ?? Array.Empty<int>()).Where(x => x > 0 && x != me).Distinct();

			foreach (var pid in uniq)
			{
				var (_, peer) = await _svc.ComputeUnreadAsync(me, pid);
				result.Add(new { peerId = pid, unread = peer });
			}

			var total = await _svc.ComputeTotalUnreadAsync(me);
			return new { total, peers = result };
		}
	}
}
