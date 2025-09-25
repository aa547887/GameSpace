using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class ShopController : Controller
    {
        private readonly IMiniGameService _miniGameService;
        private readonly GameSpacedatabaseContext _context;

        public ShopController(IMiniGameService miniGameService, GameSpacedatabaseContext context)
        {
            _miniGameService = miniGameService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                // 獲取用戶資訊
                var user = await _context.Users.FindAsync(userId);
                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                // 獲取可兌換的優惠券類型
                var couponTypes = await _context.CouponTypes
                    .Where(ct => ct.ValidFrom <= DateTime.Now && ct.ValidTo >= DateTime.Now)
                    .ToListAsync();

                // 獲取可兌換的禮券類型
                var eVoucherTypes = await _context.EvoucherTypes
                    .Where(et => et.ValidFrom <= DateTime.Now && et.ValidTo >= DateTime.Now)
                    .ToListAsync();

                // 獲取用戶現有的券
                var userCoupons = await _context.Coupons
                    .Where(c => c.UserId == userId)
                    .Include(c => c.CouponType)
                    .ToListAsync();

                var userEVouchers = await _context.Evouchers
                    .Where(e => e.UserId == userId)
                    .Include(e => e.EvoucherType)
                    .ToListAsync();

                var viewModel = new ShopViewModel
                {
                    User = user,
                    Wallet = wallet,
                    CouponTypes = couponTypes,
                    EVoucherTypes = eVoucherTypes,
                    UserCoupons = userCoupons,
                    UserEVouchers = userEVouchers
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入商店時發生錯誤：{ex.Message}";
                return View(new ShopViewModel());
            }
        }

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

                // 檢查有效期
                if (couponType.ValidFrom > DateTime.Now || couponType.ValidTo < DateTime.Now)
                {
                    return Json(new { success = false, message = "優惠券已過期" });
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

                return Json(new { 
                    success = true, 
                    message = "兌換成功",
                    newBalance = wallet.UserPoint,
                    couponCode = coupon.CouponCode
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"兌換失敗：{ex.Message}" });
            }
        }

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

                // 檢查有效期
                if (eVoucherType.ValidFrom > DateTime.Now || eVoucherType.ValidTo < DateTime.Now)
                {
                    return Json(new { success = false, message = "禮券已過期" });
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

                return Json(new { 
                    success = true, 
                    message = "兌換成功",
                    newBalance = wallet.UserPoint,
                    eVoucherCode = eVoucher.EvoucherCode
                });
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
    public class ShopViewModel
    {
        public User User { get; set; }
        public UserWallet Wallet { get; set; }
        public List<CouponType> CouponTypes { get; set; } = new();
        public List<EvoucherType> EVoucherTypes { get; set; } = new();
        public List<Coupon> UserCoupons { get; set; } = new();
        public List<Evoucher> UserEVouchers { get; set; } = new();
    }
}
