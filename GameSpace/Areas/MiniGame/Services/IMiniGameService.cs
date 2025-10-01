using GameSpace.Areas.MiniGame.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 小遊戲管理服務介面
    /// </summary>
    public interface IMiniGameService
    {
        /// <summary>
        /// 開始小遊戲
        /// </summary>
        Task<string> StartGameAsync(int userId, int petId, string gameType);
        
        /// <summary>
        /// 結束小遊戲並計算結果
        /// </summary>
        Task<bool> EndGameAsync(string sessionId, string gameResult, int pointsEarned, int petExpEarned, int? couponEarned);
        
        /// <summary>
        /// 取得會員遊戲記錄
        /// </summary>
        Task<List<MiniGameViewModel>> GetUserGameHistoryAsync(int userId, int page = 1, int pageSize = 20);
        
        /// <summary>
        /// 取得所有遊戲記錄
        /// </summary>
        Task<List<MiniGameViewModel>> GetAllGameHistoryAsync(int page = 1, int pageSize = 20);
        
        /// <summary>
        /// 取得遊戲規則設定
        /// </summary>
        Task<GameRulesViewModel> GetGameRulesAsync();
        
        /// <summary>
        /// 更新遊戲規則設定
        /// </summary>
        Task<bool> UpdateGameRulesAsync(GameRulesViewModel rules);
        
        /// <summary>
        /// 檢查每日遊戲次數限制
        /// </summary>
        Task<bool> CheckDailyGameLimitAsync(int userId);
        
        /// <summary>
        /// 取得遊戲統計資料
        /// </summary>
        Task<object> GetGameStatisticsAsync();
    }
}