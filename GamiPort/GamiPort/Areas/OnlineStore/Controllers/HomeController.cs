using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    public class HomeController : Controller
    {
        // 商城首頁（輪播、熱門排行、精選卡片等）
        public IActionResult Index() => View();
    }
}
