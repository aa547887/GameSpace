using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]
    public class AdminWalletController : Controller
    {
        private readonly IMiniGameAdminService _adminService;

        public AdminWalletController(IMiniGameAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Index(CouponQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            var userPoints = await _adminService.QueryUserPointsAsync(query);
            var users = await _adminService.GetUsersAsync();

            var viewModel = new AdminWalletIndexViewModel
            {
                UserPoints = userPoints.Items,
                Users = users,
                SearchTerm = query.SearchTerm,
                Page = userPoints.Page,
                PageSize = userPoints.PageSize,
                TotalCount = userPoints.TotalCount,
                TotalPages = userPoints.TotalPages
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AdjustPoints(int userId, int points, string reason)
        {
            var result = await _adminService.AdjustUserPointsAsync(userId, points, reason);
            if (result)
            {
                TempData["SuccessMessage"] = "點數調整成功";
            }
            else
            {
                TempData["ErrorMessage"] = "點數調整失敗";
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AdjustCoupons()
        {
            var couponTypes = await _adminService.GetCouponTypesAsync();
            var users = await _adminService.GetUsersAsync();

            var viewModel = new AdminAdjustCouponsViewModel
            {
                CouponTypes = couponTypes,
                Users = users
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AdjustCoupons(AdminAdjustCouponsViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool result;
                if (model.Action == "add")
                {
                    result = await _adminService.IssueCouponToUserAsync(model.UserId, model.CouponTypeId, model.Quantity);
                }
                else
                {
                    result = await _adminService.RemoveCouponFromUserAsync(model.UserId, model.CouponTypeId);
                }

                if (result)
                {
                    TempData["SuccessMessage"] = "優惠券調整成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "優惠券調整失敗";
                }
            }
            return RedirectToAction("AdjustCoupons");
        }

        public async Task<IActionResult> AdjustEVouchers()
        {
            var evoucherTypes = await _adminService.GetEVoucherTypesAsync();
            var users = await _adminService.GetUsersAsync();

            var viewModel = new AdminAdjustEVouchersViewModel
            {
                EVoucherTypes = evoucherTypes,
                Users = users
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AdjustEVouchers(AdminAdjustEVouchersViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool result;
                if (model.Action == "add")
                {
                    result = await _adminService.IssueEVoucherToUserAsync(model.UserId, model.EVoucherTypeId, model.Quantity);
                }
                else
                {
                    result = await _adminService.RemoveEVoucherFromUserAsync(model.UserId, model.EVoucherTypeId);
                }

                if (result)
                {
                    TempData["SuccessMessage"] = "電子優惠券調整成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "電子優惠券調整失敗";
                }
            }
            return RedirectToAction("AdjustEVouchers");
        }

        public async Task<IActionResult> TransactionHistory(CouponQueryModel query)
        {
            var transactions = await _adminService.QueryWalletTransactionsAsync(query);
            return View(transactions);
        }
    }
}
