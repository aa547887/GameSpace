// ---- 服務命名空間（一般 using）----
using GameSpace.Areas.social_hub.Services;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections; // for HttpTransportType
using Microsoft.AspNetCore.SignalR;           // for AddSignalR
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Threading.Tasks; // 因為你用了 async Task Main
using System.Linq;            // 在 CookieEvents 內有用到 .Any()

// ---- 「共用登入介面」(最小版) ----
using GameSpace.Infrastructure.Login; // ILoginIdentity, CookieAndAdminCookieLoginIdentity

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
			var gameSpaceConn =
				builder.Configuration.GetConnectionString("GameSpace")
				// ★ 可選：若你的 appsettings 有另一個 key，這行可當備援
				?? builder.Configuration.GetConnectionString("GameSpacedatabase")
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
				o.UsePerWordReplacement = true; // 每詞自訂替代（用 Mutes.replacement）
			});
			// 用別名註冊，避免撞名
			builder.Services.AddScoped<IMuteFilterAlias, MuteFilterAlias>();
			builder.Services.AddScoped<INotificationServiceAlias, NotificationServiceAlias>();
			builder.Services.AddScoped<IManagerPermissionService, ManagerPermissionServiceAlias>();


			// ===== CORS（可選：只有跨網域前端時才會啟用）=====
			var corsOrigins = builder.Configuration.GetSection("Cors:Chat:Origins").Get<string[]>();
			if (corsOrigins is { Length: > 0 })
			{
				builder.Services.AddCors(opt =>
				{
					opt.AddPolicy("chat", p => p
						.WithOrigins(corsOrigins)
						.AllowAnyHeader()
						.AllowAnyMethod()
						.AllowCredentials()
					);
				});
			}

			// ===== Session（登入流程/OTP 需要）=====
			builder.Services.AddSession(opt =>
			{
				opt.IdleTimeout = TimeSpan.FromMinutes(30);
				opt.Cookie.HttpOnly = true;
				opt.Cookie.IsEssential = true;
				opt.Cookie.SameSite = SameSiteMode.Lax;
			});

			// ===== 共用登入介面（關鍵）=====
			builder.Services.AddHttpContextAccessor(); // ★ 必須
			builder.Services.AddScoped<ILoginIdentity, CookieAndAdminCookieLoginIdentity>(); // ★ 必須

			// ★ 重要：獨立的後台 Cookie 方案，讓 CookieAndAdminCookieLoginIdentity 可以 AuthenticateAsync("AdminCookie")
			const string AdminCookieScheme = "AdminCookie";

			// ===== 後台 Cookie 驗證（給外部 LoginController 用）+「AJAX 401/403 不重導」=====
			builder.Services.AddAuthentication(options =>
			{
				// 不動 Identity 的 DefaultScheme；這裡只是額外加一個後台 Cookie 方案
			})
			.AddCookie(AdminCookieScheme, opt =>
			{
				opt.LoginPath = "/Login";
				opt.LogoutPath = "/Login/Logout";
				opt.AccessDeniedPath = "/Login/Denied";
				opt.Cookie.Name = "GameSpace.Admin";
				opt.SlidingExpiration = true;
				opt.ExpireTimeSpan = TimeSpan.FromHours(4);

				// 讓 AJAX/API 未登入/無權限回 401/403，而不是 Redirect
				opt.Events = new CookieAuthenticationEvents
				{
					OnRedirectToLogin = ctx =>
					{
						var isAjax =
							string.Equals(ctx.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
							|| ctx.Request.Headers["Accept"].Any(v => v?.Contains("json", StringComparison.OrdinalIgnoreCase) == true)
							|| ctx.Request.Path.StartsWithSegments("/Login/Me", StringComparison.OrdinalIgnoreCase);

						if (isAjax)
						{
							ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
							return Task.CompletedTask;
						}
						ctx.Response.Redirect(ctx.RedirectUri);
						return Task.CompletedTask;
					},
					OnRedirectToAccessDenied = ctx =>
					{
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

			// ===== 授權政策（對應我們寫入的 perm:* Claims）=====
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

			// 若有設定 CORS Origins，才套用該 Policy（需在 MapHub 前）
			if (corsOrigins is { Length: > 0 })
				app.UseCors("chat");

			// Cookie SameSite 與 Secure（跨網域傳 Cookie 時需要 None+Secure）
			app.UseCookiePolicy(new CookiePolicyOptions
			{
				MinimumSameSitePolicy = app.Environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
				Secure = app.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always
			});

			// **Session 必須在 Authentication 前啟用**
			app.UseSession();

			app.UseAuthentication(); // Identity + AdminCookie 皆會生效
			app.UseAuthorization();

			// ===== 路由 =====
			// 先加 area 的路由（重要）
			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			// 若使用 Identity UI
			app.MapRazorPages();

			// ===== SignalR Hub 端點（指定傳輸種類，避免環境限制導致無法連線）=====
			//app.MapHub<GameSpace.Areas.social_hub.Hubs.ChatHub>("/social_hub/chatHub", opts =>
			//{
			//	opts.Transports =
			//		HttpTransportType.WebSockets |
			//		HttpTransportType.ServerSentEvents |
			//		HttpTransportType.LongPolling;
			//});

			await app.RunAsync();
		}
	}
}
