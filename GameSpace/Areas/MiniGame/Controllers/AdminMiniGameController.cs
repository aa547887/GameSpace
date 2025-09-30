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
    public class AdminMiniGameController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminMiniGameController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MiniGame/AdminMiniGame/GameRuleSettings
        public IActionResult GameRuleSettings()
        {
            return View();
        }

        // AJAX endpoint for game rule settings
        [HttpGet]
        public async Task<IActionResult> GetGameRuleSettings()
        {
            try
            {
                var settings = await _context.MiniGameRuleSettings
                    .OrderBy(s => s.SettingName)
                    .ToListAsync();

                return Json(new { success = true, data = settings });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: MiniGame/AdminMiniGame/UpdateGameRuleSettings
        [HttpPost]
        public async Task<IActionResult> UpdateGameRuleSettings(int settingId, string settingValue, string description)
        {
            try
            {
                var setting = await _context.MiniGameRuleSettings
                    .FirstOrDefaultAsync(s => s.SettingID == settingId);

                if (setting == null)
                {
                    return Json(new { success = false, message = "找不到該設定" });
                }

                setting.SettingValue = settingValue;
                setting.Description = description;
                setting.UpdatedAt = DateTime.Now;
                setting.UpdatedByManagerId = 1; // TODO: 從認證中獲取管理員ID

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "遊戲規則更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminMiniGame/ViewMemberGameRecords
        public IActionResult ViewMemberGameRecords()
        {
            return View();
        }

        // AJAX endpoint for member game records
        [HttpGet]
        public async Task<IActionResult> GetMemberGameRecords(string searchTerm = "", DateTime? startDate = null, DateTime? endDate = null, string gameResult = "")
        {
            try
            {
                var query = _context.MiniGames
                    .Include(mg => mg.User)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(mg => 
                        mg.User.User_name.Contains(searchTerm) || 
                        mg.User.User_Account.Contains(searchTerm));
                }

                if (startDate.HasValue)
                {
                    query = query.Where(mg => mg.StartTime >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(mg => mg.EndTime <= endDate.Value);
                }

                if (!string.IsNullOrEmpty(gameResult))
                {
                    query = query.Where(mg => mg.Result == gameResult);
                }

                var gameRecords = await query
                    .OrderByDescending(mg => mg.StartTime)
                    .Select(mg => new
                    {
                        mg.GameID,
                        mg.User_ID,
                        mg.User.User_name,
                        mg.User.User_Account,
                        mg.SessionId,
                        mg.StartTime,
                        mg.EndTime,
                        mg.Result,
                        mg.PointsEarned,
                        mg.PetExpEarned,
                        mg.CouponEarned,
                        mg.CreatedAt
                    })
                    .ToListAsync();

                return Json(new { success = true, data = gameRecords });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminMiniGame/CreateGameRule
        public IActionResult CreateGameRule()
        {
            return View();
        }

        // POST: MiniGame/AdminMiniGame/CreateGameRule
        [HttpPost]
        public async Task<IActionResult> CreateGameRule(string settingName, string settingValue, string description)
        {
            try
            {
                var setting = new MiniGameRuleSettings
                {
                    SettingName = settingName,
                    SettingValue = settingValue,
                    Description = description,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedByManagerId = 1, // TODO: 從認證中獲取管理員ID
                    UpdatedByManagerId = 1
                };

                _context.MiniGameRuleSettings.Add(setting);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "遊戲規則創建成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminMiniGame/GameStatistics
        public IActionResult GameStatistics()
        {
            return View();
        }

        // AJAX endpoint for game statistics
        [HttpGet]
        public async Task<IActionResult> GetGameStatistics(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.MiniGames.AsQueryable();

                if (startDate.HasValue)
                {
                    query = query.Where(mg => mg.StartTime >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(mg => mg.EndTime <= endDate.Value);
                }

                var statistics = new
                {
                    TotalGames = await query.CountAsync(),
                    WinGames = await query.CountAsync(mg => mg.Result == "win"),
                    LoseGames = await query.CountAsync(mg => mg.Result == "lose"),
                    AbortGames = await query.CountAsync(mg => mg.Result == "abort"),
                    TotalPointsEarned = await query.SumAsync(mg => mg.PointsEarned ?? 0),
                    TotalPetExpEarned = await query.SumAsync(mg => mg.PetExpEarned ?? 0),
                    TotalCouponsEarned = await query.SumAsync(mg => mg.CouponEarned ?? 0),
                    AverageGameDuration = await query
                        .Where(mg => mg.EndTime.HasValue)
                        .Select(mg => EF.Functions.DateDiffMinute(mg.StartTime, mg.EndTime.Value))
                        .DefaultIfEmpty(0)
                        .AverageAsync()
                };

                return Json(new { success = true, data = statistics });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
