using Microsoft.AspNetCore.Mvc;


namespace GamiPort.Areas.Forum.Controllers
{
    [Area("Forum")]
    public class BoardsController : Controller
    {
        // /Forum/Boards/Index/1
        public IActionResult Index(int id)
        {
            ViewBag.ForumId = id; // 讓 Razor 傳給 Vue 的 props
            return View();
        }
    }
}
