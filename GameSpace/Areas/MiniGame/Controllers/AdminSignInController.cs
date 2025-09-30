using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
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

        // GET: MiniGame/AdminSignIn
        public async Task<IActionResult> Index()
        {
            var viewModel = new SignInOverviewViewModel();

            // 統計數據
            viewModel.TotalSignIns = await _context.UserSignInStats.CountAsync();
            viewModel.TodaySignIns = await _context.UserSignInStats
                .Where(s => s.LastSignInDate.Date == DateTime.Today)
                .CountAsync();
            viewModel.ConsecutiveSignIns = await _context.UserSignInStats
                .Where(s => s.ConsecutiveDays > 0)
                .CountAsync();
            viewModel.MaxConsecutiveDays = await _context.UserSignInStats
                .MaxAsync(s => (int?)s.ConsecutiveDays) ?? 0;

            // 簽到規則設定
            viewModel.SignInRuleSettings = await _context.SignInRuleSettings
                .OrderBy(s => s.SettingID)
                .Select(s => new SignInRuleSettingsViewModel
                {
                    SettingID = s.SettingID,
                    SettingName = s.SettingName,
                    SettingValue = s.SettingValue,
                    Description = s.Description,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    CreatedByManagerId = s.CreatedByManagerId,
                    UpdatedByManagerId = s.UpdatedByManagerId
                })
                .ToListAsync();

            // 最近簽到記錄
            viewModel.RecentSignIns = await _context.UserSignInStats
                .Include(s => s.User)
                .Where(s => s.LastSignInDate.Date >= DateTime.Today.AddDays(-7))
                .OrderByDescending(s => s.LastSignInDate)
                .Take(50)
                .Select(s => new UserSignInStatsViewModel
                {
                    UserID = s.UserID,
                    UserName = s.User.User_name,
                    LastSignInDate = s.LastSignInDate,
                    ConsecutiveDays = s.ConsecutiveDays,
                    TotalSignInDays = s.TotalSignInDays,
                    LastRewardType = s.LastRewardType,
                    LastRewardAmount = s.LastRewardAmount
                })
                .ToListAsync();

            return View(viewModel);
        }

        // GET: MiniGame/AdminSignIn/SignInRuleSettings
        public async Task<IActionResult> SignInRuleSettings()
        {
            var settings = await _context.SignInRuleSettings
                .OrderBy(s => s.SettingID)
                .ToListAsync();

            return View(settings);
        }

        // POST: MiniGame/AdminSignIn/UpdateSignInRule
        [HttpPost]
        public async Task<IActionResult> UpdateSignInRule([FromBody] UpdateSignInRuleRequest request)
        {
            try
            {
                var setting = await _context.SignInRuleSettings
                    .FirstOrDefaultAsync(s => s.SettingID == request.SettingID);

                if (setting == null)
                {
                    return Json(new { success = false, message = "設定不存在" });
                }

                setting.SettingValue = request.SettingValue;
                setting.Description = request.Description;
                setting.UpdatedAt = DateTime.UtcNow;
                setting.UpdatedByManagerId = GetCurrentManagerId();

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "簽到規則更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"更新失敗: {ex.Message}" });
            }
        }

        // POST: MiniGame/AdminSignIn/CreateSignInRule
        [HttpPost]
        public async Task<IActionResult> CreateSignInRule([FromBody] CreateSignInRuleRequest request)
        {
            try
            {
                var setting = new SignInRuleSettings
                {
                    SettingName = request.SettingName,
                    SettingValue = request.SettingValue,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedByManagerId = GetCurrentManagerId()
                };

                _context.SignInRuleSettings.Add(setting);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "簽到規則創建成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"創建失敗: {ex.Message}" });
            }
        }

        // GET: MiniGame/AdminSignIn/MemberSignInRecords
        public async Task<IActionResult> MemberSignInRecords(int? page, string searchTerm)
        {
            var pageSize = 20;
            var pageNumber = page ?? 1;

            var query = _context.UserSignInStats
                .Include(s => s.User)
                .AsQueryable();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.User.User_name.Contains(searchTerm) ||
                                       s.User.User_Account.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var signInRecords = await query
                .OrderByDescending(s => s.LastSignInDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new UserSignInStatsViewModel
                {
                    UserID = s.UserID,
                    UserName = s.User.User_name,
                    UserAccount = s.User.User_Account,
                    LastSignInDate = s.LastSignInDate,
                    ConsecutiveDays = s.ConsecutiveDays,
                    TotalSignInDays = s.TotalSignInDays,
                    LastRewardType = s.LastRewardType,
                    LastRewardAmount = s.LastRewardAmount,
                    LastSignInTime = s.LastSignInDate
                })
                .ToListAsync();

            var viewModel = new MemberSignInRecordsViewModel
            {
                SignInRecords = signInRecords,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                TotalCount = totalCount,
                SearchTerm = searchTerm
            };

            return View(viewModel);
        }

        // GET: MiniGame/AdminSignIn/MemberSignInDetail/{userId}
        public async Task<IActionResult> MemberSignInDetail(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserSignInStats)
                .Include(u => u.UserIntroduce)
                .FirstOrDefaultAsync(u => u.User_ID == userId);

            if (user == null)
            {
                return NotFound();
            }

            var signInStats = user.UserSignInStats?.FirstOrDefault();
            if (signInStats == null)
            {
                return NotFound("用戶簽到統計不存在");
            }

            var viewModel = new MemberSignInDetailViewModel
            {
                UserID = user.User_ID,
                UserName = user.User_name,
                UserAccount = user.User_Account,
                NickName = user.UserIntroduce?.User_NickName ?? "",
                LastSignInDate = signInStats.LastSignInDate,
                ConsecutiveDays = signInStats.ConsecutiveDays,
                TotalSignInDays = signInStats.TotalSignInDays,
                LastRewardType = signInStats.LastRewardType,
                LastRewardAmount = signInStats.LastRewardAmount,
                SignInHistory = await GetSignInHistory(userId)
            };

            return View(viewModel);
        }

        // GET: MiniGame/AdminSignIn/SignInStatistics
        public async Task<IActionResult> SignInStatistics()
        {
            var viewModel = new SignInStatisticsViewModel();

            // 每日簽到統計
            viewModel.DailySignInStats = await _context.UserSignInStats
                .Where(s => s.LastSignInDate.Date >= DateTime.Today.AddDays(-30))
                .GroupBy(s => s.LastSignInDate.Date)
                .Select(g => new DailySignInStatViewModel
                {
                    Date = g.Key,
                    SignInCount = g.Count(),
                    NewUsers = g.Count(s => s.TotalSignInDays == 1),
                    ReturningUsers = g.Count(s => s.TotalSignInDays > 1)
                })
                .OrderBy(s => s.Date)
                .ToListAsync();

            // 連續簽到統計
            viewModel.ConsecutiveSignInStats = await _context.UserSignInStats
                .Where(s => s.ConsecutiveDays > 0)
                .GroupBy(s => s.ConsecutiveDays)
                .Select(g => new ConsecutiveSignInStatViewModel
                {
                    ConsecutiveDays = g.Key,
                    UserCount = g.Count()
                })
                .OrderByDescending(s => s.ConsecutiveDays)
                .ToListAsync();

            // 獎勵統計
            viewModel.RewardStats = await _context.UserSignInStats
                .Where(s => !string.IsNullOrEmpty(s.LastRewardType))
                .GroupBy(s => s.LastRewardType)
                .Select(g => new RewardStatViewModel
                {
                    RewardType = g.Key,
                    TotalAmount = g.Sum(s => s.LastRewardAmount ?? 0),
                    UserCount = g.Count()
                })
                .ToListAsync();

            return View(viewModel);
        }

        // POST: MiniGame/AdminSignIn/ResetUserSignIn
        [HttpPost]
        public async Task<IActionResult> ResetUserSignIn([FromBody] ResetUserSignInRequest request)
        {
            try
            {
                var signInStats = await _context.UserSignInStats
                    .FirstOrDefaultAsync(s => s.UserID == request.UserId);

                if (signInStats == null)
                {
                    return Json(new { success = false, message = "用戶簽到統計不存在" });
                }

                signInStats.ConsecutiveDays = 0;
                signInStats.LastSignInDate = DateTime.MinValue;
                signInStats.LastRewardType = null;
                signInStats.LastRewardAmount = null;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "用戶簽到記錄已重置" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"重置失敗: {ex.Message}" });
            }
        }

        // 輔助方法
        private async Task<List<SignInHistoryViewModel>> GetSignInHistory(int userId)
        {
            // 這裡可以根據實際需求實現簽到歷史記錄
            // 目前返回空列表，實際應用中可能需要從日誌表或其他地方獲取
            return new List<SignInHistoryViewModel>();
        }

        private int GetCurrentManagerId()
        {
            // 這裡應該從當前登入的管理員中獲取ID
            // 實際應用中需要從Claims或Session中獲取
            return 1; // 暫時返回1，實際應用中需要修改
        }
    }

    // 請求模型
    public class UpdateSignInRuleRequest
    {
        public int SettingID { get; set; }
        public string SettingValue { get; set; }
        public string Description { get; set; }
    }

    public class CreateSignInRuleRequest
    {
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
        public string Description { get; set; }
    }

    public class ResetUserSignInRequest
    {
        public int UserId { get; set; }
    }
}
