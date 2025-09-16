// 路徑：Areas/social_hub/Controllers/SupportController.cs
using GameSpace.Areas.social_hub.Filters;
using GameSpace.Areas.social_hub.Models.ViewModels;
using GameSpace.Areas.social_hub.Services;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims; // ★ for Claims

// EF 實體別名
using Db = GameSpace.Models;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	// 1) 先確保「外部登入且為管理員」
	//    RequireManagerPermissions 會驗證 AdminCookie/Claims，未通過時頁面 403、AJAX 401
	[RequireManagerPermissions(RequireManager = true, CustomerService = true)]
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

		// =====================================================
		// ★★ 穩健的身分解析：Items → Claims → Cookies（gs_* 相容 sh_*）
		// =====================================================
		private int? ResolveManagerId()
		{
			// 1) HttpContext.Items（若有 SocialHubAuth/Filter 先寫入）
			if (HttpContext?.Items?["gs_kind"]?.ToString()?.Equals("manager", StringComparison.OrdinalIgnoreCase) == true)
			{
				if (HttpContext.Items.TryGetValue("gs_id", out var idObj) && idObj != null)
				{
					if (int.TryParse(idObj.ToString(), out var id) && id > 0) return id;
				}
			}
			// 有些舊碼可能直接放 sh_mid
			if (HttpContext?.Items?.TryGetValue("sh_mid", out var midObj) == true && midObj != null)
			{
				if (int.TryParse(midObj.ToString(), out var mid) && mid > 0) return mid;
			}

			// 2) Claims（AdminCookie 常見配置：mid / manager_id / NameIdentifier）
			var c = User?.FindFirst("mid")
				?? User?.FindFirst("manager_id")
				?? User?.FindFirst(ClaimTypes.NameIdentifier);
			if (c != null && int.TryParse(c.Value, out var cid) && cid > 0) return cid;

			// 3) Cookies（gs_* 為新的最小登入、相容 sh_*）
			string? getCookie(string key)
				=> Request?.Cookies != null && Request.Cookies.TryGetValue(key, out var v) ? v : null;

			var kind = (getCookie("gs_kind") ?? getCookie("sh_kind"))?.Trim().ToLowerInvariant();
			var idStr = getCookie("gs_id") ?? getCookie("sh_mid") ?? getCookie("sh_uid");
			if ((kind == "manager" || getCookie("sh_mid") != null) && int.TryParse(idStr, out var idc) && idc > 0)
				return idc;

			return null;
		}

		// =====================================================
		// [GET] /social_hub/Support
		// 作用：客服工作台首頁骨架（Index.cshtml）
		// 重點：塞 ViewBag.MeManagerId 與 ViewBag.CanAssignToOthers
		// =====================================================
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			// 仍保留服務的權限判斷（含 CustomerService=true）
			var gate = await _perm.GetCustomerServiceContextAsync(HttpContext);

			// ★ 用穩健解析器補齊 ManagerId（避免 gate.ManagerId 為 null）
			var meId = ResolveManagerId();
			ViewBag.MeManagerId = meId; // 注意：View 用的是 `as int?`，給 int? 最保險

			// 是否具「雙權限」
			ViewBag.CanAssignToOthers = meId.HasValue && await _perm.CanUseSupportAssignmentAsync(meId.Value);

			return View();
		}

		// =====================================================
		// [GET] /social_hub/Support/GetCounts
		// 作用：回傳徽章數（JSON）
		// - assigned    ：未結單 & 指派給我
		// - unassigned  ：未結單 & 尚未指派
		// - inprogress  ：未結單 &（未指派 或 指派給別人）→ 排除「指派給我」
		// - closed      ：已結單
		// =====================================================
		[HttpGet]
		public async Task<IActionResult> GetCounts()
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var q = _db.SupportTickets.AsNoTracking();

			var assignedCount = await q.Where(t => t.IsClosed != true && t.AssignedManagerId == meId).CountAsync();
			var unassignedCount = await q.Where(t => t.IsClosed != true && t.AssignedManagerId == null).CountAsync();
			var closedCount = await q.Where(t => t.IsClosed == true).CountAsync();

			// ★ 修正：排除「指派給我」
			var inprogressCount = await q.Where(t => t.IsClosed != true && (t.AssignedManagerId == null || t.AssignedManagerId != meId)).CountAsync();

			return Ok(new { assigned = assignedCount, unassigned = unassignedCount, closed = closedCount, inprogress = inprogressCount });
		}

		// ===================== 共用查詢/投影/分頁 =====================
		private static IQueryable<Db.SupportTicket> ApplyKeyword(IQueryable<Db.SupportTicket> src, string? q)
		{
			if (string.IsNullOrWhiteSpace(q)) return src;
			q = q.Trim();
			if (int.TryParse(q, out var num)) return src.Where(t => t.TicketId == num || t.UserId == num);
			return src.Where(t => t.Subject != null && t.Subject.Contains(q));
		}

		// 投影：以 LastMessageAt / CreatedAt 作為顯示與排序依據；未讀數以管理員視角計
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
				UnreadForMe = db.SupportTicketMessages
					.Where(m => m.TicketId == t.TicketId && m.SenderUserId != null && m.ReadByManagerAt == null)
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

		// =====================================================
		// [GET] /social_hub/Support/ListAssigned?page=1&q=關鍵字
		// 作用：部分檢視（指派給我）
		// 視圖：_TicketList.cshtml（Model: PagedResult<TicketListItemVM>）
		// =====================================================
		[HttpGet]
		public async Task<IActionResult> ListAssigned(int page = 1, string? q = null, int pageSize = 10)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var baseQ = _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed != true && t.AssignedManagerId == meId);

			baseQ = ApplyKeyword(baseQ, q);

			var ordered = OrderForList(ProjectTicketListVM(baseQ, _db));
			var paged = await ToPagedAsync(ordered, page, pageSize);

			// ListAssigned
			ViewData["action"] = nameof(ListAssigned);
			// 固定鍵值
			ViewBag.Query = q ?? string.Empty;
			ViewBag.CanAssignToOthers = await _perm.CanUseSupportAssignmentAsync(meId.Value);
			// ListAssigned / ListUnassigned / ListInProgress / ListClosed 裡面，在 return PartialView 之前：
			ViewBag.MeManagerId = meId.Value;   // ← 讓 Partial 能判斷 "isMine"

			return PartialView("_TicketList", paged);
		}

		// =====================================================
		// [GET] /social_hub/Support/ListUnassigned?page=1&q=關鍵字
		// 作用：部分檢視（未指派）
		// 視圖：_TicketList.cshtml
		// =====================================================
		[HttpGet]
		public async Task<IActionResult> ListUnassigned(int page = 1, string? q = null, int pageSize = 10)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var baseQ = _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed != true && t.AssignedManagerId == null);

			baseQ = ApplyKeyword(baseQ, q);

			var ordered = OrderForList(ProjectTicketListVM(baseQ, _db));
			var paged = await ToPagedAsync(ordered, page, pageSize);

			// ListUnassigned
			ViewData["action"] = nameof(ListUnassigned);

			ViewBag.Query = q ?? string.Empty;
			ViewBag.CanAssignToOthers = await _perm.CanUseSupportAssignmentAsync(meId.Value);
			// ListAssigned / ListUnassigned / ListInProgress / ListClosed 裡面，在 return PartialView 之前：
			ViewBag.MeManagerId = meId.Value;   // ← 讓 Partial 能判斷 "isMine"

			return PartialView("_TicketList", paged);
		}

		// =====================================================
		// [GET] /social_hub/Support/ListInProgress?page=1&q=關鍵字
		// 作用：部分檢視（未完成 = 未結單且不是「指派給我」）
		// 視圖：_TicketList.cshtml（僅雙權限頁面會顯示此分頁）
		// =====================================================
		[HttpGet]
		public async Task<IActionResult> ListInProgress(int page = 1, string? q = null, int pageSize = 10)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();
			if (!await _perm.CanUseSupportAssignmentAsync(meId.Value)) return Forbid();

			var baseQ = _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed != true && (t.AssignedManagerId == null || t.AssignedManagerId != meId));

			baseQ = ApplyKeyword(baseQ, q);

			var ordered = OrderForList(ProjectTicketListVM(baseQ, _db));
			var paged = await ToPagedAsync(ordered, page, pageSize);

			// ListInProgress
			ViewData["action"] = nameof(ListInProgress);
			ViewBag.Query = q ?? string.Empty;
			ViewBag.CanAssignToOthers = true;
			// ListAssigned / ListUnassigned / ListInProgress / ListClosed 裡面，在 return PartialView 之前：
			ViewBag.MeManagerId = meId.Value;   // ← 讓 Partial 能判斷 "isMine"

			return PartialView("_TicketList", paged);
		}

		// =====================================================
		// [GET] /social_hub/Support/ListClosed?page=1&q=關鍵字
		// 作用：部分檢視（已結單）
		// 視圖：_TicketList.cshtml
		// =====================================================
		[HttpGet]
		public async Task<IActionResult> ListClosed(int page = 1, string? q = null, int pageSize = 10)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var baseQ = _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed == true);

			baseQ = ApplyKeyword(baseQ, q);

			var ordered = OrderForList(ProjectTicketListVM(baseQ, _db));
			var paged = await ToPagedAsync(ordered, page, pageSize);


			// ListClosed
			ViewData["action"] = nameof(ListClosed);
			ViewBag.Query = q ?? string.Empty;
			ViewBag.CanAssignToOthers = await _perm.CanUseSupportAssignmentAsync(meId.Value);
			// ListAssigned / ListUnassigned / ListInProgress / ListClosed 裡面，在 return PartialView 之前：
			ViewBag.MeManagerId = meId.Value;   // ← 讓 Partial 能判斷 "isMine"

			return PartialView("_TicketList", paged);
		}

		// ===================== 詳細資料：指派紀錄（Partial + Page） =====================

		// [GET] /social_hub/Support/AssignmentHistory?id={ticketId}&page=1
		[HttpGet]
		public async Task<IActionResult> AssignmentHistory(int id, int page = 1)
		{
			const int pageSize = 3;
			const int maxRecords = 9;

			var baseQ = _db.Set<Db.SupportTicketAssignment>()
				.AsNoTracking()
				.Where(a => a.TicketId == id)
				.OrderByDescending(a => a.AssignedAt).ThenByDescending(a => a.AssignmentId)
				.Skip(1); // 排除最新（目前這筆）

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

		// [GET] /social_hub/Support/AssignmentHistoryPage?id={ticketId}
		[HttpGet]
		public async Task<IActionResult> AssignmentHistoryPage(int id)
		{
			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
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

		private async Task<(DateTime? start, DateTime? end, Db.SupportTicketAssignment? rec)> GetAssignmentBoundsAsync(int ticketId, int assignmentId)
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

		// [GET] /social_hub/Support/Ticket?id={ticketId}&page=1
		[HttpGet]
		public async Task<IActionResult> Ticket(int id, int page = 1)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			// 未指派：安全導向
			if (t.AssignedManagerId == null)
			{
				if (await _perm.CanUseSupportAssignmentAsync(meId.Value))
				{
					// 主管/雙權限 → 直接帶去轉單頁
					return RedirectToAction(nameof(Reassign), new { id, returnUrl = Url.Action(nameof(Index), new { area = "social_hub", tab = "unassigned" }) });
				}

				// 一般客服 → 回到首頁的「未指派」分頁，並提示
				TempData["Warn"] = "此工單尚未指派，請先指派後再進入對話。";
				return RedirectToAction(nameof(Index), new { area = "social_hub", tab = "unassigned" });
			}


			var isClosed = t.IsClosed == true;
			var isAdmin = await _perm.HasAdministratorPrivilegesAsync(meId.Value);
			var canAssignToOthers = await _perm.CanUseSupportAssignmentAsync(meId.Value);

			// 目前指派分段
			var (segStart, segEnd) = await GetCurrentSegmentBoundsAsync(id, t.AssignedManagerId);

			// 只有現任負責人把分段內未讀標記已讀
			if (t.AssignedManagerId == meId.Value && segStart != null)
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
					IsMine = (m.SenderManagerId != null && m.SenderManagerId.Value == meId.Value)
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

				MeManagerId = meId.Value,
				// 自己接單：任何客服可用（不需雙權限）
				CanAssignToMe = !isClosed && t.AssignedManagerId == null,
				// 轉單：現任負責人 或 管理員 或 具有雙權限者
				CanReassign = !isClosed && (t.AssignedManagerId == meId.Value || isAdmin || canAssignToOthers),
				// 結單：現任負責人或管理員
				CanClose = !isClosed && (t.AssignedManagerId == meId.Value || isAdmin),

				Messages = messagesAsc,
				Page = page,
				PageSize = MessagePageSize,
				TotalMessages = total
			};

			return View(vm);
		}

		// [GET] /social_hub/Support/MessageList?id={ticketId}&page=1
		[HttpGet]
		public async Task<IActionResult> MessageList(int id, int page = 1)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

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
				IsMine = (m.SenderManagerId != null && m.SenderManagerId.Value == meId.Value)
			}).ToList();

			ViewBag.Page = page;
			ViewBag.PageSize = MessagePageSize;
			ViewBag.Total = total;

			return PartialView("_MessageList", messagesAsc);
		}

		// ===================== 指派段落對話頁（依某一筆指派） =====================

		// [GET] /social_hub/Support/AssignmentConversation?id={ticketId}&aid={assignmentId}&page=1
		[HttpGet]
		public async Task<IActionResult> AssignmentConversation(int id, int aid, int page = 1)
		{
			const int pageSize = MessagePageSize;

			var (start, end, rec) = await GetAssignmentBoundsAsync(id, aid);
			if (rec == null) return NotFound();

			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			var q = _db.SupportTicketMessages.AsNoTracking().Where(m => m.TicketId == id);
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

		// [POST] /social_hub/Support/SendMessage?id={ticketId}
		[HttpPost]
		public async Task<IActionResult> SendMessage(int id, [FromForm] string text)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var t = await _db.SupportTickets.FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();
			if (t.IsClosed == true) return StatusCode(409, "此工單已結單，無法再發送訊息。");
			if (t.AssignedManagerId != meId.Value) return Forbid();

			text = (text ?? "").Trim();
			if (text.Length == 0) return BadRequest("訊息不可為空。");
			if (text.Length > 255) text = text.Substring(0, 255);

			var now = DateTime.UtcNow;

			_db.SupportTicketMessages.Add(new Db.SupportTicketMessage
			{
				TicketId = id,
				SenderUserId = null,
				SenderManagerId = meId.Value,
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

		// [POST] /social_hub/Support/AssignToMe?id={ticketId}
		[HttpPost]
		public async Task<IActionResult> AssignToMe(int id)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var t = await _db.SupportTickets.FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();
			if (t.IsClosed == true) return StatusCode(409, "此工單已結單，無法指派。");

			if (t.AssignedManagerId == meId.Value)
				return Ok(new { ok = true, assignedManagerId = meId.Value });

			if (t.AssignedManagerId != null)
				return StatusCode(409, "此工單已指派給其他客服。");

			t.AssignedManagerId = meId.Value;

			_db.Set<Db.SupportTicketAssignment>().Add(new Db.SupportTicketAssignment
			{
				TicketId = id,
				FromManagerId = null,
				ToManagerId = meId.Value,
				AssignedByManagerId = meId.Value,
				AssignedAt = DateTime.UtcNow,
				Note = null
			});

			await _db.SaveChangesAsync();
			return Ok(new { ok = true, assignedManagerId = meId.Value });
		}

		// ===================== 結單（頁面） =====================

		// [GET] /social_hub/Support/CloseForm?id={ticketId}
		[HttpGet]
		public async Task<IActionResult> CloseForm(int id)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			var isAdmin = await _perm.HasAdministratorPrivilegesAsync(meId.Value);
			if (t.IsClosed == true || (t.AssignedManagerId != meId.Value && !isAdmin))
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

		// [POST] /social_hub/Support/CloseConfirm?id={ticketId}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CloseConfirm(int id, [FromForm] string? closeNote)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var t = await _db.SupportTickets.FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			var isAdmin = await _perm.HasAdministratorPrivilegesAsync(meId.Value);
			if (t.IsClosed == true || (t.AssignedManagerId != meId.Value && !isAdmin))
				return Forbid();

			t.IsClosed = true;
			t.ClosedAt = DateTime.UtcNow;
			t.ClosedByManagerId = meId.Value;
			if (!string.IsNullOrWhiteSpace(closeNote))
			{
				var note = closeNote.Trim();
				t.CloseNote = note.Length > 255 ? note.Substring(0, 255) : note;
			}

			await _db.SaveChangesAsync();
			return RedirectToAction(nameof(Ticket), new { id });
		}

		// ===================== 轉單（頁面） =====================

		// [GET] /social_hub/Support/Reassign?id={ticketId}
		[HttpGet]
		public async Task<IActionResult> Reassign(int id, string? returnUrl = null)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();
			if (t.IsClosed == true) return Forbid();

			var isAdmin = await _perm.HasAdministratorPrivilegesAsync(meId.Value);
			var canAssignToOthers = await _perm.CanUseSupportAssignmentAsync(meId.Value);
			if (!(t.AssignedManagerId == meId.Value || isAdmin || canAssignToOthers)) return Forbid();

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

			// ★ 安全保留回跳網址（僅接受本機路徑）
			ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl)
				? returnUrl
				: Url.Action(nameof(Index), new { area = "social_hub", tab = "unassigned" });

			return View("Reassign", vm);
		}
		// [POST] /social_hub/Support/ReassignConfirm?id={ticketId}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ReassignConfirm(int id, [FromForm] int toManagerId, [FromForm] string? note, string? returnUrl = null)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var t = await _db.SupportTickets.FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();
			if (t.IsClosed == true) return Forbid();

			var isAdmin = await _perm.HasAdministratorPrivilegesAsync(meId.Value);
			var canAssignToOthers = await _perm.CanUseSupportAssignmentAsync(meId.Value);
			if (!(t.AssignedManagerId == meId.Value || isAdmin || canAssignToOthers)) return Forbid();

			if (t.AssignedManagerId == toManagerId)
				return Url.IsLocalUrl(returnUrl)
					? Redirect(returnUrl!)
					: RedirectToAction(nameof(Ticket), new { id });

			var exists = await _db.ManagerData.AsNoTracking().AnyAsync(m => m.ManagerId == toManagerId);
			if (!exists)
			{
				TempData["Error"] = "目標客服不存在。";
				return RedirectToAction(nameof(Reassign), new { id, returnUrl });
			}

			var targetHasCS = await _db.Set<Db.ManagerRolePermission>()
				.Where(rp => rp.CustomerService == true)
				.SelectMany(rp => rp.Managers)
				.AnyAsync(m => m.ManagerId == toManagerId);

			if (!targetHasCS)
			{
				TempData["Error"] = "目標客服不具客服權限。";
				return RedirectToAction(nameof(Reassign), new { id, returnUrl });
			}

			var fromId = t.AssignedManagerId;
			t.AssignedManagerId = toManagerId;

			_db.Set<Db.SupportTicketAssignment>().Add(new Db.SupportTicketAssignment
			{
				TicketId = id,
				FromManagerId = fromId,
				ToManagerId = toManagerId,
				AssignedByManagerId = meId.Value,
				AssignedAt = DateTime.UtcNow,
				Note = string.IsNullOrWhiteSpace(note) ? null : note!.Trim().Substring(0, Math.Min(255, note!.Trim().Length))
			});

			await _db.SaveChangesAsync();

			// 成功後：若有 returnUrl 就回列表，否則進對話
			return Url.IsLocalUrl(returnUrl)
				? Redirect(returnUrl!)
				: RedirectToAction(nameof(Ticket), new { id });
		}
	}
}
