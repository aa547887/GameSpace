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

			// 獲取錢包信息
			var wallet = await _context.UserWallets
				.AsNoTracking()
				.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

				ViewBag.Wallet = wallet;
			ViewBag.UserPoints = wallet?.UserPoint ?? 0;  // 添加用戶點數供View使用
			ViewBag.PetHealthStatus = GetHealthStatus(pet);

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

			// 獲取可用的膚色和背景（所有11種，包括限時活動限定已失效的）
			var skins = await _petService.GetAvailableSkinsAsync();
			var backgrounds = await _petService.GetAvailableBackgroundsAsync();

			// 獲取錢包信息
			var wallet = await _context.UserWallets
				.AsNoTracking()
				.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

			ViewBag.Skins = skins;
			ViewBag.Backgrounds = backgrounds;
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
				return Json(new { success = false, message = result.Message });
			}

			// 返回更新後的寵物狀態
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
					healthStatus = GetHealthStatus(result.Pet)
				}
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
