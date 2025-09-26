using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 優化的 MiniGame Admin 控制器
    /// 確保所有 Admin 後台功能完整實作
    /// </summary>
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public class OptimizedAdminController : MiniGameBaseController
    {
        private readonly OptimizedMiniGameAdminService _adminService;

        public OptimizedAdminController(GameSpacedatabaseContext context, OptimizedMiniGameAdminService adminService) : base(context)
        {
            _adminService = adminService;
        }

        #region 會員錢包系統

        /// <summary>
        /// 1. 查詢會員點數
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> QueryUserPoints(UserPointsQueryModel query)
        {
            try
            {
                var result = await _adminService.QueryUserPointsAsync(query);
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

                var viewModel = new AdminUserPointsViewModel
                {
                    UserWallets = result.Items,
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
                TempData["ErrorMessage"] = $"查詢會員點數時發生錯誤：{ex.Message}";
                return View(new AdminUserPointsViewModel());
            }
        }

        /// <summary>
        /// 2. 查詢會員擁有商城優惠券
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> QueryUserCoupons(CouponQueryModel query)
        {
            try
            {
                var result = await _adminService.QueryUserCouponsAsync(query);
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                var couponTypes = await _context.CouponTypes
                    .Select(ct => new { ct.Id, ct.Name })
                    .ToListAsync();

                var viewModel = new AdminUserCouponsViewModel
                {
                    UserCoupons = result.Items,
                    Users = users,
                    CouponTypes = couponTypes,
                    Query = query,
                    TotalCount = result.TotalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢會員優惠券時發生錯誤：{ex.Message}";
                return View(new AdminUserCouponsViewModel());
            }
        }

        /// <summary>
        /// 3. 查詢會員擁有電子禮券
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> QueryUserEVouchers(EVoucherQueryModel query)
        {
            try
            {
                var result = await _adminService.QueryUserEVouchersAsync(query);
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                var eVoucherTypes = await _context.EvoucherTypes
                    .Select(et => new { et.Id, et.Name })
                    .ToListAsync();

                var viewModel = new AdminUserEVouchersViewModel
                {
                    UserEVouchers = result.Items,
                    Users = users,
                    EVoucherTypes = eVoucherTypes,
                    Query = query,
                    TotalCount = result.TotalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢會員電子禮券時發生錯誤：{ex.Message}";
                return View(new AdminUserEVouchersViewModel());
            }
        }

        /// <summary>
        /// 4. 發放會員點數
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GrantPoints()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

                var viewModel = new GrantPointsModel
                {
                    Users = users
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入發放點數頁面時發生錯誤：{ex.Message}";
                return View(new GrantPointsModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> GrantPoints(GrantPointsModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                return View(model);
            }

            try
            {
                var result = await _adminService.GrantUserPointsAsync(model);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(GrantPoints));
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    model.Users = await _context.Users
                        .Select(u => new { u.Id, u.UserName, u.Email })
                        .ToListAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"發放點數失敗：{ex.Message}");
                model.Users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                return View(model);
            }
        }

        /// <summary>
        /// 5. 發放會員擁有商城優惠券
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GrantCoupons()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                var couponTypes = await _context.CouponTypes
                    .Where(ct => ct.IsActive)
                    .Select(ct => new { ct.Id, ct.Name, ct.Description })
                    .ToListAsync();

                var viewModel = new GrantCouponsModel
                {
                    Users = users,
                    CouponTypes = couponTypes
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入發放優惠券頁面時發生錯誤：{ex.Message}";
                return View(new GrantCouponsModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> GrantCoupons(GrantCouponsModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                model.CouponTypes = await _context.CouponTypes
                    .Where(ct => ct.IsActive)
                    .Select(ct => new { ct.Id, ct.Name, ct.Description })
                    .ToListAsync();
                return View(model);
            }

            try
            {
                var result = await _adminService.GrantUserCouponsAsync(model);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(GrantCoupons));
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    model.Users = await _context.Users
                        .Select(u => new { u.Id, u.UserName, u.Email })
                        .ToListAsync();
                    model.CouponTypes = await _context.CouponTypes
                        .Where(ct => ct.IsActive)
                        .Select(ct => new { ct.Id, ct.Name, ct.Description })
                        .ToListAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"發放優惠券失敗：{ex.Message}");
                model.Users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                model.CouponTypes = await _context.CouponTypes
                    .Where(ct => ct.IsActive)
                    .Select(ct => new { ct.Id, ct.Name, ct.Description })
                    .ToListAsync();
                return View(model);
            }
        }

        /// <summary>
        /// 6. 調整會員擁有電子禮券（發放）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GrantEVouchers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                var eVoucherTypes = await _context.EvoucherTypes
                    .Where(et => et.IsActive)
                    .Select(et => new { et.Id, et.Name, et.Description })
                    .ToListAsync();

                var viewModel = new GrantEVouchersModel
                {
                    Users = users,
                    EVoucherTypes = eVoucherTypes
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入發放電子禮券頁面時發生錯誤：{ex.Message}";
                return View(new GrantEVouchersModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> GrantEVouchers(GrantEVouchersModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                model.EVoucherTypes = await _context.EvoucherTypes
                    .Where(et => et.IsActive)
                    .Select(et => new { et.Id, et.Name, et.Description })
                    .ToListAsync();
                return View(model);
            }

            try
            {
                var result = await _adminService.GrantUserEVouchersAsync(model);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(GrantEVouchers));
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    model.Users = await _context.Users
                        .Select(u => new { u.Id, u.UserName, u.Email })
                        .ToListAsync();
                    model.EVoucherTypes = await _context.EvoucherTypes
                        .Where(et => et.IsActive)
                        .Select(et => new { et.Id, et.Name, et.Description })
                        .ToListAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"發放電子禮券失敗：{ex.Message}");
                model.Users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                model.EVoucherTypes = await _context.EvoucherTypes
                    .Where(et => et.IsActive)
                    .Select(et => new { et.Id, et.Name, et.Description })
                    .ToListAsync();
                return View(model);
            }
        }

        /// <summary>
        /// 7. 查看會員收支明細
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> QueryWalletHistory(WalletHistoryQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 20;

            try
            {
                var result = await _adminService.QueryWalletHistoryAsync(query);
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

                var viewModel = new AdminWalletHistoryViewModel
                {
                    WalletHistory = result.Items,
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
                TempData["ErrorMessage"] = $"查詢收支明細時發生錯誤：{ex.Message}";
                return View(new AdminWalletHistoryViewModel());
            }
        }

        #endregion

        #region 會員簽到系統

        /// <summary>
        /// 1. 簽到規則設定
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SignInRules()
        {
            try
            {
                var rules = await _adminService.GetSignInRulesAsync();
                var viewModel = new SignInRulesViewModel
                {
                    Rules = rules
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入簽到規則時發生錯誤：{ex.Message}";
                return View(new SignInRulesViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SignInRules(SignInRulesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Rules = await _adminService.GetSignInRulesAsync();
                return View(model);
            }

            try
            {
                var result = await _adminService.UpdateSignInRulesAsync(model.Rules);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(SignInRules));
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    model.Rules = await _adminService.GetSignInRulesAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新失敗：{ex.Message}");
                model.Rules = await _adminService.GetSignInRulesAsync();
                return View(model);
            }
        }

        /// <summary>
        /// 2. 查看會員簽到紀錄
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SignInRecords(SignInRecordQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 20;

            try
            {
                var result = await _adminService.QuerySignInRecordsAsync(query);
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

                var viewModel = new AdminSignInRecordsViewModel
                {
                    SignInRecords = result.Items,
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
                return View(new AdminSignInRecordsViewModel());
            }
        }

        /// <summary>
        /// 獲取簽到統計資料
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSignInStats(string period = "today")
        {
            try
            {
                var stats = await _adminService.GetSignInStatisticsAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region 寵物系統

        /// <summary>
        /// 1. 整體寵物系統規則設定
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PetSystemRules()
        {
            try
            {
                var rules = await _adminService.GetPetSystemRulesAsync();
                return View(rules);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入寵物系統規則時發生錯誤：{ex.Message}";
                return View(new PetSystemRulesViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> PetSystemRules(PetSystemRulesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await _adminService.GetPetSystemRulesAsync();
                return View(model);
            }

            try
            {
                var result = await _adminService.UpdatePetSystemRulesAsync(model);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(PetSystemRules));
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    model = await _adminService.GetPetSystemRulesAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新失敗：{ex.Message}");
                model = await _adminService.GetPetSystemRulesAsync();
                return View(model);
            }
        }

        /// <summary>
        /// 2. 會員個別寵物設定
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PetSettings(int? userId = null)
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

                var petSettings = new List<PetSettingModel>();

                if (userId.HasValue)
                {
                    petSettings = await _adminService.GetUserPetSettingsAsync(userId.Value);
                }

                var viewModel = new PetSettingsViewModel
                {
                    Users = users,
                    PetSettings = petSettings,
                    SelectedUserId = userId
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入寵物設定時發生錯誤：{ex.Message}";
                return View(new PetSettingsViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> PetSettings(PetSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                return View(model);
            }

            try
            {
                var result = await _adminService.UpdatePetSettingAsync(model.EditPet);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(PetSettings), new { userId = model.EditPet.UserId });
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    model.Users = await _context.Users
                        .Select(u => new { u.Id, u.UserName, u.Email })
                        .ToListAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新寵物設定失敗：{ex.Message}");
                model.Users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                return View(model);
            }
        }

        /// <summary>
        /// 3. 會員個別寵物清單含查詢
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PetList(PetListQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 20;

            try
            {
                var result = await _adminService.QueryPetListAsync(query);
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

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
                return View(new AdminPetListViewModel());
            }
        }

        #endregion

        #region 小遊戲系統

        /// <summary>
        /// 1. 遊戲規則設定
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GameRules()
        {
            try
            {
                var rules = await _adminService.GetGameRulesAsync();
                var viewModel = new GameRulesViewModel
                {
                    Rules = rules
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入遊戲規則時發生錯誤：{ex.Message}";
                return View(new GameRulesViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> GameRules(GameRulesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Rules = await _adminService.GetGameRulesAsync();
                return View(model);
            }

            try
            {
                var result = await _adminService.UpdateGameRulesAsync(model.Rules);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(GameRules));
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    model.Rules = await _adminService.GetGameRulesAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新失敗：{ex.Message}");
                model.Rules = await _adminService.GetGameRulesAsync();
                return View(model);
            }
        }

        /// <summary>
        /// 2. 查看會員遊戲紀錄
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GameRecords(GameRecordQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 20;

            try
            {
                var result = await _adminService.QueryGameRecordsAsync(query);
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

                var viewModel = new AdminGameRecordsViewModel
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
                return View(new AdminGameRecordsViewModel());
            }
        }

        /// <summary>
        /// 獲取遊戲統計資料
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetGameStats(string period = "today")
        {
            try
            {
                var stats = await _adminService.GetGameStatisticsAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region API 方法

        /// <summary>
        /// 獲取用戶詳細信息
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserDetails(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Email,
                        u.CreateTime,
                        Wallet = _context.UserWallets
                            .Where(w => w.UserId == userId)
                            .Select(w => new { w.UserPoint, w.LastUpdateTime })
                            .FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return Json(new { success = false, message = "找不到指定的用戶" });
                }

                return Json(new { success = true, data = user });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 快速查詢用戶點數
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> QuickQueryPoints(string searchTerm)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.UserName.Contains(searchTerm) || u.Email.Contains(searchTerm))
                    .Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Email,
                        Points = _context.UserWallets
                            .Where(w => w.UserId == u.Id)
                            .Select(w => w.UserPoint)
                            .FirstOrDefault()
                    })
                    .Take(10)
                    .ToListAsync();

                return Json(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 獲取統計數據
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var today = DateTime.Today;
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                var stats = new StatisticsOverviewModel
                {
                    TotalUsers = await _context.Users.CountAsync(),
                    TotalPets = await _context.Pets.CountAsync(),
                    TotalGames = await _context.MiniGames.CountAsync(),
                    TotalSignIns = await _context.UserSignInStats.CountAsync(),
                    TotalPointsInCirculation = await _context.UserWallets.SumAsync(w => w.UserPoint),
                    TotalCouponsIssued = await _context.Coupons.CountAsync(),
                    TotalEVouchersIssued = await _context.Evouchers.CountAsync(),
                    LastUpdated = DateTime.Now
                };

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion
    }
}
