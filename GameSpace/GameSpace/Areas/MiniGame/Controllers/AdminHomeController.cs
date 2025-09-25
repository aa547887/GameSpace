using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(Policy = "CanManageShopping")] // Requires Shopping permission
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
            // 獲取各模組的摘要資料
            var walletSummary = await _adminService.GetWalletSummaryAsync();
            var petSummary = await _adminService.GetPetSummaryAsync();
            var gameSummary = await _adminService.GetGameSummaryAsync();
            
            // 創建首頁視圖模型
            var viewModel = new
            {
                WalletSummary = walletSummary,
                PetSummary = petSummary,
                GameSummary = gameSummary
            };

            return View(viewModel);
        }
    }
}
