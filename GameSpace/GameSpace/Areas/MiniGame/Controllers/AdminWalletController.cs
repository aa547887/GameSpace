using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public class AdminWalletController : Controller
    {
        private readonly IMiniGameAdminService _adminService;

        public AdminWalletController(IMiniGameAdminService adminService)
        {
            _adminService = adminService;
        }

        // 查詢會員點數
        public async Task<IActionResult> QueryPoints(CouponQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            var userPoints = await _adminService.QueryUserPointsAsync(query);
            var users = await _adminService.GetUsersAsync();

            var viewModel = new AdminWalletIndexViewModel
            {
                UserPoints = userPoints.Items,
                Users = users,
                Query = query,
                TotalCount = userPoints.TotalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return View(viewModel);
        }

        // 查詢會員擁有商城優惠券
        public async Task<IActionResult> QueryCoupons(CouponQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            var result = await _adminService.QueryUserCouponsAsync(query);
            var users = await _adminService.GetUsersAsync();

            var viewModel = new AdminCouponIndexViewModel
            {
                Coupons = result.Items,
                Users = users,
                Query = query,
                TotalCount = result.TotalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return View(viewModel);
        }

        // 查詢會員擁有電子禮券
        public async Task<IActionResult> QueryEVouchers(EVoucherQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            var result = await _adminService.QueryUserEVouchersAsync(query);
            var users = await _adminService.GetUsersAsync();

            var viewModel = new AdminEVoucherIndexViewModel
            {
                EVouchers = result.Items,
                Users = users,
                Query = query,
                TotalCount = result.TotalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return View(viewModel);
        }

        // 發放會員點數
        [HttpGet]
        public async Task<IActionResult> GrantPoints()
        {
            var users = await _adminService.GetUsersAsync();
            var viewModel = new GrantPointsViewModel
            {
                Users = users
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GrantPoints(GrantPointsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Users = await _adminService.GetUsersAsync();
                return View(model);
            }

            try
            {
                await _adminService.GrantUserPointsAsync(model.UserId, model.Points, model.Reason);
                TempData["SuccessMessage"] = "會員點數發放成功！";
                return RedirectToAction(nameof(QueryPoints));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"發放失敗：{ex.Message}");
                model.Users = await _adminService.GetUsersAsync();
                return View(model);
            }
        }

        // 發放會員擁有商城優惠券（含發放）
        [HttpGet]
        public async Task<IActionResult> GrantCoupons()
        {
            var users = await _adminService.GetUsersAsync();
            var couponTypes = await _adminService.GetCouponTypesAsync();
            
            var viewModel = new GrantCouponsViewModel
            {
                Users = users,
                CouponTypes = couponTypes
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GrantCoupons(GrantCouponsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Users = await _adminService.GetUsersAsync();
                model.CouponTypes = await _adminService.GetCouponTypesAsync();
                return View(model);
            }

            try
            {
                await _adminService.GrantUserCouponsAsync(model.UserId, model.CouponTypeId, model.Quantity, model.Reason);
                TempData["SuccessMessage"] = "商城優惠券發放成功！";
                return RedirectToAction(nameof(QueryCoupons));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"發放失敗：{ex.Message}");
                model.Users = await _adminService.GetUsersAsync();
                model.CouponTypes = await _adminService.GetCouponTypesAsync();
                return View(model);
            }
        }

        // 調整會員擁有電子禮券（發放）
        [HttpGet]
        public async Task<IActionResult> AdjustEVouchers()
        {
            var users = await _adminService.GetUsersAsync();
            var eVoucherTypes = await _adminService.GetEVoucherTypesAsync();
            
            var viewModel = new AdjustEVouchersViewModel
            {
                Users = users,
                EVoucherTypes = eVoucherTypes
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AdjustEVouchers(AdjustEVouchersViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Users = await _adminService.GetUsersAsync();
                model.EVoucherTypes = await _adminService.GetEVoucherTypesAsync();
                return View(model);
            }

            try
            {
                await _adminService.AdjustUserEVouchersAsync(model.UserId, model.EVoucherTypeId, model.Quantity, model.Reason);
                TempData["SuccessMessage"] = "電子禮券調整成功！";
                return RedirectToAction(nameof(QueryEVouchers));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"調整失敗：{ex.Message}");
                model.Users = await _adminService.GetUsersAsync();
                model.EVoucherTypes = await _adminService.GetEVoucherTypesAsync();
                return View(model);
            }
        }

        // 查看會員收支明細
        public async Task<IActionResult> ViewHistory(WalletHistoryQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            var result = await _adminService.QueryWalletHistoryAsync(query);
            var users = await _adminService.GetUsersAsync();

            var viewModel = new AdminWalletHistoryViewModel
            {
                WalletHistories = result.Items,
                Users = users,
                Query = query,
                TotalCount = result.TotalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return View(viewModel);
        }

        // 保持舊有的Index方法以向後兼容
        public async Task<IActionResult> Index(CouponQueryModel query)
        {
            return await QueryPoints(query);
        }

        public async Task<IActionResult> AdjustPoints()
        {
            return await GrantPoints();
        }

        public async Task<IActionResult> History(WalletHistoryQueryModel query)
        {
            return await ViewHistory(query);
        }
    }
}
