using GamiPort.Areas.Forum.Dtos.Leaderboard;

namespace GamiPort.Areas.Forum.Services.Leaderboard
{
    public interface ILeaderboardService
    {
        /// <summary>
        /// 取得指定日期的排行榜（含名次變化）；limit 1~100 安全範圍。 // 專門抓今日榜 + 名次變化
        /// </summary>
        Task<LeaderboardDailyResponseDto> GetDailyWithDeltaAsync(DateTime? date, int limit = 10);

        /// <summary>
        /// 取得純排行榜（不算 delta），主要給簡化使用或除錯。// 專門抓今日榜 (不算漲跌)
        /// </summary>
        Task<LeaderboardDailyResponseDto> GetDailyAsync(DateTime? date, int limit = 10);
    }
}
