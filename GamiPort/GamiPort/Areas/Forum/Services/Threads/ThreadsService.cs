using GamiPort.Areas.Forum.Dtos.Common;
using GamiPort.Areas.Forum.Dtos.Threads;
using GamiPort.Areas.Forum.Services.Threads;
using GamiPort.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Thread = GamiPort.Models.Thread;

namespace GamiPort.Areas.Forum.Services.Threads
{
  
    public class ThreadsService : GamiPort.Areas.Forum.Services.Threads.IThreadsService
    {
        private readonly GameSpacedatabaseContext _db;
        private const string TARGET_THREAD = "thread";
        private const string TARGET_THREAD_POST = "thread_post";
        private const string LIKE = "like";

        public ThreadsService(GameSpacedatabaseContext db) => _db = db;

        public async Task<ThreadDetailDto?> GetThreadAsync(long threadId, long currentUserId)
        {
            var t = await _db.Threads
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ThreadId == threadId);

            if (t == null) return null;

            // replies count / last reply at（僅 normal 計算；視你的狀態策略調整）
            var repliesQuery = _db.ThreadPosts.AsNoTracking().Where(p => p.ThreadId == threadId && p.Status == "normal");
            var repliesCount = await repliesQuery.CountAsync();
            var lastReplyAt = await repliesQuery.MaxAsync(p => (DateTime?)p.CreatedAt);

            // likes
            var likeCount = await _db.Reactions.AsNoTracking()
                .Where(r => r.TargetType == TARGET_THREAD && r.TargetId == threadId && r.Kind == LIKE)
                .CountAsync();

            var isLiked = currentUserId > 0 && await _db.Reactions.AsNoTracking()
                .AnyAsync(r => r.UserId == currentUserId && r.TargetType == TARGET_THREAD && r.TargetId == threadId && r.Kind == LIKE);

            return new ThreadDetailDto(
                t.ThreadId,
                t.ForumId ?? 0,
                t.Title,
                t.Status,
                t.AuthorUserId??0,
                t.CreatedAt ?? DateTime.UtcNow,
                lastReplyAt,
                repliesCount,
                likeCount,
                isLiked
            );
        }

        public async Task<PagedResult<ThreadPostItemDto>> GetThreadPostsAsync(
    long threadId, string sort, int page, int size,
    long currentUserId, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            size = Math.Clamp(size, 1, 100);

            // 基底：該主題下、狀態正常的回覆
            var baseQ = _db.ThreadPosts.AsNoTracking()
                .Where(p => p.ThreadId == threadId && p.Status == "normal");

            // 只挑出「貼文按讚」的反應
            var likeQ = _db.Reactions.AsNoTracking()
                .Where(r => r.TargetType == TARGET_THREAD_POST && r.Kind == LIKE);

            // 先投影成匿名型別，把排序/分頁會用到的欄位都算好（可被 EF 翻譯）
            var q = from p in baseQ
                        // 作者暱稱
                    join u in _db.Users.AsNoTracking() on p.AuthorUserId equals u.UserId
                    // 我是否按過讚（左外連）
                    join myLike in likeQ.Where(x => x.UserId == currentUserId)
                        on p.Id equals myLike.TargetId into gj
                    from liked in gj.DefaultIfEmpty()
                    select new
                    {
                        p.Id,
                        p.ThreadId,
                        p.AuthorUserId,
                        AuthorName =   u.UserName ?? ("user_" + u.UserId),
                        CreatedAt = p.CreatedAt ?? DateTime.UtcNow,
                        p.ContentMd,
                        ContentHtml = (string?)null,
                        p.ParentPostId,
                        LikeCount = likeQ.Count(r => r.TargetId == p.Id),
                        IsLikedByMe = liked != null,
                        CanDelete = (p.AuthorUserId ?? 0) == currentUserId // 管理員再加條件
                    };

            // 排序（注意用匿名型別欄位，不要塞子查詢到 OrderBy 裡）
            var key = (sort ?? "oldest").ToLowerInvariant();
            q = key switch
            {
                "newest" => q.OrderByDescending(x => x.CreatedAt),
                "mostliked" => q.OrderByDescending(x => x.LikeCount).ThenBy(x => x.CreatedAt),
                _ => q.OrderBy(x => x.CreatedAt) // oldest
            };

            var total = await baseQ.CountAsync(ct);

            var rows = await q
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            var items = rows.Select(x => new ThreadPostItemDto(
                x.Id,
                x.AuthorUserId ?? 0,
                x.AuthorName,
                x.CreatedAt,
                x.ContentMd,
                x.ContentHtml,
                x.ParentPostId,
                x.LikeCount,
                x.IsLikedByMe,
                x.CanDelete
            )).ToList();

            return new PagedResult<ThreadPostItemDto>(items, page, size, total);
        }


