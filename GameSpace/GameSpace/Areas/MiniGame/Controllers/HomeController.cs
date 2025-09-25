using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public IActionResult Login()
        {
            // 重導向到主登入系統
            return Redirect("/Login");
        }

        [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
        public IActionResult Index()
        {
            ViewData["Title"] = "小遊戲管理系統";
            ViewData["PageTitle"] = "小遊戲管理系統首頁";
            ViewData["Description"] = "歡迎使用小遊戲管理系統，您可以在這裡管理會員錢包、簽到系統、寵物系統和小遊戲系統。";
            
            return View();
        }

        [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
        public IActionResult Dashboard()
        {
            return RedirectToAction(nameof(Index));
        }
    }
}
