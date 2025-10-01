using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物等級經驗值設定服務介面
    /// </summary>
    public interface IPetLevelExperienceSettingService
    {
        /// <summary>
        /// 取得所有等級經驗值設定
        /// </summary>
        /// <param name="searchModel">搜尋條件</param>
        /// <returns>等級經驗值設定列表</returns>
        Task<(List<PetLevelExperienceSettingListViewModel> Items, int TotalCount)> GetAllAsync(PetLevelExperienceSettingSearchViewModel searchModel);

        /// <summary>
        /// 根據ID取得等級經驗值設定
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>等級經驗值設定</returns>
        Task<PetLevelExperienceSetting?> GetByIdAsync(int id);

        /// <summary>
        /// 建立等級經驗值設定
        /// </summary>
        /// <param name="model">建立模型</param>
        /// <param name="createdBy">建立者</param>
        /// <returns>是否成功</returns>
        Task<bool> CreateAsync(PetLevelExperienceSettingCreateViewModel model, string? createdBy = null);

        /// <summary>
        /// 更新等級經驗值設定
        /// </summary>
        /// <param name="model">編輯模型</param>
        /// <param name="updatedBy">更新者</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateAsync(PetLevelExperienceSettingEditViewModel model, string? updatedBy = null);

        /// <summary>
        /// 刪除等級經驗值設定
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// 切換啟用狀態
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <param name="updatedBy">更新者</param>
        /// <returns>是否成功</returns>
        Task<bool> ToggleStatusAsync(int id, string? updatedBy = null);

        /// <summary>
        /// 檢查等級是否已存在
        /// </summary>
        /// <param name="level">等級</param>
        /// <param name="excludeId">排除的ID（編輯時使用）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsLevelExistsAsync(int level, int? excludeId = null);

        /// <summary>
        /// 取得等級統計資料
        /// </summary>
        /// <returns>統計資料</returns>
        Task<object> GetStatisticsAsync();
    }
}
