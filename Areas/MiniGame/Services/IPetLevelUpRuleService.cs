using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IPetLevelUpRuleService
    {
        Task<List<PetLevelUpRule>> GetAllLevelUpRulesAsync();
        Task<List<PetLevelUpRule>> GetActiveLevelUpRulesAsync();
        Task<PetLevelUpRule?> GetLevelUpRuleByIdAsync(int id);
        Task<bool> CreateLevelUpRuleAsync(PetLevelUpRuleCreateViewModel model, int createdBy);
        Task<bool> UpdateLevelUpRuleAsync(PetLevelUpRuleEditViewModel model, int updatedBy);
        Task<bool> DeleteLevelUpRuleAsync(int id);
        Task<bool> ToggleLevelUpRuleStatusAsync(int id);
        Task<List<PetLevelUpRule>> GetLevelUpRulesByLevelRangeAsync(int minLevel, int maxLevel);
        Task<bool> ValidateLevelUpRuleAsync(PetLevelUpRuleCreateViewModel model);
        Task<bool> BulkUpdateLevelUpRulesAsync(List<PetLevelUpRuleEditViewModel> models, int updatedBy);
    }
}
