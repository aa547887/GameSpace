using GamiPort.Areas.Forum.Dtos.Common;
using GamiPort.Areas.Forum.Dtos.Threads;

namespace GamiPort.Areas.Forum.Services.Threads
{
    using System.Threading.Tasks;

    namespace GamiPort.Areas.Forum.Services.Threads
    {
        public interface IThreadsService
        {
            Task<ThreadDetailDto?> GetThreadAsync(long threadId, long currentUserId);

            // sort: "oldest" (default) | "newest" | "mostLiked"
            Task<PagedResult<ThreadPostItemDto>> GetThreadPostsAsync(
    long threadId, string sort, int page, int size,
    long currentUserId, CancellationToken ct = default);

            Task<long> CreateThreadAsync(long userId, int forumId, string title, string contentMd);

            Task<long> CreatePostAsync(long userId, long threadId, string contentMd, long? parentPostId);

            // true=現在是已按讚，false=現在是已取消
            Task<bool> ToggleThreadLikeAsync(long userId, long threadId);

            Task<LikeStatusDto> GetThreadLikeStatusAsync(long userId, long threadId);

            Task<bool> TogglePostLikeAsync(long userId, long postId);

            Task<(bool IsLiked, int LikeCount)> GetPostLikeStatusAsync(long userId, long postId);

            Task<bool> DeleteThreadAsync(long userId, long threadId, CancellationToken ct = default);
            Task<bool> DeletePostAsync(long userId, long postId, CancellationToken ct = default);
        }
    }

}
