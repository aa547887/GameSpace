using GamiPort.Areas.Forum.Dtos.AdminPosts;
using System;
using System.Linq;
using System.Threading.Tasks;
using GamiPort.Areas.Forum.Dtos;
using GamiPort.Areas.Forum.Services.Adminpost;
using GamiPort.Models;              // GameSpacedatabaseContext / Posts
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.Forum.Services.Adminpost
{
    public sealed class PostsService : IPostsService
    {
        private readonly GameSpacedatabaseContext _db;

        public PostsService(GameSpacedatabaseContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<PostListDto>> GetFrontPostsAsync(PostQuery q)
        {
            // 安全數值
            var page = Math.Max(1, q.Page);
            var size = Math.Clamp(q.Size, 1, 100);
            var type = string.IsNullOrWhiteSpace(q.Type) ? "insight" : q.Type;

            // 只回前台可見：published
            var baseQ = _db.Posts
                .AsNoTracking()
                .Where(p => p.Type == type && p.Status == "published");

            if (q.GameId.HasValue) baseQ = baseQ.Where(p => p.GameId == q.GameId.Value);

            var total = await baseQ.CountAsync();

            var items = await baseQ
                .OrderByDescending(p => p.Pinned)
                .ThenByDescending(p => p.PublishedAt)   // 置頂優先，再新到舊
                .Skip((page - 1) * size)
                .Take(size)
                .Select(p => new PostListDto
                {
                    PostId = p.PostId,
                    Type = p.Type!,
                    GameId = p.GameId,
                    Title = p.Title!,
                    Tldr = p.Tldr,
                    BodyPreview = p.BodyMd == null ? null :
                                  (p.BodyMd.Length > 240 ? p.BodyMd.Substring(0, 240) : p.BodyMd),
                    Status = p.Status!,
                    Pinned = p.Pinned ?? false,
                    Author = new AuthorDto
                    {
                        UserId = p.CreatedBy ?? 0, // ✅ nullable → int
                        DisplayName = "Admin"
                    },
                    PublishedAt = p.PublishedAt,
                    CreatedAt = p.CreatedAt ?? DateTime.MinValue,
                    UpdatedAt = p.UpdatedAt,
                    Links = new LinkDto { Detail = $"/api/posts/{p.PostId}" }
                })
                .ToListAsync();

            return new PagedResult<PostListDto>(page, size, total, items);
        }

        public async Task<PostDetailDto?> GetFrontPostAsync(int postId)
        {
            // 單筆也只允許公開貼文
            return await _db.Posts
                .AsNoTracking()
                .Where(p => p.PostId == postId && p.Status == "published")
                .Select(p => new PostDetailDto
                {
                    PostId = p.PostId,
                    Type = p.Type!,
                    GameId = p.GameId,
                    Title = p.Title!,
                    Tldr = p.Tldr,
                    BodyMd = p.BodyMd ?? "",
                    Status = p.Status!,
                    Pinned = p.Pinned ?? false,
                    Author = new AuthorDto {
                        UserId = p.CreatedBy ?? 0, // ✅ nullable → int
                        DisplayName = "Admin"
                    },
                    PublishedAt = p.PublishedAt ?? DateTime.MinValue, // ✅ nullable → DateTime
                    CreatedAt = p.CreatedAt ?? DateTime.MinValue,  // 如果 DB 是 nullable → 改 p.CreatedAt ?? DateTime.MinValue
                    UpdatedAt = p.UpdatedAt    // 如果 DB 是 nullable → 改 p.UpdatedAt ?? DateTime.MinValue
                })
                .FirstOrDefaultAsync();
        }
    }
}
