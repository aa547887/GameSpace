using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.Settings;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IPetRulesService
    {
        // Pet 升級規則
        Task<IEnumerable<PetLevelUpRule>> GetAllLevelUpRulesAsync();
        Task<PetLevelUpRule?> GetLevelUpRuleByLevelAsync(int level);
        Task<bool> CreateLevelUpRuleAsync(PetLevelUpRule rule);
        Task<bool> UpdateLevelUpRuleAsync(PetLevelUpRule rule);
        Task<bool> DeleteLevelUpRuleAsync(int ruleId);
        Task<int> GetRequiredExpForLevelAsync(int level);

        // Pet 膚色變更規則
        Task<IEnumerable<PetSkinColorPointSettings>> GetAllSkinColorSettingsAsync();
        Task<PetSkinColorPointSettings?> GetSkinColorSettingByLevelAsync(int level);
        Task<bool> UpdateSkinColorSettingAsync(PetSkinColorPointSettings setting);
        Task<int> GetSkinColorCostForLevelAsync(int level);

        // Pet 背景變更規則
        Task<IEnumerable<PetBackgroundPointSettings>> GetAllBackgroundSettingsAsync();
        Task<PetBackgroundPointSettings?> GetBackgroundSettingByLevelAsync(int level);
        Task<bool> UpdateBackgroundSettingAsync(PetBackgroundPointSettings setting);
        Task<int> GetBackgroundCostForLevelAsync(int level);

        // Pet 互動獎勵規則
        Task<IEnumerable<PetInteractionBonusRules>> GetAllInteractionRulesAsync();
        Task<PetInteractionBonusRules?> GetInteractionRuleByTypeAsync(string interactionType);
        Task<bool> CreateInteractionRuleAsync(PetInteractionBonusRules rule);
        Task<bool> UpdateInteractionRuleAsync(PetInteractionBonusRules rule);
        Task<bool> DeleteInteractionRuleAsync(int ruleId);
        Task<bool> ToggleInteractionRuleAsync(int ruleId);

        // Pet 顏色選項管理
        Task<IEnumerable<PetColorOption>> GetAllColorOptionsAsync();
        Task<IEnumerable<PetColorOption>> GetActiveColorOptionsAsync();
        Task<bool> CreateColorOptionAsync(PetColorOption option);
        Task<bool> UpdateColorOptionAsync(PetColorOption option);
        Task<bool> DeleteColorOptionAsync(int optionId);
        Task<bool> ToggleColorOptionAsync(int optionId);
    }

    public class PetLevelUpRule
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public int RequiredExp { get; set; }
        public int PointsReward { get; set; }
        public string? UnlockFeature { get; set; }
        public bool IsActive { get; set; }

        // 額外屬性以支援控制器和驗證服務
        /// <summary>
        /// 所需經驗值（與 RequiredExp 同義，用於向後兼容）
        /// </summary>
        public int ExperienceRequired
        {
            get => RequiredExp;
            set => RequiredExp = value;
        }

        /// <summary>
        /// 經驗值獎勵（升級後獲得的額外經驗值）
        /// </summary>
        public int ExpReward { get; set; }

        /// <summary>
        /// 備註（與 UnlockFeature 同義，用於向後兼容）
        /// </summary>
        public string? Remarks
        {
            get => UnlockFeature;
            set => UnlockFeature = value;
        }
    }
}

