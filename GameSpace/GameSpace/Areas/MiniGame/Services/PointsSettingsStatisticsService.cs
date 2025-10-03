using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 點數設定統計服務實作
    /// </summary>
    public class PointsSettingsStatisticsService : IPointsSettingsStatisticsService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<PointsSettingsStatisticsService> _logger;

        public PointsSettingsStatisticsService(
            GameSpacedatabaseContext context,
            ILogger<PointsSettingsStatisticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Dictionary<string, object>> GetStatisticsAsync()
        {
            try
            {
                var stats = new Dictionary<string, object>
                {
                    ["TotalUsers"] = await _context.UserWallets.CountAsync(),
                    ["TotalPoints"] = await _context.UserWallets.SumAsync(w => w.Points),
                    ["AveragePoints"] = await _context.UserWallets.AverageAsync(w => (double)w.Points),
                    ["MaxPoints"] = await _context.UserWallets.MaxAsync(w => w.Points),
                    ["MinPoints"] = await _context.UserWallets.MinAsync(w => w.Points)
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取點數統計時發生錯誤");
                return new Dictionary<string, object>();
            }
        }

        public async Task<Dictionary<string, int>> GetPointsDistributionAsync()
        {
            try
            {
                var distribution = new Dictionary<string, int>
                {
                    ["0-1000"] = await _context.UserWallets.CountAsync(w => w.Points >= 0 && w.Points < 1000),
                    ["1000-5000"] = await _context.UserWallets.CountAsync(w => w.Points >= 1000 && w.Points < 5000),
                    ["5000-10000"] = await _context.UserWallets.CountAsync(w => w.Points >= 5000 && w.Points < 10000),
                    ["10000+"] = await _context.UserWallets.CountAsync(w => w.Points >= 10000)
                };

                return distribution;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取點數分佈時發生錯誤");
                return new Dictionary<string, int>();
            }
        }

        public async Task<Dictionary<string, int>> GetPointsUsageTrendAsync(int days = 30)
        {
            try
            {
                var startDate = DateTime.Now.AddDays(-days);
                var trend = new Dictionary<string, int>();

                // 暫時返回空字典，實際需要查詢 WalletHistory 表
                return trend;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取點數使用趨勢時發生錯誤");
                return new Dictionary<string, int>();
            }
        }
    }
}

