using GameSpace.Areas.MiniGame.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 簽到管理服務介面
    /// </summary>
    public interface ISignInStatsService
    {
        /// <summary>
        /// 取得會員簽到統計
        /// </summary>
        Task<SignInStatsViewModel?> GetUserSignInStatsAsync(int userId);
        
        /// <summary>
        /// 執行會員簽到
        /// </summary>
        Task<bool> ProcessUserSignInAsync(int userId);
        
        /// <summary>
        /// 取得簽到歷史記錄
        /// </summary>
        Task<List<SignInHistoryViewModel>> GetSignInHistoryAsync(int userId, int page = 1, int pageSize = 20);
        
        /// <summary>
        /// 取得簽到規則設定
        /// </summary>
        Task<SignInRulesViewModel> GetSignInRulesAsync();
        
        /// <summary>
        /// 更新簽到規則設定
        /// </summary>
        Task<bool> UpdateSignInRulesAsync(SignInRulesViewModel rules);
        
        /// <summary>
        /// 取得所有會員簽到統計
        /// </summary>
        Task<List<SignInStatsViewModel>> GetAllSignInStatsAsync(int page = 1, int pageSize = 20);
    }
}