using System.Linq; // ← 用到 LINQ 擴充方法要有
using GamiPort.Areas.Forum.Dtos.Leaderboard;
using GamiPort.Models;               // GameSpacedatabaseContext, Entities
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.Forum.Services.Leaderboard
{
    public sealed class LeaderboardService : ILeaderboardService
    {
        private readonly GameSpacedatabaseContext _db;
        public LeaderboardService(GameSpacedatabaseContext db) => _db = db;

        // 只取某日排行榜（不含 delta）
        public async Task<LeaderboardDailyResponseDto> GetDailyAsync(DateTime? date, int limit = 10)
        {
            var d = DateOnly.FromDateTime(date?.Date ?? DateTime.UtcNow.Date);
            limit = Math.Clamp(limit, 1, 100);

            // SQL 階段先匿名型別，ToList 後再轉 tuple（EF 不支援直接投影 tuple）
            var todayTmp = await (
                from p in _db.PopularityIndexDailies
                join g in _db.Games on p.GameId equals g.GameId
                where p.Date == d
                orderby p.IndexValue descending
                select new { p.GameId, g.Name, p.IndexValue }
            ).ToListAsync();

            var todayRows = todayTmp
                .Select(x => (GameId: x.GameId ?? 0,
                              Name: x.Name ?? string.Empty,
                              Index: x.IndexValue ?? 0m))
                .ToList();

            var ranked = DenseRankToday(todayRows)
                .Take(limit)
                .Select(x => new LeaderboardItemDto
                {
                    Rank = x.Rank,
                    GameId = x.GameId,
                    Name = x.Name,
                    Index = x.Index,
                    Delta = null,
                    Trend = "same"
                }).ToList();

            return new LeaderboardDailyResponseDto
            {
                Date = d.ToString("yyyy-MM-dd"),
                PrevDate = null,
                Items = ranked
            };
        }

        // 取某日排行榜 + 與前一個有資料的日子做名次差（delta）
        public async Task<LeaderboardDailyResponseDto> GetDailyWithDeltaAsync(DateTime? date, int limit = 10)
        {
            var d = DateOnly.FromDateTime(date?.Date ?? DateTime.UtcNow.Date);
            limit = Math.Clamp(limit, 1, 100);

            // 找 < d 的最近一個有資料日期（DateOnly?）
            var prevDate = await _db.PopularityIndexDailies
                .Where(x => x.Date != null && x.Date < d)
                .Select(x => x.Date)
                .OrderByDescending(x => x)
                .FirstOrDefaultAsync();

            var todayTmp = await (
                from p in _db.PopularityIndexDailies
                join g in _db.Games on p.GameId equals g.GameId
                where p.Date == d
                orderby p.IndexValue descending
                select new { p.GameId, g.Name, p.IndexValue }
            ).ToListAsync();

            var todayRows = todayTmp
                .Select(x => (GameId: x.GameId ?? 0,
                              Name: x.Name ?? string.Empty,
                              Index: x.IndexValue ?? 0m))
                .ToList();

            // 取「上一個日期」的榜單（可能不存在）
            List<(int GameId, decimal Index)> prevRows;
            if (prevDate.HasValue)
            {
                var prevTmp = await _db.PopularityIndexDailies
                    .Where(x => x.Date == prevDate.Value)
                    .OrderByDescending(x => x.IndexValue)
                    .Select(x => new { x.GameId, x.IndexValue })
                    .ToListAsync();

                prevRows = prevTmp
                    .Select(x => (GameId: x.GameId ?? 0, Index: x.IndexValue ?? 0m))
                    .ToList();
            }
            else
            {
                prevRows = new();
            }

            var todayRanked = DenseRankToday(todayRows); // (Rank, GameId, Name, Index)
            var prevRanked = DenseRankPrev(prevRows);   // (Rank, GameId, Index)

            var prevRankMap = prevRanked.ToDictionary(x => x.GameId, x => x.Rank);

            var items = todayRanked
                .Take(limit)
                .Select(x =>
                {
                    // delta = 昨日名次 - 今日名次；昨日無資料 => null
                    int? delta = prevRankMap.TryGetValue(x.GameId, out var prev)
                               ? prev - x.Rank
                               : (int?)null;

                    var trend = delta == null ? "new"
                              : delta > 0 ? "up"
                              : delta < 0 ? "down"
                              : "same";

                    return new LeaderboardItemDto
                    {
                        Rank = x.Rank,
                        GameId = x.GameId,
                        Name = x.Name,
                        Index = x.Index,
                        Delta = delta,
                        Trend = trend
                    };
                })
                .ToList();

            return new LeaderboardDailyResponseDto
            {
                Date = d.ToString("yyyy-MM-dd"),
                PrevDate = prevDate.HasValue ? prevDate.Value.ToString("yyyy-MM-dd") : null, // ← 保守寫法
                Items = items
            };
        }

        // ---- Dense Rank（同分同名次、不跳號）----

        private static List<(int Rank, int GameId, string Name, decimal Index)>
        DenseRankToday(List<(int GameId, string Name, decimal Index)> rows)
        {
            var result = new List<(int Rank, int GameId, string Name, decimal Index)>();
            decimal? last = null;
            int rank = 0;

            foreach (var r in rows)
            {
                if (last == null || r.Index != last.Value)
                    rank = result.Count == 0 ? 1 : result.Last().Rank + 1;

                last = r.Index;
                result.Add((Rank: rank, GameId: r.GameId, Name: r.Name, Index: r.Index));
            }
            return result;
        }

        private static List<(int Rank, int GameId, decimal Index)>
        DenseRankPrev(List<(int GameId, decimal Index)> rows)
        {
            var result = new List<(int Rank, int GameId, decimal Index)>();
            decimal? last = null;
            int rank = 0;

            foreach (var r in rows)
            {
                if (last == null || r.Index != last.Value)
                    rank = result.Count == 0 ? 1 : result.Last().Rank + 1;

                last = r.Index;
                result.Add((Rank: rank, GameId: r.GameId, Index: r.Index));
            }
            return result;
        }
    }
}
