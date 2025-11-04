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
		private readonly ILogger<GameController> _logger;

		public GameController(
			GameSpacedatabaseContext context,
			IAppCurrentUser appCurrentUser,
			IGamePlayService gamePlayService,
			ILogger<GameController> logger)
		{
			_context = context;
			_appCurrentUser = appCurrentUser;
			_gamePlayService = gamePlayService;
			_logger = logger;
		}

		/// <summary>
		/// 遊戲首頁 - 顯示難度選擇與今日剩餘次數
		/// </summary>
		public async Task<IActionResult> Index()
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

				// 傳遞資料到視圖
				ViewBag.RemainingPlays = remainingPlays;
				ViewBag.UserId = userId;

				return View();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "載入遊戲首頁時發生錯誤");
				return View();
			}
		}

		/// <summary>
		/// 選擇難度並啟動遊戲
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> SelectDifficulty(int level)
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

				// 驗證難度等級
				if (level < 1 || level > 3)
				{
					return Json(new { success = false, message = "無效的難度等級" });
				}

				// 啟動遊戲
				var (success, message, playId) = await _gamePlayService.StartGameAsync(userId, level);

				if (success)
				{
					return Json(new { success = true, message = message, playId = playId, level = level });
				}
				else
				{
					return Json(new { success = false, message = message });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "選擇難度時發生錯誤");
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
