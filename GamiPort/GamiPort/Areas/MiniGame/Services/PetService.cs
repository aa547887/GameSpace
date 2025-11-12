using GamiPort.Infrastructure.Security;
using GamiPort.Infrastructure.Time;
using GamiPort.Models;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.MiniGame.Services
{
	/// <summary>
	/// 寵物服務實現 (GamiPort 前台)
	/// </summary>
	public class PetService : IPetService
	{
		private readonly GameSpacedatabaseContext _context;
		private readonly IAppClock _appClock;

		// 互動點數成本配置
		private const int INTERACT_POINT_COST = 5;

		public PetService(GameSpacedatabaseContext context, IAppClock appClock)
		{
			_context = context;
			_appClock = appClock;
		}

		/// <summary>
		/// 獲取用戶寵物信息
		/// </summary>
		public async Task<Pet?> GetUserPetAsync(int userId)
		{
			return await _context.Pets
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
		}

		/// <summary>
		/// 執行寵物互動（餵食/洗澡/玩耍/睡覺）
		/// </summary>
		public async Task<PetInteractionResult> InteractWithPetAsync(int userId, string action)
		{
			// 獲取用戶的寵物
			var pet = await _context.Pets
				.FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

			if (pet == null)
			{
				return new PetInteractionResult
				{
					Success = false,
					Message = "未找到寵物信息"
				};
			}

			// 檢查寵物健康狀態 - 任一屬性為0時無法互動
			// 註解：已移除此限制，允許任何狀態下都可以互動
			//if (pet.Hunger == 0 || pet.Mood == 0 || pet.Stamina == 0 ||
			//	pet.Cleanliness == 0 || pet.Health == 0)
			//{
			//	return new PetInteractionResult
			//	{
			//		Success = false,
			//		Message = "寵物狀態不佳，無法進行互動"
			//	};
			//}

			// 檢查是否五值全滿 - 全滿時不允許任何互動
			if (pet.Hunger == 100 && pet.Mood == 100 && pet.Stamina == 100 &&
				pet.Cleanliness == 100 && pet.Health == 100)
			{
				return new PetInteractionResult
				{
					Success = false,
					Message = $"{pet.PetName}已經是健康寶寶了，是時候讓他獨立了🙃！",
					Pet = pet
				};
			}

			// 獲取用戶錢包（用於全滿獎勵發放）
			var wallet = await _context.UserWallets
				.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

			// 開啟事務
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				// 記錄互動前的狀態（用於計算變化量）
				var statChangesBefore = new Dictionary<string, int>
				{
					{ "hunger", pet.Hunger },
					{ "mood", pet.Mood },
					{ "stamina", pet.Stamina },
					{ "cleanliness", pet.Cleanliness },
					{ "health", pet.Health }
				};

				// 根據互動類型修改寵物屬性（鉗位到 0-100）
				// 商業規則用詞：餵食/洗澡/哄睡/休息
				var actionLower = action?.ToLower() ?? string.Empty;
				string actionMessage = "";
				switch (actionLower)
				{
					case "feed":
						// 餵食：飢餓值增加10
						// 檢查是否已滿
						if (pet.Hunger >= 100)
						{
							await transaction.RollbackAsync();
							return new PetInteractionResult
							{
								Success = false,
								Message = $"{pet.PetName}吃太飽了，禁止拍打餵食！",
								Pet = pet
							};
						}
						pet.Hunger = Math.Max(0, Math.Min(pet.Hunger + 10, 100));
						actionMessage = "餵食";
						break;
					case "bath":
						// 洗澡：清潔值增加10
						// 檢查是否已滿
						if (pet.Cleanliness >= 100)
						{
							await transaction.RollbackAsync();
							return new PetInteractionResult
							{
								Success = false,
								Message = $"{pet.PetName}已經洗好澎澎了，再洗就脫皮啦！",
								Pet = pet
							};
						}
						pet.Cleanliness = Math.Max(0, Math.Min(pet.Cleanliness + 10, 100));
						actionMessage = "洗澡";
						break;
					case "comfort":
					case "play": // 向後兼容，但建議使用 comfort
						// 哄睡：心情值增加10
						// 檢查是否已滿
						if (pet.Mood >= 100)
						{
							await transaction.RollbackAsync();
							return new PetInteractionResult
							{
								Success = false,
								Message = $"{pet.PetName}心情超High，不用再陪他玩囉！",
								Pet = pet
							};
						}
						pet.Mood = Math.Max(0, Math.Min(pet.Mood + 10, 100));
						actionMessage = "玩耍";
						break;
					case "rest":
					case "sleep": // 向後兼容，但建議使用 rest
						// 休息：體力值增加10
						// 檢查是否已滿
						if (pet.Stamina >= 100)
						{
							await transaction.RollbackAsync();
							return new PetInteractionResult
							{
								Success = false,
								Message = $"{pet.PetName}已經睡飽飽了，再哄他，就要森77了喔！",
								Pet = pet
							};
						}
						pet.Stamina = Math.Max(0, Math.Min(pet.Stamina + 10, 100));
						actionMessage = "哄睡";
						break;
					default:
						await transaction.RollbackAsync();
						return new PetInteractionResult
						{
							Success = false,
							Message = "無效的互動類型（有效值：feed/bath/comfort/rest）"
						};
				}

				// 計算數值變化
				var statChanges = new Dictionary<string, int>();
				foreach (var kvp in statChangesBefore)
				{
					int newValue = kvp.Key switch
					{
						"hunger" => pet.Hunger,
						"mood" => pet.Mood,
						"stamina" => pet.Stamina,
						"cleanliness" => pet.Cleanliness,
						"health" => pet.Health,
						_ => kvp.Value
					};
					int change = newValue - kvp.Value;
					if (change != 0)
					{
						statChanges[kvp.Key] = change;
					}
				}

				// 註解：互動不增加經驗值（用戶要求）
				// const int expPerInteraction = 10; // 每次互動獲得10經驗值
				// pet.Experience += expPerInteraction;
				// statChanges["experience"] = expPerInteraction;

				// 商業規則：全滿回復
				// 當飢餓、心情、體力、清潔四項值均達到 100 時，寵物健康值恢復至 100
				bool healthRecovered = false;
				bool isFirstDailyFullStats = false;
				int bonusExp = 0;
				int bonusPoints = 0;
				int experienceGainedThisInteraction = 0; // 追蹤本次互動獲得的經驗值

				if (pet.Hunger == 100 && pet.Mood == 100 &&
					pet.Stamina == 100 && pet.Cleanliness == 100)
				{
					// 健康值回復
					if (pet.Health < 100)
					{
						pet.Health = 100;
						statChanges["health"] = 100 - statChangesBefore["health"];
						healthRecovered = true;
					}

					// 商業規則：每日狀態全滿獎勵
					// 寵物若於每日首次同時達到飢餓、心情、體力、清潔、健康值皆 100，則額外獲得 100 點寵物經驗值 + 100 會員點數
					var today = _appClock.ToAppTime(_appClock.UtcNow).Date; // UTC+8
					var todayItemCode = $"PET-FULLSTATS-{today:yyyy-MM-dd}";

					// 檢查今日是否已發放全滿獎勵
					var alreadyGrantedToday = await _context.WalletHistories
						.AnyAsync(w => w.UserId == userId
									&& w.ItemCode == todayItemCode
									&& !w.IsDeleted);

					if (!alreadyGrantedToday && pet.Health == 100)
					{
						// 讀取獎勵配置（預設 100 經驗值、100 點數）
						bonusExp = 100; // 商業規則：每日狀態全滿獎勵 +100 經驗值
						bonusPoints = 100; // 商業規則：每日狀態全滿獎勵 +100 點會員點數
						isFirstDailyFullStats = true;

						// 發放寵物經驗值（五值全滿獎勵）
						pet.Experience += bonusExp;
						experienceGainedThisInteraction = bonusExp; // 記錄本次獲得的經驗值
						statChanges["experience"] = bonusExp; // 只有五值全滿獎勵 100

						// 發放會員點數（如果有配置）
						if (bonusPoints > 0)
						{
							wallet.UserPoint += bonusPoints;
						}

						// 記錄到 WalletHistory（用於防重複發放全滿獎勵）
						var historyRecord = new WalletHistory
						{
							UserId = userId,
							ChangeType = "Point",
							PointsChanged = bonusPoints,
							ItemCode = todayItemCode,
							Description = $"寵物狀態全滿獎勵（經驗值+{bonusExp}，點數+{bonusPoints}）",
							ChangeTime = _appClock.ToAppTime(_appClock.UtcNow),
							IsDeleted = false
						};
						_context.WalletHistories.Add(historyRecord);
					}
				}

				// 檢查升級（所有互動後都檢查，不限於五值全滿）
				int oldLevel = pet.Level;
				int totalLevelUpRewards = 0;

				var requiredExp = await GetRequiredExpForLevelAsync(pet.Level + 1);
				while (pet.Experience >= requiredExp && requiredExp > 0)
				{
					// 執行升級
					pet.Level++;
					pet.LevelUpTime = _appClock.UtcNow;
					pet.Experience -= requiredExp; // 保留溢出經驗值

					// 計算升級獎勵
					var pointsReward = CalculateLevelUpReward(pet.Level);
					wallet.UserPoint += pointsReward;
					totalLevelUpRewards += pointsReward;

					// 記錄升級獎勵到錢包歷史
					_context.WalletHistories.Add(new WalletHistory
					{
						UserId = userId,
						ChangeType = "Pet",
						PointsChanged = pointsReward,
						ItemCode = $"PET_LEVELUP_{pet.Level}",
						Description = $"寵物升級至 Level {pet.Level}",
						ChangeTime = _appClock.ToAppTime(_appClock.UtcNow),
						IsDeleted = false
					});

					// 檢查下一級
					requiredExp = await GetRequiredExpForLevelAsync(pet.Level + 1);
				}

				// 記錄升級信息到statChanges
				if (pet.Level > oldLevel)
				{
					statChanges["leveledUp"] = 1;
					statChanges["oldLevel"] = oldLevel;
					statChanges["newLevel"] = pet.Level;
					statChanges["levelUpRewards"] = totalLevelUpRewards;
				}

				// 保存更改
				_context.Pets.Update(pet);
				if (wallet != null)
				{
					_context.UserWallets.Update(wallet);
				}
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				// 構建返回消息
				string message = $"{actionMessage}成功！";

				// 重新計算下一級所需經驗值
				var expToNext = await GetRequiredExpForLevelAsync(pet.Level + 1);
				pet.ExperienceToNextLevel = expToNext;

				// 提取升級信息
				bool leveledUp = statChanges.ContainsKey("leveledUp");
				int resultOldLevel = statChanges.ContainsKey("oldLevel") ? statChanges["oldLevel"] : pet.Level;
				int resultNewLevel = statChanges.ContainsKey("newLevel") ? statChanges["newLevel"] : pet.Level;
				int levelUpRewards = statChanges.ContainsKey("levelUpRewards") ? statChanges["levelUpRewards"] : 0;

				return new PetInteractionResult
				{
					Success = true,
					Message = message,
					Pet = pet,
					StatChanges = statChanges,
					HealthRecovered = healthRecovered,
					IsFirstDailyFullStats = isFirstDailyFullStats,
					BonusExperience = bonusExp,
					BonusPoints = bonusPoints,
					LeveledUp = leveledUp,
					OldLevel = resultOldLevel,
					NewLevel = resultNewLevel,
					LevelUpRewards = levelUpRewards
				};
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return new PetInteractionResult
				{
					Success = false,
					Message = $"互動失敗：{ex.Message}"
				};
			}
		}

		/// <summary>
		/// 更新寵物外觀（膚色和背景）
		/// </summary>
		public async Task<PetUpdateAppearanceResult> UpdatePetAppearanceAsync(int userId, string skinColor, string background)
		{
			// 標準化處理，避免大小寫匹配問題
			if (!string.IsNullOrWhiteSpace(skinColor))
				skinColor = skinColor.ToUpperInvariant();
			if (!string.IsNullOrWhiteSpace(background))
				background = background.ToUpperInvariant();

			// 獲取用戶的寵物
			var pet = await _context.Pets
				.FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

			if (pet == null)
			{
				return new PetUpdateAppearanceResult
				{
					Success = false,
					Message = "未找到寵物信息"
				};
			}

			// 獲取用戶錢包
			var wallet = await _context.UserWallets
				.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

			if (wallet == null)
			{
				return new PetUpdateAppearanceResult
				{
					Success = false,
					Message = "錢包信息不存在"
				};
			}

			var totalPointCost = 0;
			var updateMessage = new List<string>();

			// 檢查膚色變更
			if (!string.IsNullOrWhiteSpace(skinColor) && pet.SkinColor != skinColor)
			{
				var skinSetting = await _context.PetSkinColorCostSettings
					.AsNoTracking()
					.FirstOrDefaultAsync(s => s.ColorCode == skinColor && !s.IsDeleted && s.IsActive);

				if (skinSetting == null)
				{
					return new PetUpdateAppearanceResult
					{
						Success = false,
						Message = "所選膚色不存在或不可用"
					};
				}

				totalPointCost += skinSetting.PointsCost;
				updateMessage.Add($"膚色：{skinSetting.ColorName}（{skinSetting.PointsCost}點）");
			}

			// 檢查背景變更
			if (!string.IsNullOrWhiteSpace(background) && pet.BackgroundColor != background)
			{
				var backgroundSetting = await _context.PetBackgroundCostSettings
					.AsNoTracking()
					.FirstOrDefaultAsync(s => s.BackgroundCode == background && !s.IsDeleted && s.IsActive);

				if (backgroundSetting == null)
				{
					return new PetUpdateAppearanceResult
					{
						Success = false,
						Message = "所選背景不存在或不可用"
					};
				}

				totalPointCost += backgroundSetting.PointsCost;
				updateMessage.Add($"背景：{backgroundSetting.BackgroundName}（{backgroundSetting.PointsCost}點）");
			}

			// 檢查是否有足夠點數
			if (totalPointCost > 0 && wallet.UserPoint < totalPointCost)
			{
				return new PetUpdateAppearanceResult
				{
					Success = false,
					Message = $"會員點數不足！需要{totalPointCost}點，目前擁有{wallet.UserPoint}點"
				};
			}

			// 如果沒有任何變更
			if (totalPointCost == 0)
			{
				return new PetUpdateAppearanceResult
				{
					Success = true,
					Message = "未進行任何變更",
					Pet = pet
				};
			}

			// 開啟事務
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var nowUtc8 = _appClock.ToAppTime(_appClock.UtcNow);

				// 更新膚色
				if (!string.IsNullOrWhiteSpace(skinColor) && pet.SkinColor != skinColor)
				{
					pet.SkinColor = skinColor;
					pet.SkinColorChangedTime = nowUtc8;
				}

				// 更新背景
				if (!string.IsNullOrWhiteSpace(background) && pet.BackgroundColor != background)
				{
					pet.BackgroundColor = background;
					pet.BackgroundColorChangedTime = nowUtc8;
				}

				// 扣除點數
				wallet.UserPoint -= totalPointCost;

				// 記錄到 WalletHistory（補充遺漏的記錄）
				var upgradeParts = new List<string>();
				if (!string.IsNullOrWhiteSpace(skinColor) && pet.SkinColor != skinColor)
				{
					upgradeParts.Add("膚色");
				}
				if (!string.IsNullOrWhiteSpace(background) && pet.BackgroundColor != background)
				{
					upgradeParts.Add("背景");
				}

				_context.WalletHistories.Add(new WalletHistory
				{
					UserId = userId,
					ChangeType = "Pet",
					PointsChanged = -totalPointCost,
					ItemCode = $"PET-UPGRADE-{userId}-{nowUtc8:yyyyMMddHHmmss}",
					Description = $"寵物外觀升級（{string.Join("、", upgradeParts)}）",
					ChangeTime = nowUtc8,
					IsDeleted = false
				});

				// 保存更改
				_context.Pets.Update(pet);
				_context.UserWallets.Update(wallet);
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				var message = $"定制成功！消耗{totalPointCost}點點數。更新項目：{string.Join("、", updateMessage)}";
				return new PetUpdateAppearanceResult
				{
					Success = true,
					Message = message,
					Pet = pet
				};
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return new PetUpdateAppearanceResult
				{
					Success = false,
					Message = $"定制失敗：{ex.Message}"
				};
			}
		}

		/// <summary>
		/// 修改寵物名稱
		/// </summary>
		public async Task<PetUpdateNameResult> UpdatePetNameAsync(int userId, string newName)
		{
			// 驗證名稱
			if (string.IsNullOrWhiteSpace(newName))
			{
				return new PetUpdateNameResult
				{
					Success = false,
					Message = "寵物名稱不能為空"
				};
			}

			// 名稱長度驗證（1-20字元）
			newName = newName.Trim();
			if (newName.Length < 1 || newName.Length > 20)
			{
				return new PetUpdateNameResult
				{
					Success = false,
					Message = "寵物名稱長度必須為 1-20 字元"
				};
			}

			// 獲取用戶的寵物
			var pet = await _context.Pets
				.FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

			if (pet == null)
			{
				return new PetUpdateNameResult
				{
					Success = false,
					Message = "未找到寵物信息"
				};
			}

			// 檢查名稱是否與當前名稱相同
			if (pet.PetName == newName)
			{
				return new PetUpdateNameResult
				{
					Success = true,
					Message = "名稱未變更",
					Pet = pet
				};
			}

			try
			{
				// 更新名稱
				pet.PetName = newName;

				// 保存更改
				_context.Pets.Update(pet);
				await _context.SaveChangesAsync();

				return new PetUpdateNameResult
				{
					Success = true,
					Message = "寵物名稱更新成功",
					Pet = pet
				};
			}
			catch (Exception ex)
			{
				return new PetUpdateNameResult
				{
					Success = false,
					Message = $"名稱更新失敗：{ex.Message}"
				};
			}
		}

		/// <summary>
		/// 獲取可用的膚色列表（包括所有11種，含限時活動限定已失效的）
		/// </summary>
		public async Task<IEnumerable<PetSkinColorCostSetting>> GetAvailableSkinsAsync()
		{
			return await _context.PetSkinColorCostSettings
				.AsNoTracking()
				.Where(s => !s.IsDeleted)
				.OrderBy(s => s.DisplayOrder)
				.ToListAsync();
		}

		/// <summary>
		/// 獲取可用的背景列表（包括所有11種，含限時活動限定已失效的）
		/// </summary>
		public async Task<IEnumerable<PetBackgroundCostSetting>> GetAvailableBackgroundsAsync()
		{
			return await _context.PetBackgroundCostSettings
				.AsNoTracking()
				.Where(s => !s.IsDeleted)
				.OrderBy(s => s.DisplayOrder ?? 0)
				.ToListAsync();
		}

		/// <summary>
		/// 增加寵物經驗值，並自動檢查升級
		/// </summary>
		public async Task<bool> AddExperienceAsync(int petId, int exp)
		{
			if (exp < 0)
			{
				return false;
			}

			var pet = await _context.Pets
				.FirstOrDefaultAsync(p => p.PetId == petId && !p.IsDeleted);

			if (pet == null)
			{
				return false;
			}

			// 增加經驗值
			pet.Experience += exp;

			// 取得用戶ID（用於升級條件檢查）
			var userId = pet.UserId;

			// 自動檢查升級（支援跨多級升級）
			var requiredExp = await GetRequiredExpForLevelAsync(pet.Level + 1);

			while (pet.Experience >= requiredExp && requiredExp > 0)
			{
				// 執行升級
				pet.Level++;
				pet.LevelUpTime = _appClock.UtcNow;
				pet.Experience -= requiredExp;

				// 計算升級獎勵
				var pointsReward = CalculateLevelUpReward(pet.Level);

				// 發放點數到錢包
				var wallet = await _context.UserWallets
					.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

				if (wallet != null)
				{
					wallet.UserPoint += pointsReward;
					_context.UserWallets.Update(wallet);
				}

				// 記錄升級獎勵到錢包歷史
				_context.WalletHistories.Add(new WalletHistory
				{
					UserId = userId,
					ChangeType = "Pet",
					PointsChanged = pointsReward,
					ItemCode = $"PET_LEVELUP_{pet.Level}",
					Description = $"寵物升級至 Level {pet.Level}",
					ChangeTime = _appClock.ToAppTime(_appClock.UtcNow),
					IsDeleted = false
				});

				// 檢查下一級
				requiredExp = await GetRequiredExpForLevelAsync(pet.Level + 1);
			}

			// 保存最終經驗值變更
			_context.Pets.Update(pet);
			await _context.SaveChangesAsync();

			return true;
		}

		/// <summary>
		/// 寵物升級並發放獎勵
		/// </summary>
		public async Task<bool> LevelUpPetAsync(int petId)
		{
			var pet = await _context.Pets
				.FirstOrDefaultAsync(p => p.PetId == petId && !p.IsDeleted);

			if (pet == null)
			{
				return false;
			}

			// 獲取當前等級所需經驗值
			var requiredExp = await GetRequiredExpForLevelAsync(pet.Level + 1);
			if (requiredExp == 0)
			{
				// 已達最高等級
				return false;
			}

			if (pet.Experience < requiredExp)
			{
				// 經驗值不足
				return false;
			}

			// 開啟事務
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var utcNow = _appClock.UtcNow;

				// 升級
				pet.Level++;
				pet.LevelUpTime = utcNow;
				pet.Experience -= requiredExp; // 保留溢出經驗值

				// 計算獎勵
				var pointsReward = CalculateLevelUpReward(pet.Level);

				// 更新用戶錢包
				var wallet = await _context.UserWallets
					.FirstOrDefaultAsync(w => w.UserId == pet.UserId && !w.IsDeleted);

				if (wallet != null)
				{
					wallet.UserPoint += pointsReward;

					// 記錄到錢包歷史
					_context.WalletHistories.Add(new WalletHistory
					{
						UserId = pet.UserId,
						ChangeType = "Pet",
						PointsChanged = pointsReward,
						ItemCode = $"PET_LEVELUP_{pet.Level}",
						Description = $"寵物升級至 Level {pet.Level}",
						ChangeTime = utcNow
					});
				}

				// 保存更改
				_context.Pets.Update(pet);
				if (wallet != null)
				{
					_context.UserWallets.Update(wallet);
				}
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return true;
			}
			catch
			{
				await transaction.RollbackAsync();
				return false;
			}
		}

		// [废弃] 移除每日首次升级限制机制 - 2025-11-10
		/*
		/// <summary>
		/// 檢查今日是否可以進行升級
		/// 規則：每天UTC+8 00:00後，第一次升級需要滿足以下條件之一：
		/// 條件A：當天第一次簽到且有獲得寵物經驗值獎勵
		/// 條件B：當天第一次達成五個屬性值全滿
		/// 當天第一次升級後，後續升級恢復正常邏輯（只要經驗值足夠就升級）
		/// </summary>
		/// <param name="userId">用戶ID</param>
		/// <returns>是否可以升級</returns>
		private async Task<bool> CheckCanLevelUpTodayAsync(int userId)
		{
			var today = _appClock.ToAppTime(_appClock.UtcNow).Date; // UTC+8
			var todayFirstLevelUpCode = $"PET-FIRST-LEVELUP-{today:yyyy-MM-dd}";

			// 1. 檢查今天是否已經完成過首次升級
			var hasFirstLevelUpToday = await _context.WalletHistories
				.AnyAsync(wh => wh.UserId == userId
							&& wh.ItemCode == todayFirstLevelUpCode
							&& !wh.IsDeleted);

			if (hasFirstLevelUpToday)
			{
				// 今天已完成首次升級，允許正常升級
				return true;
			}

			// 2. 今天尚未完成首次升級，需檢查觸發條件
			// 檢查條件A：今日是否簽到且獲得經驗值
			var hasSignInExpToday = await CheckHasSignedInWithExpTodayAsync(userId, today);

			// 檢查條件B：今日是否達成五屬性全滿
			var hasFullStatsToday = await CheckHasFullStatsTodayAsync(userId, today);

			// 滿足任一觸發條件即可升級
			return hasSignInExpToday || hasFullStatsToday;
		}
		*/

		// [废弃] 移除每日首次升级限制机制 - 2025-11-10
		/*
		/// <summary>
		/// 檢查條件A：今日是否簽到且獲得寵物經驗值
		/// </summary>
		/// <param name="userId">用戶ID</param>
		/// <param name="today">今日日期（UTC+8）</param>
		/// <returns>是否滿足條件A</returns>
		private async Task<bool> CheckHasSignedInWithExpTodayAsync(int userId, DateTime today)
		{
			// 計算今日的UTC時間範圍
			var utcTodayStart = _appClock.ToUtc(today);
			var utcTodayEnd = _appClock.ToUtc(today.AddDays(1).AddTicks(-1));

			// 檢查 UserSignInStats 表中今日簽到記錄的 ExpGained 欄位
			var todaySignIn = await _context.UserSignInStats
				.Where(s => s.UserId == userId
						&& s.SignTime >= utcTodayStart
						&& s.SignTime <= utcTodayEnd
						&& !s.IsDeleted)
				.FirstOrDefaultAsync();

			return todaySignIn != null && todaySignIn.ExpGained > 0;
		}
		*/

		// [废弃] 移除每日首次升级限制机制 - 2025-11-10
		/*
		/// <summary>
		/// 檢查條件B：今日是否達成五屬性全滿
		/// </summary>
		/// <param name="userId">用戶ID</param>
		/// <param name="today">今日日期（UTC+8）</param>
		/// <returns>是否滿足條件B</returns>
		private async Task<bool> CheckHasFullStatsTodayAsync(int userId, DateTime today)
		{
			// 檢查 WalletHistory 中今日是否有全滿獎勵記錄
			var fullStatsItemCode = $"PET-FULLSTATS-{today:yyyy-MM-dd}";

			var hasFullStatsToday = await _context.WalletHistories
				.AnyAsync(wh => wh.UserId == userId
							&& wh.ItemCode == fullStatsItemCode
							&& !wh.IsDeleted);

			return hasFullStatsToday;
		}
		*/

		// [废弃] 移除每日首次升级限制机制 - 2025-11-10
		/*
		/// <summary>
		/// 記錄今日首次升級標記
		/// </summary>
		/// <param name="userId">用戶ID</param>
		private async Task MarkFirstLevelUpTodayAsync(int userId)
		{
			var today = _appClock.ToAppTime(_appClock.UtcNow).Date; // UTC+8
			var todayFirstLevelUpCode = $"PET-FIRST-LEVELUP-{today:yyyy-MM-dd}";

			// 再次檢查，避免重複記錄
			var exists = await _context.WalletHistories
				.AnyAsync(wh => wh.UserId == userId
							&& wh.ItemCode == todayFirstLevelUpCode
							&& !wh.IsDeleted);

			if (!exists)
			{
				var firstLevelUpHistory = new WalletHistory
				{
					UserId = userId,
					ChangeType = "Pet",
					PointsChanged = 0, // 不涉及點數變動，只是標記
					ItemCode = todayFirstLevelUpCode,
					Description = $"今日首次升級已完成",
					ChangeTime = _appClock.ToAppTime(_appClock.UtcNow),
					IsDeleted = false
				};

				_context.WalletHistories.Add(firstLevelUpHistory);
			}
		}
		*/

		/// <summary>
		/// 應用啟動時初始化所有寵物升級狀態（處理種子數據的累積經驗）
		/// 只在應用啟動時調用一次
		/// </summary>
		public async Task InitializePetLevelsOnStartupAsync()
		{
			// 獲取所有需要升級的寵物（Experience >= 所需經驗值）
			var allPets = await _context.Pets
				.Where(p => !p.IsDeleted)
				.ToListAsync();

			foreach (var pet in allPets)
			{
				bool upgraded = false;
				var requiredExp = await GetRequiredExpForLevelAsync(pet.Level + 1);

				// 循環升級直到經驗不足
				while (pet.Experience >= requiredExp && requiredExp > 0)
				{
					pet.Level++;
					pet.LevelUpTime = _appClock.UtcNow;
					pet.Experience -= requiredExp;
					upgraded = true;

					// 發放升級獎勵
					var wallet = await _context.UserWallets
						.FirstOrDefaultAsync(w => w.UserId == pet.UserId && !w.IsDeleted);

					if (wallet != null)
					{
						var pointsReward = CalculateLevelUpReward(pet.Level);
						wallet.UserPoint += pointsReward;

						// 記錄升級獎勵
						_context.WalletHistories.Add(new WalletHistory
						{
							UserId = pet.UserId,
							ChangeType = "Pet",
							PointsChanged = pointsReward,
							ItemCode = $"PET_LEVELUP_{pet.Level}",
							Description = $"[啟動初始化] 寵物升級至 Level {pet.Level}",
							ChangeTime = _appClock.ToAppTime(_appClock.UtcNow),
							IsDeleted = false
						});
					}

					requiredExp = await GetRequiredExpForLevelAsync(pet.Level + 1);
				}

				if (upgraded)
				{
					_context.Pets.Update(pet);
				}
			}

			await _context.SaveChangesAsync();
		}

		/// <summary>
		/// 獲取指定等級所需的經驗值（三級公式）
		/// </summary>
		public Task<int> GetRequiredExpForLevelAsync(int level)
		{
			if (level < 1)
			{
				return Task.FromResult(0);
			}

			if (level > 250)
			{
				// 超過250級視為最高等級
				return Task.FromResult(0);
			}

			// 三級經驗值公式
			if (level <= 10)
			{
				// Level 1-10: 線性公式
				// EXP = 40 * level + 60
				return Task.FromResult(40 * level + 60);
			}
			else if (level <= 100)
			{
				// Level 11-100: 二次公式
				// EXP = 0.8 * level^2 + 380
				return Task.FromResult((int)(0.8 * level * level + 380));
			}
			else
			{
				// Level 101+: 指數公式
				// EXP = 285.69 * (1.06 ^ level)
				return Task.FromResult((int)(285.69 * Math.Pow(1.06, level)));
			}
		}

		/// <summary>
		/// 計算升級獎勵（階層式獎勵）
		/// </summary>
		private int CalculateLevelUpReward(int level)
		{
			if (level < 1)
			{
				return 0;
			}

			if (level > 250)
			{
				return 250; // 最高獎勵250點
			}

			// 階層式獎勵：每10級一個階層，每個階層獎勵 +10 點
			// Level 1-10: +10 點
			// Level 11-20: +20 點
			// Level 21-30: +30 點
			// ...
			// Level 241-250: +250 點
			int tier = Math.Min((level - 1) / 10 + 1, 25);
			return tier * 10;
		}

		/// <summary>
		/// 獲取用戶已購買的膚色列表
		/// </summary>
		public async Task<IEnumerable<string>> GetPurchasedSkinColorsAsync(int userId)
		{
			// 查詢新格式：ChangeType="PetSkinColor"，ItemCode="{UserId}-{ColorCode}"
			var newFormatPurchases = await _context.WalletHistories
				.AsNoTracking()
				.Where(w => w.UserId == userId
					&& w.ChangeType == "PetSkinColor"
					&& !w.IsDeleted)
				.Select(w => w.ItemCode)
				.ToListAsync();

			// 提取顏色代碼（去除 "{UserId}-" 前綴）
			var colorCodesFromNewFormat = newFormatPurchases
				.Where(code => !string.IsNullOrWhiteSpace(code) && code.Contains('-'))
				.Select(code => code.Split('-', 2)[1])
				.Distinct()
				.ToList();

			// 查詢舊格式：ChangeType="Point"，ItemCode="{ColorCode}"（兼容舊版購買記錄）
			var oldFormatPurchases = await _context.WalletHistories
				.AsNoTracking()
				.Where(w => w.UserId == userId
					&& w.ChangeType == "Point"
					&& !w.IsDeleted
					&& (w.Description.Contains("購買寵物膚色") || w.Description.Contains("购买宠物肤色"))
					&& w.ItemCode != null
					&& w.ItemCode.StartsWith("#"))
				.Select(w => w.ItemCode)
				.Distinct()
				.ToListAsync();

			// 合併新舊格式的顏色代碼
			var allPurchasedColors = colorCodesFromNewFormat.Concat(oldFormatPurchases).Distinct().ToList();

			// 添加0點膚色（視為已購買）
			var freeSkins = await _context.PetSkinColorCostSettings
				.AsNoTracking()
				.Where(s => s.PointsCost == 0 && !s.IsDeleted)
				.Select(s => s.ColorCode)
				.ToListAsync();

			// 添加用戶當前套用的膚色（視為已擁有）
			var currentSkinColor = await _context.Pets
				.AsNoTracking()
				.Where(p => p.UserId == userId && !p.IsDeleted)
				.Select(p => p.SkinColor)
				.FirstOrDefaultAsync();

			var result = allPurchasedColors.Concat(freeSkins);
			if (!string.IsNullOrWhiteSpace(currentSkinColor))
			{
				result = result.Append(currentSkinColor);
			}

			return result.Distinct();
		}

		/// <summary>
		/// 獲取用戶已購買的背景列表
		/// </summary>
		public async Task<IEnumerable<string>> GetPurchasedBackgroundsAsync(int userId)
		{
			// 查詢新格式：ChangeType="PetBackground"，ItemCode="{UserId}-{BackgroundCode}"
			var newFormatPurchases = await _context.WalletHistories
				.AsNoTracking()
				.Where(w => w.UserId == userId
					&& w.ChangeType == "PetBackground"
					&& !w.IsDeleted)
				.Select(w => w.ItemCode)
				.ToListAsync();

			// 提取背景代碼（去除 "{UserId}-" 前綴）
			var backgroundCodesFromNewFormat = newFormatPurchases
				.Where(code => !string.IsNullOrWhiteSpace(code) && code.Contains('-'))
				.Select(code => code.Split('-', 2)[1])
				.Distinct()
				.ToList();

			// 查詢舊格式：ChangeType="Point"，ItemCode="{BackgroundCode}"（兼容舊版購買記錄）
			var oldFormatPurchases = await _context.WalletHistories
				.AsNoTracking()
				.Where(w => w.UserId == userId
					&& w.ChangeType == "Point"
					&& !w.IsDeleted
					&& (w.Description.Contains("購買寵物背景") || w.Description.Contains("购买宠物背景"))
					&& w.ItemCode != null
					&& w.ItemCode.StartsWith("BG"))
				.Select(w => w.ItemCode)
				.Distinct()
				.ToListAsync();

			// 合併新舊格式的背景代碼
			var allPurchasedBackgrounds = backgroundCodesFromNewFormat.Concat(oldFormatPurchases).Distinct().ToList();

			// 添加0點背景（視為已購買）
			var freeBackgrounds = await _context.PetBackgroundCostSettings
				.AsNoTracking()
				.Where(s => s.PointsCost == 0 && !s.IsDeleted)
				.Select(s => s.BackgroundCode)
				.ToListAsync();

			// 添加用戶當前套用的背景（視為已擁有）
			var currentBackground = await _context.Pets
				.AsNoTracking()
				.Where(p => p.UserId == userId && !p.IsDeleted)
				.Select(p => p.BackgroundColor)
				.FirstOrDefaultAsync();

			var result = allPurchasedBackgrounds.Concat(freeBackgrounds);
			if (!string.IsNullOrWhiteSpace(currentBackground))
			{
				result = result.Append(currentBackground);
			}

			return result.Distinct();
		}

		/// <summary>
		/// 檢查膚色是否已購買
		/// </summary>
		private async Task<bool> CheckSkinColorPurchasedAsync(int userId, string colorHex)
		{
			// 標準化為大寫，避免大小寫匹配問題
			colorHex = colorHex?.ToUpperInvariant() ?? string.Empty;

			// 檢查是否為0點膚色
			var skinSetting = await _context.PetSkinColorCostSettings
				.AsNoTracking()
				.FirstOrDefaultAsync(s => s.ColorCode == colorHex && !s.IsDeleted);

			if (skinSetting != null && skinSetting.PointsCost == 0)
			{
				return true; // 0點膚色視為已購買
			}

			// 檢查 WalletHistory 是否有購買記錄（新格式）
			var itemCode = $"{userId}-{colorHex}";
			var hasNewFormatPurchase = await _context.WalletHistories
				.AsNoTracking()
				.AnyAsync(w => w.UserId == userId
					&& w.ChangeType == "PetSkinColor"
					&& w.ItemCode == itemCode
					&& !w.IsDeleted);

			if (hasNewFormatPurchase)
			{
				return true;
			}

			// 檢查 WalletHistory 是否有購買記錄（舊格式：兼容舊版）
			var hasOldFormatPurchase = await _context.WalletHistories
				.AsNoTracking()
				.AnyAsync(w => w.UserId == userId
					&& w.ChangeType == "Point"
					&& w.ItemCode == colorHex
					&& (w.Description.Contains("購買寵物膚色") || w.Description.Contains("购买宠物肤色"))
					&& !w.IsDeleted);

			return hasOldFormatPurchase;
		}

		/// <summary>
		/// 檢查背景是否已購買
		/// </summary>
		private async Task<bool> CheckBackgroundPurchasedAsync(int userId, string backgroundCode)
		{
			// 標準化為大寫，避免大小寫匹配問題
			backgroundCode = backgroundCode?.ToUpper() ?? string.Empty;

			// 檢查是否為0點背景
			var backgroundSetting = await _context.PetBackgroundCostSettings
				.AsNoTracking()
				.FirstOrDefaultAsync(s => s.BackgroundCode == backgroundCode && !s.IsDeleted);

			if (backgroundSetting != null && backgroundSetting.PointsCost == 0)
			{
				return true; // 0點背景視為已購買
			}

			// 檢查 WalletHistory 是否有購買記錄（新格式）
			var itemCode = $"{userId}-{backgroundCode}";
			var hasNewFormatPurchase = await _context.WalletHistories
				.AsNoTracking()
				.AnyAsync(w => w.UserId == userId
					&& w.ChangeType == "PetBackground"
					&& w.ItemCode == itemCode
					&& !w.IsDeleted);

			if (hasNewFormatPurchase)
			{
				return true;
			}

			// 檢查 WalletHistory 是否有購買記錄（舊格式：兼容舊版）
			var hasOldFormatPurchase = await _context.WalletHistories
				.AsNoTracking()
				.AnyAsync(w => w.UserId == userId
					&& w.ChangeType == "Point"
					&& w.ItemCode == backgroundCode
					&& (w.Description.Contains("購買寵物背景") || w.Description.Contains("购买宠物背景"))
					&& !w.IsDeleted);

			return hasOldFormatPurchase;
		}

		/// <summary>
		/// 購買膚色（不套用）
		/// </summary>
		public async Task<PetPurchaseResult> PurchaseSkinColorAsync(int userId, string colorHex)
		{
			// 驗證膚色代碼
			if (string.IsNullOrWhiteSpace(colorHex))
			{
				return new PetPurchaseResult
				{
					Success = false,
					Message = "膚色代碼不能為空"
				};
			}

			// 標準化為大寫，避免大小寫匹配問題
			colorHex = colorHex.ToUpperInvariant();

			// 獲取膚色設置
			var skinSetting = await _context.PetSkinColorCostSettings
				.AsNoTracking()
				.FirstOrDefaultAsync(s => s.ColorCode == colorHex && !s.IsDeleted);

			if (skinSetting == null)
			{
				return new PetPurchaseResult
				{
					Success = false,
					Message = "所選膚色不存在或不可用"
				};
			}

			// 檢查是否已購買
			if (await CheckSkinColorPurchasedAsync(userId, colorHex))
			{
				return new PetPurchaseResult
				{
					Success = false,
					Message = "您已經擁有此膚色"
				};
			}

			// 如果是0點膚色，直接返回成功
			if (skinSetting.PointsCost == 0)
			{
				return new PetPurchaseResult
				{
					Success = true,
					Message = "免費膚色，無需購買",
					PointsSpent = 0,
					RemainingPoints = (await _context.UserWallets
						.AsNoTracking()
						.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted))?.UserPoint ?? 0
				};
			}

			// 獲取用戶錢包
			var wallet = await _context.UserWallets
				.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

			if (wallet == null)
			{
				return new PetPurchaseResult
				{
					Success = false,
					Message = "錢包信息不存在"
				};
			}

			// 檢查點數是否足夠
			if (wallet.UserPoint < skinSetting.PointsCost)
			{
				return new PetPurchaseResult
				{
					Success = false,
					Message = $"會員點數不足！需要{skinSetting.PointsCost}點，目前擁有{wallet.UserPoint}點"
				};
			}

			// 開啟事務
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var nowUtc8 = _appClock.ToAppTime(_appClock.UtcNow);

				// 扣除點數
				wallet.UserPoint -= skinSetting.PointsCost;

				// 創建購買記錄
				var itemCode = $"{userId}-{colorHex}";
				_context.WalletHistories.Add(new WalletHistory
				{
					UserId = userId,
					ChangeType = "PetSkinColor",
					PointsChanged = -skinSetting.PointsCost,
					ItemCode = itemCode,
					Description = $"購買寵物膚色：{skinSetting.ColorName}",
					ChangeTime = nowUtc8,
					IsDeleted = false
				});

				// 保存更改
				_context.UserWallets.Update(wallet);
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return new PetPurchaseResult
				{
					Success = true,
					Message = $"成功購買膚色：{skinSetting.ColorName}",
					PointsSpent = skinSetting.PointsCost,
					RemainingPoints = wallet.UserPoint
				};
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return new PetPurchaseResult
				{
					Success = false,
					Message = $"購買失敗：{ex.Message}"
				};
			}
		}

		/// <summary>
		/// 購買背景（不套用）
		/// </summary>
		public async Task<PetPurchaseResult> PurchaseBackgroundAsync(int userId, string backgroundCode)
		{
			// 驗證背景代碼
			if (string.IsNullOrWhiteSpace(backgroundCode))
			{
				return new PetPurchaseResult
				{
					Success = false,
					Message = "背景代碼不能為空"
				};
			}

			// 標準化為大寫，避免大小寫匹配問題
			backgroundCode = backgroundCode.ToUpper();

			// 獲取背景設置
			var backgroundSetting = await _context.PetBackgroundCostSettings
				.AsNoTracking()
				.FirstOrDefaultAsync(s => s.BackgroundCode == backgroundCode && !s.IsDeleted);

			if (backgroundSetting == null)
			{
				return new PetPurchaseResult
				{
					Success = false,
					Message = "所選背景不存在或不可用"
				};
			}

			// 檢查是否已購買
			if (await CheckBackgroundPurchasedAsync(userId, backgroundCode))
			{
				return new PetPurchaseResult
				{
					Success = false,
					Message = "您已經擁有此背景"
				};
			}

			// 如果是0點背景，直接返回成功
			if (backgroundSetting.PointsCost == 0)
			{
				return new PetPurchaseResult
				{
					Success = true,
					Message = "免費背景，無需購買",
					PointsSpent = 0,
					RemainingPoints = (await _context.UserWallets
						.AsNoTracking()
						.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted))?.UserPoint ?? 0
				};
			}

			// 獲取用戶錢包
			var wallet = await _context.UserWallets
				.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

			if (wallet == null)
			{
				return new PetPurchaseResult
				{
					Success = false,
					Message = "錢包信息不存在"
				};
			}

			// 檢查點數是否足夠
			if (wallet.UserPoint < backgroundSetting.PointsCost)
			{
				return new PetPurchaseResult
				{
					Success = false,
					Message = $"會員點數不足！需要{backgroundSetting.PointsCost}點，目前擁有{wallet.UserPoint}點"
				};
			}

			// 開啟事務
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var nowUtc8 = _appClock.ToAppTime(_appClock.UtcNow);

				// 扣除點數
				wallet.UserPoint -= backgroundSetting.PointsCost;

				// 創建購買記錄
				var itemCode = $"{userId}-{backgroundCode}";
				_context.WalletHistories.Add(new WalletHistory
				{
					UserId = userId,
					ChangeType = "PetBackground",
					PointsChanged = -backgroundSetting.PointsCost,
					ItemCode = itemCode,
					Description = $"購買寵物背景：{backgroundSetting.BackgroundName}",
					ChangeTime = nowUtc8,
					IsDeleted = false
				});

				// 保存更改
				_context.UserWallets.Update(wallet);
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return new PetPurchaseResult
				{
					Success = true,
					Message = $"成功購買背景：{backgroundSetting.BackgroundName}",
					PointsSpent = backgroundSetting.PointsCost,
					RemainingPoints = wallet.UserPoint
				};
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return new PetPurchaseResult
				{
					Success = false,
					Message = $"購買失敗：{ex.Message}"
				};
			}
		}

		/// <summary>
		/// 套用已購買的膚色
		/// </summary>
		public async Task<PetApplyResult> ApplySkinColorAsync(int userId, string colorHex)
		{
			// 驗證膚色代碼
			if (string.IsNullOrWhiteSpace(colorHex))
			{
				return new PetApplyResult
				{
					Success = false,
					Message = "膚色代碼不能為空"
				};
			}

			// 標準化為大寫，避免大小寫匹配問題
			colorHex = colorHex.ToUpperInvariant();

			// 獲取膚色設置
			var skinSetting = await _context.PetSkinColorCostSettings
				.AsNoTracking()
				.FirstOrDefaultAsync(s => s.ColorCode == colorHex && !s.IsDeleted);

			if (skinSetting == null)
			{
				return new PetApplyResult
				{
					Success = false,
					Message = "所選膚色不存在或不可用"
				};
			}

			// 檢查是否已購買
			if (!await CheckSkinColorPurchasedAsync(userId, colorHex))
			{
				return new PetApplyResult
				{
					Success = false,
					Message = "您尚未購買此膚色，請先購買"
				};
			}

			// 獲取寵物
			var pet = await _context.Pets
				.FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

			if (pet == null)
			{
				return new PetApplyResult
				{
					Success = false,
					Message = "未找到寵物信息"
				};
			}

			// 檢查是否與當前膚色相同
			if (pet.SkinColor == colorHex)
			{
				return new PetApplyResult
				{
					Success = true,
					Message = "已經是當前膚色",
					Pet = pet
				};
			}

			try
			{
				var utcNow = _appClock.UtcNow;

				// 更新膚色
				pet.SkinColor = colorHex;
				pet.SkinColorChangedTime = utcNow;

				// 保存更改
				_context.Pets.Update(pet);
				await _context.SaveChangesAsync();

				return new PetApplyResult
				{
					Success = true,
					Message = $"成功套用膚色：{skinSetting.ColorName}",
					Pet = pet
				};
			}
			catch (Exception ex)
			{
				return new PetApplyResult
				{
					Success = false,
					Message = $"套用失敗：{ex.Message}"
				};
			}
		}

		/// <summary>
		/// 套用已購買的背景
		/// </summary>
		public async Task<PetApplyResult> ApplyBackgroundAsync(int userId, string backgroundCode)
		{
			// 驗證背景代碼
			if (string.IsNullOrWhiteSpace(backgroundCode))
			{
				return new PetApplyResult
				{
					Success = false,
					Message = "背景代碼不能為空"
				};
			}

			// 標準化為大寫，避免大小寫匹配問題
			backgroundCode = backgroundCode.ToUpper();

			// 獲取背景設置
			var backgroundSetting = await _context.PetBackgroundCostSettings
				.AsNoTracking()
				.FirstOrDefaultAsync(s => s.BackgroundCode == backgroundCode && !s.IsDeleted);

			if (backgroundSetting == null)
			{
				return new PetApplyResult
				{
					Success = false,
					Message = "所選背景不存在或不可用"
				};
			}

			// 檢查是否已購買
			if (!await CheckBackgroundPurchasedAsync(userId, backgroundCode))
			{
				return new PetApplyResult
				{
					Success = false,
					Message = "您尚未購買此背景，請先購買"
				};
			}

			// 獲取寵物
			var pet = await _context.Pets
				.FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

			if (pet == null)
			{
				return new PetApplyResult
				{
					Success = false,
					Message = "未找到寵物信息"
				};
			}

			// 檢查是否與當前背景相同
			if (pet.BackgroundColor == backgroundCode)
			{
				return new PetApplyResult
				{
					Success = true,
					Message = "已經是當前背景",
					Pet = pet
				};
			}

			try
			{
				var utcNow = _appClock.UtcNow;

				// 更新背景
				pet.BackgroundColor = backgroundCode;
				pet.BackgroundColorChangedTime = utcNow;

				// 保存更改
				_context.Pets.Update(pet);
				await _context.SaveChangesAsync();

				return new PetApplyResult
				{
					Success = true,
					Message = $"成功套用背景：{backgroundSetting.BackgroundName}",
					Pet = pet
				};
			}
			catch (Exception ex)
			{
				return new PetApplyResult
				{
					Success = false,
					Message = $"套用失敗：{ex.Message}"
				};
			}
		}
	}
}
