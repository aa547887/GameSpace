using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamiPort.Areas.MiniGame.Services
{
	/// <summary>
	/// 簽到服務介面 - 處理簽到相關業務邏輯
	/// </summary>
	public interface ISignInService
	{
		/// <summary>
		/// 取得目前簽到狀態：是否已簽到、連續簽到天數、下次簽到獎勵等
		/// </summary>
		/// <param name="userId">使用者 ID</param>
		/// <returns>簽到狀態 DTO</returns>
		Task<SignInStatusDto> GetCurrentSignInStatusAsync(int userId);

		/// <summary>
		/// 執行簽到：發放點數、寵物經驗、可能的優惠券
		/// </summary>
		/// <param name="userId">使用者 ID</param>
		/// <returns>簽到結果（已簽到的獎勵內容）</returns>
		Task<SignInResultDto> CheckInAsync(int userId);

		/// <summary>
		/// 取得指定月份的簽到日曆資料
		/// </summary>
		/// <param name="userId">使用者 ID</param>
		/// <param name="year">年份</param>
		/// <param name="month">月份 (1-12)</param>
		/// <returns>月份日曆數據（每日簽到與否、獎勵等）</returns>
		Task<SignInCalendarDto> GetMonthlyCalendarAsync(int userId, int year, int month);

		/// <summary>
		/// 分頁取得簽到歷史記錄
		/// </summary>
		/// <param name="userId">使用者 ID</param>
		/// <param name="page">頁碼（1-based）</param>
		/// <param name="pageSize">每頁筆數</param>
		/// <param name="year">篩選年份（可選）</param>
		/// <param name="month">篩選月份 (1-12)（可選）</param>
		/// <returns>分頁簽到歷史記錄</returns>
		Task<SignInHistoryPageDto> GetSignInHistoryAsync(int userId, int page, int pageSize, int? year = null, int? month = null);

		/// <summary>
		/// 取得所有活動的簽到規則（供使用者查看獎勵表）
		/// </summary>
		/// <returns>所有活動簽到規則列表（按日序號排序）</returns>
		Task<List<SignInRuleDto>> GetAllActiveRulesAsync();
	}

	/// <summary>
	/// 簽到狀態 DTO
	/// </summary>
	public class SignInStatusDto
	{
		/// <summary>是否今天已簽到</summary>
		public bool IsSignedInToday { get; set; }

		/// <summary>當前連續簽到天數</summary>
		public int CurrentConsecutiveDays { get; set; }

		/// <summary>最長連續簽到天數</summary>
		public int MaxConsecutiveDays { get; set; }

		/// <summary>總簽到天數</summary>
		public int TotalSignInDays { get; set; }

		/// <summary>今天的簽到獎勵（日期範圍內第幾個簽到日，例如第 1 天、第 7 天）</summary>
		public int TodaySignInDay { get; set; }

		/// <summary>今天會獲得的點數</summary>
		public int TodayPoints { get; set; }

		/// <summary>今天會獲得的寵物經驗值</summary>
		public int TodayExperience { get; set; }

		/// <summary>今天是否會獲得優惠券</summary>
		public bool TodayHasCoupon { get; set; }

		/// <summary>實際獲得的點數（已簽到時才有值）</summary>
		public int? ActualPointsGained { get; set; }

		/// <summary>實際獲得的經驗（已簽到時才有值）</summary>
		public int? ActualExpGained { get; set; }

		/// <summary>實際獲得的優惠券代碼（已簽到時才有值）</summary>
		public string? ActualCouponGained { get; set; }

		/// <summary>上次簽到時間（UTC+8 格式）</summary>
		public DateTime? LastSignInTime { get; set; }

		/// <summary>最後更新時間（UTC+8 格式）</summary>
		public DateTime UpdatedAt { get; set; }
	}

	/// <summary>
	/// 簽到結果 DTO
	/// </summary>
	public class SignInResultDto
	{
		/// <summary>簽到是否成功</summary>
		public bool Success { get; set; }

		/// <summary>錯誤訊息（若失敗）</summary>
		public string? Message { get; set; }

		/// <summary>獲得的點數</summary>
		public int PointsGained { get; set; }

		/// <summary>獲得的寵物經驗</summary>
		public int ExpGained { get; set; }

		/// <summary>獲得的優惠券代碼（可能為空）</summary>
		public string CouponGained { get; set; } = string.Empty;

		/// <summary>簽到時間（UTC+8 格式）</summary>
		public DateTime SignInTime { get; set; }

		/// <summary>當前連續簽到天數</summary>
		public int ConsecutiveDays { get; set; }
	}

	/// <summary>
	/// 月份簽到日曆 DTO
	/// </summary>
	public class SignInCalendarDto
	{
		/// <summary>年份</summary>
		public int Year { get; set; }

		/// <summary>月份</summary>
		public int Month { get; set; }

		/// <summary>該月所有簽到日期資料</summary>
		public List<SignInDayDto> Days { get; set; } = new();
	}

	/// <summary>
	/// 單日簽到資料 DTO
	/// </summary>
	public class SignInDayDto
	{
		/// <summary>日期（1-31）</summary>
		public int Day { get; set; }

		/// <summary>是否已簽到</summary>
		public bool IsSignedIn { get; set; }

		/// <summary>簽到時間（UTC+8 格式；若未簽到則為 null）</summary>
		public DateTime? SignInTime { get; set; }

		/// <summary>獲得點數</summary>
		public int? PointsGained { get; set; }

		/// <summary>獲得經驗</summary>
		public int? ExpGained { get; set; }

		/// <summary>是否獲得優惠券</summary>
		public bool HasCoupon { get; set; }

		/// <summary>當天的簽到日序號（可能為 null，若未在規則內）</summary>
		public int? SignInDayNumber { get; set; }
	}

	/// <summary>
	/// 分頁簽到歷史 DTO
	/// </summary>
	public class SignInHistoryPageDto
	{
		/// <summary>總記錄數</summary>
		public int TotalCount { get; set; }

		/// <summary>總頁數</summary>
		public int TotalPages { get; set; }

		/// <summary>當前頁碼</summary>
		public int CurrentPage { get; set; }

		/// <summary>每頁筆數</summary>
		public int PageSize { get; set; }

		/// <summary>該頁的簽到記錄</summary>
		public List<SignInHistoryItemDto> Items { get; set; } = new();
	}

	/// <summary>
	/// 單筆簽到歷史記錄 DTO
	/// </summary>
	public class SignInHistoryItemDto
	{
		/// <summary>簽到 Log ID</summary>
		public int LogId { get; set; }

		/// <summary>簽到時間（UTC+8 格式）</summary>
		public DateTime SignTime { get; set; }

		/// <summary>獲得的點數</summary>
		public int PointsGained { get; set; }

		/// <summary>獲得的寵物經驗</summary>
		public int ExpGained { get; set; }

		/// <summary>獲得的優惠券代碼</summary>
		public string CouponGained { get; set; } = string.Empty;

		/// <summary>簽到日序號（當月第幾天簽到）</summary>
		public int SignInDayNumber { get; set; }
	}

	/// <summary>
	/// 簽到規則 DTO（用於顯示獎勵規則表）
	/// </summary>
	public class SignInRuleDto
	{
		/// <summary>規則 ID</summary>
		public int Id { get; set; }

		/// <summary>簽到日序號（第幾天簽到）</summary>
		public int SignInDay { get; set; }

		/// <summary>點數獎勵</summary>
		public int Points { get; set; }

		/// <summary>經驗值獎勵</summary>
		public int Experience { get; set; }

		/// <summary>是否有優惠券獎勵</summary>
		public bool HasCoupon { get; set; }

		/// <summary>優惠券類型代碼</summary>
		public string? CouponTypeCode { get; set; }

		/// <summary>規則描述</summary>
		public string? Description { get; set; }
	}
}
