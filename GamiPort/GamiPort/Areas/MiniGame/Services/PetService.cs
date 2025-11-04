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
			if (pet.Hunger == 0 || pet.Mood == 0 || pet.Stamina == 0 ||
				pet.Cleanliness == 0 || pet.Health == 0)
			{
				return new PetInteractionResult
				{
					Success = false,
					Message = "寵物狀態不佳，無法進行互動"
				};
			}

			// 檢查用戶錢包是否有足夠點數
			var wallet = await _context.UserWallets
				.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

			if (wallet == null || wallet.UserPoint < INTERACT_POINT_COST)
			{
				return new PetInteractionResult
				{
					Success = false,
					Message = "會員點數不足，無法進行互動"
				};
			}

			// 開啟事務
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				// 根據互動類型修改寵物屬性
				var actionLower = action?.ToLower() ?? string.Empty;
				switch (actionLower)
				{
					case "feed":
						// 餵食：飢餓值增加10（最高100）
						pet.Hunger = Math.Min(pet.Hunger + 10, 100);
						break;
					case "bath":
						// 洗澡：清潔值增加10（最高100）
						pet.Cleanliness = Math.Min(pet.Cleanliness + 10, 100);
						break;
					case "play":
						// 玩耍：心情值增加10（最高100）
						pet.Mood = Math.Min(pet.Mood + 10, 100);
						break;
					case "sleep":
						// 睡覺：體力值增加10（最高100）
						pet.Stamina = Math.Min(pet.Stamina + 10, 100);
						break;
					default:
						await transaction.RollbackAsync();
						return new PetInteractionResult
						{
							Success = false,
							Message = "無效的互動類型"
						};
				}

				// 扣除會員點數
				wallet.UserPoint -= INTERACT_POINT_COST;

				// 保存更改
				_context.Pets.Update(pet);
				_context.UserWallets.Update(wallet);
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return new PetInteractionResult
				{
					Success = true,
					Message = $"互動成功！消耗{INTERACT_POINT_COST}點點數",
					Pet = pet
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
				var utcNow = _appClock.UtcNow;

				// 更新膚色
				if (!string.IsNullOrWhiteSpace(skinColor) && pet.SkinColor != skinColor)
				{
					pet.SkinColor = skinColor;
					pet.SkinColorChangedTime = utcNow;
				}

				// 更新背景
				if (!string.IsNullOrWhiteSpace(background) && pet.BackgroundColor != background)
				{
					pet.BackgroundColor = background;
					pet.BackgroundColorChangedTime = utcNow;
				}

				// 扣除點數
				wallet.UserPoint -= totalPointCost;

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
		/// 獲取可用的膚色列表
		/// </summary>
		public async Task<IEnumerable<PetSkinColorCostSetting>> GetAvailableSkinsAsync()
		{
			return await _context.PetSkinColorCostSettings
				.AsNoTracking()
				.Where(s => !s.IsDeleted && s.IsActive)
				.OrderBy(s => s.DisplayOrder)
				.ToListAsync();
		}

		/// <summary>
		/// 獲取可用的背景列表
		/// </summary>
		public async Task<IEnumerable<PetBackgroundCostSetting>> GetAvailableBackgroundsAsync()
		{
			return await _context.PetBackgroundCostSettings
				.AsNoTracking()
				.Where(s => !s.IsDeleted && s.IsActive)
				.OrderBy(s => s.DisplayOrder ?? 0)
				.ToListAsync();
		}
	}
}
