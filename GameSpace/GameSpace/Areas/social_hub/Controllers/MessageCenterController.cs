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
		// 例如 MessageCenterController.Index()
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			// 由 cookie 取得目前身分
			var (meId, kind) = GetIdentity();
			if (meId is null || meId <= 0)
				return Unauthorized();

			int? userId = string.Equals(kind, "user", StringComparison.OrdinalIgnoreCase) ? meId : null;
			int? managerId = string.Equals(kind, "manager", StringComparison.OrdinalIgnoreCase) ? meId : null;

			var q =
	from r in _context.NotificationRecipients
	join n in _context.Notifications on r.NotificationId equals n.NotificationId
	join s in _context.NotificationSources on n.SourceId equals s.SourceId
	join a in _context.NotificationActions on n.ActionId equals a.ActionId
	// 左連接寄件者（可能為 null）
	join su0 in _context.Users on n.SenderUserId equals su0.UserId into suj
	from su in suj.DefaultIfEmpty()
	join sm0 in _context.ManagerData on n.SenderManagerId equals sm0.ManagerId into smj
	from sm in smj.DefaultIfEmpty()
	where (userId != null && r.UserId == userId)
	   || (managerId != null && r.ManagerId == managerId)
	orderby n.CreatedAt descending
	select new NotificationViewModel
	{
		RecipientId = r.RecipientId,              // ← 新增
		NotificationId = n.NotificationId,
		NotificationTitle = n.Title,
		NotificationMessage = n.Message,
		SourceName = s.SourceName,
		ActionName = a.ActionName,
		// 若為系統訊息保持 null，前端已有 (item.SenderName ?? "系統")
		SenderName = su != null ? (su.UserName ?? su.UserAccount)
								: (sm != null ? sm.ManagerName : null),
		CreatedAt = n.CreatedAt,
		ReadAt = r.ReadAt
	};

			var list = await q.AsNoTracking().ToListAsync();
			return View(list);
		}



		// ========= 標記收件明細為已讀（使用者）=========
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> MarkRead(int id) // id = RecipientId
		{
			var (meId, kind) = GetIdentity();
			if (meId is null || meId <= 0) return Unauthorized();

			var rec = await _context.NotificationRecipients
				.FirstOrDefaultAsync(r => r.RecipientId == id);

			if (rec == null) return NotFound();

			// 僅允許本人標記
			bool isOwner =
				(string.Equals(kind, "user", StringComparison.OrdinalIgnoreCase) && rec.UserId == meId) ||
				(string.Equals(kind, "manager", StringComparison.OrdinalIgnoreCase) && rec.ManagerId == meId);

			if (!isOwner) return Forbid();

			if (rec.ReadAt == null)
			{
				rec.ReadAt = DateTime.UtcNow;   // ← 以時間戳代表已讀
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
	[Bind("Title,Message,SourceId,ActionId,GroupId")] Notification input,  // ← 修正 Bind 欄位名
	[FromForm] List<int> recipientIds,
	[FromForm] string recipientMode)
		{
			if (!ModelState.IsValid)
			{
				await FillCreateDropdownsAsync(input.SourceId, input.ActionId, input.GroupId, recipientMode);
				return View(input);
			}

			var (meId, kind) = GetIdentity();
			int? senderUserId = string.Equals(kind, "user", StringComparison.OrdinalIgnoreCase) ? meId : (int?)null;
			int? senderManagerId = string.Equals(kind, "manager", StringComparison.OrdinalIgnoreCase) ? meId : (int?)null;

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
					ViewBag.AllErrors = "管理員收件尚未開通。請改用：指定使用者 / 全體使用者。";
					await FillCreateDropdownsAsync(input.SourceId, input.ActionId, input.GroupId, recipientMode);
					return View(input);
			}

			// 你的 INotificationService 既有簽章（userIds, senderUserId, senderManagerId）
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

			bool isUser = string.Equals(kind, "user", StringComparison.OrdinalIgnoreCase);
			bool isManager = string.Equals(kind, "manager", StringComparison.OrdinalIgnoreCase);

			var row = await (
				from nr in _context.NotificationRecipients
				join n in _context.Notifications on nr.NotificationId equals n.NotificationId
				join s in _context.NotificationSources on n.SourceId equals s.SourceId into sj
				from s in sj.DefaultIfEmpty()
				join a in _context.NotificationActions on n.ActionId equals a.ActionId into aj
				from a in aj.DefaultIfEmpty()
				join g in _context.Groups on n.GroupId equals g.GroupId into gj
				from g in gj.DefaultIfEmpty()
				join su in _context.Users on n.SenderUserId equals su.UserId into suj
				from su in suj.DefaultIfEmpty()
				join sm in _context.ManagerData on n.SenderManagerId equals sm.ManagerId into smj
				from sm in smj.DefaultIfEmpty()
				where nr.RecipientId == id
				   && ((isUser && nr.UserId == meId) || (isManager && nr.ManagerId == meId))
				select new
				{
					nr.RecipientId,
					nr.NotificationId,
					IsRead = (nr.ReadAt != null),     // ← 以 ReadAt 推導
					ReadAt = nr.ReadAt,
					Title = n.Title,                  // ← Title
					Msg = n.Message,                  // ← Message
					SourceName = s != null ? s.SourceName : null,
					ActionName = a != null ? a.ActionName : null,
					GroupName = g != null ? g.GroupName : null,
					SenderUserName = su != null ? (su.UserName ?? su.UserAccount) : null,
					SenderManagerName = sm != null ? sm.ManagerName : null,
					n.CreatedAt
				}
			).AsNoTracking().FirstOrDefaultAsync();

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
				SenderName = row.SenderUserName ?? row.SenderManagerName ?? "系統",
				SenderType = row.SenderUserName != null ? "user" : (row.SenderManagerName != null ? "manager" : "system"),
				CreatedAt = row.CreatedAt,
				IsRead = row.IsRead,
				ReadAt = row.ReadAt,
				FullSourcePath = $"{(row.SourceName ?? "未設定")} → {(row.ActionName ?? "未設定")}",
				Links = null
			};

			// 若尚未已讀：現在標記為已讀（寫回 ReadAt）
			if (!vm.IsRead)
			{
				var rec = await _context.NotificationRecipients
					.FirstOrDefaultAsync(r => r.RecipientId == id);

				bool isOwner =
					(isUser && rec?.UserId == meId) ||
					(isManager && rec?.ManagerId == meId);

				if (rec != null && isOwner && rec.ReadAt == null)
				{
					rec.ReadAt = DateTime.UtcNow;
					await _context.SaveChangesAsync();

					vm.IsRead = true;
					vm.ReadAt = rec.ReadAt;
					ViewBag.AutoRead = true;
				}
			}

			return View(vm);
		}

	}
}
