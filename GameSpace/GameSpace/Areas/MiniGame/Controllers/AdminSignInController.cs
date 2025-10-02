using Microsoft.AspNetCore.Mvc;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminSignInController : Controller
    {
        private readonly ISignInService _signInService;
        private readonly IUserService _userService;

        public AdminSignInController(ISignInService signInService, IUserService userService)
        {
            _signInService = signInService;
            _userService = userService;
        }

        // GET: AdminSignIn
        public async Task<IActionResult> Index(string searchTerm = "", string status = "", string sortBy = "date", int page = 1, int pageSize = 10)
        {
            // 獲取所有簽到記錄
            var allSignIns = await _signInService.GetAllSignInsAsync(1, 10000);

            // 日期範圍篩選
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "today")
                    allSignIns = allSignIns.Where(s => s.SignInTime.Date == DateTime.Today);
                else if (status == "week")
                    allSignIns = allSignIns.Where(s => s.SignInTime >= DateTime.Today.AddDays(-7));
                else if (status == "month")
                    allSignIns = allSignIns.Where(s => s.SignInTime >= DateTime.Today.AddDays(-30));
            }

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                allSignIns = allSignIns.Where(s => s.Users != null &&
                    (s.Users.User_Name.Contains(searchTerm) || s.Users.User_Account.Contains(searchTerm)));
            }

            // 排序
            allSignIns = sortBy switch
            {
                "user" => allSignIns.OrderBy(s => s.Users?.User_Name),
                "points" => allSignIns.OrderByDescending(s => s.PointsGained),
                "consecutive" => allSignIns.OrderByDescending(s => s.ConsecutiveDays),
                _ => allSignIns.OrderByDescending(s => s.SignInTime)
            };

            // 分頁
            var totalCount = allSignIns.Count();
            var pagedSignIns = allSignIns
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 轉換為 SignIn 格式（相容舊 View）
            var signInItems = pagedSignIns.Select(s => new SignIn
            {
                SignInId = s.LogID,
                UserId = s.UserID,
                SignInDate = s.SignInTime,
                RewardPoints = s.PointsGained,
                ConsecutiveDays = s.ConsecutiveDays,
                Users = s.Users,
                IsActive = true
            }).ToList();

            var viewModel = new AdminSignInIndexViewModel
            {
                SignIns = new PagedResult<SignIn>
                {
                    Items = signInItems,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                }
            };

            // 設定 ViewBag 統計數據
            var allSignInsList = await _signInService.GetAllSignInsAsync(1, 10000);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalSignIns = allSignInsList.Count();
            ViewBag.TodaySignIns = allSignInsList.Count(s => s.SignInTime.Date == DateTime.Today);
            ViewBag.WeekSignIns = allSignInsList.Count(s => s.SignInTime >= DateTime.Today.AddDays(-7));
            ViewBag.MonthSignIns = allSignInsList.Count(s => s.SignInTime >= DateTime.Today.AddDays(-30));

            return View(viewModel);
        }

        // GET: AdminSignIn/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var signInDetail = await _signInService.GetSignInDetailAsync(id.Value);

            if (signInDetail == null)
            {
                return NotFound();
            }

            // 轉換為 SignIn 格式
            var signIn = new SignIn
            {
                SignInId = signInDetail.LogID,
                UserId = signInDetail.UserID,
                SignInDate = signInDetail.SignInTime,
                RewardPoints = signInDetail.PointsGained,
                ConsecutiveDays = signInDetail.ConsecutiveDays,
                Users = signInDetail.Users,
                IsActive = true
            };

            return View(signIn);
        }

        // GET: AdminSignIn/UserHistory/5
        public async Task<IActionResult> UserHistory(int? userId)
        {
            if (userId == null)
            {
                return NotFound();
            }

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            var history = await _signInService.GetSignInHistoryAsync(userId.Value, 1, 100);
            var stats = await _signInService.GetUserSignInStatisticsAsync(userId.Value);

            ViewBag.User = user;
            ViewBag.Stats = stats;

            return View(history);
        }

        // 獲取簽到統計數據
        [HttpGet]
        public async Task<IActionResult> GetSignInStats()
        {
            var stats = await _signInService.GetGlobalSignInStatisticsAsync();

            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var allSignIns = await _signInService.GetAllSignInsAsync(1, 10000);

            return Json(new
            {
                today = allSignIns.Count(s => s.SignInTime.Date == today),
                thisWeek = allSignIns.Count(s => s.SignInTime >= weekStart),
                thisMonth = allSignIns.Count(s => s.SignInTime >= monthStart),
                total = stats.TotalSignIns
            });
        }

        // 獲取簽到趨勢圖表數據
        [HttpGet]
        public async Task<IActionResult> GetSignInTrendData(int days = 30)
        {
            var trendData = await _signInService.GetSignInTrendDataAsync(days);

            var data = trendData.Select(kvp => new
            {
                date = kvp.Key,
                count = kvp.Value
            }).ToList();

            return Json(data);
        }

        // 獲取用戶簽到排行榜
        [HttpGet]
        public async Task<IActionResult> GetSignInLeaderboard(int top = 10)
        {
            var leaderboard = await _signInService.GetSignInLeaderboardAsync(top);

            var data = leaderboard.Select(l => new
            {
                userId = l.UserId,
                userName = l.UserName,
                userAccount = l.UserName,
                signInCount = l.SignInCount,
                consecutiveDays = l.ConsecutiveDays
            }).ToList();

            return Json(data);
        }

        // 獲取簽到規則設定
        [HttpGet]
        public async Task<IActionResult> GetSignInRules()
        {
            var rules = await _signInService.GetAllSignInRulesAsync();
            return Json(new { success = true, data = rules });
        }

        // 新增簽到規則
        [HttpPost]
        public async Task<IActionResult> CreateSignInRule([FromBody] SignInRule rule)
        {
            if (rule == null)
            {
                return Json(new { success = false, message = "規則資料不能為空" });
            }

            var result = await _signInService.CreateSignInRuleAsync(rule);

            if (result)
            {
                return Json(new { success = true, message = "簽到規則建立成功" });
            }
            else
            {
                return Json(new { success = false, message = "建立失敗" });
            }
        }

        // 更新簽到規則
        [HttpPost]
        public async Task<IActionResult> UpdateSignInRule([FromBody] SignInRule rule)
        {
            if (rule == null)
            {
                return Json(new { success = false, message = "規則資料不能為空" });
            }

            var result = await _signInService.UpdateSignInRuleAsync(rule);

            if (result)
            {
                return Json(new { success = true, message = "簽到規則更新成功" });
            }
            else
            {
                return Json(new { success = false, message = "更新失敗" });
            }
        }

        // 刪除簽到規則
        [HttpPost]
        public async Task<IActionResult> DeleteSignInRule(int id)
        {
            var result = await _signInService.DeleteSignInRuleAsync(id);

            if (result)
            {
                return Json(new { success = true, message = "簽到規則刪除成功" });
            }
            else
            {
                return Json(new { success = false, message = "刪除失敗" });
            }
        }

        // 切換簽到規則狀態
        [HttpPost]
        public async Task<IActionResult> ToggleSignInRule(int id)
        {
            var result = await _signInService.ToggleSignInRuleStatusAsync(id);

            if (result)
            {
                return Json(new { success = true, message = "規則狀態已切換" });
            }
            else
            {
                return Json(new { success = false, message = "切換失敗" });
            }
        }

        // 獲取簽到獎勵規則
        [HttpGet]
        public async Task<IActionResult> GetSignInRewardRules()
        {
            var rules = await _signInService.GetSignInRewardRulesAsync();
            return Json(new { success = true, data = rules });
        }
    }
}
