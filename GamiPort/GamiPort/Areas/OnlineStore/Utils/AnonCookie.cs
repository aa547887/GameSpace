using Microsoft.AspNetCore.Http;

namespace GamiPort.Areas.OnlineStore.Utils
{
	public static class AnonCookie
	{
		public const string Key = "gp_anonymous";

		public static Guid GetOrSet(HttpContext ctx)
		{
			// 1) 已有就讀出
			if (ctx.Request.Cookies.TryGetValue(Key, out var v) && Guid.TryParse(v, out var g))
				return g;

			// 2) 沒有就產生並寫入
			var guid = Guid.NewGuid();
			ctx.Response.Cookies.Append(Key, guid.ToString(), new CookieOptions
			{
				HttpOnly = true,
				Secure = ctx.Request.IsHttps,   // 本機無 HTTPS 就會自動用非 Secure
				SameSite = SameSiteMode.Lax,
				Expires = DateTimeOffset.UtcNow.AddDays(30)
			});
			return guid;
		}
	}
}