using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameSpace.Areas.social_hub.Services;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	public class MessageCenterController : Controller
	{
		private readonly GameSpacedatabaseContext _context;
		private readonly INotificationService _notificationService;

		public MessageCenterController(GameSpacedatabaseContext context, INotificationService notificationService)
		{
			_context = context;
			_notificationService = notificationService;
		}

		// =========================
		// 小工具：從 Cookie 取 int
		// =========================
		private int? TryGetCookieInt(string key)
		{
			if (!Request.Cookies.TryGetValue(key, out var val)) return null;
			return int.TryParse(val, out var n) ? n : (int?)null;
		}

		// =========================
		// Admin 檢視所有通知（簡易總覽）
		// GET: social_hub/MessageCenter
		// =========================
		public async Task<IActionResult> Index()
		{
			// 管理總覽：列出最近的通知（含來源/行為/發送者/收件人數）
			var list = await _context.Notifications
				.AsNoTracking()
				.OrderByDescending(n => n.NotificationId)
				.Select(n => new NotificationAdminListItemVM
				{
					NotificationId = n.NotificationId,
					NotificationTitle = n.NotificationTitle,
					NotificationMessage = n.NotificationMessage,
					// ↓ 依你實際模型名稱調整（例如 Source.SourceName / Action.ActionName）
					SourceName = n.Source != null ? n.Source.SourceName : null,
					ActionName = n.Action != null ? n.Action.ActionName : null,
					SenderName =
						n.Sender != null
							? (n.Sender.UserName ?? n.Sender.UserAccount)       // 如果沒有 UserName/UserAccount，請改為你的欄位
							: (n.SenderManager != null
								? n.SenderManager.ManagerName                   // 若管理員名稱欄位不同，請調整
								: "系統"),
					CreatedAt = n.CreatedAt,                                   // 若是 Nullable，請加 ?? DateTime.UtcNow
					RecipientCount = _context.NotificationRecipients.Count(r => r.NotificationId == n.NotificationId)
				})
				.Take(200)
				.ToListAsync();

			return View(list);
		}

		// =========================
		// 使用者收件匣（依 Cookie 的 sh_uid）
		// GET: social_hub/MessageCenter/Inbox
		// =========================
		public async Task<IActionResult> Inbox()
		{
			var uid = TryGetCookieInt("sh_uid");
			if (uid is null || uid.Value <= 0)
				return Unauthorized(); // 或返回空列表 View，看你需求

			var list = await _context.NotificationRecipients
				.AsNoTracking()
				.Where(nr => nr.UserId == uid.Value)
				.OrderByDescending(nr => nr.RecipientId)
				.Select(nr => new NotificationInboxItemVM
				{
					RecipientId = nr.RecipientId,
					NotificationId = nr.NotificationId,
					NotificationTitle = nr.Notification.NotificationTitle,
					NotificationMessage = nr.Notification.NotificationMessage,
					SourceName = nr.Notification.Source != null ? nr.Notification.Source.SourceName : null,
					ActionName = nr.Notification.Action != null ? nr.Notification.Action.ActionName : null,
					SenderName =
						nr.Notification.Sender != null
							? (nr.Notification.Sender.UserName ?? nr.Notification.Sender.UserAccount)
							: (nr.Notification.SenderManager != null
								? nr.Notification.SenderManager.ManagerName
								: "系統"),
					CreatedAt = nr.Notification.CreatedAt,
					IsRead = nr.IsRead
				})
				.Take(200)
				.ToListAsync();

			return View(list);
		}

		// =========================
		// 將單一收件明細標記為已讀
		// POST: social_hub/MessageCenter/MarkRead/5
		// =========================
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> MarkRead(int id)
		{
			var rec = await _context.NotificationRecipients.FirstOrDefaultAsync(r => r.RecipientId == id);
			if (rec == null) return NotFound();

			if (!rec.IsRead)
			{
				rec.IsRead = true;
				// 若你的模型有 ReadAt 欄位可加上：
				// rec.ReadAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();
			}

			return RedirectToAction(nameof(Inbox));
		}

		// =========================
		// 建立通知（Admin）
		// GET: social_hub/MessageCenter/Create
		// =========================
		public IActionResult Create()
		{
			// TODO: 若需要來源/行為下拉選單，請在 ViewData 填入選項
			return View();
		}

		// =========================
		// 建立通知（Admin）
		// POST: social_hub/MessageCenter/Create
		// =========================
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			[Bind("NotificationTitle,NotificationMessage,SourceId,ActionId")] Notification notification,
			List<int> recipientIds)
		{
			if (!ModelState.IsValid)
			{
				// TODO: 若有下拉需要回填，請在這裡放 ViewData 後回傳
				return View(notification);
			}

			int? senderUserId = TryGetCookieInt("sh_uid");
			int? senderManagerId = TryGetCookieInt("sh_mid");

			// 使用服務處理 FK 決策、收件人去重與驗證
			var added = await _notificationService.CreateAsync(
				notification,
				recipientIds ?? Enumerable.Empty<int>(),
				senderUserId,
				senderManagerId
			);

			TempData["Msg"] = $"✅ 已建立通知 #{notification.NotificationId}，成功寄給 {added} 位收件人。";
			return RedirectToAction(nameof(Index));
		}

		// =========================
		// （選用）刪除通知主檔（若要保留歷史，建議不要提供）
		// =========================
		public async Task<IActionResult> Delete(int id)
		{
			var n = await _context.Notifications.AsNoTracking().FirstOrDefaultAsync(x => x.NotificationId == id);
			if (n == null) return NotFound();
			return View(n);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var n = await _context.Notifications.FirstOrDefaultAsync(x => x.NotificationId == id);
			if (n != null)
			{
				// 先刪收件明細，再刪主檔（避免 FK）
				var recs = _context.NotificationRecipients.Where(r => r.NotificationId == id);
				_context.NotificationRecipients.RemoveRange(recs);
				_context.Notifications.Remove(n);
				await _context.SaveChangesAsync();
				TempData["Msg"] = "已刪除通知與其收件明細。";
			}
			return RedirectToAction(nameof(Index));
		}

		// =========================
		// 內部用 VM（避免相依外部 ViewModel）
		// =========================
		public class NotificationAdminListItemVM
		{
			public int NotificationId { get; set; }
			public string? NotificationTitle { get; set; }
			public string? NotificationMessage { get; set; }
			public string? SourceName { get; set; }
			public string? ActionName { get; set; }
			public string? SenderName { get; set; }
			public DateTime CreatedAt { get; set; }
			public int RecipientCount { get; set; }
		}

		public class NotificationInboxItemVM
		{
			public int RecipientId { get; set; }
			public int NotificationId { get; set; }
			public string? NotificationTitle { get; set; }
			public string? NotificationMessage { get; set; }
			public string? SourceName { get; set; }
			public string? ActionName { get; set; }
			public string? SenderName { get; set; }
			public DateTime CreatedAt { get; set; }
			public bool IsRead { get; set; }
		}
	}
}
