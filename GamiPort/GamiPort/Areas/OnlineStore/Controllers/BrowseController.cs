using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    [Route("OnlineStore/[controller]")]
    public class BrowseController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index() => View(); // Maps to Views/Browse/Index.cshtml
    }
}
