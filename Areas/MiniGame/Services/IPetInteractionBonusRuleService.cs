using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物互動狀態增益規則服務介面
    /// </summary>
    public interface IPetInteractionBonusRuleService
    {
        /// <summary>
        /// 取得所有互動狀態增益規則
        /// </summary>
        Task<List<PetInteractionBonusRule>> GetAllAsync();

        /// <summary>
        /// 取得啟用的互動狀態增益規則
        /// </summary>
        Task<List<PetInteractionBonusRule>> GetActiveAsync();

        /// <summary>
        /// 根據ID取得互動狀態增益規則
        /// </summary>
        Task<PetInteractionBonusRule?> GetByIdAsync(int id);

        /// <summary>
        /// 根據互動類型取得規則
        /// </summary>
        Task<PetInteractionBonusRule?> GetByInteractionTypeAsync(string interactionType);

        /// <summary>
        /// 建立新的互動狀態增益規則
        /// </summary>
        Task<bool> CreateAsync(PetInteractionBonusRuleCreateViewModel model);

        /// <summary>
        /// 更新互動狀態增益規則
        /// </summary>
        Task<bool> UpdateAsync(PetInteractionBonusRuleViewModel model);

        /// <summary>
        /// 刪除互動狀態增益規則
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// 切換規則啟用狀態
        /// </summary>
        Task<bool> ToggleStatusAsync(int id);

        /// <summary>
        /// 搜尋互動狀態增益規則
        /// </summary>
        Task<(List<PetInteractionBonusRuleListViewModel> Items, int TotalCount)> SearchAsync(PetInteractionBonusRuleSearchViewModel searchModel);

        /// <summary>
        /// 取得統計資料
        /// </summary>
        Task<object> GetStatisticsAsync();
    }
}
