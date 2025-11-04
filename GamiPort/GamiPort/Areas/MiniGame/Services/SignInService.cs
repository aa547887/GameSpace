using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamiPort.Areas.MiniGame.Helpers;
using GamiPort.Infrastructure.Time;
using GamiPort.Models;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.MiniGame.Services
{
	/// <summary>
	/// 簽到服務實現 - 處理簽到相關業務邏輯
	/// </summary>
	public class SignInService : ISignInService
	{
		private readonly GameSpacedatabaseContext _context;
		private readonly IAppClock _appClock;

		public SignInService(GameSpacedatabaseContext context, IAppClock appClock)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_appClock = appClock ?? throw new ArgumentNullException(nameof(appClock));
		}

		/// <summary>
		/// 取得目前簽到狀態
		/// </summary>
		public async Task<SignInStatusDto> GetCurrentSignInStatusAsync(int userId)
		{
			// 取得 UTC+8 時間
			var appNow = _appClock.ToAppTime(_appClock.UtcNow);
			var todayStart = appNow.Date;
			var todayEnd = todayStart.AddDays(1).AddTicks(-1);

			// 轉回 UTC 以供資料庫查詢
			var utcTodayStart = _appClock.ToUtc(todayStart);
			var utcTodayEnd = _appClock.ToUtc(todayEnd);

			// 查詢用戶的所有簽到記錄（未刪除）
			var userSignIns = await _context.UserSignInStats
				.Where(x => x.UserId == userId && !x.IsDeleted)
				.OrderByDescending(x => x.SignTime)
				.ToListAsync();

			// 檢查今天是否已簽到
			var isSignedInToday = userSignIns.Any(x =>
				x.SignTime >= utcTodayStart && x.SignTime <= utcTodayEnd);

			// 計算總簽到天數
			int totalSignInDays = userSignIns.Count;

			// 計算當前連續簽到天數與最長連續簽到天數
			var (currentConsecutiveDays, maxConsecutiveDays) = CalculateConsecutiveDays(userSignIns, appNow.Date);

			// 獲取今天的簽到日序號和獎勵
			int todaySignInDay = currentConsecutiveDays + (isSignedInToday ? 0 : 1);
			var todayRule = await GetSignInRuleByDayAsync(todaySignInDay);

			int todayPoints = todayRule?.Points ?? 0;
			int todayExperience = todayRule?.Experience ?? 0;
			bool todayHasCoupon = todayRule?.HasCoupon ?? false;

			// 上次簽到時間
			var lastSignIn = userSignIns.FirstOrDefault();
			var lastSignInTime = lastSignIn?.SignTime.ToUtc8();

			return new SignInStatusDto
			{
				IsSignedInToday = isSignedInToday,
				CurrentConsecutiveDays = currentConsecutiveDays,
				MaxConsecutiveDays = maxConsecutiveDays,
				TotalSignInDays = totalSignInDays,
				TodaySignInDay = todaySignInDay,
				TodayPoints = todayPoints,
				TodayExperience = todayExperience,
				TodayHasCoupon = todayHasCoupon,
				LastSignInTime = lastSignInTime,
				UpdatedAt = appNow.ToUtc8()
			};
		}

		/// <summary>
		/// 執行簽到
		/// </summary>
		public async Task<SignInResultDto> CheckInAsync(int userId)
		{
			var appNow = _appClock.ToAppTime(_appClock.UtcNow);
			var todayStart = appNow.Date;
			var todayEnd = todayStart.AddDays(1).AddTicks(-1);

			var utcTodayStart = _appClock.ToUtc(todayStart);
			var utcTodayEnd = _appClock.ToUtc(todayEnd);

			// 檢查今天是否已簽到
			var existingSignIn = await _context.UserSignInStats
				.Where(x => x.UserId == userId && !x.IsDeleted)
				.Where(x => x.SignTime >= utcTodayStart && x.SignTime <= utcTodayEnd)
				.FirstOrDefaultAsync();

			if (existingSignIn != null)
			{
				return new SignInResultDto
				{
					Success = false,
					Message = "今天已經簽到過了",
					SignInTime = appNow.ToUtc8()
				};
			}

			// 取得用戶所有簽到記錄，計算連續簽到天數
			var userSignIns = await _context.UserSignInStats
				.Where(x => x.UserId == userId && !x.IsDeleted)
				.OrderByDescending(x => x.SignTime)
				.ToListAsync();

			var (currentConsecutiveDays, _) = CalculateConsecutiveDays(userSignIns, todayStart);
			int nextSignInDay = currentConsecutiveDays + 1;

			// 取得簽到規則（根據日序號）
			var signInRule = await GetSignInRuleByDayAsync(nextSignInDay);
			if (signInRule == null)
			{
				return new SignInResultDto
				{
					Success = false,
					Message = $"簽到日 {nextSignInDay} 無對應規則",
					SignInTime = appNow.ToUtc8()
				};
			}

			// 開始交易：記錄簽到、更新用戶錢包
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var utcNow = _appClock.UtcNow;

				// 1. 建立簽到記錄
				var signInStat = new UserSignInStat
				{
					UserId = userId,
					SignTime = utcNow,
					PointsGained = signInRule.Points,
					PointsGainedTime = utcNow,
					ExpGained = signInRule.Experience,
					ExpGainedTime = utcNow,
					CouponGained = signInRule.HasCoupon ? (signInRule.CouponTypeCode ?? string.Empty) : string.Empty,
					CouponGainedTime = utcNow,
					IsDeleted = false
				};

				_context.UserSignInStats.Add(signInStat);
				await _context.SaveChangesAsync();

				// 2. 更新用戶錢包（點數）
				var wallet = await _context.UserWallets
					.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

				if (wallet != null)
				{
					wallet.UserPoint += signInRule.Points;
					_context.UserWallets.Update(wallet);
					await _context.SaveChangesAsync();
				}

				await transaction.CommitAsync();

				return new SignInResultDto
				{
					Success = true,
					Message = "簽到成功",
					PointsGained = signInRule.Points,
					ExpGained = signInRule.Experience,
					CouponGained = signInRule.HasCoupon ? (signInRule.CouponTypeCode ?? string.Empty) : string.Empty,
					SignInTime = appNow.ToUtc8(),
					ConsecutiveDays = nextSignInDay
				};
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return new SignInResultDto
				{
					Success = false,
					Message = $"簽到失敗: {ex.Message}",
					SignInTime = appNow.ToUtc8()
				};
			}
		}

		/// <summary>
		/// 取得指定月份的簽到日曆資料
		/// </summary>
		public async Task<SignInCalendarDto> GetMonthlyCalendarAsync(int userId, int year, int month)
		{
			// 驗證月份
			if (month < 1 || month > 12)
				month = DateTime.Now.Month;
			if (year < 1900)
				year = DateTime.Now.Year;

			var monthStart = new DateTime(year, month, 1);
			var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

			// 轉成 UTC 以查詢資料庫
			var utcMonthStart = _appClock.ToUtc(monthStart);
			var utcMonthEnd = _appClock.ToUtc(monthEnd);

			// 查詢該月的所有簽到記錄
			var signIns = await _context.UserSignInStats
				.Where(x => x.UserId == userId && !x.IsDeleted)
				.Where(x => x.SignTime >= utcMonthStart && x.SignTime <= utcMonthEnd)
				.ToListAsync();

			var daysInMonth = DateTime.DaysInMonth(year, month);
			var days = new List<SignInDayDto>();

			for (int day = 1; day <= daysInMonth; day++)
			{
				var dayDate = new DateTime(year, month, day);
				var dayUtcStart = _appClock.ToUtc(dayDate.Date);
				var dayUtcEnd = _appClock.ToUtc(dayDate.Date.AddDays(1).AddTicks(-1));

				var signIn = signIns.FirstOrDefault(x => x.SignTime >= dayUtcStart && x.SignTime <= dayUtcEnd);

				var dayDto = new SignInDayDto
				{
					Day = day,
					IsSignedIn = signIn != null,
					SignInTime = signIn?.SignTime.ToUtc8(),
					PointsGained = signIn?.PointsGained,
					ExpGained = signIn?.ExpGained,
					HasCoupon = !string.IsNullOrEmpty(signIn?.CouponGained),
					SignInDayNumber = null
				};

				days.Add(dayDto);
			}

			return new SignInCalendarDto
			{
				Year = year,
				Month = month,
				Days = days
			};
		}

		/// <summary>
		/// 分頁取得簽到歷史記錄
		/// </summary>
		public async Task<SignInHistoryPageDto> GetSignInHistoryAsync(int userId, int page, int pageSize, int? year = null, int? month = null)
		{
			var query = _context.UserSignInStats
				.Where(x => x.UserId == userId && !x.IsDeleted)
				.AsQueryable();

			// 若指定了年月，進行過濾
			if (year.HasValue && month.HasValue)
			{
				var monthStart = new DateTime(year.Value, month.Value, 1);
				var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

				var utcMonthStart = _appClock.ToUtc(monthStart);
				var utcMonthEnd = _appClock.ToUtc(monthEnd);

				query = query.Where(x => x.SignTime >= utcMonthStart && x.SignTime <= utcMonthEnd);
			}

			// 計算總數
			int totalCount = await query.CountAsync();
			int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

			// 確保頁碼有效
			if (page < 1) page = 1;
			if (page > totalPages && totalPages > 0) page = totalPages;

			// 排序並分頁
			var items = await query
				.OrderByDescending(x => x.SignTime)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			// 轉換為 DTO
			var historyItems = items.Select((item, index) => new SignInHistoryItemDto
			{
				LogId = item.LogId,
				SignTime = item.SignTime.ToUtc8(),
				PointsGained = item.PointsGained,
				ExpGained = item.ExpGained,
				CouponGained = item.CouponGained,
				SignInDayNumber = (page - 1) * pageSize + index + 1
			}).ToList();

			return new SignInHistoryPageDto
			{
				TotalCount = totalCount,
				TotalPages = totalPages,
				CurrentPage = page,
				PageSize = pageSize,
				Items = historyItems
			};
		}

		/// <summary>
		/// 根據簽到日序號取得對應的規則
		/// </summary>
		private async Task<SignInRule?> GetSignInRuleByDayAsync(int signInDay)
		{
			return await _context.SignInRules
				.Where(x => x.SignInDay == signInDay && x.IsActive && !x.IsDeleted)
				.FirstOrDefaultAsync();
		}

		/// <summary>
		/// 計算當前連續簽到天數與最長連續簽到天數
		/// </summary>
		private (int currentConsecutive, int maxConsecutive) CalculateConsecutiveDays(
			List<UserSignInStat> signIns, DateTime today)
		{
			if (signIns.Count == 0)
				return (0, 0);

			// 將 UTC 簽到時間轉成 UTC+8 日期
			var signInDates = signIns
				.Select(x => x.SignTime.ToUtc8().Date)
				.Distinct()
				.OrderByDescending(x => x)
				.ToList();

			int maxConsecutive = 0;
			int currentConsecutive = 0;

			// 檢查是否今天已簽到
			bool isSignedInToday = signInDates.FirstOrDefault() == today;

			// 計算最長連續與當前連續
			int consecutiveCount = 0;
			DateTime? expectedDate = isSignedInToday ? today : today.AddDays(1);

			foreach (var date in signInDates)
			{
				if (expectedDate == null || date == expectedDate)
				{
					consecutiveCount++;
					expectedDate = date.AddDays(-1);
				}
				else
				{
					maxConsecutive = Math.Max(maxConsecutive, consecutiveCount);
					consecutiveCount = 1;
					expectedDate = date.AddDays(-1);
				}
			}

			maxConsecutive = Math.Max(maxConsecutive, consecutiveCount);
			currentConsecutive = isSignedInToday ? consecutiveCount : 0;

			return (currentConsecutive, maxConsecutive);
		}
	}
}
