using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GamiPort.Areas.social_hub.Services.Abstractions;
using GamiPort.Areas.social_hub.SignalR;
using GamiPort.Areas.social_hub.SignalR.Contracts;
// ✅ 用我們的統一介面來「吃登入」
using GamiPort.Infrastructure.Security; // IAppCurrentUser
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GamiPort.Areas.social_hub.Hubs
{
	/// <summary>
	/// 一對一聊天的 SignalR Hub（即時傳送、已讀回報、未讀彙總）。
	/// 重點：
	/// - 送訊息與取歷史都交給 IChatService（DB 存原文、payload 已遮蔽）。
	/// - NotifyRead：只有資料庫真的有更新（rowsAffected > 0）才廣播回執與未讀更新，避免「假已讀」。
	/// </summary>
	[Authorize]
	public sealed class ChatHub : Hub<IChatClient>
	{
		private readonly IAppCurrentUser _me;
		private readonly IChatService _svc;
		private readonly ILogger<ChatHub> _logger;

		public ChatHub(IAppCurrentUser me, IChatService svc, ILogger<ChatHub> logger)
		{
			_me = me;
			_svc = svc;
			_logger = logger;
		}
		/// <summary>取得目前使用者 Id；優先讀取 Claims，再必要時做備援解析。</summary>
		private async Task<int> GetMeAsync()
		{
			// 快速路徑：_me.UserId 已經把 "AppUserId" Claim 轉成 int（沒有就回 0）
			var id = _me.UserId;
			return id > 0 ? id : await _me.GetUserIdAsync();
		}

		/// <summary>
		/// 連線建立時：把目前使用者加入「個人群組」，用於點對點推播。
		/// ⚠️ 用 try/catch 包住，避免生命週期未攔截例外直接讓伺服器關閉 WebSocket。
		/// </summary>
		public override async Task OnConnectedAsync()
		{
			try
			{
				var me = await GetMeAsync();
				if (me > 0)
				{
					await Groups.AddToGroupAsync(Context.ConnectionId, GroupNames.User(me));
					_logger.LogInformation("ChatHub connected. uid={Uid}, conn={ConnId}", me, Context.ConnectionId);
				}
				else
				{
					// 未登入或 Cookie 舊（沒有 AppUserId），不要丟例外，以免中止連線；記錄即可。
					_logger.LogWarning("ChatHub connected without valid user id. conn={ConnId}", Context.ConnectionId);
				}
			}
			catch (Exception ex)
			{
				// ★ 關鍵：不把例外往外拋，避免「Server returned an error on close」
				_logger.LogError(ex, "OnConnectedAsync failed. conn={ConnId}", Context.ConnectionId);
			}
			finally
			{
				await base.OnConnectedAsync();
			}
		}

		/// <summary>
		/// 連線中斷時：從個人群組移除。
		/// 同樣包 try/catch，確保這裡的例外不會再影響整體連線狀態回報。
		/// </summary>
		public override async Task OnDisconnectedAsync(Exception? ex)
		{
			try
			{
				var me = await GetMeAsync();
				if (me > 0)
					await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupNames.User(me));

				if (ex != null)
					_logger.LogWarning(ex, "ChatHub disconnected with error. conn={ConnId}");
				else
					_logger.LogInformation("ChatHub disconnected normally. conn={ConnId}", Context.ConnectionId);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "OnDisconnectedAsync failed. conn={ConnId}", Context.ConnectionId);
			}
			finally
			{
				await base.OnDisconnectedAsync(ex);
			}
		}

		/// <summary>
		/// 傳送訊息：服務層寫 DB（DB 存原文），回傳 payload（已遮蔽）→ 推播雙方 → 更新雙方未讀。
		/// </summary>
		public async Task<DirectMessagePayload> SendMessageTo(int otherId, string text)
		{
			var me = await GetMeAsync();
			if (me <= 0) throw new HubException(ErrorCodes.NotLoggedIn);
			if (otherId <= 0 || otherId == me) throw new HubException(ErrorCodes.NoPeer);

			// 服務層會裁切內容、建立對話、寫 DB，並回「已遮蔽」的 payload
			var payload = await _svc.SendDirectAsync(me, otherId, text);

			// 新訊息推播（雙向）
			await Clients.Group(GroupNames.User(me)).ReceiveDirect(payload);
			await Clients.Group(GroupNames.User(otherId)).ReceiveDirect(payload);

			// 未讀更新（雙向）
			var (totalMe, peerMe) = await _svc.ComputeUnreadAsync(me, otherId);
			await Clients.Group(GroupNames.User(me)).UnreadUpdate(new UnreadUpdatePayload { PeerId = otherId, Unread = peerMe, Total = totalMe });

			var (totalOt, peerOt) = await _svc.ComputeUnreadAsync(otherId, me);
			await Clients.Group(GroupNames.User(otherId)).UnreadUpdate(new UnreadUpdatePayload { PeerId = me, Unread = peerOt, Total = totalOt });

			return payload;
		}

		/// <summary>
		/// 我（me）對 otherId 的對話「已讀」。只有 DB 寫入成功才會廣播。
		/// </summary>
		public async Task<ReadReceiptPayload> NotifyRead(int otherId, string upToIso)
		{
			var me = await GetMeAsync();
			if (me <= 0) throw new HubException(ErrorCodes.NotLoggedIn);
			if (otherId <= 0 || otherId == me) throw new HubException(ErrorCodes.NoPeer);

			// upToIso 僅做顯示；DB 寫入不使用此時間條件（服務層以「全部未讀」標記為已讀）
			if (!DateTime.TryParse(upToIso, null, DateTimeStyles.RoundtripKind, out var upToUtc))
				upToUtc = DateTime.UtcNow;

			var rows = await _svc.MarkReadUpToAsync(me, otherId, null);
			if (rows <= 0)
			{
				_logger.LogDebug("[Hub] NotifyRead: no rows updated, skip broadcast. me={Me}, other={Other}", me, otherId);
				return new ReadReceiptPayload
				{
					FromUserId = me,
					UpToIso = upToUtc.ToString("o", CultureInfo.InvariantCulture),
					RowsAffected = 0
				};
			}

			var receipt = new ReadReceiptPayload
			{
				FromUserId = me,
				UpToIso = upToUtc.ToString("o", CultureInfo.InvariantCulture),
				RowsAffected = rows
			};

			// 已讀回執（雙向）
			await Clients.Group(GroupNames.User(me)).ReadReceipt(receipt);
			await Clients.Group(GroupNames.User(otherId)).ReadReceipt(receipt);

			// 未讀更新（雙向）
			var (totalMe, peerMe) = await _svc.ComputeUnreadAsync(me, otherId);
			await Clients.Group(GroupNames.User(me)).UnreadUpdate(new UnreadUpdatePayload { PeerId = otherId, Unread = peerMe, Total = totalMe });

			var (totalOt, peerOt) = await _svc.ComputeUnreadAsync(otherId, me);
			await Clients.Group(GroupNames.User(otherId)).UnreadUpdate(new UnreadUpdatePayload { PeerId = me, Unread = peerOt, Total = totalOt });

			return receipt;
		}
		/// <summary>回目前「全站未讀總數」。</summary>
		public async Task RefreshUnread()
		{
			var me = await GetMeAsync();
			if (me <= 0) throw new HubException(ErrorCodes.NotLoggedIn);

			var total = await _svc.ComputeTotalUnreadAsync(me);
			await Clients.User(me.ToString()).UnreadUpdate(new UnreadUpdatePayload { PeerId = 0, Unread = 0, Total = total });
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
