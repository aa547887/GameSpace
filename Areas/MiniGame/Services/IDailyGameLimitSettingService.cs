using Areas.MiniGame.Models;

namespace Areas.MiniGame.Services
{
    /// <summary>
    /// 每日遊戲次數限制設定服務介面
    /// </summary>
    public interface IDailyGameLimitSettingService
    {
        /// <summary>
        /// 取得所有設定
        /// </summary>
        Task<IEnumerable<DailyGameLimitSetting>> GetAllAsync();

        /// <summary>
        /// 根據ID取得設定
        /// </summary>
        Task<DailyGameLimitSetting?> GetByIdAsync(int id);

        /// <summary>
        /// 新增設定
        /// </summary>
        Task<DailyGameLimitSetting> CreateAsync(DailyGameLimitSettingFormViewModel model, int userId);

        /// <summary>
        /// 更新設定
        /// </summary>
        Task<bool> UpdateAsync(int id, DailyGameLimitSettingFormViewModel model, int userId);

        /// <summary>
        /// 刪除設定
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// 切換啟用狀態
        /// </summary>
        Task<bool> ToggleStatusAsync(int id);

        /// <summary>
        /// 取得啟用的設定
        /// </summary>
        Task<DailyGameLimitSetting?> GetActiveSettingAsync();

        /// <summary>
        /// 檢查設定名稱是否重複
        /// </summary>
        Task<bool> IsNameDuplicateAsync(string name, int? excludeId = null);
    }
}
