using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public class AdminMiniGameController : Controller
    {
        private readonly IMiniGameAdminService _adminService;
        private readonly IMiniGameAdminAuthService _authService;

        public AdminMiniGameController(IMiniGameAdminService adminService, IMiniGameAdminAuthService authService)
        {
            _adminService = adminService;
            _authService = authService;
        }

        // 遊戲規則設定
        [HttpGet]
        public async Task<IActionResult> GameRules()
        {
            try
            {
                var settings = await _adminService.GetMiniGameRulesAsync();
                return View(settings);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入遊戲規則設定時發生錯誤：{ex.Message}";
                return View(new MiniGameRulesViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> GameRules(MiniGameRulesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _adminService.UpdateMiniGameRulesAsync(model);
                TempData["SuccessMessage"] = "遊戲規則設定已成功更新！";
                return RedirectToAction(nameof(GameRules));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新遊戲規則設定時發生錯誤：{ex.Message}");
                return View(model);
            }
        }

        // 查看會員遊戲紀錄
        [HttpGet]
        public async Task<IActionResult> ViewGameRecords(MiniGameRecordQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            try
            {
                var result = await _adminService.QueryMiniGameRecordsAsync(query);
                var users = await _adminService.GetUsersAsync();

                var viewModel = new AdminMiniGameRecordsViewModel
                {
                    GameRecords = result.Items,
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
                TempData["ErrorMessage"] = $"查詢遊戲紀錄時發生錯誤：{ex.Message}";
                return View(new AdminMiniGameRecordsViewModel
                {
                    Query = query,
                    Users = await _adminService.GetUsersAsync()
                });
            }
        }

        // 遊戲統計分析
        [HttpGet]
        public async Task<IActionResult> GameStatistics(GameStatisticsQueryModel query)
        {
            try
            {
                var statistics = await _adminService.GetMiniGameStatisticsAsync(query);
                return View(statistics);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入遊戲統計資料時發生錯誤：{ex.Message}";
                return View(new GameStatisticsViewModel());
            }
        }

        // 保持舊有方法名稱以向後兼容
        public async Task<IActionResult> Rules()
        {
            return await GameRules();
        }

        public async Task<IActionResult> Records(MiniGameRecordQueryModel query)
        {
            return await ViewGameRecords(query);
        }
    }
}
