using GamiPort.Areas.Forum.Dtos.Common;
using GamiPort.Areas.Forum.Dtos.Threads;
using GamiPort.Areas.Forum.Services.Threads;
using GamiPort.Areas.Forum.Services.Threads.GamiPort.Areas.Forum.Services.Threads;
using GamiPort.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace GamiPort.Areas.Forum.ApiControllers
{
    [Area("Forum")]
    [Route("api/[area]/threads")]
    [ApiController]
    public class ThreadsApiController : ControllerBase
    {
        private readonly IThreadsService _svc;
        private readonly ICurrentUserService _id;
        public ThreadsApiController(IThreadsService svc, ICurrentUserService id)
        {
            _svc = svc;
            _id = id;
        }
        private long CurrentUserId => _id.UserId ?? 0;

        // TODO: 之後從 Claims 取

        //private long CurrentUserId => User?.Identity?.IsAuthenticated == true
        //    ? long.Parse(User.FindFirst("sub")!.Value) // 依你系統調整
        //    : 0;

        // 1) 取得主題詳情
        [HttpGet("{threadId:long}")]
        public async Task<IActionResult> GetThread(long threadId)
        {
            var dto = await _svc.GetThreadAsync(threadId, CurrentUserId);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // 2) 取得樓層列表（分頁 + 排序）
        [HttpGet("{threadId:long}/posts")]
        public async Task<IActionResult> GetPosts(long threadId, [FromQuery] string sort = "oldest",
            [FromQuery] int page = 1, [FromQuery] int size = 20)
        {
            var result = await _svc.GetThreadPostsAsync(threadId, sort, page, size);
            return Ok(result);
        }

        // 3) 新增主題（發文）
        [Authorize] // [AUXX]
        [HttpPost]
        public async Task<IActionResult> CreateThread([FromBody] CreateThreadRequest req)
        {
            var id = await _svc.CreateThreadAsync(CurrentUserId, req.ForumId, req.Title, req.ContentMd);
            return Created($"/api/forum/threads/{id}", new { threadId = id });
        }

        // 4) 新增回覆（回文）
        [Authorize] // [AUXX]
        [HttpPost("{threadId:long}/posts")]
        public async Task<IActionResult> CreatePost(long threadId, [FromBody] CreatePostRequest req)
        {
            var postId = await _svc.CreatePostAsync(CurrentUserId, threadId, req.ContentMd, req.ParentPostId);
            return Ok(new { postId });
        }

        // 5) 對主題按讚 / 取消讚（toggle）
        [Authorize] // [AUXX]
        [HttpPost("{threadId:long}/like")]
        public async Task<IActionResult> ToggleLike(long threadId)
        {
            var liked = await _svc.ToggleThreadLikeAsync(CurrentUserId, threadId);
            return Ok(new { liked });
        }

        // 6) 主題是否已按讚 / 讚數
        [HttpGet("{threadId:long}/like/status")]
        public async Task<IActionResult> LikeStatus(long threadId)
        {
            var dto = await _svc.GetThreadLikeStatusAsync(CurrentUserId, threadId);
            return Ok(new { isLiked = dto.IsLiked, likeCount = dto.LikeCount });
        }


        //7)
        [HttpPost("/api/forum/posts/{postId:long}/like")]
        public async Task<IActionResult> TogglePostLike(long postId)
        {
            var liked = await _svc.TogglePostLikeAsync(CurrentUserId, postId);
            return Ok(new { liked });
        }

        [HttpGet("/api/forum/posts/{postId:long}/like/status")]
        public async Task<IActionResult> PostLikeStatus(long postId)
        {
            var dto = await _svc.GetPostLikeStatusAsync(CurrentUserId, postId);
            return Ok(new { isLiked = dto.IsLiked, likeCount = dto.LikeCount });
        }


        //[HttpGet("whoami")]
        //public IActionResult WhoAmI()
        //{
        //    return Ok(new
        //    {
        //        IsAuth = User.Identity?.IsAuthenticated,
        //        UserId = User.FindFirst("sub")?.Value,   // 你 JWT / Cookie mapping 的 userId 欄位
        //        Name = User.Identity?.Name
        //    });
        //}

        [HttpGet("debug/whoami")]
        public IActionResult WhoAmI()
        {
            return Ok(new
            {
                currentUserId = CurrentUserId,
                isAuth = User?.Identity?.IsAuthenticated ?? false,
                claims = User?.Claims.Select(c => new { c.Type, c.Value })
            });
        }


    }
}
