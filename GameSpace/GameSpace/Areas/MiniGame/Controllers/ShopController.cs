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

            var user = await _context.Users.FindAsync(userId);
            var coupons = await _context.Coupons
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var eVouchers = await _context.Evouchers
                .Where(e => e.UserId == userId)
                .ToListAsync();

            var viewModel = new
            {
                User = user,
                Coupons = coupons,
                EVouchers = eVouchers
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ExchangeCoupon(int couponTypeId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new { success = false, message = "請先登入" });
            }

            var couponType = await _context.CouponTypes.FindAsync(couponTypeId);
            if (couponType == null)
            {
                return Json(new { success = false, message = "優惠券類型不存在" });
            }

            // 檢查點數（需要從其他地方獲取用戶點數）
            // var user = await _context.Users.FindAsync(userId);
            // if (user.UserPoint < couponType.PointsCost)
            // {
            //     return Json(new { success = false, message = "點數不足" });
            // }

            // 發放優惠券
            var coupon = new GameSpace.Models.Coupon
            {
                UserId = userId,
                CouponTypeId = couponTypeId,
                CouponCode = GenerateCouponCode(),
                AcquiredTime = DateTime.Now,
                IsUsed = false
            };

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "兌換成功" });
        }

        private string GenerateCouponCode()
        {
            return "CPN" + DateTime.Now.Ticks.ToString("X")[^8..];
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
}
