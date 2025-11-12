using GamiPort.Areas.Forum.Dtos.Leaderboard;
using GamiPort.Areas.Forum.Services.Leaderboard;
using Microsoft.AspNetCore.Mvc;
namespace GamiPort.Areas.Forum.ApiControllers
{
    /// <summary>
    /// 熱度排行榜 API：
    /// - /api/leaderboard/daily：取某日榜單（預設今天），不含漲跌
    /// - /api/leaderboard/daily-with-delta：取某日榜單＋與上一個有資料日期的名次變化
    /// 供前台直接使用（首頁卡片/排行榜頁）
    /// </summary>
    [ApiController]
    [Area("Forum")]
    [Route("api/[area]/leaderboard")]
    public sealed class LeaderboardApiController : ControllerBase
    {
        private readonly ILeaderboardService _svc;
        public LeaderboardApiController(ILeaderboardService svc) => _svc = svc;

        /// <summary>
        /// 取某日（預設今天）排行榜 Top N（預設 10）
        /// Query:
        ///   - date: yyyy-MM-dd（可選；不給就是今天 UTC）
        ///   - limit: 1~100（可選；預設 10）
        /// 回傳：{ date, items:[{rank, gameId, name, index}] }
        /// </summary>
        [HttpGet("daily")]
        public async Task<ActionResult<LeaderboardDailyResponseDto>> GetDaily(
            [FromQuery] DateTime? date, [FromQuery] int limit = 10)
        {
            var dto = await _svc.GetDailyAsync(date, limit);
            return Ok(dto);
        }

        /// <summary>
        /// 取某日（預設今天）排行榜 Top N + 名次變化（與上一個有資料的日子相比）
        /// Query:
        ///   - date: yyyy-MM-dd（可選）
        ///   - limit: 1~100（可選；預設 10）
        /// 回傳：{ date, prevDate, items:[{rank, gameId, name, index, delta, trend}] }
        ///   - delta: +2 / -1 / null(新進)
        ///   - trend: "up" | "down" | "same" | "new"
        /// 前端可直接用 delta/ trend 畫 ▲▼ 或 NEW 標籤。
        /// </summary>
        [HttpGet("daily-with-delta")]
        public async Task<ActionResult<LeaderboardDailyResponseDto>> GetDailyWithDelta(
            [FromQuery] DateTime? date, [FromQuery] int limit = 10)
        {
            var dto = await _svc.GetDailyWithDeltaAsync(date, limit);
            return Ok(dto);
        }
    }
}
