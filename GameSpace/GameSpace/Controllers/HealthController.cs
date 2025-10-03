using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;

namespace GameSpace.Controllers
{
    [AllowAnonymous]
    public class HealthController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public HealthController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        [HttpGet("/healthz/db")]
        public async Task<IActionResult> DatabaseHealth()
        {
            try
            {
                // 測試資料庫連線
                await _context.Database.OpenConnectionAsync();
                await _context.Database.CloseConnectionAsync();

                // 執行簡單查詢測試
                var userCount = await _context.Users.CountAsync();
                var petCount = await _context.Pets.CountAsync();
                var signInCount = await _context.UserSignInStats.CountAsync();
                var miniGameCount = await _context.MiniGames.CountAsync();

                return Json(new
                {
                    status = "ok",
                    timestamp = DateTime.UtcNow,
                    database = "connected",
                    tables = new
                    {
                        users = userCount,
                        pets = petCount,
                        signins = signInCount,
                        minigames = miniGameCount
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message,
                    database = "disconnected"
                });
            }
        }

        [HttpGet("/healthz")]
        public IActionResult Health()
        {
            return Json(new
            {
                status = "ok",
                timestamp = DateTime.UtcNow,
                service = "GameSpace MiniGame Area"
            });
        }
    }
}

