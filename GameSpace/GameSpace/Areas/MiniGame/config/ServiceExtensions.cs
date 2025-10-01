using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GameSpace.Areas.MiniGame.Data;
using GameSpace.Areas.MiniGame.Services;

namespace GameSpace.Areas.MiniGame.config
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddMiniGameServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 註冊 DbContext
            var connectionString = configuration.GetConnectionString("GameSpace");
            services.AddDbContext<MiniGameDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 註冊 Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<IEVoucherService, EVoucherService>();
            services.AddScoped<IPetService, PetService>();
            services.AddScoped<IMiniGameService, MiniGameService>();
            services.AddScoped<ISignInService, SignInService>();
            services.AddScoped<IManagerService, ManagerService>();

            return services;
        }
    }
}
