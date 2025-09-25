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
                .Include(u => u.User)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<List<UserWallet>> GetAllUserPointsAsync()
        {
            return await _context.UserWallets
                .Include(u => u.User)
                .ToListAsync();
        }

        public async Task<bool> UpdateUserPointsAsync(int userId, int points)
        {
            var wallet = await _context.UserWallets.FirstOrDefaultAsync(u => u.UserId == userId);

            if (wallet != null)
            {
                wallet.UserPoint = points;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<PagedResult<UserWallet>> QueryUserPointsAsync(CouponQueryModel query)
        {
            var queryable = _context.UserWallets
                .Include(u => u.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(u => u.User.UserName.Contains(query.SearchTerm));
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<UserWallet>
            {
                Items = items,
                Page = query.PageNumber,
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
            var wallet = await _context.UserWallets.FirstOrDefaultAsync(u => u.UserId == userId);

            if (wallet != null)
            {
                wallet.UserPoint += points;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        // 商城優惠券系統
        public async Task<List<UserCouponReadModel>> GetUserCouponsAsync(int userId)
        {
            return await _context.Coupons
                .Where(c => c.UserId == userId)
                .Include(c => c.CouponType)
                .Include(c => c.User)
                .Select(c => new UserCouponReadModel
                {
                    UserId = c.UserId,
                    UserName = c.User.UserName,
                    CouponTypeId = c.CouponTypeId,
                    CouponTypeName = c.CouponType.Name,
                    Quantity = 1,
                    LastUpdated = c.AcquiredTime
                })
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
                    AcquiredTime = DateTime.Now,
                    CouponCode = $"CPN-{DateTime.Now:yyyyMM}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}"
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

        public async Task<PagedResult<UserCouponReadModel>> QueryUserCouponsAsync(CouponQueryModel query)
        {
            var queryable = _context.Coupons
                .Include(c => c.User)
                .Include(c => c.CouponType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(c => c.User.UserName.Contains(query.SearchTerm));
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new UserCouponReadModel
                {
                    UserId = c.UserId,
                    UserName = c.User.UserName,
                    CouponTypeId = c.CouponTypeId,
                    CouponTypeName = c.CouponType.Name,
                    Quantity = 1,
                    LastUpdated = c.AcquiredTime
                })
                .ToListAsync();

            return new PagedResult<UserCouponReadModel>
            {
                Items = items,
                Page = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        // 電子優惠券系統
        public async Task<List<Evoucher>> GetUserEVouchersAsync(int userId)
        {
            return await _context.Evouchers
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> AddUserEVoucherAsync(int userId, int EvoucherTypeId, int quantity = 1)
        {
            for (int i = 0; i < quantity; i++)
            {
                var evoucher = new Evoucher
                {
                    UserId = userId,
                    EvoucherTypeId = EvoucherTypeId,
                    IsUsed = false,
                    AcquiredTime = DateTime.Now,
                    EvoucherCode = $"EV-{EvoucherTypeId}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}"
                };
                _context.Evouchers.Add(evoucher);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveUserEVoucherAsync(int evoucherId)
        {
            var evoucher = await _context.Evouchers.FindAsync(evoucherId);
            if (evoucher != null)
            {
                _context.Evouchers.Remove(evoucher);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<List<EvoucherType>> GetEVoucherTypesAsync()
        {
            return await _context.EvoucherTypes.ToListAsync();
        }

        public async Task<bool> IssueEVoucherToUserAsync(int userId, int EvoucherTypeId, int quantity)
        {
            return await AddUserEVoucherAsync(userId, EvoucherTypeId, quantity);
        }

        public async Task<bool> RemoveEVoucherFromUserAsync(int userId, int EvoucherTypeId)
        {
            var evouchers = await _context.Evouchers
                .Where(e => e.UserId == userId && e.EvoucherTypeId == EvoucherTypeId && !e.IsUsed)
                .ToListAsync();

            if (evouchers.Any())
            {
                _context.Evouchers.RemoveRange(evouchers);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<PagedResult<Evoucher>> QueryUserEVouchersAsync(EVoucherQueryModel query)
        {
            var queryable = _context.Evouchers
                .Include(e => e.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(e => e.User.UserName.Contains(query.SearchTerm));
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Evoucher>
            {
                Items = items,
                Page = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        // 簽到系統
        public async Task<List<UserSignInStat>> GetUserSignInRecordsAsync(int userId)
        {
            return await _context.UserSignInStats
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SignTime)
                .ToListAsync();
        }

        public async Task<bool> AddSignInRecordAsync(int userId, DateTime signInDate)
        {
            var signIn = new UserSignInStat
            {
                UserId = userId,
                SignTime = signInDate
            };
            _context.UserSignInStats.Add(signIn);
            return await _context.SaveChangesAsync() > 0;
        }

        
        public async Task<bool> RemoveSignInRecordAsync(int signInId)
        {
            var signIn = await _context.UserSignInStats.FirstOrDefaultAsync(s => s.SignInId == signInId);
            if (signIn != null)
            {
                _context.UserSignInStats.Remove(signIn);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
        {
            var signIn = await _context.UserSignInStats.FirstOrDefaultAsync(s => s.UserId == userId && s.SignTime.Date == signInDate.Date);
            if (signIn != null)
            {
                _context.UserSignInStats.Remove(signIn);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<PagedResult<UserSignInStat>> GetSignInStatsAsync()
        {
            var query = _context.UserSignInStats
                .Include(s => s.User)
                .OrderByDescending(s => s.SignTime);

            var totalCount = await query.CountAsync();
            var items = await query.ToListAsync();

            return new PagedResult<UserSignInStat>
            {
                Items = items,
                Page = 1,
                PageSize = totalCount, // 返回所有記錄
                TotalCount = totalCount
            };
        }

        public async Task<SignInRuleReadModel> GetSignInRuleAsync()
        {
            return new SignInRuleReadModel();
        }

        public async Task<bool> AddUserSignInRecordAsync(int userId, DateTime signInDate)
        {
            return await AddSignInRecordAsync(userId, signInDate);
        }

        public async Task<bool> RemoveUserSignInRecordAsync(int userId, DateTime signInDate)
        {
            return await RemoveSignInRecordAsync(userId, signInDate);
        }

        public async Task<GameSpace.Models.User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<List<SignInRecordReadModel>> GetUserSignInHistoryAsync(int userId)
        {
            return await GetUserSignInRecordsAsync(userId);
        }

        // 寵物系統
        public async Task<GameSpace.Models.Pet?> GetUserPetAsync(int userId)
        {
            return await _context.Pets
                .Include(p => p.User)
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
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.UserId == userId);
            if (pet != null)
            {
                pet.PetName = petName;
                pet.Experience = experience;
                pet.Level = level;
                pet.Hunger = hunger;
                pet.Health = health;
                pet.Cleanliness = cleanliness;

                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<List<GameSpace.Models.Pet>> GetPetsAsync(PetQueryModel query)
        {
            return await GetAllPetsAsync();
        }

        public async Task<PetSummary> GetPetSummaryAsync()
        {
            var totalPets = await _context.Pets.CountAsync();
            var averageLevel = await _context.Pets.AverageAsync(p => p.Level);

            return new PetSummary
            {
                TotalPets = totalPets,
                AverageLevel = (int)averageLevel
            };
        }

        public async Task<PetRuleReadModel> GetPetRuleAsync()
        {
            return new PetRuleReadModel();
        }

        public async Task<GameSpace.Models.Pet?> GetPetDetailAsync(int petId)
        {
            return await _context.Pets
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PetId == petId);
        }

        public async Task<bool> UpdatePetDetailsAsync(int petId, PetUpdateModel model)
        {
            _context.Pets.Update(pet);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<PetSkinColorChangeLog>> GetPetSkinColorChangeLogsAsync(PetQueryModel query)
        {
            return new List<PetSkinColorChangeLog>();
        }

        public async Task<List<PetBackgroundColorChangeLog>> GetPetBackgroundColorChangeLogsAsync(PetQueryModel query)
        {
            return new List<PetBackgroundColorChangeLog>();
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
                Result = result
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

            return new GameSummary
            {
                TotalGames = totalGames
            };
        }

        public async Task<GameRuleReadModel> GetGameRuleAsync()
        {
            return new GameRuleReadModel();
        }

        public async Task<GameSpace.Models.MiniGame?> GetGameDetailAsync(int playId)
        {
            return await _context.MiniGames
                .Include(g => g.User)
                .FirstOrDefaultAsync(g => g.PlayId == PlayId);
        }

        // 總覽系統
        public async Task<WalletSummary> GetWalletSummaryAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalPoints = await _context.UserWallets.SumAsync(w => w.UserPoint);

            return new WalletSummary
            {
                TotalUsers = totalUsers,
                TotalPoints = totalPoints
            };
        }

        // 交易記錄
        public async Task<PagedResult<WalletTransaction>> QueryWalletTransactionsAsync(CouponQueryModel query)
        {
            return new PagedResult<WalletTransaction>
            {
                Items = new List<WalletTransaction>(),
                Page = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = 0
            };
        }

        // 規則設定
        public SignInRuleReadModel GetSignInRule()
        {
            return new SignInRuleReadModel();
        }

        public async Task<bool> UpdateSignInRuleAsync(SignInRuleUpdateModel model)
        {
            return true;
        }

        public PetRuleReadModel GetPetRule()
        {
            return new PetRuleReadModel();
        }

        public async Task<bool> UpdatePetRuleAsync(PetRuleUpdateModel model)
        {
            return true;
        }

        public GameRuleReadModel GetGameRule()
        {
            return new GameRuleReadModel();
        }

        public async Task<bool> UpdateGameRuleAsync(GameRuleUpdateModel model)
        {
            return true;
        }
    }
}
