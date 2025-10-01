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

            // 註冊 Services
            services.AddScoped<IMiniGameAdminService, MiniGameAdminService>();
            services.AddScoped<IMiniGamePermissionService, MiniGamePermissionService>();
            services.AddScoped<IMiniGameAdminAuthService, MiniGameAdminAuthService>();
            services.AddScoped<IMiniGameAdminGate, MiniGameAdminGate>();

            // 註冊點數設定相關服務
            services.AddScoped<IPetColorCostSettingService, PetColorCostSettingService>();
            services.AddScoped<IPetBackgroundCostSettingService, PetBackgroundCostSettingService>();
            services.AddScoped<IPetColorChangeSettingsService, PetColorChangeSettingsService>();
            services.AddScoped<IPetBackgroundChangeSettingsService, PetBackgroundChangeSettingsService>();
            services.AddScoped<IPointsSettingsService, PointsSettingsService>();

            // 註冊寵物選項相關服務
            services.AddScoped<IPetColorOptionService, PetColorOptionService>();
            services.AddScoped<IPetBackgroundOptionService, PetBackgroundOptionService>();

            // 註冊寵物等級經驗值設定服務
            services.AddScoped<IPetLevelExperienceSettingService, PetLevelExperienceSettingService>(); 

            // 註冊寵物升級獎勵設定服務
            services.AddScoped<IPetLevelRewardSettingService, PetLevelRewardSettingService>();

            // 註冊寵物升級規則服務
            services.AddScoped<IPetLevelUpRuleService, PetLevelUpRuleService>();

            // 註冊寵物升級規則驗證服務
            services.AddScoped<IPetLevelUpRuleValidationService, PetLevelUpRuleValidationService>();

            return services;
        }
    }
}
