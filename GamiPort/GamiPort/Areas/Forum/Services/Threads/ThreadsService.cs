using GamiPort.Areas.Forum.Dtos.Common;
using GamiPort.Areas.Forum.Dtos.Threads;
using Microsoft.EntityFrameworkCore;
using System;

namespace GamiPort.Areas.Forum.Services.Threads
{
    public class ThreadsService : IThreadsService
    {
        private readonly DbContext _db;
        public ThreadsService(DbContext db) => _db = db;

        public async Task<ThreadDetailDto?> GetThreadAsync(long threadId)
        {
            var t = await _db.Threads.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ThreadId == threadId);
            if (t == null) return null;

            var posts = await _db.ThreadPosts.AsNoTracking()
                .Where(p => p.ThreadId == threadId)
                .OrderBy(p => p.CreatedAt)
                .Select(p => new ThreadPostRowDto(
                    p.Id, p.ThreadId, p.AuthorUserId, p.ContentMd, p.CreatedAt, p.ParentPostId))
                .ToListAsync();

            return new ThreadDetailDto(
                t.ThreadId, t.Title, t.Status, t.AuthorUserId, t.CreatedAt, posts);
        }

        public async Task<PagedResult<ThreadPostRowDto>> GetThreadPostsAsync(long threadId, int page, int size)
        {
            var q = _db.ThreadPosts.AsNoTracking()
                .Where(p => p.ThreadId == threadId)
                .OrderBy(p => p.CreatedAt);

            var total = await q.CountAsync();

            var items = await q.Skip((page - 1) * size)
                .Take(size)
                .Select(p => new ThreadPostRowDto(
                    p.Id, p.ThreadId, p.AuthorUserId, p.ContentMd, p.CreatedAt, p.ParentPostId))
                .ToListAsync();

            return new PagedResult<ThreadPostRowDto>(items, page, size, total);
        }
    }
}
