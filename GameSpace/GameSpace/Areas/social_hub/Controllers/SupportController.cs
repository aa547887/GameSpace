// =============================================================
// File: Areas/social_hub/Controllers/SupportController.cs
// Purpose: 客服工作台（Support Desk）— 後台 GameSpace
// 變更重點：SendMessage() 加入「去重保護（時間窗 + 內容比對）」避免重複寫入
//          SaveChanges 成功後，仍使用 _notifier 廣播到前台 7160 的 SupportHub（ticket:{id}）
//          ※ 不修改 DB / EF Model（遵照你的要求）
// =============================================================

using GameSpace.Areas.social_hub.Auth;
using GameSpace.Areas.social_hub.Models.ViewModels;
using GameSpace.Areas.social_hub.Permissions;
using GameSpace.Infrastructure.Time;           // IAppClock：DB 存 UTC / 顯示端轉台灣
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
// 跨站廣播
using GameSpace.Areas.social_hub.Services.Abstractions;

// EF 實體別名
using Db = GameSpace.Models;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	[SocialHubAuth] // 讓 Items: gs_kind / gs_id 先就位（相容變數轉換，不做 Authenticate）
	[RequireManagerPermissions(CustomerService = true)] // 僅客服可進
	public class SupportController : Controller
	{
		// ================================= [ Fields / Ctor ] ================================
		private readonly GameSpacedatabaseContext _db;
		private readonly IManagerPermissionService _perm;
		private readonly IAppClock _clock;
		private readonly ISupportNotifier _notifier; // 廣播到前台 7160
		private const int MessagePageSize = 30;

		public SupportController(
			GameSpacedatabaseContext db,
			IManagerPermissionService perm,
			IAppClock clock,
			ISupportNotifier notifier
		)
		{
			_db = db;
			_perm = perm;
			_clock = clock;
			_notifier = notifier;
		}

		// ============================== [ Identity Resolution ] =============================
		/// <summary>解析目前登入的管理員編號（優先 HttpContext.Items → Claims）。</summary>
		private int? ResolveManagerId()
		{
			if (HttpContext?.Items?.TryGetValue("gs_kind", out var kindObj) == true
				&& kindObj is bool isManager && isManager
				&& HttpContext.Items.TryGetValue("gs_id", out var idObj)
				&& idObj is int id && id > 0)
			{
				return id;
			}

			var p = User;
			var mgrClaim = p?.FindFirst("mgr:id")?.Value;
			if (int.TryParse(mgrClaim, out var mid) && mid > 0) return mid;

			if (p?.HasClaim(c => c.Type == "IsManager" && c.Value == "true") == true)
			{
				var idStr = p.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (int.TryParse(idStr, out var fallback) && fallback > 0) return fallback;
			}
			return null;
		}

		#region Index / Counts
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var _ = await _perm.GetCustomerServiceContextAsync(HttpContext);

			var meId = ResolveManagerId();
			ViewBag.MeManagerId = meId;
			ViewBag.CanAssignToOthers = meId.HasValue && await _perm.HasCsAssignPermissionAsync(meId.Value);
			return View();
		}

		[HttpGet]
		public async Task<IActionResult> GetCounts()
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var q = _db.SupportTickets.AsNoTracking();

			var assignedCount = await q.Where(t => t.IsClosed != true && t.AssignedManagerId == meId.Value).CountAsync();
			var unassignedCount = await q.Where(t => t.IsClosed != true && t.AssignedManagerId == null).CountAsync();
			var closedCount = await q.Where(t => t.IsClosed == true).CountAsync();
			var inprogressCount = await q.Where(t => t.IsClosed != true && (t.AssignedManagerId == null || t.AssignedManagerId != meId.Value)).CountAsync();

			return Ok(new { assigned = assignedCount, unassigned = unassignedCount, closed = closedCount, inprogress = inprogressCount });
		}
		#endregion

		#region List helpers
		private static IQueryable<Db.SupportTicket> ApplyKeyword(
			IQueryable<Db.SupportTicket> src, string? q, Microsoft.EntityFrameworkCore.DbContext? forLike = null)
		{
			if (string.IsNullOrWhiteSpace(q)) return src;
			q = q.Trim();
			if (int.TryParse(q, out var num))
				return src.Where(t => t.TicketId == num || t.UserId == num);

			return src.Where(t => EF.Functions.Like(t.Subject ?? string.Empty, $"%{q}%"));
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
				UnreadForMe = db.SupportTicketMessages
					.Where(m =>
						m.TicketId == t.TicketId &&
						m.SenderUserId != null &&          // 使用者→客服
						m.ReadByManagerAt == null &&
						m.SentAt >= (
							db.SupportTicketAssignments
								.Where(a => a.TicketId == t.TicketId
											&& t.AssignedManagerId != null
											&& a.ToManagerId == t.AssignedManagerId)
								.OrderByDescending(a => a.AssignedAt).ThenByDescending(a => a.AssignmentId)
								.Select(a => (DateTime?)a.AssignedAt)
								.FirstOrDefault()
							?? t.CreatedAt
						)
					)
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
			return new PagedResult<TicketListItemVM> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
		}
		#endregion

		#region Lists
		[HttpGet]
		public async Task<IActionResult> ListAssigned(int page = 1, string? q = null, int pageSize = 10)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var baseQ = _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed != true && t.AssignedManagerId == meId.Value);

			baseQ = ApplyKeyword(baseQ, q);

			var ordered = OrderForList(ProjectTicketListVM(baseQ, _db));
			var paged = await ToPagedAsync(ordered, page, pageSize);

			ViewData["action"] = nameof(ListAssigned);
			ViewBag.Query = q ?? string.Empty;
			ViewBag.CanAssignToOthers = await _perm.HasCsAssignPermissionAsync(meId.Value);
			ViewBag.MeManagerId = meId.Value;
			return PartialView("_TicketList", paged);
		}

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

			ViewData["action"] = nameof(ListUnassigned);
			ViewBag.Query = q ?? string.Empty;
			ViewBag.CanAssignToOthers = await _perm.HasCsAssignPermissionAsync(meId.Value);
			ViewBag.MeManagerId = meId.Value;
			return PartialView("_TicketList", paged);
		}

		[HttpGet]
		public async Task<IActionResult> ListInProgress(int page = 1, string? q = null, int pageSize = 10)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();
			if (!await _perm.HasCsAssignPermissionAsync(meId.Value)) return Forbid();

			var baseQ = _db.SupportTickets.AsNoTracking()
				.Where(t => t.IsClosed != true && (t.AssignedManagerId == null || t.AssignedManagerId != meId.Value));

			baseQ = ApplyKeyword(baseQ, q);

			var ordered = OrderForList(ProjectTicketListVM(baseQ, _db));
			var paged = await ToPagedAsync(ordered, page, pageSize);

			ViewData["action"] = nameof(ListInProgress);
			ViewBag.Query = q ?? string.Empty;
			ViewBag.CanAssignToOthers = true;
			ViewBag.MeManagerId = meId.Value;
			return PartialView("_TicketList", paged);
		}

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

			ViewData["action"] = nameof(ListClosed);
			ViewBag.Query = q ?? string.Empty;
			ViewBag.CanAssignToOthers = await _perm.HasCsAssignPermissionAsync(meId.Value);
			ViewBag.MeManagerId = meId.Value;
			return PartialView("_TicketList", paged);
		}
		#endregion

		#region Assignment history
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

		[HttpGet]
		public async Task<IActionResult> AssignmentHistoryPage(int id, int page = 1)
		{
			const int pageSize = 10;

			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			var baseQ = _db.Set<Db.SupportTicketAssignment>()
				.AsNoTracking()
				.Where(a => a.TicketId == id)
				.OrderByDescending(a => a.AssignedAt).ThenByDescending(a => a.AssignmentId);

			var total = await baseQ.CountAsync();
			if (page <= 0) page = 1;
			var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
			if (page > totalPages) page = totalPages;

			var items = await baseQ.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

			var vm = new AssignmentHistoryPageVM
			{
				TicketId = t.TicketId,
				UserId = t.UserId,
				Subject = t.Subject ?? string.Empty,
				Items = items,
				Page = page,
				PageSize = pageSize,
				TotalCount = total
			};
			return View("AssignmentHistoryPage", vm);
		}
		#endregion

		#region Segment utilities
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
		#endregion

		#region Assignment conversation
		[HttpGet]
		public async Task<IActionResult> AssignmentConversation(int id, int aid, int page = 1)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			var (segStart, segEnd, rec) = await GetAssignmentBoundsAsync(id, aid);
			if (rec == null) return NotFound();

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
					MessageText = m.MessageText ?? string.Empty,
					SentAt = m.SentAt,
					ReadByUserAt = m.ReadByUserAt,
					ReadByManagerAt = m.ReadByManagerAt,
					IsMine = (m.SenderManagerId != null && m.SenderManagerId.Value == meId.Value)
				})
				.ToList();

			var vm = new AssignmentConversationVM
			{
				TicketId = t.TicketId,
				AssignmentId = rec.AssignmentId,
				UserId = t.UserId,
				Subject = t.Subject ?? string.Empty,
				AssignedByManagerId = rec.AssignedByManagerId,
				FromManagerId = rec.FromManagerId,
				ToManagerId = rec.ToManagerId,
				AssignedAt = rec.AssignedAt,
				NextAssignedAt = segEnd,
				Note = rec.Note,
				Messages = messagesAsc,
				Page = page,
				PageSize = MessagePageSize,
				TotalMessages = total
			};

			return View("AssignmentConversation", vm);
		}
		#endregion

		#region Ticket details / Chat
		        [HttpGet]
		        public async Task<IActionResult> Ticket(int id, int page = 1)
		        {
		            var meId = ResolveManagerId();
		            if (!meId.HasValue) return Unauthorized();
		
		            var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
		            if (t == null) return NotFound();
		
		            // [Gemini] 獨立找出使用者的第一則訊息，並傳到 View
            var firstMessage = await _db.SupportTicketMessages.AsNoTracking()
                .Where(m => m.TicketId == id && m.SenderUserId != null)
                .OrderBy(m => m.SentAt).ThenBy(m => m.MessageId) // 穩定排序：先依 SentAt，再以 MessageId 當平手序，確保最早一筆
                .FirstOrDefaultAsync();
            ViewBag.InitialContent = firstMessage?.MessageText; // 僅作詳情顯示，非對話串
		
		            // 未指派 → 導回
		            if (t.AssignedManagerId == null)			{
				if (await _perm.HasCsAssignPermissionAsync(meId.Value))
					return RedirectToAction(nameof(Reassign), new { id, returnUrl = Url.Action(nameof(Index), new { area = "social_hub", tab = "unassigned" }) });

				TempData["Warn"] = "此工單尚未指派，請先指派後再進入對話。";
				return RedirectToAction(nameof(Index), new { area = "social_hub", tab = "unassigned" });
			}

			var isClosed = t.IsClosed == true;
			var canAssignAny = await _perm.HasCsAssignPermissionAsync(meId.Value);

			// 目前指派分段（UTC）
			var (segStart, segEnd) = await GetCurrentSegmentBoundsAsync(id, t.AssignedManagerId);

			// 現任負責人 → 把分段內未讀（使用者→客服）標記已讀
			if (t.AssignedManagerId == meId.Value && segStart != null)
			{
				var unreadQ = _db.SupportTicketMessages
					.Where(m => m.TicketId == id && m.SenderUserId != null && m.ReadByManagerAt == null)
					.Where(m => m.SentAt >= segStart.Value);
				if (segEnd != null) unreadQ = unreadQ.Where(m => m.SentAt < segEnd.Value);

				var unread = await unreadQ.ToListAsync();
				if (unread.Count > 0)
				{
					var nowRead = _clock.UtcNow;   // ★ UTC
					foreach (var m in unread) m.ReadByManagerAt = nowRead;
					await _db.SaveChangesAsync();
				}
			}

			if (page <= 0) page = 1;

			// 對話資料（依分段）
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
				CanAssignToMe = !isClosed && t.AssignedManagerId == null,
				CanReassign = !isClosed && (t.AssignedManagerId == meId.Value || canAssignAny),
				CanClose = !isClosed && (t.AssignedManagerId == meId.Value || canAssignAny),

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
		#endregion

		#region Messaging / AssignToMe / Close / Reassign / TicketInfo

		/// <summary>
		/// 發送訊息（AJAX）。僅現任負責人可發送；超過 255 字會截斷。
		/// ★ 新增：去重保護（只改控制器，不改 DB/EF）
		///   - 用「時間窗 + 同 sender + 同內容」偵測重複送出（例如連點、重綁事件）
		///   - 預設時間窗 8 秒內視為重複（可依需要調整）
		/// </summary>
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

			// ====== 去重保護開始（僅控制器層，無需 DB schema）======
			var now = _clock.UtcNow;                 // UTC
			const int WINDOW_SECONDS = 8;            // 去重時間窗（秒）
			var since = now.AddSeconds(-WINDOW_SECONDS);

			// 查詢最近時間窗內，同一管理員、同一工單、同一內容，是否已有一筆
			// 說明：不用 OrderBy + First 的原因是 Any 較省資源，夠用就好
			var duplicated = await _db.SupportTicketMessages
				.AsNoTracking()
				.AnyAsync(m =>
					m.TicketId == id &&
					m.SenderManagerId == meId.Value &&
					m.SenderUserId == null &&          // 管理員發出
					m.SentAt >= since &&               // 時間窗內
					m.MessageText == text);            // 內容相同

			if (duplicated)
			{
				// 前端收到 ok=true 但 dedup=true，可選擇靜默處理不再 append
				return Ok(new { ok = true, dedup = true });
			}
			// ====== 去重保護結束 ======

			// 正常寫入
			_db.SupportTicketMessages.Add(new Db.SupportTicketMessage
			{
				TicketId = id,
				SenderUserId = null,
				SenderManagerId = meId.Value,
				MessageText = text,
				SentAt = now,              // UTC
				ReadByUserAt = null,
				ReadByManagerAt = now      // 自己發的視為已讀
			});

			t.LastMessageAt = now;        // 列表排序依據
			await _db.SaveChangesAsync();

			// 即時廣播到前台（7160）SupportHub
			await _notifier.BroadcastMessageAsync(new SupportMessageDto
			{
				TicketId = id,
				SenderUserId = null,
				SenderManagerId = meId.Value,
				MessageText = text,
				SentAt = now
			});

			return Ok(new { ok = true, sentAt = now, text });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
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
				AssignedAt = _clock.UtcNow, // UTC
				Note = null
			});

			await _db.SaveChangesAsync();
			return Ok(new { ok = true, assignedManagerId = meId.Value });
		}

		[HttpGet]
		public async Task<IActionResult> Close(int id, bool modal = false)
		{
			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var canAssignAny = await _perm.HasCsAssignPermissionAsync(meId.Value);
			if (!(t.AssignedManagerId == meId.Value || canAssignAny))
				return Forbid();

			var vm = new CloseTicketVM
			{
				TicketId = t.TicketId,
				UserId = t.UserId,
				Subject = t.Subject ?? string.Empty,
				AssignedManagerId = t.AssignedManagerId,
				CreatedAt = t.CreatedAt,
				LastMessageAt = t.LastMessageAt
			};

			if (modal)
			{
				ViewData["Partial"] = "_CloseForm";
				return View("~/Areas/social_hub/Views/Shared/ModalWrapper.cshtml", vm);
			}
			return RedirectToAction(nameof(Ticket), new { id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CloseConfirm(int id, [FromForm] string? closeNote, string? returnUrl = null)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

			var t = await _db.SupportTickets.FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();

			if (t.IsClosed == true)
				return isAjax
					? Ok(new { ok = true, alreadyClosed = true })
					: (Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction(nameof(Ticket), new { id }));

			var canAssignAny = await _perm.HasCsAssignPermissionAsync(meId.Value);
			if (!(t.AssignedManagerId == meId.Value || canAssignAny))
				return Forbid();

			t.IsClosed = true;
			t.ClosedAt = _clock.UtcNow;        // UTC
			t.ClosedByManagerId = meId.Value;
			if (!string.IsNullOrWhiteSpace(closeNote))
			{
				closeNote = closeNote.Trim();
				t.CloseNote = closeNote.Length > 255 ? closeNote.Substring(0, 255) : closeNote;
			}
			else t.CloseNote = null;

			await _db.SaveChangesAsync();
			return isAjax
				? Ok(new { ok = true })
				: (Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction(nameof(Ticket), new { id }));
		}

		[HttpGet]
		public async Task<IActionResult> Reassign(int id, string? returnUrl = null, bool modal = false)
		{
			var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();
			if (t.IsClosed == true) return Forbid();

			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();

			var canAssignAny = await _perm.HasCsAssignPermissionAsync(meId.Value);
			if (!(t.AssignedManagerId == meId.Value || canAssignAny)) return Forbid();

			var candidates = await _db.Set<ManagerRolePermission>()
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
				CandidateManagerIds = candidates,
				CurrentAssignedManagerId = t.AssignedManagerId
			};

			ViewBag.MeManagerId = meId;
			ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action(nameof(Ticket), new { id });

			if (modal)
			{
				ViewData["Partial"] = "_ReassignForm";
				return View("~/Areas/social_hub/Views/Shared/ModalWrapper.cshtml", vm);
			}
			return RedirectToAction(nameof(Ticket), new { id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ReassignConfirm(int id, [FromForm] int toManagerId,
														[FromForm] string? note, string? returnUrl = null)
		{
			var meId = ResolveManagerId();
			if (!meId.HasValue) return Unauthorized();
			bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

			var t = await _db.SupportTickets.FirstOrDefaultAsync(x => x.TicketId == id);
			if (t == null) return NotFound();
			if (t.IsClosed == true) return Forbid();

			var canAssignAny = await _perm.HasCsAssignPermissionAsync(meId.Value);
			if (!(t.AssignedManagerId == meId.Value || canAssignAny)) return Forbid();

			// 1) no-op：目標 == 目前
			if (t.AssignedManagerId == toManagerId)
			{
				if (isAjax) return Ok(new { ok = true, same = true });
				return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction(nameof(Ticket), new { id });
			}

			// 2) 目前就是我 → 擋「指派給自己」
			bool assigningToSelf = (toManagerId == meId.Value);
			bool currentlyMine = (t.AssignedManagerId == meId.Value);
			if (assigningToSelf && currentlyMine)
				return isAjax ? BadRequest("目前已由你負責，無需再指派給自己。") : Forbid();

			// 3) 目標必須具客服 Gate
			bool targetHasCS = await _db.Set<ManagerRolePermission>()
				.Where(rp => rp.CustomerService == true)
				.SelectMany(rp => rp.Managers)
				.AnyAsync(m => m.ManagerId == toManagerId);

			if (!targetHasCS)
				return isAjax ? BadRequest("目標不具客服權限，無法指派。")
							  : RedirectToAction(nameof(Reassign), new { id, returnUrl });

			// 4) 寫入 Assignment（UTC）
			var fromId = t.AssignedManagerId;
			t.AssignedManagerId = toManagerId;

			_db.Set<Db.SupportTicketAssignment>().Add(new Db.SupportTicketAssignment
			{
				TicketId = id,
				FromManagerId = fromId,
				ToManagerId = toManagerId,
				AssignedByManagerId = meId.Value,
				AssignedAt = _clock.UtcNow, // UTC
				Note = string.IsNullOrWhiteSpace(note) ? null : note!.Trim()[..Math.Min(255, note.Trim().Length)]
			});

			// 轉單後直接清除未讀（使用者→客服）
			var now = _clock.UtcNow;
			var unread = await _db.SupportTicketMessages
				.Where(m => m.TicketId == id && m.SenderUserId != null && m.ReadByManagerAt == null)
				.ToListAsync();

			if (unread.Count > 0)
			{
				foreach (var m in unread) m.ReadByManagerAt = now;
			}

			await _db.SaveChangesAsync();
			return isAjax ? Ok(new { ok = true })
						  : (Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction(nameof(Ticket), new { id }));
		}

		        [HttpGet]
		        public async Task<IActionResult> TicketInfo(int id, bool modal = false)
		        {
		            var t = await _db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id);
		            if (t == null) return NotFound();
		
		            // [Gemini] 找出使用者的第一則訊息
            var firstMessage = await _db.SupportTicketMessages.AsNoTracking()
                .Where(m => m.TicketId == id && m.SenderUserId != null)
                .OrderBy(m => m.SentAt).ThenBy(m => m.MessageId) // 穩定排序：先依 SentAt，再以 MessageId 當平手序，確保最早一筆
                .FirstOrDefaultAsync();

            ViewBag.InitialContent = firstMessage?.MessageText; // 僅作詳情顯示，非對話串
		
		            var vm = new TicketInfoVM
		            {
		                TicketId = t.TicketId,
		                UserId = t.UserId,
		                Subject = t.Subject ?? "",
		                AssignedManagerId = t.AssignedManagerId,
		                CreatedAt = t.CreatedAt,
		                LastMessageAt = t.LastMessageAt,
		                IsClosed = t.IsClosed,
		                ClosedAt = t.ClosedAt,
		                CloseNote = t.CloseNote
		            };
		
		            if (modal)
		            {
		                ViewData["Partial"] = "_TicketInfo";
		                return View("~/Areas/social_hub/Views/Shared/ModalWrapper.cshtml", vm);
		            }
		            return RedirectToAction(nameof(Ticket), new { id });
		        }		
		#endregion
	}
}
