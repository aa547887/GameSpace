using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class WalletController : Controller
    {
        private readonly IMiniGameService _miniGameService;
        private readonly GameSpacedatabaseContext _context;

        public WalletController(IMiniGameService miniGameService, GameSpacedatabaseContext context)
        {
            _miniGameService = miniGameService;
            _context = context;
        }

        // 錢包首頁 - 顯示點數餘額和概覽
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                // 獲取用戶錢包資訊
                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                {
                    // 創建新錢包
                    wallet = new UserWallet
                    {
                        UserId = userId,
                        UserPoint = 0
                    };
                    _context.UserWallets.Add(wallet);
                    await _context.SaveChangesAsync();
                }

                // 獲取優惠券數量
                var couponCount = await _context.Coupons
                    .Where(c => c.UserId == userId && !c.IsUsed)
                    .CountAsync();

                // 獲取禮券數量
                var eVoucherCount = await _context.Evouchers
                    .Where(e => e.UserId == userId && !e.IsUsed)
                    .CountAsync();

                // 獲取最近5筆交易記錄
                var recentHistory = await _context.WalletHistories
                    .Where(h => h.UserId == userId)
                    .OrderByDescending(h => h.ChangeTime)
                    .Take(5)
                    .ToListAsync();

                var viewModel = new UserWalletViewModel
                {
                    UserId = userId,
                    UserPoint = wallet.UserPoint,
                    CouponCount = couponCount,
                    EVoucherCount = eVoucherCount,
                    RecentHistory = recentHistory
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入錢包資訊時發生錯誤：{ex.Message}";
                return View(new UserWalletViewModel { UserId = userId });
            }
        }

        // 收支明細
        public async Task<IActionResult> History(int page = 1, int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                var query = _context.WalletHistories
                    .Where(h => h.UserId == userId)
                    .OrderByDescending(h => h.ChangeTime);

                var totalCount = await query.CountAsync();
                var histories = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var viewModel = new WalletHistoryViewModel
                {
                    Histories = histories,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入收支明細時發生錯誤：{ex.Message}";
                return View(new WalletHistoryViewModel());
            }
        }

        // 我的優惠券
        public async Task<IActionResult> Coupons()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                var coupons = await _context.Coupons
                    .Where(c => c.UserId == userId)
                    .Include(c => c.CouponType)
                    .OrderByDescending(c => c.AcquiredTime)
                    .ToListAsync();

                var viewModel = new UserCouponsViewModel
                {
                    Coupons = coupons
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入優惠券時發生錯誤：{ex.Message}";
                return View(new UserCouponsViewModel());
            }
        }

        // 我的禮券
        public async Task<IActionResult> EVouchers()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                var eVouchers = await _context.Evouchers
                    .Where(e => e.UserId == userId)
                    .Include(e => e.EvoucherType)
                    .OrderByDescending(e => e.AcquiredTime)
                    .ToListAsync();

                var viewModel = new UserEVouchersViewModel
                {
                    EVouchers = eVouchers
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入禮券時發生錯誤：{ex.Message}";
                return View(new UserEVouchersViewModel());
            }
        }

        // 兌換優惠券
        [HttpPost]
        public async Task<IActionResult> ExchangeCoupon(int couponTypeId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new { success = false, message = "請先登入" });
            }

            try
            {
                var couponType = await _context.CouponTypes.FindAsync(couponTypeId);
                if (couponType == null)
                {
                    return Json(new { success = false, message = "優惠券類型不存在" });
                }

                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null || wallet.UserPoint < couponType.PointsCost)
                {
                    return Json(new { success = false, message = "點數不足" });
                }

                // 扣除點數
                wallet.UserPoint -= couponType.PointsCost;

                // 創建優惠券
                var coupon = new Coupon
                {
                    UserId = userId,
                    CouponTypeId = couponTypeId,
                    CouponCode = GenerateCouponCode(),
                    AcquiredTime = DateTime.Now,
                    IsUsed = false
                };

                // 記錄交易歷史
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Coupon",
                    PointsChanged = -couponType.PointsCost,
                    Description = $"兌換優惠券：{couponType.CouponName}",
                    ChangeTime = DateTime.Now
                };

                _context.Coupons.Add(coupon);
                _context.WalletHistories.Add(history);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "兌換成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"兌換失敗：{ex.Message}" });
            }
        }

        // 兌換禮券
        [HttpPost]
        public async Task<IActionResult> ExchangeEVoucher(int eVoucherTypeId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new { success = false, message = "請先登入" });
            }

            try
            {
                var eVoucherType = await _context.EvoucherTypes.FindAsync(eVoucherTypeId);
                if (eVoucherType == null)
                {
                    return Json(new { success = false, message = "禮券類型不存在" });
                }

                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null || wallet.UserPoint < eVoucherType.PointsCost)
                {
                    return Json(new { success = false, message = "點數不足" });
                }

                // 扣除點數
                wallet.UserPoint -= eVoucherType.PointsCost;

                // 創建禮券
                var eVoucher = new Evoucher
                {
                    UserId = userId,
                    EvoucherTypeId = eVoucherTypeId,
                    EvoucherCode = GenerateEVoucherCode(),
                    AcquiredTime = DateTime.Now,
                    IsUsed = false
                };

                // 記錄交易歷史
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "EVoucher",
                    PointsChanged = -eVoucherType.PointsCost,
                    Description = $"兌換禮券：{eVoucherType.EvoucherName}",
                    ChangeTime = DateTime.Now
                };

                _context.Evouchers.Add(eVoucher);
                _context.WalletHistories.Add(history);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "兌換成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"兌換失敗：{ex.Message}" });
            }
        }

        private string GenerateCouponCode()
        {
            return "CPN" + DateTime.Now.Ticks.ToString("X")[^8..];
        }

        private string GenerateEVoucherCode()
        {
            return "EV" + DateTime.Now.Ticks.ToString("X")[^10..];
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }
    }

    // ViewModels
    public class UserWalletViewModel
    {
        public int UserId { get; set; }
        public int UserPoint { get; set; }
        public int CouponCount { get; set; }
        public int EVoucherCount { get; set; }
        public List<WalletHistory> RecentHistory { get; set; } = new();
    }

    public class WalletHistoryViewModel
    {
        public List<WalletHistory> Histories { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class UserCouponsViewModel
    {
        public List<Coupon> Coupons { get; set; } = new();
    }

    public class UserEVouchersViewModel
    {
        public List<Evoucher> EVouchers { get; set; } = new();
    }
}
