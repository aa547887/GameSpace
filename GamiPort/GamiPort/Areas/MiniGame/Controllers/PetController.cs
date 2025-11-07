using GamiPort.Infrastructure.Security;
using GamiPort.Infrastructure.Time;
using GamiPort.Areas.MiniGame.Helpers;
using GamiPort.Areas.MiniGame.Services;
using GamiPort.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.MiniGame.Controllers
{
	[Area("MiniGame")]
	public class PetController : Controller
	{
		private readonly GameSpacedatabaseContext _context;
		private readonly IPetService _petService;
		private readonly IAppCurrentUser _appCurrentUser;
		private readonly IAppClock _appClock;

		public PetController(
			GameSpacedatabaseContext context,
			IPetService petService,
			IAppCurrentUser appCurrentUser,
			IAppClock appClock)
		{
			_context = context;
			_petService = petService;
			_appCurrentUser = appCurrentUser;
			_appClock = appClock;
		}

		/// <summary>
		/// 寵物主頁 - 顯示寵物信息、屬性、互動選項
		/// </summary>
		public async Task<IActionResult> Index()
		{
			// 手動檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				var returnUrl = "/MiniGame/Pet/Index";
				return Redirect($"/Login/Login/Login/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
			}

			var userId = _appCurrentUser.UserId;
			if (userId <= 0)
			{
				var returnUrl = "/MiniGame/Pet/Index";
				return Redirect($"/Login/Login/Login/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
			}

			// 獲取寵物信息
			var pet = await _petService.GetUserPetAsync(userId);
			if (pet == null)
			{
				ViewBag.ErrorMessage = "未找到寵物信息，請先創建寵物";
				return View("Index");
			}

			// 模擬升級計算顯示值（不修改數據庫，只用於正確顯示進度條）
			int displayLevel = pet.Level;
			int displayExperience = pet.Experience;

			while (true)
			{
				var requiredExp = await _petService.GetRequiredExpForLevelAsync(displayLevel + 1);
				if (requiredExp > 0 && displayExperience >= requiredExp)
				{
					displayLevel++;
					displayExperience -= requiredExp;
				}
				else
				{
					break;
				}
			}

			// 計算顯示用的下一級經驗需求
			var expToNext = await _petService.GetRequiredExpForLevelAsync(displayLevel + 1);

			// 傳遞顯示值給View（不修改pet實體）
			ViewBag.DisplayLevel = displayLevel;
			ViewBag.DisplayExperience = displayExperience;
			ViewBag.DisplayExpToNext = expToNext;

			// 獲取錢包信息
			var wallet = await _context.UserWallets
				.AsNoTracking()
				.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

				ViewBag.Wallet = wallet;
			ViewBag.UserPoints = wallet?.UserPoint ?? 0;  // 添加用戶點數供View使用
			ViewBag.PetHealthStatus = GetHealthStatus(pet);

			// 獲取用戶信息（用戶名稱和註冊日期）
			var user = await _context.Users
				.Where(u => u.UserId == userId)
				.Select(u => new {
					u.UserName,
					u.CreateAccount
				})
				.AsNoTracking()
				.FirstOrDefaultAsync();

			if (user != null)
			{
				ViewBag.UserName = user.UserName;
				ViewBag.RegistrationDate = user.CreateAccount.ToUtc8String("yyyy-MM-dd");
			}

			return View(pet);
		}

		/// <summary>
		/// 寵物定制頁面 - 顯示膚色和背景選擇
		/// </summary>
		public async Task<IActionResult> Customize()
		{
			// 手動檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				var returnUrl = "/MiniGame/Pet/Customize";
				return Redirect($"/Login/Login/Login/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
			}

			var userId = _appCurrentUser.UserId;
			if (userId <= 0)
			{
				var returnUrl = "/MiniGame/Pet/Customize";
				return Redirect($"/Login/Login/Login/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
			}

			// 獲取寵物信息
			var pet = await _petService.GetUserPetAsync(userId);
			if (pet == null)
			{
				ViewBag.ErrorMessage = "未找到寵物信息";
				return View();
			}

			// 重新計算下一級所需經驗值（確保顯示正確）
			var expToNext = await _petService.GetRequiredExpForLevelAsync(pet.Level + 1);
			pet.ExperienceToNextLevel = expToNext;

			// 獲取可用的膚色和背景（所有11種，包括限時活動限定已失效的）
			var skins = await _petService.GetAvailableSkinsAsync();
			var backgrounds = await _petService.GetAvailableBackgroundsAsync();

			// 獲取已購買的膚色和背景列表
			var purchasedSkinColors = await _petService.GetPurchasedSkinColorsAsync(userId);
			var purchasedBackgrounds = await _petService.GetPurchasedBackgroundsAsync(userId);

			// 獲取錢包信息
			var wallet = await _context.UserWallets
				.AsNoTracking()
				.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

			ViewBag.Skins = skins;
			ViewBag.Backgrounds = backgrounds;
			ViewBag.PurchasedSkinColors = purchasedSkinColors;
			ViewBag.PurchasedBackgrounds = purchasedBackgrounds;
			ViewBag.Wallet = wallet;

			return View(pet);
		}

		/// <summary>
		/// POST: 執行寵物互動 (餵食/洗澡/玩耍/睡覺)
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> Interact(string action)
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			var userId = _appCurrentUser.UserId;
			if (userId <= 0)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			// 執行互動
			var result = await _petService.InteractWithPetAsync(userId, action);

			if (!result.Success)
			{
				return Json(new { success = false, message = result.Message, pet = result.Pet != null ? new
				{
					petId = result.Pet?.PetId,
					petName = result.Pet?.PetName,
					hunger = result.Pet?.Hunger,
					mood = result.Pet?.Mood,
					stamina = result.Pet?.Stamina,
					cleanliness = result.Pet?.Cleanliness,
					health = result.Pet?.Health,
					level = result.Pet?.Level,
					experience = result.Pet?.Experience,
					experienceToNextLevel = result.Pet?.ExperienceToNextLevel,
					healthStatus = GetHealthStatus(result.Pet)
				} : null });
			}

			// 返回更新後的寵物狀態和完整的互動結果
			return Json(new
			{
				success = true,
				message = result.Message,
				pet = new
				{
					petId = result.Pet?.PetId,
					petName = result.Pet?.PetName,
					hunger = result.Pet?.Hunger,
					mood = result.Pet?.Mood,
					stamina = result.Pet?.Stamina,
					cleanliness = result.Pet?.Cleanliness,
					health = result.Pet?.Health,
					level = result.Pet?.Level,
					experience = result.Pet?.Experience,
					experienceToNextLevel = result.Pet?.ExperienceToNextLevel,
					healthStatus = GetHealthStatus(result.Pet)
				},
				statChanges = result.StatChanges,
				healthRecovered = result.HealthRecovered,
				isFirstDailyFullStats = result.IsFirstDailyFullStats,
				bonusExperience = result.BonusExperience,
				bonusPoints = result.BonusPoints
			});
		}

		/// <summary>
		/// POST: 更新寵物外觀 (膚色和背景)
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> UpdateAppearance(string skinColor, string background)
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			var userId = _appCurrentUser.UserId;
			if (userId <= 0)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			// 執行外觀更新
			var result = await _petService.UpdatePetAppearanceAsync(userId, skinColor, background);

			if (!result.Success)
			{
				return Json(new { success = false, message = result.Message });
			}

			// 獲取更新後的錢包信息
			var wallet = await _context.UserWallets
				.AsNoTracking()
				.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

			return Json(new
			{
				success = true,
				message = result.Message,
				pet = new
				{
					petId = result.Pet?.PetId,
					skinColor = result.Pet?.SkinColor,
					backgroundColor = result.Pet?.BackgroundColor,
					skinColorChangedTime = result.Pet?.SkinColorChangedTime.ToUtc8String("yyyy-MM-dd HH:mm:ss"),
					backgroundColorChangedTime = result.Pet?.BackgroundColorChangedTime.ToUtc8String("yyyy-MM-dd HH:mm:ss")
				},
				wallet = new
				{
					userPoint = wallet?.UserPoint ?? 0
				}
			});
		}

		/// <summary>
		/// POST: 更新寵物名稱
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> UpdateName(string newName)
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			var userId = _appCurrentUser.UserId;
			if (userId <= 0)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			// 執行名稱更新
			var result = await _petService.UpdatePetNameAsync(userId, newName);

			if (!result.Success)
			{
				return Json(new { success = false, message = result.Message });
			}

			return Json(new
			{
				success = true,
				message = result.Message,
				pet = new
				{
					petId = result.Pet?.PetId,
					petName = result.Pet?.PetName
				}
			});
		}

		/// <summary>
		/// GET: 檢查膚色擁有狀態
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> CheckSkinColorOwnership(string colorHex)
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			var userId = _appCurrentUser.UserId;
			if (userId <= 0)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			// 獲取已購買的膚色列表
			var purchasedColors = await _petService.GetPurchasedSkinColorsAsync(userId);
			var isOwned = purchasedColors.Contains(colorHex);

			return Json(new { success = true, isOwned });
		}

		/// <summary>
		/// GET: 檢查背景擁有狀態
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> CheckBackgroundOwnership(string backgroundCode)
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			var userId = _appCurrentUser.UserId;
			if (userId <= 0)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			// 獲取已購買的背景列表
			var purchasedBackgrounds = await _petService.GetPurchasedBackgroundsAsync(userId);
			var isOwned = purchasedBackgrounds.Contains(backgroundCode);

			return Json(new { success = true, isOwned });
		}

		/// <summary>
		/// POST: 購買膚色
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> PurchaseSkinColor(string colorHex)
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			var userId = _appCurrentUser.UserId;
			if (userId <= 0)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			// 執行購買
			var result = await _petService.PurchaseSkinColorAsync(userId, colorHex);

			return Json(new
			{
				success = result.Success,
				message = result.Message,
				pointsSpent = result.PointsSpent,
				remainingPoints = result.RemainingPoints
			});
		}

		/// <summary>
		/// POST: 套用膚色
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> ApplySkinColor(string colorHex)
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			var userId = _appCurrentUser.UserId;
			if (userId <= 0)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			// 執行套用
			var result = await _petService.ApplySkinColorAsync(userId, colorHex);

			return Json(new
			{
				success = result.Success,
				message = result.Message,
				pet = result.Pet != null ? new
				{
					skinColor = result.Pet.SkinColor,
					skinColorChangedTime = result.Pet.SkinColorChangedTime.ToUtc8String("yyyy-MM-dd HH:mm:ss")
				} : null
			});
		}

		/// <summary>
		/// POST: 購買背景
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> PurchaseBackground(string backgroundCode)
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			var userId = _appCurrentUser.UserId;
			if (userId <= 0)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			// 執行購買
			var result = await _petService.PurchaseBackgroundAsync(userId, backgroundCode);

			return Json(new
			{
				success = result.Success,
				message = result.Message,
				pointsSpent = result.PointsSpent,
				remainingPoints = result.RemainingPoints
			});
		}

		/// <summary>
		/// POST: 套用背景
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> ApplyBackground(string backgroundCode)
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			var userId = _appCurrentUser.UserId;
			if (userId <= 0)
			{
				return Json(new { success = false, message = "請先登入" });
			}

			// 執行套用
			var result = await _petService.ApplyBackgroundAsync(userId, backgroundCode);

			return Json(new
			{
				success = result.Success,
				message = result.Message,
				pet = result.Pet != null ? new
				{
					backgroundColor = result.Pet.BackgroundColor,
					backgroundColorChangedTime = result.Pet.BackgroundColorChangedTime.ToUtc8String("yyyy-MM-dd HH:mm:ss")
				} : null
			});
		}

		/// <summary>
		/// 獲取寵物健康狀態描述
		/// </summary>
		private string GetHealthStatus(Pet? pet)
		{
			if (pet == null)
				return "未知";

			// 檢查是否有任何屬性為0
			if (pet.Hunger == 0 || pet.Mood == 0 || pet.Stamina == 0 ||
				pet.Cleanliness == 0 || pet.Health == 0)
			{
				return "狀態不佳（無法互動）";
			}

			// 計算平均狀態值
			var avgStat = (pet.Hunger + pet.Mood + pet.Stamina + pet.Cleanliness + pet.Health) / 5;

			if (avgStat >= 80)
				return "非常健康";
			else if (avgStat >= 60)
				return "健康";
			else if (avgStat >= 40)
				return "一般";
			else
				return "需要照顧";
		}
	}
}