        public async Task<long> CreateThreadAsync(long userId, int forumId, string title, string contentMd)
        {
            if (userId <= 0) throw new UnauthorizedAccessException();
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("title");
            if (string.IsNullOrWhiteSpace(contentMd)) throw new ArgumentException("contentMd");

            // 可選：檢查 banned_words（略）
            var now = DateTime.UtcNow;

            using var tx = await _db.Database.BeginTransactionAsync();

            var thread = new Thread
            {
                ForumId = forumId,
                AuthorUserId = (int)userId,
                Title = title.Trim(),
                Status = "normal",
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.Threads.Add(thread);
            await _db.SaveChangesAsync();

            var post = new ThreadPost
            {
                ThreadId = thread.ThreadId,
                AuthorUserId = (int)userId,
                ContentMd = contentMd,
                ParentPostId = null,
                Status = "normal",
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.ThreadPosts.Add(post);

            // 更新 thread 活動時間（當作 last activity）
            thread.UpdatedAt = now;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return thread.ThreadId;
        }

        public async Task<long> CreatePostAsync(long userId, long threadId, string contentMd, long? parentPostId)
        {
            if (userId <= 0) throw new UnauthorizedAccessException();
            if (string.IsNullOrWhiteSpace(contentMd)) throw new ArgumentException("contentMd");

            var thread = await _db.Threads.FirstOrDefaultAsync(t => t.ThreadId == threadId);
            if (thread == null) throw new KeyNotFoundException("thread not found");
            if (thread.Status == "locked") throw new InvalidOperationException("thread locked");

            if (parentPostId.HasValue)
            {
                var parent = await _db.ThreadPosts.FirstOrDefaultAsync(p => p.Id == parentPostId.Value);
                if (parent == null || parent.ThreadId != threadId)
                    throw new InvalidOperationException("parentPost not in thread");
            }

            var now = DateTime.UtcNow;

            var post = new ThreadPost
            {
                ThreadId = threadId,
                AuthorUserId = (int)userId,
                ContentMd = contentMd,
                ParentPostId = parentPostId,
                Status = "normal",
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.ThreadPosts.Add(post);

            // 更新 thread 活動時間
            thread.UpdatedAt = now;

            await _db.SaveChangesAsync();
            return post.Id;
        }

        public async Task<bool> ToggleThreadLikeAsync(long userId, long threadId)
        {
            if (userId <= 0) throw new UnauthorizedAccessException();

            var exist = await _db.Reactions
                .FirstOrDefaultAsync(r => r.UserId == userId && r.TargetType == TARGET_THREAD && r.TargetId == threadId && r.Kind == LIKE);

            if (exist == null)
            {
                _db.Reactions.Add(new Reaction
                {
                    UserId = (int)userId,
                    TargetType = TARGET_THREAD,
                    TargetId = threadId,
                    Kind = LIKE,
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
                return true; // 現在是已按讚
            }
            else
            {
                _db.Reactions.Remove(exist);
                await _db.SaveChangesAsync();
                return false; // 現在是已取消
            }
        }

        public async Task<LikeStatusDto> GetThreadLikeStatusAsync(long userId, long threadId)
        {
            var likeCount = await _db.Reactions.AsNoTracking()
                .Where(r => r.TargetType == TARGET_THREAD && r.TargetId == threadId && r.Kind == LIKE)
                .CountAsync();

            var isLiked = userId > 0 && await _db.Reactions.AsNoTracking()
                .AnyAsync(r => r.UserId == userId && r.TargetType == TARGET_THREAD && r.TargetId == threadId && r.Kind == LIKE);

            return new LikeStatusDto(isLiked, likeCount);
        }

        //回覆文案讚

        public async Task<bool> TogglePostLikeAsync(long userId, long postId)  
        {
            var existing = await _db.Reactions
                .FirstOrDefaultAsync(r => r.UserId == userId
                                        && r.TargetType == "thread_post"
                                        && r.TargetId == postId);

            if (existing != null)
            {
                _db.Reactions.Remove(existing);
                await _db.SaveChangesAsync();
                return false; // 取消讚
            }
            else
            {
                _db.Reactions.Add(new Reaction
                {
                    UserId = (int?)userId,
                    TargetType = "thread_post",
                    TargetId = postId,
                    Kind = "like",
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
                return true; // 新增讚
            }
        }

        public async Task<(bool IsLiked, int LikeCount)> GetPostLikeStatusAsync(long userId, long postId)
        {
            var isLiked = await _db.Reactions.AnyAsync(r =>
                r.UserId == userId &&
                r.TargetType == "thread_post" &&
                r.TargetId == postId);

            var likeCount = await _db.Reactions.CountAsync(r =>
                r.TargetType == "thread_post" &&
                r.TargetId == postId);

            return (isLiked, likeCount);
        }

        public async Task<bool> DeleteThreadAsync(long userId, long threadId, CancellationToken ct = default)
        {
            // 1. 抓 thread
            var thread = await _db.Threads
                .Include(t => t.ThreadId) // ⚠️ cascade 手動刪 post
                .FirstOrDefaultAsync(t => t.ThreadId == threadId, ct);

            if (thread == null)
                return false;

            // 2. 權限檢查：只能刪自己
            if (thread.AuthorUserId != userId)
                return false; // 之後可改成 throw Forbidden

            // 3. 先刪底下所有 post（硬刪）
            if (thread.ThreadPosts != null && thread.ThreadPosts.Any())
            {
                _db.ThreadPosts.RemoveRange(thread.ThreadPosts);
            }

            // 再刪 thread
            _db.Threads.Remove(thread);
            await _db.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> DeletePostAsync(long userId, long postId, CancellationToken ct = default)
        {
            // 1. 抓 post
            var post = await _db.ThreadPosts
                .FirstOrDefaultAsync(p => p.Id == postId, ct);

            if (post == null)
                return false;

            // 2. 權限檢查：只能刪自己
            if (post.AuthorUserId != userId)

                return false;

            // 3. 刪
            _db.ThreadPosts.Remove(post);
            await _db.SaveChangesAsync(ct);

            return true;
        }




    }
}
