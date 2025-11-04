using GamiPort.Models;

namespace GamiPort.Areas.MiniGame.Services
{
	/// <summary>
	/// 前台錢包服務介面
	/// 提供會員點數、優惠券、電子禮券相關查詢功能（讀取為主）
	/// </summary>
	public interface IWalletService
	{
		/// <summary>
		/// 獲取用戶錢包信息
		/// </summary>
		Task<UserWallet?> GetUserWalletAsync(int userId);

		/// <summary>
		/// 獲取用戶點數
		/// </summary>
		Task<int> GetUserPointsAsync(int userId);

		/// <summary>
		/// 獲取用戶優惠券列表（支持模糊搜尋）
		/// </summary>
		/// <param name="userId">用戶ID</param>
		/// <param name="searchTerm">搜尋關鍵字</param>
		/// <returns>優惠券列表</returns>
		Task<IEnumerable<Coupon>> GetUserCouponsAsync(int userId, string searchTerm = "");

		/// <summary>
		/// 獲取用戶電子禮券列表（支持模糊搜尋）
		/// </summary>
		/// <param name="userId">用戶ID</param>
		/// <param name="searchTerm">搜尋關鍵字</param>
		/// <returns>電子禮券列表</returns>
		Task<IEnumerable<Evoucher>> GetUserEVouchersAsync(int userId, string searchTerm = "");

		/// <summary>
		/// 獲取用戶未使用的優惠券數量
		/// </summary>
		Task<int> GetUnusedCouponCountAsync(int userId);

		/// <summary>
		/// 獲取用戶未使用的電子禮券數量
		/// </summary>
		Task<int> GetUnusedEVoucherCountAsync(int userId);

		/// <summary>
		/// 獲取錢包交易記錄
		/// </summary>
		/// <param name="userId">用戶ID</param>
		/// <param name="pageSize">分頁大小</param>
		/// <returns>交易記錄列表</returns>
		Task<IEnumerable<WalletHistory>> GetWalletHistoryAsync(int userId, int pageSize = 10);

		/// <summary>
		/// 獲取錢包交易統計
		/// </summary>
		Task<Dictionary<string, int>> GetPointsSummaryAsync(int userId);
	}
}
