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
    [Authorize(AuthenticationSchemes = "AdminCookie")]
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
            return View();
        }

        public async Task<IActionResult> DatabaseStatus()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                return Json(new { Status = canConnect ? "Connected" : "Disconnected" });
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Error", Message = ex.Message });
            }
        }
    }
}
