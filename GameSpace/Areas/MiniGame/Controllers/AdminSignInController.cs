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
    public class AdminSignInController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminSignInController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MiniGame/AdminSignIn/SignInRuleSettings
        public IActionResult SignInRuleSettings()
        {
            return View();
        }

        // AJAX endpoint for sign-in rule settings
        [HttpGet]
        public async Task<IActionResult> GetSignInRuleSettings()
        {
            try
            {
                var settings = await _context.SignInRuleSettings
                    .OrderBy(s => s.SettingName)
                    .ToListAsync();

                return Json(new { success = true, data = settings });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: MiniGame/AdminSignIn/UpdateSignInRuleSettings
        [HttpPost]
        public async Task<IActionResult> UpdateSignInRuleSettings(int settingId, string settingValue, string description)
        {
            try
            {
                var setting = await _context.SignInRuleSettings
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

                return Json(new { success = true, message = "設定更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminSignIn/ViewMemberSignInRecords
        public IActionResult ViewMemberSignInRecords()
        {
            return View();
        }

        // AJAX endpoint for member sign-in records
        [HttpGet]
        public async Task<IActionResult> GetMemberSignInRecords(string searchTerm = "", DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.UserSignInStats
                    .Include(usis => usis.User)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(usis => 
                        usis.User.User_name.Contains(searchTerm) || 
                        usis.User.User_Account.Contains(searchTerm));
                }

                if (startDate.HasValue)
                {
                    query = query.Where(usis => usis.SignInDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(usis => usis.SignInDate <= endDate.Value);
                }

                var records = await query
                    .OrderByDescending(usis => usis.SignInDate)
                    .Select(usis => new
                    {
                        usis.SignInID,
                        usis.User_ID,
                        usis.User.User_name,
                        usis.User.User_Account,
                        usis.SignInDate,
                        usis.PointsEarned,
                        usis.PetExpEarned,
                        usis.CouponEarned,
                        usis.CreatedAt
                    })
                    .ToListAsync();

                return Json(new { success = true, data = records });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminSignIn/CreateSignInRule
        public IActionResult CreateSignInRule()
        {
            return View();
        }

        // POST: MiniGame/AdminSignIn/CreateSignInRule
        [HttpPost]
        public async Task<IActionResult> CreateSignInRule(string settingName, string settingValue, string description)
        {
            try
            {
                var setting = new SignInRuleSettings
                {
                    SettingName = settingName,
                    SettingValue = settingValue,
                    Description = description,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedByManagerId = 1, // TODO: 從認證中獲取管理員ID
                    UpdatedByManagerId = 1
                };

                _context.SignInRuleSettings.Add(setting);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "簽到規則創建成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
