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

		// GET: /MessageCenter
		public async Task<IActionResult> Index()
		{
			var notifications = await (
				from n in _context.Notifications
				join s in _context.NotificationSources
					on n.SourceId equals s.SourceId
				join a in _context.NotificationActions
					on n.ActionId equals a.ActionId
				join u in _context.Users
					on n.SenderId equals u.UserId into userGroup
				from u in userGroup.DefaultIfEmpty() // 如果發送者是管理員，可為 null
				join m in _context.ManagerRoles
					on n.SenderManagerId equals m.ManagerId into managerGroup
				from m in managerGroup.DefaultIfEmpty()
				orderby n.CreatedAt descending
				select new NotificationViewModel
				{
					NotificationId = n.NotificationId,
					NotificationTitle = n.NotificationTitle,
					NotificationMessage = n.NotificationMessage,
					SourceName = s.SourceName,
					ActionName = a.ActionName,
					SenderName = u != null ? u.UserName : (m != null ? "暫無使用著" : "系統"),
					CreatedAt = n.CreatedAt,
					IsRead = false // 如果要抓已讀，需要 join NotificationRecipients
				}
			).ToListAsync();

			return View(notifications);
		}


		// GET: /MessageCenter/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: /MessageCenter/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Notification model, int[] recipientUserIds)
		{
			if (ModelState.IsValid)
			{
				model.CreatedAt = DateTime.Now;

				_context.Notifications.Add(model);
				await _context.SaveChangesAsync();

				// 建立收件者
				foreach (var userId in recipientUserIds)
				{
					var recipient = new NotificationRecipient
					{
						NotificationId = model.NotificationId,
						UserId = userId,
						IsRead = false
					};
					_context.NotificationRecipients.Add(recipient);
				}

				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(model);
		}

		// POST: /MessageCenter/MarkAsRead/5
		[HttpPost]
		public async Task<IActionResult> MarkAsRead(int id, int userId)
		{
			var recipient = await _context.NotificationRecipients
				.FirstOrDefaultAsync(r => r.NotificationId == id && r.UserId == userId);

			if (recipient != null && !recipient.IsRead)
			{
				recipient.IsRead = true;
				recipient.ReadAt = DateTime.Now;
				await _context.SaveChangesAsync();
			}

			return Ok();
		}
	}
}

