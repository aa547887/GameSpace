// =======================
// Program.cs（純 Cookie 版；不使用 Identity，不會建立 AspNetUsers）
// 目的：使用自訂 Cookie 驗證；保留 MVC / RazorPages、Anti-forgery、路由與開發期 EF 偵錯。
// =======================

using GamiPort.Data;                       // ApplicationDbContext（目前未使用，但保留命名空間）
using GamiPort.Models;                     // GameSpacedatabaseContext（業務資料）
using GamiPort.Areas.social_hub.Services.Abstractions;
using GamiPort.Areas.social_hub.Services.Application;
using Microsoft.AspNetCore.Identity;       // 只為了 IPasswordHasher<User>/PasswordHasher<User>
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using GamiPort.Areas.Login.Services;

namespace GamiPort
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// ------------------------------------------------------------
			// 連線字串（兩個 DbContext 理論可共用；本版僅使用業務 DB）
			// 會依序找 "GameSpace" -> "GameSpacedatabase"；都沒有就丟錯
			// ------------------------------------------------------------
			var gameSpaceConn =
				builder.Configuration.GetConnectionString("GameSpace")
				?? builder.Configuration.GetConnectionString("GameSpacedatabase")
				?? throw new InvalidOperationException("Connection string 'GameSpace' not found.");

			// ------------------------------------------------------------
			// DbContext（只註冊業務 DB；不註冊 Identity 的 ApplicationDbContext）
			// ------------------------------------------------------------
			builder.Services.AddDbContext<GameSpacedatabaseContext>(options =>
			{
				options.UseSqlServer(gameSpaceConn);
				// options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking); // 讀多頁面可開
			});

			// EF 開發者例外頁（顯示完整 EF 例外、/migrations 端點）
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// ------------------------------------------------------------
			// 驗證：使用 Cookie（完全不走 Identity）
			// ------------------------------------------------------------
			builder.Services
				.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(opts =>
				{
					// 依你的登入頁面實際路由（Area=Login, Controller=Login, Action=Login）
					opts.LoginPath = "/Login/Login/Login";
					opts.LogoutPath = "/Login/Login/Logout";
					opts.AccessDeniedPath = "/Login/Login/Denied";
					opts.Cookie.Name = "GamiPort.User"; // 與後台 Cookie 保持不同名
					opts.Cookie.HttpOnly = true;
					opts.Cookie.SameSite = SameSiteMode.Lax;
					opts.ExpireTimeSpan = TimeSpan.FromDays(7);
					opts.SlidingExpiration = true;
				});

			builder.Services.AddAuthorization();

			// ------------------------------------------------------------
			// 專案服務（通知、好友關係等）
			// ------------------------------------------------------------
			builder.Services.AddMemoryCache();
			builder.Services.AddScoped<INotificationStore, NotificationStore>();
			builder.Services.AddScoped<IRelationService, RelationService>();

			// ------------------------------------------------------------
			// MVC / RazorPages / JSON 命名策略 & Anti-forgery
			// ------------------------------------------------------------
			builder.Services.AddControllersWithViews()
				.AddJsonOptions(opt => { opt.JsonSerializerOptions.PropertyNamingPolicy = null; });

			builder.Services.AddRazorPages();

			// AJAX 的防偽 Token（前端請以 header "RequestVerificationToken" 帶入）
			builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

			// ------------------------------------------------------------
			// 其他輔助服務（不走 Identity 但仍可用的雜湊、Email、取用 HttpContext）
			// ------------------------------------------------------------
			builder.Services.AddHttpContextAccessor();
			builder.Services.AddScoped<GamiPort.Services.ICurrentUserService, GamiPort.Services.CurrentUserService>();
			builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

			builder.Services.AddTransient<IEmailSender,
				// 若尚未設定 SMTP，暫時可改成 NullEmailSender
				SmtpEmailSender>();

			// ------------------------------------------------------------
			// 建立 App
			// ------------------------------------------------------------
			var app = builder.Build();

			// ------------------------------------------------------------
			// HTTP Pipeline
			// ------------------------------------------------------------
			if (app.Environment.IsDevelopment())
			{
				app.UseMigrationsEndPoint();

				// 啟動時快速檢查 DB 連線（提早發現連線字串或權限問題）
				using var scope = app.Services.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<GameSpacedatabaseContext>();
				db.Database.CanConnect();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();

			// 驗證一定在授權之前
			app.UseAuthentication();
			app.UseAuthorization();

			// MVC Controllers（含 Attribute Routing）
			app.MapControllers();

			// 傳統路由（先 Areas 再 default）
			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			// Razor Pages（若你有使用）
			app.MapRazorPages();

			app.Run();
		}
	}
}
