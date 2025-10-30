using Microsoft.AspNetCore.Mvc;


namespace GamiPort.Areas.Forum.Controllers
{
    [Area("Forum")]
    public class ThreadsController : Controller
    {
        // 這是論壇看板頁面
        // 範例 URL: /Forum/Threads?forumId=123
        public IActionResult Index(int forumId)
        {
            ViewData["ForumId"] = forumId;
            return View(); // 對應 Areas/Forum/Views/Threads/Index.cshtml
        }

        // 這是單一主題內容頁
        // 範例 URL: /Forum/Threads/Detail?threadId=456
        public IActionResult Detail(long threadId)
        {
            ViewData["ThreadId"] = threadId;
            return View(); // 對應 Areas/Forum/Views/Threads/Detail.cshtml
        }
    }
}
