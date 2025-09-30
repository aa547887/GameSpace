using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminWalletController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminWalletController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MiniGame/AdminWallet
        public IActionResult Index()
        {
            return View();
        }

        // AJAX endpoint for wallet overview data
        [HttpGet]
        public async Task<IActionResult> GetWalletOverview()
        {
            try
            {
                var walletData = await _context.UserWallets
                    .Include(uw => uw.User)
                    .Select(uw => new
                    {
                        uw.User_ID,
                        uw.User.User_name,
                        uw.User.User_Account,
                        uw.Points,
                        uw.CreatedAt,
                        uw.UpdatedAt
                    })
                    .ToListAsync();

                return Json(new { success = true, data = walletData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminWallet/CouponManagement
        public IActionResult CouponManagement()
        {
            return View();
        }

        // AJAX endpoint for coupon data
        [HttpGet]
        public async Task<IActionResult> GetCouponData()
        {
            try
            {
                var couponData = await _context.Coupons
                    .Include(c => c.User)
                    .Select(c => new
                    {
                        c.CouponID,
                        c.User_ID,
                        c.User.User_name,
                        c.User.User_Account,
                        c.CouponName,
                        c.DiscountAmount,
                        c.DiscountPercentage,
                        c.MinimumPurchaseAmount,
                        c.ExpiryDate,
                        c.IsUsed,
                        c.CreatedAt
                    })
                    .ToListAsync();

                return Json(new { success = true, data = couponData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminWallet/EVoucherManagement
        public IActionResult EVoucherManagement()
        {
            return View();
        }

        // AJAX endpoint for EVoucher data
        [HttpGet]
        public async Task<IActionResult> GetEVoucherData()
        {
            try
            {
                var evoucherData = await _context.EVouchers
                    .Include(ev => ev.User)
                    .Select(ev => new
                    {
                        ev.EVoucherID,
                        ev.User_ID,
                        ev.User.User_name,
                        ev.User.User_Account,
                        ev.VoucherName,
                        ev.VoucherValue,
                        ev.ExpiryDate,
                        ev.IsUsed,
                        ev.CreatedAt
                    })
                    .ToListAsync();

                return Json(new { success = true, data = evoucherData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminWallet/QueryMemberPoints
        public IActionResult QueryMemberPoints()
        {
            return View();
        }

        // AJAX endpoint for member points query
        [HttpGet]
        public async Task<IActionResult> GetMemberPoints(string searchTerm = "")
        {
            try
            {
                var query = _context.UserWallets
                    .Include(uw => uw.User)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(uw => 
                        uw.User.User_name.Contains(searchTerm) || 
                        uw.User.User_Account.Contains(searchTerm));
                }

                var memberPoints = await query
                    .Select(uw => new
                    {
                        uw.User_ID,
                        uw.User.User_name,
                        uw.User.User_Account,
                        uw.Points,
                        uw.CreatedAt,
                        uw.UpdatedAt
                    })
                    .ToListAsync();

                return Json(new { success = true, data = memberPoints });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminWallet/IssueMemberPoints
        public IActionResult IssueMemberPoints()
        {
            return View();
        }

        // POST: MiniGame/AdminWallet/IssueMemberPoints
        [HttpPost]
        public async Task<IActionResult> IssueMemberPoints(int userId, int points, string reason)
        {
            try
            {
                var userWallet = await _context.UserWallets
                    .FirstOrDefaultAsync(uw => uw.User_ID == userId);

                if (userWallet == null)
                {
                    return Json(new { success = false, message = "找不到該會員的錢包" });
                }

                // 更新點數
                userWallet.Points += points;
                userWallet.UpdatedAt = DateTime.Now;

                // 記錄到錢包歷史
                var walletHistory = new WalletHistory
                {
                    User_ID = userId,
                    TransactionType = "點數發放",
                    Amount = points,
                    Description = reason,
                    CreatedAt = DateTime.Now
                };

                _context.WalletHistories.Add(walletHistory);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "點數發放成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminWallet/IssueMemberCoupons
        public IActionResult IssueMemberCoupons()
        {
            return View();
        }

        // POST: MiniGame/AdminWallet/IssueMemberCoupons
        [HttpPost]
        public async Task<IActionResult> IssueMemberCoupons(int userId, string couponName, decimal discountAmount, decimal? discountPercentage, decimal? minimumPurchaseAmount, DateTime expiryDate, string reason)
        {
            try
            {
                var coupon = new Coupon
                {
                    User_ID = userId,
                    CouponName = couponName,
                    DiscountAmount = discountAmount,
                    DiscountPercentage = discountPercentage,
                    MinimumPurchaseAmount = minimumPurchaseAmount,
                    ExpiryDate = expiryDate,
                    IsUsed = false,
                    CreatedAt = DateTime.Now
                };

                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "優惠券發放成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminWallet/AdjustMemberEVouchers
        public IActionResult AdjustMemberEVouchers()
        {
            return View();
        }

        // POST: MiniGame/AdminWallet/AdjustMemberEVouchers
        [HttpPost]
        public async Task<IActionResult> AdjustMemberEVouchers(int userId, string voucherName, decimal voucherValue, DateTime expiryDate, string reason)
        {
            try
            {
                var evoucher = new EVoucher
                {
                    User_ID = userId,
                    VoucherName = voucherName,
                    VoucherValue = voucherValue,
                    ExpiryDate = expiryDate,
                    IsUsed = false,
                    CreatedAt = DateTime.Now
                };

                _context.EVouchers.Add(evoucher);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "電子禮券發放成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminWallet/ViewMemberTransactionDetails
        public IActionResult ViewMemberTransactionDetails()
        {
            return View();
        }

        // AJAX endpoint for member transaction details
        [HttpGet]
        public async Task<IActionResult> GetMemberTransactionDetails(int userId)
        {
            try
            {
                var transactionDetails = await _context.WalletHistories
                    .Include(wh => wh.User)
                    .Where(wh => wh.User_ID == userId)
                    .OrderByDescending(wh => wh.CreatedAt)
                    .Select(wh => new
                    {
                        wh.TransactionID,
                        wh.User.User_name,
                        wh.User.User_Account,
                        wh.TransactionType,
                        wh.Amount,
                        wh.Description,
                        wh.CreatedAt
                    })
                    .ToListAsync();

                return Json(new { success = true, data = transactionDetails });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
