using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class PetService : IPetService
    {
        private readonly GameSpacedatabaseContext _context;

        public PetService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // Pet 基本 CRUD
        public async Task<IEnumerable<Pet>> GetAllPetsAsync()
        {
            return await _context.Pets
                .Include(p => p.Users)
                .OrderByDescending(p => p.Level)
                .ToListAsync();
        }

        public async Task<Pet?> GetPetByIdAsync(int petId)
        {
            return await _context.Pets
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.PetID == petId);
        }

        public async Task<Pet?> GetPetByUserIdAsync(int userId)
        {
            return await _context.Pets
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.UserID == userId);
        }

        public async Task<bool> CreatePetAsync(Pet pet)
        {
            try
            {
                _context.Pets.Add(pet);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdatePetAsync(Pet pet)
        {
            try
            {
                _context.Pets.Update(pet);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeletePetAsync(int petId)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                _context.Pets.Remove(pet);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Pet 狀態管理
        public async Task<bool> UpdatePetStatsAsync(int petId, int hunger, int mood, int stamina, int cleanliness, int health)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                pet.Hunger = Math.Clamp(hunger, 0, 100);
                pet.Mood = Math.Clamp(mood, 0, 100);
                pet.Stamina = Math.Clamp(stamina, 0, 100);
                pet.Cleanliness = Math.Clamp(cleanliness, 0, 100);
                pet.Health = Math.Clamp(health, 0, 100);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> FeedPetAsync(int petId, int hungerIncrease)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                pet.Hunger = Math.Min(100, pet.Hunger + hungerIncrease);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PlayWithPetAsync(int petId, int moodIncrease)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                pet.Mood = Math.Min(100, pet.Mood + moodIncrease);
                pet.Stamina = Math.Max(0, pet.Stamina - 10); // 玩耍消耗體力
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CleanPetAsync(int petId, int cleanlinessIncrease)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                pet.Cleanliness = Math.Min(100, pet.Cleanliness + cleanlinessIncrease);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RestPetAsync(int petId, int staminaIncrease)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                pet.Stamina = Math.Min(100, pet.Stamina + staminaIncrease);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Pet 升級系統
        public async Task<bool> AddExperienceAsync(int petId, int exp)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                pet.Experience += exp;

                // 檢查是否可以升級
                var requiredExp = await GetRequiredExpForLevelAsync(pet.Level + 1);
                while (pet.Experience >= requiredExp && requiredExp > 0)
                {
                    await LevelUpPetAsync(petId);
                    pet = await GetPetByIdAsync(petId);
                    if (pet == null) break;
                    requiredExp = await GetRequiredExpForLevelAsync(pet.Level + 1);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LevelUpPetAsync(int petId)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                var requiredExp = await GetRequiredExpForLevelAsync(pet.Level + 1);
                if (pet.Experience < requiredExp) return false;

                pet.Level++;
                pet.LevelUpTime = DateTime.UtcNow;
                pet.Experience -= requiredExp;

                // 獎勵點數
                var pointsReward = pet.Level * 10;
                pet.PointsGained_LevelUp += pointsReward;
                pet.PointsGainedTime_LevelUp = DateTime.UtcNow;

                // 更新使用者錢包
                var wallet = await _context.User_Wallet.FirstOrDefaultAsync(w => w.User_Id == pet.UserID);
                if (wallet != null)
                {
                    wallet.User_Point += pointsReward;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetRequiredExpForLevelAsync(int level)
        {
            // 經驗值公式: Level * 100 + (Level - 1) * 50
            if (level <= 1) return 0;
            return level * 100 + (level - 1) * 50;
        }

        // Pet 外觀管理
        public async Task<bool> ChangeSkinColorAsync(int petId, string colorCode, int pointsCost)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                // 檢查使用者點數
                var wallet = await _context.User_Wallet.FirstOrDefaultAsync(w => w.User_Id == pet.UserID);
                if (wallet == null || wallet.User_Point < pointsCost) return false;

                // 扣除點數
                wallet.User_Point -= pointsCost;

                // 更新寵物
                pet.SkinColor = colorCode;
                pet.SkinColorChangedTime = DateTime.UtcNow;
                pet.PointsChanged_SkinColor += pointsCost;

                // 記錄歷史
                var history = new WalletHistory
                {
                    UserID = pet.UserID,
                    ChangeType = "Pet",
                    PointsChanged = -pointsCost,
                    ItemCode = $"SKIN_{colorCode}",
                    Description = $"寵物換膚色: {colorCode}",
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistory.Add(history);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ChangeBackgroundColorAsync(int petId, string colorCode, int pointsCost)
        {
            try
            {
                var pet = await GetPetByIdAsync(petId);
                if (pet == null) return false;

                // 檢查使用者點數
                var wallet = await _context.User_Wallet.FirstOrDefaultAsync(w => w.User_Id == pet.UserID);
                if (wallet == null || wallet.User_Point < pointsCost) return false;

                // 扣除點數
                wallet.User_Point -= pointsCost;

                // 更新寵物
                pet.BackgroundColor = colorCode;
                pet.BackgroundColorChangedTime = DateTime.UtcNow;
                pet.PointsChanged_BackgroundColor += pointsCost;

                // 記錄歷史
                var history = new WalletHistory
                {
                    UserID = pet.UserID,
                    ChangeType = "Pet",
                    PointsChanged = -pointsCost,
                    ItemCode = $"BG_{colorCode}",
                    Description = $"寵物換背景: {colorCode}",
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistory.Add(history);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<PetColorOption>> GetAvailableColorsAsync()
        {
            return await _context.PetColorOptions
                .Where(o => o.IsActive)
                .OrderBy(o => o.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<GameSpace.Areas.MiniGame.Models.ViewModels.PetBackgroundOption>> GetAvailableBackgroundsAsync()
        {
            var entities = await _context.PetBackgroundOptions
                .Where(o => o.IsActive)
                .OrderBy(o => o.SortOrder)
                .ToListAsync();

            // 將 Entity 映射為 ViewModel
            return entities.Select(e => new GameSpace.Areas.MiniGame.Models.ViewModels.PetBackgroundOption
            {
                BackgroundId = e.BackgroundOptionId,
                BackgroundName = e.BackgroundName,
                ImageUrl = null, // Entity 中沒有 ImageUrl，可根據 BackgroundCode 生成或留空
                RequiredPoints = 0, // Entity 中沒有此欄位，預設為 0（免費）
                IsDefault = e.SortOrder == 0, // 假設 SortOrder 為 0 的是預設背景
                IsUnlocked = true, // 所有啟用的背景視為已解鎖
                Description = null // Entity 中沒有描述欄位
            }).ToList();
        }

        // Pet 統計
        public async Task<Dictionary<string, int>> GetPetStatsSummaryAsync(int petId)
        {
            var pet = await GetPetByIdAsync(petId);
            if (pet == null) return new Dictionary<string, int>();

            return new Dictionary<string, int>
            {
                { "Level", pet.Level },
                { "Experience", pet.Experience },
                { "Hunger", pet.Hunger },
                { "Mood", pet.Mood },
                { "Stamina", pet.Stamina },
                { "Cleanliness", pet.Cleanliness },
                { "Health", pet.Health },
                { "PointsSpentOnSkin", pet.PointsChanged_SkinColor },
                { "PointsSpentOnBackground", pet.PointsChanged_BackgroundColor },
                { "PointsEarnedFromLevelUp", pet.PointsGained_LevelUp }
            };
        }

        public async Task<IEnumerable<Pet>> GetTopLevelPetsAsync(int count = 10)
        {
            return await _context.Pets
                .Include(p => p.Users)
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.Experience)
                .Take(count)
                .ToListAsync();
        }
    }
}

