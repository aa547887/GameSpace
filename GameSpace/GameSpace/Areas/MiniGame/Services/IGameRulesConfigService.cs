using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 遊戲規則完整設定服務介面
    /// 提供遊戲規則的完整讀取與更新功能（包含關卡、冒險結果、健康檢查）
    /// </summary>
    public interface IGameRulesConfigService
    {
        /// <summary>
        /// 取得完整遊戲規則設定（包含關卡、冒險結果影響、健康檢查）
        /// </summary>
        Task<GameRulesConfigViewModel> GetCompleteGameRulesAsync();

        /// <summary>
        /// 更新關卡設定
        /// </summary>
        Task<(bool success, string message)> UpdateLevelConfigAsync(LevelConfigInputModel model, int? updatedBy = null);

        /// <summary>
        /// 更新冒險結果影響設定（勝利/失敗）
        /// </summary>
        Task<(bool success, string message)> UpdateAdventureResultImpactAsync(AdventureResultImpactInputModel model, int? updatedBy = null);

        /// <summary>
        /// 更新每日遊戲次數限制
        /// </summary>
        Task<(bool success, string message)> UpdateDailyLimitAsync(int dailyLimit, int? updatedBy = null);
    }
}
