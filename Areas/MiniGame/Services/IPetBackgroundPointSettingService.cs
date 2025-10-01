using Areas.MiniGame.Models;

namespace Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物換背景所需點數設定服務介面
    /// </summary>
    public interface IPetBackgroundPointSettingService
    {
        /// <summary>
        /// 取得所有設定
        /// </summary>
        /// <param name="page">頁數</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <param name="searchKeyword">搜尋關鍵字</param>
        /// <param name="isEnabledFilter">啟用狀態篩選</param>
        /// <returns>設定列表</returns>
        Task<PetBackgroundPointSettingListViewModel> GetSettingsAsync(int page = 1, int pageSize = 10, string? searchKeyword = null, bool? isEnabledFilter = null);

        /// <summary>
        /// 根據ID取得設定
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>設定資料</returns>
        Task<PetBackgroundPointSettingViewModel?> GetSettingByIdAsync(int id);

        /// <summary>
        /// 根據寵物等級取得設定
        /// </summary>
        /// <param name="petLevel">寵物等級</param>
        /// <returns>設定資料</returns>
        Task<PetBackgroundPointSettingViewModel?> GetSettingByPetLevelAsync(int petLevel);

        /// <summary>
        /// 新增設定
        /// </summary>
        /// <param name="model">設定資料</param>
        /// <param name="createdBy">建立者ID</param>
        /// <returns>是否成功</returns>
        Task<bool> CreateSettingAsync(PetBackgroundPointSettingViewModel model, int createdBy);

        /// <summary>
        /// 更新設定
        /// </summary>
        /// <param name="model">設定資料</param>
        /// <param name="updatedBy">更新者ID</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateSettingAsync(PetBackgroundPointSettingViewModel model, int updatedBy);

        /// <summary>
        /// 刪除設定
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteSettingAsync(int id);

        /// <summary>
        /// 切換啟用狀態
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <param name="updatedBy">更新者ID</param>
        /// <returns>是否成功</returns>
        Task<bool> ToggleEnabledAsync(int id, int updatedBy);

        /// <summary>
        /// 檢查寵物等級是否已存在設定
        /// </summary>
        /// <param name="petLevel">寵物等級</param>
        /// <param name="excludeId">排除的設定ID（用於更新時檢查）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsPetLevelExistsAsync(int petLevel, int? excludeId = null);
    }
}
