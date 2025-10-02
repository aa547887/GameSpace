using GameSpace.Areas.MiniGame.Models;

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
        Task<IEnumerable<PetColorOptions>> GetAllColorOptionsAsync();
        Task<IEnumerable<PetColorOptions>> GetActiveColorOptionsAsync();
        Task<bool> CreateColorOptionAsync(PetColorOptions option);
        Task<bool> UpdateColorOptionAsync(PetColorOptions option);
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
    }
}
