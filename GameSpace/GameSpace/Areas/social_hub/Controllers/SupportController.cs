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

// EF 實體別名
using Db = GameSpace.Models;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	[CustomerServiceOnly] // 具有 customer_service 才能進入整個區域
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

			// 是否具「雙權限」（customer_service + UserStatusManagement）
			var canAssignToOthers = await _perm.CanUseSupportAssignmentAsync(gate.ManagerId ?? 0);
			ViewBag.CanAssignToOthers = canAssignToOthers;

			return View();
		}

		// ===================== 徽章數 =====================
		[HttpGet]
		public async Task<IActionResult> GetCounts()
		{
			var meId = (await _perm.GetCustomerServiceContextAsync(HttpContext)).ManagerId ?? 0;

			var assignedCount = await _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed != true && t.AssignedManagerId == meId)
				.CountAsync();

			var unassignedCount = await _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed != true && t.AssignedManagerId == null)
				.CountAsync();

			var closedCount = await _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed == true)
				.CountAsync();

			// 未完成（所有未結案）
			var inprogressCount = await _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed != true)
				.CountAsync();

			return Ok(new { assigned = assignedCount, unassigned = unassignedCount, closed = closedCount, inprogress = inprogressCount });
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

		// 原本的投影 (請移除對 LastActivityAt 的指定)
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
				// ❌ 刪掉這行：LastActivityAt = (t.LastMessageAt ?? t.CreatedAt),
				UnreadForMe = db.SupportTicketMessages
					.Where(m => m.TicketId == t.TicketId
							 && m.SenderUserId != null
							 && m.ReadByManagerAt == null)
					.Count()
			});
		}

		// 原本的排序 (改用合併運算)
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

		// ===================== 清單（Partial） =====================
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
			// 是否雙權限（列表上可顯示「指派他人」用）
			var meId = (await _perm.GetCustomerServiceContextAsync(HttpContext)).ManagerId ?? 0;
			ViewBag.CanAssignToOthers = await _perm.CanUseSupportAssignmentAsync(meId);
			return PartialView("_TicketList", paged);
		}

		// ★ 只有雙權限可見／可用：列出所有未結案工單，方便跨人員轉單
		[HttpGet]
		public async Task<IActionResult> ListInProgress(int page = 1, string? q = null, int pageSize = 10)
		{
			var meId = (await _perm.GetCustomerServiceContextAsync(HttpContext)).ManagerId ?? 0;

			if (!await _perm.CanUseSupportAssignmentAsync(meId))
				return Forbid();

			IQueryable<Db.SupportTicket> baseQ = _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed != true);

			baseQ = ApplyKeyword(baseQ, q);

			var ordered = OrderForList(ProjectTicketListVM(baseQ, _db));
			var paged = await ToPagedAsync(ordered, page, pageSize);

			ViewData["action"] = nameof(ListInProgress);
			ViewBag.Query = q ?? string.Empty;
			ViewBag.CanAssignToOthers = true;
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

		// ===================== 詳細資料：指派紀錄（Partial + Page） =====================

		// 左側卡片：只顯示「歷史」—排除目前這筆（最新），最多 9 筆、每頁 3 筆
		[HttpGet]
		public async Task<IActionResult> AssignmentHistory(int id, int page = 1)
		{
			const int pageSize = 3;
			const int maxRecords = 9;

			var baseQ = _db.Set<Db.SupportTicketAssignment>()
				.AsNoTracking()
				.Where(a => a.TicketId == id)
				.OrderByDescending(a => a.AssignedAt).ThenByDescending(a => a.AssignmentId)
				.Skip(1); // 排除目前這筆

			var totalActual = await baseQ.CountAsync();
			var total = Math.Min(totalActual, maxRecords);
			var totalPages = (int)Math.Ceiling(total / (double)pageSize);
			if (totalPages <= 0) totalPages = 1;
			if (page <= 0) page = 1;
			if (page > totalPages) page = totalPages;

			var items = await baseQ
				.Take(maxRecords)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			ViewBag.Page = page;
			ViewBag.TotalPages = totalPages;
			ViewBag.TicketId = id;

			return PartialView("_AssignmentHistory", items);
		}

		// 完整歷史頁
		[HttpGet]
		public async Task<IActionResult> AssignmentHistoryPage(int id)
		{
			var t = await _db.SupportTickets.AsNoTracking()
				.FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			var list = await _db.Set<Db.SupportTicketAssignment>()
				.AsNoTracking()
				.Where(a => a.TicketId == id)
				.OrderByDescending(a => a.AssignedAt).ThenByDescending(a => a.AssignmentId)
				.ToListAsync();

			var vm = new AssignmentHistoryPageVM
			{
				TicketId = t.TicketId,
				UserId = t.UserId,
				Subject = t.Subject ?? string.Empty,
				Items = list
			};

			return View("AssignmentHistoryPage", vm);
		}

		// ===================== 分段輔助（依指派時間切段） =====================
		private async Task<(DateTime? start, DateTime? end)> GetCurrentSegmentBoundsAsync(int ticketId, int? currentAssigneeId)
		{
			if (currentAssigneeId == null) return (null, null);

			var rec = await _db.Set<Db.SupportTicketAssignment>()
				.AsNoTracking()
				.Where(a => a.TicketId == ticketId && a.ToManagerId == currentAssigneeId.Value)
				.OrderBy(a => a.AssignedAt).ThenBy(a => a.AssignmentId)
				.LastOrDefaultAsync();

			if (rec == null) return (null, null);

			var next = await _db.Set<Db.SupportTicketAssignment>()
				.AsNoTracking()
				.Where(a => a.TicketId == ticketId &&
					   (a.AssignedAt > rec.AssignedAt ||
					   (a.AssignedAt == rec.AssignedAt && a.AssignmentId > rec.AssignmentId)))
				.OrderBy(a => a.AssignedAt).ThenBy(a => a.AssignmentId)
				.FirstOrDefaultAsync();

			return (rec.AssignedAt, next?.AssignedAt);
		}

		private async Task<(DateTime? start, DateTime? end, Db.SupportTicketAssignment? rec)>
			GetAssignmentBoundsAsync(int ticketId, int assignmentId)
		{
			var rec = await _db.Set<Db.SupportTicketAssignment>()
				.AsNoTracking()
				.FirstOrDefaultAsync(a => a.TicketId == ticketId && a.AssignmentId == assignmentId);
			if (rec == null) return (null, null, null);

			var next = await _db.Set<Db.SupportTicketAssignment>()
				.AsNoTracking()
				.Where(a => a.TicketId == ticketId &&
					   (a.AssignedAt > rec.AssignedAt ||
					   (a.AssignedAt == rec.AssignedAt && a.AssignmentId > rec.AssignmentId)))
				.OrderBy(a => a.AssignedAt).ThenBy(a => a.AssignmentId)
				.FirstOrDefaultAsync();

			return (rec.AssignedAt, next?.AssignedAt, rec);
		}

		// ===================== 工單詳情/訊息（主頁僅顯示目前指派這段） =====================
		[HttpGet]
		public async Task<IActionResult> Ticket(int id, int page = 1)
		{
			var gate = await _perm.GetCustomerServiceContextAsync(HttpContext);
			var meId = gate.ManagerId ?? 0;

			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			// 未指派：禁止進入對話（必須先指派）
			if (t.AssignedManagerId == null)
				return StatusCode(409, "此工單尚未指派，請先指派後再進入對話。");

			var isClosed = t.IsClosed == true;
			var isAdmin = await _perm.HasAdministratorPrivilegesAsync(meId);
			var canAssignToOthers = await _perm.CanUseSupportAssignmentAsync(meId);

			// 目前指派分段
			var (segStart, segEnd) = await GetCurrentSegmentBoundsAsync(id, t.AssignedManagerId);

			// 只有現任負責人把分段內未讀標記已讀
			if (t.AssignedManagerId == meId && segStart != null)
			{
				var unreadQ = _db.SupportTicketMessages
					.Where(m => m.TicketId == id && m.SenderUserId != null && m.ReadByManagerAt == null)
					.Where(m => m.SentAt >= segStart.Value);
				if (segEnd != null) unreadQ = unreadQ.Where(m => m.SentAt < segEnd.Value);

				var unread = await unreadQ.ToListAsync();
				if (unread.Count > 0)
				{
					var now = DateTime.UtcNow;
					foreach (var m in unread) m.ReadByManagerAt = now;
					await _db.SaveChangesAsync();
				}
			}

			if (page <= 0) page = 1;

			var baseQ = _db.SupportTicketMessages.AsNoTracking().Where(m => m.TicketId == id);
			if (segStart != null) baseQ = baseQ.Where(m => m.SentAt >= segStart.Value);
			if (segEnd != null) baseQ = baseQ.Where(m => m.SentAt < segEnd.Value);
			baseQ = baseQ.OrderByDescending(m => m.SentAt);

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
				// 自己接單：任何客服可用（不需雙權限）
				CanAssignToMe = !isClosed && t.AssignedManagerId == null,
				// 轉單：現任負責人 或 管理員 或 具有雙權限者
				CanReassign = !isClosed && (t.AssignedManagerId == meId || isAdmin || canAssignToOthers),
				// 結單：現任負責人或管理員
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
			var meId = (await _perm.GetCustomerServiceContextAsync(HttpContext)).ManagerId ?? 0;

			if (page <= 0) page = 1;

			var ticket = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (ticket == null) return NotFound();
			if (ticket.AssignedManagerId == null)
				return StatusCode(409, "此工單尚未指派，請先指派後再查看對話。");

			var (segStart, segEnd) = await GetCurrentSegmentBoundsAsync(id, ticket.AssignedManagerId);

			var q = _db.SupportTicketMessages.AsNoTracking().Where(m => m.TicketId == id);
			if (segStart != null) q = q.Where(m => m.SentAt >= segStart.Value);
			if (segEnd != null) q = q.Where(m => m.SentAt < segEnd.Value);
			q = q.OrderByDescending(m => m.SentAt);

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

		// ===================== 指派段落對話頁（依某一筆指派） =====================
		[HttpGet]
		public async Task<IActionResult> AssignmentConversation(int id, int aid, int page = 1)
		{
			const int pageSize = MessagePageSize;

			var (start, end, rec) = await GetAssignmentBoundsAsync(id, aid);
			if (rec == null) return NotFound();

			var t = await _db.SupportTickets.AsNoTracking()
				.FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			var q = _db.SupportTicketMessages.AsNoTracking()
				.Where(m => m.TicketId == id);
			if (start != null) q = q.Where(m => m.SentAt >= start.Value);
			if (end != null) q = q.Where(m => m.SentAt < end.Value);

			q = q.OrderByDescending(m => m.SentAt);

			if (page <= 0) page = 1;
			var total = await q.CountAsync();
			var pageItems = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

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
					IsMine = (m.SenderManagerId != null && m.SenderManagerId == rec.ToManagerId)
				})
				.ToList();

			var vm = new AssignmentConversationVM
			{
				TicketId = id,
				AssignmentId = aid,
				UserId = t.UserId,
				Subject = t.Subject ?? string.Empty,

				FromManagerId = rec.FromManagerId,
				ToManagerId = rec.ToManagerId,
				AssignedByManagerId = rec.AssignedByManagerId,
				AssignedAt = rec.AssignedAt,
				NextAssignedAt = end,
				Note = rec.Note,

				Messages = messagesAsc,
				Page = page,
				PageSize = pageSize,
				TotalMessages = total
			};

			return View(vm);
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

		// ===================== 指派給我（AJAX）：任何客服可自接 =====================
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
			var canAssignToOthers = await _perm.CanUseSupportAssignmentAsync(meId);

			// 允許：現任負責人 或 管理員 或 具有雙權限者
			if (!(t.AssignedManagerId == meId || isAdmin || canAssignToOthers))
				return Forbid();

			// 目標候選：只要具有 customer_service 即可被指派（目標不需雙權限）
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
			var canAssignToOthers = await _perm.CanUseSupportAssignmentAsync(meId);

			// 允許：現任負責人 或 管理員 或 具有雙權限者
			if (!(t.AssignedManagerId == meId || isAdmin || canAssignToOthers))
				return Forbid();

			// 同人轉派：視為無變更
			if (t.AssignedManagerId == toManagerId)
				return RedirectToAction(nameof(Ticket), new { id });

			// 目標客服存在且具「customer_service」
			var exists = await _db.ManagerData.AsNoTracking().AnyAsync(m => m.ManagerId == toManagerId);
			if (!exists)
			{
				TempData["Error"] = "目標客服不存在。";
				return RedirectToAction(nameof(Reassign), new { id });
			}

			var targetHasCS = await _db.Set<Db.ManagerRolePermission>()
				.Where(rp => rp.CustomerService == true)
				.SelectMany(rp => rp.Managers)
				.AnyAsync(m => m.ManagerId == toManagerId);

			if (!targetHasCS)
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
