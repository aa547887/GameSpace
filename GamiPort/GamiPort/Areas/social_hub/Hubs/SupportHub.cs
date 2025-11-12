// =============================================================
// File: Areas/social_hub/Hubs/SupportHub.cs
// Purpose:
//   前台(GamiPort)的客服 SignalR Hub：讓「前台使用者」與「後台管理員」加入同一工單群組，
//   並且提供一組僅供「後台服務端」呼叫的伺服器方法（以 access_token=JoinSecret 授權）。
//
// Highlights:
//   - 使用者 Join(ticketId)：驗證 ticket.UserId == 目前登入使用者
//   - 管理員 JoinAsManager(ticketId, managerId, expires, sig)：HMAC 簽章 + DB 授權
//   - 管理員 NudgeAsManager(...)：同簽章授權，用於催刷新
//   - 後台跨站推播：ServerSendToTicketMessage(dto)（需 access_token == Support:JoinSecret）
//   - 事件命名：同時推 "msg"（輕量）與 "ticket.message"（完整 payload）
// Security:
//   - 後台服務端需以 SignalR.Client 連線，並在 WithUrl(...) 以 AccessTokenProvider 傳入 JoinSecret。
//   - 本 Hub 僅對伺服器方法（ServerSend*）檢查 access_token，瀏覽器端一般連線不需此值。
// Time:
//   - DB 存 UTC；此 Hub 僅傳遞資料，不做時區轉換。
// =============================================================

using GamiPort.Infrastructure.Security;               // IAppCurrentUser
using GamiPort.Models;                               // GameSpacedatabaseContext / SupportTicket / Assignments / Messages
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

// ★ 若你的 DTO 放在 Abstractions 命名空間，引用它；
//   名稱以你專案實際為準（下方用 SupportMessageDto）。
using GamiPort.Areas.social_hub.Services.Abstractions;

namespace GamiPort.Areas.social_hub.Hubs
{
	/// <summary>
	/// SupportHub（前台）；讓使用者與管理員共享 ticket 群組；提供後台跨站推播入口。
	/// </summary>
	public sealed class SupportHub : Hub
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IConfiguration _cfg;
		private readonly IAppCurrentUser _me;

		// 管理員簽章有效視窗（秒）：過期/超前太多都拒絕，避免重放
		private const int SIG_MAX_FUTURE_SKEW_SECONDS = 120;

		public SupportHub(GameSpacedatabaseContext db, IConfiguration cfg, IAppCurrentUser me)
		{
			_db = db;
			_cfg = cfg;
			_me = me;
		}

		private static string GroupName(int ticketId) => $"ticket:{ticketId}";

