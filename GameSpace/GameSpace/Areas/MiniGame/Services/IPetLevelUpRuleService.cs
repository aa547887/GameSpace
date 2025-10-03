using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物等級提升規則服務介面
    /// </summary>
    public interface IPetLevelUpRuleService
    {
        /// <summary>
        /// 獲取所有等級提升規則
        /// </summary>
        Task<List<PetRuleReadModel>> GetAllRulesAsync();

        /// <summary>
        /// 根據ID獲取規則
        /// </summary>
        Task<PetRuleReadModel?> GetRuleByIdAsync(int ruleId);

        /// <summary>
        /// 創建新規則
        /// </summary>
        Task<bool> CreateRuleAsync(PetRuleReadModel rule);

        /// <summary>
        /// 更新規則
        /// </summary>
        Task<bool> UpdateRuleAsync(PetRuleReadModel rule);

        /// <summary>
        /// 刪除規則
        /// </summary>
        Task<bool> DeleteRuleAsync(int ruleId);

        /// <summary>
        /// 計算寵物升級所需經驗值
        /// </summary>
        Task<int> CalculateExpRequiredAsync(int currentLevel);

        /// <summary>
        /// 驗證規則
        /// </summary>
        Task<(bool IsValid, string ErrorMessage)> ValidateRuleAsync(PetRuleReadModel rule);
    }
}

