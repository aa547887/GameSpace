using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(Policy = "CanManageShopping")] // Requires Shopping permission
    public class AdminMiniGameController : Controller
    {
        private readonly IMiniGameAdminService _adminService;
        private readonly IMiniGameAdminAuthService _authService;

        public AdminMiniGameController(IMiniGameAdminService adminService, IMiniGameAdminAuthService authService)
        {
            _adminService = adminService;
            _authService = authService;
        }

        public async Task<IActionResult> Index(GameQueryModel query)
        {
            var model = new AdminMiniGameIndexViewModel
            {
                GameRecords = await _adminService.GetGameRecordsAsync(query),
                GameSummary = await _adminService.GetGameSummaryAsync(),
                Query = query,
                Sidebar = new SidebarViewModel()
            };
            return View(model);
        }

        public async Task<IActionResult> Rules()
        {
            var model = new AdminMiniGameRulesViewModel
            {
                GameRule = await _adminService.GetGameRuleAsync(),
                Sidebar = new SidebarViewModel()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRules(GameRuleUpdateModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _adminService.UpdateGameRuleAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "遊戲規則更新成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "遊戲規則更新失敗";
                }
            }
            return RedirectToAction("Rules");
        }

        public async Task<IActionResult> Details(int gameId)
        {
            var model = new AdminMiniGameDetailsViewModel
            {
                Game = await _adminService.GetGameDetailAsync(gameId),
                Sidebar = new SidebarViewModel()
            };
            return View(model);
        }
    }
}
