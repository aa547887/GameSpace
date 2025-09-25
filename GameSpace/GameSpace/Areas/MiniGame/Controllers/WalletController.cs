using Microsoft.AspNetCore.Mvc;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class WalletController : Controller
    {
        public IActionResult Index()
        {
            return NotFound();
        }

        public IActionResult History()
        {
            return NotFound();
        }
    }
}
