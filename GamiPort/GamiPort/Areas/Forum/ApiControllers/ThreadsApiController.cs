using GamiPort.Areas.Forum.Dtos.Common;
using GamiPort.Areas.Forum.Dtos.Threads;
using GamiPort.Areas.Forum.Services.Threads;
using Microsoft.AspNetCore.Mvc;


namespace GamiPort.Areas.Forum.ApiControllers
{
    [Area("Forum")]
    [ApiController]
    [Route("api/threads")]
    public class ThreadsApiController : ControllerBase
    {
        private readonly IThreadsService _svc;
        public ThreadsApiController(IThreadsService svc) => _svc = svc;

        // GET /api/threads/{id}
        [HttpGet("{id:long}")]
        public async Task<ActionResult<ThreadDetailDto>> One([FromRoute] long id)
        {
            var dto = await _svc.GetThreadAsync(id);
            return dto is null ? NotFound() : Ok(dto);
        }

        // GET /api/threads/{id}/posts?page=1&size=50
        [HttpGet("{id:long}/posts")]
        public async Task<ActionResult<PagedResult<ThreadPostRowDto>>> Posts(
            [FromRoute] long id, [FromQuery] int page = 1, [FromQuery] int size = 50)
            => Ok(await _svc.GetThreadPostsAsync(id, page, size));
    }
}
