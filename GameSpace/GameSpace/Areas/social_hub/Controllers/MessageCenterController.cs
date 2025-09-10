using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

using GameSpace.Models; // DbContext 與模型
using GameSpace.Areas.social_hub.Services;
using GameSpace.Areas.social_hub.Models.ViewModels;

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

		// ========= 身分讀取（相容新舊 cookie）=========
		private (int? id, string kind) GetIdentity()
		{
			string? idStr = null;
			string? kind = null;

			if (Request.Cookies.TryGetValue("gs_id", out var gsid)) idStr = gsid;
			if (Request.Cookies.TryGetValue("gs_kind", out var gskind)) kind = gskind;

			if (string.IsNullOrWhiteSpace(idStr))
			{
				if (Request.Cookies.TryGetValue("sh_mid", out var mid))
				{
					idStr = mid;
					kind = "manager";
				}
				else if (Request.Cookies.TryGetValue("sh_uid", out var uid))
				{
					idStr = uid;
					kind = "user";
				}
			}

			if (!int.TryParse(idStr, out var idVal)) return (null, kind ?? "");
			return (idVal, kind ?? "");
		}

		// ========= 通知中心（統一入口）=========
		// 使用者：顯示自己的通知；管理員：顯示建置中；未登入：401
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var (id, kind) = GetIdentity();
			if (id is null || id <= 0) return Unauthorized();

			if (string.Equals(kind, "user", StringComparison.OrdinalIgnoreCase))
			{
				var list = await _context.NotificationRecipients
					.AsNoTracking()
					.Where(nr => nr.UserId == id.Value)
					.OrderByDescending(nr => nr.RecipientId)
					.Select(nr => new NotificationViewModel
					{
						RecipientId = nr.RecipientId,                // ← 這行很重要
						NotificationId = nr.NotificationId,
						NotificationTitle = nr.Notification.NotificationTitle ?? string.Empty,
						NotificationMessage = nr.Notification.NotificationMessage ?? string.Empty,
						SourceName = nr.Notification.Source != null ? nr.Notification.Source.SourceName : null,
						ActionName = nr.Notification.Action != null ? nr.Notification.Action.ActionName : null,
						SenderName = nr.Notification.Sender != null
						? (nr.Notification.Sender.UserName ?? nr.Notification.Sender.UserAccount)
						: (nr.Notification.SenderManager != null
							? nr.Notification.SenderManager.ManagerName
							: "系統"),
						CreatedAt = nr.Notification.CreatedAt,
						IsRead = nr.IsRead
					})
					.Take(200)
					.ToListAsync();

				ViewBag.Mode = "user";
				return View(list);
			}

			ViewBag.Mode = "manager";
			return View(new List<NotificationViewModel>());
		}

		// ========= 標記收件明細為已讀（使用者）=========
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> MarkRead(int id) // id = RecipientId
		{
			var (meId, kind) = GetIdentity();
			if (meId is null || meId <= 0 || !string.Equals(kind, "user", StringComparison.OrdinalIgnoreCase))
				return Unauthorized();

			var rec = await _context.NotificationRecipients.FirstOrDefaultAsync(r => r.RecipientId == id);
			if (rec == null) return NotFound();
			if (rec.UserId != meId.Value) return Forbid();

			if (!rec.IsRead)
			{
				rec.IsRead = true;
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}

		// ========= 建立頁：共用下拉選單填充 =========
		private async Task FillCreateDropdownsAsync(int? sourceId = null, int? actionId = null, int? groupId = null, string? recipientMode = null)
		{
			ViewBag.Sources = new SelectList(
				await _context.NotificationSources.AsNoTracking().OrderBy(s => s.SourceId).ToListAsync(),
				"SourceId", "SourceName", sourceId);

			ViewBag.Actions = new SelectList(
				await _context.NotificationActions.AsNoTracking().OrderBy(a => a.ActionId).ToListAsync(),
				"ActionId", "ActionName", actionId);

			var users = await _context.Users.AsNoTracking()
				.OrderBy(u => u.UserId)
				.Select(u => new
				{
					u.UserId,
					Name = (u.UserName ?? u.UserAccount) ?? ("User #" + u.UserId)
				})
				.ToListAsync();
			ViewBag.Users = new SelectList(users, "UserId", "Name");

			ViewBag.Groups = new SelectList(
				await _context.Groups.AsNoTracking().OrderBy(g => g.GroupId).ToListAsync(),
				"GroupId", "GroupName", groupId);

			ViewBag.RecipientMode = recipientMode ?? "specific";
		}

		// ========= 建立通知（GET）=========
		[HttpGet]
		public async Task<IActionResult> Create()
		{
			await FillCreateDropdownsAsync();
			return View(new Notification());
		}

		// ========= 建立通知（POST）=========
		// 支援 recipientMode：specific（多選）/ all_users（全體使用者）
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			[Bind("NotificationTitle,NotificationMessage,SourceId,ActionId,GroupId")] Notification input,
			[FromForm] List<int> recipientIds,
			[FromForm] string recipientMode)
		{
			if (!ModelState.IsValid)
			{
				await FillCreateDropdownsAsync(input.SourceId, input.ActionId, input.GroupId, recipientMode);
				return View(input);
			}

			// 讀目前登入身分作為 Sender
			var (meId, kind) = GetIdentity();
			int? senderUserId = string.Equals(kind, "user", StringComparison.OrdinalIgnoreCase) ? meId : null;
			int? senderManagerId = string.Equals(kind, "manager", StringComparison.OrdinalIgnoreCase) ? meId : null;

			IEnumerable<int> finalRecipients = Enumerable.Empty<int>();

			recipientMode = (recipientMode ?? "specific").Trim().ToLowerInvariant();
			switch (recipientMode)
			{
				case "specific":
					finalRecipients = recipientIds?.Distinct() ?? Enumerable.Empty<int>();
					if (!finalRecipients.Any())
					{
						ViewBag.AllErrors = "請至少選擇一位收件人，或改用「全體使用者」。";
						await FillCreateDropdownsAsync(input.SourceId, input.ActionId, input.GroupId, recipientMode);
						return View(input);
					}
					break;

				case "all_users":
					finalRecipients = await _context.Users
						.AsNoTracking()
						.Select(u => u.UserId)
						.ToListAsync();
					if (!finalRecipients.Any())
					{
						ViewBag.AllErrors = "目前系統內沒有任何使用者可寄送。";
						await FillCreateDropdownsAsync(input.SourceId, input.ActionId, input.GroupId, recipientMode);
						return View(input);
					}
					break;

				case "all_managers":
				case "both":
				default:
					ViewBag.AllErrors = "管理員收件尚未開通（資料表尚未加入 ManagerId）。目前僅支援：指定使用者 / 全體使用者。";
					await FillCreateDropdownsAsync(input.SourceId, input.ActionId, input.GroupId, recipientMode);
					return View(input);
			}

			var added = await _notificationService.CreateAsync(
				input,
				finalRecipients,
				senderUserId,
				senderManagerId
			);

			TempData["Msg"] = $"✅ 已建立通知 #{input.NotificationId}，成功寄給 {added} 位收件人。";
			return RedirectToAction(nameof(Index));
		}

		// ========= （選用）刪除通知 =========
		[HttpGet]
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
				var recs = _context.NotificationRecipients.Where(r => r.NotificationId == id);
				_context.NotificationRecipients.RemoveRange(recs);
				_context.Notifications.Remove(n);
				await _context.SaveChangesAsync();
				TempData["Msg"] = "已刪除通知與其收件明細。";
			}
			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		public async Task<IActionResult> Detail(int id) // id = RecipientId
		{
			var (meId, kind) = GetIdentity();
			if (meId is null || meId <= 0) return Unauthorized();

			if (string.Equals(kind, "user", StringComparison.OrdinalIgnoreCase))
			{
				// 先投影必要欄位（可由 DB 計算），再在記憶體中補充語意欄位
				var row = await _context.NotificationRecipients
					.AsNoTracking()
					.Where(nr => nr.RecipientId == id && nr.UserId == meId.Value)
					.Select(nr => new
					{
						nr.RecipientId,
						nr.NotificationId,
						nr.IsRead,
						Title = nr.Notification.NotificationTitle,
						Msg = nr.Notification.NotificationMessage,
						SourceName = nr.Notification.Source != null ? nr.Notification.Source.SourceName : null,
						ActionName = nr.Notification.Action != null ? nr.Notification.Action.ActionName : null,
						GroupName = nr.Notification.Group != null ? nr.Notification.Group.GroupName : null,
						SenderUser = nr.Notification.Sender != null ? (nr.Notification.Sender.UserName ?? nr.Notification.Sender.UserAccount) : null,
						SenderManager = nr.Notification.SenderManager != null ? nr.Notification.SenderManager.ManagerName : null,
						nr.Notification.CreatedAt
					})
					.FirstOrDefaultAsync();

				if (row == null) return NotFound();

				var vm = new NotificationDetailViewModel
				{
					RecipientId = row.RecipientId,
					NotificationId = row.NotificationId,
					NotificationTitle = row.Title ?? string.Empty,
					NotificationMessage = (row.Msg ?? string.Empty).Trim(),
					SourceName = row.SourceName,
					ActionName = row.ActionName,
					GroupName = row.GroupName,
					SenderName = row.SenderUser ?? row.SenderManager ?? "系統",
					SenderType = row.SenderUser != null ? "user" : (row.SenderManager != null ? "manager" : "system"),
					CreatedAt = row.CreatedAt,
					IsRead = row.IsRead,
					FullSourcePath = $"{(row.SourceName ?? "未設定")} → {(row.ActionName ?? "未設定")}",
					Links = null
				};

				// 若尚未已讀：標記為已讀；資料表如果沒有 ReadAt，這裡只把 VM 的 ReadAt 顯示為現在時間
				if (!vm.IsRead)
				{
					var rec = await _context.NotificationRecipients
						.FirstOrDefaultAsync(r => r.RecipientId == id && r.UserId == meId.Value);

					if (rec != null && !rec.IsRead)
					{
						rec.IsRead = true;
						// 如果你的模型有 rec.ReadAt，這裡可加上：
						// rec.ReadAt = DateTime.UtcNow;
						await _context.SaveChangesAsync();

						vm.IsRead = true;
						vm.ReadAt = DateTime.UtcNow; // 僅顯示用（若資料庫無此欄位）
						ViewBag.AutoRead = true;
					}
				}

				return View(vm);
			}

			// 管理員：目前顯示占位（待 ManagerId 加入收件設計後再開）
			return View("DetailManager");
		}

	}
}
