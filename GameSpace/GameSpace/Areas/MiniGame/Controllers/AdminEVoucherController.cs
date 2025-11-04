using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Infrastructure.Time;
using GameSpace.Areas.MiniGame.Services;
using EVoucherCreateModel = GameSpace.Areas.MiniGame.Models.EVoucherCreateModel;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class AdminEVoucherController : MiniGameBaseController
    {
        private readonly IFuzzySearchService _fuzzySearchService;

        public AdminEVoucherController(GameSpacedatabaseContext context, IAppClock appClock, IFuzzySearchService fuzzySearchService)
            : base(context, appClock)
        {
            _fuzzySearchService = fuzzySearchService;
        }

        // GET: AdminEVoucher
        public async Task<IActionResult> Index(string searchTerm = "", string status = "", string evoucherType = "", string sortBy = "code", int page = 1, int pageSize = 10)
        {
            var query = _context.Evouchers
                .Include(e => e.User)
                .Include(e => e.EvoucherType)
                .AsQueryable();

            // 模糊搜尋：SearchTerm（聯集OR邏輯，使用 FuzzySearchService）
            var hasSearchTerm = !string.IsNullOrWhiteSpace(searchTerm);

            List<int> matchedEVoucherIds = new List<int>();
            Dictionary<int, int> evoucherPriority = new Dictionary<int, int>();

            if (hasSearchTerm)
            {
                var term = searchTerm.Trim();

                // 查詢所有電子禮券並使用 FuzzySearchService 計算優先順序
                var allEVouchers = await _context.Evouchers
                    .Include(e => e.User)
                    .Include(e => e.EvoucherType)
                    .AsNoTracking()
                    .Select(e => new {
                        e.EvoucherId,
                        e.EvoucherCode,
                        UserName = e.User != null ? e.User.UserName : "",
                        UserAccount = e.User != null ? e.User.UserAccount : "",
                        TypeName = e.EvoucherType != null ? e.EvoucherType.Name : ""
                    })
                    .ToListAsync();

                foreach (var evoucher in allEVouchers)
                {
                    int priority = 0;

                    // 電子禮券代碼精確匹配優先
                    if (evoucher.EvoucherCode.Equals(term, StringComparison.OrdinalIgnoreCase))
                    {
                        priority = 1; // 完全匹配 EvoucherCode
                    }
                    else if (evoucher.EvoucherCode.StartsWith(term, StringComparison.OrdinalIgnoreCase))
                    {
                        priority = 2; // 開頭匹配
                    }
                    else if (evoucher.EvoucherCode.Contains(term, StringComparison.OrdinalIgnoreCase))
                    {
                        priority = 3; // 包含匹配
                    }

                    // 如果代碼沒有匹配，嘗試類型名稱匹配
                    if (priority == 0 && !string.IsNullOrEmpty(evoucher.TypeName))
                    {
                        if (evoucher.TypeName.Equals(term, StringComparison.OrdinalIgnoreCase))
                        {
                            priority = 1;
                        }
                        else if (evoucher.TypeName.Contains(term, StringComparison.OrdinalIgnoreCase))
                        {
                            priority = 3;
                        }
                    }

                    // 如果還沒匹配，嘗試用戶名模糊搜尋
                    if (priority == 0)
                    {
                        priority = _fuzzySearchService.CalculateMatchPriority(
                            term,
                            evoucher.UserAccount,
                            evoucher.UserName
                        );
                    }

                    // 如果匹配成功（priority > 0），加入結果
                    if (priority > 0)
                    {
                        matchedEVoucherIds.Add(evoucher.EvoucherId);
                        evoucherPriority[evoucher.EvoucherId] = priority;
                    }
                }

                query = query.Where(e => matchedEVoucherIds.Contains(e.EvoucherId));
            }

            // 狀態篩選 - 使用台灣時間
            if (!string.IsNullOrEmpty(status))
            {
                var nowUtc = _appClock.UtcNow;
                var now = _appClock.ToAppTime(nowUtc);
                query = status switch
                {
                    "unused" => query.Where(e => !e.IsUsed && e.EvoucherType.ValidTo >= now),
                    "used" => query.Where(e => e.IsUsed),
                    "expired" => query.Where(e => !e.IsUsed && e.EvoucherType.ValidTo < now),
                    _ => query
                };
            }

            // 禮券類型篩選
            if (!string.IsNullOrEmpty(evoucherType) && int.TryParse(evoucherType, out int typeId))
            {
                query = query.Where(e => e.EvoucherTypeId == typeId);
            }

            // 計算統計數據（從篩選後的查詢）
            var totalCount = await query.CountAsync();

            // 優先順序排序：先取資料再排序
            var allItems = await query.ToListAsync();
            var evouchers = allItems;

            if (hasSearchTerm)
            {
                // 在記憶體中進行優先順序排序
                var ordered = allItems.OrderBy(e =>
                {
                    // 如果電子禮券匹配，返回對應優先順序
                    if (evoucherPriority.ContainsKey(e.EvoucherId))
                    {
                        return evoucherPriority[e.EvoucherId];
                    }
                    return 99;
                });

                // 次要排序
                var sorted = sortBy switch
                {
                    "type" => ordered.ThenBy(e => e.EvoucherType.Name),
                    "user" => ordered.ThenBy(e => e.User.UserName),
                    "acquired" => ordered.ThenByDescending(e => e.AcquiredTime),
                    "used" => ordered.ThenByDescending(e => e.UsedTime),
                    _ => ordered.ThenBy(e => e.EvoucherCode)
                };

                evouchers = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                // 沒有搜尋條件時使用資料庫排序
                var sorted = sortBy switch
                {
                    "type" => allItems.OrderBy(e => e.EvoucherType.Name),
                    "user" => allItems.OrderBy(e => e.User.UserName),
                    "acquired" => allItems.OrderByDescending(e => e.AcquiredTime),
                    "used" => allItems.OrderByDescending(e => e.UsedTime),
                    _ => allItems.OrderBy(e => e.EvoucherCode)
                };

                evouchers = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            var viewModel = new AdminEVoucherIndexViewModel
            {
                Evouchers = new PagedResult<Evoucher>
                {
                    Items = evouchers,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                }
            };

            // 設定 ViewBag 用於搜尋和篩選
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.EvoucherType = evoucherType;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalEVouchers = totalCount;

            // Build filtered query for statistics (before pagination)
            var statsQuery = _context.Evouchers
                .Include(e => e.User)
                .Include(e => e.EvoucherType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                statsQuery = statsQuery.Where(e => e.EvoucherCode.Contains(searchTerm) ||
                                       e.User.UserName.Contains(searchTerm) ||
                                       e.EvoucherType.Name.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(status))
            {
                var nowUtc = _appClock.UtcNow;
                var now = _appClock.ToAppTime(nowUtc);
                statsQuery = status switch
                {
                    "unused" => statsQuery.Where(e => !e.IsUsed && e.EvoucherType.ValidTo >= now),
                    "used" => statsQuery.Where(e => e.IsUsed),
                    "expired" => statsQuery.Where(e => !e.IsUsed && e.EvoucherType.ValidTo < now),
                    _ => statsQuery
                };
            }

            if (!string.IsNullOrEmpty(evoucherType) && int.TryParse(evoucherType, out int statsTypeId))
            {
                statsQuery = statsQuery.Where(e => e.EvoucherTypeId == statsTypeId);
            }

            ViewBag.UsedEVouchers = await statsQuery.CountAsync(e => e.IsUsed);
            ViewBag.UnusedEVouchers = await statsQuery.CountAsync(e => !e.IsUsed);
            ViewBag.EvoucherTypes = await _context.EvoucherTypes.CountAsync();
            ViewBag.EvoucherTypeList = await _context.EvoucherTypes.ToListAsync();

            return View(viewModel);
        }

        // GET: AdminEVoucher/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVoucher = await _context.Evouchers
                .Include(e => e.User)
                .Include(e => e.EvoucherType)
                .FirstOrDefaultAsync(m => m.EvoucherId == id);

            if (eVoucher == null)
            {
                return NotFound();
            }

            return View(eVoucher);
        }

        // GET: AdminEVoucher/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.EvoucherTypes = await _context.EvoucherTypes.ToListAsync();
            ViewBag.Users = await _context.Users.ToListAsync();
            return View();
        }

        // POST: AdminEVoucher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EvoucherCode,UserId,EvoucherTypeId,AcquiredTime")] EVoucherCreateModel model)
        {
            if (ModelState.IsValid)
            {
                // 檢查禮券代碼是否已存在
                if (await _context.Evouchers.AnyAsync(e => e.EvoucherCode == model.EvoucherCode))
                {
                    ModelState.AddModelError("EvoucherCode", "此禮券代碼已存在");
                    ViewBag.EvoucherTypes = await _context.EvoucherTypes.ToListAsync();
                    ViewBag.Users = await _context.Users.ToListAsync();
                    return View(model);
                }

                // 使用台灣時間
                var nowUtc = _appClock.UtcNow;
                var nowTaiwanTime = _appClock.ToAppTime(nowUtc);

                var eVoucher = new Evoucher
                {
                    EvoucherTypeId = model.EvoucherTypeId,
                    UserId = model.UserId,
                    EvoucherCode = model.EvoucherCode,
                    AcquiredTime = model.AcquiredTime ?? nowTaiwanTime,  // 使用台灣時間
                    UsedTime = null,  // 明確設為 null
                    IsUsed = false,
                    IsDeleted = false  // 必填字段：未刪除
                };

                _context.Add(eVoucher);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "電子禮券建立成功";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.EvoucherTypes = await _context.EvoucherTypes.ToListAsync();
            ViewBag.Users = await _context.Users.ToListAsync();
            return View(model);
        }

        // GET: AdminEVoucher/CreateType
        public IActionResult CreateType()
        {
            return View();
        }

        // POST: AdminEVoucher/CreateType
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateType([Bind("Name,Description,ValueAmount,ValidFrom,ValidTo,PointsCost,TotalAvailable")] EvoucherType eVoucherType)
        {
            if (ModelState.IsValid)
            {
                // 驗證：截止日必須晚於起始日
                if (eVoucherType.ValidFrom >= eVoucherType.ValidTo)
                {
                    ModelState.AddModelError("ValidTo", "有效截止日必須晚於有效起始日");
                    return View(eVoucherType);
                }

                // 設定預設值
                eVoucherType.IsDeleted = false;
                eVoucherType.DeletedAt = null;
                eVoucherType.DeletedBy = null;
                eVoucherType.DeleteReason = null;

                _context.Add(eVoucherType);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"電子禮券類型「{eVoucherType.Name}」建立成功";
                return RedirectToAction("AdjustEVoucher", "WalletAdmin");
            }

            return View(eVoucherType);
        }

        // POST: AdminEVoucher/DisableEVoucherType
        // 停用電子禮券類型 (軟刪除)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableEVoucherType(int id)
        {
            try
            {
                var evoucherType = await _context.EvoucherTypes.FindAsync(id);
                if (evoucherType == null)
                {
                    TempData["ErrorMessage"] = "找不到要停用的電子禮券類型";
                    return RedirectToAction("AdjustEVoucher", "WalletAdmin");
                }

                // 軟刪除：設定 IsDeleted = true
                evoucherType.IsDeleted = true;
                evoucherType.DeletedAt = _appClock.UtcNow;
                evoucherType.DeleteReason = "管理員停用";

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"電子禮券類型「{evoucherType.Name}」已停用";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"停用失敗：{ex.Message}";
            }

            return RedirectToAction("AdjustEVoucher", "WalletAdmin");
        }

        // POST: AdminEVoucher/DeleteEVoucherType
        // 刪除電子禮券類型 (軟刪除)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEVoucherType(int id)
        {
            try
            {
                var evoucherType = await _context.EvoucherTypes.FindAsync(id);
                if (evoucherType == null)
                {
                    TempData["ErrorMessage"] = "找不到要刪除的電子禮券類型";
                    return RedirectToAction("AdjustEVoucher", "WalletAdmin");
                }

                // 軟刪除：設定 IsDeleted = true
                evoucherType.IsDeleted = true;
                evoucherType.DeletedAt = _appClock.UtcNow;
                evoucherType.DeleteReason = "管理員透過後台刪除";

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"電子禮券類型「{evoucherType.Name}」已刪除（軟刪除）";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"刪除失敗：{ex.Message}";
            }

            return RedirectToAction("AdjustEVoucher", "WalletAdmin");
        }

        // GET: AdminEVoucher/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVoucher = await _context.Evouchers.FindAsync(id);
            if (eVoucher == null)
            {
                return NotFound();
            }

            var model = new AdminEVoucherCreateViewModel
            {
                EvoucherTypeId = eVoucher.EvoucherTypeId,
                UserId = eVoucher.UserId,
                EvoucherCode = eVoucher.EvoucherCode,
                AcquiredTime = eVoucher.AcquiredTime,
                UsedTime = eVoucher.UsedTime,
                IsUsed = eVoucher.IsUsed
            };

            ViewBag.EvoucherTypes = await _context.EvoucherTypes.ToListAsync();
            ViewBag.Users = await _context.Users.ToListAsync();
            return View(model);
        }

        // POST: AdminEVoucher/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,EvoucherTypeId")] EVoucherEditModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var eVoucher = await _context.Evouchers.FindAsync(id);
                    if (eVoucher == null)
                    {
                        return NotFound();
                    }

                    // 只允許變更關聯（User、Type），券碼與使用狀態由其他動作管理
                    
                    
                    
                    
                    
                    
                    
                    
                    eVoucher.EvoucherTypeId = model.EvoucherTypeId;
                    eVoucher.UserId = model.UserId;
                    
                    
                    
                    

                    _context.Update(eVoucher);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "電子禮券更新成功";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EvoucherExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.EvoucherTypes = await _context.EvoucherTypes.ToListAsync();
            ViewBag.Users = await _context.Users.ToListAsync();
            return View(model);
        }

        // GET: AdminEVoucher/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVoucher = await _context.Evouchers
                .Include(e => e.User)
                .Include(e => e.EvoucherType)
                .FirstOrDefaultAsync(m => m.EvoucherId == id);

            if (eVoucher == null)
            {
                return NotFound();
            }

            return View(eVoucher);
        }

        // POST: AdminEVoucher/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eVoucher = await _context.Evouchers.FindAsync(id);
            if (eVoucher != null)
            {
                _context.Evouchers.Remove(eVoucher);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "電子禮券刪除成功";
            }

            return RedirectToAction(nameof(Index));
        }

        // 切換禮券使用狀態
        [HttpPost]
        public async Task<IActionResult> ToggleUsage(int id)
        {
            var eVoucher = await _context.Evouchers.FindAsync(id);
            if (eVoucher != null)
            {
                eVoucher.IsUsed = !eVoucher.IsUsed;
                if (eVoucher.IsUsed && !eVoucher.UsedTime.HasValue)
                {
                    // 使用台灣時間
                    var nowUtc = _appClock.UtcNow;
                    eVoucher.UsedTime = _appClock.ToAppTime(nowUtc);
                }
                else if (!eVoucher.IsUsed)
                {
                    eVoucher.UsedTime = null;  // 明確設為 null
                }

                _context.Update(eVoucher);
                await _context.SaveChangesAsync();

                return Json(new { success = true, isUsed = eVoucher.IsUsed });
            }

            return Json(new { success = false });
        }

        // 獲取禮券統計數據
        [HttpGet]
        public async Task<IActionResult> GetEVoucherStats()
        {
            // 使用台灣時間
            var nowUtc = _appClock.UtcNow;
            var now = _appClock.ToAppTime(nowUtc);
            var stats = new
            {
                total = await _context.Evouchers.CountAsync(),
                used = await _context.Evouchers.CountAsync(e => e.IsUsed),
                unused = await _context.Evouchers.CountAsync(e => !e.IsUsed),
                expired = await _context.Evouchers.CountAsync(e => !e.IsUsed && e.EvoucherType.ValidTo < now),
                types = await _context.EvoucherTypes.CountAsync()
            };

            return Json(stats);
        }

        // 獲取禮券類型分佈
        [HttpGet]
        public async Task<IActionResult> GetEvoucherTypeDistribution()
        {
            var distribution = await _context.Evouchers
                .GroupBy(e => e.EvoucherType.Name)
                .Select(g => new
                {
                    type = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(g => g.count)
                .ToListAsync();

            return Json(distribution);
        }

        // 獲取禮券使用趨勢
        [HttpGet]
        public async Task<IActionResult> GetEVoucherUsageTrend(int days = 30)
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-days);

            var trend = await _context.Evouchers
                .Where(e => e.UsedTime >= startDate)
                .GroupBy(e => e.UsedTime.Value.Date)
                .Select(g => new
                {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    count = g.Count()
                })
                .OrderBy(g => g.date)
                .ToListAsync();

            return Json(trend);
        }

        private bool EvoucherExists(int id)
        {
            return _context.Evouchers.Any(e => e.EvoucherId == id);
        }

        #region EvoucherType Management

        // GET: AdminEVoucher/EvoucherTypes
        public async Task<IActionResult> EvoucherTypes(string searchTerm = "", string status = "", string sortBy = "name", int page = 1, int pageSize = 10)
        {
            var query = _context.EvoucherTypes.AsQueryable();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(et => et.Name.Contains(searchTerm) || et.Description.Contains(searchTerm));
            }

            // 狀態篩選 - 使用台灣時間
            if (!string.IsNullOrEmpty(status))
            {
                var nowUtc = _appClock.UtcNow;
                var now = _appClock.ToAppTime(nowUtc);
                query = status switch
                {
                    "active" => query.Where(et => et.ValidFrom <= now && et.ValidTo >= now),
                    "upcoming" => query.Where(et => et.ValidFrom > now),
                    "expired" => query.Where(et => et.ValidTo < now),
                    "available" => query.Where(et => et.TotalAvailable > 0),
                    _ => query
                };
            }

            // 排序
            query = sortBy switch
            {
                "value" => query.OrderByDescending(et => et.ValueAmount),
                "points" => query.OrderBy(et => et.PointsCost),
                "validfrom" => query.OrderBy(et => et.ValidFrom),
                "available" => query.OrderByDescending(et => et.TotalAvailable),
                _ => query.OrderBy(et => et.Name)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 計算每個類型的使用統計
            foreach (var item in items)
            {
                var typeId = item.EvoucherTypeId;
                var totalVouchers = await _context.Evouchers.CountAsync(e => e.EvoucherTypeId == typeId);
                var usedVouchers = await _context.Evouchers.CountAsync(e => e.EvoucherTypeId == typeId && e.IsUsed);

                ViewData[$"TotalVouchers_{typeId}"] = totalVouchers;
                ViewData[$"UsedVouchers_{typeId}"] = usedVouchers;
                ViewData[$"UnusedVouchers_{typeId}"] = totalVouchers - usedVouchers;
            }

            ViewBag.TotalCount = totalCount;
            ViewBag.PageNumber = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.SortBy = sortBy;

            return View(items);
        }

        // GET: AdminEVoucher/CreateEvoucherType
        public IActionResult CreateEvoucherType()
        {
            return View();
        }

        // POST: AdminEVoucher/CreateEvoucherType
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvoucherType([Bind("EvoucherTypeId,Name,ValueAmount,ValidFrom,ValidTo,PointsCost,TotalAvailable,Description")] EvoucherType eVoucherType)
        {
            if (ModelState.IsValid)
            {
                // 驗證邏輯
                if (eVoucherType.ValidFrom >= eVoucherType.ValidTo)
                {
                    ModelState.AddModelError("ValidFrom", "開始日期必須早於結束日期");
                    return View(eVoucherType);
                }

                if (eVoucherType.ValueAmount <= 0)
                {
                    ModelState.AddModelError("ValueAmount", "禮券面額必須大於 0");
                    return View(eVoucherType);
                }

                if (eVoucherType.PointsCost < 0)
                {
                    ModelState.AddModelError("PointsCost", "所需點數不能為負數");
                    return View(eVoucherType);
                }

                if (eVoucherType.TotalAvailable < 0)
                {
                    ModelState.AddModelError("TotalAvailable", "可用數量不能為負數");
                    return View(eVoucherType);
                }

                // 檢查名稱是否重複
                if (await _context.EvoucherTypes.AnyAsync(et => et.Name == eVoucherType.Name))
                {
                    ModelState.AddModelError("Name", "此禮券類型名稱已存在");
                    return View(eVoucherType);
                }

                _context.Add(eVoucherType);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "電子禮券類型建立成功";
                return RedirectToAction(nameof(EvoucherTypes));
            }

            return View(eVoucherType);
        }

        // GET: AdminEVoucher/EditEvoucherType/5
        public async Task<IActionResult> EditEvoucherType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVoucherType = await _context.EvoucherTypes.FindAsync(id);
            if (eVoucherType == null)
            {
                return NotFound();
            }

            return View(eVoucherType);
        }

        // POST: AdminEVoucher/EditEvoucherType/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvoucherType(int id, [Bind("EvoucherTypeId,Name,ValueAmount,ValidFrom,ValidTo,PointsCost,TotalAvailable,Description")] EvoucherType eVoucherType)
        {
            if (id != eVoucherType.EvoucherTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // 驗證邏輯
                if (eVoucherType.ValidFrom >= eVoucherType.ValidTo)
                {
                    ModelState.AddModelError("ValidFrom", "開始日期必須早於結束日期");
                    return View(eVoucherType);
                }

                if (eVoucherType.ValueAmount <= 0)
                {
                    ModelState.AddModelError("ValueAmount", "禮券面額必須大於 0");
                    return View(eVoucherType);
                }

                if (eVoucherType.PointsCost < 0)
                {
                    ModelState.AddModelError("PointsCost", "所需點數不能為負數");
                    return View(eVoucherType);
                }

                if (eVoucherType.TotalAvailable < 0)
                {
                    ModelState.AddModelError("TotalAvailable", "可用數量不能為負數");
                    return View(eVoucherType);
                }

                // 檢查名稱是否與其他類型重複
                if (await _context.EvoucherTypes.AnyAsync(et => et.Name == eVoucherType.Name && et.EvoucherTypeId != id))
                {
                    ModelState.AddModelError("Name", "此禮券類型名稱已被其他類型使用");
                    return View(eVoucherType);
                }

                try
                {
                    _context.Update(eVoucherType);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "電子禮券類型更新成功";
                    return RedirectToAction(nameof(EvoucherTypes));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EvoucherTypeExists(eVoucherType.EvoucherTypeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(eVoucherType);
        }

        // GET: AdminEVoucher/DeleteEvoucherType/5
        public async Task<IActionResult> DeleteEvoucherType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVoucherType = await _context.EvoucherTypes
                .FirstOrDefaultAsync(m => m.EvoucherTypeId == id);

            if (eVoucherType == null)
            {
                return NotFound();
            }

            // 檢查是否有相關的電子禮券
            var relatedVouchersCount = await _context.Evouchers.CountAsync(e => e.EvoucherTypeId == id);
            ViewBag.RelatedVouchersCount = relatedVouchersCount;

            return View(eVoucherType);
        }

        // POST: AdminEVoucher/DeleteEvoucherType/5
        [HttpPost, ActionName("DeleteEvoucherType")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvoucherTypeConfirmed(int id)
        {
            // 檢查是否有相關的電子禮券
            var relatedVouchersCount = await _context.Evouchers.CountAsync(e => e.EvoucherTypeId == id);
            if (relatedVouchersCount > 0)
            {
                TempData["ErrorMessage"] = $"無法刪除：此禮券類型仍有 {relatedVouchersCount} 個相關電子禮券";
                return RedirectToAction(nameof(EvoucherTypes));
            }

            var eVoucherType = await _context.EvoucherTypes.FindAsync(id);
            if (eVoucherType != null)
            {
                _context.EvoucherTypes.Remove(eVoucherType);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "電子禮券類型刪除成功";
            }

            return RedirectToAction(nameof(EvoucherTypes));
        }

        // GET: AdminEVoucher/EvoucherTypeDetails/5
        public async Task<IActionResult> EvoucherTypeDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVoucherType = await _context.EvoucherTypes
                .FirstOrDefaultAsync(m => m.EvoucherTypeId == id);

            if (eVoucherType == null)
            {
                return NotFound();
            }

            // 取得使用此類型的電子禮券統計 - 使用台灣時間
            var relatedVouchers = await _context.Evouchers
                .Include(e => e.User)
                .Where(e => e.EvoucherTypeId == id)
                .ToListAsync();

            var nowUtc = _appClock.UtcNow;
            var now = _appClock.ToAppTime(nowUtc);

            ViewBag.TotalVouchers = relatedVouchers.Count;
            ViewBag.UsedVouchers = relatedVouchers.Count(e => e.IsUsed);
            ViewBag.UnusedVouchers = relatedVouchers.Count(e => !e.IsUsed);
            ViewBag.ExpiredVouchers = relatedVouchers.Count(e => !e.IsUsed && eVoucherType.ValidTo < now);
            ViewBag.ActiveVouchers = relatedVouchers.Count(e => !e.IsUsed && eVoucherType.ValidFrom <= now && eVoucherType.ValidTo >= now);
            ViewBag.TotalValue = relatedVouchers.Count * eVoucherType.ValueAmount;
            ViewBag.UsedValue = relatedVouchers.Count(e => e.IsUsed) * eVoucherType.ValueAmount;
            ViewBag.RemainingValue = relatedVouchers.Count(e => !e.IsUsed) * eVoucherType.ValueAmount;

            // 最近發放的禮券
            ViewBag.RecentVouchers = relatedVouchers
                .OrderByDescending(e => e.AcquiredTime)
                .Take(10)
                .ToList();

            return View(eVoucherType);
        }

        // POST: AdminEVoucher/UpdateAvailability/5
        [HttpPost]
        public async Task<IActionResult> UpdateAvailability(int id, int totalAvailable)
        {
            var eVoucherType = await _context.EvoucherTypes.FindAsync(id);
            if (eVoucherType != null)
            {
                if (totalAvailable < 0)
                {
                    return Json(new { success = false, message = "可用數量不能為負數" });
                }

                eVoucherType.TotalAvailable = totalAvailable;
                _context.Update(eVoucherType);
                await _context.SaveChangesAsync();

                return Json(new { success = true, totalAvailable = eVoucherType.TotalAvailable });
            }

            return Json(new { success = false, message = "找不到該禮券類型" });
        }

        // GET: AdminEVoucher/GetEvoucherTypeStats
        [HttpGet]
        public async Task<IActionResult> GetEvoucherTypeStats()
        {
            // 使用台灣時間
            var nowUtc = _appClock.UtcNow;
            var now = _appClock.ToAppTime(nowUtc);
            var stats = new
            {
                totalTypes = await _context.EvoucherTypes.CountAsync(),
                activeTypes = await _context.EvoucherTypes.CountAsync(et => et.ValidFrom <= now && et.ValidTo >= now),
                upcomingTypes = await _context.EvoucherTypes.CountAsync(et => et.ValidFrom > now),
                expiredTypes = await _context.EvoucherTypes.CountAsync(et => et.ValidTo < now),
                totalValue = await _context.EvoucherTypes.SumAsync(et => et.ValueAmount * et.TotalAvailable),
                totalAvailable = await _context.EvoucherTypes.SumAsync(et => et.TotalAvailable)
            };

            return Json(stats);
        }

        // GET: AdminEVoucher/GetTopEvoucherTypes
        [HttpGet]
        public async Task<IActionResult> GetTopEvoucherTypes(int top = 5)
        {
            var typeStats = new List<object>();

            var types = await _context.EvoucherTypes
                .OrderByDescending(et => et.TotalAvailable)
                .Take(top)
                .ToListAsync();

            foreach (var type in types)
            {
                var totalVouchers = await _context.Evouchers.CountAsync(e => e.EvoucherTypeId == type.EvoucherTypeId);
                var usedVouchers = await _context.Evouchers.CountAsync(e => e.EvoucherTypeId == type.EvoucherTypeId && e.IsUsed);

                typeStats.Add(new
                {
                    name = type.Name,
                    valueAmount = type.ValueAmount,
                    totalAvailable = type.TotalAvailable,
                    totalIssued = totalVouchers,
                    totalUsed = usedVouchers,
                    usageRate = totalVouchers > 0 ? (double)usedVouchers / totalVouchers * 100 : 0
                });
            }

            return Json(typeStats);
        }

        // POST: AdminEVoucher/BulkIssueVouchers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkIssueVouchers(int typeId, int quantity, List<int> userIds)
        {
            var eVoucherType = await _context.EvoucherTypes.FindAsync(typeId);
            if (eVoucherType == null)
            {
                TempData["ErrorMessage"] = "找不到該禮券類型";
                return RedirectToAction(nameof(EvoucherTypes));
            }

            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "發放數量必須大於 0";
                return RedirectToAction(nameof(EvoucherTypeDetails), new { id = typeId });
            }

            if (eVoucherType.TotalAvailable < quantity * userIds.Count)
            {
                TempData["ErrorMessage"] = $"可用數量不足。需要 {quantity * userIds.Count}，但只有 {eVoucherType.TotalAvailable} 可用";
                return RedirectToAction(nameof(EvoucherTypeDetails), new { id = typeId });
            }

            var issuedCount = 0;
            var random = new Random();

            foreach (var userId in userIds)
            {
                for (int i = 0; i < quantity; i++)
                {
                    var code = $"EV-{eVoucherType.Name.Substring(0, Math.Min(4, eVoucherType.Name.Length)).ToUpper()}-{random.Next(1000, 9999)}-{random.Next(100000, 999999)}";

                    // 確保代碼唯一
                    while (await _context.Evouchers.AnyAsync(e => e.EvoucherCode == code))
                    {
                        code = $"EV-{eVoucherType.Name.Substring(0, Math.Min(4, eVoucherType.Name.Length)).ToUpper()}-{random.Next(1000, 9999)}-{random.Next(100000, 999999)}";
                    }

                    // 使用台灣時間
                    var nowUtc = _appClock.UtcNow;
                    var nowTaiwanTime = _appClock.ToAppTime(nowUtc);

                    var voucher = new Evoucher
                    {
                        EvoucherTypeId = typeId,
                        UserId = userId,
                        EvoucherCode = code,
                        AcquiredTime = nowTaiwanTime,  // 使用台灣時間
                        UsedTime = null,  // 明確設為 null (剛發放時未使用)
                        IsUsed = false,
                        IsDeleted = false  // 必填字段：未刪除
                    };

                    _context.Add(voucher);
                    issuedCount++;
                }
            }

            // 更新可用數量
            eVoucherType.TotalAvailable -= issuedCount;
            _context.Update(eVoucherType);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"成功發放 {issuedCount} 張電子禮券給 {userIds.Count} 位用戶";
            return RedirectToAction(nameof(EvoucherTypeDetails), new { id = typeId });
        }

        private bool EvoucherTypeExists(int id)
        {
            return _context.EvoucherTypes.Any(e => e.EvoucherTypeId == id);
        }

        #endregion
    }
}




