using Microsoft.AspNetCore.Mvc;

namespace Gamiports.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    // 這樣就能以 /OnlineStore/Member/Favorites 存取
    [Route("OnlineStore/[controller]/[action]")]
    public class MemberController : Controller
    {
        [HttpGet]
        public IActionResult Favorites()
        {
            // 會自動找 Areas/OnlineStore/Views/Member/Favorites.cshtml
            return View();
        }
    }
}
