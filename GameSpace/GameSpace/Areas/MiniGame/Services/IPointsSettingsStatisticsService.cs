namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 點數設定統計服務介面
    /// </summary>
    public interface IPointsSettingsStatisticsService
    {
        /// <summary>
        /// 獲取點數統計資訊
        /// </summary>
        Task<Dictionary<string, object>> GetStatisticsAsync();

        /// <summary>
        /// 獲取點數分佈資訊
        /// </summary>
        Task<Dictionary<string, int>> GetPointsDistributionAsync();

        /// <summary>
        /// 獲取點數使用趨勢
        /// </summary>
        Task<Dictionary<string, int>> GetPointsUsageTrendAsync(int days = 30);
    }
}

