using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.Forum.Controllers
{
    [Area("Forum")]
    [Authorize] // 進頁面就要登入，避免 API 401 彈回
    public class MeController : Controller
    {
        public IActionResult Threads() => View("Index", model: "/api/Forum/me/threads");
        public IActionResult Posts() => View("Index", model: "/api/Forum/me/posts");
        public IActionResult Likes() => View("Index", model: "/api/Forum/me/likes/threads");
    }
}
