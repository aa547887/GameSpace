using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    [Route("OnlineStore/store")]
    public class StoreController : Controller
    {
        // GET: /OnlineStore/store
        [HttpGet("")]
        public IActionResult Index() => View("~/Areas/OnlineStore/Views/Store/Index.cshtml");

        // GET: /OnlineStore/store/browse
        [HttpGet("browse")]
        public IActionResult Browse() => View("~/Areas/OnlineStore/Views/Store/Browse.cshtml");

        // GET: /OnlineStore/store/product/{code}
        [HttpGet("product/{code}")]
        public IActionResult Product(string code)
        {
            ViewBag.ProductCode = code; // 前端用 code 去打 API
            return View("~/Areas/OnlineStore/Views/Store/Product.cshtml");
        }

        // GET: /OnlineStore/store/rankings
        [HttpGet("rankings")]
        public IActionResult Rankings() => View("~/Areas/OnlineStore/Views/Store/Rankings.cshtml");
    }
}
