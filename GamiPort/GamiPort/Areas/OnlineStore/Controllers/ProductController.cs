using GameSpace.Areas.OnlineStore.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    public class ProductController : Controller
    {
        // 頁面用前端打 API 取資料，這裡只回 View
        [HttpGet("/OnlineStore/Product/Detail/{id:int}")]
        public IActionResult Detail(int id)
        {
            ViewData["ProductId"] = id;
            return View();
        }
    }
}
