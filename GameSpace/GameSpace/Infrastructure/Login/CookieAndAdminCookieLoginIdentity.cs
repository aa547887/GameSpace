// Infrastructure/Login/CookieAndAdminCookieLoginIdentity.cs
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace GameSpace.Infrastructure.Login
{
	/// <summary>
	/// 登入身分解析：
	/// 1) 優先從 AdminCookie（外部系統）解析 claims；
	/// 2) 解析不到再退回舊制 cookies：gs_id / gs_kind；
	/// 3) 不查資料庫、不改外部登入流程。
	/// </summary>
	public class CookieAndAdminCookieLoginIdentity : ILoginIdentity
	{
		private readonly IHttpContextAccessor _http;
		private const string AdminCookieScheme = "AdminCookie";

		public CookieAndAdminCookieLoginIdentity(IHttpContextAccessor http) => _http = http;

		/// <summary>
		/// 取得目前請求的登入身分（介面需要 CancellationToken 參數）。
		/// </summary>
		public async Task<LoginIdentityResult> GetAsync(CancellationToken ct = default)
		{
			var ctx = _http.HttpContext;
			if (ctx == null || ct.IsCancellationRequested) return new LoginIdentityResult();

			// ① 優先：外部 AdminCookie（claims 來源可靠）
			var auth = await ctx.AuthenticateAsync(AdminCookieScheme);
			if (auth.Succeeded && auth.Principal != null)
			{
				var p = auth.Principal;

				// 取 id：優先 NameIdentifier；其次 mgr:id / usr:id
				int? id = null;
				var idStr = p.FindFirst(ClaimTypes.NameIdentifier)?.Value
							?? p.FindFirst("mgr:id")?.Value
							?? p.FindFirst("usr:id")?.Value;
				if (int.TryParse(idStr, out var parsed)) id = parsed;

				// 推導 kind：
				// - IsManager=true
				// - 或 perm:Admin=true
				// - 或存在 mgr:id
				var isManager =
					string.Equals(p.FindFirst("IsManager")?.Value, "true", System.StringComparison.OrdinalIgnoreCase) ||
					string.Equals(p.FindFirst("perm:Admin")?.Value, "true", System.StringComparison.OrdinalIgnoreCase) ||
					p.HasClaim(c => c.Type == "mgr:id");

				if (id.HasValue)
				{
					if (isManager)
						return new LoginIdentityResult
						{
							ManagerId = id.Value,
							Kind = "manager",
							DisplayName = $"Manager#{id.Value}"
						};

					return new LoginIdentityResult
					{
						UserId = id.Value,
						Kind = "user",
						DisplayName = $"User#{id.Value}"
					};
				}
				// 有登入但沒有可解析的 id → 視為 guest
			}

			// ② 退回舊制 cookies（gs_id / gs_kind）
			var idCookie = ctx.Request.Cookies["gs_id"];
			var kindCookie = ctx.Request.Cookies["gs_kind"]?.ToLowerInvariant();

			if (int.TryParse(idCookie, out var id2) && !string.IsNullOrWhiteSpace(kindCookie))
			{
				return kindCookie == "manager"
					? new LoginIdentityResult { ManagerId = id2, Kind = "manager", DisplayName = $"Manager#{id2}" }
					: new LoginIdentityResult { UserId = id2, Kind = "user", DisplayName = $"User#{id2}" };
			}

			// ③ 都沒有 → guest
			return new LoginIdentityResult();
		}
	}
}
