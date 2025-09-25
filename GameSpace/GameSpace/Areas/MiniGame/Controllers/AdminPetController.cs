using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public class AdminPetController : Controller
    {
        private readonly IMiniGameAdminService _adminService;
        private readonly IMiniGameAdminAuthService _authService;

        public AdminPetController(IMiniGameAdminService adminService, IMiniGameAdminAuthService authService)
        {
            _adminService = adminService;
            _authService = authService;
        }

        // 整體寵物系統規則設定
        [HttpGet]
        public async Task<IActionResult> SystemRules()
        {
            try
            {
                var settings = await _adminService.GetPetSystemRulesAsync();
                return View(settings);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入寵物系統規則時發生錯誤：{ex.Message}";
                return View(new PetSystemRulesViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SystemRules(PetSystemRulesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _adminService.UpdatePetSystemRulesAsync(model);
                TempData["SuccessMessage"] = "寵物系統規則已成功更新！";
                return RedirectToAction(nameof(SystemRules));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新寵物系統規則時發生錯誤：{ex.Message}");
                return View(model);
            }
        }

        // 會員個別寵物設定：手動調整基本資料
        [HttpGet]
        public async Task<IActionResult> IndividualSettings(int? userId = null)
        {
            try
            {
                var users = await _adminService.GetUsersAsync();
                var viewModel = new IndividualPetSettingsViewModel
                {
                    Users = users
                };

                if (userId.HasValue)
                {
                    var pet = await _adminService.GetUserPetAsync(userId.Value);
                    if (pet != null)
                    {
                        viewModel.SelectedUserId = userId.Value;
                        viewModel.Pet = pet;
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入寵物設定時發生錯誤：{ex.Message}";
                return View(new IndividualPetSettingsViewModel
                {
                    Users = await _adminService.GetUsersAsync()
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> IndividualSettings(IndividualPetSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Users = await _adminService.GetUsersAsync();
                if (model.SelectedUserId > 0)
                {
                    model.Pet = await _adminService.GetUserPetAsync(model.SelectedUserId);
                }
                return View(model);
            }

            try
            {
                await _adminService.UpdateUserPetAsync(model.SelectedUserId, model.Pet);
                TempData["SuccessMessage"] = "寵物基本資料已成功更新！";
                return RedirectToAction(nameof(IndividualSettings), new { userId = model.SelectedUserId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新寵物資料時發生錯誤：{ex.Message}");
                model.Users = await _adminService.GetUsersAsync();
                return View(model);
            }
        }

        // 會員個別寵物清單含查詢
        [HttpGet]
        public async Task<IActionResult> ListWithQuery(PetQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            try
            {
                var result = await _adminService.QueryUserPetsAsync(query);
                var users = await _adminService.GetUsersAsync();

                var viewModel = new AdminPetListViewModel
                {
                    Pets = result.Items,
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
                TempData["ErrorMessage"] = $"查詢寵物清單時發生錯誤：{ex.Message}";
                return View(new AdminPetListViewModel
                {
                    Query = query,
                    Users = await _adminService.GetUsersAsync()
                });
            }
        }

        // 換膚／換背景紀錄查詢
        [HttpGet]
        public async Task<IActionResult> ColorChangeHistory(PetColorChangeQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            try
            {
                var result = await _adminService.QueryPetColorChangeHistoryAsync(query);
                var users = await _adminService.GetUsersAsync();

                var viewModel = new PetColorChangeHistoryViewModel
                {
                    ColorChangeHistory = result.Items,
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
                TempData["ErrorMessage"] = $"查詢換膚換背景紀錄時發生錯誤：{ex.Message}";
                return View(new PetColorChangeHistoryViewModel
                {
                    Query = query,
                    Users = await _adminService.GetUsersAsync()
                });
            }
        }

        // 保持舊有方法名稱以向後兼容
        public async Task<IActionResult> Rules()
        {
            return await SystemRules();
        }

        public async Task<IActionResult> MemberPets()
        {
            return await IndividualSettings();
        }

        public async Task<IActionResult> PetDetails(PetQueryModel query)
        {
            return await ListWithQuery(query);
        }
    }
}
