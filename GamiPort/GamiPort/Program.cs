using GamiPort.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GamiPort
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// 連線字串：沿用你的 GameSpace / GameSpacedatabase
			var gameSpaceConn =
				builder.Configuration.GetConnectionString("GameSpace")
				?? builder.Configuration.GetConnectionString("GameSpacedatabase")
				?? throw new InvalidOperationException("Connection string 'GameSpace' not found.");

			// ★ (A) 先註冊 ApplicationDbContext（Identity 的 Store 會用到）
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(gameSpaceConn));

			// 若你的專案還有業務 DbContext（例如 GameSpacedatabaseContext），在這裡一起註冊：
			// builder.Services.AddDbContext<GameSpacedatabaseContext>(options =>
			//     options.UseSqlServer(gameSpaceConn));

			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// ★ (B) 再註冊 Identity，並把 Store 指向 ApplicationDbContext
			builder.Services
				.AddDefaultIdentity<IdentityUser>(options =>
				{
					// 你現有設定：需要信箱驗證才可登入；若沒有寄信流程，可先暫時關掉
					options.SignIn.RequireConfirmedAccount = true;
				})
				.AddEntityFrameworkStores<ApplicationDbContext>();

			builder.Services.AddControllersWithViews();
			builder.Services.AddRazorPages();

			var app = builder.Build();

			// HTTP pipeline
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

			// ★ (C) 你原本漏掉這行：必須在 Authorization 前呼叫
			app.UseAuthentication();
			app.UseAuthorization();

			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
			);
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}"
			);
			app.MapRazorPages();

			app.Run();
		}
	}
}
