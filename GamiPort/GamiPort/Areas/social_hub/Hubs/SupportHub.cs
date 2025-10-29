using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GamiPort.Models;                       // GameSpacedatabaseContext / SupportTicket / Assignments
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using GamiPort.Infrastructure.Security;      // IAppCurrentUser（你現有的服務）

namespace GamiPort.Areas.social_hub.Hubs
{
	/// <summary>
	/// SupportHub（仍在前台），讓「前台使用者」與「後台管理員」加入相同工單群組。
	/// - 使用者路線：Join(ticketId) → 只允許工單本人（Cookie/Claims 驗證）
	/// - 管理員路線：JoinAsManager(ticketId, managerId, expires, sig) → 簽章 + DB 授權檢查
	/// 事件命名：沿用你現有廣播的 "msg"（SignalRSupportNotifier 會發到 group: "ticket:{id}"）
	/// </summary>
	public sealed class SupportHub : Hub
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IConfiguration _config;
		private readonly IAppCurrentUser _me;

		// 簽章有效視窗（秒）：避免過期簽章被重放
		private const int SIG_MAX_FUTURE_SKEW_SECONDS = 120;

		public SupportHub(GameSpacedatabaseContext db, IConfiguration config, IAppCurrentUser me)
		{
			_db = db;
			_config = config;
			_me = me;
		}

		private static string GroupName(int ticketId) => $"ticket:{ticketId}";

		/// <summary>
		/// 使用者加入（同站 Cookie 驗證）
		/// 僅允許工單本人（ticket.UserId == meUserId）
		/// </summary>
		public async Task Join(int ticketId)
		{
			var meUserId = await _me.GetUserIdAsync(); // 你現有服務：Claims("AppUserId") 或備援解析
			if (meUserId <= 0)
				throw new HubException("Unauthenticated user.");

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

		/// <summary>
		/// 管理員加入（跨站，改採簽章驗證 + 伺服端授權判斷）
		/// - managerId / expires / sig 由後台頁面以相同密鑰產生
		/// - 伺服端另外檢查此 manager 是否「被指派」或具備主管權限（此處預留鉤子）
		/// </summary>
		public async Task JoinAsManager(int ticketId, int managerId, long expires, string sig)
		{
			if (!VerifySignature(ticketId, managerId, expires, sig))
				throw new HubException("Invalid signature or expired.");

			var ticket = await _db.SupportTickets
				.AsNoTracking()
				.Where(t => t.TicketId == ticketId)
				.Select(t => new { t.TicketId, t.AssignedManagerId })
				.FirstOrDefaultAsync();

			if (ticket is null)
				throw new HubException("Ticket not found.");

			var eligible = false;

			// 1) 目前指派者
			if (ticket.AssignedManagerId.HasValue && ticket.AssignedManagerId.Value == managerId)
			{
				eligible = true;
			}
			else
			{
				// 2) 歷史最近一次指派者
				var lastAssignee = await _db.SupportTicketAssignments
					.AsNoTracking()
					.Where(a => a.TicketId == ticketId)
					.OrderByDescending(a => a.AssignedAt)
					.Select(a => a.ToManagerId)
					.FirstOrDefaultAsync();

				if (lastAssignee == managerId)
					eligible = true;

				// 3) （可選）客服主管/管理員全域檢視權
				if (!eligible && await HasSupervisorPermissionAsync(managerId))
					eligible = true;
			}

			if (!eligible)
				throw new HubException("Manager not authorized for this ticket.");

			await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(ticketId));
			await Clients.Caller.SendAsync("joined", ticketId, new { asManager = true });
		}

		public Task Leave(int ticketId)
			=> Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(ticketId));

		// ======== 私有工具 ========

		private bool VerifySignature(int ticketId, int managerId, long expires, string sigHex)
		{
			var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			if (expires < now) return false;                                   // 已過期
			if (expires - now > SIG_MAX_FUTURE_SKEW_SECONDS) return false;     // 時間窗超過（防重放）

			var secret = _config["Support:JoinSecret"];
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

			try
			{
				return Convert.FromHexString(hex);
			}
			catch
			{
				return null;
			}
		}


		/// <summary>
		/// TODO：依你後台權限系統實作。
		/// 例如查 ManagerRole/Claims 是否包含「Support.Supervisor」之類。
		/// 目前預設 false，代表未指派也無主管權時不可加入任意工單。
		/// </summary>
		private Task<bool> HasSupervisorPermissionAsync(int managerId)
			=> Task.FromResult(false);
	}
}
