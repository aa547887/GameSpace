//// Areas/OnlineStore/Controllers/BrowseController.cs
//using Microsoft.AspNetCore.Mvc;

//namespace GamiPort.Areas.OnlineStore.Controllers
//{
//    [Area("OnlineStore")]
//    [Route("OnlineStore/[controller]")]
//    public class BrowseController : Controller
//    {
//        [HttpGet("")]
//        [HttpGet("Index")]
//        public IActionResult Index()
//            => RedirectToAction("Browse", "Store", new { area = "OnlineStore" });
//    }
//}
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    public class BrowseController : Controller
    {
        public IActionResult Index() => View(); // 對應上面的 Views/Browse/Index.cshtml
    }
}

