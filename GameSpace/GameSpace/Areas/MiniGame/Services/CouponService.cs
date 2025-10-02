using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Data;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 優惠券服務實作
    /// </summary>
    public class CouponService : ICouponService
    {
        private readonly MiniGameDbContext _context;

        public CouponService(MiniGameDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得所有優惠券
        /// </summary>
        public async Task<IEnumerable<Coupon>> GetAllCouponsAsync()
        {
            return await _context.Coupons
                .Include(c => c.CouponType)
                .OrderByDescending(c => c.AcquiredTime)
                .ToListAsync();
        }

        /// <summary>
        /// 根據ID取得優惠券
        /// </summary>
        public async Task<Coupon?> GetCouponByIdAsync(int couponId)
        {
            return await _context.Coupons
                .Include(c => c.CouponType)
                .FirstOrDefaultAsync(c => c.CouponID == couponId);
        }

        /// <summary>
        /// 根據使用者ID取得優惠券清單
        /// </summary>
        public async Task<IEnumerable<Coupon>> GetCouponsByUserIdAsync(int userId)
        {
            return await _context.Coupons
                .Include(c => c.CouponType)
                .Where(c => c.UserID == userId)
                .OrderByDescending(c => c.AcquiredTime)
                .ToListAsync();
        }

        /// <summary>
        /// 根據優惠券代碼取得優惠券
        /// </summary>
        public async Task<Coupon?> GetCouponByCodeAsync(string couponCode)
        {
            return await _context.Coupons
                .Include(c => c.CouponType)
                .FirstOrDefaultAsync(c => c.CouponCode == couponCode);
        }

        /// <summary>
        /// 建立新優惠券
        /// </summary>
        public async Task<bool> CreateCouponAsync(Coupon coupon)
        {
            try
            {
                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 更新優惠券
        /// </summary>
        public async Task<bool> UpdateCouponAsync(Coupon coupon)
        {
            try
            {
                _context.Coupons.Update(coupon);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 使用優惠券
        /// </summary>
        public async Task<bool> UseCouponAsync(int couponId, int? orderId = null)
        {
            try
            {
                var coupon = await _context.Coupons.FindAsync(couponId);
                if (coupon == null || coupon.IsUsed)
                    return false;

                coupon.IsUsed = true;
                coupon.UsedTime = DateTime.Now;
                coupon.UsedInOrderID = orderId;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 發放優惠券給使用者
        /// </summary>
        public async Task<bool> GrantCouponToUserAsync(int userId, int couponTypeId)
        {
            try
            {
                var couponType = await _context.CouponTypes.FindAsync(couponTypeId);
                if (couponType == null)
                    return false;

                var coupon = new Coupon
                {
                    CouponCode = GenerateCouponCode(),
                    CouponTypeID = couponTypeId,
                    UserID = userId,
                    IsUsed = false,
                    AcquiredTime = DateTime.Now
                };

                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 取得所有優惠券類型
        /// </summary>
        public async Task<IEnumerable<CouponType>> GetAllCouponTypesAsync()
        {
            return await _context.CouponTypes
                .OrderBy(ct => ct.Name)
                .ToListAsync();
        }

        /// <summary>
        /// 根據ID取得優惠券類型
        /// </summary>
        public async Task<CouponType?> GetCouponTypeByIdAsync(int couponTypeId)
        {
            return await _context.CouponTypes.FindAsync(couponTypeId);
        }

        /// <summary>
        /// 產生優惠券代碼
        /// </summary>
        private string GenerateCouponCode()
        {
            var yearMonth = DateTime.Now.ToString("yyMM");
            var random = new Random().Next(100000, 999999).ToString();
            return $"CPN-{yearMonth}-{random}";
        }
    }
}
