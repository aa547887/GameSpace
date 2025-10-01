using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Data;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminSignInController : Controller
    {
        private readonly GameSpaceContext _context;

        public AdminSignInController(GameSpaceContext context)
        {
            _context = context;
        }

        // GET: AdminSignIn
        public async Task<IActionResult> Index(string searchTerm = "", string status = "", string sortBy = "date", int page = 1, int pageSize = 10)
        {
            var query = _context.SignIn
                .Include(s => s.Users)
                .AsQueryable();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.Users.User_name.Contains(searchTerm) || 
                                       s.Users.User_account.Contains(searchTerm));
            }

            // 狀態篩選
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "today")
                    query = query.Where(s => s.SignInDate.Date == DateTime.Today);
                else if (status == "week")
                    query = query.Where(s => s.SignInDate >= DateTime.Today.AddDays(-7));
                else if (status == "month")
                    query = query.Where(s => s.SignInDate >= DateTime.Today.AddDays(-30));
            }

            // 排序
            query = sortBy switch
            {
                "user" => query.OrderBy(s => s.Users.User_name),
                "points" => query.OrderByDescending(s => s.RewardPoints),
                "consecutive" => query.OrderByDescending(s => s.ConsecutiveDays),
                _ => query.OrderByDescending(s => s.SignInDate)
            };

            // 分頁
            var totalCount = await query.CountAsync();
            var signIns = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new AdminSignInIndexViewModel
            {
                SignIns = new PagedResult<SignIn>
                {
                    Items = signIns,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                }
            };

            // 設定 ViewBag 用於搜尋和篩選
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalSignIns = totalCount;
            ViewBag.TodaySignIns = await _context.SignIn.CountAsync(s => s.SignInDate.Date == DateTime.Today);
            ViewBag.WeekSignIns = await _context.SignIn.CountAsync(s => s.SignInDate >= DateTime.Today.AddDays(-7));
            ViewBag.MonthSignIns = await _context.SignIn.CountAsync(s => s.SignInDate >= DateTime.Today.AddDays(-30));

            return View(viewModel);
        }

        // GET: AdminSignIn/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var signIn = await _context.SignIn
                .Include(s => s.Users)
                .FirstOrDefaultAsync(m => m.SignInId == id);

            if (signIn == null)
            {
                return NotFound();
            }

            return View(signIn);
        }

        // GET: AdminSignIn/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminSignIn/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminSignInCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var signIn = new SignIn
                {
                    SignInName = model.SignInName,
                    SignInDescription = model.SignInDescription,
                    RewardPoints = model.RewardPoints,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now
                };

                _context.Add(signIn);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "簽到活動建立成功";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: AdminSignIn/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var signIn = await _context.SignIn.FindAsync(id);
            if (signIn == null)
            {
                return NotFound();
            }

            var model = new AdminSignInCreateViewModel
            {
                SignInName = signIn.SignInName,
                SignInDescription = signIn.SignInDescription,
                RewardPoints = signIn.RewardPoints,
                StartDate = signIn.StartDate,
                EndDate = signIn.EndDate,
                IsActive = signIn.IsActive
            };

            return View(model);
        }

        // POST: AdminSignIn/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminSignInCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var signIn = await _context.SignIn.FindAsync(id);
                    if (signIn == null)
                    {
                        return NotFound();
                    }

                    signIn.SignInName = model.SignInName;
                    signIn.SignInDescription = model.SignInDescription;
                    signIn.RewardPoints = model.RewardPoints;
                    signIn.StartDate = model.StartDate;
                    signIn.EndDate = model.EndDate;
                    signIn.IsActive = model.IsActive;

                    _context.Update(signIn);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "簽到活動更新成功";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SignInExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(model);
        }

        // GET: AdminSignIn/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var signIn = await _context.SignIn
                .Include(s => s.Users)
                .FirstOrDefaultAsync(m => m.SignInId == id);

            if (signIn == null)
            {
                return NotFound();
            }

            return View(signIn);
        }

        // POST: AdminSignIn/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var signIn = await _context.SignIn.FindAsync(id);
            if (signIn != null)
            {
                _context.SignIn.Remove(signIn);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "簽到活動刪除成功";
            }

            return RedirectToAction(nameof(Index));
        }

        // 切換簽到活動狀態
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var signIn = await _context.SignIn.FindAsync(id);
            if (signIn != null)
            {
                signIn.IsActive = !signIn.IsActive;
                _context.Update(signIn);
                await _context.SaveChangesAsync();

                return Json(new { success = true, isActive = signIn.IsActive });
            }

            return Json(new { success = false });
        }

        // 獲取簽到統計數據
        [HttpGet]
        public async Task<IActionResult> GetSignInStats()
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var stats = new
            {
                today = await _context.SignIn.CountAsync(s => s.SignInDate.Date == today),
                thisWeek = await _context.SignIn.CountAsync(s => s.SignInDate >= weekStart),
                thisMonth = await _context.SignIn.CountAsync(s => s.SignInDate >= monthStart),
                total = await _context.SignIn.CountAsync()
            };

            return Json(stats);
        }

        // 獲取簽到趨勢圖表數據
        [HttpGet]
        public async Task<IActionResult> GetSignInTrendData(int days = 30)
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-days);

            var data = new List<object>();

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var count = await _context.SignIn.CountAsync(s => s.SignInDate.Date == date);

                data.Add(new
                {
                    date = date.ToString("yyyy-MM-dd"),
                    count
                });
            }

            return Json(data);
        }

        // 獲取用戶簽到排行榜
        [HttpGet]
        public async Task<IActionResult> GetSignInLeaderboard(int top = 10)
        {
            var leaderboard = await _context.Users
                .Select(u => new
                {
                    userId = u.User_Id,
                    userName = u.User_name,
                    userAccount = u.User_account,
                    signInCount = _context.SignIn.Count(s => s.UserId == u.User_Id),
                    consecutiveDays = _context.SignIn
                        .Where(s => s.UserId == u.User_Id)
                        .OrderByDescending(s => s.SignInDate)
                        .Select(s => s.ConsecutiveDays)
                        .FirstOrDefault()
                })
                .OrderByDescending(u => u.signInCount)
                .Take(top)
                .ToListAsync();

            return Json(leaderboard);
        }

        private bool SignInExists(int id)
        {
            return _context.SignIn.Any(e => e.SignInId == id);
        }
    }
}
