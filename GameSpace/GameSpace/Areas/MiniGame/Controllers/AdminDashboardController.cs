using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminDashboardController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public AdminDashboardController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel();

            try
            {
                // 基本統計數據
                viewModel.TotalUsers = await _context.Users.CountAsync();
                viewModel.ActiveUsers = await _context.Users.CountAsync(u => u.IsActive);
                viewModel.TotalPets = await _context.Pets.CountAsync();
                viewModel.TotalMiniGames = await _context.MiniGames.CountAsync();
                viewModel.TotalCoupons = await _context.Coupons.CountAsync();
                viewModel.TotalEVouchers = await _context.EVouchers.CountAsync();

                // 今日統計
                var today = DateTime.Today;
                viewModel.TodaySignIns = await _context.UserSignInStats
                    .CountAsync(s => s.SignTime.Date == today);
                viewModel.TodayGames = await _context.MiniGames
                    .CountAsync(g => g.StartTime.Date == today);

                // 本月統計
                var thisMonth = new DateTime(today.Year, today.Month, 1);
                viewModel.ThisMonthSignIns = await _context.UserSignInStats
                    .CountAsync(s => s.SignTime >= thisMonth);
                viewModel.ThisMonthGames = await _context.MiniGames
                    .CountAsync(g => g.StartTime >= thisMonth);

                // 活躍用戶統計（最近7天）
                var sevenDaysAgo = today.AddDays(-7);
                viewModel.ActiveUsersLast7Days = await _context.Users
                    .CountAsync(u => u.LastLoginDate >= sevenDaysAgo);

                // 寵物統計
                viewModel.PetsWithMaxLevel = await _context.Pets
                    .CountAsync(p => p.PetLevel >= 10);
                viewModel.PetsWithMaxHappiness = await _context.Pets
                    .CountAsync(p => p.Happiness >= 100);

                // 錢包統計
                viewModel.TotalPointsInCirculation = await _context.UserWallets
                    .SumAsync(w => w.UserPoint);
                viewModel.AveragePointsPerUser = viewModel.TotalUsers > 0 
                    ? viewModel.TotalPointsInCirculation / viewModel.TotalUsers 
                    : 0;

                // 優惠券統計
                viewModel.UsedCoupons = await _context.Coupons
                    .CountAsync(c => c.IsUsed);
                viewModel.AvailableCoupons = await _context.Coupons
                    .CountAsync(c => !c.IsUsed);

                // 電子券統計
                viewModel.UsedEVouchers = await _context.EVouchers
                    .CountAsync(e => e.IsUsed);
                viewModel.AvailableEVouchers = await _context.EVouchers
                    .CountAsync(e => !e.IsUsed);

                // 最近活動
                viewModel.RecentSignIns = await _context.UserSignInStats
                    .Include(s => s.User)
                    .OrderByDescending(s => s.SignTime)
                    .Take(5)
                    .Select(s => new RecentActivityViewModel
                    {
                        UserName = s.User.User_name,
                        Activity = "簽到",
                        Time = s.SignTime,
                        PointsEarned = s.PointsEarned
                    })
                    .ToListAsync();

                viewModel.RecentGames = await _context.MiniGames
                    .Include(g => g.User)
                    .OrderByDescending(g => g.StartTime)
                    .Take(5)
                    .Select(g => new RecentActivityViewModel
                    {
                        UserName = g.User.User_name,
                        Activity = "小遊戲",
                        Time = g.StartTime,
                        PointsEarned = g.PointsEarned
                    })
                    .ToListAsync();

                // 圖表數據
                viewModel.SignInChartData = await GetSignInChartDataAsync();
                viewModel.GameChartData = await GetGameChartDataAsync();
                viewModel.PointsChartData = await GetPointsChartDataAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                TempData["ErrorMessage"] = $"載入儀表板時發生錯誤：{ex.Message}";
                return View(viewModel);
            }
        }

        private async Task<List<ChartData>> GetSignInChartDataAsync()
        {
            var today = DateTime.Today;
            var chartData = new List<ChartData>();

            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var count = await _context.UserSignInStats
                    .CountAsync(s => s.SignTime.Date == date);
                
                chartData.Add(new ChartData
                {
                    Label = date.ToString("MM/dd"),
                    Value = count
                });
            }

            return chartData;
        }

        private async Task<List<ChartData>> GetGameChartDataAsync()
        {
            var today = DateTime.Today;
            var chartData = new List<ChartData>();

            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var count = await _context.MiniGames
                    .CountAsync(g => g.StartTime.Date == date);
                
                chartData.Add(new ChartData
                {
                    Label = date.ToString("MM/dd"),
                    Value = count
                });
            }

            return chartData;
        }

        private async Task<List<ChartData>> GetPointsChartDataAsync()
        {
            var today = DateTime.Today;
            var chartData = new List<ChartData>();

            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var points = await _context.WalletHistories
                    .Where(w => w.ChangeTime.Date == date && w.ChangeType == "Point")
                    .SumAsync(w => w.ChangeAmount);
                
                chartData.Add(new ChartData
                {
                    Label = date.ToString("MM/dd"),
                    Value = (int)points
                });
            }

            return chartData;
        }
    }
}
