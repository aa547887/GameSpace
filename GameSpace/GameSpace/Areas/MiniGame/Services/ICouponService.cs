using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface ICouponService
    {
        Task<IEnumerable<Coupon>> GetAllCouponsAsync();
        Task<IEnumerable<Coupon>> GetCouponsByUserIdAsync(int userId);
        Task<IEnumerable<Coupon>> GetUnusedCouponsByUserIdAsync(int userId);
        Task<Coupon?> GetCouponByIdAsync(int couponId);
        Task<Coupon?> GetCouponByCodeAsync(string couponCode);
        Task<bool> CreateCouponAsync(Coupon coupon);
        Task<bool> UpdateCouponAsync(Coupon coupon);
        Task<bool> DeleteCouponAsync(int couponId);
        Task<bool> UseCouponAsync(int couponId, int? orderId = null);
        Task<bool> GrantCouponToUserAsync(int userId, int couponTypeId);
        Task<int> GetCouponCountByUserIdAsync(int userId);
        Task<IEnumerable<CouponType>> GetAllCouponTypesAsync();
        Task<CouponType?> GetCouponTypeByIdAsync(int couponTypeId);
    }
}
