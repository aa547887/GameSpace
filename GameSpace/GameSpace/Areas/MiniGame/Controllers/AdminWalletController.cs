using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using System.Text.Json;

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

        // 1. 查詢會員點數
        [HttpGet]
        public async Task<IActionResult> QueryPoints(WalletQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 20;

            try
            {
                var result = await QueryUserPointsAsync(query);
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

            var viewModel = new AdminWalletIndexViewModel
            {
                    UserPoints = result.Items,
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
                TempData["ErrorMessage"] = $"查詢會員點數時發生錯誤：{ex.Message}";
                return View(new AdminWalletIndexViewModel());
            }
        }

        // 2. 查詢會員擁有商城優惠券
        [HttpGet]
        public async Task<IActionResult> QueryCoupons(CouponQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 20;

            try
            {
                var result = await QueryUserCouponsAsync(query);
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

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

        // 3. 查詢會員擁有電子禮券
        [HttpGet]
        public async Task<IActionResult> QueryEVouchers(EVoucherQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 20;

            try
            {
                var result = await QueryUserEVouchersAsync(query);
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

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

        // 4. 發放會員點數
        [HttpGet]
        public async Task<IActionResult> GrantPoints()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

                var viewModel = new GrantPointsModel
            {
                Users = users
            };

            return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入發放點數頁面時發生錯誤：{ex.Message}";
                return View(new GrantPointsModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> GrantPoints(GrantPointsModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                return View(model);
            }

            try
            {
                await GrantUserPointsAsync(model);
                TempData["SuccessMessage"] = $"成功發放 {model.Points} 點給用戶 {model.UserId}！";
                return RedirectToAction(nameof(GrantPoints));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"發放點數失敗：{ex.Message}");
                model.Users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                return View(model);
            }
        }

        // 5. 發放會員擁有商城優惠券
        [HttpGet]
        public async Task<IActionResult> GrantCoupons()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

                var couponTypes = await _context.CouponTypes
                    .Where(ct => ct.IsActive)
                    .Select(ct => new { ct.Id, ct.Name, ct.Description })
                    .ToListAsync();

                var viewModel = new GrantCouponsModel
            {
                Users = users,
                CouponTypes = couponTypes
            };

            return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入發放優惠券頁面時發生錯誤：{ex.Message}";
                return View(new GrantCouponsModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> GrantCoupons(GrantCouponsModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                model.CouponTypes = await _context.CouponTypes
                    .Where(ct => ct.IsActive)
                    .Select(ct => new { ct.Id, ct.Name, ct.Description })
                    .ToListAsync();
                return View(model);
            }

            try
            {
                await GrantUserCouponsAsync(model);
                TempData["SuccessMessage"] = $"成功發放 {model.Quantity} 張優惠券給用戶 {model.UserId}！";
                return RedirectToAction(nameof(GrantCoupons));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"發放優惠券失敗：{ex.Message}");
                model.Users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                model.CouponTypes = await _context.CouponTypes
                    .Where(ct => ct.IsActive)
                    .Select(ct => new { ct.Id, ct.Name, ct.Description })
                    .ToListAsync();
                return View(model);
            }
        }

        // 6. 調整會員擁有電子禮券（發放）
        [HttpGet]
        public async Task<IActionResult> GrantEVouchers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

                var eVoucherTypes = await _context.EvoucherTypes
                    .Where(et => et.IsActive)
                    .Select(et => new { et.Id, et.Name, et.Description })
                    .ToListAsync();

                var viewModel = new GrantEVouchersModel
            {
                Users = users,
                EVoucherTypes = eVoucherTypes
            };

            return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入發放電子禮券頁面時發生錯誤：{ex.Message}";
                return View(new GrantEVouchersModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> GrantEVouchers(GrantEVouchersModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                model.EVoucherTypes = await _context.EvoucherTypes
                    .Where(et => et.IsActive)
                    .Select(et => new { et.Id, et.Name, et.Description })
                    .ToListAsync();
                return View(model);
            }

            try
            {
                await GrantUserEVouchersAsync(model);
                TempData["SuccessMessage"] = $"成功發放 {model.Quantity} 張電子禮券給用戶 {model.UserId}！";
                return RedirectToAction(nameof(GrantEVouchers));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"發放電子禮券失敗：{ex.Message}");
                model.Users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                model.EVoucherTypes = await _context.EvoucherTypes
                    .Where(et => et.IsActive)
                    .Select(et => new { et.Id, et.Name, et.Description })
                    .ToListAsync();
                return View(model);
            }
        }

        // 7. 查看會員收支明細
        [HttpGet]
        public async Task<IActionResult> QueryHistory(WalletHistoryQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 20;

            try
            {
                var result = await QueryWalletHistoryAsync(query);
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

            var viewModel = new AdminWalletHistoryViewModel
            {
                    WalletHistory = result.Items,
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

        // 查看詳細收支明細
        [HttpGet]
        public async Task<IActionResult> ViewHistory(int userId, string transactionType = "all")
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的用戶！";
                    return RedirectToAction(nameof(QueryHistory));
                }

                var history = await GetUserWalletHistoryAsync(userId, transactionType);

                var viewModel = new AdminWalletHistoryDetailViewModel
                {
                    User = user,
                    WalletHistory = history,
                    TransactionType = transactionType
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查看收支明細時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(QueryHistory));
            }
        }

        // API: 獲取用戶詳細信息
        [HttpGet]
        public async Task<IActionResult> GetUserDetails(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Email,
                        u.CreateTime,
                        Wallet = _context.UserWallets
                            .Where(w => w.UserId == userId)
                            .Select(w => new { w.UserPoint, w.LastUpdateTime })
                            .FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return Json(new { success = false, message = "找不到指定的用戶" });
                }

                return Json(new { success = true, data = user });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: 快速查詢用戶點數
        [HttpGet]
        public async Task<IActionResult> QuickQueryPoints(string searchTerm)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.UserName.Contains(searchTerm) || u.Email.Contains(searchTerm))
                    .Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Email,
                        Points = _context.UserWallets
                            .Where(w => w.UserId == u.Id)
                            .Select(w => w.UserPoint)
                            .FirstOrDefault()
                    })
                    .Take(10)
                    .ToListAsync();

                return Json(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: 獲取統計數據
        [HttpGet]
        public async Task<IActionResult> GetWalletStats()
        {
            try
            {
                var today = DateTime.Today;
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                var stats = new
                {
                    TotalUsers = await _context.Users.CountAsync(),
                    TotalPoints = await _context.UserWallets.SumAsync(w => w.UserPoint),
                    TotalCoupons = await _context.Coupons.CountAsync(),
                    TotalEVouchers = await _context.Evouchers.CountAsync(),
                    TodayTransactions = await _context.WalletHistories
                        .Where(wh => wh.TransactionTime.Date == today)
                        .CountAsync(),
                    ThisMonthTransactions = await _context.WalletHistories
                        .Where(wh => wh.TransactionTime >= thisMonth)
                        .CountAsync()
                };

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 私有方法：查詢用戶點數
        private async Task<PagedResult<UserWalletModel>> QueryUserPointsAsync(WalletQueryModel query)
        {
            var queryable = _context.UserWallets
                .Include(w => w.User)
                .AsQueryable();

            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(w => w.UserId == query.UserId.Value);
            }

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(w => 
                    w.User.UserName.Contains(query.SearchTerm) || 
                    w.User.Email.Contains(query.SearchTerm));
            }

            if (query.MinPoints.HasValue)
            {
                queryable = queryable.Where(w => w.UserPoint >= query.MinPoints.Value);
            }

            if (query.MaxPoints.HasValue)
            {
                queryable = queryable.Where(w => w.UserPoint <= query.MaxPoints.Value);
            }

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(w => w.UserPoint)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(w => new UserWalletModel
                {
                    UserId = w.UserId,
                    UserName = w.User.UserName,
                    Email = w.User.Email,
                    UserPoint = w.UserPoint,
                    LastUpdateTime = w.LastUpdateTime
                })
                .ToListAsync();

            return new PagedResult<UserWalletModel>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        // 私有方法：查詢用戶優惠券
        private async Task<PagedResult<UserCouponModel>> QueryUserCouponsAsync(CouponQueryModel query)
        {
            var queryable = _context.Coupons
                .Include(c => c.User)
                .Include(c => c.CouponType)
                .AsQueryable();

            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(c => c.UserId == query.UserId.Value);
            }

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(c => 
                    c.User.UserName.Contains(query.SearchTerm) || 
                    c.User.Email.Contains(query.SearchTerm) ||
                    c.CouponType.Name.Contains(query.SearchTerm));
            }

            if (query.CouponTypeId.HasValue)
            {
                queryable = queryable.Where(c => c.CouponTypeId == query.CouponTypeId.Value);
            }

            if (query.IsUsed.HasValue)
            {
                queryable = queryable.Where(c => c.IsUsed == query.IsUsed.Value);
            }

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(c => c.CreateTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new UserCouponModel
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    UserName = c.User.UserName,
                    Email = c.User.Email,
                    CouponTypeId = c.CouponTypeId,
                    CouponTypeName = c.CouponType.Name,
                    IsUsed = c.IsUsed,
                    CreateTime = c.CreateTime,
                    UsedTime = c.UsedTime
                })
                .ToListAsync();

            return new PagedResult<UserCouponModel>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        // 私有方法：查詢用戶電子禮券
        private async Task<PagedResult<UserEVoucherModel>> QueryUserEVouchersAsync(EVoucherQueryModel query)
        {
            var queryable = _context.Evouchers
                .Include(e => e.User)
                .Include(e => e.EvoucherType)
                .AsQueryable();

            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(e => e.UserId == query.UserId.Value);
            }

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(e => 
                    e.User.UserName.Contains(query.SearchTerm) || 
                    e.User.Email.Contains(query.SearchTerm) ||
                    e.EvoucherType.Name.Contains(query.SearchTerm));
            }

            if (query.EVoucherTypeId.HasValue)
            {
                queryable = queryable.Where(e => e.EvoucherTypeId == query.EVoucherTypeId.Value);
            }

            if (query.IsUsed.HasValue)
            {
                queryable = queryable.Where(e => e.IsUsed == query.IsUsed.Value);
            }

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(e => e.CreateTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(e => new UserEVoucherModel
                {
                    Id = e.Id,
                    UserId = e.UserId,
                    UserName = e.User.UserName,
                    Email = e.User.Email,
                    EVoucherTypeId = e.EvoucherTypeId,
                    EVoucherTypeName = e.EvoucherType.Name,
                    IsUsed = e.IsUsed,
                    CreateTime = e.CreateTime,
                    UsedTime = e.UsedTime
                })
                .ToListAsync();

            return new PagedResult<UserEVoucherModel>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        // 私有方法：發放用戶點數
        private async Task GrantUserPointsAsync(GrantPointsModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == model.UserId);

                if (wallet == null)
                {
                    wallet = new UserWallet
                    {
                        UserId = model.UserId,
                        UserPoint = 0,
                        CreateTime = DateTime.Now,
                        LastUpdateTime = DateTime.Now
                    };
                    _context.UserWallets.Add(wallet);
                }

                wallet.UserPoint += model.Points;
                wallet.LastUpdateTime = DateTime.Now;

                var history = new WalletHistory
                {
                    UserId = model.UserId,
                    TransactionType = "Grant",
                    Amount = model.Points,
                    Description = model.Description ?? "管理員發放點數",
                    TransactionTime = DateTime.Now
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

        // 私有方法：發放用戶優惠券
        private async Task GrantUserCouponsAsync(GrantCouponsModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var couponType = await _context.CouponTypes
                    .FirstOrDefaultAsync(ct => ct.Id == model.CouponTypeId);

                if (couponType == null)
                {
                    throw new Exception("找不到指定的優惠券類型");
                }

                for (int i = 0; i < model.Quantity; i++)
                {
                    var coupon = new Coupon
                    {
                        UserId = model.UserId,
                        CouponTypeId = model.CouponTypeId,
                        IsUsed = false,
                        CreateTime = DateTime.Now
                    };

                    _context.Coupons.Add(coupon);
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

        // 私有方法：發放用戶電子禮券
        private async Task GrantUserEVouchersAsync(GrantEVouchersModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var eVoucherType = await _context.EvoucherTypes
                    .FirstOrDefaultAsync(et => et.Id == model.EVoucherTypeId);

                if (eVoucherType == null)
                {
                    throw new Exception("找不到指定的電子禮券類型");
                }

                for (int i = 0; i < model.Quantity; i++)
                {
                    var eVoucher = new Evoucher
                    {
                        UserId = model.UserId,
                        EvoucherTypeId = model.EVoucherTypeId,
                        IsUsed = false,
                        CreateTime = DateTime.Now
                    };

                    _context.Evouchers.Add(eVoucher);
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

        // 私有方法：查詢錢包歷史
        private async Task<PagedResult<WalletHistoryModel>> QueryWalletHistoryAsync(WalletHistoryQueryModel query)
        {
            var queryable = _context.WalletHistories
                .Include(wh => wh.User)
                .AsQueryable();

            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(wh => wh.UserId == query.UserId.Value);
            }

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(wh => 
                    wh.User.UserName.Contains(query.SearchTerm) || 
                    wh.User.Email.Contains(query.SearchTerm) ||
                    wh.Description.Contains(query.SearchTerm));
            }

            if (!string.IsNullOrEmpty(query.TransactionType))
            {
                queryable = queryable.Where(wh => wh.TransactionType == query.TransactionType);
            }

            if (query.StartDate.HasValue)
            {
                queryable = queryable.Where(wh => wh.TransactionTime >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                queryable = queryable.Where(wh => wh.TransactionTime <= query.EndDate.Value);
            }

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(wh => wh.TransactionTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(wh => new WalletHistoryModel
                {
                    Id = wh.Id,
                    UserId = wh.UserId,
                    UserName = wh.User.UserName,
                    Email = wh.User.Email,
                    TransactionType = wh.TransactionType,
                    Amount = wh.Amount,
                    Description = wh.Description,
                    TransactionTime = wh.TransactionTime
                })
                .ToListAsync();

            return new PagedResult<WalletHistoryModel>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        // 私有方法：獲取用戶錢包歷史
        private async Task<List<WalletHistoryModel>> GetUserWalletHistoryAsync(int userId, string transactionType)
        {
            var queryable = _context.WalletHistories
                .Include(wh => wh.User)
                .Where(wh => wh.UserId == userId);

            if (transactionType != "all")
            {
                queryable = queryable.Where(wh => wh.TransactionType == transactionType);
            }

            return await queryable
                .OrderByDescending(wh => wh.TransactionTime)
                .Select(wh => new WalletHistoryModel
                {
                    Id = wh.Id,
                    UserId = wh.UserId,
                    UserName = wh.User.UserName,
                    Email = wh.User.Email,
                    TransactionType = wh.TransactionType,
                    Amount = wh.Amount,
                    Description = wh.Description,
                    TransactionTime = wh.TransactionTime
                })
                .ToListAsync();
        }
    }
}
