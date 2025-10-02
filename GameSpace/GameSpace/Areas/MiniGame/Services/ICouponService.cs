using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 優惠券服務介面
    /// </summary>
    public interface ICouponService
    {
        /// <summary>
        /// 取得所有優惠券
        /// </summary>
        /// <returns>優惠券清單</returns>
        Task<IEnumerable<Coupon>> GetAllCouponsAsync();

        /// <summary>
        /// 根據ID取得優惠券
        /// </summary>
        /// <param name="couponId">優惠券ID</param>
        /// <returns>優惠券資料</returns>
        Task<Coupon?> GetCouponByIdAsync(int couponId);

        /// <summary>
        /// 根據使用者ID取得優惠券清單
        /// </summary>
        /// <param name="userId">使用者ID</param>
        /// <returns>使用者的優惠券清單</returns>
        Task<IEnumerable<Coupon>> GetCouponsByUserIdAsync(int userId);

        /// <summary>
        /// 根據優惠券代碼取得優惠券
        /// </summary>
        /// <param name="couponCode">優惠券代碼</param>
        /// <returns>優惠券資料</returns>
        Task<Coupon?> GetCouponByCodeAsync(string couponCode);

        /// <summary>
        /// 建立新優惠券
        /// </summary>
        /// <param name="coupon">優惠券資料</param>
        /// <returns>建立結果</returns>
        Task<bool> CreateCouponAsync(Coupon coupon);

        /// <summary>
        /// 更新優惠券
        /// </summary>
        /// <param name="coupon">優惠券資料</param>
        /// <returns>更新結果</returns>
        Task<bool> UpdateCouponAsync(Coupon coupon);

        /// <summary>
        /// 使用優惠券
        /// </summary>
        /// <param name="couponId">優惠券ID</param>
        /// <param name="orderId">訂單ID</param>
        /// <returns>使用結果</returns>
        Task<bool> UseCouponAsync(int couponId, int? orderId = null);

        /// <summary>
        /// 發放優惠券給使用者
        /// </summary>
        /// <param name="userId">使用者ID</param>
        /// <param name="couponTypeId">優惠券類型ID</param>
        /// <returns>發放結果</returns>
        Task<bool> GrantCouponToUserAsync(int userId, int couponTypeId);

        /// <summary>
        /// 取得所有優惠券類型
        /// </summary>
        /// <returns>優惠券類型清單</returns>
        Task<IEnumerable<CouponType>> GetAllCouponTypesAsync();

        /// <summary>
        /// 根據ID取得優惠券類型
        /// </summary>
        /// <param name="couponTypeId">優惠券類型ID</param>
        /// <returns>優惠券類型資料</returns>
        Task<CouponType?> GetCouponTypeByIdAsync(int couponTypeId);
    }
}
