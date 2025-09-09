using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.Forum.Models.Posts;

// EF 實體 / DbContext（照你的實際命名空間）
using GameSpace.Models;

// ViewModels（照你剛剛的檔案結構）
using GameSpace.Areas.Forum.Models;                // ThreadRowVm, ThreadsListVm
using GameSpace.Areas.Forum.Models.Posts;          // ThreadPostRowVm, PostsListVm

namespace GameSpace.Areas.Forum.Controllers
{
    [Area("Forum")]
    [Route("Forum/[controller]/[action]")]
    public class ThreadsController : Controller
    {
        private readonly GameSpacedatabaseContext _db;

        public ThreadsController(GameSpacedatabaseContext db)
        {
            _db = db;
        }

        // --------------------------
        // 1) 論壇清單（forums）
        // GET /Forum/Threads/Index
        // --------------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var forums = await _db.Forums
                .AsNoTracking()
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(forums); // 對應 Areas/Forum/Views/Threads/Index.cshtml
        }

        // -------------------------------------------------------
        // 2) 主題清單（threads）含：搜尋 q、狀態篩選、分頁 page/size
        // GET /Forum/Threads/List?forumId=1&q=abc&status=normal&page=1&size=20
        // -------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> List(int forumId, string? q, string? status, int page = 1, int size = 20)
        {
            if (page < 1) page = 1;
            if (size < 1) size = 20;

            var query = _db.Threads
                .AsNoTracking()
                .Where(t => t.ForumId == forumId);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(t => t.Title.Contains(q));

            if (!string.IsNullOrWhiteSpace(status) &&
                !status.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                query = query.Where(t => t.Status == status);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.UpdatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(t => new ThreadRowVm
                {
                    ThreadId = t.ThreadId,
                    ForumId = t.ForumId,
                    Title = t.Title,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    // 聚合：回覆數、讚數（量小先用 Count；之後可優化成 GroupBy）
                    ReplyCount = _db.ThreadPosts.Count(p => p.ThreadId == t.ThreadId && p.Status == "normal"),
                    LikeCount = _db.Reactions.Count(r => r.TargetType == "thread" && r.TargetId == t.ThreadId && r.Kind == "like"),
                          // 收藏數（bookmarks: target_type='thread'）
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

            return View(vm); // 對應 Areas/Forum/Views/Threads/List.cshtml（@model ThreadsListVm）
        }

        // ---------------------------------------
        // 3) 修改主題狀態（hidden/archived/normal）
        // POST /Forum/Threads/ChangeStatus
        // ---------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(long id, string status, int? forumId)
        {
            var t = await _db.Threads.FindAsync(id);
            if (t == null) return NotFound();

            t.Status = status;
            t.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(List), new { forumId = forumId ?? t.ForumId });
        }

        // -------------------------------------------------
        // 4) 回覆清單（thread_posts）— 可先顯示，之後再加搜尋/分頁
        // GET /Forum/Threads/Posts?threadId=123
        // -------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Posts(long threadId)
        {
            var rows = await _db.ThreadPosts
                .AsNoTracking()
                .Where(p => p.ThreadId == threadId)
                .OrderBy(p => p.CreatedAt)
                .Select(p => new ThreadPostRowVm
                {
                    Id = p.Id,
                    ThreadId = p.ThreadId,
                    AuthorUserId = p.AuthorUserId,
                    ContentMd = p.ContentMd,
                    ParentPostId = p.ParentPostId,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    // 讚數（reactions: target_type='post' & kind='like'）
                    LikeCount = _db.Reactions.Count(r => r.TargetType == "post" && r.TargetId == p.Id && r.Kind == "like"),
                    // 收藏數（bookmarks: target_type='post'）
                    BookmarkCount = _db.Bookmarks.Count(b => b.TargetType == "post" && b.TargetId == p.Id)
                })
                .ToListAsync();

            var vm = new PostsListVm
            {
                ThreadId = threadId,
                Total = rows.Count,
                Items = rows
            };

            return View(vm); // 對應 Areas/Forum/Views/Threads/Posts.cshtml（@model PostsListVm）
        }

        // --------------------------------------
        // 5) 修改回覆狀態（normal/hidden/deleted）
        // POST /Forum/Threads/ChangePostStatus
        // --------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePostStatus(long id, string status, long threadId)
        {
            var p = await _db.ThreadPosts.FindAsync(id);
            if (p == null) return NotFound();

            p.Status = status;
            p.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Posts), new { threadId });
        }
    }
}
