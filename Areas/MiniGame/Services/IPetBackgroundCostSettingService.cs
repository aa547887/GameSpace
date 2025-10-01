using Areas.MiniGame.Models;
using Areas.MiniGame.Models.ViewModels;

namespace Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物換背景所需點數設定服務介面
    /// </summary>
    public interface IPetBackgroundCostSettingService
    {
        /// <summary>
        /// 取得所有寵物換背景所需點數設定
        /// </summary>
        Task<List<PetBackgroundCostSetting>> GetAllAsync();

        /// <summary>
        /// 取得分頁的寵物換背景所需點數設定
        /// </summary>
        Task<PetBackgroundCostSettingViewModels.IndexViewModel> GetPagedAsync(
            int page = 1, 
            int pageSize = 10, 
            string? searchTerm = null, 
            bool? isActive = null);

        /// <summary>
        /// 根據ID取得寵物換背景所需點數設定
        /// </summary>
        Task<PetBackgroundCostSetting?> GetByIdAsync(int id);

        /// <summary>
        /// 建立新的寵物換背景所需點數設定
        /// </summary>
        Task<bool> CreateAsync(PetBackgroundCostSettingViewModels.CreateViewModel model);

        /// <summary>
        /// 更新寵物換背景所需點數設定
        /// </summary>
        Task<bool> UpdateAsync(PetBackgroundCostSettingViewModels.EditViewModel model);

        /// <summary>
        /// 刪除寵物換背景所需點數設定
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// 檢查設定名稱是否已存在
        /// </summary>
        Task<bool> SettingNameExistsAsync(string settingName, int? excludeId = null);

        /// <summary>
        /// 取得啟用的寵物換背景所需點數設定
        /// </summary>
        Task<List<PetBackgroundCostSetting>> GetActiveSettingsAsync();
    }
}
