using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Route("MiniGame/[controller]")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dashboardData = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                ActiveUsers = await _context.Users.Where(u => u.User_LockoutEnd == null || u.User_LockoutEnd < DateTime.UtcNow).CountAsync(),
                TotalGamesPlayed = await _context.MiniGame.CountAsync(),
                NewSignInsToday = await _context.UserSignInStats
                    .Where(s => s.SignTime.Date == DateTime.Today)
                    .CountAsync(),
                RecentActivity = await GetRecentActivityAsync()
            };

            return View(dashboardData);
        }

        [HttpGet]
        public async Task<JsonResult> GetDashboardData()
        {
            try
            {
                var data = new
                {
                    totalUsers = await _context.Users.CountAsync(),
                    activeUsers = await _context.Users.Where(u => u.User_Status == true).CountAsync(),
                    totalGamesPlayed = await _context.MiniGame.CountAsync(),
                    todaySignIns = await _context.UserSignInStats
                        .Where(s => s.SignTime.Date == DateTime.Today)
                        .CountAsync(),
                    activePets = await _context.Pet.Where(p => p.Health > 0).CountAsync(),
                    recentActivity = await GetRecentActivityAsync()
                };

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<List<ActivityLogViewModel>> GetRecentActivityAsync()
        {
            var activities = new List<ActivityLogViewModel>();

            // Recent sign-ins
            var recentSignIns = await _context.UserSignInStats
                .Include(s => s.User)
                .OrderByDescending(s => s.SignTime)
                .Take(10)
                .Select(s => new ActivityLogViewModel
                {
                    Timestamp = s.SignTime,
                    Module = "簽到系統",
                    Operation = "每日簽到",
                    UserName = s.User.User_name,
                    Status = "成功"
                })
                .ToListAsync();

            activities.AddRange(recentSignIns);

            // Recent games
            var recentGames = await _context.MiniGame
                .Include(g => g.User)
                .OrderByDescending(g => g.StartTime)
                .Take(10)
                .Select(g => new ActivityLogViewModel
                {
                    Timestamp = g.StartTime,
                    Module = "小遊戲系統",
                    Operation = "遊戲挑戦",
                    UserName = g.User.User_name,
                    Status = g.Result
                })
                .ToListAsync();

            activities.AddRange(recentGames);

            return activities.OrderByDescending(a => a.Timestamp).Take(15).ToList();
        }
    }
}
