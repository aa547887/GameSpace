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

			// �s�u�r��G�u�ΧA�� GameSpace / GameSpacedatabase
			var gameSpaceConn =
				builder.Configuration.GetConnectionString("GameSpace")
				?? builder.Configuration.GetConnectionString("GameSpacedatabase")
				?? throw new InvalidOperationException("Connection string 'GameSpace' not found.");

			// �� (A) �����U ApplicationDbContext�]Identity �� Store �|�Ψ�^
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(gameSpaceConn));

			// �Y�A���M���٦��~�� DbContext�]�Ҧp GameSpacedatabaseContext�^�A�b�o�̤@�_���U�G
			// builder.Services.AddDbContext<GameSpacedatabaseContext>(options =>
			//     options.UseSqlServer(gameSpaceConn));

			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// �� (B) �A���U Identity�A�ç� Store ���V ApplicationDbContext
			builder.Services
				.AddDefaultIdentity<IdentityUser>(options =>
				{
					// �A�{���]�w�G�ݭn�H�c���Ҥ~�i�n�J�F�Y�S���H�H�y�{�A�i���Ȯ�����
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

			// �� (C) �A�쥻�|���o��G�����b Authorization �e�I�s
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
