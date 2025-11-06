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

	/// <summary>
	/// 使用優惠券
	/// </summary>
	/// <param name="couponId">優惠券ID</param>
	/// <param name="userId">用戶ID</param>
	/// <param name="orderId">訂單ID（可選）</param>
	/// <returns>是否成功</returns>
	Task<bool> UseCouponAsync(int couponId, int userId, int? orderId = null);

	/// <summary>
	/// 兌換電子禮券
	/// </summary>
	/// <param name="evoucherId">電子禮券ID</param>
	/// <param name="userId">用戶ID</param>
	/// <returns>是否成功</returns>
	Task<bool> RedeemEVoucherAsync(int evoucherId, int userId);

	/// <summary>
	/// 獲取錢包交易歷史（分頁、篩選、模糊搜尋）
	/// </summary>
	/// <param name="userId">用戶ID</param>
	/// <param name="pageNumber">頁碼（從1開始）</param>
	/// <param name="pageSize">每頁筆數</param>
	/// <param name="changeType">交易類型篩選（Point/Coupon/EVoucher）</param>
	/// <param name="startDate">開始日期</param>
	/// <param name="endDate">結束日期</param>
	/// <param name="searchTerm">搜尋關鍵字（Description）</param>
	/// <returns>交易記錄列表和總筆數</returns>
	Task<(IEnumerable<WalletHistory> items, int totalCount)> GetWalletHistoryAsync(
		int userId,
		int pageNumber,
		int pageSize,
		string? changeType = null,
		DateTime? startDate = null,
		DateTime? endDate = null,
		string? searchTerm = null);

	/// <summary>
	/// 使用點數兌換優惠券
	/// </summary>
	/// <param name="userId">用戶ID</param>
	/// <param name="couponTypeId">優惠券類型ID</param>
	/// <param name="quantity">兌換數量（預設1）</param>
	/// <returns>成功兌換的優惠券代碼列表</returns>
	Task<(bool success, string message, List<string> couponCodes)> ExchangeForCouponAsync(
		int userId,
		int couponTypeId,
		int quantity = 1);

	/// <summary>
	/// 使用點數兌換電子禮券
	/// </summary>
	/// <param name="userId">用戶ID</param>
	/// <param name="evoucherTypeId">電子禮券類型ID</param>
	/// <param name="quantity">兌換數量（預設1）</param>
	/// <returns>成功兌換的電子禮券代碼列表</returns>
	Task<(bool success, string message, List<string> evoucherCodes)> ExchangeForEVoucherAsync(
		int userId,
		int evoucherTypeId,
		int quantity = 1);

	/// <summary>
	/// 獲取所有可兌換的優惠券類型
	/// </summary>
	/// <returns>優惠券類型列表</returns>
	Task<IEnumerable<CouponType>> GetAvailableCouponTypesAsync();

	/// <summary>
	/// 獲取所有可兌換的電子禮券類型
	/// </summary>
	/// <returns>電子禮券類型列表</returns>
	Task<IEnumerable<EvoucherType>> GetAvailableEVoucherTypesAsync();
	}
}
