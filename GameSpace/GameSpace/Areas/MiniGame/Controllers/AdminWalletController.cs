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
            var userPoints = await _adminService.QueryUserPointsAsync(query);
            var users = await _adminService.GetUsersAsync();
            var walletSummary = await _adminService.GetWalletSummaryAsync();

            var viewModel = new AdminWalletIndexViewModel
            {
                Wallets = userPoints,
                Users = users,
                SearchTerm = query.SearchTerm,
                Page = userPoints.Page,
                PageSize = userPoints.PageSize,
                TotalCount = userPoints.TotalCount,
                TotalPages = userPoints.TotalPages,
                WalletSummary = walletSummary
            };

            return View(viewModel);
        }

        // 查詢會員點數
        public async Task<IActionResult> QueryPoints(CouponQueryModel query)
        {
            var userPoints = await _adminService.QueryUserPointsAsync(query);
            var users = await _adminService.GetUsersAsync();

            var viewModel = new AdminWalletIndexViewModel
            {
                Wallets = userPoints,
                Users = users,
                SearchTerm = query.SearchTerm,
                Page = userPoints.Page,
                PageSize = userPoints.PageSize,
                TotalCount = userPoints.TotalCount,
                TotalPages = userPoints.TotalPages
            };

            return View(viewModel);
        }

        // 查詢會員商城優惠券
        public async Task<IActionResult> QueryCoupons(CouponQueryModel query)
        {
            var userCoupons = await _adminService.QueryUserCouponsAsync(query);
            var users = await _adminService.GetUsersAsync();

            var viewModel = new AdminQueryCouponsViewModel
            {
                Coupons = userCoupons,
                Users = users,
                SearchTerm = query.SearchTerm,
                Page = userCoupons.Page,
                PageSize = userCoupons.PageSize,
                TotalCount = userCoupons.TotalCount,
                TotalPages = userCoupons.TotalPages
            };

            return View(viewModel);
        }

        // 查詢會員電子禮券
        public async Task<IActionResult> QueryEVouchers(CouponQueryModel query)
        {
            var userEVouchers = await _adminService.QueryUserEVouchersAsync(query);
            var users = await _adminService.GetUsersAsync();

            var viewModel = new AdminQueryEVouchersViewModel
            {
                EVouchers = userEVouchers,
                Users = users,
                SearchTerm = query.SearchTerm,
                Page = userEVouchers.Page,
                PageSize = userEVouchers.PageSize,
                TotalCount = userEVouchers.TotalCount,
                TotalPages = userEVouchers.TotalPages
            };

            return View(viewModel);
        }

        // 調整會員點數 - GET
        public async Task<IActionResult> AdjustPoints(int? userId)
        {
            var users = await _adminService.GetUsersAsync();
            var viewModel = new AdminAdjustPointsViewModel
            {
                Users = users,
                UserId = userId ?? 0
            };

            if (userId.HasValue)
            {
                var userWallet = await _adminService.GetUserPointsAsync(userId.Value);
                if (userWallet != null)
                {
                    viewModel.CurrentPoints = userWallet.UserPoint;
                }
            }

            return View(viewModel);
        }

        // 調整會員點數 - POST
        [HttpPost]
        public async Task<IActionResult> AdjustPoints(AdminAdjustPointsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _adminService.AdjustUserPointsAsync(model.UserId, model.Points, model.Reason);
                if (result)
                {
                    TempData["SuccessMessage"] = "點數調整成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "點數調整失敗";
                }
            }
            return RedirectToAction("AdjustPoints", new { userId = model.UserId });
        }

        // 調整會員商城優惠券 - GET
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

        // 調整會員商城優惠券 - POST
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

        // 調整會員電子禮券 - GET
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

        // 調整會員電子禮券 - POST
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

        // 查詢會員收支明細
        public async Task<IActionResult> QueryHistory(CouponQueryModel query)
        {
            var transactions = await _adminService.QueryWalletTransactionsAsync(query);
            var users = await _adminService.GetUsersAsync();

            var viewModel = new AdminQueryHistoryViewModel
            {
                Transactions = transactions,
                Users = users,
                SearchTerm = query.SearchTerm,
                Page = transactions.Page,
                PageSize = transactions.PageSize,
                TotalCount = transactions.TotalCount,
                TotalPages = transactions.TotalPages
            };

            return View(viewModel);
        }

        // 交易記錄歷史
        public async Task<IActionResult> TransactionHistory(CouponQueryModel query)
        {
            var transactions = await _adminService.QueryWalletTransactionsAsync(query);
            return View(transactions);
        }
    }
}
