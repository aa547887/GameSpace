using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public class AdminController : MiniGameBaseController
    {
        private readonly IMiniGameAdminService _adminService;

        public AdminController(IMiniGameAdminService adminService)
        {
            _adminService = adminService;
        }

        // MiniGame Admin 首頁儀表板
        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = await _adminService.GetDashboardDataAsync();
                var model = new AdminDashboardViewModel
                {
                    TotalUsers = dashboardData.TotalUsers,
                    TotalPets = dashboardData.TotalPets,
                    TotalPoints = dashboardData.TotalPoints,
                    TotalCoupons = dashboardData.TotalCoupons,
                    TotalEVouchers = dashboardData.TotalEVouchers,
                    RecentSignIns = dashboardData.RecentSignIns,
                    RecentGameRecords = dashboardData.RecentGameRecords,
                    RecentWalletTransactions = dashboardData.RecentWalletTransactions,
                    SystemStats = dashboardData.SystemStats
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入儀表板時發生錯誤：{ex.Message}";
                return View(new AdminDashboardViewModel());
            }
        }

        // 系統統計資料 API
        [HttpGet]
        public async Task<IActionResult> GetSystemStats()
        {
            try
            {
                var stats = await _adminService.GetSystemStatsAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 快速查詢用戶
        [HttpGet]
        public async Task<IActionResult> QuickSearchUser(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return Json(new { success = true, users = new List<object>() });

                var users = await _adminService.SearchUsersAsync(query);
                var result = users.Select(u => new
                {
                    id = u.UserId,
                    name = u.UserName,
                    email = u.Email,
                    points = u.Points
                }).ToList();

                return Json(new { success = true, users = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
