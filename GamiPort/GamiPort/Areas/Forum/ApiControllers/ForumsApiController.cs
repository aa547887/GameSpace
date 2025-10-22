using GamiPort.Areas.Forum.Dtos.Common;
using GamiPort.Areas.Forum.Dtos.Forum;
using GamiPort.Areas.Forum.Dtos.Threads;
using GamiPort.Areas.Forum.Services.Forums;
using Microsoft.AspNetCore.Mvc;


namespace GamiPort.Areas.Forum.ApiControllers
{
    [Area("Forum")]
    [ApiController]
    [Route("api/forums")]
    public class ForumsApiController : ControllerBase
    {
        private readonly IForumsService _svc;
        public ForumsApiController(IForumsService svc) => _svc = svc;

        // 1) 論壇清單
        // GET /api/forums
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ForumListItemDto>>> List()
            => Ok(await _svc.GetForumsAsync());

        // 2) 取得單一論壇
        // GET /api/forums/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ForumDetailDto>> One([FromRoute] int id)
        {
            var dto = await _svc.GetForumAsync(id);
            return dto is null ? NotFound() : Ok(dto);
        }

        // 3) 由 gameId 找論壇
        // GET /api/forums/by-game/{gameId}
        [HttpGet("by-game/{gameId:int}")]
        public async Task<ActionResult<ForumDetailDto>> ByGame([FromRoute] int gameId)
        {
            var dto = await _svc.GetForumByGameAsync(gameId);
            return dto is null ? NotFound() : Ok(dto);
        }

        // 4) 取該論壇的主題列表（分頁 + 排序）
        // GET /api/forums/{id}/threads?sort=lastReply|created|hot&page=1&size=20
        [HttpGet("{id:int}/threads")]
        public async Task<ActionResult<PagedResult<ThreadListItemDto>>> Threads(
            [FromRoute] int id, [FromQuery] string sort = "lastReply",
            [FromQuery] int page = 1, [FromQuery] int size = 20)
            => Ok(await _svc.GetThreadsByForumAsync(id, sort, page, size));
    }
}
