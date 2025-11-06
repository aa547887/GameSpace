using GamiPort.Areas.Forum.Dtos.Me;
using GamiPort.Areas.Forum.Dtos.Common;
namespace GamiPort.Areas.Forum.Services.Me
{
    public interface IMeContentService
    {
        Task<PagedResult<MyThreadRowDto>> GetMyThreadsAsync(long userId, string sort, int page, int size,
        CancellationToken ct = default);
        Task<PagedResult<MyPostRowDto>> GetMyPostsAsync(long userId, string sort, int page, int size,
        CancellationToken ct = default);
        Task<PagedResult<MyLikedThreadRowDto>> GetMyLikedThreadsAsync(long userId, string sort, int page, int size,
        CancellationToken ct = default);
    }
}
