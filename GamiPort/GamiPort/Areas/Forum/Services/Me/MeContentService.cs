using GamiPort.Areas.Forum.Dtos.Common;
using GamiPort.Areas.Forum.Dtos.Me;
using GamiPort.Areas.Forum.Dtos.Threads;
using GamiPort.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Thread = GamiPort.Models.Thread;



namespace GamiPort.Areas.Forum.Services.Me

{
    public class MeContentService : IMeContentService
    {
        private readonly GameSpacedatabaseContext _db;
        private const string TARGET_THREAD = "thread";
        private const string TARGET_THREAD_POST = "thread_post";
        private const string LIKE = "like";

        public MeContentService(GameSpacedatabaseContext db) => _db = db;

        public async Task<PagedResult<MyThreadRowDto>> GetMyThreadsAsync(long userId, string sort, int page, int size, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            size = Math.Clamp(size, 1, 100);

            // 只看自己發的主題
            var q = _db.Threads.AsNoTracking()
                .Where(t => t.AuthorUserId == userId);

            // 聚合：回覆數、最後回覆時間、讚數
            var repliesQ = _db.ThreadPosts.AsNoTracking().Where(p => p.Status == "normal");
            var likesQ = _db.Reactions.AsNoTracking()
                .Where(r => r.TargetType == TARGET_THREAD && r.Kind == LIKE);

            IOrderedQueryable<Thread> ordered = sort?.ToLowerInvariant() switch
            {
                "mostreplied" => q.OrderByDescending(t => repliesQ.Count(p => p.ThreadId == t.ThreadId))
                                  .ThenByDescending(t => t.UpdatedAt),
                _ => q.OrderByDescending(t => t.UpdatedAt) // latest（以 updatedAt = 最後活動排序）
            };

            var total = await q.CountAsync();

            var items = await ordered
                .Skip((page - 1) * size)
                .Take(size)
                .Select(t => new MyThreadRowDto(
                    t.ThreadId,
                    t.ForumId ??0,
                    t.Title,
                    t.CreatedAt ?? DateTime.UtcNow,
                    repliesQ.Where(p => p.ThreadId == t.ThreadId).Max(p => (DateTime?)p.CreatedAt),
                    repliesQ.Count(p => p.ThreadId == t.ThreadId),
                    likesQ.Count(r => r.TargetId == t.ThreadId)
                ))
                .ToListAsync();

            return new PagedResult<MyThreadRowDto>(items, page, size, total);
        }

        public async Task<PagedResult<MyPostRowDto>> GetMyPostsAsync(long userId, string sort, int page, int size, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            size = Math.Clamp(size, 1, 100);

            var postsQ = _db.ThreadPosts.AsNoTracking()
                .Where(p => p.AuthorUserId == userId && p.Status == "normal");

            var likesPostQ = _db.Reactions.AsNoTracking()
                .Where(r => r.TargetType == TARGET_THREAD_POST && r.Kind == LIKE);

            IOrderedQueryable<ThreadPost> ordered = sort?.ToLowerInvariant() switch
            {
                // 先做最新在前
                _ => postsQ.OrderByDescending(p => p.CreatedAt)
            };

            var total = await postsQ.CountAsync();

            var items = await ordered
                .Skip((page - 1) * size)
                .Take(size)
                .Select(p => new MyPostRowDto(
                    p.Id,
                    p.ThreadId ?? 0,
                    _db.Threads.Where(t => t.ThreadId == p.ThreadId).Select(t => t.Title).FirstOrDefault() ?? "",
                    p.ContentMd,
                    p.CreatedAt ?? DateTime.UtcNow,
                    p.ParentPostId,
                    likesPostQ.Count(r => r.TargetId == p.Id)
                ))
                .ToListAsync();

            return new PagedResult<MyPostRowDto>(items, page, size, total);
        }

        public async Task<PagedResult<MyLikedThreadRowDto>> GetMyLikedThreadsAsync(
    long userId, string sort, int page, int size, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            size = Math.Clamp(size, 1, 100);

            var myLikesQ = _db.Reactions.AsNoTracking()
                .Where(r => r.UserId == userId && r.TargetType == "thread" && r.Kind == "like");

            var likesQ = _db.Reactions.AsNoTracking()
                .Where(r => r.TargetType == "thread" && r.Kind == "like");

            var repliesQ = _db.ThreadPosts.AsNoTracking()
                .Where(p => p.Status == "normal");

            // ❌ 不要用 IOrderedQueryable<dynamic>
            // ✔ 讓編譯器用匿名型別推斷
            var baseQ =
                from r in myLikesQ
                join t in _db.Threads.AsNoTracking() on r.TargetId equals t.ThreadId
                select new { r, t };

            var orderedQ = (sort?.ToLowerInvariant()) switch
            {
                "mostliked" => baseQ
                    .OrderByDescending(x => likesQ.Count(l => l.TargetId == x.t.ThreadId))
                    .ThenByDescending(x => x.r.CreatedAt),
                _ => baseQ
                    .OrderByDescending(x => x.r.CreatedAt) // 我最近按的在前
            };

            var total = await baseQ.CountAsync();

            var items = await orderedQ
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new MyLikedThreadRowDto(
                    x.t.ThreadId,
                    x.t.ForumId ??0,
                    x.t.Title,
                    x.r.CreatedAt ?? DateTime.UtcNow,
                    likesQ.Count(l => l.TargetId == x.t.ThreadId),
                    repliesQ.Count(p => p.ThreadId == x.t.ThreadId)
                ))
                .ToListAsync();

            return new PagedResult<MyLikedThreadRowDto>(items, page, size, total);
        }

    }
}
