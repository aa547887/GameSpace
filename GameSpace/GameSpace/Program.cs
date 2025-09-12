// ---- 服務命名空間（一般 using）----
using GameSpace.Areas.social_hub.Services;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections; // for HttpTransportType
using Microsoft.AspNetCore.SignalR;           // for AddSignalR
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks; // 因為你用了 async Task Main

// ---- 型別別名（避免方案裡若有重複介面/命名空間不一致，導致 DI 對不到）----
using IMuteFilterAlias = GameSpace.Areas.social_hub.Services.IMuteFilter;
using INotificationServiceAlias = GameSpace.Areas.social_hub.Services.INotificationService;
using MuteFilterAlias = GameSpace.Areas.social_hub.Services.MuteFilter;
using NotificationServiceAlias = GameSpace.Areas.social_hub.Services.NotificationService;
// ✅ 權限服務的別名
using IManagerPermissionServiceAlias = GameSpace.Areas.social_hub.Services.IManagerPermissionService;
using ManagerPermissionServiceAlias = GameSpace.Areas.social_hub.Services.ManagerPermissionService;

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

			// ===== SignalR =====
			builder.Services.AddSignalR(options =>
			{
				options.EnableDetailedErrors = true;                      // 有助偵錯
				options.KeepAliveInterval = TimeSpan.FromSeconds(15);     // 心跳
				options.ClientTimeoutInterval = TimeSpan.FromSeconds(60); // 逾時
			})
			.AddJsonProtocol(cfg =>
			{
				// 保留 C# 的屬性大小寫（方便前端與後端對齊命名）
				cfg.PayloadSerializerOptions.PropertyNamingPolicy = null;
			});

			// ===== CORS（可選：只有跨網域前端時才會啟用）=====
			// 在 appsettings.json 內放：
			// "Cors": { "Chat": { "Origins": [ "https://your-frontend.example.com" ] } }
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
			
			app.UseAuthentication(); // Identity
			app.UseAuthorization();

			// ===== 路由 =====
			// 先加 area 的路由（重要）
			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

			// 再加一般的 default 路由
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

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
