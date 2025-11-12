using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using GamiPort.Infrastructure.Security; // IAppCurrentUser：取得目前登入者資訊
using GamiPort.Models;                  // EF 模型（Notification/NotificationRecipient 等）

namespace GamiPort.Areas.social_hub.Controllers
{
    [Area("social_hub")]
    /// <summary>
    /// 前台（social_hub）通知中心：提供列表、詳情與標記已讀。
    /// 僅顯示目前登入使用者所收到的通知。
    /// </summary>
    public class NotificationController : Controller
    {
        private readonly GameSpacedatabaseContext _db;
        private readonly IAppCurrentUser _me;

        public NotificationController(GameSpacedatabaseContext db, IAppCurrentUser me)
        {
            _db = db;
            _me = me;
        }

        /// <summary>
        /// 通知列表：抓取目前登入使用者的收件紀錄，並提供分頁（每頁 10 筆）。
        /// 會載入來源/動作/寄送者等關聯資料，供畫面顯示名稱。
        /// </summary>
        public async Task<IActionResult> Index(int page = 1)
        {
            // 取得目前登入的使用者 Id（未登入則回傳 0）
            var currentUserId = await _me.GetUserIdAsync();
            if (currentUserId <= 0) return Unauthorized();

            // 基礎查詢（含顯示用的關聯）
            var baseQuery = _db.NotificationRecipients
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.Source)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.Action)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.SenderUser)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.SenderManager)
                .Where(nr => nr.UserId == currentUserId);

            // 分頁：固定 10 筆
            const int pageSize = 10;
            if (page < 1) page = 1;

            // 整體統計（非僅本頁）
            var totalCount = await baseQuery.CountAsync();
            var unreadCount = await baseQuery.CountAsync(x => x.ReadAt == null);
            var totalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            // 取本頁資料
            var list = await baseQuery
                .OrderByDescending(nr => nr.Notification.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 傳遞分頁資訊
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.UnreadCount = unreadCount;

            return View(list);
        }

        /// <summary>
        /// 通知詳情：僅允許本人檢視；若尚未讀，進入詳情時自動標記為已讀（UTC 時間）。
        /// 載入來源/動作/群組/寄送者資訊，以利頁面顯示完整脈絡。
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var currentUserId = await _me.GetUserIdAsync();
            if (currentUserId <= 0) return Unauthorized();

            var recipient = await _db.NotificationRecipients
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.Source)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.Action)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.Group)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.SenderUser)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.SenderManager)
                .FirstOrDefaultAsync(nr => nr.RecipientId == id && nr.UserId == currentUserId);

            if (recipient == null) return NotFound();

            // 首次閱讀：自動標記已讀
            if (recipient.ReadAt == null)
            {
                recipient.ReadAt = System.DateTime.UtcNow;
                await _db.SaveChangesAsync();
                ViewBag.AutoRead = true;
            }

            return View(recipient);
        }

        /// <summary>
        /// 通知詳情（局部檢視）：回傳 HTML 片段，供列表以 AJAX 載入顯示。
        /// 與 Detail 相同，首次閱讀會自動標記為已讀（UTC）。
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DetailPartial(int id)
        {
            var currentUserId = await _me.GetUserIdAsync();
            if (currentUserId <= 0) return Unauthorized();

            var recipient = await _db.NotificationRecipients
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.Source)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.Action)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.Group)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.SenderUser)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.SenderManager)
                .FirstOrDefaultAsync(nr => nr.RecipientId == id && nr.UserId == currentUserId);

            if (recipient == null) return NotFound();

            if (recipient.ReadAt == null)
            {
                recipient.ReadAt = System.DateTime.UtcNow;
                await _db.SaveChangesAsync();
                ViewBag.AutoRead = true;
            }

            return PartialView("_DetailPartial", recipient);
        }

        /// <summary>
        /// 標記為已讀：僅允許本人對自己的收件紀錄操作。
        /// 支援 AJAX 與一般 POST 兩種情境（AJAX 回 JSON，否則導回列表）。
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            var currentUserId = await _me.GetUserIdAsync();
            if (currentUserId <= 0) return Unauthorized();

            var recipient = await _db.NotificationRecipients
                .FirstOrDefaultAsync(nr => nr.RecipientId == id && nr.UserId == currentUserId);

            if (recipient == null) return NotFound();

            if (recipient.ReadAt == null)
            {
                recipient.ReadAt = System.DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            // 判斷是否為 AJAX 請求（避免 StringValues 與 string 的 == 歧義）
            var isAjax = Request.Headers.TryGetValue("X-Requested-With", out var xrw)
                         && string.Equals(xrw.ToString(), "XMLHttpRequest", System.StringComparison.OrdinalIgnoreCase);
            if (isAjax)
            {
                return Json(new { ok = true, id = recipient.RecipientId, readAt = recipient.ReadAt });
            }

            TempData["Msg"] = "已標記為已讀。";
            return RedirectToAction(nameof(Index));
        }
    }
}
