using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
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
            try
            {
                var signInStats = await _adminService.GetSignInStatsAsync();
                var users = await _adminService.GetUsersAsync();

                var model = new AdminSignInIndexViewModel
                {
                    SignInStats = signInStats.Items,
                    Users = users,
                    TotalCount = signInStats.TotalCount,
                    PageNumber = signInStats.PageNumber,
                    PageSize = signInStats.PageSize
                };
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入簽到統計時發生錯誤：{ex.Message}";
                return View(new AdminSignInIndexViewModel());
            }
        }

        public async Task<IActionResult> Rules()
        {
            try
            {
                var signInRule = await _adminService.GetSignInRuleAsync();
                var model = new AdminSignInRulesViewModel
                {
                    SignInRule = signInRule
                };
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入簽到規則時發生錯誤：{ex.Message}";
                return View(new AdminSignInRulesViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRules(SignInRuleUpdateModel model)
        {
            if (ModelState.IsValid)
            {
                try
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
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"更新簽到規則時發生錯誤：{ex.Message}";
                }
            }
            return RedirectToAction("Rules");
        }

        public async Task<IActionResult> UserHistory(int userId)
        {
            try
            {
                var user = await _adminService.GetUserByIdAsync(userId);
                var signInHistory = await _adminService.GetUserSignInHistoryAsync(userId);

                var model = new AdminSignInUserHistoryViewModel
                {
                    UserId = userId,
                    UserName = user?.UserName ?? "未知用戶",
                    SignInHistory = signInHistory
                };
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入用戶簽到歷史時發生錯誤：{ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSignInRecord(int userId, DateTime signInDate)
        {
            try
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
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"新增簽到記錄時發生錯誤：{ex.Message}";
            }
            return RedirectToAction("UserHistory", new { userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSignInRecord(int userId, DateTime signInDate)
        {
            try
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
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"移除簽到記錄時發生錯誤：{ex.Message}";
            }
            return RedirectToAction("UserHistory", new { userId });
        }
    }
}
