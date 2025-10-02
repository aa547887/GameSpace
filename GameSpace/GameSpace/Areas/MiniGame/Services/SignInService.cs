using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class SignInService : ISignInService
    {
        private readonly GameSpacedatabaseContext _context;

        public SignInService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // 簽到基本操作
        public async Task<bool> SignInAsync(int userId)
        {
            try
            {
                // 檢查今天是否已簽到
                if (!await CanSignInTodayAsync(userId))
                    return false;

                var today = DateTime.UtcNow.Date;
                var lastSignIn = await GetLastSignInDateAsync(userId);
                var consecutiveDays = await GetConsecutiveDaysAsync(userId);

                // 計算連續天數
                if (lastSignIn.HasValue && lastSignIn.Value.Date == today.AddDays(-1))
                {
                    consecutiveDays++;
                }
                else if (!lastSignIn.HasValue || lastSignIn.Value.Date < today.AddDays(-1))
                {
                    consecutiveDays = 1;
                }

                // 計算獎勵
                var reward = await CalculateSignInRewardAsync(userId);

                // 新增簽到記錄
                var signInLog = new UserSignInStats
                {
                    UserID = userId,
                    SignInTime = DateTime.UtcNow,
                    ConsecutiveDays = consecutiveDays,
                    PointsGained = reward.Points,
                    ExpGained = reward.Experience,
                    CouponGained = reward.CouponCode
                };
                _context.User_SignInStats.Add(signInLog);

                // 發放獎勵
                await GrantSignInRewardAsync(userId, reward);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CanSignInTodayAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var signedInToday = await _context.User_SignInStats
                .AnyAsync(s => s.UserID == userId && s.SignInTime.Date == today);

            return !signedInToday;
        }

        public async Task<DateTime?> GetLastSignInDateAsync(int userId)
        {
            var lastSignIn = await _context.User_SignInStats
                .Where(s => s.UserID == userId)
                .OrderByDescending(s => s.SignInTime)
                .FirstOrDefaultAsync();

            return lastSignIn?.SignInTime;
        }

        public async Task<int> GetConsecutiveDaysAsync(int userId)
        {
            var lastSignIn = await _context.User_SignInStats
                .Where(s => s.UserID == userId)
                .OrderByDescending(s => s.SignInTime)
                .FirstOrDefaultAsync();

            return lastSignIn?.ConsecutiveDays ?? 0;
        }

        // 簽到記錄查詢
        public async Task<IEnumerable<UserSignInStats>> GetSignInHistoryAsync(int userId, int pageNumber = 1, int pageSize = 20)
        {
            return await _context.User_SignInStats
                .Where(s => s.UserID == userId)
                .OrderByDescending(s => s.SignInTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserSignInStats>> GetAllSignInsAsync(int pageNumber = 1, int pageSize = 50)
        {
            return await _context.User_SignInStats
                .Include(s => s.Users)
                .OrderByDescending(s => s.SignInTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserSignInStats>> GetSignInsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.User_SignInStats
                .Include(s => s.Users)
                .Where(s => s.SignInTime >= startDate && s.SignInTime <= endDate)
                .OrderByDescending(s => s.SignInTime)
                .ToListAsync();
        }

        public async Task<UserSignInStats?> GetSignInDetailAsync(int logId)
        {
            return await _context.User_SignInStats
                .Include(s => s.Users)
                .FirstOrDefaultAsync(s => s.LogID == logId);
        }

        // 簽到獎勵
        public async Task<SignInReward> CalculateSignInRewardAsync(int userId)
        {
            var consecutiveDays = await GetConsecutiveDaysAsync(userId);
            var nextDay = consecutiveDays + 1;

            var reward = new SignInReward
            {
                Points = 10, // 基礎點數
                Experience = 5, // 基礎經驗
                ConsecutiveDayBonus = 0
            };

            // 連續簽到獎勵規則
            var rules = await GetSignInRewardRulesAsync();
            var applicableRule = rules
                .Where(r => r.DayNumber <= nextDay)
                .OrderByDescending(r => r.DayNumber)
                .FirstOrDefault();

            if (applicableRule != null)
            {
                reward.Points = applicableRule.Points;
                reward.Experience = applicableRule.Experience;

                if (applicableRule.HasCoupon)
                {
                    reward.CouponCode = $"SIGNIN_{nextDay}";
                }
            }

            // 連續天數加成
            if (nextDay >= 7)
            {
                reward.ConsecutiveDayBonus = nextDay / 7 * 5;
                reward.Points += reward.ConsecutiveDayBonus;
            }

            return reward;
        }

        public async Task<bool> GrantSignInRewardAsync(int userId, SignInReward reward)
        {
            try
            {
                // 發放點數
                var wallet = await _context.User_Wallet.FirstOrDefaultAsync(w => w.User_Id == userId);
                if (wallet != null)
                {
                    wallet.User_Point += reward.Points;

                    var history = new WalletHistory
                    {
                        UserID = userId,
                        ChangeType = "SignIn",
                        PointsChanged = reward.Points,
                        ItemCode = "SIGNIN_REWARD",
                        Description = $"簽到獎勵 (連續 {(reward.ConsecutiveDayBonus > 0 ? "+" + reward.ConsecutiveDayBonus : "")}天加成)",
                        ChangeTime = DateTime.UtcNow
                    };
                    _context.WalletHistory.Add(history);
                }

                // 發放經驗給寵物
                var pet = await _context.Pet.FirstOrDefaultAsync(p => p.UserID == userId);
                if (pet != null)
                {
                    pet.Experience += reward.Experience;
                }

                // 發放優惠券
                if (!string.IsNullOrEmpty(reward.CouponCode))
                {
                    // 查找對應的優惠券類型
                    var couponType = await _context.CouponType.FirstOrDefaultAsync(ct => ct.IsActive);
                    if (couponType != null)
                    {
                        var coupon = new Coupon
                        {
                            UserID = userId,
                            CouponTypeID = couponType.CouponTypeID,
                            CouponCode = $"{reward.CouponCode}_{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                            IssueTime = DateTime.UtcNow,
                            ExpiryTime = DateTime.UtcNow.AddDays(30),
                            IsUsed = false
                        };
                        _context.Coupon.Add(coupon);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<SignInRewardRule>> GetSignInRewardRulesAsync()
        {
            // 預設簽到獎勵規則
            return new List<SignInRewardRule>
            {
                new SignInRewardRule { DayNumber = 1, Points = 10, Experience = 5, HasCoupon = false },
                new SignInRewardRule { DayNumber = 3, Points = 15, Experience = 8, HasCoupon = false },
                new SignInRewardRule { DayNumber = 7, Points = 25, Experience = 15, HasCoupon = true },
                new SignInRewardRule { DayNumber = 14, Points = 40, Experience = 25, HasCoupon = true },
                new SignInRewardRule { DayNumber = 30, Points = 100, Experience = 50, HasCoupon = true }
            };
        }

        // 簽到統計
        public async Task<SignInStatistics> GetUserSignInStatisticsAsync(int userId)
        {
            var allSignIns = await _context.User_SignInStats
                .Where(s => s.UserID == userId)
                .OrderByDescending(s => s.SignInTime)
                .ToListAsync();

            var stats = new SignInStatistics
            {
                TotalSignIns = allSignIns.Count,
                CurrentStreak = await GetConsecutiveDaysAsync(userId),
                LastSignInDate = await GetLastSignInDateAsync(userId),
                TotalPointsEarned = allSignIns.Sum(s => s.PointsGained),
                TotalExpEarned = allSignIns.Sum(s => s.ExpGained),
                TotalCouponsEarned = allSignIns.Count(s => !string.IsNullOrEmpty(s.CouponGained))
            };

            // 計算最長連續簽到
            stats.LongestStreak = allSignIns.Any()
                ? allSignIns.Max(s => s.ConsecutiveDays)
                : 0;

            return stats;
        }

        public async Task<SignInStatistics> GetGlobalSignInStatisticsAsync()
        {
            var allSignIns = await _context.User_SignInStats.ToListAsync();

            return new SignInStatistics
            {
                TotalSignIns = allSignIns.Count,
                CurrentStreak = 0, // 全域統計不適用
                LongestStreak = allSignIns.Any() ? allSignIns.Max(s => s.ConsecutiveDays) : 0,
                TotalPointsEarned = allSignIns.Sum(s => s.PointsGained),
                TotalExpEarned = allSignIns.Sum(s => s.ExpGained),
                TotalCouponsEarned = allSignIns.Count(s => !string.IsNullOrEmpty(s.CouponGained)),
                LastSignInDate = allSignIns.Any() ? allSignIns.Max(s => s.SignInTime) : null
            };
        }

        public async Task<Dictionary<string, int>> GetSignInTrendDataAsync(int days = 30)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var signIns = await _context.User_SignInStats
                .Where(s => s.SignInTime >= startDate)
                .GroupBy(s => s.SignInTime.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            return signIns.ToDictionary(
                x => x.Date.ToString("yyyy-MM-dd"),
                x => x.Count
            );
        }

        public async Task<IEnumerable<UserSignInRanking>> GetSignInLeaderboardAsync(int count = 10)
        {
            var rankings = await _context.User_SignInStats
                .Include(s => s.Users)
                .GroupBy(s => s.UserID)
                .Select(g => new UserSignInRanking
                {
                    UserId = g.Key,
                    UserName = g.First().Users.User_Account,
                    SignInCount = g.Count(),
                    ConsecutiveDays = g.Max(s => s.ConsecutiveDays),
                    TotalPoints = g.Sum(s => s.PointsGained)
                })
                .OrderByDescending(r => r.SignInCount)
                .ThenByDescending(r => r.ConsecutiveDays)
                .Take(count)
                .ToListAsync();

            return rankings;
        }

        // 簽到規則管理
        public async Task<IEnumerable<SignInRule>> GetAllSignInRulesAsync()
        {
            return await _context.SignInRules
                .OrderBy(r => r.ConsecutiveDays)
                .ToListAsync();
        }

        public async Task<SignInRule?> GetSignInRuleByIdAsync(int ruleId)
        {
            return await _context.SignInRules
                .FirstOrDefaultAsync(r => r.Id == ruleId);
        }

        public async Task<bool> CreateSignInRuleAsync(SignInRule rule)
        {
            try
            {
                rule.CreatedAt = DateTime.UtcNow;
                _context.SignInRules.Add(rule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSignInRuleAsync(SignInRule rule)
        {
            try
            {
                rule.UpdatedAt = DateTime.UtcNow;
                _context.SignInRules.Update(rule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteSignInRuleAsync(int ruleId)
        {
            try
            {
                var rule = await GetSignInRuleByIdAsync(ruleId);
                if (rule == null) return false;

                _context.SignInRules.Remove(rule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleSignInRuleStatusAsync(int ruleId)
        {
            try
            {
                var rule = await GetSignInRuleByIdAsync(ruleId);
                if (rule == null) return false;

                rule.IsActive = !rule.IsActive;
                rule.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
