using GameSpace.Areas.MiniGame.Data;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class SignInStatsService : ISignInStatsService
    {
        private readonly MiniGameDbContext _context;
        private readonly ILogger<SignInStatsService> _logger;

        public SignInStatsService(MiniGameDbContext context, ILogger<SignInStatsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<UserSignInStats>> GetAllSignInStatsAsync()
        {
            try
            {
                return await _context.UserSignInStats
                    .OrderByDescending(s => s.SignTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得所有簽到統計時發生錯誤");
                return new List<UserSignInStats>();
            }
        }

        public async Task<IEnumerable<UserSignInStats>> GetSignInStatsByUserIdAsync(int userId)
        {
            try
            {
                return await _context.UserSignInStats
                    .Where(s => s.UserID == userId)
                    .OrderByDescending(s => s.SignTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者 {UserId} 的簽到統計時發生錯誤", userId);
                return new List<UserSignInStats>();
            }
        }

        public async Task<UserSignInStats?> GetSignInStatsByIdAsync(int statsId)
        {
            try
            {
                return await _context.UserSignInStats
                    .FirstOrDefaultAsync(s => s.StatsID == statsId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得簽到統計 {StatsId} 時發生錯誤", statsId);
                return null;
            }
        }

        public async Task<UserSignInStats?> GetTodaySignInAsync(int userId)
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                return await _context.UserSignInStats
                    .Where(s => s.UserID == userId && s.SignTime >= today && s.SignTime < tomorrow)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者 {UserId} 今日簽到記錄時發生錯誤", userId);
                return null;
            }
        }

        public async Task<UserSignInStats?> GetLatestSignInAsync(int userId)
        {
            try
            {
                return await _context.UserSignInStats
                    .Where(s => s.UserID == userId)
                    .OrderByDescending(s => s.SignTime)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者 {UserId} 最新簽到記錄時發生錯誤", userId);
                return null;
            }
        }

        public async Task<bool> CreateSignInStatsAsync(UserSignInStats signInStats)
        {
            try
            {
                _context.UserSignInStats.Add(signInStats);
                await _context.SaveChangesAsync();
                _logger.LogInformation("成功建立簽到統計記錄，使用者 {UserId}", signInStats.UserID);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立簽到統計記錄時發生錯誤");
                return false;
            }
        }

        public async Task<bool> UpdateSignInStatsAsync(UserSignInStats signInStats)
        {
            try
            {
                _context.UserSignInStats.Update(signInStats);
                await _context.SaveChangesAsync();
                _logger.LogInformation("成功更新簽到統計記錄 {StatsId}", signInStats.StatsID);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新簽到統計記錄 {StatsId} 時發生錯誤", signInStats.StatsID);
                return false;
            }
        }

        public async Task<bool> DeleteSignInStatsAsync(int statsId)
        {
            try
            {
                var signInStats = await _context.UserSignInStats.FindAsync(statsId);
                if (signInStats == null) return false;

                _context.UserSignInStats.Remove(signInStats);
                await _context.SaveChangesAsync();
                _logger.LogInformation("成功刪除簽到統計記錄 {StatsId}", statsId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除簽到統計記錄 {StatsId} 時發生錯誤", statsId);
                return false;
            }
        }

        public async Task<bool> ProcessDailySignInAsync(int userId)
        {
            try
            {
                // 檢查今日是否已簽到
                var todaySignIn = await GetTodaySignInAsync(userId);
                if (todaySignIn != null)
                {
                    _logger.LogWarning("使用者 {UserId} 今日已簽到", userId);
                    return false;
                }

                // 計算連續簽到天數
                var consecutiveDays = await CalculateConsecutiveDaysAsync(userId);

                // 計算獎勵
                var (pointsEarned, petExpEarned, couponEarned) = CalculateSignInRewards(consecutiveDays);

                // 建立簽到記錄
                var signInStats = new UserSignInStats
                {
                    UserID = userId,
                    SignTime = DateTime.Now,
                    PointsEarned = pointsEarned,
                    PetExpEarned = petExpEarned,
                    CouponEarned = couponEarned,
                    ConsecutiveDays = consecutiveDays
                };

                return await CreateSignInStatsAsync(signInStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理使用者 {UserId} 每日簽到時發生錯誤", userId);
                return false;
            }
        }

        public async Task<int> CalculateConsecutiveDaysAsync(int userId)
        {
            try
            {
                var latestSignIn = await GetLatestSignInAsync(userId);
                if (latestSignIn == null) return 1;

                var yesterday = DateTime.Today.AddDays(-1);
                var latestSignInDate = latestSignIn.SignTime.Date;

                // 如果最後簽到是昨天，連續天數+1
                if (latestSignInDate == yesterday)
                {
                    return latestSignIn.ConsecutiveDays + 1;
                }
                // 如果不是昨天，重新開始計算
                else
                {
                    return 1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算使用者 {UserId} 連續簽到天數時發生錯誤", userId);
                return 1;
            }
        }

        public async Task<int> GetSignInCountByUserIdAsync(int userId)
        {
            try
            {
                return await _context.UserSignInStats
                    .Where(s => s.UserID == userId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者 {UserId} 簽到次數時發生錯誤", userId);
                return 0;
            }
        }

        public async Task<int> GetTotalPointsEarnedAsync(int userId)
        {
            try
            {
                return await _context.UserSignInStats
                    .Where(s => s.UserID == userId)
                    .SumAsync(s => s.PointsEarned);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者 {UserId} 簽到總點數時發生錯誤", userId);
                return 0;
            }
        }

        public async Task<bool> HasSignedInTodayAsync(int userId)
        {
            try
            {
                var todaySignIn = await GetTodaySignInAsync(userId);
                return todaySignIn != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查使用者 {UserId} 今日簽到狀態時發生錯誤", userId);
                return false;
            }
        }

        private (int pointsEarned, int petExpEarned, int? couponEarned) CalculateSignInRewards(int consecutiveDays)
        {
            // 基礎點數獎勵
            var basePoints = 10;
            var bonusPoints = Math.Min(consecutiveDays - 1, 20); // 最多額外20點
            var pointsEarned = basePoints + bonusPoints;

            // 寵物經驗值獎勵（每5天連續簽到給予經驗值）
            var petExpEarned = consecutiveDays % 5 == 0 ? 15 : 0;

            // 優惠券獎勵（每7天連續簽到有機會獲得）
            int? couponEarned = null;
            if (consecutiveDays % 7 == 0)
            {
                var random = new Random();
                if (random.NextDouble() < 0.3) // 30% 機率獲得優惠券
                {
                    couponEarned = 1;
                }
            }

            return (pointsEarned, petExpEarned, couponEarned);
        }
    }
}
