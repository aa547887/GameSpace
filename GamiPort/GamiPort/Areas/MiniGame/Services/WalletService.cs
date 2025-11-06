using GamiPort.Infrastructure.Time;
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
		private readonly IAppClock _appClock;

		public WalletService(
			GameSpacedatabaseContext context,
			IFuzzySearchService fuzzySearchService,
			ILogger<WalletService> logger,
			IAppClock appClock)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_fuzzySearchService = fuzzySearchService ?? throw new ArgumentNullException(nameof(fuzzySearchService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_appClock = appClock ?? throw new ArgumentNullException(nameof(appClock));
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
					.Include(c => c.CouponType)
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
					.Include(e => e.EvoucherType)
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

		/// <summary>
		/// 使用優惠券
		/// 5層驗證: 存在性 > 重複使用 > 所有權 > 有效期 > 訂單ID
		/// </summary>
		public async Task<bool> UseCouponAsync(int couponId, int userId, int? orderId = null)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				// 1. 查詢優惠券（包含類型信息）
				var coupon = await _context.Coupons
					.Include(c => c.CouponType)
					.FirstOrDefaultAsync(c => c.CouponId == couponId && !c.IsDeleted);

				// 2. 驗證存在性
				if (coupon == null)
				{
					_logger.LogWarning("優惠券不存在: CouponId={CouponId}", couponId);
					await transaction.RollbackAsync();
					return false;
				}

				// 3. 驗證是否已使用
				if (coupon.IsUsed)
				{
					_logger.LogWarning("優惠券已使用: CouponId={CouponId}, UsedTime={UsedTime}", couponId, coupon.UsedTime);
					await transaction.RollbackAsync();
					return false;
				}

				// 4. 驗證所有權
				if (coupon.UserId != userId)
				{
					_logger.LogWarning("優惠券不屬於該用戶: CouponId={CouponId}, UserId={UserId}, CouponUserId={CouponUserId}",
						couponId, userId, coupon.UserId);
					await transaction.RollbackAsync();
					return false;
				}

				// 5. 驗證有效期（UTC+8 台灣時間）
				if (coupon.CouponType != null)
				{
					var nowUtc = _appClock.UtcNow;
					var now = _appClock.ToAppTime(nowUtc);

					if (now < coupon.CouponType.ValidFrom)
					{
						_logger.LogWarning("優惠券尚未生效: CouponId={CouponId}, ValidFrom={ValidFrom}, Now={Now}",
							couponId, coupon.CouponType.ValidFrom, now);
						await transaction.RollbackAsync();
						return false;
					}

					if (now > coupon.CouponType.ValidTo)
					{
						_logger.LogWarning("優惠券已過期: CouponId={CouponId}, ValidTo={ValidTo}, Now={Now}",
							couponId, coupon.CouponType.ValidTo, now);
						await transaction.RollbackAsync();
						return false;
					}
				}

				// 6. 標記為已使用
				var usedTime = _appClock.ToAppTime(_appClock.UtcNow);
				coupon.IsUsed = true;
				coupon.UsedTime = usedTime;
				if (orderId.HasValue)
				{
					coupon.UsedInOrderId = orderId.Value;
				}

				_context.Coupons.Update(coupon);
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				_logger.LogInformation("優惠券使用成功: CouponId={CouponId}, UserId={UserId}, OrderId={OrderId}",
					couponId, userId, orderId);
				return true;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "使用優惠券失敗: CouponId={CouponId}, UserId={UserId}", couponId, userId);
				return false;
			}
		}

		/// <summary>
		/// 兌換電子禮券
		/// 驗證: 存在性 > 重複使用 > 所有權 > 有效期
		/// 創建兌換記錄（EvoucherRedeemLog）
		/// </summary>
		public async Task<bool> RedeemEVoucherAsync(int evoucherId, int userId)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				// 1. 查詢電子禮券（包含類型信息）
				var evoucher = await _context.Evouchers
					.Include(e => e.EvoucherType)
					.FirstOrDefaultAsync(e => e.EvoucherId == evoucherId && !e.IsDeleted);

				// 2. 驗證存在性
				if (evoucher == null)
				{
					_logger.LogWarning("電子禮券不存在: EvoucherId={EvoucherId}", evoucherId);
					await transaction.RollbackAsync();
					return false;
				}

				// 3. 驗證是否已使用
				if (evoucher.IsUsed)
				{
					_logger.LogWarning("電子禮券已使用: EvoucherId={EvoucherId}, UsedTime={UsedTime}",
						evoucherId, evoucher.UsedTime);
					await transaction.RollbackAsync();
					return false;
				}

				// 4. 驗證所有權
				if (evoucher.UserId != userId)
				{
					_logger.LogWarning("電子禮券不屬於該用戶: EvoucherId={EvoucherId}, UserId={UserId}, EvoucherUserId={EvoucherUserId}",
						evoucherId, userId, evoucher.UserId);
					await transaction.RollbackAsync();
					return false;
				}

				// 5. 驗證有效期（UTC+8 台灣時間）
				if (evoucher.EvoucherType != null)
				{
					var nowUtc = _appClock.UtcNow;
					var now = _appClock.ToAppTime(nowUtc);

					if (now < evoucher.EvoucherType.ValidFrom)
					{
						_logger.LogWarning("電子禮券尚未生效: EvoucherId={EvoucherId}, ValidFrom={ValidFrom}, Now={Now}",
							evoucherId, evoucher.EvoucherType.ValidFrom, now);
						await transaction.RollbackAsync();
						return false;
					}

					if (now > evoucher.EvoucherType.ValidTo)
					{
						_logger.LogWarning("電子禮券已過期: EvoucherId={EvoucherId}, ValidTo={ValidTo}, Now={Now}",
							evoucherId, evoucher.EvoucherType.ValidTo, now);
						await transaction.RollbackAsync();
						return false;
					}
				}

				// 6. 標記為已使用
				var usedTime = _appClock.ToAppTime(_appClock.UtcNow);
				evoucher.IsUsed = true;
				evoucher.UsedTime = usedTime;

				// 7. 創建兌換記錄
				var redeemLog = new EvoucherRedeemLog
				{
					EvoucherId = evoucherId,
					UserId = userId,
					ScannedAt = usedTime,
					Status = "Redeemed",
					IsDeleted = false
				};

				_context.Evouchers.Update(evoucher);
				_context.EvoucherRedeemLogs.Add(redeemLog);
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				_logger.LogInformation("電子禮券兌換成功: EvoucherId={EvoucherId}, UserId={UserId}",
					evoucherId, userId);
				return true;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "兌換電子禮券失敗: EvoucherId={EvoucherId}, UserId={UserId}", evoucherId, userId);
				return false;
			}
		}

		/// <summary>
		/// 獲取錢包交易歷史（分頁、篩選、模糊搜尋）
		/// 支援 5 級優先級匹配和 OR 邏輯
		/// </summary>
		public async Task<(IEnumerable<WalletHistory> items, int totalCount)> GetWalletHistoryAsync(
			int userId,
			int pageNumber,
			int pageSize,
			string? changeType = null,
			DateTime? startDate = null,
			DateTime? endDate = null,
			string? searchTerm = null)
		{
			try
			{
				// 確保頁碼和頁面大小有效
				pageNumber = Math.Max(1, pageNumber);
				pageSize = Math.Clamp(pageSize, 10, 200);

				// 建立基礎查詢
				var query = _context.WalletHistories
					.AsNoTracking()
					.Where(h => h.UserId == userId && !h.IsDeleted);

				// 篩選交易類型（支持多重分類）
				if (!string.IsNullOrWhiteSpace(changeType))
				{
					if (changeType == "Point")
					{
						// 點數篩選：包含點數交易 + 兌換優惠券/電子禮券（都涉及點數扣除）
						query = query.Where(h =>
							h.ChangeType == "Point" ||
							(h.Description != null && (h.Description.Contains("兌換優惠券") || h.Description.Contains("兌換商城優惠券") || h.Description.Contains("兌換電子禮券") || h.Description.Contains("兌換禮券")))
						);
					}
					else if (changeType == "Coupon")
					{
						// 優惠券篩選：包含優惠券交易 + 兌換優惠券
						query = query.Where(h =>
							h.ChangeType == "Coupon" ||
							(h.Description != null && (h.Description.Contains("兌換優惠券") || h.Description.Contains("兌換商城優惠券")))
						);
					}
					else if (changeType == "EVoucher")
					{
						// 電子禮券篩選：包含電子禮券交易 + 兌換電子禮券
						query = query.Where(h =>
							h.ChangeType == "EVoucher" ||
							(h.Description != null && (h.Description.Contains("兌換電子禮券") || h.Description.Contains("兌換禮券")))
						);
					}
					else
					{
						// 其他類型：精確匹配
						query = query.Where(h => h.ChangeType == changeType);
					}
				}

				// 篩選日期範圍（轉換為 UTC）
				if (startDate.HasValue)
				{
					var utcStart = _appClock.ToUtc(startDate.Value);
					query = query.Where(h => h.ChangeTime >= utcStart);
				}

				if (endDate.HasValue)
				{
					var endDateUtc8 = endDate.Value.AddDays(1).AddTicks(-1);
					var utcEnd = _appClock.ToUtc(endDateUtc8);
					query = query.Where(h => h.ChangeTime <= utcEnd);
				}

				// 取得所有結果（用於模糊搜尋和排序）
				var allResults = await query.ToListAsync();

				// 應用模糊搜尋 (OR 邏輯: Description OR ItemCode)
				if (!string.IsNullOrWhiteSpace(searchTerm))
				{
					allResults = allResults
						.Where(h =>
							_fuzzySearchService.IsMatch(searchTerm, h.Description, h.ItemCode)
						)
						.ToList();

					// 按模糊搜尋優先順序排序
					allResults = allResults
						.OrderBy(h => _fuzzySearchService.CalculateMatchPriority(searchTerm, h.Description, h.ItemCode))
						.ThenByDescending(h => h.ChangeTime)
						.ToList();
				}
				else
				{
					// 無搜尋條件時，按交易時間降序排列
					allResults = allResults
						.OrderByDescending(h => h.ChangeTime)
						.ToList();
				}

				// 計算總筆數
				int totalCount = allResults.Count;

				// 分頁
				var items = allResults
					.Skip((pageNumber - 1) * pageSize)
					.Take(pageSize)
					.ToList();

				return (items, totalCount);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "獲取錢包交易歷史失敗: UserId={UserId}, SearchTerm={SearchTerm}",
					userId, searchTerm);
				return (Enumerable.Empty<WalletHistory>(), 0);
			}
		}

		/// <summary>
		/// 使用點數兌換優惠券
		/// </summary>
		public async Task<(bool success, string message, List<string> couponCodes)> ExchangeForCouponAsync(
			int userId,
			int couponTypeId,
			int quantity = 1)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				// 1. 驗證數量
				if (quantity <= 0 || quantity > 100)
				{
					return (false, "兌換數量必須在 1-100 之間", new List<string>());
				}

				// 2. 查詢優惠券類型
				var couponType = await _context.CouponTypes
					.AsNoTracking()
					.FirstOrDefaultAsync(ct => ct.CouponTypeId == couponTypeId && !ct.IsDeleted);

				if (couponType == null)
				{
					_logger.LogWarning("優惠券類型不存在: CouponTypeId={CouponTypeId}", couponTypeId);
					await transaction.RollbackAsync();
					return (false, "優惠券類型不存在", new List<string>());
				}

				// 3. 檢查有效期
				var nowUtc8 = _appClock.ToAppTime(_appClock.UtcNow);
				if (couponType.ValidTo < nowUtc8)
				{
					await transaction.RollbackAsync();
					return (false, $"優惠券類型「{couponType.Name}」已過期", new List<string>());
				}

				// 4. 計算所需點數
				int totalPointsRequired = couponType.PointsCost * quantity;

				// 5. 查詢用戶錢包
				var wallet = await _context.UserWallets
					.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

				if (wallet == null)
				{
					_logger.LogWarning("用戶錢包不存在: UserId={UserId}", userId);
					await transaction.RollbackAsync();
					return (false, "錢包不存在", new List<string>());
				}

				// 6. 檢查點數是否足夠
				if (wallet.UserPoint < totalPointsRequired)
				{
					await transaction.RollbackAsync();
					return (false, $"點數不足，需要 {totalPointsRequired} 點，目前只有 {wallet.UserPoint} 點", new List<string>());
				}

				// 7. 扣除點數
				wallet.UserPoint -= totalPointsRequired;

				// 8. 生成優惠券
				var generatedCodes = new List<string>();
				for (int i = 0; i < quantity; i++)
				{
					var couponCode = await GenerateUniqueCouponCodeAsync();
					var coupon = new Coupon
					{
						CouponCode = couponCode,
						CouponTypeId = couponTypeId,
						UserId = userId,
						IsUsed = false,
						AcquiredTime = nowUtc8,
						UsedTime = null,
						UsedInOrderId = null,
						IsDeleted = false
					};
					_context.Coupons.Add(coupon);
					generatedCodes.Add(couponCode);
				}

				// 9. 記錄錢包歷史
				var history = new WalletHistory
				{
					UserId = userId,
					ChangeType = "Point",
					PointsChanged = -totalPointsRequired,
					ItemCode = string.Join(", ", generatedCodes),
					Description = $"兌換優惠券「{couponType.Name}」x{quantity}",
					ChangeTime = nowUtc8,
					IsDeleted = false
				};
				_context.WalletHistories.Add(history);

				// 10. 提交事務
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				_logger.LogInformation(
					"成功兌換優惠券: UserId={UserId}, CouponTypeId={CouponTypeId}, Quantity={Quantity}, PointsUsed={PointsUsed}",
					userId, couponTypeId, quantity, totalPointsRequired);

				return (true, $"成功兌換 {quantity} 張優惠券「{couponType.Name}」", generatedCodes);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "兌換優惠券失敗: UserId={UserId}, CouponTypeId={CouponTypeId}",
					userId, couponTypeId);
				return (false, "兌換失敗，請稍後再試", new List<string>());
			}
		}

		/// <summary>
		/// 使用點數兌換電子禮券
		/// </summary>
		public async Task<(bool success, string message, List<string> evoucherCodes)> ExchangeForEVoucherAsync(
			int userId,
			int evoucherTypeId,
			int quantity = 1)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				// 1. 驗證數量
				if (quantity <= 0 || quantity > 100)
				{
					return (false, "兌換數量必須在 1-100 之間", new List<string>());
				}

				// 2. 查詢電子禮券類型
				var evoucherType = await _context.EvoucherTypes
					.AsNoTracking()
					.FirstOrDefaultAsync(et => et.EvoucherTypeId == evoucherTypeId && !et.IsDeleted);

				if (evoucherType == null)
				{
					_logger.LogWarning("電子禮券類型不存在: EVoucherTypeId={EVoucherTypeId}", evoucherTypeId);
					await transaction.RollbackAsync();
					return (false, "電子禮券類型不存在", new List<string>());
				}

				// 3. 檢查庫存
				if (evoucherType.TotalAvailable < quantity)
				{
					await transaction.RollbackAsync();
					return (false, $"電子禮券「{evoucherType.Name}」庫存不足，剩餘 {evoucherType.TotalAvailable} 張", new List<string>());
				}

				// 4. 檢查有效期
				var nowUtc8 = _appClock.ToAppTime(_appClock.UtcNow);
				if (evoucherType.ValidTo < nowUtc8)
				{
					await transaction.RollbackAsync();
					return (false, $"電子禮券類型「{evoucherType.Name}」已過期", new List<string>());
				}

				// 5. 計算所需點數
				int totalPointsRequired = evoucherType.PointsCost * quantity;

				// 6. 查詢用戶錢包
				var wallet = await _context.UserWallets
					.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

				if (wallet == null)
				{
					_logger.LogWarning("用戶錢包不存在: UserId={UserId}", userId);
					await transaction.RollbackAsync();
					return (false, "錢包不存在", new List<string>());
				}

				// 7. 檢查點數是否足夠
				if (wallet.UserPoint < totalPointsRequired)
				{
					await transaction.RollbackAsync();
					return (false, $"點數不足，需要 {totalPointsRequired} 點，目前只有 {wallet.UserPoint} 點", new List<string>());
				}

				// 8. 扣除點數
				wallet.UserPoint -= totalPointsRequired;

				// 9. 扣除庫存（需要重新載入以更新）
				var evoucherTypeToUpdate = await _context.EvoucherTypes
					.FirstOrDefaultAsync(et => et.EvoucherTypeId == evoucherTypeId);
				if (evoucherTypeToUpdate != null)
				{
					evoucherTypeToUpdate.TotalAvailable -= quantity;
				}

				// 10. 生成電子禮券
				var generatedCodes = new List<string>();
				for (int i = 0; i < quantity; i++)
				{
					var evoucherCode = await GenerateUniqueEVoucherCodeAsync();
					var evoucher = new Evoucher
					{
						EvoucherCode = evoucherCode,
						EvoucherTypeId = evoucherTypeId,
						UserId = userId,
						IsUsed = false,
						AcquiredTime = nowUtc8,
						UsedTime = null,
						IsDeleted = false
					};
					_context.Evouchers.Add(evoucher);
					generatedCodes.Add(evoucherCode);
				}

				// 11. 記錄錢包歷史
				var history = new WalletHistory
				{
					UserId = userId,
					ChangeType = "Point",
					PointsChanged = -totalPointsRequired,
					ItemCode = string.Join(", ", generatedCodes),
					Description = $"兌換電子禮券「{evoucherType.Name}」x{quantity}",
					ChangeTime = nowUtc8,
					IsDeleted = false
				};
				_context.WalletHistories.Add(history);

				// 12. 提交事務
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				_logger.LogInformation(
					"成功兌換電子禮券: UserId={UserId}, EVoucherTypeId={EVoucherTypeId}, Quantity={Quantity}, PointsUsed={PointsUsed}",
					userId, evoucherTypeId, quantity, totalPointsRequired);

				return (true, $"成功兌換 {quantity} 張電子禮券「{evoucherType.Name}」", generatedCodes);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "兌換電子禮券失敗: UserId={UserId}, EVoucherTypeId={EVoucherTypeId}",
					userId, evoucherTypeId);
				return (false, "兌換失敗，請稍後再試", new List<string>());
			}
		}

		/// <summary>
		/// 獲取所有可兌換的優惠券類型
		/// </summary>
		public async Task<IEnumerable<CouponType>> GetAvailableCouponTypesAsync()
		{
			try
			{
				var nowUtc8 = _appClock.ToAppTime(_appClock.UtcNow);
				return await _context.CouponTypes
					.AsNoTracking()
					.Where(ct => !ct.IsDeleted && ct.ValidTo >= nowUtc8)
					.OrderBy(ct => ct.PointsCost)
					.ToListAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "獲取可兌換優惠券類型失敗");
				return Enumerable.Empty<CouponType>();
			}
		}

		/// <summary>
		/// 獲取所有可兌換的電子禮券類型
		/// </summary>
		public async Task<IEnumerable<EvoucherType>> GetAvailableEVoucherTypesAsync()
		{
			try
			{
				var nowUtc8 = _appClock.ToAppTime(_appClock.UtcNow);
				return await _context.EvoucherTypes
					.AsNoTracking()
					.Where(et => !et.IsDeleted && et.ValidTo >= nowUtc8 && et.TotalAvailable > 0)
					.OrderBy(et => et.PointsCost)
					.ToListAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "獲取可兌換電子禮券類型失敗");
				return Enumerable.Empty<EvoucherType>();
			}
		}

		/// <summary>
		/// 生成唯一的優惠券代碼 (GUID-based with retry mechanism)
		/// 格式: CPN-YYYYMM-{GUID前8碼}
		/// </summary>
		private async Task<string> GenerateUniqueCouponCodeAsync()
		{
			int maxRetries = 10;
			for (int i = 0; i < maxRetries; i++)
			{
				var now = _appClock.ToAppTime(_appClock.UtcNow);
				var yearMonth = now.ToString("yyyyMM");
				var uniqueId = Guid.NewGuid().ToString("N")[..8].ToUpper();
				var code = $"CPN-{yearMonth}-{uniqueId}";

				// Check if code already exists in database
				bool exists = await _context.Coupons
					.AsNoTracking()
					.AnyAsync(c => c.CouponCode == code);

				if (!exists)
				{
					return code;
				}

				_logger.LogWarning("優惠券代碼衝突，重試中: Code={Code}, Attempt={Attempt}", code, i + 1);
			}

			// Fallback: use full timestamp + GUID if all retries failed
			var fallbackCode = $"CPN-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..30];
			_logger.LogError("優惠券代碼生成失敗，使用 Fallback: Code={Code}", fallbackCode);
			return fallbackCode;
		}

		/// <summary>
		/// 生成唯一的電子禮券代碼 (GUID-based with retry mechanism)
		/// 格式: EV-YYYYMM-{GUID前8碼}
		/// </summary>
		private async Task<string> GenerateUniqueEVoucherCodeAsync()
		{
			int maxRetries = 10;
			for (int i = 0; i < maxRetries; i++)
			{
				var now = _appClock.ToAppTime(_appClock.UtcNow);
				var yearMonth = now.ToString("yyyyMM");
				var uniqueId = Guid.NewGuid().ToString("N")[..8].ToUpper();
				var code = $"EV-{yearMonth}-{uniqueId}";

				// Check if code already exists in database
				bool exists = await _context.Evouchers
					.AsNoTracking()
					.AnyAsync(e => e.EvoucherCode == code);

				if (!exists)
				{
					return code;
				}

				_logger.LogWarning("電子禮券代碼衝突，重試中: Code={Code}, Attempt={Attempt}", code, i + 1);
			}

			// Fallback: use full timestamp + GUID if all retries failed
			var fallbackCode = $"EV-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..30];
			_logger.LogError("電子禮券代碼生成失敗，使用 Fallback: Code={Code}", fallbackCode);
			return fallbackCode;
		}
	}
}
