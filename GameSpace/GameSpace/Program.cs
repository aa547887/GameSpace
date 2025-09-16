// ---- 服務命名空間（一般 using）----
using GameSpace.Areas.social_hub.Services;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
// 權限服務的別名
using IManagerPermissionServiceAlias = GameSpace.Areas.social_hub.Services.IManagerPermissionService;
// ---- 型別別名（避免方案裡若有重複介面/命名空間不一致，導致 DI 對不到）----
using IMuteFilterAlias = GameSpace.Areas.social_hub.Services.IMuteFilter;
using INotificationServiceAlias = GameSpace.Areas.social_hub.Services.INotificationService;
using ManagerPermissionServiceAlias = GameSpace.Areas.social_hub.Services.ManagerPermissionService;
using MuteFilterAlias = GameSpace.Areas.social_hub.Services.MuteFilter;
using NotificationServiceAlias = GameSpace.Areas.social_hub.Services.NotificationService;

namespace GameSpace
{
	public class Program
	{
		public static async Task Main(string[] args) // ⬅ async Main（用於啟動預熱）
		{
			var builder = WebApplication.CreateBuilder(args);

			// ===== 連線字串 =====
			var identityConn = builder.Configuration.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
			var gameSpaceConn = builder.Configuration.GetConnectionString("GameSpace")
				?? throw new InvalidOperationException("Connection string 'GameSpace' not found.");

			// ===== DbContexts =====
			builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer(identityConn));
			builder.Services.AddDbContext<GameSpacedatabaseContext>(opt => opt.UseSqlServer(gameSpaceConn));

			// 開發期資料庫錯誤頁
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// ===== Identity =====
			builder.Services
				.AddDefaultIdentity<IdentityUser>(opt => opt.SignIn.RequireConfirmedAccount = true)
				.AddEntityFrameworkStores<ApplicationDbContext>();

			// ===== MVC / Razor =====
			builder.Services.AddControllersWithViews();
			builder.Services.AddRazorPages();

			// ===== social_hub 相關服務註冊 =====
			builder.Services.AddMemoryCache();

