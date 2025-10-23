// =======================
// Program.cs（正式登入優先版）
// 目的：只吃 Identity 登入 Cookie 的 Claims 取得目前使用者 UserId；
//      不再啟用 ?asUser / gp_dev_uid 等開發回退。
//      內含：DbContext、Identity、SignalR、Areas/MVC/RazorPages、Anti-forgery、路由。
// =======================

using GamiPort.Data;                           // ApplicationDbContext（Identity / 登入資料）
using GamiPort.Models;                         // GameSpacedatabaseContext（業務資料：聊天、通知、好友…）
using GamiPort.Areas.social_hub.Services.Abstractions;
using GamiPort.Areas.social_hub.Services.Application;
using GamiPort.Infrastructure.Login;           // ILoginIdentity / AppClaimsFactory / ClaimFirstLoginIdentity
using GamiPort.Areas.social_hub.Hubs;          // ChatHub（SignalR Hub）
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// ★ 新增：聊天室共用設定/常數（MessagePolicyOptions 等）
using GamiPort.Areas.social_hub.SignalR;

namespace GamiPort
{
	public class Program
	{
		public static void Main(string[] args)
		{
			// 1) 建立主機與 DI 容器
			var builder = WebApplication.CreateBuilder(args);

			// ------------------------------------------------------------
			// 2) 連線字串：兩個 DbContext（Identity 與業務 DB）共用同一組；
			//    若未來要分庫，可另外在 appsettings 加另一組連線字串並分流。
			// ------------------------------------------------------------
			var gameSpaceConn =
				builder.Configuration.GetConnectionString("GameSpace")
				?? builder.Configuration.GetConnectionString("GameSpacedatabase")
				?? throw new InvalidOperationException("Connection string 'GameSpace' not found.");

			// ------------------------------------------------------------
			// 3) MVC / RazorPages 服務
			//    - AddControllersWithViews：提供傳統 MVC + Areas + Views
			//    - AddRazorPages：提供 Identity 預設 UI（/Identity/...）或自訂 Razor Pages
			// ------------------------------------------------------------
			builder.Services
				.AddControllersWithViews()
				.AddJsonOptions(opt =>
				{
					// 統一 JSON 命名；不自動轉小駝峰，避免前端/後端屬性名不一致
					opt.JsonSerializerOptions.PropertyNamingPolicy = null;
				});
			builder.Services.AddRazorPages();

			// ------------------------------------------------------------
			// 4) DbContext 註冊
			//    - ApplicationDbContext：Identity（登入/使用者/Claims）
			//    - GameSpacedatabaseContext：你的業務資料（聊天、通知、好友等表）
			// ------------------------------------------------------------
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseSqlServer(gameSpaceConn);
				// options.EnableSensitiveDataLogging(); // 可選：開發期追蹤 SQL 參數
			});

			builder.Services.AddDbContext<GameSpacedatabaseContext>(options =>
			{
				options.UseSqlServer(gameSpaceConn);
				// options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking); // 可選：查詢量大頁面使用
			});

			// EF 開發者例外頁（顯示完整 EF 例外、提供 migrations 端點）
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// ------------------------------------------------------------
			// 5) Identity 設定
			//    - AddDefaultIdentity 會設定 Cookie、SecurityStamp 驗證等預設
			//    - 不強制信箱驗證（你有自己的登入頁）
			// ------------------------------------------------------------
			builder.Services
				.AddDefaultIdentity<IdentityUser>(options =>
				{
					options.SignIn.RequireConfirmedAccount = false;
				})
				.AddEntityFrameworkStores<ApplicationDbContext>();

			// 自訂應用程式 Cookie 行為：登入/拒絕路徑、Cookie 名稱等
			builder.Services.ConfigureApplicationCookie(opt =>
			{
				opt.LoginPath = "/Login/Login";
				opt.AccessDeniedPath = "/Login/Login/Denied";
				opt.Cookie.Name = "GamiPort.User";     // 與其他後台/子系統 Cookie 區隔
													   // opt.SlidingExpiration = true;       // 可選：滑動過期
			});

