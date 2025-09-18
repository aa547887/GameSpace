using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class MiniGameAdminService : IMiniGameAdminService
    {
        private readonly GameSpacedatabaseContext _context;

        public MiniGameAdminService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // 會員點數系統
        public async Task<UserWallet?> GetUserPointsAsync(int userId)
        {
            return await _context.UserWallets
                .FirstOrDefaultAsync(up => up.UserId == userId);
        }

        public async Task<List<UserWallet>> GetAllUserPointsAsync()
        {
            return await _context.UserWallets
                .Include(up => up.User)
                .ToListAsync();
        }

        public async Task<bool> UpdateUserPointsAsync(int userId, int points)
        {
            var userWallet = await _context.UserWallets
                .FirstOrDefaultAsync(up => up.UserId == userId);
            
            if (userWallet == null)
            {
                userWallet = new UserWallet
                {
                    UserId = userId,
                    UserPoint = points
                };
                _context.UserWallets.Add(userWallet);
            }
            else
            {
                userWallet.UserPoint = points;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<PaginatedResult<UserWallet>> QueryUserPointsAsync(CouponQueryModel query)
        {
            var queryable = _context.UserWallets.Include(up => up.User).AsQueryable();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(up => up.User.UserName.Contains(query.SearchTerm));
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PaginatedResult<UserWallet>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<List<GameSpace.Models.User>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<bool> AdjustUserPointsAsync(int userId, int points, string reason)
        {
            var userWallet = await _context.UserWallets
                .FirstOrDefaultAsync(up => up.UserId == userId);
            
            if (userWallet == null)
            {
                userWallet = new UserWallet
                {
                    UserId = userId,
                    UserPoint = points
                };
                _context.UserWallets.Add(userWallet);
            }
            else
            {
                userWallet.UserPoint += points;
            }

            // 記錄交易
            var transaction = new WalletTransaction
            {
                UserId = userId,
                TransactionType = points > 0 ? "增加" : "扣除",
                Amount = Math.Abs(points),
                Description = reason,
                TransactionDate = DateTime.Now
            };
            _context.Set<WalletTransaction>().Add(transaction);

            return await _context.SaveChangesAsync() > 0;
        }

        // 商城優惠券系統
        public async Task<List<GameSpace.Models.Coupon>> GetUserCouponsAsync(int userId)
        {
            return await _context.Coupons
                .Include(c => c.CouponType)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> AddUserCouponAsync(int userId, int couponTypeId, int quantity = 1)
        {
            for (int i = 0; i < quantity; i++)
            {
                var coupon = new GameSpace.Models.Coupon
                {
                    UserId = userId,
                    CouponTypeId = couponTypeId,
                    IsUsed = false,
                    CreatedDate = DateTime.Now
                };
                _context.Coupons.Add(coupon);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveUserCouponAsync(int couponId)
        {
            var coupon = await _context.Coupons.FindAsync(couponId);
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<List<GameSpace.Models.CouponType>> GetCouponTypesAsync()
        {
            return await _context.CouponTypes.ToListAsync();
        }

        public async Task<bool> IssueCouponToUserAsync(int userId, int couponTypeId, int quantity)
        {
            return await AddUserCouponAsync(userId, couponTypeId, quantity);
        }

        public async Task<bool> RemoveCouponFromUserAsync(int userId, int couponTypeId)
        {
            var coupons = await _context.Coupons
                .Where(c => c.UserId == userId && c.CouponTypeId == couponTypeId && !c.IsUsed)
                .ToListAsync();
            
            if (coupons.Any())
            {
                _context.Coupons.RemoveRange(coupons);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<PaginatedResult<GameSpace.Models.Coupon>> QueryUserCouponsAsync(CouponQueryModel query)
        {
            var queryable = _context.Coupons
                .Include(c => c.CouponType)
                .Include(c => c.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(c => c.User.UserName.Contains(query.SearchTerm));
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PaginatedResult<GameSpace.Models.Coupon>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        // 電子優惠券系統
        public async Task<List<Evoucher>> GetUserEVouchersAsync(int userId)
        {
            return await _context.EVouchers
                .Include(e => e.EVoucherType)
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> AddUserEVoucherAsync(int userId, int evoucherTypeId, int quantity = 1)
        {
            for (int i = 0; i < quantity; i++)
            {
                var evoucher = new Evoucher
                {
                    UserId = userId,
                    EVoucherTypeId = evoucherTypeId,
                    IsUsed = false,
                    CreatedDate = DateTime.Now
                };
                _context.EVouchers.Add(evoucher);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveUserEVoucherAsync(int evoucherId)
        {
            var evoucher = await _context.EVouchers.FindAsync(evoucherId);
            if (evoucher != null)
            {
                _context.EVouchers.Remove(evoucher);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<List<EvoucherType>> GetEVoucherTypesAsync()
        {
            return await _context.EVoucherTypes.ToListAsync();
        }

        public async Task<bool> IssueEVoucherToUserAsync(int userId, int evoucherTypeId, int quantity)
        {
            return await AddUserEVoucherAsync(userId, evoucherTypeId, quantity);
        }

        public async Task<bool> RemoveEVoucherFromUserAsync(int userId, int evoucherTypeId)
        {
            var evouchers = await _context.EVouchers
                .Where(e => e.UserId == userId && e.EVoucherTypeId == evoucherTypeId && !e.IsUsed)
                .ToListAsync();
            
            if (evouchers.Any())
            {
                _context.EVouchers.RemoveRange(evouchers);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<PaginatedResult<Evoucher>> QueryUserEVouchersAsync(CouponQueryModel query)
        {
            var queryable = _context.EVouchers
                .Include(e => e.EVoucherType)
                .Include(e => e.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(e => e.User.UserName.Contains(query.SearchTerm));
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PaginatedResult<Evoucher>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        // 簽到系統
        public async Task<List<UserSignInStat>> GetUserSignInRecordsAsync(int userId)
        {
            return await _context.UserSignInStats
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SignInDate)
                .ToListAsync();
        }

        public async Task<bool> AddSignInRecordAsync(int userId, DateTime signInDate)
        {
            var signIn = new UserSignInStat
            {
                UserId = userId,
                SignInDate = signInDate
            };
            _context.UserSignInStats.Add(signIn);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveSignInRecordAsync(int signInId)
        {
            var signIn = await _context.UserSignInStats.FindAsync(signInId);
            if (signIn != null)
            {
                _context.UserSignInStats.Remove(signIn);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<List<UserSignInStat>> GetSignInStatsAsync(SignInQueryModel query)
        {
            return await _context.UserSignInStats
                .Include(s => s.User)
                .OrderByDescending(s => s.SignInDate)
                .ToListAsync();
        }

        public async Task<SignInRuleReadModel> GetSignInRuleAsync()
        {
            var rule = await _context.SignInRules.FirstOrDefaultAsync();
            if (rule == null)
            {
                return new GameSpace.Areas.MiniGame.Models.SignInRuleReadModel
                {
                    RuleName = "Daily Sign-in Rule",
                    DailyPoints = 10,
                    WeeklyBonus = 50,
                    MonthlyBonus = 200
                };
            }

            return new GameSpace.Areas.MiniGame.Models.SignInRuleReadModel
            {
                RuleName = rule.RuleName,
                DailyPoints = rule.DailyPoints,
                WeeklyBonus = rule.WeeklyBonus,
                MonthlyBonus = rule.MonthlyBonus
            };
        }

        public async Task<bool> AddUserSignInRecordAsync(int userId, DateTime signInDate)
        {
            return await AddSignInRecordAsync(userId, signInDate);
        }

        public async Task<bool> RemoveUserSignInRecordAsync(int userId, DateTime signInDate)
        {
            var signIn = await _context.UserSignInStats
                .FirstOrDefaultAsync(s => s.UserId == userId && s.SignInDate.Date == signInDate.Date);
            if (signIn != null)
            {
                _context.UserSignInStats.Remove(signIn);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<GameSpace.Models.User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<List<UserSignInStat>> GetUserSignInHistoryAsync(int userId)
        {
            return await GetUserSignInRecordsAsync(userId);
        }

        // 寵物系統
        public async Task<GameSpace.Models.Pet?> GetUserPetAsync(int userId)
        {
            return await _context.Pets
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<List<GameSpace.Models.Pet>> GetAllPetsAsync()
        {
            return await _context.Pets
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task<bool> UpdatePetAsync(int userId, string petName, string color, string background, int experience, int level, int hunger, int happiness, int health, int energy, int cleanliness)
        {
            var pet = await _context.Pets
                .FirstOrDefaultAsync(p => p.UserId == userId);
            
            if (pet == null)
            {
                pet = new GameSpace.Models.Pet
                {
                    UserId = userId,
                    PetName = petName,
                    Color = color,
                    Background = background,
                    Experience = experience,
                    Level = level,
                    Hunger = hunger,
                    Happiness = happiness,
                    Health = health,
                    Energy = energy,
                    Cleanliness = cleanliness
                };
                _context.Pets.Add(pet);
            }
            else
            {
                pet.PetName = petName;
                pet.Color = color;
                pet.Background = background;
                pet.Experience = experience;
                pet.Level = level;
                pet.Hunger = hunger;
                pet.Happiness = happiness;
                pet.Health = health;
                pet.Energy = energy;
                pet.Cleanliness = cleanliness;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<GameSpace.Models.Pet>> GetPetsAsync(PetQueryModel query)
        {
            return await GetAllPetsAsync();
        }

        public async Task<PetSummary> GetPetSummaryAsync()
        {
            var totalPets = await _context.Pets.CountAsync();
            var activePets = await _context.Pets.CountAsync(p => p.Level > 0);
            var averageLevel = await _context.Pets.AverageAsync(p => (double?)p.Level) ?? 0;

            return new PetSummary
            {
                TotalPets = totalPets,
                ActivePets = activePets,
                AverageLevel = averageLevel
            };
        }

        public async Task<PetRuleReadModel> GetPetRuleAsync()
        {
            var rule = await _context.PetRules.FirstOrDefaultAsync();
            if (rule == null)
            {
                return new GameSpace.Areas.MiniGame.Models.PetRuleReadModel
                {
                    RuleName = "Pet System Rule",
                    LevelUpExp = 100,
                    MaxLevel = 50,
                    ColorChangeCost = 50,
                    BackgroundChangeCost = 100
                };
            }

            return new GameSpace.Areas.MiniGame.Models.PetRuleReadModel
            {
                RuleName = rule.RuleName,
                LevelUpExp = rule.LevelUpExp,
                MaxLevel = rule.MaxLevel,
                ColorChangeCost = rule.ColorChangeCost,
                BackgroundChangeCost = rule.BackgroundChangeCost
            };
        }

        public async Task<GameSpace.Models.Pet?> GetPetDetailAsync(int petId)
        {
            return await _context.Pets
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PetId == petId);
        }

        public async Task<bool> UpdatePetDetailsAsync(int petId, PetUpdateModel model)
        {
            var pet = await _context.Pets.FindAsync(petId);
            if (pet == null) return false;
            
            pet.PetName = model.PetName;
            pet.Color = model.Color;
            pet.Background = model.Background;
            pet.Experience = model.Experience;
            pet.Level = model.Level;
            pet.Hunger = model.Hunger;
            pet.Happiness = model.Happiness;
            pet.Health = model.Health;
            pet.Energy = model.Energy;
            pet.Cleanliness = model.Cleanliness;
            
            _context.Pets.Update(pet);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<PetSkinColorChangeLog>> GetPetSkinColorChangeLogsAsync(int petId)
        {
            return await _context.Set<PetSkinColorChangeLog>()
                .Where(l => l.PetId == petId)
                .OrderByDescending(l => l.ChangeDate)
                .ToListAsync();
        }

        public async Task<List<PetBackgroundColorChangeLog>> GetPetBackgroundColorChangeLogsAsync(int petId)
        {
            return await _context.Set<PetBackgroundColorChangeLog>()
                .Where(l => l.PetId == petId)
                .OrderByDescending(l => l.ChangeDate)
                .ToListAsync();
        }

        // 小遊戲系統
        public async Task<List<GameSpace.Models.MiniGame>> GetUserGameRecordsAsync(int userId)
        {
            return await _context.MiniGames
                .Where(g => g.UserId == userId)
                .OrderByDescending(g => g.StartTime)
                .ToListAsync();
        }

        public async Task<bool> AddGameRecordAsync(int userId, DateTime startTime, DateTime? endTime, string result, int pointsReward, int expReward, int couponReward)
        {
            var game = new GameSpace.Models.MiniGame
            {
                UserId = userId,
                StartTime = startTime,
                EndTime = endTime,
                Result = result,
                PointsReward = pointsReward,
                ExpReward = expReward,
                CouponReward = couponReward
            };
            _context.MiniGames.Add(game);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<GameSpace.Models.MiniGame>> GetGameRecordsAsync(GameQueryModel query)
        {
            return await _context.MiniGames
                .Include(g => g.User)
                .OrderByDescending(g => g.StartTime)
                .ToListAsync();
        }

        public async Task<GameSummary> GetGameSummaryAsync()
        {
            var totalGames = await _context.MiniGames.CountAsync();
            var completedGames = await _context.MiniGames.CountAsync(g => g.Result == "win" || g.Result == "lose");
            var averageScore = await _context.MiniGames.AverageAsync(g => (double?)g.PointsReward) ?? 0;

            return new GameSummary
            {
                TotalGames = totalGames,
                CompletedGames = completedGames,
                AverageScore = averageScore
            };
        }

        public async Task<GameRuleReadModel> GetGameRuleAsync()
        {
            var rule = await _context.GameRules.FirstOrDefaultAsync();
            if (rule == null)
            {
                return new GameSpace.Areas.MiniGame.Models.GameRuleReadModel
                {
                    RuleName = "Game System Rule",
                    DailyLimit = 3,
                    MonsterCount = 5,
                    MonsterSpeed = 1.0,
                    WinPoints = 100,
                    WinExp = 50
                };
            }

            return new GameSpace.Areas.MiniGame.Models.GameRuleReadModel
            {
                RuleName = rule.RuleName,
                DailyLimit = rule.DailyLimit,
                MonsterCount = rule.MonsterCount,
                MonsterSpeed = rule.MonsterSpeed,
                WinPoints = rule.WinPoints,
                WinExp = rule.WinExp
            };
        }

        public async Task<GameSpace.Models.MiniGame?> GetGameDetailAsync(int gameId)
        {
            return await _context.MiniGames
                .Include(g => g.User)
                .FirstOrDefaultAsync(g => g.GameId == gameId);
        }

        // 總覽系統
        public async Task<WalletSummary> GetWalletSummaryAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalPoints = await _context.UserWallets.SumAsync(uw => uw.UserPoint);
            var totalCoupons = await _context.Coupons.CountAsync(c => !c.IsUsed);
            var totalEVouchers = await _context.EVouchers.CountAsync(e => !e.IsUsed);

            return new WalletSummary
            {
                TotalUsers = totalUsers,
                TotalPoints = totalPoints,
                TotalCoupons = totalCoupons,
                TotalEVouchers = totalEVouchers
            };
        }

        // 交易記錄
        public async Task<PaginatedResult<WalletTransaction>> QueryWalletTransactionsAsync(CouponQueryModel query)
        {
            var queryable = _context.Set<WalletTransaction>()
                
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(t => t.UserName.Contains(query.SearchTerm));
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PaginatedResult<WalletTransaction>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        // 規則設定
        public SignInRuleReadModel GetSignInRule()
        {
            return GetSignInRuleAsync().Result;
        }

        public async Task<bool> UpdateSignInRuleAsync(SignInRuleUpdateModel model)
        {
            var rule = await _context.SignInRules.FirstOrDefaultAsync();
            if (rule == null)
            {
                rule = new GameSpace.Models.SignInRule
                {
                    RuleName = model.RuleName,
                    DailyPoints = model.DailyPoints,
                    WeeklyBonus = model.WeeklyBonus,
                    MonthlyBonus = model.MonthlyBonus
                };
                _context.SignInRules.Add(rule);
            }
            else
            {
                rule.RuleName = model.RuleName;
                rule.DailyPoints = model.DailyPoints;
                rule.WeeklyBonus = model.WeeklyBonus;
                rule.MonthlyBonus = model.MonthlyBonus;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public PetRuleReadModel GetPetRule()
        {
            return GetPetRuleAsync().Result;
        }

        public async Task<bool> UpdatePetRuleAsync(PetRuleUpdateModel model)
        {
            var rule = await _context.PetRules.FirstOrDefaultAsync();
            if (rule == null)
            {
                rule = new GameSpace.Models.PetRule
                {
                    RuleName = model.RuleName,
                    LevelUpExp = model.LevelUpExp,
                    MaxLevel = model.MaxLevel,
                    ColorChangeCost = model.ColorChangeCost,
                    BackgroundChangeCost = model.BackgroundChangeCost
                };
                _context.PetRules.Add(rule);
            }
            else
            {
                rule.RuleName = model.RuleName;
                rule.LevelUpExp = model.LevelUpExp;
                rule.MaxLevel = model.MaxLevel;
                rule.ColorChangeCost = model.ColorChangeCost;
                rule.BackgroundChangeCost = model.BackgroundChangeCost;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public GameRuleReadModel GetGameRule()
        {
            return GetGameRuleAsync().Result;
        }

        public async Task<bool> UpdateGameRuleAsync(GameRuleUpdateModel model)
        {
            var rule = await _context.GameRules.FirstOrDefaultAsync();
            if (rule == null)
            {
                rule = new GameSpace.Models.GameRule
                {
                    RuleName = model.RuleName,
                    DailyLimit = model.DailyLimit,
                    MonsterCount = model.MonsterCount,
                    MonsterSpeed = model.MonsterSpeed,
                    WinPoints = model.WinPoints,
                    WinExp = model.WinExp
                };
                _context.GameRules.Add(rule);
            }
            else
            {
                rule.RuleName = model.RuleName;
                rule.DailyLimit = model.DailyLimit;
                rule.MonsterCount = model.MonsterCount;
                rule.MonsterSpeed = model.MonsterSpeed;
                rule.WinPoints = model.WinPoints;
                rule.WinExp = model.WinExp;
            }

            return await _context.SaveChangesAsync() > 0;
        }
    }
}

