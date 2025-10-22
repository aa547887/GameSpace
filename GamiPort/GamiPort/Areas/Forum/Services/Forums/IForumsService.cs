using  GamiPort.Areas.Forum.Dtos.Common;
using GamiPort.Areas.Forum.Dtos.Forum;
using GamiPort.Areas.Forum.Dtos.Threads;

namespace GamiPort.Areas.Forum.Services.Forums
{
    public interface IForumsService
    {
        Task<IReadOnlyList<ForumListItemDto>> GetForumsAsync();
        Task<ForumDetailDto?> GetForumAsync(int forumId);
        Task<ForumDetailDto?> GetForumByGameAsync(int gameId);

        Task<PagedResult<ThreadListItemDto>> GetThreadsByForumAsync(
            int forumId, string sort, int page, int size);
    }
}
