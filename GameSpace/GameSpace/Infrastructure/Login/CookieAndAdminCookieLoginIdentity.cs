using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace GameSpace.Infrastructure.Login
{
	/// <summary>
	/// 先讀外部的 AdminCookie（你的系統為主），讀不到再退回 gs_*（保留舊快速登入相容）。
	/// 不查資料庫、不改外部登入流程。
	/// </summary>
	public class CookieAndAdminCookieLoginIdentity : ILoginIdentity
	{
		private readonly IHttpContextAccessor _http;
		private const string AdminCookieScheme = "AdminCookie";

		public CookieAndAdminCookieLoginIdentity(IHttpContextAccessor http) => _http = http;

		public async Task<LoginIdentityResult> GetAsync(CancellationToken ct = default)
		{
			var ctx = _http.HttpContext;
			if (ctx == null) return new LoginIdentityResult();

			// ① 優先：外部 AdminCookie（claims 來源可靠）
			var auth = await ctx.AuthenticateAsync(AdminCookieScheme);
			if (auth.Succeeded && auth.Principal is ClaimsPrincipal p)
			{
				var idStr = p.FindFirst("mgr:id")?.Value
							?? p.FindFirst(ClaimTypes.NameIdentifier)?.Value;

				if (int.TryParse(idStr, out var mid))
				{
					var isManager =
						p.HasClaim("IsManager", "true") ||
						p.HasClaim(ClaimTypes.Role, "Manager");

					return new LoginIdentityResult
					{
						ManagerId = mid,
						Kind = isManager ? "manager" : "user",
						DisplayName = p.FindFirst(ClaimTypes.Name)?.Value ?? $"#{mid}"
					};
				}
			}

			// ② 次要：舊制 gs_*（你保留的快速登入）
			var idCookie = ctx.Request.Cookies["gs_id"];
			var kindCookie = ctx.Request.Cookies["gs_kind"]?.ToLowerInvariant();

			if (int.TryParse(idCookie, out var id) && !string.IsNullOrWhiteSpace(kindCookie))
			{
				return kindCookie == "manager"
					? new LoginIdentityResult { ManagerId = id, Kind = "manager", DisplayName = $"Manager#{id}" }
					: new LoginIdentityResult { UserId = id, Kind = "user", DisplayName = $"User#{id}" };
			}

			// ③ 都沒有 → guest
			return new LoginIdentityResult();
		}
	}
}
