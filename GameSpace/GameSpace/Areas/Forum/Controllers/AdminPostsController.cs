using GameSpace.Areas.Forum.Models;
//using GameSpace.Areas.Forum.Models
using GameSpace.Models; // ← 這裡有 Scaffold 出來的 Post 實體（對應 posts 表）
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.Forum.Controllers
{
    [Area("Forum")]
    [Route("Forum/[controller]/[action]")]
    public class AdminPostsController : Controller
    {
        private readonly GameSpacedatabaseContext _db;
        public AdminPostsController(GameSpacedatabaseContext db) => _db = db;

        // ===== 列表 =====
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 不動 DB：直接用既有的 Post 實體查，再投影到 ListItem VM
            var list = await _db.Posts
                .AsNoTracking()
                .OrderByDescending(p => p.Pinned)
                .ThenByDescending(p => p.PublishedAt)
                .ThenByDescending(p => p.CreatedAt)
                .Select(p => new AdminPostListItemVm
                {
                    AdminPostId = p.PostId,     // ← 關鍵：映射成你 View 期望的名字
                    Title = p.Title,
                    Status = p.Status ?? "draft",
                    Pinned = p.Pinned ?? false,
                    CreatedAt = p.CreatedAt ?? DateTime.MinValue,
                    PublishedAt = p.PublishedAt
                })
                .ToListAsync();

            return View(list);
        }

        // ===== 建立 =====
        [HttpGet]
        public IActionResult Create() => View(new PostEditVm());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostEditVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var entity = new Post
            {
                Title = vm.Title,
                Tldr = vm.Tldr,
                BodyMd = vm.BodyMd,
                Pinned = vm.Pinned,
                Status = "draft",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Type = vm.Type ?? "insight",
                GameId = vm.GameId
                // CreatedBy / PublishedAt 視你的登入狀態再填
            };

            _db.Posts.Add(entity);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ===== 編輯 =====
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _db.Posts.AsNoTracking().FirstOrDefaultAsync(x => x.PostId == id);
            if (p == null) return NotFound();

            var vm = new PostEditVm
            {
                PostId = p.PostId,
                Title = p.Title,
                Tldr = p.Tldr,
                BodyMd = p.BodyMd,
                Pinned = p.Pinned ?? false,
                Type = p.Type,
                GameId = p.GameId
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PostEditVm vm)
        {
            if (!vm.PostId.HasValue) return BadRequest();

            var p = await _db.Posts.FirstOrDefaultAsync(x => x.PostId == vm.PostId.Value);
            if (p == null) return NotFound();

            p.Title = vm.Title;
            p.Tldr = vm.Tldr;
            p.BodyMd = vm.BodyMd;
            p.Pinned = vm.Pinned;
            p.Type = vm.Type ?? p.Type;
            p.GameId = vm.GameId;
            p.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ===== 發布（Index 上的按鈕）=====
        [HttpPost]
        public async Task<IActionResult> Publish(int id)
        {
            var p = await _db.Posts.FirstOrDefaultAsync(x => x.PostId == id);
            if (p == null) return NotFound();

            if ((p.Status ?? "draft") == "draft")
            {
                p.Status = "published";
                p.PublishedAt = DateTime.UtcNow;
                p.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            // 你的 Index.cshtml 是 form post → 走 redirect 才會回列表
            return RedirectToAction(nameof(Index));
        }

        // ===== 隱藏（Index 上的按鈕）=====
        [HttpPost]
        public async Task<IActionResult> Hide(int id)
        {
            var p = await _db.Posts.FirstOrDefaultAsync(x => x.PostId == id);
            if (p == null) return NotFound();

            p.Status = "hidden";
            p.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ===== 切換置頂（如果你之後加按鈕）=====
        [HttpPost]
        public async Task<IActionResult> TogglePin(int id)
        {
            var p = await _db.Posts.FirstOrDefaultAsync(x => x.PostId == id);
            if (p == null) return NotFound();

            p.Pinned = !p.Pinned;
            p.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
