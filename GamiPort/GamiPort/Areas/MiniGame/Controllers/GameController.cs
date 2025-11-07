using GamiPort.Areas.MiniGame.Helpers;
using GamiPort.Areas.MiniGame.Services;
using GamiPort.Infrastructure.Security;
using GamiPort.Models;
using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.MiniGame.Controllers
{
	[Area("MiniGame")]
	public class GameController : Controller
	{
		private readonly GameSpacedatabaseContext _context;
		private readonly IAppCurrentUser _appCurrentUser;
		private readonly IGamePlayService _gamePlayService;
		private readonly IPetService _petService;
		private readonly ILogger<GameController> _logger;

		public GameController(
			GameSpacedatabaseContext context,
			IAppCurrentUser appCurrentUser,
			IGamePlayService gamePlayService,
			IPetService petService,
			ILogger<GameController> logger)
		{
			_context = context;
			_appCurrentUser = appCurrentUser;
			_gamePlayService = gamePlayService;
			_petService = petService;
			_logger = logger;
		}

		/// <summary>
		/// 遊戲首頁 - 顯示難度選擇與今日剩餘次數
		/// </summary>
		public async Task<IActionResult> Index(int? selectedLevel = null, string? gameResult = null, int? rewardPoints = null, int? rewardExperience = null)
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				var returnUrl = "/MiniGame/Game/Index";
				return Redirect($"/Login/Login/Login/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
			}

			try
			{
				int userId = _appCurrentUser.UserId;
				if (userId == 0)
				{
					userId = (int)await _appCurrentUser.GetUserIdAsync();
				}

				if (userId == 0)
				{
					return Unauthorized();
				}

				// 獲取今日剩餘遊戲次數
				int remainingPlays = await _gamePlayService.GetUserRemainingPlaysAsync(userId);

				// 獲取用戶寵物資料（用於顯示在遊戲畫面）
				var pet = await _petService.GetUserPetAsync(userId);
				string petSkinColor = pet?.SkinColor ?? "#ff6b6b";
				string petName = pet?.PetName ?? "寵物";

				// 傳遞資料到視圖
				ViewBag.TodayRemainingPlays = remainingPlays;
				ViewBag.UserId = userId;
				ViewBag.SelectedLevel = selectedLevel ?? 0;
				ViewBag.GameResult = gameResult;
				ViewBag.RewardPoints = rewardPoints ?? 0;
				ViewBag.RewardExperience = rewardExperience ?? 0;
				ViewBag.PetSkinColor = petSkinColor;
				ViewBag.PetName = petName;

				// TODO: 獲取統計數據
				ViewBag.MonthlyWins = 0; // await _gamePlayService.GetMonthlyWinsAsync(userId);
				ViewBag.MonthlyPoints = 0; // await _gamePlayService.GetMonthlyPointsAsync(userId);
				ViewBag.WinRate = "0"; // await _gamePlayService.GetWinRateAsync(userId);

				return View();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "載入遊戲首頁時發生錯誤");
				return View();
			}
		}

		/// <summary>
		/// 選擇難度 - 重定向到 Index 並顯示所選難度
		/// </summary>
		[HttpPost]
		public IActionResult SelectDifficulty(int level)
		{
			if (User.Identity?.IsAuthenticated != true)
			{
				return Unauthorized();
			}

			// 驗證難度等級
			if (level < 1 || level > 3)
			{
				return RedirectToAction("Index");
			}

			// 重定向到 Index 並傳遞選擇的難度
			return RedirectToAction("Index", new { selectedLevel = level });
		}

		/// <summary>
		/// 開始遊戲 - 實際啟動遊戲並扣除次數
		/// 自動根據難度進程機制決定關卡等級
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> StartGame()
		{
			if (User.Identity?.IsAuthenticated != true)
			{
				return Json(new { success = false, message = "未授權的用戶" });
			}

			try
			{
				int userId = _appCurrentUser.UserId;
				if (userId == 0)
				{
					userId = (int)await _appCurrentUser.GetUserIdAsync();
				}

				if (userId == 0)
				{
					return Json(new { success = false, message = "未授權的用戶" });
				}

				// 啟動遊戲（自動計算關卡、扣除遊戲次數、創建遊戲記錄）
				var (success, message, playId, level) = await _gamePlayService.StartGameAsync(userId);

				if (success)
				{
					// 獲取剩餘次數
					int remainingPlays = await _gamePlayService.GetUserRemainingPlaysAsync(userId);

					// 根據關卡決定配置（怪物數量、速度倍率）
					int monsterCount = level switch
					{
						1 => 6,
						2 => 8,
						3 => 10,
						_ => 6
					};

					decimal speedMultiplier = level switch
					{
						1 => 1.0m,
						2 => 1.5m,
						3 => 2.0m,
						_ => 1.0m
					};

					// 儲存遊戲 ID 和關卡到 TempData 供遊戲結束時使用
					TempData["CurrentPlayId"] = playId;
					TempData["CurrentLevel"] = level;

					return Json(new {
						success = true,
						message = message,
						sessionId = playId,
						level = level,
						remainingPlays = remainingPlays,
						monsterCount = monsterCount,
						speedMultiplier = speedMultiplier
					});
				}
				else
				{
					return Json(new { success = false, message = message });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "啟動遊戲時發生錯誤");
				return Json(new { success = false, message = "啟動遊戲失敗，請稍後重試" });
			}
		}

		/// <summary>
		/// 結束遊戲並發放獎勵
		/// 參數: level (難度), result ('Win'/'Lose'/'Abort'), points (獲得點數), experience (獲得經驗值)
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> EndGame(int playId, int level, string result, int points, int experience)
		{
			if (User.Identity?.IsAuthenticated != true)
			{
				return Unauthorized();
			}

			try
			{
				int userId = _appCurrentUser.UserId;
				if (userId == 0)
				{
					userId = (int)await _appCurrentUser.GetUserIdAsync();
				}

				if (userId == 0)
				{
					return Json(new { success = false, message = "未授權的用戶" });
				}

				// 結束遊戲並發放獎勵
				var (success, message) = await _gamePlayService.EndGameAsync(
					userId,
					playId,
					level,
					result,
					experience,
					points);

				if (success)
				{
					// 獲取更新後的剩餘次數
					int remainingPlays = await _gamePlayService.GetUserRemainingPlaysAsync(userId);
					return Json(new
					{
						success = true,
						message = message,
						remainingPlays = remainingPlays,
						pointsGained = result == "Win" ? points : 0,
						expGained = result == "Win" ? experience : 0
					});
				}
				else
				{
					return Json(new { success = false, message = message });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "結束遊戲時發生錯誤 (PlayId: {PlayId})", playId);
				return Json(new { success = false, message = "結束遊戲失敗，請稍後重試" });
			}
		}

		/// <summary>
		/// 遊戲歷史 - 支持分頁和篩選
		/// </summary>
		public async Task<IActionResult> History(int? page, int? level, DateTime? startDate, DateTime? endDate)
		{
			if (User.Identity?.IsAuthenticated != true)
			{
				var returnUrl = $"/MiniGame/Game/History?page={page ?? 1}&level={level}&startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
				return Redirect($"/Login/Login/Login/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
			}

			try
			{
				int userId = _appCurrentUser.UserId;
				if (userId == 0)
				{
					userId = (int)await _appCurrentUser.GetUserIdAsync();
				}

				if (userId == 0)
				{
					return Unauthorized();
				}

				int currentPage = page ?? 1;
				int pageSize = 10;

				// 獲取遊戲歷史
				var (games, totalCount) = await _gamePlayService.GetGameHistoryAsync(
					userId,
					currentPage,
					pageSize,
					level,
					startDate,
					endDate);

				// 計算分頁資訊
				int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

				// 構建 ViewModel
				var viewModel = new GameHistoryViewModel
				{
					Games = games,
					CurrentPage = currentPage,
					PageSize = pageSize,
					TotalCount = totalCount,
					TotalPages = totalPages,
					SelectedLevel = level,
					StartDate = startDate,
					EndDate = endDate
				};

				return View(viewModel);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "載入遊戲歷史時發生錯誤");
				return View(new GameHistoryViewModel());
			}
		}
	}

	/// <summary>
	/// 遊戲歷史視圖模型
	/// </summary>
	public class GameHistoryViewModel
	{
		public List<Models.MiniGame> Games { get; set; } = new();
		public int CurrentPage { get; set; }
		public int PageSize { get; set; }
		public int TotalCount { get; set; }
		public int TotalPages { get; set; }
		public int? SelectedLevel { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
	}
}
