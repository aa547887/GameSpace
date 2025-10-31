using Microsoft.AspNetCore.Http;
using System;

namespace GamiPort.Areas.OnlineStore.Infrastructure
{
	public static class CartCookie
	{
		private const string CookieName = "cart_id";
		private const int Days = 30;

		public static Guid GetOrCreate(HttpContext http)
		{
			if (http.Request.Cookies.TryGetValue(CookieName, out var raw) &&
				Guid.TryParse(raw, out var id) && id != Guid.Empty)
			{
				return id;
			}

			var newId = Guid.NewGuid();
			var opts = new CookieOptions
			{
				HttpOnly = false,
				IsEssential = true,
				SameSite = SameSiteMode.Lax,
				Secure = http.Request.IsHttps,
				Expires = DateTimeOffset.UtcNow.AddDays(Days)
			};
			http.Response.Cookies.Append(CookieName, newId.ToString(), opts);
			return newId;
		}

		public static Guid Get(HttpContext http)
		{
			if (http.Request.Cookies.TryGetValue(CookieName, out var raw) &&
				Guid.TryParse(raw, out var id))
				return id;
			return Guid.Empty;
		}

		public static void Drop(HttpContext http)
		{
			http.Response.Cookies.Delete(CookieName);
		}
	}
}
