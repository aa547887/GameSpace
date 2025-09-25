using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(Policy = "CanManageShopping")] // Requires Shopping permission
    public class AdminSignInController : Controller
    {
        private readonly IMiniGameAdminService _adminService;
        private readonly IMiniGameAdminAuthService _authService;

        public AdminSignInController(IMiniGameAdminService adminService, IMiniGameAdminAuthService authService)
        {
            _adminService = adminService;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminSignInIndexViewModel
            {
                SignInStats = await _adminService.GetSignInStatsAsync(),
                Sidebar = new SidebarViewModel()
            };
            return View(model);
        }

        public async Task<IActionResult> Rules()
        {
            var model = new AdminSignInRulesViewModel
            {
                SignInRule = await _adminService.GetSignInRuleAsync(),
                Sidebar = new SidebarViewModel()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRules(SignInRuleUpdateModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _adminService.UpdateSignInRuleAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "簽到規則更新成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "簽到規則更新失敗";
                }
            }
            return RedirectToAction("Rules");
        }

        public async Task<IActionResult> UserHistory(int userId)
        {
            var model = new AdminSignInUserHistoryViewModel
            {
                UserId = userId,
                UserName = _adminService.GetUserByIdAsync(userId).Result?.UserName ?? "",
                SignInHistory = await _adminService.GetUserSignInHistoryAsync(userId),
                Sidebar = new SidebarViewModel()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSignInRecord(int userId, DateTime signInDate)
        {
            var success = await _adminService.AddUserSignInRecordAsync(userId, signInDate);
            if (success)
            {
                TempData["SuccessMessage"] = "簽到記錄新增成功";
            }
            else
            {
                TempData["ErrorMessage"] = "簽到記錄新增失敗";
            }
            return RedirectToAction("UserHistory", new { userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSignInRecord(int userId, DateTime signInDate)
        {
            var success = await _adminService.RemoveUserSignInRecordAsync(userId, signInDate);
            if (success)
            {
                TempData["SuccessMessage"] = "簽到記錄移除成功";
            }
            else
            {
                TempData["ErrorMessage"] = "簽到記錄移除失敗";
            }
            return RedirectToAction("UserHistory", new { userId });
        }
    }
}
