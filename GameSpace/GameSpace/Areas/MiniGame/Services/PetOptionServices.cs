using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物顏色選項服務介面
    /// </summary>
    public interface IPetColorOptionService
    {
        /// <summary>
        /// 取得所有顏色選項
        /// </summary>
        Task<List<PetColorOption>> GetAllColorOptionsAsync();

        /// <summary>
        /// 取得啟用的顏色選項
        /// </summary>
        Task<List<PetColorOption>> GetActiveColorOptionsAsync();

        /// <summary>
        /// 根據ID取得顏色選項
        /// </summary>
        Task<PetColorOption?> GetColorOptionByIdAsync(int id);

        /// <summary>
        /// 建立顏色選項
        /// </summary>
        Task<bool> CreateColorOptionAsync(PetColorOptionViewModel model, int createdBy);

        /// <summary>
        /// 更新顏色選項
        /// </summary>
        Task<bool> UpdateColorOptionAsync(PetColorOptionViewModel model, int updatedBy);

        /// <summary>
        /// 刪除顏色選項
        /// </summary>
        Task<bool> DeleteColorOptionAsync(int id);

        /// <summary>
        /// 切換顏色選項啟用狀態
        /// </summary>
        Task<bool> ToggleColorOptionStatusAsync(int id, int updatedBy);

        /// <summary>
        /// 搜尋顏色選項
        /// </summary>
        Task<List<PetColorOption>> SearchColorOptionsAsync(string keyword, bool activeOnly = false);

        /// <summary>
        /// 取得顏色選項統計資料
        /// </summary>
        Task<(int total, int active, int inactive)> GetColorOptionStatisticsAsync();
    }

    /// <summary>
    /// 寵物背景選項服務介面
    /// </summary>
    public interface IPetBackgroundOptionService
    {
        /// <summary>
        /// 取得所有背景選項
        /// </summary>
        Task<List<PetBackgroundOption>> GetAllBackgroundOptionsAsync();

        /// <summary>
        /// 取得啟用的背景選項
        /// </summary>
        Task<List<PetBackgroundOption>> GetActiveBackgroundOptionsAsync();

        /// <summary>
        /// 根據ID取得背景選項
        /// </summary>
        Task<PetBackgroundOption?> GetBackgroundOptionByIdAsync(int id);

        /// <summary>
        /// 建立背景選項
        /// </summary>
        Task<bool> CreateBackgroundOptionAsync(PetBackgroundOptionViewModel model, int createdBy);

        /// <summary>
        /// 更新背景選項
        /// </summary>
        Task<bool> UpdateBackgroundOptionAsync(PetBackgroundOptionViewModel model, int updatedBy);

        /// <summary>
        /// 刪除背景選項
        /// </summary>
        Task<bool> DeleteBackgroundOptionAsync(int id);

        /// <summary>
        /// 切換背景選項啟用狀態
        /// </summary>
        Task<bool> ToggleBackgroundOptionStatusAsync(int id, int updatedBy);

        /// <summary>
        /// 搜尋背景選項
        /// </summary>
        Task<List<PetBackgroundOption>> SearchBackgroundOptionsAsync(string keyword, bool activeOnly = false);

        /// <summary>
        /// 取得背景選項統計資料
        /// </summary>
        Task<(int total, int active, int inactive)> GetBackgroundOptionStatisticsAsync();
    }
}

