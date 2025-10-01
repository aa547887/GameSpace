using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Data;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.config
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddMiniGameServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 註冊 MiniGameDbContext - 使用 GameSpace 連線字串
            services.AddDbContext<MiniGameDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("GameSpace")));

            // 註冊 Services
            services.AddScoped<IMiniGameAdminService, MiniGameAdminService>();
            services.AddScoped<IMiniGamePermissionService, MiniGamePermissionService>();
            services.AddScoped<IMiniGameAdminAuthService, MiniGameAdminAuthService>();
            services.AddScoped<IMiniGameAdminGate, MiniGameAdminGate>();

            return services;
        }
    }
}
