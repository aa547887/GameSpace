using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class CouponTypeService : ICouponTypeService
    {
        private readonly GameSpacedatabaseContext _context;

        public CouponTypeService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // CouponType 基本 CRUD
        public async Task<IEnumerable<CouponType>> GetAllCouponTypesAsync()
        {
            return await _context.CouponType
                .OrderByDescending(ct => ct.CreatedAt)
                .ToListAsync();
        }

        public async Task<CouponType?> GetCouponTypeByIdAsync(int couponTypeId)
        {
            return await _context.CouponType
                .FirstOrDefaultAsync(ct => ct.CouponTypeID == couponTypeId);
        }

        public async Task<bool> CreateCouponTypeAsync(CouponType couponType)
        {
            try
            {
                couponType.CreatedAt = DateTime.UtcNow;
                couponType.IsActive = true;
                _context.CouponType.Add(couponType);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCouponTypeAsync(CouponType couponType)
        {
            try
            {
                couponType.UpdatedAt = DateTime.UtcNow;
                _context.CouponType.Update(couponType);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCouponTypeAsync(int couponTypeId)
        {
            try
            {
                var couponType = await GetCouponTypeByIdAsync(couponTypeId);
                if (couponType == null) return false;

                _context.CouponType.Remove(couponType);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // CouponType 狀態管理
        public async Task<bool> ActivateCouponTypeAsync(int couponTypeId)
        {
            try
            {
                var couponType = await GetCouponTypeByIdAsync(couponTypeId);
                if (couponType == null) return false;

                couponType.IsActive = true;
                couponType.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeactivateCouponTypeAsync(int couponTypeId)
        {
            try
            {
                var couponType = await GetCouponTypeByIdAsync(couponTypeId);
                if (couponType == null) return false;

                couponType.IsActive = false;
                couponType.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // CouponType 查詢
        public async Task<IEnumerable<CouponType>> GetActiveCouponTypesAsync()
        {
            return await _context.CouponType
                .Where(ct => ct.IsActive)
                .OrderBy(ct => ct.CouponTypeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<CouponType>> GetCouponTypesByDiscountTypeAsync(string discountType)
        {
            return await _context.CouponType
                .Where(ct => ct.DiscountType == discountType)
                .OrderByDescending(ct => ct.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CouponType>> GetCouponTypesByPointsCostAsync(int minPoints, int maxPoints)
        {
            return await _context.CouponType
                .Where(ct => ct.PointsCost >= minPoints && ct.PointsCost <= maxPoints)
                .OrderBy(ct => ct.PointsCost)
                .ToListAsync();
        }

        // CouponType 統計
        public async Task<int> GetTotalCouponTypesCountAsync()
        {
            return await _context.CouponType.CountAsync();
        }

        public async Task<int> GetActiveCouponTypesCountAsync()
        {
            return await _context.CouponType.CountAsync(ct => ct.IsActive);
        }

        public async Task<Dictionary<string, int>> GetCouponTypesDistributionAsync()
        {
            var distribution = await _context.CouponType
                .GroupBy(ct => ct.DiscountType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            return distribution.ToDictionary(x => x.Type, x => x.Count);
        }

        public async Task<IEnumerable<CouponTypeUsageStats>> GetCouponTypeUsageStatsAsync()
        {
            var couponTypes = await _context.CouponType.ToListAsync();
            var stats = new List<CouponTypeUsageStats>();

            foreach (var couponType in couponTypes)
            {
                var coupons = await _context.Coupon
                    .Where(c => c.CouponTypeID == couponType.CouponTypeID)
                    .ToListAsync();

                var totalIssued = coupons.Count;
                var totalUsed = coupons.Count(c => c.IsUsed);
                var totalUnused = coupons.Count(c => !c.IsUsed);

                stats.Add(new CouponTypeUsageStats
                {
                    CouponTypeId = couponType.CouponTypeID,
                    Name = couponType.CouponTypeName,
                    TotalIssued = totalIssued,
                    TotalUsed = totalUsed,
                    TotalUnused = totalUnused,
                    UsageRate = totalIssued > 0 ? (decimal)totalUsed / totalIssued * 100 : 0
                });
            }

            return stats.OrderByDescending(s => s.TotalIssued);
        }
    }
}

