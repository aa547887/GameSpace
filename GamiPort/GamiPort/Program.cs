// =======================
// Program.cs（純 Cookie 版；不使用 Identity，不會建立 AspNetUsers）
// =======================

using GamiPort.Models;                     // GameSpacedatabaseContext（業務資料）
using GamiPort.Areas.social_hub.Services.Abstractions;
using GamiPort.Areas.social_hub.Services.Application;
using Microsoft.AspNetCore.Identity;       // IPasswordHasher<User>
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using GamiPort.Areas.Login.Services;       // IEmailSender

// === 新增的 using ===
using GamiPort.Infrastructure.Security;    // IAppCurrentUser
using GamiPort.Infrastructure.Login;       // ILoginIdentity
using GamiPort.Areas.social_hub.Hubs;      // ChatHub

// 購物車
using GamiPort.Areas.OnlineStore.Services;

// ★ 新增：ECPay 服務命名空間
using GamiPort.Areas.OnlineStore.Payments;

namespace GamiPort
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// 連線字串
			var gameSpaceConn =
				builder.Configuration.GetConnectionString("GameSpace")
				?? builder.Configuration.GetConnectionString("GameSpacedatabase")
				?? throw new InvalidOperationException("Connection string 'GameSpace' not found.");

			// DbContext（業務 DB）
			builder.Services.AddDbContext<GameSpacedatabaseContext>(options =>
			{
				options.UseSqlServer(gameSpaceConn);
			});

			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// 驗證：Cookie
			builder.Services
				.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(opts =>
				{
					opts.LoginPath = "/Login/Login/Login";
					opts.LogoutPath = "/Login/Login/Logout";
					opts.AccessDeniedPath = "/Login/Login/Denied";

					opts.Cookie.Name = "GamiPort.User";
					opts.Cookie.HttpOnly = true;
					opts.Cookie.SameSite = SameSiteMode.Lax;
					opts.ExpireTimeSpan = TimeSpan.FromDays(7);
					opts.SlidingExpiration = true;
				});

			builder.Services.AddAuthorization();

			// 專案服務（社群）
			builder.Services.AddMemoryCache();
			builder.Services.AddScoped<INotificationStore, NotificationStore>();
			builder.Services.AddScoped<IRelationService, RelationService>();
			builder.Services.AddScoped<IChatService, ChatService>();
			builder.Services.AddSingleton<IChatNotifier, SignalRChatNotifier>();

			builder.Services.AddControllersWithViews()
				.AddJsonOptions(opt => { opt.JsonSerializerOptions.PropertyNamingPolicy = null; });

			builder.Services.AddRazorPages();

			builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

			builder.Services.AddHttpContextAccessor();
			builder.Services.AddScoped<GamiPort.Services.ICurrentUserService, GamiPort.Services.CurrentUserService>();
			builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
			builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

			builder.Services.AddScoped<IAppCurrentUser, AppCurrentUser>();
			builder.Services.AddScoped<ILoginIdentity, ClaimFirstLoginIdentity>();

			builder.Services.AddSignalR();
			builder.Services.AddSignalR(options =>
			{
				options.EnableDetailedErrors = true;
				options.KeepAliveInterval = TimeSpan.FromSeconds(15);
				options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
			});

			// 購物車＋Session
			builder.Services.AddDistributedMemoryCache();
			builder.Services.AddScoped<ICartService, SqlCartService>();
			builder.Services.AddSession(options =>
			{
				options.Cookie.Name = ".GamiPort.Cart.Session";
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
				options.IdleTimeout = TimeSpan.FromHours(2);
			});
			builder.Services.AddScoped<ILookupService, SqlLookupService>();

			// ========== ★ ECPay 服務註冊（唯一需要的兩行） ==========
			builder.Services.AddHttpContextAccessor();                         // BuildCreditRequest 會用到
			builder.Services.AddScoped<EcpayPaymentService>();                 // 我們的付款服務
																			   // =====================================================

			// 建立 App
			var app = builder.Build();

			if (app.Environment.IsDevelopment())
			{
				app.UseMigrationsEndPoint();
				using var scope = app.Services.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<GameSpacedatabaseContext>();
				_ = db.Database.CanConnect();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();

			app.UseSession();     // 必須在 Auth 之前

			app.UseAuthentication();
			app.UseAuthorization();

			app.MapControllers();

			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.MapRazorPages();

			app.MapHub<ChatHub>("/social_hub/chathub");

			app.Run();
		}
	}
}
