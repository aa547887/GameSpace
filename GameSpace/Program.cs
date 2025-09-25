// ---- 服務命名空間（一般 using）----
using GameSpace.Areas.social_hub.Services;
using GameSpace.Data;
using GameSpace.Models;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections; // for HttpTransportType
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;           // for AddSignalR
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;            // CookieEvents 內有用到 .Any()
using System.Threading.Tasks; // async Main

// ---- 社群 Hub / 過濾器 / 共用登入 ----
using GameSpace.Areas.social_hub.Hubs;
using GameSpace.Infrastructure.Login;

// ---- MiniGame Area 服務 ----
using GameSpace.Areas.MiniGame.config;

// ---- 型別別名（避免撞名）----
using IMuteFilterAlias = GameSpace.Areas.social_hub.Services.IMuteFilter;
using INotificationServiceAlias = GameSpace.Areas.social_hub.Services.INotificationService;
using ManagerPermissionServiceAlias = GameSpace.Areas.social_hub.Services.ManagerPermissionService;
using MuteFilterAlias = GameSpace.Areas.social_hub.Services.MuteFilter;
using NotificationServiceAlias = GameSpace.Areas.social_hub.Services.NotificationService;

namespace GameSpace
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// ========== 1) 組態與連線字串（排序：先讀設定，再建 DbContext） ==========
			var identityConn = builder.Configuration.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
			var gameSpaceConn =
				builder.Configuration.GetConnectionString("GameSpace")
				?? builder.Configuration.GetConnectionString("GameSpacedatabase")
				?? throw new InvalidOperationException("Connection string 'GameSpace' not found.");

			// ========== 2) DbContexts（排序：先 Identity，再業務 DB） ==========
			builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer(identityConn));
			builder.Services.AddDbContext<GameSpacedatabaseContext>(opt => opt.UseSqlServer(gameSpaceConn));

			// 開發期資料庫錯誤頁
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// ========== 3) Identity（排序：先註冊 Identity，再註冊 MVC/Razor） ==========
			builder.Services
				.AddDefaultIdentity<IdentityUser>(opt => opt.SignIn.RequireConfirmedAccount = true)
				.AddEntityFrameworkStores<ApplicationDbContext>();

			// ========== 4) MVC / Razor ==========
			builder.Services.AddControllersWithViews();
			builder.Services.AddRazorPages();

			// ========== 5) SignalR（★必須：讓 IHubContext<ChatHub> 可注入） ==========
			builder.Services.AddSignalR();

			// ========== 6) social_hub 相關服務（MuteFilter/通知/權限） ==========
			builder.Services.AddMemoryCache();

			// 穢語過濾選項（MuteFilter 透過 IOptions 取得）
			builder.Services.Configure<GameSpace.Areas.social_hub.Services.MuteFilterOptions>(o =>
			{
				o.MaskStyle = GameSpace.Areas.social_hub.Services.MaskStyle.Asterisks;
				o.FixedLabel = "【封鎖】";
				o.FuzzyBetweenCjkChars = true;
				o.MaskExactLength = false;
				o.CacheTtlSeconds = 30;
				o.UsePerWordReplacement = true; // 每詞自訂替代（用 Mutes.replacement）
			});

			// 以別名註冊，避免撞名
			builder.Services.AddScoped<IMuteFilterAlias, MuteFilterAlias>();
			builder.Services.AddScoped<INotificationServiceAlias, NotificationServiceAlias>();
			builder.Services.AddScoped<IManagerPermissionService, ManagerPermissionServiceAlias>();

			// ========== 6.5) MiniGame Area 服務（★必須：讓 MiniGame 功能可注入） ==========
			builder.Services.AddMiniGameServices(builder.Configuration);

			// ========== 7) CORS（如需跨網域前端；排序：UseRouting 之後、MapHub 之前） ==========
			var corsOrigins = builder.Configuration.GetSection("Cors:Chat:Origins").Get<string[]>();
			if (corsOrigins is { Length: > 0 })
			{
				builder.Services.AddCors(opt =>
				{
					opt.AddPolicy("chat", p => p
						.WithOrigins(corsOrigins)
						.AllowAnyHeader()
						.AllowAnyMethod()
						.AllowCredentials());
				});
			}

			// ========== 8) Session（排序：在 Authentication 之前啟用） ==========
			builder.Services.AddSession(opt =>
			{
				opt.IdleTimeout = TimeSpan.FromMinutes(30);
				opt.Cookie.HttpOnly = true;
				opt.Cookie.IsEssential = true;
				opt.Cookie.SameSite = SameSiteMode.Lax;
			});

			// ========== 9) 共用登入介面（★最小登入，供 Controller/Hub 讀取目前身分） ==========
			builder.Services.AddHttpContextAccessor();
			builder.Services.AddScoped<ILoginIdentity, CookieAndAdminCookieLoginIdentity>();

			// ========== 10) 後台 Cookie 方案（AdminCookie） ==========
			const string AdminCookieScheme = "AdminCookie";

			// 後台 Cookie 驗證 + AJAX 401/403 不重導
			builder.Services.AddAuthentication(options => { /* 不更動 Identity 既有 DefaultScheme */ })
			.AddCookie(AdminCookieScheme, opt =>
			{
				opt.LoginPath = "/Login";
				opt.LogoutPath = "/Login/Logout";
				opt.AccessDeniedPath = "/Login/Denied";
				opt.Cookie.Name = "GameSpace.Admin";
				opt.SlidingExpiration = true;
				opt.ExpireTimeSpan = TimeSpan.FromHours(4);

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

			// ========== 11) 授權政策（與 Claims 對應） ==========
			builder.Services.AddAuthorization(options =>
			{
				options.AddPolicy("CanManageShopping", p => p.RequireClaim("perm:Shopping", "true"));
				options.AddPolicy("CanAdmin", p => p.RequireClaim("perm:Admin", "true"));
				options.AddPolicy("CanMessage", p => p.RequireClaim("perm:Message", "true"));
				options.AddPolicy("CanUserStatus", p => p.RequireClaim("perm:UserStat", "true"));
				options.AddPolicy("CanPet", p => p.RequireClaim("perm:Pet", "true"));
				options.AddPolicy("CanCS", p => p.RequireClaim("perm:CS", "true"));
				options.AddPolicy("AdminOnly", p => p.RequireClaim("perm:Admin", "true")); // MiniGame 管理員專用
			});

            //// 商品串API()ImgBB 新增：註冊 IHttpClientFactory（解決 ImgBB 上傳的 HttpClient 依賴注入問題）
            //builder.Services.AddHttpClient();
            //// 綁定設定檔到 ImgBbOptions（簡版）
            //// ✅（建議版）含啟動時驗證 ApiKey 是否存在
            //builder.Services.AddOptions<ImgBbOptions>()
            //    .Bind(builder.Configuration.GetSection("ImgBB"))
            //    .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "ImgBB:ApiKey 未設定")
            //    .ValidateOnStart();
            //builder.Services.Configure<ImgBbOptions>(builder.Configuration.GetSection("ImgBB"));
            ////商城圖片API (使用ImgBB) (可能會用到 如果伺服器上傳限制：如需調高 Kestrel/ IIS 限制)
            ////builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 104857600); // 100 MB

            var app = builder.Build();

			// ========== 12) 啟動預熱（可選；失敗不擋站） ==========
			using (var scope = app.Services.CreateScope())
			{
				try
				{
					var filter = scope.ServiceProvider.GetRequiredService<IMuteFilterAlias>();
					await filter.RefreshAsync(); // 預熱 Regex 與快取
				}
				catch { /* ignore */ }
			}

			// ========== 13) Middleware 管線（排序很重要） ==========
			if (app.Environment.IsDevelopment())
			{
				//app.UseMigrationsEndPoint(); 未來需調整回來
				app.UseDeveloperExceptionPage(); //未來需刪掉
			}
			else
			{
				app.UseExceptionHandler("/Home/Maintenance");
				
				 //app.UseHsts();   未來需要調整回來
			}
			app.UseStatusCodePagesWithReExecute("/Home/Http{0}");  // 4xx/5xx 統一轉送(未來必須需刪掉)
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting(); // ★ 路由要先啟用，後續 CORS/認證/授權/Hub 才能正確掛上

			// CORS（若有設定）——★排序：MapHub 之前
			if (corsOrigins is { Length: > 0 })
				app.UseCors("chat");

			// Cookie SameSite 與 Secure（跨網域傳 Cookie 時需要 None+Secure；本地開發可 Lax）
			app.UseCookiePolicy(new CookiePolicyOptions
			{
				MinimumSameSitePolicy = app.Environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
				Secure = app.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always
			});

			// ★ Session 一定要在 Authentication 之前
			app.UseSession();

			app.UseAuthentication(); // Identity + AdminCookie
			app.UseAuthorization();

			// ========== 14) 路由（先 Area，再預設 MVC，再 RazorPages） ==========
			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

			// ========== 15) SignalR Hub 端點（★必要：否則前端連不到、也無法注入 IHubContext） ==========
			app.MapHub<GameSpace.Areas.social_hub.Hubs.ChatHub>("/social_hub/chatHub", opts =>
			{
				// 支援多種傳輸，避免環境限制（如無 WS）
				opts.Transports =
					HttpTransportType.WebSockets |
					HttpTransportType.ServerSentEvents |
					HttpTransportType.LongPolling;
			});

			// ========== 16) 啟動 ==========
			await app.RunAsync();
		}
	}
}