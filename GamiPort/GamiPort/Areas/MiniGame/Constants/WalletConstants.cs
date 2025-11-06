namespace GamiPort.Areas.MiniGame.Constants
{
	/// <summary>
	/// 錢包系統常數定義（前台）
	/// 定義錢包、優惠券、電子禮券相關的所有 Magic Numbers
	/// </summary>
	public static class WalletConstants
	{
		// ========== 分頁設置 ==========

		/// <summary>
		/// GetWalletHistoryAsync 默認頁面大小
		/// </summary>
		public const int DEFAULT_PAGE_SIZE = 10;

		/// <summary>
		/// GetWalletHistoryAsync（分頁版本）默認頁面大小
		/// </summary>
		public const int DEFAULT_PAGE_SIZE_PAGED = 20;

		/// <summary>
		/// 默認頁碼
		/// </summary>
		public const int DEFAULT_PAGE_NUMBER = 1;

		/// <summary>
		/// 分頁大小最小值
		/// </summary>
		public const int MIN_PAGE_SIZE = 10;

		/// <summary>
		/// 分頁大小最大值
		/// </summary>
		public const int MAX_PAGE_SIZE = 200;

		// ========== 統計摘要鍵名 ==========

		/// <summary>
		/// 點數統計 - 當前點數鍵名
		/// </summary>
		public const string KEY_CURRENT_POINTS = "CurrentPoints";

		/// <summary>
		/// 點數統計 - 累計獲得鍵名
		/// </summary>
		public const string KEY_TOTAL_EARNED = "TotalEarned";

		/// <summary>
		/// 點數統計 - 累計消費鍵名
		/// </summary>
		public const string KEY_TOTAL_SPENT = "TotalSpent";

		/// <summary>
		/// 點數統計 - 交易次數鍵名
		/// </summary>
		public const string KEY_TRANSACTION_COUNT = "TransactionCount";

		// ========== 默認數值 ==========

		/// <summary>
		/// 錢包不存在時的默認點數值
		/// </summary>
		public const int DEFAULT_POINT_VALUE = 0;

		/// <summary>
		/// 查詢失敗時的默認計數值
		/// </summary>
		public const int DEFAULT_COUNT_VALUE = 0;

		// ========== 日期計算 ==========

		/// <summary>
		/// 日期計算 - 添加天數（包含結束日期整天）
		/// </summary>
		public const int DAYS_TO_ADD_FOR_END_DATE = 1;

		/// <summary>
		/// 日期計算 - 減少Ticks（包含結束日期整天，計算到 23:59:59.9999999）
		/// </summary>
		public const long TICKS_TO_SUBTRACT_FOR_INCLUSIVE_END = -1;

		// ========== 點數比較 ==========

		/// <summary>
		/// 點數比較 - 獲得點數判斷閾值 (> 0)
		/// </summary>
		public const int ZERO_COMPARISON = 0;

		/// <summary>
		/// 分頁偏移量計算常數 (page - 1)
		/// </summary>
		public const int PAGE_OFFSET_CALCULATION = 1;

		// ========== 交易類型（ChangeType）==========

		/// <summary>
		/// 變更類型 - 點數變動
		/// </summary>
		public const string CHANGE_TYPE_POINT = "Point";

		/// <summary>
		/// 變更類型 - 優惠券
		/// </summary>
		public const string CHANGE_TYPE_COUPON = "Coupon";

		/// <summary>
		/// 變更類型 - 電子禮券
		/// </summary>
		public const string CHANGE_TYPE_EVOUCHER = "EVoucher";

		/// <summary>
		/// 變更類型 - 遊戲獎勵
		/// </summary>
		public const string CHANGE_TYPE_GAME_REWARD = "遊戲獎勵";

		/// <summary>
		/// 變更類型 - 簽到獎勵
		/// </summary>
		public const string CHANGE_TYPE_SIGNIN_REWARD = "簽到獎勵";

		/// <summary>
		/// 變更類型 - 寵物升級獎勵
		/// </summary>
		public const string CHANGE_TYPE_PET_LEVELUP = "寵物升級獎勵";

		// ========== 狀態值 ==========

		/// <summary>
		/// 電子禮券狀態 - 已兌換
		/// </summary>
		public const string EVOUCHER_STATUS_REDEEMED = "Redeemed";

		/// <summary>
		/// 電子禮券狀態 - 已撤銷
		/// </summary>
		public const string EVOUCHER_STATUS_REVOKED = "Revoked";
	}
}
