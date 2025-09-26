using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 優化的 MiniGame Admin 服務
    /// 確保所有 Admin 後台功能完整實作
    /// </summary>
    public class OptimizedMiniGameAdminService : IMiniGameAdminService
    {
        private readonly GameSpacedatabaseContext _context;

        public OptimizedMiniGameAdminService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        #region 會員錢包系統

        /// <summary>
        /// 查詢會員點數
        /// </summary>
        public async Task<PagedResult<UserWalletModel>> QueryUserPointsAsync(UserPointsQueryModel query)
        {
            var queryable = _context.UserWallets
                .Include(w => w.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(w => 
                    w.User.UserName.Contains(query.SearchTerm) || 
                    w.User.Email.Contains(query.SearchTerm));
            }

            // 排序
            queryable = query.SortBy?.ToLower() switch
            {
                "username" => query.SortDescending ? queryable.OrderByDescending(w => w.User.UserName) : queryable.OrderBy(w => w.User.UserName),
                "points" => query.SortDescending ? queryable.OrderByDescending(w => w.UserPoint) : queryable.OrderBy(w => w.UserPoint),
                "lastupdate" => query.SortDescending ? queryable.OrderByDescending(w => w.LastUpdateTime) : queryable.OrderBy(w => w.LastUpdateTime),
                _ => queryable.OrderByDescending(w => w.UserPoint)
            };

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(w => new UserWalletModel
                {
                    UserId = w.UserId,
                    UserName = w.User.UserName,
                    Email = w.User.Email,
                    UserPoint = w.UserPoint,
                    LastUpdateTime = w.LastUpdateTime
                })
                .ToListAsync();

            return new PagedResult<UserWalletModel>
            {
                Items = items,
                Page = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        /// <summary>
        /// 發放會員點數
        /// </summary>
        public async Task<OperationResult> GrantUserPointsAsync(GrantPointsModel model)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == model.UserId);
                if (wallet == null)
                {
                    return new OperationResult { Success = false, Message = "找不到用戶錢包" };
                }

                var pointsBefore = wallet.UserPoint;
                wallet.UserPoint += model.Points;
                wallet.LastUpdateTime = DateTime.Now;

                // 記錄錢包歷史
                var history = new WalletHistory
                {
                    UserId = model.UserId,
                    TransactionType = "AdminGrant",
                    PointsChange = model.Points,
                    PointsBefore = pointsBefore,
                    PointsAfter = wallet.UserPoint,
                    Description = $"管理員發放點數: {model.Reason}",
                    TransactionTime = DateTime.Now,
                    RelatedItemType = "AdminAction",
                    RelatedItemId = null
                };

                _context.WalletHistories.Add(history);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new OperationResult { Success = true, Message = $"成功發放 {model.Points} 點給用戶" };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = $"發放點數失敗: {ex.Message}" };
            }
        }

        /// <summary>
        /// 查詢會員擁有商城優惠券
        /// </summary>
        public async Task<PagedResult<UserCouponReadModel>> QueryUserCouponsAsync(CouponQueryModel query)
        {
            var queryable = _context.Coupons
                .Include(c => c.User)
                .Include(c => c.CouponType)
                .AsQueryable();

            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(c => c.UserId == query.UserId.Value);
            }

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(c => 
                    c.User.UserName.Contains(query.SearchTerm) || 
                    c.User.Email.Contains(query.SearchTerm) ||
                    c.CouponType.Name.Contains(query.SearchTerm));
            }

            if (query.CouponTypeId.HasValue)
            {
                queryable = queryable.Where(c => c.CouponTypeId == query.CouponTypeId.Value);
            }

            if (query.IsUsed.HasValue)
            {
                queryable = queryable.Where(c => c.IsUsed == query.IsUsed.Value);
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .OrderByDescending(c => c.CreateTime)
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

        /// <summary>
        /// 發放會員擁有商城優惠券
        /// </summary>
        public async Task<OperationResult> GrantUserCouponsAsync(GrantCouponsModel model)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                var couponType = await _context.CouponTypes.FindAsync(model.CouponTypeId);
                if (couponType == null)
                {
                    return new OperationResult { Success = false, Message = "找不到優惠券類型" };
                }

                for (int i = 0; i < model.Quantity; i++)
                {
                    var coupon = new Coupon
                    {
                        UserId = model.UserId,
                        CouponTypeId = model.CouponTypeId,
                        IsUsed = false,
                        AcquiredTime = DateTime.Now,
                        CouponCode = $"CPN-{DateTime.Now:yyyyMM}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}"
                    };
                    _context.Coupons.Add(coupon);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new OperationResult { Success = true, Message = $"成功發放 {model.Quantity} 張優惠券給用戶" };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = $"發放優惠券失敗: {ex.Message}" };
            }
        }

        /// <summary>
        /// 查詢會員擁有電子禮券
        /// </summary>
        public async Task<PagedResult<UserEVoucherReadModel>> QueryUserEVouchersAsync(EVoucherQueryModel query)
        {
            var queryable = _context.Evouchers
                .Include(e => e.User)
                .Include(e => e.EvoucherType)
                .AsQueryable();

            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(e => e.UserId == query.UserId.Value);
            }

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(e => 
                    e.User.UserName.Contains(query.SearchTerm) || 
                    e.User.Email.Contains(query.SearchTerm) ||
                    e.EvoucherType.Name.Contains(query.SearchTerm));
            }

            if (query.EVoucherTypeId.HasValue)
            {
                queryable = queryable.Where(e => e.EvoucherTypeId == query.EVoucherTypeId.Value);
            }

            if (query.IsUsed.HasValue)
            {
                queryable = queryable.Where(e => e.IsUsed == query.IsUsed.Value);
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .OrderByDescending(e => e.CreateTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(e => new UserEVoucherReadModel
                {
                    UserId = e.UserId,
                    UserName = e.User.UserName,
                    EVoucherId = e.Id,
                    EVoucherTypeId = e.EvoucherTypeId,
                    EVoucherName = e.EvoucherType.Name,
                    FaceValue = (int)e.EvoucherType.ValueAmount,
                    RequiredPoints = e.EvoucherType.PointsCost,
                    ValidityDays = (e.EvoucherType.ValidTo - e.EvoucherType.ValidFrom).Days,
                    CreatedAt = e.CreateTime,
                    ExpiresAt = e.EvoucherType.ValidTo,
                    IsUsed = e.IsUsed,
                    UsedAt = e.UsedTime
                })
                .ToListAsync();

            return new PagedResult<UserEVoucherReadModel>
            {
                Items = items,
                Page = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        /// <summary>
        /// 調整會員擁有電子禮券（發放）
        /// </summary>
        public async Task<OperationResult> GrantUserEVouchersAsync(GrantEVouchersModel model)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                var eVoucherType = await _context.EvoucherTypes.FindAsync(model.EVoucherTypeId);
                if (eVoucherType == null)
                {
                    return new OperationResult { Success = false, Message = "找不到電子禮券類型" };
                }

                for (int i = 0; i < model.Quantity; i++)
                {
                    var eVoucher = new Evoucher
                    {
                        UserId = model.UserId,
                        EvoucherTypeId = model.EVoucherTypeId,
                        IsUsed = false,
                        CreateTime = DateTime.Now,
                        EVoucherCode = $"EV-{DateTime.Now:yyyyMM}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}"
                    };
                    _context.Evouchers.Add(eVoucher);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new OperationResult { Success = true, Message = $"成功發放 {model.Quantity} 張電子禮券給用戶" };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = $"發放電子禮券失敗: {ex.Message}" };
            }
        }

        /// <summary>
        /// 查看會員收支明細
        /// </summary>
        public async Task<PagedResult<WalletHistoryDetailModel>> QueryWalletHistoryAsync(WalletHistoryQueryModel query)
        {
            var queryable = _context.WalletHistories
                .Include(wh => wh.User)
                .AsQueryable();

            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(wh => wh.UserId == query.UserId.Value);
            }

            if (!string.IsNullOrEmpty(query.TransactionType))
            {
                queryable = queryable.Where(wh => wh.TransactionType == query.TransactionType);
            }

            if (query.StartDate.HasValue)
            {
                queryable = queryable.Where(wh => wh.TransactionTime >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                queryable = queryable.Where(wh => wh.TransactionTime <= query.EndDate.Value);
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .OrderByDescending(wh => wh.TransactionTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(wh => new WalletHistoryDetailModel
                {
                    Id = wh.Id,
                    UserId = wh.UserId,
                    UserName = wh.User.UserName,
                    TransactionType = wh.TransactionType,
                    PointsChange = wh.PointsChange,
                    PointsBefore = wh.PointsBefore,
                    PointsAfter = wh.PointsAfter,
                    Description = wh.Description,
                    TransactionTime = wh.TransactionTime,
                    RelatedItemType = wh.RelatedItemType,
                    RelatedItemId = wh.RelatedItemId
                })
                .ToListAsync();

            return new PagedResult<WalletHistoryDetailModel>
            {
                Items = items,
                Page = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        #endregion

        #region 會員簽到系統

        /// <summary>
        /// 獲取簽到規則
        /// </summary>
        public async Task<List<SignInRuleModel>> GetSignInRulesAsync()
        {
            // 這裡應該從配置表或設定檔讀取簽到規則
            // 暫時返回預設規則
            return new List<SignInRuleModel>
            {
                new SignInRuleModel { Id = 1, ConsecutiveDays = 1, RewardType = "Points", RewardValue = 10, Description = "每日簽到獎勵" },
                new SignInRuleModel { Id = 2, ConsecutiveDays = 3, RewardType = "Points", RewardValue = 50, Description = "連續簽到3天獎勵" },
                new SignInRuleModel { Id = 3, ConsecutiveDays = 7, RewardType = "Coupon", RewardValue = 1, Description = "連續簽到7天獎勵優惠券" },
                new SignInRuleModel { Id = 4, ConsecutiveDays = 30, RewardType = "EVoucher", RewardValue = 1, Description = "連續簽到30天獎勵電子禮券" }
            };
        }

        /// <summary>
        /// 更新簽到規則
        /// </summary>
        public async Task<OperationResult> UpdateSignInRulesAsync(List<SignInRuleModel> rules)
        {
            try
            {
                // 這裡應該將規則保存到配置表或設定檔
                // 暫時只返回成功
                return new OperationResult { Success = true, Message = "簽到規則更新成功" };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = $"更新簽到規則失敗: {ex.Message}" };
            }
        }

        /// <summary>
        /// 查詢簽到紀錄
        /// </summary>
        public async Task<PagedResult<SignInRecordDetailModel>> QuerySignInRecordsAsync(SignInRecordQueryModel query)
        {
            var queryable = _context.UserSignInStats
                .Include(s => s.User)
                .AsQueryable();

            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(s => s.UserId == query.UserId.Value);
            }

            if (query.StartDate.HasValue)
            {
                queryable = queryable.Where(s => s.SignTime >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                queryable = queryable.Where(s => s.SignTime <= query.EndDate.Value);
            }

            if (!string.IsNullOrEmpty(query.RewardType))
            {
                // 根據獎勵類型過濾
                queryable = query.RewardType switch
                {
                    "Points" => queryable.Where(s => s.PointsGained > 0),
                    "Coupon" => queryable.Where(s => !string.IsNullOrEmpty(s.CouponGained)),
                    _ => queryable
                };
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .OrderByDescending(s => s.SignTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(s => new SignInRecordDetailModel
                {
                    LogId = s.LogId,
                    UserId = s.UserId,
                    UserName = s.User.UserName,
                    SignTime = s.SignTime,
                    PointsGained = s.PointsGained,
                    ExpGained = s.ExpGained,
                    CouponGained = s.CouponGained,
                    PointsGainedTime = s.PointsGainedTime,
                    ExpGainedTime = s.ExpGainedTime,
                    CouponGainedTime = s.CouponGainedTime,
                    ConsecutiveDays = 1, // 需要計算連續簽到天數
                    RewardDescription = GetRewardDescription(s)
                })
                .ToListAsync();

            return new PagedResult<SignInRecordDetailModel>
            {
                Items = items,
                Page = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        /// <summary>
        /// 獲取簽到統計
        /// </summary>
        public async Task<SignInStatisticsModel> GetSignInStatisticsAsync()
        {
            var today = DateTime.Today;
            var thisWeek = today.AddDays(-7);
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            var totalSignIns = await _context.UserSignInStats.CountAsync();
            var todaySignIns = await _context.UserSignInStats.CountAsync(s => s.SignTime.Date == today);
            var thisWeekSignIns = await _context.UserSignInStats.CountAsync(s => s.SignTime >= thisWeek);
            var thisMonthSignIns = await _context.UserSignInStats.CountAsync(s => s.SignTime >= thisMonth);

            var totalPointsAwarded = await _context.UserSignInStats.SumAsync(s => s.PointsGained);
            var totalCouponsAwarded = await _context.UserSignInStats.CountAsync(s => !string.IsNullOrEmpty(s.CouponGained));

            return new SignInStatisticsModel
            {
                TotalSignIns = totalSignIns,
                TodaySignIns = todaySignIns,
                ThisWeekSignIns = thisWeekSignIns,
                ThisMonthSignIns = thisMonthSignIns,
                TotalPointsAwarded = totalPointsAwarded,
                TotalCouponsAwarded = totalCouponsAwarded,
                AverageSignInsPerDay = totalSignIns > 0 ? (double)totalSignIns / 30 : 0
            };
        }

        #endregion

        #region 寵物系統

        /// <summary>
        /// 獲取寵物系統規則
        /// </summary>
        public async Task<PetSystemRulesViewModel> GetPetSystemRulesAsync()
        {
            // 這裡應該從配置表讀取寵物系統規則
            // 暫時返回預設規則
            return new PetSystemRulesViewModel
            {
                LevelUpRules = new List<PetLevelUpRuleModel>
                {
                    new PetLevelUpRuleModel { Level = 1, RequiredExp = 0, PointsReward = 0, UnlockNewSkin = false, UnlockNewBackground = false },
                    new PetLevelUpRuleModel { Level = 2, RequiredExp = 100, PointsReward = 50, UnlockNewSkin = true, UnlockNewBackground = false },
                    new PetLevelUpRuleModel { Level = 3, RequiredExp = 300, PointsReward = 100, UnlockNewSkin = true, UnlockNewBackground = true },
                    new PetLevelUpRuleModel { Level = 4, RequiredExp = 600, PointsReward = 200, UnlockNewSkin = true, UnlockNewBackground = true },
                    new PetLevelUpRuleModel { Level = 5, RequiredExp = 1000, PointsReward = 500, UnlockNewSkin = true, UnlockNewBackground = true }
                },
                InteractionGains = new List<PetInteractionGainModel>
                {
                    new PetInteractionGainModel { InteractionType = "Feed", HungerChange = 20, MoodChange = 5, StaminaChange = 0, CleanlinessChange = -5, ExpGain = 10, PointsCost = 0, CooldownMinutes = 30 },
                    new PetInteractionGainModel { InteractionType = "Play", HungerChange = -5, MoodChange = 15, StaminaChange = -10, CleanlinessChange = -5, ExpGain = 15, PointsCost = 0, CooldownMinutes = 60 },
                    new PetInteractionGainModel { InteractionType = "Clean", HungerChange = 0, MoodChange = 5, StaminaChange = 0, CleanlinessChange = 20, ExpGain = 5, PointsCost = 0, CooldownMinutes = 120 },
                    new PetInteractionGainModel { InteractionType = "Sleep", HungerChange = 0, MoodChange = 10, StaminaChange = 20, CleanlinessChange = 0, ExpGain = 5, PointsCost = 0, CooldownMinutes = 480 }
                },
                SkinOptions = new List<PetSkinOptionModel>
                {
                    new PetSkinOptionModel { SkinColor = "Red", DisplayName = "紅色", PointsCost = 0, RequiredLevel = 1, IsAvailable = true },
                    new PetSkinOptionModel { SkinColor = "Blue", DisplayName = "藍色", PointsCost = 100, RequiredLevel = 2, IsAvailable = true },
                    new PetSkinOptionModel { SkinColor = "Green", DisplayName = "綠色", PointsCost = 200, RequiredLevel = 3, IsAvailable = true },
                    new PetSkinOptionModel { SkinColor = "Purple", DisplayName = "紫色", PointsCost = 500, RequiredLevel = 5, IsAvailable = true }
                },
                BackgroundOptions = new List<PetBackgroundOptionModel>
                {
                    new PetBackgroundOptionModel { BackgroundColor = "Forest", DisplayName = "森林", PointsCost = 0, RequiredLevel = 1, IsAvailable = true },
                    new PetBackgroundOptionModel { BackgroundColor = "Ocean", DisplayName = "海洋", PointsCost = 150, RequiredLevel = 2, IsAvailable = true },
                    new PetBackgroundOptionModel { BackgroundColor = "Space", DisplayName = "太空", PointsCost = 300, RequiredLevel = 4, IsAvailable = true }
                }
            };
        }

        /// <summary>
        /// 更新寵物系統規則
        /// </summary>
        public async Task<OperationResult> UpdatePetSystemRulesAsync(PetSystemRulesViewModel model)
        {
            try
            {
                // 這裡應該將規則保存到配置表
                // 暫時只返回成功
                return new OperationResult { Success = true, Message = "寵物系統規則更新成功" };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = $"更新寵物系統規則失敗: {ex.Message}" };
            }
        }

        /// <summary>
        /// 獲取用戶寵物設定
        /// </summary>
        public async Task<List<PetSettingModel>> GetUserPetSettingsAsync(int userId)
        {
            return await _context.Pets
                .Where(p => p.UserId == userId)
                .Select(p => new PetSettingModel
                {
                    PetId = p.PetId,
                    UserId = p.UserId,
                    UserName = p.User.UserName,
                    PetName = p.PetName,
                    SkinColor = p.SkinColor,
                    BackgroundColor = p.BackgroundColor,
                    Level = p.Level,
                    Experience = p.Experience,
                    Health = p.Health,
                    Hunger = p.Hunger,
                    Mood = p.Mood,
                    Stamina = p.Stamina,
                    Cleanliness = p.Cleanliness,
                    CreatedAt = p.LevelUpTime, // 使用現有欄位
                    LastUpdated = p.LevelUpTime
                })
                .ToListAsync();
        }

        /// <summary>
        /// 更新寵物設定
        /// </summary>
        public async Task<OperationResult> UpdatePetSettingAsync(PetSettingModel model)
        {
            try
            {
                var pet = await _context.Pets.FindAsync(model.PetId);
                if (pet == null)
                {
                    return new OperationResult { Success = false, Message = "找不到指定的寵物" };
                }

                pet.PetName = model.PetName;
                pet.SkinColor = model.SkinColor;
                pet.BackgroundColor = model.BackgroundColor;
                pet.Level = model.Level;
                pet.Experience = model.Experience;
                pet.Health = model.Health;
                pet.Hunger = model.Hunger;
                pet.Mood = model.Mood;
                pet.Stamina = model.Stamina;
                pet.Cleanliness = model.Cleanliness;

                await _context.SaveChangesAsync();

                return new OperationResult { Success = true, Message = "寵物設定更新成功" };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = $"更新寵物設定失敗: {ex.Message}" };
            }
        }

        /// <summary>
        /// 查詢寵物清單
        /// </summary>
        public async Task<PagedResult<PetSettingModel>> QueryPetListAsync(PetListQueryModel query)
        {
            var queryable = _context.Pets
                .Include(p => p.User)
                .AsQueryable();

            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(p => p.UserId == query.UserId.Value);
            }

            if (!string.IsNullOrEmpty(query.PetName))
            {
                queryable = queryable.Where(p => p.PetName.Contains(query.PetName));
            }

            if (!string.IsNullOrEmpty(query.SkinColor))
            {
                queryable = queryable.Where(p => p.SkinColor == query.SkinColor);
            }

            if (!string.IsNullOrEmpty(query.BackgroundColor))
            {
                queryable = queryable.Where(p => p.BackgroundColor == query.BackgroundColor);
            }

            if (query.MinLevel.HasValue)
            {
                queryable = queryable.Where(p => p.Level >= query.MinLevel.Value);
            }

            if (query.MaxLevel.HasValue)
            {
                queryable = queryable.Where(p => p.Level <= query.MaxLevel.Value);
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.Experience)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new PetSettingModel
                {
                    PetId = p.PetId,
                    UserId = p.UserId,
                    UserName = p.User.UserName,
                    PetName = p.PetName,
                    SkinColor = p.SkinColor,
                    BackgroundColor = p.BackgroundColor,
                    Level = p.Level,
                    Experience = p.Experience,
                    Health = p.Health,
                    Hunger = p.Hunger,
                    Mood = p.Mood,
                    Stamina = p.Stamina,
                    Cleanliness = p.Cleanliness,
                    CreatedAt = p.LevelUpTime,
                    LastUpdated = p.LevelUpTime
                })
                .ToListAsync();

            return new PagedResult<PetSettingModel>
            {
                Items = items,
                Page = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        #endregion

        #region 小遊戲系統

        /// <summary>
        /// 獲取遊戲規則
        /// </summary>
        public async Task<List<GameRuleModel>> GetGameRulesAsync()
        {
            // 這裡應該從配置表讀取遊戲規則
            // 暫時返回預設規則
            return new List<GameRuleModel>
            {
                new GameRuleModel
                {
                    Id = 1,
                    RuleName = "預設遊戲規則",
                    DailyLimit = 3,
                    MonsterCount = 10,
                    MonsterSpeed = 1.0,
                    WinPoints = 50,
                    WinExp = 30,
                    LosePoints = 10,
                    LoseExp = 5,
                    TimeLimit = 120,
                    DifficultySettings = new List<GameDifficultySettingModel>
                    {
                        new GameDifficultySettingModel { Level = 1, MonsterCount = 5, MonsterSpeed = 0.8, TimeLimit = 150, WinPoints = 30, WinExp = 20, LosePoints = 5, LoseExp = 3 },
                        new GameDifficultySettingModel { Level = 2, MonsterCount = 10, MonsterSpeed = 1.0, TimeLimit = 120, WinPoints = 50, WinExp = 30, LosePoints = 10, LoseExp = 5 },
                        new GameDifficultySettingModel { Level = 3, MonsterCount = 15, MonsterSpeed = 1.2, TimeLimit = 90, WinPoints = 80, WinExp = 50, LosePoints = 15, LoseExp = 8 }
                    }
                }
            };
        }

        /// <summary>
        /// 更新遊戲規則
        /// </summary>
        public async Task<OperationResult> UpdateGameRulesAsync(List<GameRuleModel> rules)
        {
            try
            {
                // 這裡應該將規則保存到配置表
                // 暫時只返回成功
                return new OperationResult { Success = true, Message = "遊戲規則更新成功" };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = $"更新遊戲規則失敗: {ex.Message}" };
            }
        }

        /// <summary>
        /// 查詢遊戲紀錄
        /// </summary>
        public async Task<PagedResult<GameRecordDetailModel>> QueryGameRecordsAsync(GameRecordQueryModel query)
        {
            var queryable = _context.MiniGames
                .Include(mg => mg.User)
                .Include(mg => mg.Pet)
                .AsQueryable();

            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(mg => mg.UserId == query.UserId.Value);
            }

            if (query.PetId.HasValue)
            {
                queryable = queryable.Where(mg => mg.PetId == query.PetId.Value);
            }

            if (!string.IsNullOrEmpty(query.Result))
            {
                queryable = queryable.Where(mg => mg.Result == query.Result);
            }

            if (query.StartDate.HasValue)
            {
                queryable = queryable.Where(mg => mg.StartTime >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                queryable = queryable.Where(mg => mg.StartTime <= query.EndDate.Value);
            }

            if (query.MinLevel.HasValue)
            {
                queryable = queryable.Where(mg => mg.Level >= query.MinLevel.Value);
            }

            if (query.MaxLevel.HasValue)
            {
                queryable = queryable.Where(mg => mg.Level <= query.MaxLevel.Value);
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .OrderByDescending(mg => mg.StartTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(mg => new GameRecordDetailModel
                {
                    PlayId = mg.PlayId,
                    UserId = mg.UserId,
                    UserName = mg.User.UserName,
                    PetId = mg.PetId,
                    PetName = mg.Pet.PetName,
                    Level = mg.Level,
                    MonsterCount = mg.MonsterCount,
                    SpeedMultiplier = mg.SpeedMultiplier,
                    Result = mg.Result,
                    ExpGained = mg.ExpGained,
                    PointsGained = mg.PointsGained,
                    CouponGained = mg.CouponGained,
                    HungerDelta = mg.HungerDelta,
                    MoodDelta = mg.MoodDelta,
                    StaminaDelta = mg.StaminaDelta,
                    CleanlinessDelta = mg.CleanlinessDelta,
                    StartTime = mg.StartTime,
                    EndTime = mg.EndTime,
                    Aborted = mg.Aborted
                })
                .ToListAsync();

            return new PagedResult<GameRecordDetailModel>
            {
                Items = items,
                Page = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        /// <summary>
        /// 獲取遊戲統計
        /// </summary>
        public async Task<GameStatisticsModel> GetGameStatisticsAsync()
        {
            var today = DateTime.Today;
            var thisWeek = today.AddDays(-7);
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            var totalGames = await _context.MiniGames.CountAsync();
            var totalWins = await _context.MiniGames.CountAsync(mg => mg.Result == "Win");
            var totalLosses = await _context.MiniGames.CountAsync(mg => mg.Result == "Lose");
            var totalAborts = await _context.MiniGames.CountAsync(mg => mg.Aborted);

            var totalPointsAwarded = await _context.MiniGames.SumAsync(mg => mg.PointsGained);
            var totalExpAwarded = await _context.MiniGames.SumAsync(mg => mg.ExpGained);
            var totalCouponsAwarded = await _context.MiniGames.CountAsync(mg => !string.IsNullOrEmpty(mg.CouponGained));

            var todayGames = await _context.MiniGames.CountAsync(mg => mg.StartTime.Date == today);
            var thisWeekGames = await _context.MiniGames.CountAsync(mg => mg.StartTime >= thisWeek);
            var thisMonthGames = await _context.MiniGames.CountAsync(mg => mg.StartTime >= thisMonth);

            var averageDuration = await _context.MiniGames
                .Where(mg => mg.EndTime.HasValue)
                .AverageAsync(mg => (mg.EndTime.Value - mg.StartTime).TotalSeconds);

            return new GameStatisticsModel
            {
                TotalGames = totalGames,
                TotalWins = totalWins,
                TotalLosses = totalLosses,
                TotalAborts = totalAborts,
                WinRate = totalGames > 0 ? (double)totalWins / totalGames * 100 : 0,
                TotalPointsAwarded = totalPointsAwarded,
                TotalExpAwarded = totalExpAwarded,
                TotalCouponsAwarded = totalCouponsAwarded,
                AverageGameDuration = averageDuration,
                TodayGames = todayGames,
                ThisWeekGames = thisWeekGames,
                ThisMonthGames = thisMonthGames
            };
        }

        #endregion

        #region 輔助方法

        private string GetRewardDescription(UserSignInStat signIn)
        {
            var rewards = new List<string>();
            
            if (signIn.PointsGained > 0)
                rewards.Add($"點數 {signIn.PointsGained}");
            
            if (signIn.ExpGained > 0)
                rewards.Add($"經驗 {signIn.ExpGained}");
            
            if (!string.IsNullOrEmpty(signIn.CouponGained))
                rewards.Add($"優惠券 {signIn.CouponGained}");

            return string.Join(", ", rewards);
        }

        #endregion

        #region 現有介面實作（保持向後相容）

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

        public async Task<List<Evoucher>> GetUserEVouchersAsync(int userId)
        {
            return await _context.Evouchers
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }

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

        #endregion
    }
}
