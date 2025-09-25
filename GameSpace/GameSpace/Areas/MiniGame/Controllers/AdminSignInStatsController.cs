using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public class AdminSignInStatsController : Controller
    {
        private readonly IMiniGameAdminService _adminService;
        private readonly IMiniGameAdminAuthService _authService;

        public AdminSignInStatsController(IMiniGameAdminService adminService, IMiniGameAdminAuthService authService)
        {
            _adminService = adminService;
            _authService = authService;
        }

        // 簽到規則設定
        [HttpGet]
        public async Task<IActionResult> RuleSettings()
        {
            try
            {
                var settings = await _adminService.GetSignInRuleSettingsAsync();
                return View(settings);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入簽到規則設定時發生錯誤：{ex.Message}";
                return View(new SignInRuleSettingsViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> RuleSettings(SignInRuleSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _adminService.UpdateSignInRuleSettingsAsync(model);
                TempData["SuccessMessage"] = "簽到規則設定已成功更新！";
                return RedirectToAction(nameof(RuleSettings));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新簽到規則設定時發生錯誤：{ex.Message}");
                return View(model);
            }
        }

        // 查看會員簽到紀錄
        [HttpGet]
        public async Task<IActionResult> ViewRecords(SignInStatsQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            try
            {
                var result = await _adminService.QuerySignInStatsAsync(query);
                var users = await _adminService.GetUsersAsync();

                var viewModel = new AdminSignInStatsViewModel
                {
                    SignInStats = result.Items,
                    Users = users,
                    Query = query,
                    TotalCount = result.TotalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢簽到紀錄時發生錯誤：{ex.Message}";
                return View(new AdminSignInStatsViewModel
                {
                    Query = query,
                    Users = await _adminService.GetUsersAsync()
                });
            }
        }

        // 查詢簽到紀錄
        [HttpGet]
        public async Task<IActionResult> QueryRecords(SignInStatsQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            try
            {
                var result = await _adminService.QuerySignInStatsAsync(query);
                var users = await _adminService.GetUsersAsync();

                var viewModel = new AdminSignInStatsViewModel
                {
                    SignInStats = result.Items,
                    Users = users,
                    Query = query,
                    TotalCount = result.TotalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢簽到紀錄時發生錯誤：{ex.Message}";
                return View(new AdminSignInStatsViewModel
                {
                    Query = query,
                    Users = await _adminService.GetUsersAsync()
                });
            }
        }

        // 保持舊有方法名稱以向後兼容
        public async Task<IActionResult> Rules()
        {
            return await RuleSettings();
        }

        public async Task<IActionResult> Records(SignInStatsQueryModel query)
        {
            return await ViewRecords(query);
        }

        public async Task<IActionResult> Adjust()
        {
            // 重導向到查看紀錄頁面，因為調整功能整合在查看頁面中
            return RedirectToAction(nameof(ViewRecords));
        }
    }
}
