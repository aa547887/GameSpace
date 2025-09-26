using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Route("MiniGame/[controller]")]
    public class AdminWalletController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminWalletController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminWalletViewModel
            {
                TotalPointsDistributed = await _context.WalletHistory
                    .Where(h => h.PointsChanged > 0)
                    .SumAsync(h => h.PointsChanged),
                TodayPointChanges = await _context.WalletHistory
                    .Where(h => h.ChangeTime.Date == DateTime.Today)
                    .CountAsync(),
                TotalCouponsIssued = await _context.Coupon.CountAsync(),
                TotalEVouchersIssued = await _context.EVoucher.CountAsync()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<JsonResult> GetWalletOverview()
        {
            try
            {
                var data = new
                {
                    totalPoints = await _context.User_Wallet.SumAsync(w => w.User_Point),
                    todayPointChanges = await _context.WalletHistory
                        .Where(h => h.ChangeTime.Date == DateTime.Today)
                        .CountAsync(),
                    totalCoupons = await _context.Coupon.CountAsync(),
                    totalEVouchers = await _context.EVoucher.CountAsync(),
                    walletHistory = await GetWalletHistoryAsync()
                };

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetWalletHistory(int page = 1, int pageSize = 50)
        {
            try
            {
                var history = await _context.WalletHistory
                    .Include(h => h.User)
                    .OrderByDescending(h => h.ChangeTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(h => new
                    {
                        logId = h.LogID,
                        userId = h.UserID,
                        userName = h.User.User_name,
                        changeType = h.ChangeType,
                        amount = h.PointsChanged,
                        description = h.Description,
                        changeTime = h.ChangeTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        status = "已完成"
                    })
                    .ToListAsync();

                return Json(new { success = true, data = history });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> AdjustUserPoints(int userId, int pointsChange, string reason)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "用戶不存在" });
                }

                var wallet = await _context.User_Wallet.FirstOrDefaultAsync(w => w.User_Id == userId);
                if (wallet == null)
                {
                    wallet = new User_Wallet { User_Id = userId, User_Point = 0 };
                    _context.User_Wallet.Add(wallet);
                }

                // Check if adjustment would result in negative balance
                if (wallet.User_Point + pointsChange < 0)
                {
                    return Json(new { success = false, message = "調整後點數不能為負數" });
                }

                wallet.User_Point += pointsChange;

                // Add history record
                var historyRecord = new WalletHistory
                {
                    UserID = userId,
                    ChangeType = "Manual",
                    PointsChanged = pointsChange,
                    Description = $"管理員調整：{reason}",
                    ChangeTime = DateTime.UtcNow
                };

                _context.WalletHistory.Add(historyRecord);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "點數調整成功", newBalance = wallet.User_Point });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetUserWalletInfo(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.User_Wallet)
                    .FirstOrDefaultAsync(u => u.User_ID == userId);

                if (user == null)
                {
                    return Json(new { success = false, message = "用戶不存在" });
                }

                var recentHistory = await _context.WalletHistory
                    .Where(h => h.UserID == userId)
                    .OrderByDescending(h => h.ChangeTime)
                    .Take(10)
                    .Select(h => new
                    {
                        changeType = h.ChangeType,
                        amount = h.PointsChanged,
                        description = h.Description,
                        changeTime = h.ChangeTime.ToString("yyyy-MM-dd HH:mm:ss")
                    })
                    .ToListAsync();

                var data = new
                {
                    userId = user.User_ID,
                    userName = user.User_name,
                    currentPoints = user.User_Wallet?.User_Point ?? 0,
                    recentHistory = recentHistory
                };

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<List<object>> GetWalletHistoryAsync()
        {
            return await _context.WalletHistory
                .Include(h => h.User)
                .OrderByDescending(h => h.ChangeTime)
                .Take(20)
                .Select(h => new
                {
                    logId = h.LogID,
                    userId = h.UserID,
                    userName = h.User.User_name,
                    changeType = h.ChangeType,
                    amount = h.PointsChanged,
                    description = h.Description,
                    changeTime = h.ChangeTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    status = "已完成"
                })
                .Cast<object>()
                .ToListAsync();
        }
    }
}
