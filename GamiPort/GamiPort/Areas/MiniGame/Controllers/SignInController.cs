using System.Threading.Tasks;
using GamiPort.Areas.MiniGame.Services;
using GamiPort.Infrastructure.Security;
using GamiPort.Infrastructure.Time;
using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.MiniGame.Controllers
{
	[Area("MiniGame")]
	public class SignInController : Controller
	{
		private readonly ISignInService _signInService;
		private readonly IAppCurrentUser _appCurrentUser;
		private readonly IAppClock _appClock;

		public SignInController(
			ISignInService signInService,
			IAppCurrentUser appCurrentUser,
			IAppClock appClock)
		{
			_signInService = signInService ?? throw new ArgumentNullException(nameof(signInService));
			_appCurrentUser = appCurrentUser ?? throw new ArgumentNullException(nameof(appCurrentUser));
			_appClock = appClock ?? throw new ArgumentNullException(nameof(appClock));
		}

		/// <summary>
		/// 簽到日曆首頁 - 顯示當月的簽到日曆與簽到狀態
		/// </summary>
		public async Task<IActionResult> Index(int? year, int? month)
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				var returnUrl = "/MiniGame/SignIn/Index";
				return Redirect($"/Login/Login/Login/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
			}

			// 取得當前使用者 ID
			int userId = _appCurrentUser.UserId;
			if (userId <= 0)
			{
				return Unauthorized();
			}

			// 預設為當前年月
			var appNow = _appClock.ToAppTime(_appClock.UtcNow);
			year ??= appNow.Year;
			month ??= appNow.Month;

			// 取得簽到狀態與月曆資料
			var status = await _signInService.GetCurrentSignInStatusAsync(userId);
			var calendar = await _signInService.GetMonthlyCalendarAsync(userId, year.Value, month.Value);

			// 準備 ViewModel
			var viewModel = new SignInIndexViewModel
			{
				Status = status,
				Calendar = calendar,
				CurrentYear = year.Value,
				CurrentMonth = month.Value
			};

			return View(viewModel);
		}

		/// <summary>
		/// 執行簽到 - POST 方法，處理簽到邏輯
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CheckIn()
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				return Unauthorized();
			}

			// 取得當前使用者 ID
			int userId = _appCurrentUser.UserId;
			if (userId <= 0)
			{
				return Unauthorized();
			}

			// 執行簽到
			var result = await _signInService.CheckInAsync(userId);

			// 若請求為 AJAX，回傳 JSON
			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				if (result.Success)
				{
					return Json(new { success = true, message = result.Message, data = result });
				}
				else
				{
					return Json(new { success = false, message = result.Message });
				}
			}

			// 重定向回簽到頁面
			var appNow = _appClock.ToAppTime(_appClock.UtcNow);
			return RedirectToAction(nameof(Index), new { year = appNow.Year, month = appNow.Month });
		}

		/// <summary>
		/// 簽到歷史 - 分頁顯示簽到記錄
		/// </summary>
		public async Task<IActionResult> History(int page = 1, int? year = null, int? month = null)
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				var returnUrl = "/MiniGame/SignIn/History";
				return Redirect($"/Login/Login/Login/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
			}

			// 取得當前使用者 ID
			int userId = _appCurrentUser.UserId;
			if (userId <= 0)
			{
				return Unauthorized();
			}

			// 分頁大小
			const int pageSize = 20;

			// 取得分頁簽到歷史
			var history = await _signInService.GetSignInHistoryAsync(userId, page, pageSize, year, month);

			// 準備 ViewModel
			var viewModel = new SignInHistoryViewModel
			{
				History = history,
				FilterYear = year,
				FilterMonth = month
			};

			return View(viewModel);
		}
	}

	/// <summary>
	/// 簽到首頁 ViewModel
	/// </summary>
	public class SignInIndexViewModel
	{
		/// <summary>簽到狀態</summary>
		public SignInStatusDto Status { get; set; } = new();

		/// <summary>月曆資料</summary>
		public SignInCalendarDto Calendar { get; set; } = new();

		/// <summary>當前年份</summary>
		public int CurrentYear { get; set; }

		/// <summary>當前月份</summary>
		public int CurrentMonth { get; set; }
	}

	/// <summary>
	/// 簽到歷史 ViewModel
	/// </summary>
	public class SignInHistoryViewModel
	{
		/// <summary>分頁歷史記錄</summary>
		public SignInHistoryPageDto History { get; set; } = new();

		/// <summary>篩選年份（可選）</summary>
		public int? FilterYear { get; set; }

		/// <summary>篩選月份（可選）</summary>
		public int? FilterMonth { get; set; }
	}
}
