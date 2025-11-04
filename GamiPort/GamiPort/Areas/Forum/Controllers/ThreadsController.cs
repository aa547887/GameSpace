using Microsoft.AspNetCore.Mvc;
using GamiPort.Models;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.Forum.Controllers
{
    [Area("Forum")]
    public class ThreadsController : Controller
    {
        private readonly GameSpacedatabaseContext _db;

        // ★ 建構子注入 DbContext（或你也可以改用 IForumsService）
        public ThreadsController(GameSpacedatabaseContext db)
        {
            _db = db;
        }

        // /Forum/Threads/Index?forumId=8
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int forumId)
        {
            if (forumId <= 0) return BadRequest("forumId required");

            // ★ 這裡把 forum 撈出來
            var forum = await _db.Forums
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(x => x.ForumId == forumId);

            ViewBag.ForumId = forumId;
            ViewBag.ForumName = forum?.Name ?? "未命名論壇";   // ★ 不會再 CS0103 了
            return View();
        }

        // /Forum/Threads/Detail?threadId=456
        [HttpGet]
        public IActionResult Detail([FromQuery] long threadId)
        {
            ViewData["ThreadId"] = threadId;
            return View();
        }
    }
}
