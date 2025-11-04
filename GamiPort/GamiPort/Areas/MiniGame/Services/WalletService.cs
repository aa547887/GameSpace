using GamiPort.Models;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.MiniGame.Services
{
	/// <summary>
	/// 前台錢包服務實現
	/// 提供會員點數、優惠券、電子禮券相關查詢功能
	/// </summary>
	public class WalletService : IWalletService
	{
		private readonly GameSpacedatabaseContext _context;
		private readonly IFuzzySearchService _fuzzySearchService;
		private readonly ILogger<WalletService> _logger;

		public WalletService(
			GameSpacedatabaseContext context,
			IFuzzySearchService fuzzySearchService,
			ILogger<WalletService> logger)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_fuzzySearchService = fuzzySearchService ?? throw new ArgumentNullException(nameof(fuzzySearchService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// 獲取用戶錢包信息
		/// </summary>
		public async Task<UserWallet?> GetUserWalletAsync(int userId)
		{
			try
			{
				return await _context.UserWallets
					.AsNoTracking()
					.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "獲取用戶錢包信息失敗: UserId={UserId}", userId);
				return null;
			}
		}

		/// <summary>
		/// 獲取用戶點數
		/// </summary>
		public async Task<int> GetUserPointsAsync(int userId)
		{
			try
			{
				var wallet = await GetUserWalletAsync(userId);
				return wallet?.UserPoint ?? 0;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "獲取用戶點數失敗: UserId={UserId}", userId);
				return 0;
			}
		}

		/// <summary>
		/// 獲取用戶優惠券列表（支持模糊搜尋）
		/// 使用 5 級優先級匹配: 完全匹配 > 開頭匹配 > 包含匹配 > 模糊匹配 > 分詞匹配
		/// </summary>
		public async Task<IEnumerable<Coupon>> GetUserCouponsAsync(int userId, string searchTerm = "")
		{
			try
			{
				var coupons = await _context.Coupons
					.AsNoTracking()
					.Where(c => c.UserId == userId && !c.IsDeleted)
					.ToListAsync();

				// 應用模糊搜尋 (OR 邏輯: 優惠券代碼 OR 優惠券類型名稱)
				if (!string.IsNullOrWhiteSpace(searchTerm))
				{
					coupons = coupons
						.Where(c =>
							string.IsNullOrWhiteSpace(searchTerm) ||
							_fuzzySearchService.IsMatch(searchTerm, c.CouponCode, c.CouponType?.Name)
						)
						.ToList();

					// 按模糊搜尋優先順序排序
					coupons = coupons
						.OrderBy(c => _fuzzySearchService.CalculateMatchPriority(searchTerm, c.CouponCode, c.CouponType?.Name))
						.ThenByDescending(c => c.AcquiredTime)
						.ToList();
				}
				else
				{
					// 無搜尋條件時，按取得時間降序排列
					coupons = coupons
						.OrderByDescending(c => c.AcquiredTime)
						.ToList();
				}

				return coupons;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "獲取用戶優惠券列表失敗: UserId={UserId}, SearchTerm={SearchTerm}", userId, searchTerm);
				return Enumerable.Empty<Coupon>();
			}
		}

		/// <summary>
		/// 獲取用戶電子禮券列表（支持模糊搜尋）
		/// 使用 5 級優先級匹配: 完全匹配 > 開頭匹配 > 包含匹配 > 模糊匹配 > 分詞匹配
		/// </summary>
		public async Task<IEnumerable<Evoucher>> GetUserEVouchersAsync(int userId, string searchTerm = "")
		{
			try
			{
				var evouchers = await _context.Evouchers
					.AsNoTracking()
					.Where(e => e.UserId == userId && !e.IsDeleted)
					.ToListAsync();

				// 應用模糊搜尋 (OR 邏輯: 電子禮券代碼 OR 電子禮券類型名稱)
				if (!string.IsNullOrWhiteSpace(searchTerm))
				{
					evouchers = evouchers
						.Where(e =>
							string.IsNullOrWhiteSpace(searchTerm) ||
							_fuzzySearchService.IsMatch(searchTerm, e.EvoucherCode, e.EvoucherType?.Name)
						)
						.ToList();

					// 按模糊搜尋優先順序排序
					evouchers = evouchers
						.OrderBy(e => _fuzzySearchService.CalculateMatchPriority(searchTerm, e.EvoucherCode, e.EvoucherType?.Name))
						.ThenByDescending(e => e.AcquiredTime)
						.ToList();
				}
				else
				{
					// 無搜尋條件時，按取得時間降序排列
					evouchers = evouchers
						.OrderByDescending(e => e.AcquiredTime)
						.ToList();
				}

				return evouchers;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "獲取用戶電子禮券列表失敗: UserId={UserId}, SearchTerm={SearchTerm}", userId, searchTerm);
				return Enumerable.Empty<Evoucher>();
			}
		}

		/// <summary>
		/// 獲取用戶未使用的優惠券數量
		/// </summary>
		public async Task<int> GetUnusedCouponCountAsync(int userId)
		{
			try
			{
				return await _context.Coupons
					.AsNoTracking()
					.CountAsync(c => c.UserId == userId && !c.IsUsed && !c.IsDeleted);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "獲取未使用優惠券數量失敗: UserId={UserId}", userId);
				return 0;
			}
		}

		/// <summary>
		/// 獲取用戶未使用的電子禮券數量
		/// </summary>
		public async Task<int> GetUnusedEVoucherCountAsync(int userId)
		{
			try
			{
				return await _context.Evouchers
					.AsNoTracking()
					.CountAsync(e => e.UserId == userId && !e.IsUsed && !e.IsDeleted);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "獲取未使用電子禮券數量失敗: UserId={UserId}", userId);
				return 0;
			}
		}

		/// <summary>
		/// 獲取錢包交易記錄
		/// </summary>
		public async Task<IEnumerable<WalletHistory>> GetWalletHistoryAsync(int userId, int pageSize = 10)
		{
			try
			{
				return await _context.WalletHistories
					.AsNoTracking()
					.Where(h => h.UserId == userId && !h.IsDeleted)
					.OrderByDescending(h => h.ChangeTime)
					.Take(pageSize)
					.ToListAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "獲取錢包交易記錄失敗: UserId={UserId}", userId);
				return Enumerable.Empty<WalletHistory>();
			}
		}

		/// <summary>
		/// 獲取錢包交易統計
		/// </summary>
		public async Task<Dictionary<string, int>> GetPointsSummaryAsync(int userId)
		{
			try
			{
				var wallet = await GetUserWalletAsync(userId);
				var history = await _context.WalletHistories
					.AsNoTracking()
					.Where(h => h.UserId == userId && !h.IsDeleted)
					.ToListAsync();

				var totalEarned = history
					.Where(h => h.PointsChanged > 0)
					.Sum(h => h.PointsChanged);

				var totalSpent = history
					.Where(h => h.PointsChanged < 0)
					.Sum(h => Math.Abs(h.PointsChanged));

				return new Dictionary<string, int>
				{
					{ "CurrentPoints", wallet?.UserPoint ?? 0 },
					{ "TotalEarned", totalEarned },
					{ "TotalSpent", totalSpent },
					{ "TransactionCount", history.Count }
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "獲取錢包交易統計失敗: UserId={UserId}", userId);
				return new Dictionary<string, int>
				{
					{ "CurrentPoints", 0 },
					{ "TotalEarned", 0 },
					{ "TotalSpent", 0 },
					{ "TransactionCount", 0 }
				};
			}
		}
	}
}
