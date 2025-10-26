using GamiPort.Areas.Forum.Dtos.Common;
using GamiPort.Areas.Forum.Dtos.Threads;

namespace GamiPort.Areas.Forum.Services.Threads
{
    public interface IThreadsService
    {
        Task<ThreadDetailDto?> GetThreadAsync(long threadId);
        Task<PagedResult<ThreadPostRowDto>> GetThreadPostsAsync(long threadId, int page, int size);
    }
}
