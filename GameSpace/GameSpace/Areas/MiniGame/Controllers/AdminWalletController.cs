using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public class AdminWalletController : MiniGameBaseController
    {
        private readonly IMiniGameAdminService _adminService;

        public AdminWalletController(GameSpacedatabaseContext context, IMiniGameAdminService adminService) : base(context)
        {
            _adminService = adminService;
        }

        // 查詢會員點數
        public async Task<IActionResult> QueryPoints(WalletQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            try
            {
                var userPoints = await QueryUserPointsAsync(query);
                var users = await _context.Users.ToListAsync();

                var viewModel = new AdminWalletIndexViewModel
                {
                    UserPoints = userPoints.Items,
                    Users = users,
                    Query = query,
                    TotalCount = userPoints.TotalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢會員點數時發生錯誤：{ex.Message}";
                return View(new AdminWalletIndexViewModel());
            }
        }

        // 查詢會員擁有商城優惠券
        public async Task<IActionResult> QueryCoupons(CouponQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            try
            {
                var result = await QueryUserCouponsAsync(query);
                var users = await _context.Users.ToListAsync();

                var viewModel = new AdminCouponIndexViewModel
                {
                    Coupons = result.Items,
                    Users = users,
                    Query = query,
                    TotalCount = result.TotalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢會員優惠券時發生錯誤：{ex.Message}";
                return View(new AdminCouponIndexViewModel());
            }
        }

        // 查詢會員擁有電子禮券
        public async Task<IActionResult> QueryEVouchers(EVoucherQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            try
            {
                var result = await QueryUserEVouchersAsync(query);
                var users = await _context.Users.ToListAsync();

                var viewModel = new AdminEVoucherIndexViewModel
                {
                    EVouchers = result.Items,
                    Users = users,
                    Query = query,
                    TotalCount = result.TotalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢會員電子禮券時發生錯誤：{ex.Message}";
                return View(new AdminEVoucherIndexViewModel());
            }
        }

        // 發放會員點數
        [HttpGet]
        public async Task<IActionResult> GrantPoints()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                var viewModel = new GrantPointsViewModel
                {
                    Users = users
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入發放點數頁面時發生錯誤：{ex.Message}";
                return View(new GrantPointsViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> GrantPoints(GrantPointsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Users = await _context.Users.ToListAsync();
                return View(model);
            }

            try
            {
                await GrantUserPointsAsync(model.UserId, model.Points, model.Reason);
                TempData["SuccessMessage"] = "會員點數發放成功！";
                return RedirectToAction(nameof(QueryPoints));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"發放失敗：{ex.Message}");
                model.Users = await _context.Users.ToListAsync();
                return View(model);
            }
        }

        // 發放會員擁有商城優惠券（含發放）
        [HttpGet]
        public async Task<IActionResult> GrantCoupons()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                var couponTypes = await _context.CouponTypes.ToListAsync();
                
                var viewModel = new GrantCouponsViewModel
                {
                    Users = users,
                    CouponTypes = couponTypes
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入發放優惠券頁面時發生錯誤：{ex.Message}";
                return View(new GrantCouponsViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> GrantCoupons(GrantCouponsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Users = await _context.Users.ToListAsync();
                model.CouponTypes = await _context.CouponTypes.ToListAsync();
                return View(model);
            }

            try
            {
                await GrantUserCouponsAsync(model.UserId, model.CouponTypeId, model.Quantity, model.Reason);
                TempData["SuccessMessage"] = "商城優惠券發放成功！";
                return RedirectToAction(nameof(QueryCoupons));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"發放失敗：{ex.Message}");
                model.Users = await _context.Users.ToListAsync();
                model.CouponTypes = await _context.CouponTypes.ToListAsync();
                return View(model);
            }
        }

        // 調整會員擁有電子禮券（發放）
        [HttpGet]
        public async Task<IActionResult> GrantEVouchers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                var eVoucherTypes = await _context.EVoucherTypes.ToListAsync();
                
                var viewModel = new GrantEVouchersViewModel
                {
                    Users = users,
                    EVoucherTypes = eVoucherTypes
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入發放電子禮券頁面時發生錯誤：{ex.Message}";
                return View(new GrantEVouchersViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> GrantEVouchers(GrantEVouchersViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Users = await _context.Users.ToListAsync();
                model.EVoucherTypes = await _context.EVoucherTypes.ToListAsync();
                return View(model);
            }

            try
            {
                await GrantUserEVouchersAsync(model.UserId, model.EVoucherTypeId, model.Quantity, model.Reason);
                TempData["SuccessMessage"] = "電子禮券發放成功！";
                return RedirectToAction(nameof(QueryEVouchers));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"發放失敗：{ex.Message}");
                model.Users = await _context.Users.ToListAsync();
                model.EVoucherTypes = await _context.EVoucherTypes.ToListAsync();
                return View(model);
            }
        }

        // 查看會員收支明細
        public async Task<IActionResult> ViewHistory(WalletHistoryQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            try
            {
                var result = await QueryWalletHistoryAsync(query);
                var users = await _context.Users.ToListAsync();

                var viewModel = new AdminWalletHistoryViewModel
                {
                    WalletHistories = result.Items,
                    Users = users,
                    Query = query,
                    TotalCount = result.TotalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢收支明細時發生錯誤：{ex.Message}";
                return View(new AdminWalletHistoryViewModel());
            }
        }

        // AJAX API 方法
        [HttpGet]
        public async Task<IActionResult> GetUserPoints(int userId)
        {
            try
            {
                var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId);
                var points = wallet?.UserPoint ?? 0;
                return Json(new { success = true, points = points });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserCoupons(int userId)
        {
            try
            {
                var coupons = await _context.Coupons
                    .Include(c => c.CouponType)
                    .Where(c => c.UserId == userId)
                    .Select(c => new
                    {
                        couponId = c.CouponId,
                        couponCode = c.CouponCode,
                        couponName = c.CouponType.Name,
                        isUsed = c.IsUsed,
                        acquiredTime = c.AcquiredTime
                    })
                    .ToListAsync();

                return Json(new { success = true, coupons = coupons });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserEVouchers(int userId)
        {
            try
            {
                var eVouchers = await _context.EVouchers
                    .Include(e => e.EVoucherType)
                    .Where(e => e.UserId == userId)
                    .Select(e => new
                    {
                        eVoucherId = e.EVoucherId,
                        eVoucherCode = e.EVoucherCode,
                        eVoucherName = e.EVoucherType.Name,
                        valueAmount = e.EVoucherType.ValueAmount,
                        isUsed = e.IsUsed,
                        acquiredTime = e.AcquiredTime
                    })
                    .ToListAsync();

                return Json(new { success = true, eVouchers = eVouchers });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 私有方法
        private async Task<PagedResult<UserPointsModel>> QueryUserPointsAsync(WalletQueryModel query)
        {
            var queryable = _context.UserWallets
                .Include(w => w.User)
                .AsQueryable();

            if (query.UserId.HasValue)
                queryable = queryable.Where(w => w.UserId == query.UserId.Value);

            if (!string.IsNullOrEmpty(query.UserName))
                queryable = queryable.Where(w => w.User.UserName.Contains(query.UserName));

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(w => w.UserPoint)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(w => new UserPointsModel
                {
                    UserId = w.UserId,
                    UserName = w.User.UserName,
                    UserAccount = w.User.UserAccount,
                    Points = w.UserPoint
                })
                .ToListAsync();

            return new PagedResult<UserPointsModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        private async Task<PagedResult<UserCouponModel>> QueryUserCouponsAsync(CouponQueryModel query)
        {
            var queryable = _context.Coupons
                .Include(c => c.User)
                .Include(c => c.CouponType)
                .AsQueryable();

            if (query.UserId.HasValue)
                queryable = queryable.Where(c => c.UserId == query.UserId.Value);

            if (!string.IsNullOrEmpty(query.UserName))
                queryable = queryable.Where(c => c.User.UserName.Contains(query.UserName));

            if (query.IsUsed.HasValue)
                queryable = queryable.Where(c => c.IsUsed == query.IsUsed.Value);

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(c => c.AcquiredTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new UserCouponModel
                {
                    CouponId = c.CouponId,
                    UserId = c.UserId,
                    UserName = c.User.UserName,
                    CouponCode = c.CouponCode,
                    CouponName = c.CouponType.Name,
                    DiscountValue = c.CouponType.DiscountValue,
                    DiscountType = c.CouponType.DiscountType,
                    IsUsed = c.IsUsed,
                    AcquiredTime = c.AcquiredTime,
                    UsedTime = c.UsedTime
                })
                .ToListAsync();

            return new PagedResult<UserCouponModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        private async Task<PagedResult<UserEVoucherModel>> QueryUserEVouchersAsync(EVoucherQueryModel query)
        {
            var queryable = _context.EVouchers
                .Include(e => e.User)
                .Include(e => e.EVoucherType)
                .AsQueryable();

            if (query.UserId.HasValue)
                queryable = queryable.Where(e => e.UserId == query.UserId.Value);

            if (!string.IsNullOrEmpty(query.UserName))
                queryable = queryable.Where(e => e.User.UserName.Contains(query.UserName));

            if (query.IsUsed.HasValue)
                queryable = queryable.Where(e => e.IsUsed == query.IsUsed.Value);

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(e => e.AcquiredTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(e => new UserEVoucherModel
                {
                    EVoucherId = e.EVoucherId,
                    UserId = e.UserId,
                    UserName = e.User.UserName,
                    EVoucherCode = e.EVoucherCode,
                    EVoucherName = e.EVoucherType.Name,
                    ValueAmount = e.EVoucherType.ValueAmount,
                    IsUsed = e.IsUsed,
                    AcquiredTime = e.AcquiredTime,
                    UsedTime = e.UsedTime
                })
                .ToListAsync();

            return new PagedResult<UserEVoucherModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        private async Task<PagedResult<WalletHistoryModel>> QueryWalletHistoryAsync(WalletHistoryQueryModel query)
        {
            var queryable = _context.WalletHistories
                .Include(w => w.User)
                .AsQueryable();

            if (query.UserId.HasValue)
                queryable = queryable.Where(w => w.UserId == query.UserId.Value);

            if (!string.IsNullOrEmpty(query.UserName))
                queryable = queryable.Where(w => w.User.UserName.Contains(query.UserName));

            if (!string.IsNullOrEmpty(query.ChangeType))
                queryable = queryable.Where(w => w.ChangeType == query.ChangeType);

            if (query.StartDate.HasValue)
                queryable = queryable.Where(w => w.ChangeTime >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                queryable = queryable.Where(w => w.ChangeTime <= query.EndDate.Value);

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(w => w.ChangeTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(w => new WalletHistoryModel
                {
                    LogId = w.LogId,
                    UserId = w.UserId,
                    UserName = w.User.UserName,
                    ChangeType = w.ChangeType,
                    PointsChanged = w.PointsChanged,
                    ItemCode = w.ItemCode,
                    Description = w.Description,
                    ChangeTime = w.ChangeTime
                })
                .ToListAsync();

            return new PagedResult<WalletHistoryModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        private async Task GrantUserPointsAsync(int userId, int points, string reason)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId);
                if (wallet == null)
                {
                    wallet = new UserWallet { UserId = userId, UserPoint = 0 };
                    _context.UserWallets.Add(wallet);
                }

                wallet.UserPoint += points;

                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Point",
                    PointsChanged = points,
                    Description = reason ?? $"管理員發放 {points} 點",
                    ChangeTime = DateTime.Now
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task GrantUserCouponsAsync(int userId, int couponTypeId, int quantity, string reason)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var couponType = await _context.CouponTypes.FindAsync(couponTypeId);
                if (couponType == null)
                    throw new Exception("找不到指定的優惠券類型");

                for (int i = 0; i < quantity; i++)
                {
                    var coupon = new Coupon
                    {
                        CouponCode = GenerateCouponCode(),
                        CouponTypeId = couponTypeId,
                        UserId = userId,
                        IsUsed = false,
                        AcquiredTime = DateTime.Now
                    };
                    _context.Coupons.Add(coupon);

                    var history = new WalletHistory
                    {
                        UserId = userId,
                        ChangeType = "Coupon",
                        PointsChanged = 0,
                        ItemCode = coupon.CouponCode,
                        Description = reason ?? $"管理員發放優惠券：{couponType.Name}",
                        ChangeTime = DateTime.Now
                    };
                    _context.WalletHistories.Add(history);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task GrantUserEVouchersAsync(int userId, int eVoucherTypeId, int quantity, string reason)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var eVoucherType = await _context.EVoucherTypes.FindAsync(eVoucherTypeId);
                if (eVoucherType == null)
                    throw new Exception("找不到指定的電子禮券類型");

                for (int i = 0; i < quantity; i++)
                {
                    var eVoucher = new EVoucher
                    {
                        EVoucherCode = GenerateEVoucherCode(),
                        EVoucherTypeId = eVoucherTypeId,
                        UserId = userId,
                        IsUsed = false,
                        AcquiredTime = DateTime.Now
                    };
                    _context.EVouchers.Add(eVoucher);

                    var history = new WalletHistory
                    {
                        UserId = userId,
                        ChangeType = "EVoucher",
                        PointsChanged = 0,
                        ItemCode = eVoucher.EVoucherCode,
                        Description = reason ?? $"管理員發放電子禮券：{eVoucherType.Name}",
                        ChangeTime = DateTime.Now
                    };
                    _context.WalletHistories.Add(history);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private string GenerateCouponCode()
        {
            return "COUPON" + DateTime.Now.Ticks.ToString("X")[^8..];
        }

        private string GenerateEVoucherCode()
        {
            return "EVOUCHER" + DateTime.Now.Ticks.ToString("X")[^12..];
        }

        // 保持舊有的Index方法以向後兼容
        public async Task<IActionResult> Index(WalletQueryModel query)
        {
            return await QueryPoints(query);
        }

        public async Task<IActionResult> AdjustPoints()
        {
            return await GrantPoints();
        }

        public async Task<IActionResult> History(WalletHistoryQueryModel query)
        {
            return await ViewHistory(query);
        }
    }

    // ViewModels
    public class WalletQueryModel
    {
        public int? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class CouponQueryModel
    {
        public int? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool? IsUsed { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class EVoucherQueryModel
    {
        public int? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool? IsUsed { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class WalletHistoryQueryModel
    {
        public int? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AdminWalletIndexViewModel
    {
        public List<UserPointsModel> UserPoints { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public WalletQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class AdminCouponIndexViewModel
    {
        public List<UserCouponModel> Coupons { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public CouponQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class AdminEVoucherIndexViewModel
    {
        public List<UserEVoucherModel> EVouchers { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public EVoucherQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class AdminWalletHistoryViewModel
    {
        public List<WalletHistoryModel> WalletHistories { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public WalletHistoryQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class GrantPointsViewModel
    {
        public int UserId { get; set; }
        public int Points { get; set; }
        public string Reason { get; set; } = string.Empty;
        public List<User> Users { get; set; } = new();
    }

    public class GrantCouponsViewModel
    {
        public int UserId { get; set; }
        public int CouponTypeId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public List<User> Users { get; set; } = new();
        public List<CouponType> CouponTypes { get; set; } = new();
    }

    public class GrantEVouchersViewModel
    {
        public int UserId { get; set; }
        public int EVoucherTypeId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public List<User> Users { get; set; } = new();
        public List<EVoucherType> EVoucherTypes { get; set; } = new();
    }

    public class UserPointsModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserAccount { get; set; } = string.Empty;
        public int Points { get; set; }
    }

    public class UserCouponModel
    {
        public int CouponId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string CouponCode { get; set; } = string.Empty;
        public string CouponName { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public string DiscountType { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
    }

    public class UserEVoucherModel
    {
        public int EVoucherId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string EVoucherCode { get; set; } = string.Empty;
        public string EVoucherName { get; set; } = string.Empty;
        public decimal ValueAmount { get; set; }
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
    }

    public class WalletHistoryModel
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public int PointsChanged { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ChangeTime { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
