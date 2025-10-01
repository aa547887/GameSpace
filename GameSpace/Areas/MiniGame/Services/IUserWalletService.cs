using GameSpace.Areas.MiniGame.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 會員錢包服務介面
    /// </summary>
    public interface IUserWalletService
    {
        /// <summary>
        /// 取得會員錢包資訊
        /// </summary>
        Task<UserWalletViewModel?> GetUserWalletAsync(int userId);
        
        /// <summary>
        /// 調整會員點數
        /// </summary>
        Task<bool> AdjustUserPointsAsync(int userId, int points, string reason);
        
        /// <summary>
        /// 取得會員錢包歷史記錄
        /// </summary>
        Task<List<WalletHistoryViewModel>> GetWalletHistoryAsync(int userId, int page = 1, int pageSize = 20);
        
        /// <summary>
        /// 取得會員優惠券列表
        /// </summary>
        Task<List<CouponViewModel>> GetUserCouponsAsync(int userId);
        
        /// <summary>
        /// 取得會員電子禮券列表
        /// </summary>
        Task<List<EVoucherViewModel>> GetUserEVouchersAsync(int userId);
        
        /// <summary>
        /// 發放優惠券給會員
        /// </summary>
        Task<bool> IssueCouponToUserAsync(int userId, int couponTypeId);
        
        /// <summary>
        /// 發放電子禮券給會員
        /// </summary>
        Task<bool> IssueEVoucherToUserAsync(int userId, int evoucherTypeId);
        
        /// <summary>
        /// 刪除會員優惠券
        /// </summary>
        Task<bool> RemoveUserCouponAsync(int couponId);
        
        /// <summary>
        /// 刪除會員電子禮券
        /// </summary>
        Task<bool> RemoveUserEVoucherAsync(int evoucherId);
    }
}