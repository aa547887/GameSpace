namespace GamiPort.Areas.MiniGame.Constants
{
	/// <summary>
	/// 簽到系統常數定義（前台）
	/// 定義簽到相關的所有 Magic Numbers
	/// </summary>
	public static class SignInConstants
	{
		// ========== 日期計算 ==========

		/// <summary>
		/// 計算當天結束時間 (23:59:59.9999999) 時減去的 Tick 數
		/// </summary>
		public const long TICKS_TO_END_OF_DAY = -1;

		/// <summary>
		/// 計算明天的日期時加上的天數
		/// </summary>
		public const int DAYS_TO_ADD_FOR_TOMORROW = 1;

		/// <summary>
		/// 計算下個月的日期時加上的月數
		/// </summary>
		public const int MONTHS_TO_ADD_FOR_NEXT_MONTH = 1;

		/// <summary>
		/// 計算前一天日期（連續簽到計算）時減去的天數
		/// </summary>
		public const int DAYS_TO_SUBTRACT_FOR_YESTERDAY = -1;

		// ========== 預設值 ==========

		/// <summary>
		/// 無簽到規則時的預設點數
		/// </summary>
		public const int DEFAULT_POINTS_IF_NO_RULE = 0;

		/// <summary>
		/// 無簽到規則時的預設經驗值
		/// </summary>
		public const int DEFAULT_EXPERIENCE_IF_NO_RULE = 0;

		/// <summary>
		/// 無簽到規則時的預設優惠券狀態
		/// </summary>
		public const bool DEFAULT_HAS_COUPON_IF_NO_RULE = false;

		/// <summary>
		/// 空優惠券代碼
		/// </summary>
		public const string EMPTY_COUPON_CODE = "";

		// ========== 簽到計算 ==========

		/// <summary>
		/// 計算下一個簽到日序號的增量
		/// </summary>
		public const int SIGN_IN_DAY_INCREMENT = 1;

		/// <summary>
		/// 已簽到時的連續天數偏移（今天簽到不算明天）
		/// </summary>
		public const int CONSECUTIVE_DAY_OFFSET_IF_SIGNED = 0;

		/// <summary>
		/// 未簽到時的連續天數偏移（+1 表示今天尚未簽到）
		/// </summary>
		public const int CONSECUTIVE_DAY_OFFSET_IF_NOT = 1;

		// ========== 日期驗證 ==========

		/// <summary>
		/// 月份最小值
		/// </summary>
		public const int MIN_MONTH = 1;

		/// <summary>
		/// 月份最大值
		/// </summary>
		public const int MAX_MONTH = 12;

		/// <summary>
		/// 年份最小值
		/// </summary>
		public const int MIN_YEAR = 1900;

		/// <summary>
		/// 日曆開始日期
		/// </summary>
		public const int CALENDAR_DAY_START = 1;

		// ========== 分頁計算 ==========

		/// <summary>
		/// 最小頁碼
		/// </summary>
		public const int MIN_PAGE_NUMBER = 1;

		/// <summary>
		/// 最小總頁數
		/// </summary>
		public const int MIN_TOTAL_PAGES = 0;

		/// <summary>
		/// 頁碼轉索引的偏移 (page - 1)
		/// </summary>
		public const int PAGE_INDEX_OFFSET = 1;

		// ========== 連續簽到初始值 ==========

		/// <summary>
		/// 初始連續簽到天數
		/// </summary>
		public const int INITIAL_CONSECUTIVE_COUNT = 0;

		/// <summary>
		/// 初始最大連續天數
		/// </summary>
		public const int INITIAL_MAX_CONSECUTIVE = 0;

		/// <summary>
		/// 連續簽到計數遞增值
		/// </summary>
		public const int CONSECUTIVE_COUNT_INCREMENT = 1;

		// ========== 錯誤訊息 ==========

		/// <summary>
		/// 重複簽到錯誤訊息
		/// </summary>
		public const string ERROR_ALREADY_SIGNED_IN = "今天已經簽到過了";

		/// <summary>
		/// 無對應規則錯誤訊息模板（需使用 string.Format 格式化）
		/// 使用方式：string.Format(SignInConstants.ERROR_NO_RULE_FOR_DAY, dayNumber)
		/// </summary>
		public const string ERROR_NO_RULE_FOR_DAY = "簽到日 {0} 無對應規則";

		/// <summary>
		/// 簽到失敗訊息模板（需使用 string.Format 格式化）
		/// 使用方式：string.Format(SignInConstants.ERROR_CHECK_IN_FAILED, exceptionMessage)
		/// </summary>
		public const string ERROR_CHECK_IN_FAILED = "簽到失敗: {0}";

		// ========== 成功訊息 ==========

		/// <summary>
		/// 簽到成功訊息
		/// </summary>
		public const string SUCCESS_CHECK_IN = "簽到成功";
	}
}
