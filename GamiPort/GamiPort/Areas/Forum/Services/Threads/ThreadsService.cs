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

        public async Task<PagedResult<ThreadPostRowDto>> GetThreadPostsAsync(long threadId, string sort, int page, int size)
        {
            page = Math.Max(page, 1);
            size = Math.Clamp(size, 1, 100);

            // 基底查詢（僅 normal；自己決定是否顯示 hidden）
            var q = _db.ThreadPosts
                .AsNoTracking()
                .Where(p => p.ThreadId == threadId && p.Status == "normal");

            // likeCount 子查詢
            var likeQ = _db.Reactions
                .AsNoTracking()
                .Where(r => r.TargetType == TARGET_THREAD_POST && r.Kind == LIKE);

            // 排序
            IOrderedQueryable<ThreadPost> ordered;
            switch (sort?.ToLowerInvariant())
            {
                case "newest":
                    ordered = q.OrderByDescending(p => p.CreatedAt);
                    break;
                case "mostliked":
                    // 依讚數排序，其次按時間
                    ordered = q
                        .OrderByDescending(p => likeQ.Count(r => r.TargetId == p.Id))
                        .ThenBy(p => p.CreatedAt);
                    break;
                default:
                    ordered = q.OrderBy(p => p.CreatedAt); // oldest
                    break;
            }

            var total = await q.CountAsync();

            var items = await ordered
                .Skip((page - 1) * size)
                .Take(size)
                .Select(p => new ThreadPostRowDto(
                    p.Id,
                    p.ThreadId ?? 0,
                    p.AuthorUserId ?? 0,
                    p.ContentMd,
                    p.CreatedAt ?? DateTime.UtcNow,
                    p.ParentPostId,
                    likeQ.Count(r => r.TargetId == p.Id)
                ))
                .ToListAsync();

            return new PagedResult<ThreadPostRowDto>(items, page, size, total);
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





    }
}
