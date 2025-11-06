using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.Forum.Controllers
{
    [Area("Forum")] // 告訴 MVC：這是 Forum Area
    public class HomeController : Controller
    {
        // GET /Forum
        public IActionResult Index()
        {
            // 直接載入 Areas/Forum/Views/Home/Index.cshtml
            return View();
        }
    }
}
