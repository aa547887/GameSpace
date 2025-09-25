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
            ViewData["Title"] = "小遊戲管理系統 - 登入";
            return View();
        }

        [Authorize(AuthenticationSchemes = "AdminCookie")]
        public IActionResult Index()
        {
            ViewData["Title"] = "小遊戲管理系統";
            return View();
        }
    }
}
