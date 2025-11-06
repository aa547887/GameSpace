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
		private readonly IQRCodeService _qrCodeService;
		private readonly ILogger<WalletController> _logger;

		public WalletController(
			GameSpacedatabaseContext context,
			IWalletService walletService,
			IAppCurrentUser currentUser,
			IFuzzySearchService fuzzySearchService,
			IQRCodeService qrCodeService,
			ILogger<WalletController> logger)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
			_currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
			_fuzzySearchService = fuzzySearchService ?? throw new ArgumentNullException(nameof(fuzzySearchService));
			_qrCodeService = qrCodeService ?? throw new ArgumentNullException(nameof(qrCodeService));
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

				// 構建視圖數據（使用 ViewBag）
				ViewBag.Coupons = coupons.ToList();
				ViewBag.SearchTerm = search;
				ViewBag.TotalCount = coupons.Count();
				ViewBag.UnusedCount = coupons.Count(c => !c.IsUsed);
				ViewBag.UsedCount = coupons.Count(c => c.IsUsed);

				return View();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "優惠券列表加載失敗: UserId={UserId}", _currentUser.UserId);
				return RedirectToAction("Index");
			}
		}

		/// <summary>
		/// 電子禮券列表 - 顯示用戶所有電子禮券，支持模糊搜尋和篩選
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> EVouchers(string search = "", string filter = "all")
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

				// 根據篩選條件過濾
				var eVoucherList = eVouchers.ToList();
				if (filter == "unused")
				{
					eVoucherList = eVoucherList.Where(e => !e.IsUsed).ToList();
				}
				else if (filter == "used")
				{
					eVoucherList = eVoucherList.Where(e => e.IsUsed).ToList();
				}
				// filter == "all" 則顯示全部

				// 計算完整統計（不受篩選影響）
				var totalEVouchers = eVouchers.ToList();
				var qrCodeData = new Dictionary<int, string>();

				foreach (var voucher in eVoucherList)
				{
					// 生成 QR Code 內容（包含禮券代碼和基本信息）
					var qrContent = $"EVOUCHER:{voucher.EvoucherCode}|ID:{voucher.EvoucherId}|VALUE:{voucher.EvoucherType?.ValueAmount ?? 0}";
					var qrCodeBase64 = _qrCodeService.GenerateQRCodeBase64(qrContent, 15);

					if (!string.IsNullOrEmpty(qrCodeBase64))
					{
						qrCodeData[voucher.EvoucherId] = qrCodeBase64;
					}
				}

				// 構建視圖數據
				var viewData = new
				{
					EVouchers = eVoucherList,
					QRCodeData = qrCodeData,
					SearchTerm = search,
					Filter = filter,
					TotalCount = totalEVouchers.Count,
					UnusedCount = totalEVouchers.Count(e => !e.IsUsed),
					UsedCount = totalEVouchers.Count(e => e.IsUsed)
				};

				return View(viewData);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "電子禮券列表加載失敗: UserId={UserId}", _currentUser.UserId);
				return RedirectToAction("Index");
			}
		}

		/// <summary>
		/// 兌換頁面 - 顯示可兌換的優惠券和電子禮券類型
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> Exchange()
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				var returnUrl = "/MiniGame/Wallet/Exchange";
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

				// 獲取當前點數
				var userPoints = await _walletService.GetUserPointsAsync(userId);

				// 獲取可兌換的優惠券類型
				var couponTypes = await _walletService.GetAvailableCouponTypesAsync();

				// 獲取可兌換的電子禮券類型
				var evoucherTypes = await _walletService.GetAvailableEVoucherTypesAsync();

				// 構建視圖模型
				var viewModel = new Dictionary<string, object>
				{
					{ "UserPoints", userPoints },
					{ "CouponTypes", couponTypes },
					{ "EVoucherTypes", evoucherTypes }
				};

				return View(viewModel);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "兌換頁面加載失敗: UserId={UserId}", _currentUser.UserId);
				return RedirectToAction("Index");
			}
		}

		/// <summary>
		/// 兌換優惠券 - POST
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ExchangeForCoupon(int couponTypeId, int quantity = 1)
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				return Json(new { success = false, message = "未登入" });
			}

			try
			{
				var userId = _currentUser.UserId;
				if (userId <= 0)
				{
					return Json(new { success = false, message = "無法取得用戶ID" });
				}

				// 執行兌換
				var (success, message, couponCodes) = await _walletService.ExchangeForCouponAsync(userId, couponTypeId, quantity);

				if (success)
				{
					_logger.LogInformation("用戶 {UserId} 成功兌換優惠券: CouponTypeId={CouponTypeId}, Quantity={Quantity}",
						userId, couponTypeId, quantity);
				}

				return Json(new { success, message, couponCodes });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "兌換優惠券失敗: UserId={UserId}, CouponTypeId={CouponTypeId}",
					_currentUser.UserId, couponTypeId);
				return Json(new { success = false, message = "兌換失敗，請稍後再試" });
			}
		}

		/// <summary>
		/// 兌換電子禮券 - POST
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ExchangeForEVoucher(int evoucherTypeId, int quantity = 1)
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				return Json(new { success = false, message = "未登入" });
			}

			try
			{
				var userId = _currentUser.UserId;
				if (userId <= 0)
				{
					return Json(new { success = false, message = "無法取得用戶ID" });
				}

				// 執行兌換
				var (success, message, evoucherCodes) = await _walletService.ExchangeForEVoucherAsync(userId, evoucherTypeId, quantity);

				if (success)
				{
					_logger.LogInformation("用戶 {UserId} 成功兌換電子禮券: EVoucherTypeId={EVoucherTypeId}, Quantity={Quantity}",
						userId, evoucherTypeId, quantity);
				}

				return Json(new { success, message, evoucherCodes });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "兌換電子禮券失敗: UserId={UserId}, EVoucherTypeId={EVoucherTypeId}",
					_currentUser.UserId, evoucherTypeId);
				return Json(new { success = false, message = "兌換失敗，請稍後再試" });
			}
		}

		/// <summary>
		/// 錢包交易歷史 - 支持分頁、篩選、模糊搜尋
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> History(
			int pageNumber = 1,
			int pageSize = 20,
			string? changeType = null,
			DateTime? startDate = null,
			DateTime? endDate = null,
			string? searchTerm = null)
		{
			// 檢查登入狀態
			if (User.Identity?.IsAuthenticated != true)
			{
				var returnUrl = "/MiniGame/Wallet/History";
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

				// 驗證分頁參數
				if (pageNumber < 1) pageNumber = 1;
				if (pageSize < 10) pageSize = 10;
				if (pageSize > 200) pageSize = 200;

				// 獲取交易歷史（分頁、篩選、模糊搜尋）
				var (items, totalCount) = await _walletService.GetWalletHistoryAsync(
					userId,
					pageNumber,
					pageSize,
					changeType,
					startDate,
					endDate,
					searchTerm);

				// 計算分頁信息
				var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

				// 獲取用戶當前點數
				var currentPoints = await _walletService.GetUserPointsAsync(userId);

				// 構建視圖模型
				var viewModel = new
				{
					Items = items.ToList(),
					CurrentPoints = currentPoints,
					Pagination = new
					{
						PageNumber = pageNumber,
						PageSize = pageSize,
						TotalCount = totalCount,
						TotalPages = totalPages,
						HasPreviousPage = pageNumber > 1,
						HasNextPage = pageNumber < totalPages
					},
					Filters = new
					{
						ChangeType = changeType,
						StartDate = startDate,
						EndDate = endDate,
						SearchTerm = searchTerm
					}
				};

				return View(viewModel);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "錢包交易歷史加載失敗: UserId={UserId}", _currentUser.UserId);
				return RedirectToAction("Index");
			}
		}
	}
}
