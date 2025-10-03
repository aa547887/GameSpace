using GameSpace.Areas.MiniGame.Models.Settings;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物背景變更設定服務實作
    /// </summary>
    public class PetBackgroundChangeSettingsService : IPetBackgroundChangeSettingsService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<PetBackgroundChangeSettingsService> _logger;

        public PetBackgroundChangeSettingsService(
            GameSpacedatabaseContext context,
            ILogger<PetBackgroundChangeSettingsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PetSettings?> GetSettingsAsync()
        {
            try
            {
                // 暫時返回預設設定
                return new PetSettings
                {
                    ColorChangePointCost = 2000,
                    BackgroundChangePointCost = 3000,
                    MaxLevel = 100,
                    ExpMultiplier = 1.5m,
                    IsActive = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取寵物背景變更設定時發生錯誤");
                return null;
            }
        }

        public async Task<bool> UpdateSettingsAsync(PetSettings settings)
        {
            try
            {
                // 暫時實作
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物背景變更設定時發生錯誤");
                return false;
            }
        }

        public async Task<int> GetBackgroundChangePointCostAsync()
        {
            try
            {
                var settings = await GetSettingsAsync();
                return settings?.BackgroundChangePointCost ?? 3000;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取背景變更點數成本時發生錯誤");
                return 3000;
            }
        }

        public async Task<bool> CanUserChangeBgackgroundAsync(int userId)
        {
            try
            {
                var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId);
                if (wallet == null) return false;

                var cost = await GetBackgroundChangePointCostAsync();
                return wallet.Points >= cost;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查用戶 {UserId} 是否可變更背景時發生錯誤", userId);
                return false;
            }
        }
    }
}

