using GameSpace.Areas.MiniGame.Data;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class CouponService : ICouponService
    {
        private readonly MiniGameDbContext _context;
        private readonly ILogger<CouponService> _logger;

        public CouponService(MiniGameDbContext context, ILogger<CouponService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Coupon>> GetAllCouponsAsync()
        {
            try
            {
                return await _context.Coupons
                    .Include(c => c.CouponType)
                    .OrderByDescending(c => c.AcquiredTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得所有優惠券時發生錯誤");
                return new List<Coupon>();
            }
        }

        public async Task<IEnumerable<Coupon>> GetCouponsByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Coupons
                    .Include(c => c.CouponType)
                    .Where(c => c.UserID == userId)
                    .OrderByDescending(c => c.AcquiredTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者 {UserId} 的優惠券時發生錯誤", userId);
                return new List<Coupon>();
            }
        }

        public async Task<IEnumerable<Coupon>> GetUnusedCouponsByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Coupons
                    .Include(c => c.CouponType)
                    .Where(c => c.UserID == userId && !c.IsUsed)
                    .OrderByDescending(c => c.AcquiredTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者 {UserId} 未使用的優惠券時發生錯誤", userId);
                return new List<Coupon>();
            }
        }

        public async Task<Coupon?> GetCouponByIdAsync(int couponId)
        {
            try
            {
                return await _context.Coupons
                    .Include(c => c.CouponType)
                    .FirstOrDefaultAsync(c => c.CouponID == couponId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得優惠券 {CouponId} 時發生錯誤", couponId);
                return null;
            }
        }

        public async Task<Coupon?> GetCouponByCodeAsync(string couponCode)
        {
            try
            {
                return await _context.Coupons
                    .Include(c => c.CouponType)
                    .FirstOrDefaultAsync(c => c.CouponCode == couponCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得優惠券代碼 {CouponCode} 時發生錯誤", couponCode);
                return null;
            }
        }

        public async Task<bool> CreateCouponAsync(Coupon coupon)
        {
            try
            {
                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();
                _logger.LogInformation("成功建立優惠券 {CouponCode}", coupon.CouponCode);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立優惠券時發生錯誤");
                return false;
            }
        }

        public async Task<bool> UpdateCouponAsync(Coupon coupon)
        {
            try
            {
                _context.Coupons.Update(coupon);
                await _context.SaveChangesAsync();
                _logger.LogInformation("成功更新優惠券 {CouponId}", coupon.CouponID);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新優惠券 {CouponId} 時發生錯誤", coupon.CouponID);
                return false;
            }
        }

        public async Task<bool> DeleteCouponAsync(int couponId)
        {
            try
            {
                var coupon = await _context.Coupons.FindAsync(couponId);
                if (coupon == null) return false;

                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();
                _logger.LogInformation("成功刪除優惠券 {CouponId}", couponId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除優惠券 {CouponId} 時發生錯誤", couponId);
                return false;
            }
        }

        public async Task<bool> UseCouponAsync(int couponId, int? orderId = null)
        {
            try
            {
                var coupon = await _context.Coupons.FindAsync(couponId);
                if (coupon == null || coupon.IsUsed) return false;

                coupon.IsUsed = true;
                coupon.UsedTime = DateTime.Now;
                coupon.UsedInOrderID = orderId;

                await _context.SaveChangesAsync();
                _logger.LogInformation("成功使用優惠券 {CouponId}", couponId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "使用優惠券 {CouponId} 時發生錯誤", couponId);
                return false;
            }
        }

        public async Task<bool> GrantCouponToUserAsync(int userId, int couponTypeId)
        {
            try
            {
                var couponType = await _context.CouponTypes.FindAsync(couponTypeId);
                if (couponType == null) return false;

                var couponCode = GenerateCouponCode();
                var coupon = new Coupon
                {
                    CouponCode = couponCode,
                    CouponTypeID = couponTypeId,
                    UserID = userId,
                    IsUsed = false,
                    AcquiredTime = DateTime.Now
                };

                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();
                _logger.LogInformation("成功發放優惠券 {CouponCode} 給使用者 {UserId}", couponCode, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發放優惠券給使用者 {UserId} 時發生錯誤", userId);
                return false;
            }
        }

        public async Task<int> GetCouponCountByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Coupons
                    .Where(c => c.UserID == userId && !c.IsUsed)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者 {UserId} 優惠券數量時發生錯誤", userId);
                return 0;
            }
        }

        public async Task<IEnumerable<CouponType>> GetAllCouponTypesAsync()
        {
            try
            {
                return await _context.CouponTypes
                    .OrderBy(ct => ct.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得所有優惠券類型時發生錯誤");
                return new List<CouponType>();
            }
        }

        public async Task<CouponType?> GetCouponTypeByIdAsync(int couponTypeId)
        {
            try
            {
                return await _context.CouponTypes.FindAsync(couponTypeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得優惠券類型 {CouponTypeId} 時發生錯誤", couponTypeId);
                return null;
            }
        }

        private string GenerateCouponCode()
        {
            var yearMonth = DateTime.Now.ToString("yyMM");
            var random = new Random();
            var randomCode = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return $"CPN-{yearMonth}-{randomCode}";
        }
    }
}
