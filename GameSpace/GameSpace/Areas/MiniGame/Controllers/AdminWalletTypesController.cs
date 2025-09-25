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
        private readonly IMiniGameAdminService _adminService;

        public AdminWalletTypesController(GameSpace.Models.GameSpacedatabaseContext context, IMiniGameAdminService adminService) : base(context)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var walletTypes = await _adminService.GetWalletTypesAsync();
                var viewModel = new AdminWalletTypesViewModel
                {
                    WalletTypes = walletTypes
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入錢包類型時發生錯誤：{ex.Message}";
                return View(new AdminWalletTypesViewModel());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new WalletTypeCreateModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WalletTypeCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _adminService.CreateWalletTypeAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "錢包類型創建成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "錢包類型創建失敗";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"錢包類型創建失敗：{ex.Message}";
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var walletType = await _adminService.GetWalletTypeForEditAsync(id);
                if (walletType == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的錢包類型";
                    return RedirectToAction(nameof(Index));
                }

                return View(walletType);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入錢包類型時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(WalletTypeUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _adminService.UpdateWalletTypeAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "錢包類型更新成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "錢包類型更新失敗";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"錢包類型更新失敗：{ex.Message}";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _adminService.DeleteWalletTypeAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "錢包類型刪除成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "錢包類型刪除失敗";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"錢包類型刪除失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    public class AdminWalletTypesViewModel
    {
        public List<WalletTypeReadModel> WalletTypes { get; set; } = new();
        public string Sidebar { get; set; } = "admin";
    }
}
