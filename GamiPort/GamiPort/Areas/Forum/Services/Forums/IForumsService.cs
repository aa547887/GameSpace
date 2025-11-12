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


        // 新版：7 + ct
        Task<PagedResult<ThreadListItemDto>> GetThreadsByForumAsync(
    int forumId, string sort, int page, int size,
    string? keyword = null, bool inContent = false, bool inGame = false,
    long currentUserId = 0, CancellationToken ct = default);


        // ✅ 新增：搜尋論壇（依中文+英文+看板名）
        Task<IReadOnlyList<ForumListItemDto>> SearchForumsAsync(
            string keyword, CancellationToken ct = default);

        // ✅ 新增：依遊戲名找論壇（跳轉用）
        Task<ForumDetailDto?> GetForumByGameNameAsync(
            string gameName, CancellationToken ct = default);
        //跨所有論壇搜主題

        Task<PagedResult<GlobalThreadSearchResultDto>> SearchThreadsAcrossForumsAsync(
    string keyword, int page, int size, long currentUserId, CancellationToken ct);
    }
}
