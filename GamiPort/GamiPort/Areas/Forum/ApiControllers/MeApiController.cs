using GamiPort.Areas.Forum.Services.Me;
using GamiPort.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.Forum.ApiControllers
{
    [Area("Forum")]
    [Route("api/[area]/me")]
    [ApiController]
    [Authorize] // 這支整個需要登入
    public class MeApiController : ControllerBase
    {
        private readonly IMeContentService _svc;
        private readonly ICurrentUserService _me;

        public MeApiController(IMeContentService svc, ICurrentUserService me)
        {
            _svc = svc;
            _me = me;
        }

        private static (int page, int size) Normalize(int page, int size)
        {
            if (page < 1) page = 1;
            if (size < 1) size = 20;
            if (size > 100) size = 100;
            return (page, size);
        }

        [HttpGet("threads")]
        public async Task<IActionResult> MyThreads(
            [FromQuery] string sort = "latest",
            [FromQuery] int page = 1,
            [FromQuery] int size = 20,
            CancellationToken ct = default)
        {
            var uid = _me.UserId;               // int?
            if (uid is null) return Unauthorized();

            (page, size) = Normalize(page, size);
            var data = await _svc.GetMyThreadsAsync(uid.Value, sort, page, size, ct);
            return Ok(data);
        }

        [HttpGet("posts")]
        public async Task<IActionResult> MyPosts(
            [FromQuery] string sort = "latest",
            [FromQuery] int page = 1,
            [FromQuery] int size = 20,
            CancellationToken ct = default)
        {
            var uid = _me.UserId;
            if (uid is null) return Unauthorized();

            (page, size) = Normalize(page, size);
            var data = await _svc.GetMyPostsAsync(uid.Value, sort, page, size, ct);
            return Ok(data);
        }

        [HttpGet("likes/threads")]
        public async Task<IActionResult> MyLikedThreads(
            [FromQuery] string sort = "latestLiked",
            [FromQuery] int page = 1,
            [FromQuery] int size = 20,
            CancellationToken ct = default)
        {
            var uid = _me.UserId;
            if (uid is null) return Unauthorized();

            (page, size) = Normalize(page, size);
            var data = await _svc.GetMyLikedThreadsAsync(uid.Value, sort, page, size, ct);
            return Ok(data);
        }
    }
}
