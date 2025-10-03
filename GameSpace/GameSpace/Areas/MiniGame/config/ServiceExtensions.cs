﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Services;

namespace GameSpace.Areas.MiniGame.config
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddMiniGameServices(this IServiceCollection services, IConfiguration configuration)
        {
            // MiniGame Area 使用共享的 GameSpacedatabaseContext (已在 Program.cs 註冊)
            // 不需要在此註冊 DbContext

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

            // 註冊每日遊戲次數限制驗證服務
            services.AddScoped<IDailyGameLimitValidationService, DailyGameLimitValidationService>();

            // 註冊寵物成本設定服務
            services.AddScoped<IPetSkinColorCostSettingService, PetSkinColorCostSettingService>();
            services.AddScoped<IPetBackgroundCostSettingService, PetBackgroundCostSettingService>();

            // 註冊寵物換色/背景設定服務
            services.AddScoped<IPetColorChangeSettingsService, PetColorChangeSettingsService>();

            // ==================== Phase 1: 新增 11 個核心 Service ====================
            // 註冊寵物管理服務
            services.AddScoped<IPetService, PetService>();

            // 註冊錢包管理服務
            services.AddScoped<IWalletService, WalletService>();

            // 註冊簽到管理服務
            services.AddScoped<ISignInService, SignInService>();

            // 註冊系統診斷服務
            services.AddScoped<IDiagnosticsService, DiagnosticsService>();

            // 註冊儀表板服務
            services.AddScoped<IDashboardService, DashboardService>();

            // 註冊使用者管理服務
            services.AddScoped<IUserService, UserService>();

            // 註冊管理員管理服務
            services.AddScoped<IManagerService, ManagerService>();

            // 註冊優惠券類型管理服務
            services.AddScoped<ICouponTypeService, CouponTypeService>();

            // 註冊電子禮券類型管理服務
            services.AddScoped<IEVoucherTypeService, EVoucherTypeService>();

            // 註冊寵物規則管理服務
            services.AddScoped<IPetRulesService, PetRulesService>();

            // 註冊遊戲規則管理服務
            services.AddScoped<IGameRulesService, GameRulesService>();
            // =========================================================================

            // ==================== Phase 2: 新增額外服務 ====================
            // 註冊寵物等級提升規則服務
            services.AddScoped<IPetLevelUpRuleService, PetLevelUpRuleService>();

            // 註冊寵物背景變更設定服務
            services.AddScoped<IPetBackgroundChangeSettingsService, PetBackgroundChangeSettingsService>();

            // 註冊點數設定統計服務
            services.AddScoped<IPointsSettingsStatisticsService, PointsSettingsStatisticsService>();
            // ===============================================================

            return services;
        }
    }
}


