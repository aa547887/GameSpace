using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MiniGame/Admin
        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel();

            // 統計數據
            viewModel.TotalUsers = await _context.Users.CountAsync();
            viewModel.ActiveUsers = await _context.Users
                .Where(u => u.UserRights.User_Status == true)
                .CountAsync();
            viewModel.TotalGamesPlayed = await _context.MiniGames.CountAsync();
            viewModel.NewSignInsToday = await _context.UserSignInStats
                .Where(s => s.SignTime.Date == DateTime.Today)
                .CountAsync();
            viewModel.TotalPets = await _context.Pets.CountAsync();
            viewModel.ActivePets = await _context.Pets
                .Where(p => p.Health > 0)
                .CountAsync();
            viewModel.TotalPointsIssued = await _context.WalletHistories
                .Where(w => w.PointsChanged > 0)
                .SumAsync(w => w.PointsChanged);
            viewModel.TotalCouponsIssued = await _context.Coupons.CountAsync();
            viewModel.TotalEVouchersIssued = await _context.EVouchers.CountAsync();

            // 最近活動
            viewModel.RecentActivity = await _context.WalletHistories
                .Include(w => w.User)
                .OrderByDescending(w => w.ChangeTime)
                .Take(10)
                .Select(w => new ActivityLogViewModel
                {
                    Timestamp = w.ChangeTime,
                    Module = "錢包",
                    Operation = w.ChangeType,
                    UserName = w.User.User_name,
                    Status = "成功",
                    Details = w.Description
                })
                .ToListAsync();

            // 活躍用戶
            viewModel.TopUsers = await _context.Users
                .Include(u => u.UserWallet)
                .Include(u => u.UserSignInStats)
                .Include(u => u.MiniGames)
                .Include(u => u.Pet)
                .OrderByDescending(u => u.UserWallet.User_Point)
                .Take(10)
                .Select(u => new UserStatsViewModel
                {
                    UserID = u.User_ID,
                    UserName = u.User_name,
                    NickName = u.UserIntroduce.User_NickName,
                    TotalPoints = u.UserWallet.User_Point,
                    SignInCount = u.UserSignInStats.Count(),
                    GamesPlayed = u.MiniGames.Count(),
                    PetLevel = u.Pet != null ? u.Pet.Level : 0,
                    LastActive = u.MiniGames.Any() ? u.MiniGames.Max(mg => mg.StartTime) : DateTime.MinValue
                })
                .ToListAsync();

            return View(viewModel);
        }

        // GET: MiniGame/Admin/GetDashboardData
        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var data = new
                {
                    totalUsers = await _context.Users.CountAsync(),
                    activeUsers = await _context.Users
                        .Where(u => u.UserRights.User_Status == true)
                        .CountAsync(),
                    totalGamesPlayed = await _context.MiniGames.CountAsync(),
                    todaySignIns = await _context.UserSignInStats
                        .Where(s => s.SignTime.Date == DateTime.Today)
                        .CountAsync(),
                    totalPets = await _context.Pets.CountAsync(),
                    activePets = await _context.Pets
                        .Where(p => p.Health > 0)
                        .CountAsync(),
                    totalPointsIssued = await _context.WalletHistories
                        .Where(w => w.PointsChanged > 0)
                        .SumAsync(w => w.PointsChanged),
                    totalCouponsIssued = await _context.Coupons.CountAsync(),
                    totalEVouchersIssued = await _context.EVouchers.CountAsync(),
                    recentActivity = await _context.WalletHistories
                        .Include(w => w.User)
                        .OrderByDescending(w => w.ChangeTime)
                        .Take(10)
                        .Select(w => new
                        {
                            timestamp = w.ChangeTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            module = "錢包",
                            operation = w.ChangeType,
                            userName = w.User.User_name,
                            status = "成功",
                            details = w.Description
                        })
                        .ToListAsync()
                };

                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/Admin/UserManagement
        public async Task<IActionResult> UserManagement()
        {
            var users = await _context.Users
                .Include(u => u.UserIntroduce)
                .Include(u => u.UserRights)
                .Include(u => u.UserWallet)
                .Include(u => u.UserSignInStats)
                .Include(u => u.MiniGames)
                .Include(u => u.Pet)
                .OrderByDescending(u => u.User_ID)
                .ToListAsync();

            return View(users);
        }

        // POST: MiniGame/Admin/ToggleUserStatus
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int userId)
        {
            try
            {
                var userRights = await _context.UserRights
                    .FirstOrDefaultAsync(ur => ur.User_Id == userId);

                if (userRights != null)
                {
                    userRights.User_Status = !userRights.User_Status;
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "用戶狀態已更新" });
                }

                return Json(new { success = false, message = "找不到用戶" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: MiniGame/Admin/AdjustUserPoints
        [HttpPost]
        public async Task<IActionResult> AdjustUserPoints(int userId, int points, string reason)
        {
            try
            {
                var userWallet = await _context.UserWallets
                    .FirstOrDefaultAsync(uw => uw.User_Id == userId);

                if (userWallet != null)
                {
                    userWallet.User_Point += points;

                    // 記錄異動
                    var walletHistory = new WalletHistory
                    {
                        UserID = userId,
                        ChangeType = points > 0 ? "管理員發放" : "管理員扣除",
                        PointsChanged = points,
                        Description = reason,
                        ChangeTime = DateTime.Now
                    };

                    _context.WalletHistories.Add(walletHistory);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "點數調整成功" });
                }

                return Json(new { success = false, message = "找不到用戶錢包" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
