using GamiPort.Areas.social_hub.Services.Abstractions; // IProfanityFilter
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace GamiPort.Areas.social_hub.Controllers
{
	/// <summary>
	/// 前端用的穢語規則 API。
	/// 特色：
	/// - GET /social_hub/profanity/list?nocache=1：當 nocache=1 時，先強制 Reload 再回傳。
	/// - 規則為已轉義 literal regex（pattern+flags+replacement）。
	/// - 由「好友清單」開啟時統一呼叫本端點，聊天室僅使用規則，不再自行抓取。
	/// </summary>
	[Area("social_hub")]
	[ApiController]
	[Route("social_hub/profanity")]
	public sealed class ProfanityController : ControllerBase
	{
		private readonly IProfanityFilter _filter;
		public ProfanityController(IProfanityFilter filter) => _filter = filter;

		/// <summary>
		/// 取前端用的遮蔽規則。
		/// 可用 querystring：nocache=1 → 先強制重載 DB 規則（立即生效）。
		/// </summary>
		[HttpGet("list")]
		[AllowAnonymous] // 前台聊天也要能取，視需要改為 [Authorize]
		public async Task<IActionResult> GetRules([FromQuery] int? nocache = null)
		{
			// 好友清單開啟會帶 nocache=1，強制即時重載
			if (nocache == 1)
				await _filter.ReloadAsync();

			var (ver, rules) = _filter.GetClientRules();
			return Ok(new
			{
				version = ver,
				rules = rules.Select(r => new { pattern = r.pattern, replacement = r.replacement, flags = r.flags })
			});
		}

		/// <summary>
		/// 手動刷新（後台需要時可呼叫）。可加權限限制。
		/// </summary>
		[HttpPost("reload")]
		public async Task<IActionResult> Reload()
		{
			await _filter.ReloadAsync();
			return NoContent();
		}
	}
}