		// =====================================================
		// 使用者加入（同站 Cookie 驗證）
		// =====================================================
		/// <summary>使用者加入自己的工單群組。</summary>
		public async Task Join(int ticketId)
		{
			var meUserId = await _me.GetUserIdAsync();
			if (meUserId <= 0)
				throw new HubException("Unauthenticated.");

			var ticket = await _db.SupportTickets
				.AsNoTracking()
				.Where(t => t.TicketId == ticketId)
				.Select(t => new { t.TicketId, t.UserId })
				.FirstOrDefaultAsync();

			if (ticket is null)
				throw new HubException("Ticket not found.");

			if (ticket.UserId != meUserId)
				throw new HubException("Forbidden: not your ticket.");

			await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(ticketId));
			await Clients.Caller.SendAsync("joined", ticketId, new { asManager = false });
		}

		// =====================================================
		// 管理員加入（跨站；簽章 + DB 授權）
		// =====================================================
		/// <summary>
		/// 管理員加入指定工單群組（需 HMAC 簽章 + DB 授權）。
		/// expires 為 Unix 秒；sig = HMACSHA256(ticketId|managerId|expires, JoinSecret) 的 hex。
		/// </summary>
		public async Task JoinAsManager(int ticketId, int managerId, long expires, string sig)
		{
			if (!VerifySignature(ticketId, managerId, expires, sig))
				throw new HubException("Invalid signature or expired.");

			if (!await IsManagerEligibleAsync(ticketId, managerId))
				throw new HubException("Manager not authorized for this ticket.");

			await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(ticketId));
			await Clients.Caller.SendAsync("joined", ticketId, new { asManager = true });
		}

		/// <summary>
		/// 管理員 nudge：用在後台送訊息成功後，立即通知群組刷新（與 JoinAsManager 相同授權）。
		/// </summary>
		public async Task NudgeAsManager(int ticketId, int managerId, long expires, string sig)
		{
			if (!VerifySignature(ticketId, managerId, expires, sig))
				throw new HubException("Invalid signature or expired.");

			if (!await IsManagerEligibleAsync(ticketId, managerId))
				throw new HubException("Manager not authorized for this ticket.");

			await BroadcastTicketChangedAsync(ticketId);
		}

		// =====================================================
		// 後台跨站推播（僅伺服器可呼叫）
		// =====================================================
		// SupportHub.cs
		public async Task ServerSendToTicketMessage(SupportMessageDto dto)
		{
			if (!IsFromBackend(out var reason))
				throw new HubException("Forbidden: " + reason);

			if (dto is null || dto.TicketId <= 0)
				throw new HubException("Invalid payload.");

			var group = Clients.Group(GroupName(dto.TicketId));

			// ✅ 僅送「ticket.message」：前台就只會 append 一次
			await group.SendAsync("ticket.message", dto, CancellationToken.None);

		}

		// =====================================================
		// 離開群組
		// =====================================================
		public Task Leave(int ticketId)
			=> Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(ticketId));

		// =====================================================
		// 私有工具：授權/廣播/HMAC
		// =====================================================

		/// <summary>
		/// 僅用於辨識「後台服務端」連線：
		/// - 優先讀取 query: access_token
		/// - 其次讀取 Authorization: Bearer &lt;token&gt;
		/// 回傳是否通過，並輸出失敗原因（便於你現在除錯）
		/// </summary>
		private bool IsFromBackend(out string reason)
		{
			reason = "unknown";

			var http = Context.GetHttpContext();
			if (http is null) { reason = "no http context"; return false; }

			var secret = (_cfg["Support:JoinSecret"] ?? "").Trim();
			if (string.IsNullOrEmpty(secret)) { reason = "server secret empty"; return false; }

			// 1) query: access_token
			var token = http.Request.Query["access_token"].ToString()?.Trim();

			// 2) Authorization: Bearer <token>
			if (string.IsNullOrEmpty(token))
			{
				var authz = http.Request.Headers["Authorization"].ToString();
				if (!string.IsNullOrEmpty(authz) && authz.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
					token = authz.Substring("Bearer ".Length).Trim();
			}

			if (string.IsNullOrEmpty(token)) { reason = "no token provided"; return false; }

			// 嚴格比對
			var ok = string.Equals(token, secret, StringComparison.Ordinal);
			reason = ok ? "ok" : "token not match";
			return ok;
		}

		/// <summary>
		/// 管理員是否具備查看/操作此工單之權限：
		/// 1) 目前指派者
		/// 2) 歷史最近一次指派者（某些流程需要）
		/// 3) 具主管/全域檢視權（HasSupervisorPermissionAsync）
		/// </summary>
		private async Task<bool> IsManagerEligibleAsync(int ticketId, int managerId)
		{
			var ticket = await _db.SupportTickets
				.AsNoTracking()
				.Where(t => t.TicketId == ticketId)
				.Select(t => new { t.AssignedManagerId })
				.FirstOrDefaultAsync();

			if (ticket is null) return false;

			// 1) 現任負責人
			if (ticket.AssignedManagerId.HasValue && ticket.AssignedManagerId.Value == managerId)
				return true;

			// 2) 歷史最近一次指派者
			var lastAssignee = await _db.SupportTicketAssignments
				.AsNoTracking()
				.Where(a => a.TicketId == ticketId)
				.OrderByDescending(a => a.AssignedAt).ThenByDescending(a => a.AssignmentId)
				.Select(a => a.ToManagerId)
				.FirstOrDefaultAsync();

			if (lastAssignee == managerId)
				return true;

			// 3) 主管/全域權限
			if (await HasSupervisorPermissionAsync(managerId))
				return true;

			return false;
		}

		/// <summary>
		/// 對該工單群組發出「有更新」訊號：同時送 "msg" 與 "ticket.message"。
		/// </summary>
		private Task BroadcastTicketChangedAsync(int ticketId)
		{
			var payload = new { ticketId };
			var group = Clients.Group(GroupName(ticketId));
			var t1 = group.SendAsync("msg", payload);
			var t2 = group.SendAsync("ticket.message", payload);
			return Task.WhenAll(t1, t2);
		}

		/// <summary>
		/// 管理員簽章驗證（HMACSHA256(ticketId|managerId|expires, JoinSecret)）。
		/// - expires：Unix 秒；過期或超前太多都拒絕，避免重放。
		/// </summary>
		private bool VerifySignature(int ticketId, int managerId, long expires, string sigHex)
		{
			var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			if (expires < now) return false;                                   // 已過期
			if (expires - now > SIG_MAX_FUTURE_SKEW_SECONDS) return false;     // 時間窗過大（防重放）

			var secret = _cfg["Support:JoinSecret"];
			if (string.IsNullOrWhiteSpace(secret)) return false;

			var data = $"{ticketId}|{managerId}|{expires}";
			using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
			var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
			var provided = HexToBytes(sigHex);

			if (provided is null || provided.Length != computed.Length) return false;
			return CryptographicOperations.FixedTimeEquals(computed, provided);
		}

		private static byte[]? HexToBytes(string hex)
		{
			if (string.IsNullOrWhiteSpace(hex)) return null;
			if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
				hex = hex[2..];
			if (hex.Length % 2 != 0) return null;

			try { return Convert.FromHexString(hex); }
			catch { return null; }
		}

		/// <summary>
		/// TODO：依你的權限系統實作（如查某張 ManagerRole/Claim 是否具 Support.Supervisor）。
		/// 現暫時回 false。
		/// </summary>
		private Task<bool> HasSupervisorPermissionAsync(int managerId)
			=> Task.FromResult(false);
	}
}
