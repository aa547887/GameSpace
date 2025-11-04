using GamiPort.Areas.MiniGame.Services;
using GamiPort.Infrastructure.Security;
using GamiPort.Models;
using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.MiniGame.Controllers
{
	[Area("MiniGame")]
	public class WalletController : Controller
	{
		private readonly GameSpacedatabaseContext _context;
		private readonly IWalletService _walletService;
		private readonly IAppCurrentUser _currentUser;
		private readonly IFuzzySearchService _fuzzySearchService;
		private readonly ILogger<WalletController> _logger;

		public WalletController(
			GameSpacedatabaseContext context,
			IWalletService walletService,
			IAppCurrentUser currentUser,
			IFuzzySearchService fuzzySearchService,
			ILogger<WalletController> logger)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
			_currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
			_fuzzySearchService = fuzzySearchService ?? throw new ArgumentNullException(nameof(fuzzySearchService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// 錢包主頁 - 顯示點數、優惠券、電子禮券概況
		/// </summary>
		public async Task<IActionResult> Index()
		{
			// 檢查登入狀態，未登入跳轉到正確的登入頁面
			if (User.Identity?.IsAuthenticated != true)
			{
				var returnUrl = "/MiniGame/Wallet/Index";
				return Redirect($"/Login/Login/Login/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
			}

			try
			{
				var userId = _currentUser.UserId;
				if (userId <= 0)
				{
					_logger.LogWarning("無法取得用戶ID");
					return RedirectToAction("Index", "Home");
				}

				// 獲取錢包信息
				var wallet = await _walletService.GetUserWalletAsync(userId);
				if (wallet == null)
				{
					_logger.LogWarning("用戶錢包不存在: UserId={UserId}", userId);
					return RedirectToAction("Index", "Home");
				}

				// 獲取統計信息
				var summary = await _walletService.GetPointsSummaryAsync(userId);
				var unusedCouponCount = await _walletService.GetUnusedCouponCountAsync(userId);
				var unusedEVoucherCount = await _walletService.GetUnusedEVoucherCountAsync(userId);

				// 構建視圖模型
				var viewModel = new Dictionary<string, object>
				{
					{ "CurrentPoints", wallet.UserPoint },
					{ "TotalEarned", summary.ContainsKey("TotalEarned") ? summary["TotalEarned"] : 0 },
					{ "TotalSpent", summary.ContainsKey("TotalSpent") ? summary["TotalSpent"] : 0 },
					{ "TransactionCount", summary.ContainsKey("TransactionCount") ? summary["TransactionCount"] : 0 },
					{ "UnusedCouponCount", unusedCouponCount },
					{ "UnusedEVoucherCount", unusedEVoucherCount }
				};

				return View(viewModel);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "錢包主頁加載失敗");
				return RedirectToAction("Index", "Home");
			}
		}

		/// <summary>
		/// 優惠券列表 - 顯示用戶所有優惠券，支持模糊搜尋
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> Coupons(string search = "")
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				var returnUrl = "/MiniGame/Wallet/Coupons";
				return Redirect($"/Login/Login/Login/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
			}

			try
			{
				var userId = _currentUser.UserId;
				if (userId <= 0)
				{
					_logger.LogWarning("無法取得用戶ID");
					return RedirectToAction("Index");
				}

				// 獲取優惠券列表（支持模糊搜尋）
				var coupons = await _walletService.GetUserCouponsAsync(userId, search);

				// 構建視圖數據
				var viewData = new
				{
					Coupons = coupons,
					SearchTerm = search,
					TotalCount = coupons.Count(),
					UnusedCount = coupons.Count(c => !c.IsUsed),
					UsedCount = coupons.Count(c => c.IsUsed)
				};

				return View(viewData);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "優惠券列表加載失敗: UserId={UserId}", _currentUser.UserId);
				return RedirectToAction("Index");
			}
		}

		/// <summary>
		/// 電子禮券列表 - 顯示用戶所有電子禮券，支持模糊搜尋
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> EVouchers(string search = "")
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				var returnUrl = "/MiniGame/Wallet/EVouchers";
				return Redirect($"/Login/Login/Login/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
			}

			try
			{
				var userId = _currentUser.UserId;
				if (userId <= 0)
				{
					_logger.LogWarning("無法取得用戶ID");
					return RedirectToAction("Index");
				}

				// 獲取電子禮券列表（支持模糊搜尋）
				var eVouchers = await _walletService.GetUserEVouchersAsync(userId, search);

				// 構建視圖數據
				var viewData = new
				{
					EVouchers = eVouchers,
					SearchTerm = search,
					TotalCount = eVouchers.Count(),
					UnusedCount = eVouchers.Count(e => !e.IsUsed),
					UsedCount = eVouchers.Count(e => e.IsUsed)
				};

				return View(viewData);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "電子禮券列表加載失敗: UserId={UserId}", _currentUser.UserId);
				return RedirectToAction("Index");
			}
		}
	}
}
