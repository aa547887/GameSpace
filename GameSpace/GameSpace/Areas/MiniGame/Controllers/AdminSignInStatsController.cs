using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(Policy = "CanUserStatus")] // Requires UserStatus permission
    public class AdminSignInStatsController : Controller
    {
        private readonly IMiniGameAdminService _adminService;
        private readonly IMiniGameAdminAuthService _authService;

        public AdminSignInStatsController(IMiniGameAdminService adminService, IMiniGameAdminAuthService authService)
        {
            _adminService = adminService;
            _authService = authService;
        }

        public async Task<IActionResult> Index(SignInQueryModel query)
        {
            var stats = await _adminService.GetSignInStatsAsync();
            return View(stats);
        }

        public async Task<IActionResult> SetRule()
        {
            var rule = await _adminService.GetSignInRuleAsync();
            return View(rule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetRule(SignInRuleUpdateModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _adminService.UpdateSignInRuleAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "簽到規則更新成功";
                    return RedirectToAction("SetRule");
                }
                else
                {
                    TempData["ErrorMessage"] = "簽到規則更新失敗";
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRecord(int userId, DateTime signInDate)
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
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRecord(int userId, DateTime signInDate)
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
            return RedirectToAction("Index");
        }
    }
}
