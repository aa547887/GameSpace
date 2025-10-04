using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using SignInRuleEntity = GameSpace.Models.SignInRule;
using SignInRuleDto = GameSpace.Areas.MiniGame.Services.SignInRule;

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
                var signInLog = new UserSignInStat
                {
                    UserId = userId,
                    SignTime = DateTime.UtcNow,
                    PointsGained = reward.Points,
                    ExpGained = reward.Experience,
                    CouponGained = reward.CouponCode ?? string.Empty
                };
                _context.UserSignInStats.Add(signInLog);

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
            var signedInToday = await _context.UserSignInStats
                .AnyAsync(s => s.UserId == userId && s.SignTime.Date == today);

            return !signedInToday;
        }

        public async Task<DateTime?> GetLastSignInDateAsync(int userId)
        {
            var lastSignIn = await _context.UserSignInStats
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SignTime)
                .FirstOrDefaultAsync();

            return lastSignIn?.SignTime;
        }

        public async Task<int> GetConsecutiveDaysAsync(int userId)
        {
            var signIns = await _context.UserSignInStats
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SignTime)
                .ToListAsync();

            if (!signIns.Any()) return 0;

            int consecutiveDays = 1;
            var currentDate = signIns.First().SignTime.Date;

            for (int i = 1; i < signIns.Count; i++)
            {
                var previousDate = signIns[i].SignTime.Date;
                if (currentDate.AddDays(-1) == previousDate)
                {
                    consecutiveDays++;
                    currentDate = previousDate;
                }
                else
                {
                    break;
                }
            }

            return consecutiveDays;
        }

        // 簽到記錄查詢
        public async Task<IEnumerable<UserSignInStats>> GetSignInHistoryAsync(int userId, int pageNumber = 1, int pageSize = 20)
        {
            var signIns = await _context.UserSignInStats
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SignTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 轉換為ViewModel
            return signIns.Select(s => new UserSignInStats
            {
                StatsID = s.LogId,
                UserID = s.UserId,
                SignTime = s.SignTime,
                PointsEarned = s.PointsGained,
                PetExpEarned = s.ExpGained,
                CouponEarned = string.IsNullOrEmpty(s.CouponGained) ? (int?)null : int.TryParse(s.CouponGained, out var couponId) ? couponId : (int?)null,
                ConsecutiveDays = 0 // 需要動態計算
            });
        }

        public async Task<IEnumerable<UserSignInStats>> GetAllSignInsAsync(int pageNumber = 1, int pageSize = 50)
        {
            var signIns = await _context.UserSignInStats
                .Include(s => s.User)
                .OrderByDescending(s => s.SignTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 轉換為ViewModel
            return signIns.Select(s => new UserSignInStats
            {
                StatsID = s.LogId,
                UserID = s.UserId,
                SignTime = s.SignTime,
                PointsEarned = s.PointsGained,
                PetExpEarned = s.ExpGained,
                CouponEarned = string.IsNullOrEmpty(s.CouponGained) ? (int?)null : int.TryParse(s.CouponGained, out var couponId) ? couponId : (int?)null,
                ConsecutiveDays = 0 // 需要動態計算
            });
        }

        public async Task<IEnumerable<UserSignInStats>> GetSignInsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var signIns = await _context.UserSignInStats
                .Include(s => s.User)
                .Where(s => s.SignTime >= startDate && s.SignTime <= endDate)
                .OrderByDescending(s => s.SignTime)
                .ToListAsync();

            // 轉換為ViewModel
            return signIns.Select(s => new UserSignInStats
            {
                StatsID = s.LogId,
                UserID = s.UserId,
                SignTime = s.SignTime,
                PointsEarned = s.PointsGained,
                PetExpEarned = s.ExpGained,
                CouponEarned = string.IsNullOrEmpty(s.CouponGained) ? (int?)null : int.TryParse(s.CouponGained, out var couponId) ? couponId : (int?)null,
                ConsecutiveDays = 0 // 需要動態計算
            });
        }

        public async Task<UserSignInStats?> GetSignInDetailAsync(int logId)
        {
            var signIn = await _context.UserSignInStats
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.LogId == logId);

            if (signIn == null) return null;

            // 轉換為ViewModel
            return new UserSignInStats
            {
                StatsID = signIn.LogId,
                UserID = signIn.UserId,
                SignTime = signIn.SignTime,
                PointsEarned = signIn.PointsGained,
                PetExpEarned = signIn.ExpGained,
                CouponEarned = string.IsNullOrEmpty(signIn.CouponGained) ? (int?)null : int.TryParse(signIn.CouponGained, out var couponId) ? couponId : (int?)null,
                ConsecutiveDays = await GetConsecutiveDaysAsync(signIn.UserId)
            };
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
                var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId);
                if (wallet != null)
                {
                    wallet.UserPoint += reward.Points;

                    var history = new WalletHistory
                    {
                        UserId = userId,
                        ChangeType = "SignIn",
                        PointsChanged = reward.Points,
                        ItemCode = "SIGNIN_REWARD",
                        Description = $"簽到獎勵 (連續 {(reward.ConsecutiveDayBonus > 0 ? "+" + reward.ConsecutiveDayBonus : "")}天加成)",
                        ChangeTime = DateTime.UtcNow
                    };
                    _context.WalletHistories.Add(history);
                }

                // 發放經驗給寵物
                var pet = await _context.Pets.FirstOrDefaultAsync(p => p.UserId == userId);
                if (pet != null)
                {
                    pet.Experience += reward.Experience;
                }

                // 發放優惠券
                if (!string.IsNullOrEmpty(reward.CouponCode))
                {
                    // 查找對應的優惠券類型
                    var couponType = await _context.CouponTypes.FirstOrDefaultAsync(ct => (ct.ValidFrom <= DateTime.UtcNow && ct.ValidTo >= DateTime.UtcNow));
                    if (couponType != null)
                    {
                        var coupon = new Coupon
                        {
                            UserId = userId,
                            CouponTypeId = couponType.CouponTypeId,
                            CouponCode = $"{reward.CouponCode}_{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                            AcquiredTime = DateTime.UtcNow,
                            IsUsed = false
                        };
                        _context.Coupons.Add(coupon);
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
            var allSignIns = await _context.UserSignInStats
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SignTime)
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

            // 計算最長連續簽到 (需要遍歷所有記錄計算)
            stats.LongestStreak = await GetConsecutiveDaysAsync(userId);

            return stats;
        }

        public async Task<SignInStatistics> GetGlobalSignInStatisticsAsync()
        {
            var allSignIns = await _context.UserSignInStats.ToListAsync();

            return new SignInStatistics
            {
                TotalSignIns = allSignIns.Count,
                CurrentStreak = 0, // 全域統計不適用
                LongestStreak = 0, // 全域統計中最長連續需要針對每個用戶計算
                TotalPointsEarned = allSignIns.Sum(s => s.PointsGained),
                TotalExpEarned = allSignIns.Sum(s => s.ExpGained),
                TotalCouponsEarned = allSignIns.Count(s => !string.IsNullOrEmpty(s.CouponGained)),
                LastSignInDate = allSignIns.Any() ? allSignIns.Max(s => s.SignTime) : null
            };
        }

        public async Task<Dictionary<string, int>> GetSignInTrendDataAsync(int days = 30)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var signIns = await _context.UserSignInStats
                .Where(s => s.SignTime >= startDate)
                .GroupBy(s => s.SignTime.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            return signIns.ToDictionary(
                x => x.Date.ToString("yyyy-MM-dd"),
                x => x.Count
            );
        }

        public async Task<IEnumerable<UserSignInRanking>> GetSignInLeaderboardAsync(int count = 10)
        {
            var userIds = await _context.UserSignInStats
                .Select(s => s.UserId)
                .Distinct()
                .ToListAsync();

            var rankings = new List<UserSignInRanking>();

            foreach (var userId in userIds)
            {
                var userSignIns = await _context.UserSignInStats
                    .Include(s => s.User)
                    .Where(s => s.UserId == userId)
                    .ToListAsync();

                if (userSignIns.Any())
                {
                    rankings.Add(new UserSignInRanking
                    {
                        UserId = userId,
                        UserName = userSignIns.First().User.User_Account,
                        SignInCount = userSignIns.Count,
                        ConsecutiveDays = await GetConsecutiveDaysAsync(userId),
                        TotalPoints = userSignIns.Sum(s => s.PointsGained)
                    });
                }
            }

            return rankings
                .OrderByDescending(r => r.SignInCount)
                .ThenByDescending(r => r.ConsecutiveDays)
                .Take(count);
        }

        // 簽到規則管理
        public async Task<IEnumerable<SignInRule>> GetAllSignInRulesAsync()
        {
            var dbRules = await _context.SignInRules
                .Cast<SignInRuleEntity>()
                .OrderBy(r => r.SignInDay)
                .ToListAsync();

            // Map database entity to DTO
            return dbRules.Select(r => new SignInRule
            {
                Id = r.Id,
                RuleName = r.Description ?? string.Empty,
                Description = r.Description,
                ConsecutiveDays = r.SignInDay,
                PointsReward = r.Points,
                ExpReward = r.Experience,
                CouponTypeId = r.CouponTypeCode,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            });
        }

        public async Task<SignInRule?> GetSignInRuleByIdAsync(int ruleId)
        {
            var dbRule = await _context.SignInRules
                .Cast<SignInRuleEntity>()
                .FirstOrDefaultAsync(r => r.Id == ruleId);

            if (dbRule == null) return null;

            return new SignInRule
            {
                Id = dbRule.Id,
                RuleName = dbRule.Description ?? string.Empty,
                Description = dbRule.Description,
                ConsecutiveDays = dbRule.SignInDay,
                PointsReward = dbRule.Points,
                ExpReward = dbRule.Experience,
                CouponTypeId = dbRule.CouponTypeCode,
                IsActive = dbRule.IsActive,
                CreatedAt = dbRule.CreatedAt,
                UpdatedAt = dbRule.UpdatedAt
            };
        }

        public async Task<bool> CreateSignInRuleAsync(SignInRule rule)
        {
            try
            {
                var dbRule = new SignInRuleEntity
                {
                    SignInDay = rule.ConsecutiveDays,
                    Points = rule.PointsReward,
                    Experience = rule.ExpReward,
                    HasCoupon = !string.IsNullOrEmpty(rule.CouponTypeId),
                    CouponTypeCode = rule.CouponTypeId,
                    IsActive = rule.IsActive,
                    Description = string.IsNullOrWhiteSpace(rule.Description) ? rule.RuleName : rule.Description,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Add(dbRule);
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
                var dbRule = await _context.SignInRules
                    .Cast<SignInRuleEntity>()
                    .FirstOrDefaultAsync(r => r.Id == rule.Id);
                if (dbRule == null) return false;

                dbRule.SignInDay = rule.ConsecutiveDays;
                dbRule.Points = rule.PointsReward;
                dbRule.Experience = rule.ExpReward;
                dbRule.HasCoupon = !string.IsNullOrEmpty(rule.CouponTypeId);
                dbRule.CouponTypeCode = rule.CouponTypeId;
                dbRule.IsActive = rule.IsActive;
                dbRule.Description = string.IsNullOrWhiteSpace(rule.Description) ? rule.RuleName : rule.Description;
                dbRule.UpdatedAt = DateTime.UtcNow;

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
                var dbRule = await _context.SignInRules
                    .Cast<SignInRuleEntity>()
                    .FirstOrDefaultAsync(r => r.Id == ruleId);
                if (dbRule == null) return false;

                _context.Remove(dbRule);
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
                var dbRule = await _context.SignInRules
                    .Cast<SignInRuleEntity>()
                    .FirstOrDefaultAsync(r => r.Id == ruleId);
                if (dbRule == null) return false;

                dbRule.IsActive = !dbRule.IsActive;
                dbRule.UpdatedAt = DateTime.UtcNow;
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


