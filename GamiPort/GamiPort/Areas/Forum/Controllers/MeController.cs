using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.Forum.Controllers
{
    [Area("Forum")]
    public class MeController : Controller
    {
        private bool IsLogin => User?.Identity?.IsAuthenticated == true;

        private string BuildLoginUrl()
        {
            var back = (Request.Path + Request.QueryString).ToString();
            return $"/Login/Login/Login/Login?ReturnUrl={Uri.EscapeDataString(back)}";
        }

        public IActionResult Threads()
        {
            if (!IsLogin) return Redirect(BuildLoginUrl());
            return View("Index", model: "/api/forum/me/threads"); // ← 小寫，比較保險
        }

        public IActionResult Posts()
        {
            if (!IsLogin) return Redirect(BuildLoginUrl());
            return View("Index", model: "/api/forum/me/posts");
        }

        public IActionResult Likes()
        {
            if (!IsLogin) return Redirect(BuildLoginUrl());
            return View("Index", model: "/api/forum/me/likes/threads");
        }
    }

}
