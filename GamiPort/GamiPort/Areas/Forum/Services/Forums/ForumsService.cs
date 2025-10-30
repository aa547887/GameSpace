using GamiPort.Areas.Forum.Dtos.Common;
using GamiPort.Areas.Forum.Dtos.Forum;
using GamiPort.Areas.Forum.Dtos.Threads;
using GamiPort.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using Thread = GamiPort.Models.Thread;
namespace GamiPort.Areas.Forum.Services.Forums
{

    public class ForumsService : IForumsService
    {
        private readonly GameSpacedatabaseContext _db;

        public ForumsService(GameSpacedatabaseContext db) => _db = db;

        private sealed record ThreadEnriched(Thread T, int ReplyCount, DateTime LastActivity, double? Score);

        public async Task<IReadOnlyList<ForumListItemDto>> GetForumsAsync()
        {
            return await _db.Forums.AsNoTracking()
                .OrderBy(f => f.Name)
                .Select(f => new ForumListItemDto(
                    f.ForumId,
                    f.GameId ?? 0,
                    f.Name,
                    f.Description
                ))
                .ToListAsync();
        }

        public async Task<ForumDetailDto?> GetForumAsync(int forumId)
        {
            var f = await _db.Forums.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ForumId == forumId);

            return f == null
                ? null
                : new ForumDetailDto(f.ForumId, f.GameId ?? 0, f.Name, f.Description);
        }

        public async Task<ForumDetailDto?> GetForumByGameAsync(int gameId)
        {
            var f = await _db.Forums.AsNoTracking()
                .FirstOrDefaultAsync(x => x.GameId == gameId);

            return f == null
                ? null
                : new ForumDetailDto(f.ForumId, f.GameId ?? 0, f.Name, f.Description);
        }

        // ★ ① 介面要求的「舊版 4 參數」：為了相容，直接轉呼叫新版
        public Task<PagedResult<ThreadListItemDto>> GetThreadsByForumAsync(
            int forumId, string sort, int page, int size)
            => GetThreadsByForumAsync(
                forumId, sort, page, size,
                keyword: null, inContent: false, inGame: false, ct: default);

        // ★ ② 新版：簽名一定要包含 CancellationToken（最後一個參數）
        public async Task<PagedResult<ThreadListItemDto>> GetThreadsByForumAsync(
    int forumId, string sort, int page, int size,
    string? keyword = null, bool inContent = false, bool inGame = false,
    CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            size = Math.Clamp(size, 1, 100);

            var baseQ = _db.Threads.AsNoTracking()
                .Where(t => t.ForumId == forumId);

            // 關鍵字（標題 + 可選內容）
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim();

                var pred = _db.Threads.AsNoTracking()
                    .Where(t => t.ForumId == forumId && EF.Functions.Like(t.Title, $"%{k}%"));

                if (inContent)
                {
                    var contentHit =
                        from t in _db.Threads.AsNoTracking()
                        join p in _db.ThreadPosts.AsNoTracking() on t.ThreadId equals p.ThreadId
                        where t.ForumId == forumId
                              && p.ContentMd != null
                              && EF.Functions.Like(p.ContentMd, $"%{k}%")
                        select t;

                    pred = pred.Concat(contentHit);
                }

                baseQ = pred.Distinct();
            }

            var now = DateTime.UtcNow;

            // 先用匿名型別在 DB 端把要排序/分頁的欄位統統算好
            var q = baseQ
                .Select(t => new
                {
                    t.ThreadId,
                    t.Title,
                    t.Status,
                    CreatedAt = t.CreatedAt,                   // 可為 null
                    UpdatedAt = t.UpdatedAt,                   // 可為 null
                    ReplyCount = _db.ThreadPosts.Count(p => p.ThreadId == t.ThreadId),
                    LastActivity = (t.UpdatedAt ?? t.CreatedAt ?? now)
                });

            var sortKey = (sort ?? "lastReply").ToLowerInvariant();

