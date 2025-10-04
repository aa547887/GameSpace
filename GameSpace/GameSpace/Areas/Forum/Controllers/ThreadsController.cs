using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// EF 實體 / DbContext
using GameSpace.Models;

// ViewModels
using GameSpace.Areas.Forum.Models;       // ThreadRowVm, ThreadsListVm
using GameSpace.Areas.Forum.Models.Posts; // ThreadPostRowVm, PostsListVm

namespace GameSpace.Areas.Forum.Controllers
{
    [Area("Forum")]
    [Route("Forum/[controller]/[action]")]
    public class ThreadsController : Controller
    {
        private readonly GameSpacedatabaseContext _db;
        public ThreadsController(GameSpacedatabaseContext db) => _db = db;

        // 1) 論壇清單
        [HttpGet]
        public async Task<IActionResult> Index(string? q, string sort = "created", string dir = "desc")
        {
            var query = _db.Forums.AsNoTracking();

            // 搜尋（名稱包含）
            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = $"%{q!.Trim()}%";
                query = query.Where(f => EF.Functions.Like((f.Name ?? string.Empty), keyword));
            }

            // 排序（id / name / created）
            bool desc = dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
            query = sort switch
            {
                "id" => desc ? query.OrderByDescending(f => f.ForumId)
                             : query.OrderBy(f => f.ForumId),

                "name" => desc
                    ? query.OrderByDescending(f => EF.Functions.Collate(f.Name, "Chinese_Taiwan_Stroke_CI_AS"))
                           .ThenBy(f => f.ForumId)
                    : query.OrderBy(f => EF.Functions.Collate(f.Name, "Chinese_Taiwan_Stroke_CI_AS"))
                           .ThenBy(f => f.ForumId),

                _ /*created*/ => desc ? query.OrderByDescending(f => f.CreatedAt).ThenBy(f => f.ForumId)
                                      : query.OrderBy(f => f.CreatedAt).ThenBy(f => f.ForumId)
            };

            ViewBag.Q = q;
            ViewBag.Sort = sort;
            ViewBag.Dir = dir;

            var list = await query.ToListAsync();
            return View(list); // 型別仍是 IEnumerable<GameSpace.Models.Forum>
        }


        // 2) 主題清單（搜尋/篩選/分頁）
        [HttpGet]
        public async Task<IActionResult> List(int forumId, string? q, string? status, int page = 1, int size = 20)
        {
            if (page < 1) page = 1;
            if (size < 1) size = 20;

            var query = _db.Threads.AsNoTracking().Where(t => t.ForumId == forumId);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = $"%{q!.Trim()}%";
                query = query.Where(t => EF.Functions.Like((t.Title ?? string.Empty), keyword));
            }

            if (!string.IsNullOrWhiteSpace(status) &&
                !status.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(t => t.Status != null && t.Status == status);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.UpdatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(t => new ThreadRowVm
                {
                    ThreadId = t.ThreadId,
                    ForumId = t.ForumId,
                    Title = t.Title ?? string.Empty,
                    Status = string.IsNullOrWhiteSpace(t.Status) ? "normal" : t.Status!,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    ReplyCount = _db.ThreadPosts.Count(p => p.ThreadId == t.ThreadId && p.Status == "normal"),
                    LikeCount = _db.Reactions.Count(r => r.TargetType == "thread" && r.TargetId == t.ThreadId && r.Kind == "like"),
                    BookmarkCount = _db.Bookmarks.Count(b => b.TargetType == "thread" && b.TargetId == t.ThreadId)
                })
                .ToListAsync();

            var vm = new ThreadsListVm
            {
                ForumId = forumId,
                Q = q,
                Status = string.IsNullOrWhiteSpace(status) ? "ALL" : status,
                Page = page,
                Size = size,
                Total = total,
                Items = items
            };

            return View(vm);
        }

        // 3) 修改主題狀態（white-list）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(long id, string? status, int? forumId)
        {
            var thread = await _db.Threads.FirstOrDefaultAsync(x => x.ThreadId == id);
            if (thread == null) return NotFound();

            var normalizedStatus = (status ?? string.Empty).Trim().ToLowerInvariant();
            var isAllowed = normalizedStatus is "normal" or "hidden" or "archived" or "deleted";
            var targetStatus = isAllowed ? normalizedStatus : (thread.Status ?? "normal");

            if (!string.Equals(thread.Status, targetStatus, StringComparison.OrdinalIgnoreCase))
            {
                thread.Status = targetStatus;
                thread.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            var redirectForumId = forumId ?? thread.ForumId;
            return RedirectToAction(nameof(List), new { forumId = redirectForumId ?? 0 });
        }

        // 4) 回覆清單（帶 forumId 回去、帶作者顯示名）
        [HttpGet]
        public async Task<IActionResult> Posts(long threadId)
        {
            // 先取 forumId 讓「回主題列表」有得帶
            var t = await _db.Threads
                .AsNoTracking()
                .Where(x => x.ThreadId == threadId)
                .Select(x => new { x.ForumId })
                .FirstOrDefaultAsync();
            if (t == null) return NotFound();

            var rows = await (
                from p in _db.ThreadPosts.AsNoTracking().Where(p => p.ThreadId == threadId)
                join u in _db.Users.AsNoTracking() on p.AuthorUserId equals u.UserId into uu
                from u in uu.DefaultIfEmpty() // 作者被刪也不炸
                orderby p.CreatedAt
                select new ThreadPostRowVm
                {
                    Id = p.Id,
                    ThreadId = p.ThreadId,
                    AuthorUserId = p.AuthorUserId,
                    AuthorName = u.UserName, // ★ 只用現有欄位
                    ContentMd = p.ContentMd ?? string.Empty,
                    ParentPostId = p.ParentPostId,
                    Status = string.IsNullOrWhiteSpace(p.Status) ? "normal" : p.Status!,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    LikeCount = _db.Reactions.Count(r => r.TargetType == "post" && r.TargetId == p.Id && r.Kind == "like"),
                    BookmarkCount = _db.Bookmarks.Count(b => b.TargetType == "post" && b.TargetId == p.Id)
                })
                .ToListAsync();

            var vm = new PostsListVm { ThreadId = threadId, Total = rows.Count, Items = rows };
            ViewBag.ForumId = t.ForumId ?? threadId; // ★ 給「← 回主題列表」
            return View(vm);
        }


        // 5) 修改回覆狀態（white-list）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePostStatus(long id, string? status, long threadId)
        {
            var post = await _db.ThreadPosts.FirstOrDefaultAsync(x => x.Id == id && x.ThreadId == threadId);
            if (post == null) return NotFound();

            var normalizedStatus = (status ?? string.Empty).Trim().ToLowerInvariant();
            var isAllowed = normalizedStatus is "normal" or "hidden" or "deleted";
            var targetStatus = isAllowed ? normalizedStatus : (post.Status ?? "normal");

            if (!string.Equals(post.Status, targetStatus, StringComparison.OrdinalIgnoreCase))
            {
                post.Status = targetStatus;
                post.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Posts), new { threadId });
        }






    }
}

