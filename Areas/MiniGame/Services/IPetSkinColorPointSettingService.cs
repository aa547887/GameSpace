using Areas.MiniGame.Models;

namespace Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物換色所需點數設定服務介面
    /// </summary>
    public interface IPetSkinColorPointSettingService
    {
        /// <summary>
        /// 取得所有設定
        /// </summary>
        /// <param name="page">頁數</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>設定列表</returns>
        Task<PetSkinColorPointSettingListViewModel> GetAllAsync(int page = 1, int pageSize = 10);

        /// <summary>
        /// 根據ID取得設定
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>設定資料</returns>
        Task<PetSkinColorPointSettingViewModel?> GetByIdAsync(int id);

        /// <summary>
        /// 根據寵物等級取得設定
        /// </summary>
        /// <param name="petLevel">寵物等級</param>
        /// <returns>設定資料</returns>
        Task<PetSkinColorPointSettingViewModel?> GetByPetLevelAsync(int petLevel);

        /// <summary>
        /// 新增設定
        /// </summary>
        /// <param name="model">設定資料</param>
        /// <param name="userId">使用者ID</param>
        /// <returns>是否成功</returns>
        Task<bool> CreateAsync(PetSkinColorPointSettingViewModel model, int userId);

        /// <summary>
        /// 更新設定
        /// </summary>
        /// <param name="model">設定資料</param>
        /// <param name="userId">使用者ID</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateAsync(PetSkinColorPointSettingViewModel model, int userId);

        /// <summary>
        /// 刪除設定
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// 切換啟用狀態
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <param name="userId">使用者ID</param>
        /// <returns>是否成功</returns>
        Task<bool> ToggleEnabledAsync(int id, int userId);
    }
}
