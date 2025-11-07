using Microsoft.AspNetCore.Mvc;
using GamiPort.Services;

namespace GamiPort.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    [Route("OnlineStore/[controller]")] // => /OnlineStore/Store/...
    public class StoreController : Controller
    {
        private readonly ICurrentUserService _currentUser;
        public StoreController(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        /// <summary>
        /// 首頁：/OnlineStore、/OnlineStore/Store、/OnlineStore/Store/Index
        /// </summary>
        [HttpGet("")]
        [HttpGet("Index")]
        [HttpGet("~/OnlineStore")]
        public IActionResult Index()
        {
            return View();
        }

        // =========================
        // 舊連結轉向（Redirect）
        // =========================

        /// <summary>
        /// 舊：/OnlineStore/Store/Browse -> 新：/OnlineStore/Browse/Index
        /// </summary>
        [HttpGet("Browse")]
        public IActionResult BrowseLegacy()
            => RedirectToAction("Index", "Browse", new { area = "OnlineStore" });

        /// <summary>
        /// 舊：/OnlineStore/Store/Product/{id} -> 新：/OnlineStore/Product/Detail/{id}
        /// </summary>
        [HttpGet("Product/{id:int}")]
        public IActionResult ProductLegacy(int id)
            => RedirectToAction("Detail", "Product", new { area = "OnlineStore", id });

        /// <summary>
        /// 舊：/OnlineStore/Store/Rankings -> 目前導回首頁
        /// </summary>
        [HttpGet("Rankings")]
        public IActionResult RankingsLegacy()
            => RedirectToAction("Index");

        /// <summary>
        /// 我的收藏頁（需登入）
        /// </summary>
        [HttpGet("Favorites")]
        public IActionResult Favorites()
        {
            if (!_currentUser.IsAuthenticated)
            {
                return Redirect("/Login/Login/Login");
            }
            return View();
        }
    }
}