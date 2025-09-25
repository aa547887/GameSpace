using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]
    public class AdminWalletTypesController : MiniGameBaseController
    {
        public AdminWalletTypesController(GameSpace.Models.GameSpacedatabaseContext context) : base(context)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
