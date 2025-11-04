using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    [Route("OnlineStore/[controller]")] // => /OnlineStore/Store/...
    public class StoreController : Controller
    {
        /// <summary>
        /// 商城首頁
        /// 支援：/OnlineStore、/OnlineStore/Store、/OnlineStore/Store/Index
        /// </summary>
        [HttpGet("")]
        [HttpGet("Index")]
        [HttpGet("~/OnlineStore")]                 // 直接以區路徑進來也到首頁
        public IActionResult Index()
        {
            // 將使用慣例路徑：Areas/OnlineStore/Views/Store/Index.cshtml
            return View();
        }

        // =========================
        // 兼容舊連結（Redirect）
        // =========================

        /// <summary>
        /// 舊：/OnlineStore/Store/Browse  -> 新：/OnlineStore/Browse/Index
        /// </summary>
        [HttpGet("Browse")]
        public IActionResult BrowseLegacy()
            => RedirectToAction("Index", "Browse", new { area = "OnlineStore" });

        /// <summary>
        /// 舊：/OnlineStore/Store/Product/{id} -> 新：/OnlineStore/Product/Detail/{id}
        /// （原本用 code 的話請改成 id；若一定要 code，告訴我我改成以 code 轉 id）
        /// </summary>
        [HttpGet("Product/{id:int}")]
        public IActionResult ProductLegacy(int id)
            => RedirectToAction("Detail", "Product", new { area = "OnlineStore", id });

        /// <summary>
        /// 舊：/OnlineStore/Store/Rankings -> 建議未來獨立 RankingsController/Index
        /// 目前先導回首頁，避免 404。若你已做 RankingsController，再把這段刪掉即可。
        /// </summary>
        [HttpGet("Rankings")]
        public IActionResult RankingsLegacy()
            => RedirectToAction("Index");
    }
}
