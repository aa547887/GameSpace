using GamiPort.Infrastructure.Time;
using GamiPort.Models;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Infrastructure.BackgroundServices
{
	/// <summary>
	/// 寵物每日衰減背景服務
	/// 每日 UTC+8 00:00 自動執行，應用寵物屬性衰減
	/// 商業規則：飢餓值 -20、心情值 -30、體力值 -10、清潔值 -20
	/// </summary>
	public class PetDailyDecayService : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<PetDailyDecayService> _logger;

		public PetDailyDecayService(
			IServiceScopeFactory scopeFactory,
			ILogger<PetDailyDecayService> logger)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("寵物每日衰減服務已啟動");

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					// 計算下次執行時間（明天 00:00 UTC+8）
					var now = DateTime.UtcNow;
					var taipeiNow = TimeZoneInfo.ConvertTimeFromUtc(now, TimeZones.Taipei);
					var tomorrow = taipeiNow.Date.AddDays(1);
					var tomorrowUtc = TimeZoneInfo.ConvertTimeToUtc(tomorrow, TimeZones.Taipei);
					var delay = tomorrowUtc - now;

					_logger.LogInformation(
						"寵物每日衰減服務將在 {NextRun} (UTC+8) 執行，距離現在 {Delay}",
						tomorrow, delay);

					// 等待到明天 00:00
					await Task.Delay(delay, stoppingToken);

					// 執行每日衰減
					await ApplyDailyDecayAsync();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "寵物每日衰減服務發生錯誤");
					// 發生錯誤時等待 1 小時後重試
					await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
				}
			}

			_logger.LogInformation("寵物每日衰減服務已停止");
		}

		/// <summary>
		/// 執行每日衰減邏輯
		/// </summary>
		private async Task ApplyDailyDecayAsync()
		{
			using var scope = _scopeFactory.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<GameSpacedatabaseContext>();

			try
			{
				// 讀取衰減配置（從 SystemSettings）
				var hungerDecay = await GetSettingAsync(context, "Pet.DailyDecay.HungerDecay", 20);
				var moodDecay = await GetSettingAsync(context, "Pet.DailyDecay.MoodDecay", 30);
				var staminaDecay = await GetSettingAsync(context, "Pet.DailyDecay.StaminaDecay", 10);
				var cleanlinessDecay = await GetSettingAsync(context, "Pet.DailyDecay.CleanlinessDecay", 20);

				// 查詢所有未刪除的寵物
				var pets = await context.Pets
					.Where(p => !p.IsDeleted)
					.ToListAsync();

				// 應用衰減（使用鉗位確保不低於 0）
				foreach (var pet in pets)
				{
					pet.Hunger = Math.Max(0, pet.Hunger - hungerDecay);
					pet.Mood = Math.Max(0, pet.Mood - moodDecay);
					pet.Stamina = Math.Max(0, pet.Stamina - staminaDecay);
					pet.Cleanliness = Math.Max(0, pet.Cleanliness - cleanlinessDecay);
				}

				await context.SaveChangesAsync();

				_logger.LogInformation(
					"每日衰減完成：已更新 {Count} 隻寵物（飢餓-{H}、心情-{M}、體力-{S}、清潔-{C}）",
					pets.Count, hungerDecay, moodDecay, staminaDecay, cleanlinessDecay);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "執行每日衰減時發生錯誤");
			}
		}

		/// <summary>
		/// 從 SystemSettings 讀取設定值
		/// </summary>
		private async Task<int> GetSettingAsync(GameSpacedatabaseContext context, string key, int defaultValue)
		{
			var setting = await context.SystemSettings.AsNoTracking()
				.FirstOrDefaultAsync(s => s.SettingKey == key);
			return setting != null && int.TryParse(setting.SettingValue, out int value) ? value : defaultValue;
		}
	}
}
