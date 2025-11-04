using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Models;
using GameSpace.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 遊戲規則完整設定服務實作
    /// </summary>
    public class GameRulesConfigService : IGameRulesConfigService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ISystemSettingsService _settingsService;
        private readonly ISystemSettingsMutationService _mutationService;
        private readonly IAppClock _appClock;
        private readonly ILogger<GameRulesConfigService> _logger;

        public GameRulesConfigService(
            GameSpacedatabaseContext context,
            ISystemSettingsService settingsService,
            ISystemSettingsMutationService mutationService,
            IAppClock appClock,
            ILogger<GameRulesConfigService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _mutationService = mutationService ?? throw new ArgumentNullException(nameof(mutationService));
            _appClock = appClock ?? throw new ArgumentNullException(nameof(appClock));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 取得完整遊戲規則設定（包含關卡、冒險結果影響、健康檢查）
        /// </summary>
        public async Task<GameRulesConfigViewModel> GetCompleteGameRulesAsync()
        {
            try
            {
                // 讀取基本設定
                var dailyLimit = await _settingsService.GetSettingIntAsync("Game.DefaultDailyLimit", 3);

                // 讀取3個關卡設定
                var levelConfigs = new List<LevelConfigViewModel>();
                for (int level = 1; level <= 3; level++)
                {
                    var monsterCount = await _settingsService.GetSettingIntAsync($"Game.Level{level}.MonsterCount", level == 1 ? 6 : level == 2 ? 8 : 10);
                    var speedMultiplier = await _settingsService.GetSettingDecimalAsync($"Game.Level{level}.SpeedMultiplier", level == 1 ? 1.0m : level == 2 ? 1.5m : 2.0m);
                    var expReward = await _settingsService.GetSettingIntAsync($"Game.Level{level}.ExperienceReward", level * 100);
                    var pointsReward = await _settingsService.GetSettingIntAsync($"Game.Level{level}.PointsReward", level * 10);
                    var hasCoupon = level == 3 ? await _settingsService.GetSettingBoolAsync("Game.Level3.HasCoupon", true) : false;
                    var couponType = level == 3 ? await _settingsService.GetSettingStringAsync("Game.Level3.CouponType", "GAME_LEVEL3_BONUS") : null;

                    levelConfigs.Add(new LevelConfigViewModel
                    {
                        Level = level,
                        MonsterCount = monsterCount,
                        SpeedMultiplier = speedMultiplier,
                        ExperienceReward = expReward,
                        PointsReward = pointsReward,
                        HasCoupon = hasCoupon,
                        CouponType = couponType,
                        Description = $"第{level}關：擊敗 {monsterCount} 個怪物（速度 {speedMultiplier}x）"
                    });
                }

                // 讀取冒險勝利影響
                var winHunger = await _settingsService.GetSettingIntAsync("Game.Result.Win.HungerDelta", -20);
                var winMood = await _settingsService.GetSettingIntAsync("Game.Result.Win.MoodDelta", 30);
                var winStamina = await _settingsService.GetSettingIntAsync("Game.Result.Win.StaminaDelta", -20);
                var winCleanliness = await _settingsService.GetSettingIntAsync("Game.Result.Win.CleanlinessDelta", -20);

                // 讀取冒險失敗影響
                var loseHunger = await _settingsService.GetSettingIntAsync("Game.Result.Lose.HungerDelta", -20);
                var loseMood = await _settingsService.GetSettingIntAsync("Game.Result.Lose.MoodDelta", -30);
                var loseStamina = await _settingsService.GetSettingIntAsync("Game.Result.Lose.StaminaDelta", -20);
                var loseCleanliness = await _settingsService.GetSettingIntAsync("Game.Result.Lose.CleanlinessDelta", -20);

                // 計算統計數據
                var totalGames = await _context.MiniGames.Where(g => !g.IsDeleted).CountAsync();
                var taipeiNow = _appClock.ToAppTime(_appClock.UtcNow);
                var startUtc = _appClock.ToUtc(taipeiNow.Date);
                var endUtc = _appClock.ToUtc(taipeiNow.Date.AddDays(1));
                var todayGames = await _context.MiniGames
                    .Where(g => g.StartTime >= startUtc && g.StartTime < endUtc && !g.IsDeleted)
                    .CountAsync();

                var viewModel = new GameRulesConfigViewModel
                {
                    GameName = "Adventure Game - 冒險遊戲",
                    Description = "經典冒險遊戲，通過擊敗怪物來獲得經驗值和點數獎勵。首次從第1關開始，勝利後進入下一關，失敗則留在當前關卡。",
                    DailyPlayLimit = dailyLimit,
                    IsActive = true,
                    TotalGamesPlayed = totalGames,
                    TodayGamesPlayed = todayGames,
                    LastUpdated = _appClock.UtcNow,
                    LevelConfigs = levelConfigs,
                    WinImpact = new AdventureResultImpact
                    {
                        HungerDelta = winHunger,
                        MoodDelta = winMood,
                        StaminaDelta = winStamina,
                        CleanlinessDelta = winCleanliness
                    },
                    LoseImpact = new AdventureResultImpact
                    {
                        HungerDelta = loseHunger,
                        MoodDelta = loseMood,
                        StaminaDelta = loseStamina,
                        CleanlinessDelta = loseCleanliness
                    },
                    HealthCheckThreshold = new HealthCheckThreshold
                    {
                        MinHunger = 1,
                        MinMood = 1,
                        MinStamina = 1,
                        MinCleanliness = 1,
                        MinHealth = 1,
                        Message = "寵物任一屬性值為 0 時無法開始冒險，請先透過互動恢復寵物狀態"
                    }
                };

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得遊戲規則設定失敗");
                throw;
            }
        }

        /// <summary>
        /// 更新關卡設定
        /// </summary>
        public async Task<(bool success, string message)> UpdateLevelConfigAsync(LevelConfigInputModel model, int? updatedBy = null)
        {
            if (model.Level < 1 || model.Level > 3)
            {
                return (false, "關卡等級必須在1-3之間");
            }

            try
            {
                var settings = new Dictionary<string, string>
                {
                    { $"Game.Level{model.Level}.MonsterCount", model.MonsterCount.ToString() },
                    { $"Game.Level{model.Level}.SpeedMultiplier", model.SpeedMultiplier.ToString("F1") },
                    { $"Game.Level{model.Level}.ExperienceReward", model.ExperienceReward.ToString() },
                    { $"Game.Level{model.Level}.PointsReward", model.PointsReward.ToString() }
                };

                // 只有第3關可以設定優惠券
                if (model.Level == 3)
                {
                    settings.Add("Game.Level3.HasCoupon", model.HasCoupon.ToString().ToLower());
                    if (model.HasCoupon && !string.IsNullOrWhiteSpace(model.CouponType))
                    {
                        settings.Add("Game.Level3.CouponType", model.CouponType);
                    }
                }

                var result = await _mutationService.BatchUpsertSettingsAsync(settings, updatedBy);

                if (result.success)
                {
                    _logger.LogInformation("關卡{Level}設定更新成功，更新了{Count}個設定", model.Level, result.updatedCount);
                    return (true, $"第{model.Level}關設定更新成功");
                }

                return (false, result.message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新關卡{Level}設定失敗", model.Level);
                return (false, $"更新失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新冒險結果影響設定（勝利/失敗）
        /// </summary>
        public async Task<(bool success, string message)> UpdateAdventureResultImpactAsync(AdventureResultImpactInputModel model, int? updatedBy = null)
        {
            if (model.ResultType != "Win" && model.ResultType != "Lose")
            {
                return (false, "結果類型必須是 Win 或 Lose");
            }

            try
            {
                var settings = new Dictionary<string, string>
                {
                    { $"Game.Result.{model.ResultType}.HungerDelta", model.HungerDelta.ToString() },
                    { $"Game.Result.{model.ResultType}.MoodDelta", model.MoodDelta.ToString() },
                    { $"Game.Result.{model.ResultType}.StaminaDelta", model.StaminaDelta.ToString() },
                    { $"Game.Result.{model.ResultType}.CleanlinessDelta", model.CleanlinessDelta.ToString() }
                };

                var result = await _mutationService.BatchUpsertSettingsAsync(settings, updatedBy);

                if (result.success)
                {
                    _logger.LogInformation("冒險{ResultType}影響設定更新成功", model.ResultType);
                    return (true, $"冒險{(model.ResultType == "Win" ? "勝利" : "失敗")}影響設定更新成功");
                }

                return (false, result.message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新冒險結果影響失敗");
                return (false, $"更新失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新每日遊戲次數限制
        /// </summary>
        public async Task<(bool success, string message)> UpdateDailyLimitAsync(int dailyLimit, int? updatedBy = null)
        {
            if (dailyLimit < 1 || dailyLimit > 100)
            {
                return (false, "每日次數必須在1-100之間");
            }

            try
            {
                var result = await _mutationService.UpsertSettingIntAsync(
                    "Game.DefaultDailyLimit",
                    dailyLimit,
                    "Default daily game limit",
                    updatedBy);

                if (result.success)
                {
                    _logger.LogInformation("每日遊戲次數限制更新為{DailyLimit}", dailyLimit);
                    return (true, $"每日次數限制已更新為 {dailyLimit} 次");
                }

                return (false, result.message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新每日次數限制失敗");
                return (false, $"更新失敗：{ex.Message}");
            }
        }
    }
}
