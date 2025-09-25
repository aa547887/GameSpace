using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public class AdminHomeController : Controller
    {
        private readonly IMiniGameAdminService _adminService;
        private readonly IMiniGameAdminAuthService _authService;

        public AdminHomeController(IMiniGameAdminService adminService, IMiniGameAdminAuthService authService)
        {
            _adminService = adminService;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // 獲取各模組的摘要資料
                var walletSummary = await _adminService.GetWalletSummaryAsync();
                var petSummary = await _adminService.GetPetSummaryAsync();
                var gameSummary = await _adminService.GetGameSummaryAsync();
                var signInSummary = await _adminService.GetSignInSummaryAsync();
                
                // 創建首頁視圖模型
                var viewModel = new AdminDashboardViewModel
                {
                    WalletSummary = walletSummary,
                    PetSummary = petSummary,
                    GameSummary = gameSummary,
                    SignInSummary = signInSummary
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入儀表板資料時發生錯誤：{ex.Message}";
                return View(new AdminDashboardViewModel());
            }
        }

        public async Task<IActionResult> Dashboard()
        {
            return await Index();
        }
    }

    // ViewModels
    public class AdminDashboardViewModel
    {
        public WalletSummaryModel WalletSummary { get; set; } = new();
        public PetSummaryModel PetSummary { get; set; } = new();
        public GameSummaryModel GameSummary { get; set; } = new();
        public SignInSummaryModel SignInSummary { get; set; } = new();
    }

    public class WalletSummaryModel
    {
        public int TotalUsers { get; set; }
        public int TotalPoints { get; set; }
        public int ActiveUsers { get; set; }
        public int CouponCount { get; set; }
        public int EVoucherCount { get; set; }
    }

    public class PetSummaryModel
    {
        public int TotalPets { get; set; }
        public int ActivePets { get; set; }
        public int AverageLevel { get; set; }
        public int ColorChangesToday { get; set; }
    }

    public class GameSummaryModel
    {
        public int TotalGames { get; set; }
        public int GamesToday { get; set; }
        public int WinRate { get; set; }
        public int AverageScore { get; set; }
    }

    public class SignInSummaryModel
    {
        public int TotalSignIns { get; set; }
        public int SignInsToday { get; set; }
        public int ActiveUsers { get; set; }
        public int ConsecutiveSignIns { get; set; }
    }
}
