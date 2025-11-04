using GamiPort.Areas.Forum.Dtos;
using GamiPort.Areas.Forum.Dtos.AdminPosts;
using GamiPort.Areas.Forum.Services.Adminpost;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;



namespace GamiPort.Areas.Forum.ApiControllers
{
    [Area("Forum")]
    [ApiController]
    [Route("api/posts")] // 前台公開路由：不綁 Area 前綴，網址更短
    public sealed class PostsApiController : ControllerBase
    {
        private readonly IPostsService _svc;

        public PostsApiController(IPostsService svc)
        {
            _svc = svc;
        }

        /// <summary>
        /// 前台列表：僅回 published，置頂優先，支援 gameId 篩選
        /// GET /api/posts?type=insight&gameId=123&page=1&size=20
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFront([FromQuery] PostQuery query)
        {
            var result = await _svc.GetFrontPostsAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// 前台單筆：僅回 published，否則 404
        /// GET /api/posts/{postId}
        /// </summary>
        [HttpGet("{postId:int}")]
        public async Task<IActionResult> GetFrontById([FromRoute] int postId)
        {
            var dto = await _svc.GetFrontPostAsync(postId);
            if (dto == null) return NotFound();
            return Ok(dto);
        }
    }
}