            // 排序：只用匿名型別欄位
            q = sortKey switch
            {
                "created" => q.OrderByDescending(x => x.CreatedAt ?? x.LastActivity),
                "hot" => q.Select(x => new
                {
                    x.ThreadId,
                    x.Title,
                    x.Status,
                    x.CreatedAt,
                    x.UpdatedAt,
                    x.ReplyCount,
                    x.LastActivity,
                    Score = (double)x.ReplyCount * 1.6
                                            - (double)EF.Functions.DateDiffHour(x.LastActivity, now) * 0.08
                })
                              .OrderByDescending(x => x.Score)
                              .ThenByDescending(x => x.LastActivity)
                              .Select(x => new
                              {
                                  x.ThreadId,
                                  x.Title,
                                  x.Status,
                                  x.CreatedAt,
                                  x.UpdatedAt,
                                  x.ReplyCount,
                                  x.LastActivity
                              }),
                _ => q.OrderByDescending(x => x.LastActivity) // lastReply
            };

            var total = await q.CountAsync(ct);

            var rows = await q
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            // 這裡才 map 成 DTO
            var items = rows.Select(x => new ThreadListItemDto(
                x.ThreadId,
                x.Title,
                x.Status,
                x.CreatedAt ?? x.LastActivity,   // 補非 null
                x.UpdatedAt,
                x.ReplyCount
            )).ToList();

            return new PagedResult<ThreadListItemDto>(items, page, size, total);
        }



        public async Task<IReadOnlyList<ForumListItemDto>> SearchForumsAsync(
    string keyword, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return Array.Empty<ForumListItemDto>();
            var k = keyword.Trim();

            var q =
                from f in _db.Forums.AsNoTracking()
                join g in _db.Games.AsNoTracking() on f.GameId equals g.GameId into gj
                from g in gj.DefaultIfEmpty()
                where EF.Functions.Like(f.Name, $"%{k}%")
                   || (g != null && (
                          EF.Functions.Like(g.Name, $"%{k}%")
                       || (g.NameZh != null && EF.Functions.Like(g.NameZh, $"%{k}%"))
                   ))
                select new { f, g };

            // 打分排序：精確命中>開頭命中>一般包含
            var rows = await q
                .Select(x => new
                {
                    x.f,
                    score =
                        (x.g != null && (x.g.Name == k || x.g.NameZh == k) || x.f.Name == k) ? 3 :
                        (x.g != null && (x.g.Name.StartsWith(k) || (x.g.NameZh != null && x.g.NameZh.StartsWith(k))) || x.f.Name.StartsWith(k)) ? 2 : 1
                })
                .OrderByDescending(x => x.score)
                .ThenBy(x => x.f.Name)
                .Select(x => new ForumListItemDto(
                    x.f.ForumId,
                    x.f.GameId ?? 0,
                    x.f.Name,
                    x.f.Description
                ))
                .ToListAsync(ct);

            return rows;
        }

        public async Task<ForumDetailDto?> GetForumByGameNameAsync(
            string gameName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(gameName)) return null;
            var k = gameName.Trim();

            // 先找「完全相等」
            var exact = await (
                from f in _db.Forums.AsNoTracking()
                join g in _db.Games.AsNoTracking() on f.GameId equals g.GameId into gj
                from g in gj.DefaultIfEmpty()
                where (g != null && (g.Name == k || g.NameZh == k)) || f.Name == k
                select new ForumDetailDto(f.ForumId, f.GameId ?? 0, f.Name, f.Description)
            ).FirstOrDefaultAsync(ct);

            if (exact != null) return exact;

            // 再退而求其次：包含/前綴
            var fallback = await (
                from f in _db.Forums.AsNoTracking()
                join g in _db.Games.AsNoTracking() on f.GameId equals g.GameId into gj
                from g in gj.DefaultIfEmpty()
                where EF.Functions.Like(f.Name, $"%{k}%")
                   || (g != null && (
                          EF.Functions.Like(g.Name, $"%{k}%")
                       || (g.NameZh != null && EF.Functions.Like(g.NameZh, $"%{k}%"))
                      ))
                orderby f.Name
                select new ForumDetailDto(f.ForumId, f.GameId ?? 0, f.Name, f.Description)
            ).FirstOrDefaultAsync(ct);

            return fallback;
        }





    }

}



