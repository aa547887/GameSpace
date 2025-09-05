// ---- 服務命名空間（一般 using）----
using GameSpace.Areas.social_hub.Services;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
// ---- 型別別名（避免方案裡若有重複介面/命名空間不一致，導致 DI 對不到）----
using IMuteFilterAlias = GameSpace.Areas.social_hub.Services.IMuteFilter;
using INotificationServiceAlias = GameSpace.Areas.social_hub.Services.INotificationService;
using MuteFilterAlias = GameSpace.Areas.social_hub.Services.MuteFilter;
using NotificationServiceAlias = GameSpace.Areas.social_hub.Services.NotificationService;

namespace GameSpace
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(connectionString));

			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// GameSpace 主資料庫（EF DbContext）
			var gameSpaceConnectionString = builder.Configuration.GetConnectionString("GameSpace");
			builder.Services.AddDbContext<GameSpacedatabaseContext>(options =>
				options.UseSqlServer(gameSpaceConnectionString));

			// ASP.NET Core Identity
			builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
				.AddEntityFrameworkStores<ApplicationDbContext>();

			// MVC
			builder.Services.AddControllersWithViews();

			// ===== social_hub 相關服務註冊 =====
			builder.Services.AddMemoryCache();
			builder.Services.AddScoped<GameSpace.Areas.social_hub.Services.IMuteFilter,
							GameSpace.Areas.social_hub.Services.MuteFilter>();
			builder.Services.AddScoped<GameSpace.Areas.social_hub.Services.INotificationService,
										GameSpace.Areas.social_hub.Services.NotificationService>();

			// SignalR
			builder.Services.AddSignalR();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
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

			app.UseAuthentication(); // Identity
			app.UseAuthorization();

			// 先加 area 的路由（重要）
			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

			// 再加一般的 default 路由
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.MapRazorPages();

			// 註冊 SignalR hub（使用完整型別名稱，避免命名空間衝突）
			app.MapHub<GameSpace.Areas.social_hub.Hubs.ChatHub>("/social_hub/chatHub");

			app.Run();
		}
	}
}
