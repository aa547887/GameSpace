using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Linq;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using SignInRuleDto = GameSpace.Areas.MiniGame.Services.SignInRule;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme)]
    public class AdminSignInController : MiniGameBaseController
    {
        private readonly ISignInService _signInService;
        private readonly IUserService _userService;

        private const string DailyPointsSettingKey = "MiniGame.SignIn.DailyPoints";
        private const string WeeklyBonusPointsSettingKey = "MiniGame.SignIn.WeeklyBonusPoints";
        private const string MonthlyBonusPointsSettingKey = "MiniGame.SignIn.MonthlyBonusPoints";
        private const string ConsecutiveRequirementSettingKey = "MiniGame.SignIn.ConsecutiveDaysRequired";
        private const string RuleDescriptionSettingKey = "MiniGame.SignIn.Description";
        private const int MonthlyRewardThreshold = 30;
        private const int DefaultDailyPoints = 10;
        private const int DefaultWeeklyPoints = 50;
        private const int DefaultMonthlyPoints = 200;
        private const int DefaultConsecutiveRequirement = 7;

        public AdminSignInController(GameSpacedatabaseContext context, ISignInService signInService, IUserService userService) : base(context)
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
                    allSignIns = allSignIns.Where(s => s.SignTime.Date == DateTime.Today);
                else if (status == "week")
                    allSignIns = allSignIns.Where(s => s.SignTime >= DateTime.Today.AddDays(-7));
                else if (status == "month")
                    allSignIns = allSignIns.Where(s => s.SignTime >= DateTime.Today.AddDays(-30));
            }

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                allSignIns = allSignIns.Where(s => s.Users != null &&
                    (s.Users.UserName.Contains(searchTerm) || s.Users.UserAccount.Contains(searchTerm)));
            }

            // 排序
            allSignIns = sortBy switch
            {
                "user" => allSignIns.OrderBy(s => s.Users?.UserName),
                "points" => allSignIns.OrderByDescending(s => s.PointsGained),
                "consecutive" => allSignIns.OrderByDescending(s => s.ConsecutiveDays),
                _ => allSignIns.OrderByDescending(s => s.SignTime)
            };

            // 分頁
            var totalCount = allSignIns.Count();
            var pagedSignIns = allSignIns
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new AdminSignInIndexViewModel
            {
                SignIns = new PagedResult<UserSignInStats>
                {
                    Items = pagedSignIns,
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
            ViewBag.TodaySignIns = allSignInsList.Count(s => s.SignTime.Date == DateTime.Today);
            ViewBag.WeekSignIns = allSignInsList.Count(s => s.SignTime >= DateTime.Today.AddDays(-7));
            ViewBag.MonthSignIns = allSignInsList.Count(s => s.SignTime >= DateTime.Today.AddDays(-30));

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

            return View(signInDetail);
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
                today = allSignIns.Count(s => s.SignTime.Date == today),
                thisWeek = allSignIns.Count(s => s.SignTime >= weekStart),
                thisMonth = allSignIns.Count(s => s.SignTime >= monthStart),
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
        public async Task<IActionResult> CreateSignInRule([FromBody] GameSpace.Areas.MiniGame.Services.SignInRule rule)
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
        public async Task<IActionResult> UpdateSignInRule([FromBody] GameSpace.Areas.MiniGame.Services.SignInRule rule)
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

        #region Statistics and Reports

        // GET: AdminSignIn/Statistics
        public async Task<IActionResult> Statistics(DateTime? startDate, DateTime? endDate)
        {
            // 設定預設日期範圍（最近30天）
            startDate ??= DateTime.Today.AddDays(-30);
            endDate ??= DateTime.Today;

            // 取得統計數據
            var allSignIns = await _signInService.GetAllSignInsAsync(1, 100000);
            var signInsInRange = allSignIns.Where(s => s.SignTime.Date >= startDate.Value.Date && s.SignTime.Date <= endDate.Value.Date).ToList();

            // 基本統計
            var stats = new
            {
                TotalSignIns = signInsInRange.Count,
                UniqueUsers = signInsInRange.Select(s => s.UserID).Distinct().Count(),
                TotalPointsGained = signInsInRange.Sum(s => s.PointsGained),
                AveragePointsPerSignIn = signInsInRange.Any() ? signInsInRange.Average(s => s.PointsGained) : 0,
                MaxConsecutiveDays = signInsInRange.Any() ? signInsInRange.Max(s => s.ConsecutiveDays) : 0,
                AverageConsecutiveDays = signInsInRange.Any() ? signInsInRange.Average(s => s.ConsecutiveDays) : 0
            };

            // 每日簽到趨勢
            var dailyTrend = signInsInRange
                .GroupBy(s => s.SignTime.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count(),
                    UniqueUsers = g.Select(s => s.UserID).Distinct().Count(),
                    TotalPoints = g.Sum(s => s.PointsGained)
                })
                .OrderBy(d => d.Date)
                .ToList();

            // 星期分佈
            var weekdayDistribution = signInsInRange
                .GroupBy(s => s.SignTime.DayOfWeek)
                .Select(g => new
                {
                    DayOfWeek = g.Key.ToString(),
                    Count = g.Count()
                })
                .OrderBy(d => (int)Enum.Parse<DayOfWeek>(d.DayOfWeek))
                .ToList();

            // 時段分佈（按小時）
            var hourlyDistribution = signInsInRange
                .GroupBy(s => s.SignTime.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .OrderBy(h => h.Hour)
                .ToList();

            // 連續簽到天數分佈
            var consecutiveDaysDistribution = signInsInRange
                .GroupBy(s => s.ConsecutiveDays)
                .Select(g => new
                {
                    ConsecutiveDays = g.Key,
                    Count = g.Count()
                })
                .OrderBy(c => c.ConsecutiveDays)
                .ToList();

            // 獎勵點數分佈
            var pointsDistribution = signInsInRange
                .GroupBy(s => s.PointsGained / 10 * 10) // 按10點分組
                .Select(g => new
                {
                    PointsRange = $"{g.Key}-{g.Key + 9}",
                    Count = g.Count()
                })
                .OrderBy(p => p.PointsRange)
                .ToList();

            ViewBag.StartDate = startDate.Value;
            ViewBag.EndDate = endDate.Value;
            ViewBag.Stats = stats;
            ViewBag.DailyTrend = dailyTrend;
            ViewBag.WeekdayDistribution = weekdayDistribution;
            ViewBag.HourlyDistribution = hourlyDistribution;
            ViewBag.ConsecutiveDaysDistribution = consecutiveDaysDistribution;
            ViewBag.PointsDistribution = pointsDistribution;

            return View();
        }

        // GET: AdminSignIn/UserStatistics
        public async Task<IActionResult> UserStatistics(int? userId, int page = 1, int pageSize = 20)
        {
            if (userId.HasValue)
            {
                // 顯示特定用戶的簽到統計
                var user = await _userService.GetUserByIdAsync(userId.Value);
                if (user == null)
                {
                    return NotFound();
                }

                var userSignIns = await _signInService.GetSignInHistoryAsync(userId.Value, 1, 1000);
                var userStats = await _signInService.GetUserSignInStatisticsAsync(userId.Value);

                ViewBag.User = user;
                ViewBag.UserSignIns = userSignIns;
                ViewBag.UserStats = userStats;

                return View("UserStatisticsDetail");
            }
            else
            {
                // 顯示所有用戶的簽到統計列表
                var allSignIns = await _signInService.GetAllSignInsAsync(1, 100000);

            var userStats = allSignIns
                    .GroupBy(s => new { s.UserID, s.Users })
                    .Select(g => new
                    {
                        UserId = g.Key.UserID,
                        User = g.Key.Users,
                        TotalSignIns = g.Count(),
                        TotalPoints = g.Sum(s => s.PointsGained),
                        MaxConsecutive = g.Max(s => s.ConsecutiveDays),
                        LastSignIn = g.Max(s => s.SignTime),
                        AveragePoints = g.Average(s => s.PointsGained)
                    })
                    .OrderByDescending(u => u.TotalSignIns)
                    .ToList();

                var totalCount = userStats.Count;
                var pagedStats = userStats
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.UserStats = pagedStats;
                ViewBag.TotalCount = totalCount;
                ViewBag.PageNumber = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                return View();
            }
        }

        // GET: AdminSignIn/MonthlyReport
        public async Task<IActionResult> MonthlyReport(int? year, int? month)
        {
            year ??= DateTime.Today.Year;
            month ??= DateTime.Today.Month;

            var startDate = new DateTime(year.Value, month.Value, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var allSignIns = await _signInService.GetAllSignInsAsync(1, 100000);
            var monthlySignIns = allSignIns
                .Where(s => s.SignTime >= startDate && s.SignTime <= endDate)
                .ToList();

            // 每日統計
            var dailyStats = new List<object>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var daySignIns = monthlySignIns.Where(s => s.SignTime.Date == date).ToList();
                dailyStats.Add(new
                {
                    Date = date,
                    DayOfWeek = date.DayOfWeek.ToString(),
                    TotalSignIns = daySignIns.Count,
                    UniqueUsers = daySignIns.Select(s => s.UserID).Distinct().Count(),
                    TotalPoints = daySignIns.Sum(s => s.PointsGained)
                });
            }

            // 月度總結
            var monthlySummary = new
            {
                Year = year.Value,
                Month = month.Value,
                TotalSignIns = monthlySignIns.Count,
                UniqueUsers = monthlySignIns.Select(s => s.UserID).Distinct().Count(),
                TotalPoints = monthlySignIns.Sum(s => s.PointsGained),
                AverageSignInsPerDay = monthlySignIns.Count / (double)DateTime.DaysInMonth(year.Value, month.Value),
                PeakDay = dailyStats.OrderByDescending(d => ((dynamic)d).TotalSignIns).FirstOrDefault()
            };

            ViewBag.Year = year.Value;
            ViewBag.Month = month.Value;
            ViewBag.DailyStats = dailyStats;
            ViewBag.MonthlySummary = monthlySummary;

            return View();
        }

        // GET: AdminSignIn/ExportReport
        public async Task<IActionResult> ExportReport(DateTime? startDate, DateTime? endDate, string format = "csv")
        {
            startDate ??= DateTime.Today.AddDays(-30);
            endDate ??= DateTime.Today;

            var allSignIns = await _signInService.GetAllSignInsAsync(1, 100000);
            var signInsInRange = allSignIns
                .Where(s => s.SignTime.Date >= startDate.Value.Date && s.SignTime.Date <= endDate.Value.Date)
                .OrderByDescending(s => s.SignTime)
                .ToList();

            if (format.ToLower() == "csv")
            {
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("簽到ID,用戶ID,用戶名稱,簽到時間,獲得點數,連續天數,經驗值,優惠券");

                foreach (var signIn in signInsInRange)
                {
                    csv.AppendLine($"{signIn.LogID},{signIn.UserID},{signIn.Users?.UserName},{signIn.SignTime:yyyy-MM-dd HH:mm:ss},{signIn.PointsGained},{signIn.ConsecutiveDays},{signIn.ExpGained},{signIn.CouponGained}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"SignInReport_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv");
            }
            else
            {
                // JSON format
                var jsonData = signInsInRange.Select(s => new
                {
                    signInId = s.LogID,
                    userId = s.UserID,
                    userName = s.Users?.UserName,
                    signInTime = s.SignTime,
                    pointsGained = s.PointsGained,
                    consecutiveDays = s.ConsecutiveDays,
                    expGained = s.ExpGained,
                    couponGained = s.CouponGained
                });

                return Json(jsonData);
            }
        }

        // GET: AdminSignIn/ComparisonReport
        public async Task<IActionResult> ComparisonReport(DateTime? period1Start, DateTime? period1End, DateTime? period2Start, DateTime? period2End)
        {
            // 預設比較最近兩個月
            period1End ??= DateTime.Today;
            period1Start ??= period1End.Value.AddDays(-30);
            period2End ??= period1Start.Value.AddDays(-1);
            period2Start ??= period2End.Value.AddDays(-30);

            var allSignIns = await _signInService.GetAllSignInsAsync(1, 100000);

            var period1SignIns = allSignIns.Where(s => s.SignTime.Date >= period1Start.Value.Date && s.SignTime.Date <= period1End.Value.Date).ToList();
            var period2SignIns = allSignIns.Where(s => s.SignTime.Date >= period2Start.Value.Date && s.SignTime.Date <= period2End.Value.Date).ToList();

            var comparison = new
            {
                Period1 = new
                {
                    StartDate = period1Start.Value,
                    EndDate = period1End.Value,
                    TotalSignIns = period1SignIns.Count,
                    UniqueUsers = period1SignIns.Select(s => s.UserID).Distinct().Count(),
                    TotalPoints = period1SignIns.Sum(s => s.PointsGained),
                    AveragePointsPerSignIn = period1SignIns.Any() ? period1SignIns.Average(s => s.PointsGained) : 0
                },
                Period2 = new
                {
                    StartDate = period2Start.Value,
                    EndDate = period2End.Value,
                    TotalSignIns = period2SignIns.Count,
                    UniqueUsers = period2SignIns.Select(s => s.UserID).Distinct().Count(),
                    TotalPoints = period2SignIns.Sum(s => s.PointsGained),
                    AveragePointsPerSignIn = period2SignIns.Any() ? period2SignIns.Average(s => s.PointsGained) : 0
                }
            };

            // 計算成長率
            var growth = new
            {
                SignInsGrowth = comparison.Period2.TotalSignIns > 0
                    ? ((comparison.Period1.TotalSignIns - comparison.Period2.TotalSignIns) / (double)comparison.Period2.TotalSignIns * 100)
                    : 0,
                UsersGrowth = comparison.Period2.UniqueUsers > 0
                    ? ((comparison.Period1.UniqueUsers - comparison.Period2.UniqueUsers) / (double)comparison.Period2.UniqueUsers * 100)
                    : 0,
                PointsGrowth = comparison.Period2.TotalPoints > 0
                    ? ((comparison.Period1.TotalPoints - comparison.Period2.TotalPoints) / (double)comparison.Period2.TotalPoints * 100)
                    : 0
            };

            ViewBag.Comparison = comparison;
            ViewBag.Growth = growth;

            return View();
        }

        #endregion

        #region SignIn Rules Configuration

        // GET: AdminSignIn/Rules
        public async Task<IActionResult> Rules()
        {
            var viewModel = await LoadSignInRuleConfigurationAsync();
            return View(viewModel);
        }

        // POST: AdminSignIn/UpdateRules
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRules(SignInRuleConfigViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Rules", model);
            }

            if (model.SignInRule == null)
            {
                ModelState.AddModelError(string.Empty, "簽到規則資料遺失，請重新輸入。");
                model.SignInRule = new SignInRuleConfig();
                return View("Rules", model);
            }

            model.SignInRule.Description = model.SignInRule.Description?.Trim();

            IDbContextTransaction? transaction = null;
            try
            {
                transaction = await _context.Database.BeginTransactionAsync();

                if (!await SaveSignInSettingsAsync(model.SignInRule))
                {
                    await transaction.RollbackAsync();
                    await transaction.DisposeAsync();
                    transaction = null;
                    ModelState.AddModelError(string.Empty, "儲存簽到設定失敗，請稍後再試。");
                    return View("Rules", model);
                }

                if (!await ApplySignInRewardRulesAsync(model.SignInRule))
                {
                    await transaction.RollbackAsync();
                    await transaction.DisposeAsync();
                    transaction = null;
                    ModelState.AddModelError(string.Empty, "更新簽到獎勵規則失敗，請稍後再試。");
                    return View("Rules", model);
                }

                await transaction.CommitAsync();
                await transaction.DisposeAsync();
                transaction = null;

                await LogOperationAsync("SignInRules.Update", BuildAuditSummary(model.SignInRule));

                TempData["Success"] = "簽到規則已成功儲存。";
                return RedirectToAction(nameof(Rules));
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                    await transaction.DisposeAsync();
                    transaction = null;
                }

                ModelState.AddModelError(string.Empty, $"儲存簽到規則時發生錯誤：{ex.Message}");
                return View("Rules", model);
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        private async Task<SignInRuleConfigViewModel> LoadSignInRuleConfigurationAsync()
        {
            var signInRules = (await _signInService.GetAllSignInRulesAsync()).ToList();

            var dailyRule = signInRules
                .OrderBy(r => r.ConsecutiveDays)
                .FirstOrDefault(r => r.ConsecutiveDays <= 1);

            var streakRule = signInRules
                .Where(r => r.ConsecutiveDays > 1 && r.ConsecutiveDays < MonthlyRewardThreshold)
                .OrderBy(r => Math.Abs(r.ConsecutiveDays - DefaultConsecutiveRequirement))
                .FirstOrDefault();

            var monthlyRule = signInRules
                .Where(r => r.ConsecutiveDays >= MonthlyRewardThreshold)
                .OrderBy(r => r.ConsecutiveDays)
                .FirstOrDefault();

            var descriptionSetting = await GetSystemSettingAsync(RuleDescriptionSettingKey, string.Empty);
            var resolvedDescription = string.IsNullOrWhiteSpace(descriptionSetting)
                ? dailyRule?.Description ?? streakRule?.Description ?? monthlyRule?.Description ?? "每日簽到可獲得點數，達成連續簽到可獲得額外獎勵。"
                : descriptionSetting.Trim();

            var model = new SignInRuleConfigViewModel
            {
                SignInRule = new SignInRuleConfig
                {
                    DailyPoints = await GetIntSettingAsync(DailyPointsSettingKey, dailyRule?.PointsReward ?? DefaultDailyPoints, 0),
                    WeeklyBonusPoints = await GetIntSettingAsync(WeeklyBonusPointsSettingKey, streakRule?.PointsReward ?? DefaultWeeklyPoints, 0),
                    MonthlyBonusPoints = await GetIntSettingAsync(MonthlyBonusPointsSettingKey, monthlyRule?.PointsReward ?? DefaultMonthlyPoints, 0),
                    ConsecutiveDays = await GetIntSettingAsync(ConsecutiveRequirementSettingKey, streakRule?.ConsecutiveDays ?? DefaultConsecutiveRequirement, 1),
                    Description = resolvedDescription
                }
            };

            if (model.SignInRule.ConsecutiveDays < 1)
            {
                model.SignInRule.ConsecutiveDays = 1;
            }

            return model;
        }

        private async Task<bool> SaveSignInSettingsAsync(SignInRuleConfig config)
        {
            var description = (config.Description ?? string.Empty).Trim();
            var updates = new Dictionary<string, string>
            {
                [DailyPointsSettingKey] = config.DailyPoints.ToString(),
                [WeeklyBonusPointsSettingKey] = config.WeeklyBonusPoints.ToString(),
                [MonthlyBonusPointsSettingKey] = config.MonthlyBonusPoints.ToString(),
                [ConsecutiveRequirementSettingKey] = config.ConsecutiveDays.ToString(),
                [RuleDescriptionSettingKey] = description
            };

            foreach (var update in updates)
            {
                if (!await SetSystemSettingAsync(update.Key, update.Value))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> ApplySignInRewardRulesAsync(SignInRuleConfig config)
        {
            var rules = (await _signInService.GetAllSignInRulesAsync()).ToList();

            if (!await UpsertSignInRuleAsync(rules, 1, config.DailyPoints, "每日簽到獎勵", config.Description))
            {
                return false;
            }

            if (!await UpsertSignInRuleAsync(rules, config.ConsecutiveDays, config.WeeklyBonusPoints, $"連續簽到{config.ConsecutiveDays}天獎勵", config.Description))
            {
                return false;
            }

            if (!await UpsertSignInRuleAsync(rules, MonthlyRewardThreshold, config.MonthlyBonusPoints, "每月簽到獎勵", config.Description))
            {
                return false;
            }

            return true;
        }

        private async Task<bool> UpsertSignInRuleAsync(List<SignInRuleDto> existingRules, int consecutiveDays, int points, string ruleName, string? baseDescription)
        {
            var targetRule = existingRules.FirstOrDefault(r => r.ConsecutiveDays == consecutiveDays);

            if (targetRule != null)
            {
                targetRule.PointsReward = points;

                if (targetRule.ExpReward <= 0 && points > 0)
                {
                    targetRule.ExpReward = Math.Max(1, points / 2);
                }

                targetRule.RuleName = ruleName;
                targetRule.Description = BuildRuleDescription(consecutiveDays, points, baseDescription);
                targetRule.IsActive = true;
                targetRule.UpdatedAt = DateTime.UtcNow;

                return await _signInService.UpdateSignInRuleAsync(targetRule);
            }

            var newRule = new SignInRuleDto
            {
                RuleName = ruleName,
                Description = BuildRuleDescription(consecutiveDays, points, baseDescription),
                ConsecutiveDays = consecutiveDays,
                PointsReward = points,
                ExpReward = points > 0 ? Math.Max(1, points / 2) : 0,
                CouponTypeId = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return await _signInService.CreateSignInRuleAsync(newRule);
        }

        private async Task<int> GetIntSettingAsync(string key, int fallback, int minValue)
        {
            var raw = await GetSystemSettingAsync(key, fallback.ToString());

            if (!int.TryParse(raw, out var value))
            {
                value = fallback;
            }

            if (value < minValue)
            {
                value = minValue;
            }

            return value;
        }

        private static string BuildRuleDescription(int consecutiveDays, int points, string? extraDescription)
        {
            var segments = new List<string>
            {
                $"連續簽到 {consecutiveDays} 天"
            };

            if (points > 0)
            {
                segments.Add($"獲得 {points} 點數");
            }
            else
            {
                segments.Add("無額外點數獎勵");
            }

            if (!string.IsNullOrWhiteSpace(extraDescription))
            {
                segments.Add(extraDescription.Trim());
            }

            return string.Join("｜", segments);
        }

        private static string BuildAuditSummary(SignInRuleConfig config)
        {
            return $"每日{config.DailyPoints}點、週獎勵{config.WeeklyBonusPoints}點、月獎勵{config.MonthlyBonusPoints}點、連續簽到要求{config.ConsecutiveDays}天";
        }

        #endregion
    }
}


