using GamiPort.Areas.Forum.Dtos.Common;
using GamiPort.Areas.Forum.Dtos.Threads;
using Microsoft.EntityFrameworkCore;
using GamiPort.Models;
using System;

namespace GamiPort.Areas.Forum.Services.Threads
{
    public class ThreadsService : IThreadsService
    {
        private readonly GameSpacedatabaseContext _db;
        public ThreadsService(GameSpacedatabaseContext db) => _db = db;

        public async Task<ThreadDetailDto?> GetThreadAsync(long threadId)
        {
            var t = await _db.Threads.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ThreadId == threadId);
            if (t == null) return null;

            var posts = await _db.ThreadPosts.AsNoTracking()
                .Where(p => p.ThreadId == threadId)
                .OrderBy(p => p.CreatedAt)
                .Select(p => new ThreadPostRowDto(
                    p.Id, p.ThreadId??0, p.AuthorUserId ?? 0, p.ContentMd, p.CreatedAt ?? DateTime.MinValue, p.ParentPostId))
                .ToListAsync();

            return new ThreadDetailDto(
                t.ThreadId, t.Title, t.Status, t.AuthorUserId ?? 0, t.CreatedAt ?? DateTime.MinValue, posts);
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
                    p.Id, p.ThreadId ?? 0, p.AuthorUserId ?? 0, p.ContentMd, p.CreatedAt ?? DateTime.MinValue, p.ParentPostId))
                .ToListAsync();

            return new PagedResult<ThreadPostRowDto>(items, page, size, total);
        }
    }
}