			// [Authorize] 授權服務（若聊天/關係 API 要求登入）
			builder.Services.AddAuthorization();

			// ------------------------------------------------------------
			// 6) 專案服務（通知、好友關係等）
			// ------------------------------------------------------------
			builder.Services.AddMemoryCache();
			builder.Services.AddScoped<INotificationStore, NotificationStore>();
			builder.Services.AddScoped<IRelationService, RelationService>();

			// ------------------------------------------------------------
			// 6.x) ★ 聊天室核心 DI（重構後需要）
			// ------------------------------------------------------------
			// 訊息策略（例如文字最大長度），可由 appsettings 覆寫；不設則採預設值（MaxContentLength=255）
			builder.Services.Configure<MessagePolicyOptions>(builder.Configuration.GetSection("Chat:MessagePolicy"));
			// DM 業務邏輯集中於 Service（Hub/Controller 一律委派）
			builder.Services.AddScoped<IChatService, ChatService>();
			// 封裝廣播（目前用 SignalR；未來可替換為其他推送機制）
			builder.Services.AddSingleton<IChatNotifier, SignalRChatNotifier>();

			// ------------------------------------------------------------
			// 7) ★ 核心：目前使用者識別（只吃登入 Cookie）
			//    - AppClaimsFactory：建立/刷新 ClaimsPrincipal 時，自動把 Users.UserId 補到 Claim("AppUserId")
			//    - ILoginIdentity：使用 ClaimFirstLoginIdentity（優先讀 "AppUserId"；備援解析 "U:<id>" 或 UserName 對應）
			// ------------------------------------------------------------
			builder.Services.AddHttpContextAccessor();
			builder.Services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, AppClaimsFactory>();
			builder.Services.AddScoped<ILoginIdentity, ClaimFirstLoginIdentity>();

			// ------------------------------------------------------------
			// 8) SignalR（聊天即時通訊）
			// ------------------------------------------------------------
			builder.Services.AddSignalR();

			// ------------------------------------------------------------
			// 9) Anti-forgery：若 Controller 使用 [ValidateAntiForgeryToken]，
			//    前端可在 fetch header 夾帶 RequestVerificationToken。
			// ------------------------------------------------------------
			builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

			// 10) 建立應用
			var app = builder.Build();

			// ------------------------------------------------------------
			// 11) HTTP Pipeline（中介軟體順序很重要）
			// ------------------------------------------------------------
			if (app.Environment.IsDevelopment())
			{
				// EF 錯誤詳情頁 / migrations 端點
				app.UseMigrationsEndPoint();

				// 啟動時快速檢查 DB 連線（提早發現連線字串或權限問題）
				using var scope = app.Services.CreateScope();
				var db1 = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
				var db2 = scope.ServiceProvider.GetRequiredService<GameSpacedatabaseContext>();
				db1.Database.CanConnect();
				db2.Database.CanConnect();

				// ⚠️ 正式登入優先：不再掛 ?asUser/gp_dev_uid 的開發中介，避免混淆身分來源
				// app.UseMiddleware<GamiPort.Infrastructure.Login.DevQueryLoginMiddleware>();
			}
			else
			{
				// 正式環境：例外處理 + HSTS
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			// 靜態檔、路由與安全性中介
			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();

			// 身分驗證 / 授權（順序一定是先驗證再授權）
			app.UseAuthentication();
			app.UseAuthorization();

			// MVC 控制器（含 Attribute Routing）
			app.MapControllers();

			// 傳統路由（先 Areas 再 default）
			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
			);

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}"
			);

			// Razor Pages（Identity 預設 UI 需要）
			app.MapRazorPages();

			// SignalR Hub 路由：前端（_FloatingDock 等）連至此位址
			app.MapHub<ChatHub>("/social_hub/chatHub");

			// 12) 啟動應用
			app.Run();
		}
	}
}
