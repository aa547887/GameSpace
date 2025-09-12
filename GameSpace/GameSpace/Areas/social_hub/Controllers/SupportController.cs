// Areas/social_hub/Controllers/SupportController.cs
using GameSpace.Areas.social_hub.Filters;
using GameSpace.Areas.social_hub.Models.ViewModels;
using GameSpace.Areas.social_hub.Services;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

// EF 實體別名（你的 Scaffold 實體在 GameSpace.Models）
using Db = GameSpace.Models;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	[CustomerServiceOnly] // 只有 customer_service=true 的管理員可進
	public class SupportController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IManagerPermissionService _perm;

		private const int MessagePageSize = 30;

		public SupportController(GameSpacedatabaseContext db, IManagerPermissionService perm)
		{
			_db = db;
			_perm = perm;
		}

		// ===================== 首頁骨架 =====================
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var gate = await _perm.GetCustomerServiceContextAsync(HttpContext);
			ViewBag.MeManagerId = gate.ManagerId;
			return View();
		}

		// ===================== 徽章數 =====================
		[HttpGet]
		public async Task<IActionResult> GetCounts()
		{
			var gate = await _perm.GetCustomerServiceContextAsync(HttpContext);
			var meId = gate.ManagerId ?? 0;

			var assignedCount = await _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed != true && t.AssignedManagerId == meId)
				.CountAsync();

			var unassignedCount = await _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed != true && t.AssignedManagerId == null)
				.CountAsync();

			var closedCount = await _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed == true)
				.CountAsync();

			return Ok(new { assigned = assignedCount, unassigned = unassignedCount, closed = closedCount });
		}

		// ===================== 共用查詢/投影/分頁 =====================
		private static IQueryable<Db.SupportTicket> ApplyKeyword(IQueryable<Db.SupportTicket> src, string? q)
		{
			if (string.IsNullOrWhiteSpace(q)) return src;

			q = q.Trim();
			if (int.TryParse(q, out var num))
			{
				return src.Where(t => t.TicketId == num || t.UserId == num);
			}
			else
			{
				return src.Where(t => t.Subject != null && t.Subject.Contains(q));
			}
		}

		private static IQueryable<TicketListItemVM> ProjectTicketListVM(
			IQueryable<Db.SupportTicket> query,
			GameSpacedatabaseContext db)
		{
			return query.Select(t => new TicketListItemVM
			{
				TicketId = t.TicketId,
				UserId = t.UserId,
				Subject = t.Subject ?? string.Empty,
				AssignedManagerId = t.AssignedManagerId,
				IsClosed = t.IsClosed == true,
				CreatedAt = t.CreatedAt,
				LastMessageAt = t.LastMessageAt,
				// 只計「使用者→客服」且尚未被客服讀取
				UnreadForMe = db.SupportTicketMessages
					.Where(m => m.TicketId == t.TicketId
							 && m.SenderUserId != null
							 && m.ReadByManagerAt == null)
					.Count()
			});
		}

		private static IQueryable<TicketListItemVM> OrderForList(IQueryable<TicketListItemVM> q)
			=> q.OrderByDescending(x => x.LastMessageAt ?? x.CreatedAt);

		private static async Task<PagedResult<TicketListItemVM>> ToPagedAsync(
			IQueryable<TicketListItemVM> q, int page, int pageSize)
		{
			if (page <= 0) page = 1;
			if (pageSize <= 0) pageSize = 10;

			var total = await q.CountAsync();
			var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

			return new PagedResult<TicketListItemVM>
			{
				Items = items,
				Page = page,
				PageSize = pageSize,
				TotalCount = total
			};
		}

		// ===================== 三個清單（Partial） =====================

		[HttpGet]
		public async Task<IActionResult> ListAssigned(int page = 1, string? q = null, int pageSize = 10)
		{
			var meId = (await _perm.GetCustomerServiceContextAsync(HttpContext)).ManagerId ?? 0;

			IQueryable<Db.SupportTicket> baseQ = _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed != true && t.AssignedManagerId == meId);

			baseQ = ApplyKeyword(baseQ, q);

			var ordered = OrderForList(ProjectTicketListVM(baseQ, _db));
			var paged = await ToPagedAsync(ordered, page, pageSize);

			ViewData["action"] = nameof(ListAssigned);
			ViewBag.Query = q ?? string.Empty;
			return PartialView("_TicketList", paged);
		}

		[HttpGet]
		public async Task<IActionResult> ListUnassigned(int page = 1, string? q = null, int pageSize = 10)
		{
			IQueryable<Db.SupportTicket> baseQ = _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed != true && t.AssignedManagerId == null);

			baseQ = ApplyKeyword(baseQ, q);

			var ordered = OrderForList(ProjectTicketListVM(baseQ, _db));
			var paged = await ToPagedAsync(ordered, page, pageSize);

			ViewData["action"] = nameof(ListUnassigned);
			ViewBag.Query = q ?? string.Empty;
			return PartialView("_TicketList", paged);
		}

		[HttpGet]
		public async Task<IActionResult> ListClosed(int page = 1, string? q = null, int pageSize = 10)
		{
			IQueryable<Db.SupportTicket> baseQ = _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed == true);

			baseQ = ApplyKeyword(baseQ, q);

			var ordered = OrderForList(ProjectTicketListVM(baseQ, _db));
			var paged = await ToPagedAsync(ordered, page, pageSize);

			ViewData["action"] = nameof(ListClosed);
			ViewBag.Query = q ?? string.Empty;
			return PartialView("_TicketList", paged);
		}

		// ===================== 工單詳情/訊息 =====================

		[HttpGet]
		public async Task<IActionResult> Ticket(int id, int page = 1)
		{
			var gate = await _perm.GetCustomerServiceContextAsync(HttpContext);
			var meId = gate.ManagerId ?? 0;

			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			var isClosed = t.IsClosed == true;
			var isAdmin = await _perm.HasAdministratorPrivilegesAsync(meId);

			// 已讀補記：把「使用者→客服」未讀標記為已讀
			var unread = await _db.SupportTicketMessages
				.Where(m => m.TicketId == id && m.SenderUserId != null && m.ReadByManagerAt == null)
				.ToListAsync();

			if (unread.Count > 0)
			{
				var now = DateTime.UtcNow;
				foreach (var m in unread) m.ReadByManagerAt = now;
				await _db.SaveChangesAsync();
			}

			// 訊息分頁（最新優先取回、頁面升序顯示）
			if (page <= 0) page = 1;

			var baseQ = _db.SupportTicketMessages.AsNoTracking()
				.Where(m => m.TicketId == id)
				.OrderByDescending(m => m.SentAt);

			var total = await baseQ.CountAsync();
			var pageItems = await baseQ.Skip((page - 1) * MessagePageSize).Take(MessagePageSize).ToListAsync();

			var messagesAsc = pageItems
				.OrderBy(m => m.SentAt)
				.Select(m => new SupportMessageVM
				{
					MessageId = m.MessageId,
					TicketId = m.TicketId,
					SenderUserId = m.SenderUserId,
					SenderManagerId = m.SenderManagerId,
					MessageText = m.MessageText ?? "",
					SentAt = m.SentAt,
					ReadByUserAt = m.ReadByUserAt,
					ReadByManagerAt = m.ReadByManagerAt,
					IsMine = (m.SenderManagerId != null && m.SenderManagerId.Value == meId)
				})
				.ToList();

			var vm = new TicketDetailVM
			{
				TicketId = t.TicketId,
				UserId = t.UserId,
				Subject = t.Subject ?? "",
				IsClosed = isClosed,
				CreatedAt = t.CreatedAt,
				ClosedAt = t.ClosedAt,
				CloseNote = t.CloseNote,
				AssignedManagerId = t.AssignedManagerId,
				LastMessageAt = t.LastMessageAt,

				MeManagerId = meId,
				CanAssignToMe = !isClosed && t.AssignedManagerId == null,
				CanReassign = !isClosed && (t.AssignedManagerId == meId || isAdmin),
				CanClose = !isClosed && (t.AssignedManagerId == meId || isAdmin),

				Messages = messagesAsc,
				Page = page,
				PageSize = MessagePageSize,
				TotalMessages = total
			};

			return View(vm);
		}

		[HttpGet]
		public async Task<IActionResult> MessageList(int id, int page = 1)
		{
			var gate = await _perm.GetCustomerServiceContextAsync(HttpContext);
			var meId = gate.ManagerId ?? 0;

			if (page <= 0) page = 1;

			var q = _db.SupportTicketMessages.AsNoTracking()
				.Where(m => m.TicketId == id)
				.OrderByDescending(m => m.SentAt);

			var total = await q.CountAsync();
			var pageItems = await q.Skip((page - 1) * MessagePageSize).Take(MessagePageSize).ToListAsync();

			var messagesAsc = pageItems.OrderBy(m => m.SentAt).Select(m => new SupportMessageVM
			{
				MessageId = m.MessageId,
				TicketId = m.TicketId,
				SenderUserId = m.SenderUserId,
				SenderManagerId = m.SenderManagerId,
				MessageText = m.MessageText ?? "",
				SentAt = m.SentAt,
				ReadByUserAt = m.ReadByUserAt,
				ReadByManagerAt = m.ReadByManagerAt,
				IsMine = (m.SenderManagerId != null && m.SenderManagerId.Value == meId)
			}).ToList();

			ViewBag.Page = page;
			ViewBag.PageSize = MessagePageSize;
			ViewBag.Total = total;

			return PartialView("_MessageList", messagesAsc);
		}

		// ===================== 發送訊息（AJAX） =====================
		[HttpPost]
		public async Task<IActionResult> SendMessage(int id, [FromForm] string text)
		{
			var meId = (await _perm.GetCustomerServiceContextAsync(HttpContext)).ManagerId ?? 0;

			var t = await _db.SupportTickets.FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();
			if (t.IsClosed == true) return StatusCode(409, "此工單已結單，無法再發送訊息。");
			if (t.AssignedManagerId != meId) return Forbid();

			text = (text ?? "").Trim();
			if (text.Length == 0) return BadRequest("訊息不可為空。");
			if (text.Length > 255) text = text.Substring(0, 255);

			var now = DateTime.UtcNow;

			_db.SupportTicketMessages.Add(new Db.SupportTicketMessage
			{
				TicketId = id,
				SenderUserId = null,
				SenderManagerId = meId,
				MessageText = text,
				SentAt = now,
				ReadByUserAt = null,
				ReadByManagerAt = now
			});

			t.LastMessageAt = now;
			await _db.SaveChangesAsync();

			return Ok(new { ok = true, sentAt = now, text });
		}

		// ===================== 指派給我（AJAX） =====================
		[HttpPost]
		public async Task<IActionResult> AssignToMe(int id)
		{
			var meId = (await _perm.GetCustomerServiceContextAsync(HttpContext)).ManagerId ?? 0;

			var t = await _db.SupportTickets.FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();
			if (t.IsClosed == true) return StatusCode(409, "此工單已結單，無法指派。");

			if (t.AssignedManagerId == meId)
				return Ok(new { ok = true, assignedManagerId = meId });

			if (t.AssignedManagerId != null)
				return StatusCode(409, "此工單已指派給其他客服。");

			t.AssignedManagerId = meId;

			_db.Set<Db.SupportTicketAssignment>().Add(new Db.SupportTicketAssignment
			{
				TicketId = id,
				FromManagerId = null,
				ToManagerId = meId,
				AssignedByManagerId = meId,
				AssignedAt = DateTime.UtcNow,
				Note = null
			});

			await _db.SaveChangesAsync();
			return Ok(new { ok = true, assignedManagerId = meId });
		}

		// ===================== 結單（頁面） =====================
		[HttpGet]
		public async Task<IActionResult> CloseForm(int id)
		{
			var meId = (await _perm.GetCustomerServiceContextAsync(HttpContext)).ManagerId ?? 0;

			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			var isAdmin = await _perm.HasAdministratorPrivilegesAsync(meId);
			if (t.IsClosed == true || (t.AssignedManagerId != meId && !isAdmin))
				return Forbid();

			var vm = new CloseTicketVM
			{
				TicketId = t.TicketId,
				UserId = t.UserId,
				Subject = t.Subject ?? "",
				AssignedManagerId = t.AssignedManagerId,
				IsClosed = t.IsClosed == true,
				CreatedAt = t.CreatedAt,
				LastMessageAt = t.LastMessageAt
			};
			return View("Close", vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CloseConfirm(int id, [FromForm] string? closeNote)
		{
			var meId = (await _perm.GetCustomerServiceContextAsync(HttpContext)).ManagerId ?? 0;

			var t = await _db.SupportTickets.FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			var isAdmin = await _perm.HasAdministratorPrivilegesAsync(meId);
			if (t.IsClosed == true || (t.AssignedManagerId != meId && !isAdmin))
				return Forbid();

			t.IsClosed = true;
			t.ClosedAt = DateTime.UtcNow;
			t.ClosedByManagerId = meId;
			if (!string.IsNullOrWhiteSpace(closeNote))
			{
				var note = closeNote.Trim();
				t.CloseNote = note.Length > 255 ? note.Substring(0, 255) : note;
			}

			await _db.SaveChangesAsync();
			return RedirectToAction(nameof(Ticket), new { id });
		}

		// ===================== 轉單（頁面） =====================
		[HttpGet]
		public async Task<IActionResult> Reassign(int id)
		{
			var meId = (await _perm.GetCustomerServiceContextAsync(HttpContext)).ManagerId ?? 0;

			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();
			if (t.IsClosed == true) return Forbid();

			var isAdmin = await _perm.HasAdministratorPrivilegesAsync(meId);
			if (t.AssignedManagerId != meId && !isAdmin)
				return Forbid();

			var candidates = await _db.Set<Db.ManagerRolePermission>()
				.Where(rp => rp.CustomerService == true)
				.SelectMany(rp => rp.Managers.Select(m => m.ManagerId))
				.Distinct()
				.OrderBy(m => m)
				.ToListAsync();

			var vm = new ReassignTicketVM
			{
				TicketId = t.TicketId,
				UserId = t.UserId,
				Subject = t.Subject ?? "",
				CurrentAssignedManagerId = t.AssignedManagerId,
				CandidateManagerIds = candidates
			};
			return View("Reassign", vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ReassignConfirm(int id, [FromForm] int toManagerId, [FromForm] string? note)
		{
			var meId = (await _perm.GetCustomerServiceContextAsync(HttpContext)).ManagerId ?? 0;

			var t = await _db.SupportTickets.FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();
			if (t.IsClosed == true) return Forbid();

			var isAdmin = await _perm.HasAdministratorPrivilegesAsync(meId);
			if (t.AssignedManagerId != meId && !isAdmin)
				return Forbid();

			// 同人轉派：視為無變更
			if (t.AssignedManagerId == toManagerId)
				return RedirectToAction(nameof(Ticket), new { id });

			// 目標客服存在且具客服權限
			var exists = await _db.ManagerData.AsNoTracking().AnyAsync(m => m.ManagerId == toManagerId);
			if (!exists)
			{
				TempData["Error"] = "目標客服不存在。";
				return RedirectToAction(nameof(Reassign), new { id });
			}

			var targetIsCS = await _db.Set<Db.ManagerRolePermission>()
				.Where(rp => rp.CustomerService == true)
				.SelectMany(rp => rp.Managers)
				.AnyAsync(m => m.ManagerId == toManagerId);

			if (!targetIsCS)
			{
				TempData["Error"] = "目標客服不具客服權限。";
				return RedirectToAction(nameof(Reassign), new { id });
			}

			var fromId = t.AssignedManagerId;
			t.AssignedManagerId = toManagerId;

			_db.Set<Db.SupportTicketAssignment>().Add(new Db.SupportTicketAssignment
			{
				TicketId = id,
				FromManagerId = fromId,
				ToManagerId = toManagerId,
				AssignedByManagerId = meId,
				AssignedAt = DateTime.UtcNow,
				Note = string.IsNullOrWhiteSpace(note) ? null : note!.Trim().Substring(0, Math.Min(255, note!.Trim().Length))
			});

			await _db.SaveChangesAsync();
			return RedirectToAction(nameof(Ticket), new { id });
		}
	}
}
