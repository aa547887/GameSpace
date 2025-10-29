// =======================
// Program.cs（純 Cookie 版；不使用 Identity，不會建立 AspNetUsers）
// 目的：使用自訂 Cookie 驗證；保留 MVC / RazorPages、Anti-forgery、路由與開發期 EF 偵錯。
// 並新增：SignalR Hub 映射、我方統一介面 IAppCurrentUser（集中「吃登入」）、ILoginIdentity 備援。
//
// 【本次關鍵說明】
// 1) 穢語遮蔽服務 IProfanityFilter 為 Singleton，但「不直接吃 DbContext（Scoped）」：
//    → 改在服務內使用 IServiceScopeFactory.CreateScope() 取得短命 DbContext（GameSpacedatabaseContext）。
//    → 因此本檔「不需要」註冊 AddDbContextFactory / AddPooledDbContextFactory。
// 2) 客訴採「單一前台 Hub」：映射 SupportHub 為 /hubs/support，後台頁也連線到這個端點；並正確啟用 CORS。
// 3) 其他註冊與中介軟體順序維持不變（UseCors 需在 Auth 前、Routing 後）。
// =======================

using GamiPort.Models;                     // GameSpacedatabaseContext（業務資料）
using GamiPort.Areas.social_hub.Services.Abstractions;
using GamiPort.Areas.social_hub.Services.Application;
using Microsoft.AspNetCore.Identity;       // 只用 IPasswordHasher<User> / PasswordHasher<User>（升級舊明文）
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using GamiPort.Areas.Login.Services;       // IEmailSender / SmtpEmailSender（若未設定可換 NullEmailSender）

