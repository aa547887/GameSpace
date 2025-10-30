using GamiPort.Areas.Forum.Dtos.Common;
using GamiPort.Areas.Forum.Dtos.Forum;
using GamiPort.Areas.Forum.Dtos.Threads;
using GamiPort.Areas.Forum.Services.Forums;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;


namespace GamiPort.Areas.Forum.ApiControllers
{


    // 把這支 Controller 放到 "Forum" Area（路由/檔案歸位）
    [Area("Forum")]

    // ApiController 會啟用一堆 Web API 友善預設：
    // 1) 自動 ModelState 驗證 2) 自動從 query/route/body 綁定參數
    // 3) 400 問題細節回應（ProblemDetails）等
    [ApiController]

    // 路由前綴：所有 action 都會長在 /api/forums/... 底下
    [Route("api/forums")]
    public class ForumsApiController : ControllerBase // API 用 ControllerBase（不需要 View 支援）
    {
        private readonly IForumsService _svc;

        // 透過 DI 拿到服務層（商業邏輯都在 Service）
        public ForumsApiController(IForumsService svc) => _svc = svc;

        // 1) 取得論壇清單（用在論壇首頁清單）
        // HTTP: GET /api/forums
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ForumListItemDto>>> List()
            // 直接把 Service 資料丟回去 → 200 OK + JSON
            => Ok(await _svc.GetForumsAsync());

        // 2) 取得單一論壇詳情
        // HTTP: GET /api/forums/{id}
        [HttpGet("{id:int}")] // :int 代表路由約束，只接受整數
        public async Task<ActionResult<ForumDetailDto>> One([FromRoute] int id)
        {
            // 從 Service 查資料
            var dto = await _svc.GetForumAsync(id);

            // 查不到 → 404 Not Found；查到 → 200 OK + JSON
            return dto is null ? NotFound() : Ok(dto);
        }

        // 3) 由遊戲 ID 反查該遊戲的論壇
        // HTTP: GET /api/forums/by-game/{gameId}
        [HttpGet("by-game/{gameId:int}")]
        public async Task<ActionResult<ForumDetailDto>> ByGame([FromRoute] int gameId)
        {
            var dto = await _svc.GetForumByGameAsync(gameId);
            return dto is null ? NotFound() : Ok(dto);
        }

        // 4) 取該論壇的主題列表（支援排序 + 分頁）
        // HTTP: GET /api/forums/{id}/threads?sort=lastReply|created|hot&page=1&size=20
        [HttpGet("{id:int}/threads")]
        public async Task<ActionResult<PagedResult<ThreadListItemDto>>> Threads(
            [FromRoute] int id,                     // 路由的論壇 id
            [FromQuery] string sort = "lastReply",  // 排序（預設 lastReply）
            [FromQuery] int page = 1,               // 第幾頁（預設 1）
            [FromQuery] int size = 20,              // 每頁幾筆（預設 20）
                                                    // 丟給 Service 做真正的搜尋/排序/分頁邏輯
                                                    // ▼ 新增三個，預設不搜尋內容/遊戲，只搜標題
            [FromQuery] string? keyword = null,
            [FromQuery] bool inContent = false,
            [FromQuery] bool inGame = false
)
        {
            // 只多把參數丟給 Service，其他不動
            var result = await _svc.GetThreadsByForumAsync(id, sort, page, size, keyword, inContent, inGame);
            return Ok(result);
        }

        //=> Ok(await _svc.GetThreadsByForumAsync(id, sort, page, size));
    

        // GET /api/forums/search?keyword=LOL
        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyList<ForumListItemDto>>> SearchForums(
            [FromQuery] string keyword, CancellationToken ct = default)
        {
            var rows = await _svc.SearchForumsAsync(keyword, ct);
            return Ok(rows);
        }

        // GET /api/forums/by-game-name/{name}
        [HttpGet("by-game-name/{name}")]
        public async Task<ActionResult<ForumDetailDto>> ByGameName(
            [FromRoute] string name, CancellationToken ct = default)
        {
            var dto = await _svc.GetForumByGameNameAsync(name, ct);
            return dto is null ? NotFound() : Ok(dto);
        }
    }


    }
