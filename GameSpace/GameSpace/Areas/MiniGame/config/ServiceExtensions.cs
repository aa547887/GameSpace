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
            // 註冊 MiniGameDbContext - 使用 GameSpace 連線字串
            services.AddDbContext<MiniGameDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("GameSpace")));

            // 註冊核心管理服務
            services.AddScoped<IMiniGameAdminService, MiniGameAdminService>();
            services.AddScoped<IMiniGamePermissionService, MiniGamePermissionService>();
            services.AddScoped<IMiniGameAdminAuthService, MiniGameAdminAuthService>();
            services.AddScoped<IMiniGameAdminGate, MiniGameAdminGate>();

            // 註冊錢包服務
            services.AddScoped<IUserWalletService, UserWalletService>();

            // 註冊優惠券服務
            services.AddScoped<ICouponService, CouponService>();

            // 註冊電子禮券服務
            services.AddScoped<IEVoucherService, EVoucherService>();

            // 註冊簽到統計服務
            services.AddScoped<ISignInStatsService, SignInStatsService>();

            // 註冊小遊戲服務
            services.AddScoped<IMiniGameService, MiniGameService>();

            // 註冊寵物選項管理服務
            services.AddScoped<IPetColorOptionService, PetColorOptionService>();
            services.AddScoped<IPetBackgroundOptionService, PetBackgroundOptionService>();

            // 註冊寵物等級經驗值設定服務
            services.AddScoped<IPetLevelExperienceSettingService, PetLevelExperienceSettingService>(); 

            // 註冊寵物升級獎勵設定服務
            services.AddScoped<IPetLevelRewardSettingService, PetLevelRewardSettingService>();

            // 註冊寵物升級規則驗證服務
            services.AddScoped<IPetLevelUpRuleValidationService, PetLevelUpRuleValidationService>();

            // 註冊每日遊戲次數限制服務
            services.AddScoped<IDailyGameLimitService, DailyGameLimitService>();

            // 註冊寵物成本設定服務
            services.AddScoped<IPetSkinColorCostSettingService, PetSkinColorCostSettingService>();
            services.AddScoped<IPetBackgroundCostSettingService, PetBackgroundCostSettingService>();

            // 註冊寵物換色/背景設定服務
            services.AddScoped<IPetColorChangeSettingsService, PetColorChangeSettingsService>();

            return services;
        }
    }
}