// === 新增/確認的 using（本檔有用到的服務/端點） ===
using GamiPort.Infrastructure.Security;    // ★ 我方統一介面 IAppCurrentUser / AppCurrentUser
using GamiPort.Infrastructure.Login;       // ★ 備援解析 ILoginIdentity / ClaimFirstLoginIdentity
using GamiPort.Areas.social_hub.Hubs;      // ★ ChatHub（DM 用）／SupportHub（客訴用）

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

			// ------------------------------------------------------------
			// DbContext 註冊：GameSpacedatabaseContext（業務資料庫）
			// ------------------------------------------------------------
			builder.Services.AddDbContext<GameSpacedatabaseContext>(options =>
			{
				options.UseSqlServer(gameSpaceConn);
			});

			// 1) CORS：允許後台的網域/連接埠跨站連線到本服務的 Hub（客訴採單一前台 Hub）
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("SupportCors", policy =>
				{
					policy
						.WithOrigins(
							"https://localhost:7042", // GameSpace 後台 HTTPS
							"http://localhost:5211"   // GameSpace 後台 HTTP
						)
						.AllowAnyHeader()
						.AllowAnyMethod()
						.AllowCredentials(); // Hub 連線需要
				});
			});

			// EF 開發者例外頁（顯示完整 EF 例外、/migrations 端點）
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// 驗證：Cookie
			builder.Services
				.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(opts =>
				{
					opts.LoginPath = "/Login/Login/Login";
					opts.LogoutPath = "/Login/Login/Logout";
					opts.AccessDeniedPath = "/Login/Login/Denied";

					// Cookie 屬性
					opts.Cookie.Name = "GamiPort.User";   // 與後台 Cookie 不同名，避免互蓋
					opts.Cookie.HttpOnly = true;
					opts.Cookie.SameSite = SameSiteMode.Lax; // Step1 Hub 允許匿名連線，先用 Lax 即可
					opts.ExpireTimeSpan = TimeSpan.FromDays(7);
					opts.SlidingExpiration = true;
				});

			// 授權（有 [Authorize] / Policy 時會用到；本案先走預設）
			builder.Services.AddAuthorization();

			// ------------------------------------------------------------
			// 專案服務（通知、好友關係等）
			// ------------------------------------------------------------
			builder.Services.AddMemoryCache();
			builder.Services.AddScoped<INotificationStore, NotificationStore>();
			builder.Services.AddScoped<IRelationService, RelationService>();

			// === Chat / 廣播服務（DM 與訊號通知） ===
			// IChatService：與資料庫互動（寫訊息、計算未讀…）→ 建議 Scoped
			builder.Services.AddScoped<IChatService, ChatService>();

			// === 客訴服務 ===
			builder.Services.AddScoped<ISupportService, SupportService>();
			builder.Services.AddSingleton<ISupportNotifier, SignalRSupportNotifier>();

			// IChatNotifier：透過 IHubContext<ChatHub> 對用戶/群組廣播 → 可 Singleton（IHubContext 執行緒安全）
			builder.Services.AddSingleton<IChatNotifier, SignalRChatNotifier>();

			// ===== 穢語遮蔽 =====
			// ProfanityFilter 為 Singleton，但不直接吃 DbContext（Scoped）：
			// → 服務內部以 IServiceScopeFactory.CreateScope() 取得「短命」DbContext，避免 DI 生命週期衝突。
			builder.Services.AddSingleton<IProfanityFilter, ProfanityFilter>();

			// ------------------------------------------------------------
			// MVC / RazorPages / JSON 命名策略 & Anti-forgery
			// ------------------------------------------------------------
			builder.Services.AddControllersWithViews()
				// JSON 保留原本的屬性大小寫（不轉 camelCase）
				.AddJsonOptions(opt => { opt.JsonSerializerOptions.PropertyNamingPolicy = null; });

			builder.Services.AddRazorPages();

			// AJAX 的防偽 Token（前端請以 header "RequestVerificationToken" 帶入）
			builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

			// ------------------------------------------------------------
			// 其他輔助服務（不走 Identity 但仍可用的雜湊、Email、取用 HttpContext）
			// ------------------------------------------------------------
			builder.Services.AddHttpContextAccessor();

			// 保留原本的服務（不要動）：它自己的 ICurrentUserService
			// 我們不覆蓋它，以免影響原先依賴；我們另走自己的 IAppCurrentUser。
			builder.Services.AddScoped<GamiPort.Services.ICurrentUserService, GamiPort.Services.CurrentUserService>();

			// 雜湊服務（升級舊明文密碼用）：IPasswordHasher<User>
			builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

			// 郵件發送（若尚未設定 SMTP，先改成 NullEmailSender 比較保險）
			builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

			// ------------------------------------------------------------
			// ★ 我方統一介面：IAppCurrentUser（集中讀取當前登入者資訊）
			//   讀取順序：AppUserId Claim -> NameIdentifier（純數字或 "U:<id>" 可解析）
			//            -> 備援呼叫 ILoginIdentity（必要時查一次 DB 對應）
			//   並使用 HttpContext.Items 做「同一請求快取」避免多次查詢。
			// ------------------------------------------------------------
			builder.Services.AddScoped<IAppCurrentUser, AppCurrentUser>();

			// 備援服務：ILoginIdentity -> ClaimFirstLoginIdentity
			// 說明：當 Claims 不完整（沒有 AppUserId、或 NameIdentifier 不能直接解析）時，
			//       AppCurrentUser 會呼叫它做一次較穩健的對應（必要時查 DB）。
			builder.Services.AddScoped<ILoginIdentity, ClaimFirstLoginIdentity>();

			// ------------------------------------------------------------
			// SignalR（聊天室必備）— 開啟詳細錯誤與穩定心跳
			// ------------------------------------------------------------
			builder.Services.AddSignalR(options =>
			{
				options.EnableDetailedErrors = true;                      // 讓前端拿到更清楚的錯誤
				options.KeepAliveInterval = TimeSpan.FromSeconds(15);     // 伺服器送 keep-alive 的頻率
				options.ClientTimeoutInterval = TimeSpan.FromSeconds(60); // 客端容忍逾時
			});
			

			// ========== ★ ECPay 服務註冊（唯一需要的兩行） ==========
			builder.Services.AddHttpContextAccessor();                         // BuildCreditRequest 會用到
			builder.Services.AddScoped<EcpayPaymentService>();                 // 我們的付款服務
			builder.Services.AddScoped<ILookupService, SqlLookupService>();                        // =====================================================
			builder.Services.AddScoped<ICartService, SqlCartService>();
			// ------------------------------------------------------------
			// 建立 App
			// ------------------------------------------------------------
			var app = builder.Build();

			// ------------------------------------------------------------
			// HTTP Pipeline
			// ------------------------------------------------------------
			if (app.Environment.IsDevelopment())
			{
				// 顯示 EF 相關的開發頁、/migrations 端點
				app.UseMigrationsEndPoint();

				// 啟動時快速檢查 DB 連線（提早發現連線字串或權限問題）
				using var scope = app.Services.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<GameSpacedatabaseContext>();
				_ = db.Database.CanConnect(); // 回傳 bool；此處只為提早觸發連線測試
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			// ------------------------------------------------------------
			// 啟動時先載入一次穢語規則（建議保留）
			// 讓 IProfanityFilter 有最新規則可用；之後前端每次進聊天會再帶 nocache=1 強制刷新。
			// ------------------------------------------------------------
			using (var scope = app.Services.CreateScope())
			{
				var filter = scope.ServiceProvider.GetRequiredService<IProfanityFilter>();
				filter.ReloadAsync().GetAwaiter().GetResult();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();

			// ✅ CORS 要在 Routing 後、Auth 前
			app.UseCors("SupportCors");

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

			// ★ DM 的 ChatHub（保留，與客訴無衝突）
			app.MapHub<ChatHub>("/social_hub/chathub");

			// 客訴 Hub（建議加 RequireCors 指定同一政策）
			app.MapHub<SupportHub>("/hubs/support").RequireCors("SupportCors");

			app.Run();
		}
	}
}
