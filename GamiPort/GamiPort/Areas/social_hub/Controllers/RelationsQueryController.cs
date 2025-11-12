using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamiPort.Models;

namespace GamiPort.Areas.social_hub.Controllers
{
    [Area("social_hub")]
    [ApiController]
    [Route("social_hub/relations")] // shares base with RelationsController
    public sealed class RelationsQueryController : ControllerBase
    {
        private readonly GameSpacedatabaseContext _db;
        public RelationsQueryController(GameSpacedatabaseContext db) => _db = db;

        // GET social_hub/relations/friends?onlyAccepted=true&take=200
        [HttpGet("friends")]
        public async Task<IActionResult> Friends(
            [FromQuery] bool onlyAccepted = true,
            [FromQuery] int take = 200,
            CancellationToken ct = default)
        {
            var currentUserId = User.Identity?.IsAuthenticated == true
                ? int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0")
                : 0;

            if (currentUserId == 0) return Unauthorized();

            var friends = await _db.Relations
                .AsNoTracking()
                .Where(r => (r.UserIdSmall == currentUserId || r.UserIdLarge == currentUserId) && r.StatusId == 2) // StatusId 2 for ACCEPTED
                .Include(r => r.UserIdLargeNavigation)
                .Include(r => r.UserIdSmallNavigation)
                .Select(r => new
                {
                    FriendUserId = r.UserIdSmall == currentUserId ? r.UserIdLarge : r.UserIdSmall,
                    FriendName = (r.UserIdSmall == currentUserId
                                  ? (r.UserIdLargeNavigation != null ? r.UserIdLargeNavigation.UserName : null)
                                  : (r.UserIdSmallNavigation != null ? r.UserIdSmallNavigation.UserName : null)) ?? "",
                })
                .Take(take)
                .ToListAsync(ct);

            return Ok(friends);
        }

        // GET social_hub/relations/pair-info?a=<userId1>&b=<userId2>
        [HttpGet("pair-info")]
        public async Task<IActionResult> PairInfo([FromQuery] int a, [FromQuery] int b, CancellationToken ct)
        {
            if (a <= 0 || b <= 0) return BadRequest(new { reason = "invalid user ids" });
            var small = Math.Min(a, b);
            var large = Math.Max(a, b);

            var info = await _db.Relations
                .AsNoTracking()
                .Where(r => r.UserIdSmall == small && r.UserIdLarge == large)
                .Select(r => new
                {
                    r.RequestedBy,
                    StatusCode = _db.RelationStatuses
                        .Where(s => s.StatusId == r.StatusId)
                        .Select(s => s.StatusCode)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(ct);

            if (info is null)
                return Ok(new { statusCode = "NONE", requestedBy = (int?)null });

            return Ok(new { statusCode = info.StatusCode ?? "NONE", requestedBy = info.RequestedBy });
        }
    }
}

