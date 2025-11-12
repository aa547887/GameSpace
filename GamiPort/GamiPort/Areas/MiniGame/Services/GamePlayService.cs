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
		private readonly IPetService _petService;

		public GamePlayService(
			GameSpacedatabaseContext context,
			IAppClock appClock,
			ILogger<GamePlayService> logger,
			IPetService petService)
		{
			_context = context;
			_appClock = appClock;
			_logger = logger;
			_petService = petService;
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
		/// 獲取用戶下一次應挑戰的關卡等級
		/// 規則：首次從第 1 關開始，勝利提升至下一關，失敗保持當前關卡，最高第 3 關
		/// </summary>
		private async Task<int> GetUserNextGameLevelAsync(int userId)
		{
			try
			{
				// 查詢該用戶最後一場完成的遊戲（非中止）
				var lastGame = await _context.MiniGames
					.AsNoTracking()
					.Where(g => g.UserId == userId
							 && g.EndTime != null  // 已完成
							 && g.Result != null   // 有結果
							 && !g.Aborted         // 非中止
							 && !g.IsDeleted)      // 未刪除
					.OrderByDescending(g => g.EndTime)
					.FirstOrDefaultAsync();

				if (lastGame == null)
				{
					// 首次遊戲，從第 1 關開始
					return 1;
				}

				// 根據上次結果決定下次關卡
				int nextLevel;
				if (lastGame.Result == "Win")
				{
					// 勝利：提升至下一關（最高第 3 關）
					nextLevel = Math.Min(lastGame.Level + 1, 3);
				}
				else // "Lose"
				{
					// 失敗：留在同一關
					nextLevel = lastGame.Level;
				}

				return nextLevel;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "計算下一關卡等級時發生錯誤 (UserId: {UserId})", userId);
				return 1; // 發生錯誤時預設為第 1 關
			}
		}

		/// <summary>
		/// 開始遊戲
		/// 檢查剩餘次數、建立遊戲記錄
		/// 自動根據用戶歷史記錄決定關卡難度
		/// </summary>
		public async Task<(bool success, string message, int? playId, int level)> StartGameAsync(int userId)
		{
			try
			{
				// 自動計算用戶下一次應挑戰的關卡
				int level = await GetUserNextGameLevelAsync(userId);

				// 檢查用戶是否存在且未被鎖定（檢查 UserLockoutEnd 是否為 null 或已過期）
				var user = await _context.Users
					.AsNoTracking()
					.FirstOrDefaultAsync(u => u.UserId == userId && (u.UserLockoutEnd == null || u.UserLockoutEnd <= _appClock.UtcNow));

				if (user == null)
				{
					return (false, "用戶不存在或已被鎖定", null, 0);
				}

				// 檢查今日剩餘次數
				int remaining = await GetUserRemainingPlaysAsync(userId);
				if (remaining <= 0)
				{
					return (false, "今日遊戲次數已用完，請明天再來", null, 0);
				}

				// 獲取用戶的寵物（取第一隻，或根據邏輯選擇）
				var pet = await _context.Pets
					.AsNoTracking()
					.FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

				if (pet == null)
				{
					return (false, "用戶無寵物資料", null, 0);
				}

				// 商業規則：冒險開始前檢查寵物狀態（已移除限制）
				// 註解：已移除此限制，允許任何狀態下都可以開始冒險
				//if (pet.Hunger == 0 || pet.Mood == 0 || pet.Stamina == 0 ||
				//	pet.Cleanliness == 0 || pet.Health == 0)
				//{
				//	var statusList = new List<string>();
				//	if (pet.Hunger == 0) statusList.Add("飢餓");
				//	if (pet.Mood == 0) statusList.Add("心情");
				//	if (pet.Stamina == 0) statusList.Add("體力");
				//	if (pet.Cleanliness == 0) statusList.Add("清潔");
				//	if (pet.Health == 0) statusList.Add("健康");
				//
				//	return (false, $"寵物狀態不佳（{string.Join("、", statusList)}值為0），請先進行互動恢復寵物狀態", null, 0);
				//}

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

				return (true, $"遊戲已啟動 - 第 {level} 關", game.PlayId, level);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "開始遊戲時發生錯誤 (UserId: {UserId})", userId);
				return (false, "啟動遊戲失敗，請稍後重試", null, 0);
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

				// 獲取關卡對應的獎勵配置
				var (rewardExp, rewardPoints, hasCoupon) = GetRewardsByLevel(level);

				// 只有勝利才發放獎勵
				if (result == "Win")
				{
					game.ExpGained = rewardExp;
					game.ExpGainedTime = _appClock.UtcNow;
					game.PointsGained = rewardPoints;
					game.PointsGainedTime = _appClock.UtcNow;

					// 更新用戶錢包點數
					var wallet = await _context.UserWallets
						.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

					if (wallet != null)
					{
						wallet.UserPoint += rewardPoints;
						wallet.IsDeleted = false; // 確保未軟刪除
						_context.UserWallets.Update(wallet);
					}

					// 添加WalletHistory記錄 - 點數獎勵
					if (rewardPoints > 0 && wallet != null)
					{
						var appNowForHistory = _appClock.ToAppTime(_appClock.UtcNow);
						var pointHistory = new WalletHistory
						{
							UserId = userId,
							ChangeType = "Point",
							PointsChanged = rewardPoints,
							ItemCode = $"GAME-STAGE-{level}",
							Description = $"冒險勝利獎勵 (第{level}關)",
							ChangeTime = appNowForHistory,
							IsDeleted = false
						};
						_context.WalletHistories.Add(pointHistory);
					}

					// 添加WalletHistory記錄 - 優惠券獎勵（第3關）
					if (hasCoupon && level == 3)
					{
						var appNowForHistory = _appClock.ToAppTime(_appClock.UtcNow);
						var couponHistory = new WalletHistory
						{
							UserId = userId,
							ChangeType = "Coupon",
							PointsChanged = 1,
							ItemCode = "GAME-COUPON-STAGE3",
							Description = $"冒險勝利優惠券獎勵 (第3關)",
							ChangeTime = appNowForHistory,
							IsDeleted = false
						};
						_context.WalletHistories.Add(couponHistory);
					}

					// 根據難度更新寵物屬性 Delta
					ApplyGameResultToPetStats(game, true);
				}
				else
				{
					// 失敗或中止也會有屬性影響
					ApplyGameResultToPetStats(game, false);
				}

				// 真正更新寵物的屬性值（應用 Delta）
				var pet = await _context.Pets.FirstOrDefaultAsync(p => p.PetId == game.PetId && !p.IsDeleted);
				if (pet != null)
				{
					// 應用變化並鉗位到 0-100
					pet.Hunger = Math.Max(0, Math.Min(100, pet.Hunger + game.HungerDelta));
					pet.Mood = Math.Max(0, Math.Min(100, pet.Mood + game.MoodDelta));
					pet.Stamina = Math.Max(0, Math.Min(100, pet.Stamina + game.StaminaDelta));
					pet.Cleanliness = Math.Max(0, Math.Min(100, pet.Cleanliness + game.CleanlinessDelta));

					// 檢查全滿回復：當飢餓、心情、體力、清潔四項值均達到 100 時，寵物健康值恢復至 100
					if (pet.Hunger == 100 && pet.Mood == 100 &&
						pet.Stamina == 100 && pet.Cleanliness == 100)
					{
						pet.Health = 100;
					}

					_context.Pets.Update(pet);
				}

				_context.MiniGames.Update(game);
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				// 勝利後更新寵物經驗值並觸發升級檢查（在事務外執行）
				if (result == "Win" && experience > 0)
				{
					try
					{
						await _petService.AddExperienceAsync(game.PetId, experience);
					}
					catch (Exception petEx)
					{
						_logger.LogError(petEx, "更新寵物經驗值時發生錯誤 (PetId: {PetId}, Exp: {Experience})", game.PetId, experience);
						// 不影響遊戲結果，僅記錄錯誤
					}
				}

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
		/// 第 1 關：6 隻怪物
		/// 第 2 關：8 隻怪物
		/// 第 3 關：10 隻怪物
		/// </summary>
		private int GetMonsterCountByLevel(int level)
		{
			return level switch
			{
				1 => 6,
				2 => 8,
				3 => 10,
				_ => 6
			};
		}

		/// <summary>
		/// 根據難度等級取得速度倍數
		/// 第 1 關：1.0 倍速度
		/// 第 2 關：1.5 倍速度
		/// 第 3 關：2.0 倍速度
		/// </summary>
		private decimal GetSpeedMultiplierByLevel(int level)
		{
			return level switch
			{
				1 => 1.0m,
				2 => 1.5m,
				3 => 2.0m,
				_ => 1.0m
			};
		}

		/// <summary>
		/// 根據關卡等級計算獎勵（經驗值和點數）
		/// 第 1 關：+100 經驗值，+10 點數
		/// 第 2 關：+200 經驗值，+20 點數
		/// 第 3 關：+300 經驗值，+30 點數，+1 張商城優惠券
		/// </summary>
		private (int experience, int points, bool hasCoupon) GetRewardsByLevel(int level)
		{
			return level switch
			{
				1 => (100, 10, false),
				2 => (200, 20, false),
				3 => (300, 30, true),
				_ => (100, 10, false)
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
