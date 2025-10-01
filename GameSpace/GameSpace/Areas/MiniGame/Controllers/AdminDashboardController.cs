using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Data;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminDashboardController : Controller
    {
        private readonly GameSpaceContext _context;

        public AdminDashboardController(GameSpaceContext context)
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
                viewModel.TotalPets = await _context.Pet.CountAsync();
                viewModel.TotalMiniGames = await _context.MiniGame.CountAsync();
                viewModel.TotalCoupons = await _context.Coupon.CountAsync();
                viewModel.TotalEVouchers = await _context.EVoucher.CountAsync();

                // 今日數據
                var today = DateTime.Today;
                viewModel.TodaySignIns = await _context.SignIn.CountAsync(s => s.SignInDate.Date == today);
                viewModel.TodayGamePlays = await _context.GamePlayRecord.CountAsync(g => g.PlayTime.Date == today);

                // 點數統計
                viewModel.TotalPointsEarned = await _context.Wallet
                    .Where(w => w.TransactionType == "earn")
                    .SumAsync(w => w.Amount);
                viewModel.TotalPointsSpent = await _context.Wallet
                    .Where(w => w.TransactionType == "spend")
                    .SumAsync(w => w.Amount);

                // 圖表數據 - 用戶成長（過去30天）
                viewModel.UserGrowthData = await GetUserGrowthData();
                
                // 圖表數據 - 遊戲遊玩次數（過去7天）
                viewModel.GamePlayData = await GetGamePlayData();
                
                // 圖表數據 - 點數使用（過去7天）
                viewModel.PointsData = await GetPointsData();
            }
            catch (Exception ex)
            {
                // 記錄錯誤但不中斷頁面載入
                Console.WriteLine($"Dashboard data loading error: {ex.Message}");
            }

            return View(viewModel);
        }

        private async Task<List<ChartData>> GetUserGrowthData()
        {
            var data = new List<ChartData>();
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-30);

            for (int i = 0; i < 30; i++)
            {
                var date = startDate.AddDays(i);
                var count = await _context.Users.CountAsync(u => u.User_registration_date.Date <= date);
                data.Add(new ChartData
                {
                    Label = date.ToString("MM/dd"),
                    Count = count
                });
            }

            return data;
        }

        private async Task<List<ChartData>> GetGamePlayData()
        {
            var data = new List<ChartData>();
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-7);

            for (int i = 0; i < 7; i++)
            {
                var date = startDate.AddDays(i);
                var count = await _context.GamePlayRecord.CountAsync(g => g.PlayTime.Date == date);
                data.Add(new ChartData
                {
                    Label = date.ToString("MM/dd"),
                    Count = count
                });
            }

            return data;
        }

        private async Task<List<ChartData>> GetPointsData()
        {
            var data = new List<ChartData>();
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-7);

            for (int i = 0; i < 7; i++)
            {
                var date = startDate.AddDays(i);
                var earned = await _context.Wallet
                    .Where(w => w.TransactionDate.Date == date && w.TransactionType == "earn")
                    .SumAsync(w => w.Amount);
                var spent = await _context.Wallet
                    .Where(w => w.TransactionDate.Date == date && w.TransactionType == "spend")
                    .SumAsync(w => w.Amount);
                
                data.Add(new ChartData
                {
                    Label = date.ToString("MM/dd"),
                    Value = earned - spent
                });
            }

            return data;
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(string chartType)
        {
            try
            {
                List<ChartData> data = chartType switch
                {
                    "userGrowth" => await GetUserGrowthData(),
                    "gamePlay" => await GetGamePlayData(),
                    "points" => await GetPointsData(),
                    _ => new List<ChartData>()
                };

                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentActivity()
        {
            try
            {
                var recentActivities = new List<object>();

                // 最近註冊的用戶
                var recentUsers = await _context.Users
                    .OrderByDescending(u => u.User_registration_date)
                    .Take(5)
                    .Select(u => new
                    {
                        Type = "user",
                        Message = $"新用戶 {u.User_name} 註冊",
                        Time = u.User_registration_date,
                        Icon = "fas fa-user-plus"
                    })
                    .ToListAsync();

                // 最近的遊戲遊玩記錄
                var recentGamePlays = await _context.GamePlayRecord
                    .Include(g => g.MiniGame)
                    .Include(g => g.Users)
                    .OrderByDescending(g => g.PlayTime)
                    .Take(5)
                    .Select(g => new
                    {
                        Type = "game",
                        Message = $"{g.Users.User_name} 遊玩了 {g.MiniGame.GameName}",
                        Time = g.PlayTime,
                        Icon = "fas fa-gamepad"
                    })
                    .ToListAsync();

                // 最近的簽到記錄
                var recentSignIns = await _context.SignIn
                    .Include(s => s.Users)
                    .OrderByDescending(s => s.SignInDate)
                    .Take(5)
                    .Select(s => new
                    {
                        Type = "signin",
                        Message = $"{s.Users.User_name} 完成簽到",
                        Time = s.SignInDate,
                        Icon = "fas fa-calendar-check"
                    })
                    .ToListAsync();

                recentActivities.AddRange(recentUsers);
                recentActivities.AddRange(recentGamePlays);
                recentActivities.AddRange(recentSignIns);

                // 按時間排序並取前10個
                var sortedActivities = recentActivities
                    .OrderByDescending(a => ((dynamic)a).Time)
                    .Take(10)
                    .ToList();

                return Json(sortedActivities);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
