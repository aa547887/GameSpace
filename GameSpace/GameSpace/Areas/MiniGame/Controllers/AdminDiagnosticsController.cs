using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public class AdminDiagnosticsController : Controller
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminDiagnosticsController(GameSpacedatabaseContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Index()
        {
            try
            {
                var diagnostics = new DiagnosticsViewModel
                {
                    Environment = _environment.EnvironmentName,
                    ApplicationName = Assembly.GetExecutingAssembly().GetName().Name,
                    Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown"
                };

                return View(diagnostics);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入診斷資訊時發生錯誤：{ex.Message}";
                return View(new DiagnosticsViewModel());
            }
        }

        public async Task<IActionResult> DatabaseStatus()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var connectionString = _context.Database.GetConnectionString();
                
                var status = new
                {
                    Status = canConnect ? "Connected" : "Disconnected",
                    ConnectionString = connectionString?.Substring(0, Math.Min(50, connectionString.Length)) + "...",
                    Timestamp = DateTime.Now
                };

                return Json(status);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    Status = "Error", 
                    Message = ex.Message,
                    Timestamp = DateTime.Now
                });
            }
        }

        public async Task<IActionResult> SystemInfo()
        {
            try
            {
                var info = new
                {
                    MachineName = Environment.MachineName,
                    OSVersion = Environment.OSVersion.ToString(),
                    ProcessorCount = Environment.ProcessorCount,
                    WorkingSet = Environment.WorkingSet,
                    TickCount = Environment.TickCount,
                    UserDomainName = Environment.UserDomainName,
                    UserName = Environment.UserName,
                    Version = Environment.Version.ToString(),
                    Timestamp = DateTime.Now
                };

                return Json(info);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    Error = ex.Message,
                    Timestamp = DateTime.Now
                });
            }
        }

        public async Task<IActionResult> DatabaseStats()
        {
            try
            {
                var stats = new
                {
                    Users = await _context.Users.CountAsync(),
                    UserWallets = await _context.UserWallets.CountAsync(),
                    Pets = await _context.Pets.CountAsync(),
                    MiniGames = await _context.MiniGames.CountAsync(),
                    Coupons = await _context.Coupons.CountAsync(),
                    EVouchers = await _context.Evouchers.CountAsync(),
                    WalletHistories = await _context.WalletHistories.CountAsync(),
                    UserSignInStats = await _context.UserSignInStats.CountAsync(),
                    Timestamp = DateTime.Now
                };

                return Json(stats);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    Error = ex.Message,
                    Timestamp = DateTime.Now
                });
            }
        }
    }

    // ViewModels
    public class DiagnosticsViewModel
    {
        public string Environment { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }
}
