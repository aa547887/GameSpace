using GamiPort.Areas.Forum.Services.Me;
using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.Forum.ApiControllers
{
    [Area("Forum")]
    [Route("api/[area]/me")]
    [ApiController]
    public class MeApiController : ControllerBase
    {
        private readonly IMeContentService _svc;
        public MeApiController(IMeContentService svc) => _svc = svc;

        // 測試階段：先寫死；之後接 JWT 再改 Claims。
        private long CurrentUserId => 10000001;

        // 之後開啟：
        // private long CurrentUserId => long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        //[Authorize] // 之後開
        [HttpGet("threads")]
        public async Task<IActionResult> MyThreads([FromQuery] string sort = "latest", [FromQuery] int page = 1, [FromQuery] int size = 20)
            => Ok(await _svc.GetMyThreadsAsync(CurrentUserId, sort, page, size));

        //[Authorize]
        [HttpGet("posts")]
        public async Task<IActionResult> MyPosts([FromQuery] string sort = "latest", [FromQuery] int page = 1, [FromQuery] int size = 20)
            => Ok(await _svc.GetMyPostsAsync(CurrentUserId, sort, page, size));

        //[Authorize]
        [HttpGet("likes/threads")]
        public async Task<IActionResult> MyLikedThreads([FromQuery] string sort = "latestLiked", [FromQuery] int page = 1, [FromQuery] int size = 20)
            => Ok(await _svc.GetMyLikedThreadsAsync(CurrentUserId, sort, page, size));
    }
}
