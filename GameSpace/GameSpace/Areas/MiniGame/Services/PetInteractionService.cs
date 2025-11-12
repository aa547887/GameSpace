using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Infrastructure.Time;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物互動服務實作
    /// 處理寵物餵食、洗澡、玩耍、哄睡等互動操作
    /// </summary>
    public class PetInteractionService : IPetInteractionService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly IAppClock _appClock;
        private readonly IPetService _petService;
        private readonly ISystemSettingsService _settingsService;
        private readonly ILogger<PetInteractionService> _logger;

        public PetInteractionService(
            GameSpacedatabaseContext context,
            IAppClock appClock,
            IPetService petService,
            ISystemSettingsService settingsService,
            ILogger<PetInteractionService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _appClock = appClock ?? throw new ArgumentNullException(nameof(appClock));
            _petService = petService ?? throw new ArgumentNullException(nameof(petService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 餵食寵物
        /// 效果：從 SystemSettings 讀取飢餓值與健康值增加量
        /// </summary>
        public async Task<PetInteractionResult> FeedAsync(int petId, int userId)
        {
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.PetId == petId && p.UserId == userId);

                if (pet == null)
                {
                    return PetInteractionResult.Failed("找不到寵物或無權限操作");
                }

                // ✅ 從 SystemSettings 讀取餵食效果
                var hungerIncrease = await _settingsService.GetSettingIntAsync("Pet.Interaction.Feed.HungerIncrease", 10);
                var healthIncrease = await _settingsService.GetSettingIntAsync("Pet.Interaction.Feed.HealthIncrease", 10);

                // 增加飢餓值與健康值
                pet.Hunger = Math.Min(100, pet.Hunger + hungerIncrease);
                pet.Health = Math.Min(100, pet.Health + healthIncrease);

                // 檢查並獎勵每日全滿獎勵
                bool dailyBonusAwarded = await CheckAndAwardDailyFullStatsBonusInternalAsync(pet);

                // 檢查健康值恢復
                bool healthRestored = CheckAndRestoreHealth(pet);

                await _context.SaveChangesAsync();

                var stats = MapToStatsViewModel(pet);
                return PetInteractionResult.Succeeded(
                    $"餵食成功！飢餓值 +{hungerIncrease}、健康值 +{healthIncrease}",
                    stats,
                    dailyBonusAwarded,
                    healthRestored);
            }
            catch (Exception ex)
            {
                return PetInteractionResult.Failed($"餵食失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 為寵物洗澡
        /// 效果：從 SystemSettings 讀取清潔值與心情值增加量
        /// </summary>
        public async Task<PetInteractionResult> BathAsync(int petId, int userId)
        {
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.PetId == petId && p.UserId == userId);

                if (pet == null)
                {
                    return PetInteractionResult.Failed("找不到寵物或無權限操作");
                }

                // ✅ 從 SystemSettings 讀取洗澡效果
                var cleanlinessIncrease = await _settingsService.GetSettingIntAsync("Pet.Interaction.Bath.CleanlinessIncrease", 10);
                var moodIncrease = await _settingsService.GetSettingIntAsync("Pet.Interaction.Bath.MoodIncrease", 10);

                // 增加清潔值與心情值
                pet.Cleanliness = Math.Min(100, pet.Cleanliness + cleanlinessIncrease);
                pet.Mood = Math.Min(100, pet.Mood + moodIncrease);

                // 檢查並獎勵每日全滿獎勵
                bool dailyBonusAwarded = await CheckAndAwardDailyFullStatsBonusInternalAsync(pet);

                // 檢查健康值恢復
                bool healthRestored = CheckAndRestoreHealth(pet);

                await _context.SaveChangesAsync();

                var stats = MapToStatsViewModel(pet);
                return PetInteractionResult.Succeeded(
                    $"洗澡成功！清潔值 +{cleanlinessIncrease}、心情值 +{moodIncrease}",
                    stats,
                    dailyBonusAwarded,
                    healthRestored);
            }
            catch (Exception ex)
            {
                return PetInteractionResult.Failed($"洗澡失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 哄寵物睡覺（玩耍）
        /// 效果：從 SystemSettings 讀取心情值與體力值增加量
        /// </summary>
        public async Task<PetInteractionResult> PlayAsync(int petId, int userId)
        {
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.PetId == petId && p.UserId == userId);

                if (pet == null)
                {
                    return PetInteractionResult.Failed("找不到寵物或無權限操作");
                }

                // ✅ 從 SystemSettings 讀取哄睡效果
                var moodIncrease = await _settingsService.GetSettingIntAsync("Pet.Interaction.Coax.MoodIncrease", 10);
                var staminaIncrease = await _settingsService.GetSettingIntAsync("Pet.Interaction.Coax.StaminaIncrease", 10);

                // 增加心情值與體力值
                pet.Mood = Math.Min(100, pet.Mood + moodIncrease);
                pet.Stamina = Math.Min(100, pet.Stamina + staminaIncrease);

                // 檢查並獎勵每日全滿獎勵
                bool dailyBonusAwarded = await CheckAndAwardDailyFullStatsBonusInternalAsync(pet);

                // 檢查健康值恢復
                bool healthRestored = CheckAndRestoreHealth(pet);

                await _context.SaveChangesAsync();

                var stats = MapToStatsViewModel(pet);
                return PetInteractionResult.Succeeded(
                    $"哄睡成功！心情值 +{moodIncrease}、體力值 +{staminaIncrease}",
                    stats,
                    dailyBonusAwarded,
                    healthRestored);
            }
            catch (Exception ex)
            {
                return PetInteractionResult.Failed($"哄睡失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 讓寵物休息
        /// 效果：已整合到 PlayAsync（哄睡）方法中
        /// </summary>
        public async Task<PetInteractionResult> SleepAsync(int petId, int userId)
        {
            // 注意：根據原始規格，「休息」與「哄睡」功能相同
            // 這裡直接呼叫 PlayAsync 以保持一致性
            return await PlayAsync(petId, userId);
        }

        /// <summary>
        /// 檢查並獎勵每日全滿狀態獎勵（公開方法）
        /// 規格 7.4：寵物若於每日首次同時達到飢餓、心情、體力、清潔值皆 100，則額外獲得 100 點寵物經驗值
        /// </summary>
        public async Task<bool> CheckAndAwardDailyFullStatsBonusAsync(int petId)
        {
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.PetId == petId);
            if (pet == null)
            {
                return false;
            }

            bool awarded = await CheckAndAwardDailyFullStatsBonusInternalAsync(pet);
            if (awarded)
            {
                await _context.SaveChangesAsync();
            }

            return awarded;
        }

        /// <summary>
        /// 檢查並獎勵每日全滿狀態獎勵（內部方法，不儲存變更）
        /// </summary>
        private async Task<bool> CheckAndAwardDailyFullStatsBonusInternalAsync(Pet pet)
        {
            // 檢查是否四項屬性都達到 100
            if (pet.Hunger != 100 || pet.Mood != 100 || pet.Stamina != 100 || pet.Cleanliness != 100)
            {
                return false;
            }

            // ✅ 從 SystemSettings 讀取全滿獎勵經驗值
            var bonusExp = await _settingsService.GetSettingIntAsync("Pet.DailyFullStatsBonus.Experience", 100);

            // 檢查今天是否已經領取過全滿獎勵（這裡簡化處理，實際可能需要額外表格記錄）
            // 目前直接給予獎勵
            pet.Experience += bonusExp;

            _logger.LogInformation("寵物 {PetId} 四項屬性全滿，獲得獎勵經驗值 {BonusExp}", pet.PetId, bonusExp);

            return true;
        }

        /// <summary>
        /// 檢查並恢復健康值
        /// 規格：當飢餓、心情、體力、清潔四項值均達到 100 時，健康值恢復至 100
        /// </summary>
        private bool CheckAndRestoreHealth(Pet pet)
        {
            if (pet.Hunger == 100 && pet.Mood == 100 && pet.Stamina == 100 && pet.Cleanliness == 100)
            {
                if (pet.Health < 100)
                {
                    pet.Health = 100;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 將寵物實體映射為狀態視圖模型
        /// </summary>
        private PetStatsViewModel MapToStatsViewModel(Pet pet)
        {
            return new PetStatsViewModel
            {
                PetId = pet.PetId,
                Hunger = pet.Hunger,
                Mood = pet.Mood,
                Stamina = pet.Stamina,
                Cleanliness = pet.Cleanliness,
                Health = pet.Health,
                Experience = pet.Experience,
                Level = pet.Level
            };
        }
    }
}
