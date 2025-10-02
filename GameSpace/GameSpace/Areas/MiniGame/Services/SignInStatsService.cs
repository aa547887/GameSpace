using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Data;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 簽到統計服務實作
    /// </summary>
    public class SignInStatsService : ISignInStatsService
    {
        private readonly MiniGameDbContext _context;

        public SignInStatsService(MiniGameDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得所有簽到記錄
        /// </summary>
        public async Task<IEnumerable<UserSignInStats>> GetAllSignInStatsAsync()
        {
            return await _context.UserSignInStats
                .OrderByDescending(s => s.SignTime)
                .ToListAsync();
        }

        /// <summary>
        /// 根據ID取得簽到記錄
        /// </summary>
        public async Task<UserSignInStats?> GetSignInStatsByIdAsync(int statsId)
        {
            return await _context.UserSignInStats.FindAsync(statsId);
        }

        /// <summary>
        /// 根據使用者ID取得簽到記錄
        /// </summary>
        public async Task<IEnumerable<UserSignInStats>> GetSignInStatsByUserIdAsync(int userId)
        {
            return await _context.UserSignInStats
                .Where(s => s.UserID == userId)
                .OrderByDescending(s => s.SignTime)
                .ToListAsync();
        }

        /// <summary>
        /// 取得使用者今日簽到記錄
        /// </summary>
        public async Task<UserSignInStats?> GetTodaySignInAsync(int userId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _context.UserSignInStats
                .Where(s => s.UserID == userId && s.SignTime >= today && s.SignTime < tomorrow)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 建立簽到記錄
        /// </summary>
        public async Task<bool> CreateSignInStatsAsync(UserSignInStats signInStats)
        {
            try
            {
                _context.UserSignInStats.Add(signInStats);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 更新簽到記錄
        /// </summary>
        public async Task<bool> UpdateSignInStatsAsync(UserSignInStats signInStats)
        {
            try
            {
                _context.UserSignInStats.Update(signInStats);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 執行使用者簽到
        /// </summary>
        public async Task<bool> PerformSignInAsync(int userId)
        {
            try
            {
                // 檢查今日是否已簽到
                var todaySignIn = await GetTodaySignInAsync(userId);
                if (todaySignIn != null)
                    return false; // 今日已簽到

                // 計算簽到獎勵
                var (points, exp, coupon) = await CalculateSignInRewardsAsync(userId);

                var signInStats = new UserSignInStats
                {
                    UserID = userId,
                    SignTime = DateTime.Now,
                    PointsEarned = points,
                    PetExpEarned = exp,
                    CouponEarned = coupon,
                    ConsecutiveDays = await GetConsecutiveSignInDaysAsync(userId) + 1
                };

                return await CreateSignInStatsAsync(signInStats);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 計算簽到獎勵
        /// </summary>
        private async Task<(int points, int exp, int coupon)> CalculateSignInRewardsAsync(int userId)
        {
            // 取得連續簽到天數
            var consecutiveDays = await GetConsecutiveSignInDaysAsync(userId);
            
            // 基礎點數：5-30點（依連續天數）
            var points = Math.Min(5 + consecutiveDays * 2, 30);
            
            // 經驗值：0-20（隨機）
            var exp = new Random().Next(0, 21);
            
            // 優惠券：每7天簽到有機會獲得
            var coupon = (consecutiveDays % 7 == 0 && consecutiveDays > 0) ? 1 : 0;

            return (points, exp, coupon);
        }

        /// <summary>
        /// 取得連續簽到天數
        /// </summary>
        private async Task<int> GetConsecutiveSignInDaysAsync(int userId)
        {
            var recentSignIns = await _context.UserSignInStats
                .Where(s => s.UserID == userId)
                .OrderByDescending(s => s.SignTime)
                .Take(30) // 取最近30天
                .ToListAsync();

            if (!recentSignIns.Any())
                return 0;

            var consecutiveDays = 0;
            var checkDate = DateTime.Today.AddDays(-1); // 從昨天開始檢查

            foreach (var signIn in recentSignIns)
            {
                if (signIn.SignTime.Date == checkDate)
                {
                    consecutiveDays++;
                    checkDate = checkDate.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return consecutiveDays;
        }

        /// <summary>
        /// 取得簽到統計資料
        /// </summary>
        public async Task<object> GetSignInStatisticsAsync()
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            var todayCount = await _context.UserSignInStats
                .Where(s => s.SignTime >= today)
                .CountAsync();

            var monthCount = await _context.UserSignInStats
                .Where(s => s.SignTime >= thisMonth)
                .CountAsync();

            var totalCount = await _context.UserSignInStats.CountAsync();

            return new
            {
                TodaySignIns = todayCount,
                MonthSignIns = monthCount,
                TotalSignIns = totalCount
            };
        }
    }
}
