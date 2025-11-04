using GameSpace.Areas.MiniGame.Models;
using GameSpace.Infrastructure.Time;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物每日屬性衰減背景服務
    /// 每日 UTC 00:00 自動執行，對所有寵物的飢餓、心情、體力、清潔、健康值進行衰減
    /// </summary>
    public class PetDailyDecayBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PetDailyDecayBackgroundService> _logger;
        private readonly IAppClock _appClock;

        public PetDailyDecayBackgroundService(
            IServiceProvider serviceProvider,
            IAppClock appClock,
            ILogger<PetDailyDecayBackgroundService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _appClock = appClock ?? throw new ArgumentNullException(nameof(appClock));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("寵物每日屬性衰減背景服務已啟動");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 計算距離下一個 UTC 00:00 的時間
                    var now = _appClock.UtcNow;
                    var nextMidnight = now.Date.AddDays(1); // 下一個 UTC 午夜
                    var delay = nextMidnight - now;

                    _logger.LogInformation("下一次寵物屬性衰減將在 {NextRun} UTC 執行（{Delay} 後）",
                        nextMidnight, delay);

                    // 等待直到下一個午夜
                    await Task.Delay(delay, stoppingToken);

                    // 執行每日衰減
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await PerformDailyDecayAsync();
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("寵物每日屬性衰減背景服務正在停止");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "寵物每日屬性衰減背景服務發生錯誤");
                    // 發生錯誤後等待 1 小時再重試
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        /// <summary>
        /// 執行每日寵物屬性衰減
        /// </summary>
        private async Task PerformDailyDecayAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<GameSpacedatabaseContext>();
            var settingsService = scope.ServiceProvider.GetRequiredService<ISystemSettingsService>();

            try
            {
                _logger.LogInformation("開始執行寵物每日屬性衰減，時間：{Time} UTC", _appClock.UtcNow);

                // ✅ 從 SystemSettings 讀取衰減值（預設值與資料庫一致）
                var hungerDecay = await settingsService.GetSettingIntAsync("Pet.DailyDecay.HungerDecay", 20);
                var moodDecay = await settingsService.GetSettingIntAsync("Pet.DailyDecay.MoodDecay", 30);
                var staminaDecay = await settingsService.GetSettingIntAsync("Pet.DailyDecay.StaminaDecay", 10);
                var cleanlinessDecay = await settingsService.GetSettingIntAsync("Pet.DailyDecay.CleanlinessDecay", 20);
                var healthDecay = await settingsService.GetSettingIntAsync("Pet.DailyDecay.HealthDecay", 0);

                _logger.LogInformation(
                    "衰減配置：飢餓 -{Hunger}、心情 -{Mood}、體力 -{Stamina}、清潔 -{Cleanliness}、健康 -{Health}",
                    hungerDecay, moodDecay, staminaDecay, cleanlinessDecay, healthDecay);

                // 查找所有需要衰減的寵物（未刪除且 IsActive）
                var pets = await context.Pets
                    .Where(p => !p.IsDeleted)
                    .ToListAsync();

                if (pets.Count == 0)
                {
                    _logger.LogInformation("目前沒有需要衰減的寵物");
                    return;
                }

                _logger.LogInformation("找到 {Count} 隻寵物需要進行屬性衰減", pets.Count);

                int affectedCount = 0;
                foreach (var pet in pets)
                {
                    // 應用衰減（不低於 0）
                    pet.Hunger = Math.Max(0, pet.Hunger - hungerDecay);
                    pet.Mood = Math.Max(0, pet.Mood - moodDecay);
                    pet.Stamina = Math.Max(0, pet.Stamina - staminaDecay);
                    pet.Cleanliness = Math.Max(0, pet.Cleanliness - cleanlinessDecay);
                    pet.Health = Math.Max(0, pet.Health - healthDecay);

                    affectedCount++;
                }

                // 儲存變更
                await context.SaveChangesAsync();

                _logger.LogInformation(
                    "寵物每日屬性衰減完成，共影響 {Count} 隻寵物，時間：{Time} UTC",
                    affectedCount, _appClock.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "執行寵物每日屬性衰減時發生錯誤");
                throw;
            }
        }

    }
}
