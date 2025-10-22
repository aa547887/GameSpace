using GamiPort.Areas.Forum.Dtos.Common;
using GamiPort.Areas.Forum.Dtos.Forum;
using GamiPort.Areas.Forum.Dtos.Threads;
using Microsoft.EntityFrameworkCore;
using System;

namespace GamiPort.Areas.Forum.Services.Forums
{
    public class ForumsService : IForumsService
    {
        private readonly AppDbContext _db;
        public ForumsService(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<ForumListItemDto>> GetForumsAsync()
        {
            return await _db.Forums.AsNoTracking()
                .OrderBy(f => f.Name)
                .Select(f => new ForumListItemDto(f.ForumId, f.GameId, f.Name, f.Description))
                .ToListAsync();
        }

        public async Task<ForumDetailDto?> GetForumAsync(int forumId)
        {
            var f = await _db.Forums.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ForumId == forumId);
            return f == null ? null : new ForumDetailDto(f.ForumId, f.GameId, f.Name, f.Description);
        }

        public async Task<ForumDetailDto?> GetForumByGameAsync(int gameId)
        {
            var f = await _db.Forums.AsNoTracking()
                .FirstOrDefaultAsync(x => x.GameId == gameId);
            return f == null ? null : new ForumDetailDto(f.ForumId, f.GameId, f.Name, f.Description);
        }

        public async Task<PagedResult<ThreadListItemDto>> GetThreadsByForumAsync(
            int forumId, string sort, int page, int size)
        {
            var q = _db.Threads.AsNoTracking().Where(t => t.ForumId == forumId);

            q = sort switch
            {
                "created" => q.OrderByDescending(t => t.CreatedAt),
                "hot" => q.OrderByDescending(t => t.UpdatedAt), // 之後可換熱門欄位/計算
                _ => q.OrderByDescending(t => t.UpdatedAt)  // lastReply 預設
            };

            var total = await q.CountAsync();

            var items = await q.Skip((page - 1) * size)
                .Take(size)
                .Select(t => new ThreadListItemDto(
                    t.ThreadId,
                    t.Title,
                    t.Status,
                    t.CreatedAt,
                    t.UpdatedAt,
                    _db.ThreadPosts.Count(p => p.ThreadId == t.ThreadId)
                ))
                .ToListAsync();

            return new PagedResult<ThreadListItemDto>(items, page, size, total);
        }
    }
}
