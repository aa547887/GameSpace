using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameSpace.Areas.social_hub.Services;
using GameSpace.Data;
using GameSpace.Filters;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	public class NotificationsController : Controller
	{
		private readonly GameSpacedatabaseContext _context;
		private readonly INotificationService _notificationService;

		public NotificationsController(GameSpacedatabaseContext context, INotificationService notificationService)
		{
			_context = context;
			_notificationService = notificationService;
		}

		// 取得目前 Cookie 中的整數（sh_uid / sh_mid）
		private int? TryGetCookieInt(string key)
		{
			if (!Request.Cookies.TryGetValue(key, out var val)) return null;
			return int.TryParse(val, out var n) ? n : (int?)null;
		}

		private async Task<bool> IsCurrentUserAdminAsync()
		{
			var uid = TryGetCookieInt("sh_uid");
			if (uid is null || uid <= 0) return false;

			// 依你實際的管理員/角色模型調整（以下沿用你原本的查法）
			return await _context.ManagerData
				.AsNoTracking()
				.Where(m => m.ManagerId == uid)
				.SelectMany(m => m.ManagerRoles)
				.AnyAsync(rp => rp.ManagerRoleId == 1 || rp.ManagerRoleId == 2 || rp.ManagerRoleId == 8);
		}

		public async Task<IActionResult> Index()
		{
			ViewBag.IsAdmin = await IsCurrentUserAdminAsync();

			// 簡單列出主檔；若要顯示收件人/已讀，請改查 NotificationRecipients
			var list = await _context.Notifications
				.AsNoTracking()
				.OrderByDescending(n => n.CreatedAt)
				.ToListAsync();

			return View(list);
		}

		[AdminOnly]
		public IActionResult Create() => View();

		/// <summary>
		/// 建立單一通知並指定多位收件人（收件人放在 NotificationRecipients，不在 Notification 本體）
		/// </summary>
		[AdminOnly]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			[Bind("NotificationTitle,NotificationMessage,SourceId,ActionId")] Notification input,
			List<int> recipientIds)
		{
			if (!ModelState.IsValid) return View(input);

			var senderUserId = TryGetCookieInt("sh_uid");
			var senderManagerId = TryGetCookieInt("sh_mid");

			// 用服務統一處理：Sender 欄位判斷、收件人去重與有效性過濾
			var added = await _notificationService.CreateAsync(
				input,
				recipientIds ?? Enumerable.Empty<int>(),
				senderUserId,
				senderManagerId
			);

			TempData["Toast"] = $"✅ 已建立通知 #{input.NotificationId}，成功寄給 {added} 位收件人。";
			return RedirectToAction(nameof(Index));
		}

		/// <summary>
		/// 範例：依管理員角色群發（注意：若 ManagerId 不是 Users.UserId，將被服務層過濾掉）
		/// 你可以依實際資料表關聯，將 Manager 映射到對應的 UserId 再傳入。
		/// </summary>
		[AdminOnly]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> BroadcastToRole(
			int roleId,
			[Bind("NotificationTitle,NotificationMessage,SourceId,ActionId")] Notification template)
		{
			var senderUserId = TryGetCookieInt("sh_uid");
			var senderManagerId = TryGetCookieInt("sh_mid");

			// 目前先取 ManagerId 作為收件清單；如果這些 ID 不在 Users，就會在服務層被過濾掉（不會造成 FK 錯誤）
			var receivers = await _context.ManagerData
				.AsNoTracking()
				.Where(m => m.ManagerRoles.Any(rp => rp.ManagerRoleId == roleId))
				.Select(m => m.ManagerId)
				.ToListAsync();

			var added = await _notificationService.CreateAsync(
				template,
				receivers,
				senderUserId,
				senderManagerId
			);

			TempData["Toast"] = $"📣 群發完成（有效收件人：{added} 位）。";
			return RedirectToAction(nameof(Index));
		}
	}
}
