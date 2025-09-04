using GameSpace.Areas.social_hub.Models.ViewModels;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	public class MessageCenterController : Controller
	{
		private readonly GameSpacedatabaseContext _context;

		public MessageCenterController(GameSpacedatabaseContext context)
		{
			_context = context;
		}

		// =========================
		// 讀取通知列表（測試用）
		// =========================
		public async Task<IActionResult> Index()
		{
			// 暫時手動指定測試用 UserId（一般使用者）
			int userId = 10000012; // 替換成資料庫裡的一般使用者 Id

			var notifications = await _context.NotificationRecipients
				.Where(nr => nr.UserId == userId)
				.Include(nr => nr.Notification)
					.ThenInclude(n => n.Action)
				.Include(nr => nr.Notification)
					.ThenInclude(n => n.Source)
				.Include(nr => nr.Notification)
					.ThenInclude(n => n.Sender)
				.Include(nr => nr.Notification)
					.ThenInclude(n => n.SenderManager)
				.OrderByDescending(nr => nr.Notification.CreatedAt)
				.ToListAsync();

			var model = notifications
				.Where(nr => nr.Notification != null)
				.Select(nr => new NotificationViewModel
				{
					NotificationId = nr.Notification.NotificationId,
					NotificationTitle = nr.Notification.NotificationTitle,
					NotificationMessage = nr.Notification.NotificationMessage,
					CreatedAt = nr.Notification.CreatedAt,
					IsRead = nr.IsRead,
					// 判斷發送者是一般使用者或管理員
					SenderName = nr.Notification.Sender?.UserName
								 ?? nr.Notification.SenderManager?.ManagerName
								 ?? "系統",
					ActionName = nr.Notification.Action?.ActionName ?? "未設定",
					SourceName = nr.Notification.Source?.SourceName ?? "未設定"
				})
				.ToList();

			return View(model);
		}

		// =========================
		// 標記通知為已讀
		// =========================
		[HttpPost]
		public async Task<IActionResult> MarkAsRead(int recipientId)
		{
			var recipient = await _context.NotificationRecipients.FindAsync(recipientId);
			if (recipient == null)
				return NotFound();

			if (!recipient.IsRead)
			{
				recipient.IsRead = true;
				recipient.ReadAt = DateTime.Now;
				await _context.SaveChangesAsync();
			}

			return Ok();
		}

		// =========================
		// 新增通知（測試用）
		// =========================
		[HttpGet]
		public async Task<IActionResult> TestNotification()
		{
			// 指定收件人一般使用者 Id
			int userId = 10000012; // Users 表中存在的一般使用者

			var user = await _context.Users.FindAsync(userId);
			if (user == null)
				return Content($"UserId {userId} 不存在");

			// 建立測試通知（管理員發送）
			var notification = new Notification
			{
				SourceId = 1,
				ActionId = 1,
				SenderId = null,            // 現在可以為 null
				SenderManagerId = 30000012, // 管理員發送
				NotificationTitle = "測試通知",
				NotificationMessage = "這是一則測試通知",
				CreatedAt = DateTime.Now
			};

			notification.NotificationRecipients.Add(new NotificationRecipient
			{
				UserId = 10000012, // 一般使用者
				IsRead = false
			});



			_context.Notifications.Add(notification);
			await _context.SaveChangesAsync();

			return Content("測試通知已新增完成！");
		}
	}
}
