using GamiPort.Areas.MiniGame.Helpers;
using GamiPort.Infrastructure.Time;
using GamiPort.Models;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.MiniGame.Services
{
	/// <summary>
	/// 小遊戲玩法服務實作（前台）
	/// 實作遊戲邏輯: 檢查剩餘次數、開始遊戲、結束遊戲並發放獎勵、查詢歷史
	/// </summary>
	public class GamePlayService : IGamePlayService
	{
		private readonly GameSpacedatabaseContext _context;
		private readonly IAppClock _appClock;
		private readonly ILogger<GamePlayService> _logger;

		public GamePlayService(
			GameSpacedatabaseContext context,
			IAppClock appClock,
			ILogger<GamePlayService> logger)
		{
			_context = context;
			_appClock = appClock;
			_logger = logger;
		}

		/// <summary>
		/// 獲取用戶今日剩餘遊戲次數
		/// 默認每日 3 次，可從 SystemSettings 配置讀取
		/// </summary>
		public async Task<int> GetUserRemainingPlaysAsync(int userId)
		{
			try
			{
				// 讀取今日限制設定（預設 3 次）
				int dailyLimit = await GetDailyGameLimitAsync();

				// 計算今日的 UTC 時間邊界
				var appNow = _appClock.ToAppTime(_appClock.UtcNow);
				var todayStart = appNow.Date; // 00:00:00 UTC+8
				var todayEnd = todayStart.AddDays(1).AddTicks(-1); // 23:59:59.9999999 UTC+8

				// 轉回 UTC 進行數據庫查詢
				var utcStart = _appClock.ToUtc(todayStart);
				var utcEnd = _appClock.ToUtc(todayEnd);

				// 統計今日已玩次數（非中止的遊戲）
				var todayPlayCount = await _context.MiniGames
					.AsNoTracking()
					.CountAsync(g =>
						g.UserId == userId &&
						!g.IsDeleted &&
						g.StartTime >= utcStart &&
						g.StartTime <= utcEnd &&
						!g.Aborted);

				int remaining = Math.Max(0, dailyLimit - todayPlayCount);
				return remaining;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "獲取今日剩餘遊戲次數時發生錯誤 (UserId: {UserId})", userId);
				return 0;
			}
		}

		/// <summary>
		/// 開始遊戲
		/// 檢查剩餘次數、建立遊戲記錄
		/// </summary>
		public async Task<(bool success, string message, int? playId)> StartGameAsync(int userId, int level)
		{
			try
			{
				// 驗證難度等級
				if (level < 1 || level > 3)
				{
					return (false, "無效的難度等級 (必須 1-3)", null);
				}

				// 檢查用戶是否存在
				var user = await _context.Users
					.AsNoTracking()
					.FirstOrDefaultAsync(u => u.UserId == userId && !u.UserLockoutEnabled);

				if (user == null)
				{
					return (false, "用戶不存在或已被鎖定", null);
				}

				// 檢查今日剩餘次數
				int remaining = await GetUserRemainingPlaysAsync(userId);
				if (remaining <= 0)
				{
					return (false, "今日遊戲次數已用完，請明天再來", null);
				}

				// 獲取用戶的寵物（取第一隻，或根據邏輯選擇）
				var pet = await _context.Pets
					.AsNoTracking()
					.FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

				if (pet == null)
				{
					return (false, "用戶無寵物資料", null);
				}

				// 建立遊戲記錄
				var game = new Models.MiniGame
				{
					UserId = userId,
					PetId = pet.PetId,
					Level = level,
					MonsterCount = GetMonsterCountByLevel(level),
					SpeedMultiplier = GetSpeedMultiplierByLevel(level),
					Result = "進行中",
					ExpGained = 0,
					ExpGainedTime = _appClock.UtcNow,
					PointsGained = 0,
					PointsGainedTime = _appClock.UtcNow,
					CouponGained = string.Empty,
					CouponGainedTime = _appClock.UtcNow,
					HungerDelta = 0,
					MoodDelta = 0,
					StaminaDelta = 0,
					CleanlinessDelta = 0,
					StartTime = _appClock.UtcNow,
					EndTime = null,
					Aborted = false,
					IsDeleted = false
				};

				_context.MiniGames.Add(game);
				await _context.SaveChangesAsync();

				return (true, "遊戲已啟動", game.PlayId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "開始遊戲時發生錯誤 (UserId: {UserId}, Level: {Level})", userId, level);
				return (false, "啟動遊戲失敗，請稍後重試", null);
			}
		}

		/// <summary>
		/// 結束遊戲並發放獎勵
		/// 規則: 只有 Win 才發放獎勵（點數、經驗值）
		/// </summary>
		public async Task<(bool success, string message)> EndGameAsync(
			int userId,
			int playId,
			int level,
			string result,
			int experience,
			int points)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				// 查找遊戲記錄
				var game = await _context.MiniGames.FirstOrDefaultAsync(g =>
					g.PlayId == playId &&
					g.UserId == userId &&
					!g.IsDeleted);

				if (game == null)
				{
					await transaction.RollbackAsync();
					return (false, "遊戲記錄不存在");
				}

				// 驗證結果值
				if (!new[] { "Win", "Lose", "Abort" }.Contains(result))
				{
					await transaction.RollbackAsync();
					return (false, "無效的遊戲結果");
				}

				// 更新遊戲記錄
				game.Result = result;
				game.EndTime = _appClock.UtcNow;
				game.Aborted = (result == "Abort");

				// 只有勝利才發放獎勵
				if (result == "Win")
				{
					game.ExpGained = experience;
					game.ExpGainedTime = _appClock.UtcNow;
					game.PointsGained = points;
					game.PointsGainedTime = _appClock.UtcNow;

					// 更新用戶錢包點數
					var wallet = await _context.UserWallets
						.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

					if (wallet != null)
					{
						wallet.UserPoint += points;
						wallet.IsDeleted = false; // 確保未軟刪除
						_context.UserWallets.Update(wallet);
					}

					// 更新寵物經驗值
					var pet = await _context.Pets
						.FirstOrDefaultAsync(p => p.PetId == game.PetId && !p.IsDeleted);

					if (pet != null)
					{
						pet.Experience += experience;
						pet.CurrentExperience = pet.Experience;
						_context.Pets.Update(pet);

						// 根據難度更新寵物屬性 Delta
						ApplyGameResultToPetStats(game, true);
					}
				}
				else
				{
					// 失敗或中止也會有屬性影響
					var pet = await _context.Pets
						.FirstOrDefaultAsync(p => p.PetId == game.PetId && !p.IsDeleted);

					if (pet != null)
					{
						ApplyGameResultToPetStats(game, false);
					}
				}

				_context.MiniGames.Update(game);
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return (true, $"遊戲結束 - {result}，獎勵已發放");
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "結束遊戲時發生錯誤 (PlayId: {PlayId}, UserId: {UserId})", playId, userId);
				return (false, "結束遊戲失敗，請稍後重試");
			}
		}

		/// <summary>
		/// 獲取遊戲歷史（分頁、支持篩選）
		/// </summary>
		public async Task<(List<Models.MiniGame> games, int totalCount)> GetGameHistoryAsync(
			int userId,
			int page,
			int pageSize,
			int? level = null,
			DateTime? startDate = null,
			DateTime? endDate = null)
		{
			try
			{
				// 建立查詢
				var query = _context.MiniGames
					.AsNoTracking()
					.Where(g => g.UserId == userId && !g.IsDeleted && g.EndTime != null);

				// 篩選難度
				if (level.HasValue && level >= 1 && level <= 3)
				{
					query = query.Where(g => g.Level == level.Value);
				}

				// 篩選日期範圍（轉換為 UTC 進行查詢）
				if (startDate.HasValue || endDate.HasValue)
				{
					if (startDate.HasValue)
					{
						var utcStart = _appClock.ToUtc(startDate.Value);
						query = query.Where(g => g.StartTime >= utcStart);
					}

					if (endDate.HasValue)
					{
						var endDateUtc8 = endDate.Value.AddDays(1).AddTicks(-1);
						var utcEnd = _appClock.ToUtc(endDateUtc8);
						query = query.Where(g => g.StartTime <= utcEnd);
					}
				}

				// 統計總筆數
				int totalCount = await query.CountAsync();

				// 分頁查詢
				var games = await query
					.OrderByDescending(g => g.StartTime)
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.ToListAsync();

				return (games, totalCount);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "獲取遊戲歷史時發生錯誤 (UserId: {UserId})", userId);
				return (new List<Models.MiniGame>(), 0);
			}
		}

		/// <summary>
		/// 獲取每日遊戲限制次數（從 SystemSettings 讀取，預設 3）
		/// </summary>
		private async Task<int> GetDailyGameLimitAsync()
		{
			try
			{
				var setting = await _context.SystemSettings
					.AsNoTracking()
					.FirstOrDefaultAsync(s => s.SettingKey == "DailyGameLimit");

				if (setting != null && int.TryParse(setting.SettingValue, out int limit))
				{
					return limit;
				}

				return 3; // 預設 3 次
			}
			catch
			{
				return 3; // 讀取失敗也預設 3 次
			}
		}

		/// <summary>
		/// 根據難度等級取得怪物數量
		/// Level 1: 3, Level 2: 5, Level 3: 7
		/// </summary>
		private int GetMonsterCountByLevel(int level)
		{
			return level switch
			{
				1 => 3,
				2 => 5,
				3 => 7,
				_ => 3
			};
		}

		/// <summary>
		/// 根據難度等級取得速度倍數
		/// Level 1: 1.0, Level 2: 1.2, Level 3: 1.5
		/// </summary>
		private decimal GetSpeedMultiplierByLevel(int level)
		{
			return level switch
			{
				1 => 1.0m,
				2 => 1.2m,
				3 => 1.5m,
				_ => 1.0m
			};
		}

		/// <summary>
		/// 應用遊戲結果到寵物屬性 Delta
		/// 規則: Win 時飢餓-20、心情+30、體力-20、清潔-20
		///      Lose/Abort 時飢餓-20、心情-30、體力-20、清潔-20
		/// </summary>
		private void ApplyGameResultToPetStats(Models.MiniGame game, bool isWin)
		{
			if (isWin)
			{
				// 勝利: 飢餓-20、心情+30、體力-20、清潔-20
				game.HungerDelta = -20;
				game.MoodDelta = 30;
				game.StaminaDelta = -20;
				game.CleanlinessDelta = -20;
			}
			else
			{
				// 失敗或中止: 飢餓-20、心情-30、體力-20、清潔-20
				game.HungerDelta = -20;
				game.MoodDelta = -30;
				game.StaminaDelta = -20;
				game.CleanlinessDelta = -20;
			}
		}
	}
}