			// 穢語過濾選項（MuteFilter 透過 IOptions 取得）
			builder.Services.Configure<GameSpace.Areas.social_hub.Services.MuteFilterOptions>(o =>
			{
				o.MaskStyle = GameSpace.Areas.social_hub.Services.MaskStyle.Asterisks; // replacement 空時用
				o.FixedLabel = "【封鎖】";
				o.FuzzyBetweenCjkChars = true;
				o.MaskExactLength = false;
				o.CacheTtlSeconds = 30;

				// ★ 開啟每詞自訂替代（用 Mutes.replacement）
				o.UsePerWordReplacement = true;
			});
			// 用別名註冊，避免撞名
			builder.Services.AddScoped<IMuteFilterAlias, MuteFilterAlias>();
			builder.Services.AddScoped<INotificationServiceAlias, NotificationServiceAlias>();
			builder.Services.AddScoped<IManagerPermissionServiceAlias, ManagerPermissionServiceAlias>();


// [保留] SignalR
builder.Services.AddSignalR();

// [新增] Session（登入流程/OTP 要用）
builder.Services.AddSession(opt =>
{
	opt.IdleTimeout = TimeSpan.FromMinutes(30);
	opt.Cookie.HttpOnly = true;
	opt.Cookie.IsEssential = true;
});

// ★★ 重要：獨立的後台 Cookie 方案，避免干擾 Identity 的 Cookie（Identity 仍用它的）
const string AdminCookieScheme = "AdminCookie";

// [新增] 後台 Cookie 驗證（給 LoginController 用）+「AJAX 401 不重導」
builder.Services.AddAuthentication(options =>
{
	// 不動 Identity 的預設方案，只是額外加一個後台 Cookie 方案
})
.AddCookie(AdminCookieScheme, opt =>
{
	opt.LoginPath = "/Login";
	opt.LogoutPath = "/Login/Logout";
	opt.AccessDeniedPath = "/Login/Denied";
	opt.Cookie.Name = "GameSpace.Admin";
	opt.SlidingExpiration = true;
	opt.ExpireTimeSpan = TimeSpan.FromHours(4);

	// ★ 讓 AJAX/API 端點在未登入/無權限時回 401/403，而不是 Redirect
	opt.Events = new CookieAuthenticationEvents
	{
		OnRedirectToLogin = ctx =>
		{
			// 針對你會用 fetch 的端點（例如 /Login/Me）直接回 401
			if (ctx.Request.Path.StartsWithSegments("/Login/Me", StringComparison.OrdinalIgnoreCase))
			{
				ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return Task.CompletedTask;
			}

			// 一般 AJAX 判斷：X-Requested-With 或 Accept 要 JSON
			var isAjax =
				string.Equals(ctx.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
				|| ctx.Request.Headers["Accept"].Any(v => v?.Contains("json", StringComparison.OrdinalIgnoreCase) == true);

			if (isAjax)
			{
				ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return Task.CompletedTask;
			}

			// 其餘情況維持原本導向登入頁
			ctx.Response.Redirect(ctx.RedirectUri);
			return Task.CompletedTask;
		},
		OnRedirectToAccessDenied = ctx =>
		{
			// 無權限時（403）也同樣處理 AJAX
			var isAjax =
				string.Equals(ctx.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
				|| ctx.Request.Headers["Accept"].Any(v => v?.Contains("json", StringComparison.OrdinalIgnoreCase) == true);

			if (isAjax)
			{
				ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
				return Task.CompletedTask;
			}

			ctx.Response.Redirect(ctx.RedirectUri);
			return Task.CompletedTask;
		}
	};
});

// ★ 建立授權政策（對應我們寫入的 perm:* Claims）
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("CanManageShopping", p => p.RequireClaim("perm:Shopping", "true"));
	options.AddPolicy("CanAdmin", p => p.RequireClaim("perm:Admin", "true"));
	options.AddPolicy("CanMessage", p => p.RequireClaim("perm:Message", "true"));
	options.AddPolicy("CanUserStatus", p => p.RequireClaim("perm:UserStat", "true"));
	options.AddPolicy("CanPet", p => p.RequireClaim("perm:Pet", "true"));
	options.AddPolicy("CanCS", p => p.RequireClaim("perm:CS", "true"));
});

var app = builder.Build();

			// ===== 啟動預熱（失敗不擋站）=====
			using (var scope = app.Services.CreateScope())
			{
				try
				{
					var filter = scope.ServiceProvider.GetRequiredService<IMuteFilterAlias>();
					await filter.RefreshAsync(); // 預熱 Regex 與快取
				}
				catch { /* ignore */ }
			}

			// ===== Middleware 管線 =====
			if (app.Environment.IsDevelopment())
			{
				app.UseMigrationsEndPoint();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();          // [順序確認] 要在 Auth 前
app.UseAuthentication();   // 會同時支援 Identity 的 Cookie 與 AdminCookie
app.Use(async (ctx, next) =>
{
	var admin = await ctx.AuthenticateAsync("AdminCookie");
	if (admin.Succeeded && admin.Principal != null)
	{
		if (!(ctx.User?.Identity?.IsAuthenticated ?? false))
			ctx.User = admin.Principal;
	}
	await next();
});
app.UseAuthorization();

			// ===== 路由 =====
			// 先加 area 的路由（重要）
			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

// [保留] 若使用 Identity UI
app.MapRazorPages();

			// ===== SignalR Hub 端點（指定傳輸種類，避免環境限制導致無法連線）=====
			app.MapHub<GameSpace.Areas.social_hub.Hubs.ChatHub>("/social_hub/chatHub", opts =>
			{
				opts.Transports =
					HttpTransportType.WebSockets |
					HttpTransportType.ServerSentEvents |
					HttpTransportType.LongPolling;
			});

			await app.RunAsync();
		}
	}
}
