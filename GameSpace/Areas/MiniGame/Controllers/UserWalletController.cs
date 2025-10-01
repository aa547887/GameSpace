using Microsoft.AspNetCore.Mvc;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.ViewModels;
using GameSpace.Areas.MiniGame.Filters;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 會員錢包管理控制器
    /// </summary>
    [Area("MiniGame")]
    [MiniGameAdminAuthorize("UserStatusManagement")]
    public class UserWalletController : Controller
    {
        private readonly IUserWalletService _userWalletService;
        private readonly ILogger<UserWalletController> _logger;

        public UserWalletController(IUserWalletService userWalletService, ILogger<UserWalletController> logger)
        {
            _userWalletService = userWalletService;
            _logger = logger;
        }

        /// <summary>
        /// 會員錢包管理首頁
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            try
            {
                // 這裡應該實作分頁查詢所有會員錢包資訊
                // 暫時回傳空列表，實際實作時需要加入會員查詢邏輯
                var wallets = new List<UserWalletViewModel>();
                
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = 1;

                return View(wallets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得會員錢包列表失敗");
                TempData["ErrorMessage"] = "取得會員錢包列表失敗";
                return View(new List<UserWalletViewModel>());
            }
        }

        /// <summary>
        /// 取得會員錢包詳情
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int userId)
        {
            try
            {
                var wallet = await _userWalletService.GetUserWalletAsync(userId);
                if (wallet == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的會員錢包";
                    return RedirectToAction(nameof(Index));
                }

                // 取得錢包歷史記錄
                wallet.WalletHistory = await _userWalletService.GetWalletHistoryAsync(userId, 1, 50);
                
                // 取得優惠券列表
                wallet.UserCoupons = await _userWalletService.GetUserCouponsAsync(userId);
                
                // 取得電子禮券列表
                wallet.UserEVouchers = await _userWalletService.GetUserEVouchersAsync(userId);

                return View(wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得會員錢包詳情失敗，UserID: {UserId}", userId);
                TempData["ErrorMessage"] = "取得會員錢包詳情失敗";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 調整會員點數
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustPoints(int userId, int? adjustPoints, string? adjustReason)
        {
            try
            {
                if (adjustPoints == null || adjustPoints == 0)
                {
                    TempData["ErrorMessage"] = "請輸入有效的點數調整值";
                    return RedirectToAction(nameof(Details), new { userId });
                }

                if (string.IsNullOrWhiteSpace(adjustReason))
                {
                    TempData["ErrorMessage"] = "請輸入調整原因";
                    return RedirectToAction(nameof(Details), new { userId });
                }

                var success = await _userWalletService.AdjustUserPointsAsync(userId, adjustPoints.Value, adjustReason);
                if (success)
                {
                    TempData["SuccessMessage"] = $"成功調整會員點數 {adjustPoints} 點";
                }
                else
                {
                    TempData["ErrorMessage"] = "調整會員點數失敗";
                }

                return RedirectToAction(nameof(Details), new { userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "調整會員點數失敗，UserID: {UserId}, Points: {Points}", userId, adjustPoints);
                TempData["ErrorMessage"] = "調整會員點數失敗";
                return RedirectToAction(nameof(Details), new { userId });
            }
        }

        /// <summary>
        /// 發放優惠券
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IssueCoupon(int userId, int couponTypeId)
        {
            try
            {
                var success = await _userWalletService.IssueCouponToUserAsync(userId, couponTypeId);
                if (success)
                {
                    TempData["SuccessMessage"] = "成功發放優惠券";
                }
                else
                {
                    TempData["ErrorMessage"] = "發放優惠券失敗";
                }

                return RedirectToAction(nameof(Details), new { userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發放優惠券失敗，UserID: {UserId}, CouponTypeID: {CouponTypeId}", userId, couponTypeId);
                TempData["ErrorMessage"] = "發放優惠券失敗";
                return RedirectToAction(nameof(Details), new { userId });
            }
        }

        /// <summary>
        /// 發放電子禮券
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IssueEVoucher(int userId, int evoucherTypeId)
        {
            try
            {
                var success = await _userWalletService.IssueEVoucherToUserAsync(userId, evoucherTypeId);
                if (success)
                {
                    TempData["SuccessMessage"] = "成功發放電子禮券";
                }
                else
                {
                    TempData["ErrorMessage"] = "發放電子禮券失敗";
                }

                return RedirectToAction(nameof(Details), new { userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發放電子禮券失敗，UserID: {UserId}, EVoucherTypeID: {EVoucherTypeId}", userId, evoucherTypeId);
                TempData["ErrorMessage"] = "發放電子禮券失敗";
                return RedirectToAction(nameof(Details), new { userId });
            }
        }

        /// <summary>
        /// 刪除優惠券
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCoupon(int couponId, int userId)
        {
            try
            {
                var success = await _userWalletService.RemoveUserCouponAsync(couponId);
                if (success)
                {
                    TempData["SuccessMessage"] = "成功刪除優惠券";
                }
                else
                {
                    TempData["ErrorMessage"] = "刪除優惠券失敗";
                }

                return RedirectToAction(nameof(Details), new { userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除優惠券失敗，CouponID: {CouponId}", couponId);
                TempData["ErrorMessage"] = "刪除優惠券失敗";
                return RedirectToAction(nameof(Details), new { userId });
            }
        }

        /// <summary>
        /// 刪除電子禮券
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveEVoucher(int evoucherId, int userId)
        {
            try
            {
                var success = await _userWalletService.RemoveUserEVoucherAsync(evoucherId);
                if (success)
                {
                    TempData["SuccessMessage"] = "成功刪除電子禮券";
                }
                else
                {
                    TempData["ErrorMessage"] = "刪除電子禮券失敗";
                }

                return RedirectToAction(nameof(Details), new { userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除電子禮券失敗，EVoucherID: {EVoucherId}", evoucherId);
                TempData["ErrorMessage"] = "刪除電子禮券失敗";
                return RedirectToAction(nameof(Details), new { userId });
            }
        }
    }
}