using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.social_hub.Auth;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme)]
    public class AdminEVoucherController : MiniGameBaseController
    {
        private readonly GameSpacedatabaseContext _context;

        public AdminEVoucherController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // GET: AdminEVoucher
        public async Task<IActionResult> Index(string searchTerm = "", string status = "", string evoucherType = "", string sortBy = "code", int page = 1, int pageSize = 10)
        {
            var query = _context.EVoucher
                .Include(e => e.Users)
                .Include(e => e.EVoucherType)
                .AsQueryable();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => e.EVoucherCode.Contains(searchTerm) || 
                                       e.Users.User_name.Contains(searchTerm) || 
                                       e.EVoucherType.Name.Contains(searchTerm));
            }

            // 狀態篩選
            if (!string.IsNullOrEmpty(status))
            {
                var now = DateTime.Now;
                query = status switch
                {
                    "unused" => query.Where(e => !e.IsUsed && e.EVoucherType.ValidTo >= now),
                    "used" => query.Where(e => e.IsUsed),
                    "expired" => query.Where(e => !e.IsUsed && e.EVoucherType.ValidTo < now),
                    _ => query
                };
            }

            // 禮券類型篩選
            if (!string.IsNullOrEmpty(evoucherType) && int.TryParse(evoucherType, out int typeId))
            {
                query = query.Where(e => e.EVoucherTypeID == typeId);
            }

            // 排序
            query = sortBy switch
            {
                "type" => query.OrderBy(e => e.EVoucherType.Name),
                "user" => query.OrderBy(e => e.Users.User_name),
                "acquired" => query.OrderByDescending(e => e.AcquiredTime),
                "used" => query.OrderByDescending(e => e.UsedTime),
                _ => query.OrderBy(e => e.EVoucherCode)
            };

            // 分頁
            var totalCount = await query.CountAsync();
            var evouchers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new AdminEVoucherIndexViewModel
            {
                EVouchers = new PagedResult<EVoucher>
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
            ViewBag.EVoucherType = evoucherType;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalEVouchers = totalCount;
            ViewBag.UsedEVouchers = await _context.EVoucher.CountAsync(e => e.IsUsed);
            ViewBag.UnusedEVouchers = await _context.EVoucher.CountAsync(e => !e.IsUsed);
            ViewBag.EVoucherTypes = await _context.EVoucherType.CountAsync();
            ViewBag.EVoucherTypeList = await _context.EVoucherType.ToListAsync();

            return View(viewModel);
        }

        // GET: AdminEVoucher/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVoucher = await _context.EVoucher
                .Include(e => e.Users)
                .Include(e => e.EVoucherType)
                .FirstOrDefaultAsync(m => m.EVoucherID == id);

            if (eVoucher == null)
            {
                return NotFound();
            }

            return View(eVoucher);
        }

        // GET: AdminEVoucher/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.EVoucherTypes = await _context.EVoucherType.Where(et => et.IsActive).ToListAsync();
            ViewBag.Users = await _context.Users.Where(u => u.IsActive).ToListAsync();
            return View();
        }

        // POST: AdminEVoucher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminEVoucherCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 檢查禮券代碼是否已存在
                if (await _context.EVoucher.AnyAsync(e => e.EVoucherCode == model.EVoucherCode))
                {
                    ModelState.AddModelError("EVoucherCode", "此禮券代碼已存在");
                    ViewBag.EVoucherTypes = await _context.EVoucherType.Where(et => et.IsActive).ToListAsync();
                    ViewBag.Users = await _context.Users.Where(u => u.IsActive).ToListAsync();
                    return View(model);
                }

                var eVoucher = new EVoucher
                {
                    EVoucherTypeID = model.EVoucherTypeID,
                    UserID = model.UserID,
                    EVoucherCode = model.EVoucherCode,
                    AcquiredTime = model.AcquiredTime ?? DateTime.Now,
                    UsedTime = model.UsedTime,
                    IsUsed = model.IsUsed
                };

                _context.Add(eVoucher);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "電子禮券建立成功";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.EVoucherTypes = await _context.EVoucherType.Where(et => et.IsActive).ToListAsync();
            ViewBag.Users = await _context.Users.Where(u => u.IsActive).ToListAsync();
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
        public async Task<IActionResult> CreateType(AdminEVoucherTypeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var eVoucherType = new EVoucherType
                {
                    Name = model.Name,
                    Description = model.Description,
                    ValueAmount = model.ValueAmount,
                    ValidDays = model.ValidDays,
                    ImageUrl = model.ImageUrl,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now
                };

                _context.Add(eVoucherType);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "禮券類型建立成功";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: AdminEVoucher/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVoucher = await _context.EVoucher.FindAsync(id);
            if (eVoucher == null)
            {
                return NotFound();
            }

            var model = new AdminEVoucherCreateViewModel
            {
                EVoucherTypeID = eVoucher.EVoucherTypeID,
                UserID = eVoucher.UserID,
                EVoucherCode = eVoucher.EVoucherCode,
                AcquiredTime = eVoucher.AcquiredTime,
                UsedTime = eVoucher.UsedTime,
                IsUsed = eVoucher.IsUsed
            };

            ViewBag.EVoucherTypes = await _context.EVoucherType.Where(et => et.IsActive).ToListAsync();
            ViewBag.Users = await _context.Users.Where(u => u.IsActive).ToListAsync();
            return View(model);
        }

        // POST: AdminEVoucher/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminEVoucherCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var eVoucher = await _context.EVoucher.FindAsync(id);
                    if (eVoucher == null)
                    {
                        return NotFound();
                    }

                    // 檢查禮券代碼是否已被其他禮券使用
                    if (await _context.EVoucher.AnyAsync(e => e.EVoucherCode == model.EVoucherCode && e.EVoucherID != id))
                    {
                        ModelState.AddModelError("EVoucherCode", "此禮券代碼已被其他禮券使用");
                        ViewBag.EVoucherTypes = await _context.EVoucherType.Where(et => et.IsActive).ToListAsync();
                        ViewBag.Users = await _context.Users.Where(u => u.IsActive).ToListAsync();
                        return View(model);
                    }

                    eVoucher.EVoucherTypeID = model.EVoucherTypeID;
                    eVoucher.UserID = model.UserID;
                    eVoucher.EVoucherCode = model.EVoucherCode;
                    eVoucher.AcquiredTime = model.AcquiredTime ?? DateTime.Now;
                    eVoucher.UsedTime = model.UsedTime;
                    eVoucher.IsUsed = model.IsUsed;

                    _context.Update(eVoucher);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "電子禮券更新成功";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EVoucherExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.EVoucherTypes = await _context.EVoucherType.Where(et => et.IsActive).ToListAsync();
            ViewBag.Users = await _context.Users.Where(u => u.IsActive).ToListAsync();
            return View(model);
        }

        // GET: AdminEVoucher/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVoucher = await _context.EVoucher
                .Include(e => e.Users)
                .Include(e => e.EVoucherType)
                .FirstOrDefaultAsync(m => m.EVoucherID == id);

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
            var eVoucher = await _context.EVoucher.FindAsync(id);
            if (eVoucher != null)
            {
                _context.EVoucher.Remove(eVoucher);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "電子禮券刪除成功";
            }

            return RedirectToAction(nameof(Index));
        }

        // 切換禮券使用狀態
        [HttpPost]
        public async Task<IActionResult> ToggleUsage(int id)
        {
            var eVoucher = await _context.EVoucher.FindAsync(id);
            if (eVoucher != null)
            {
                eVoucher.IsUsed = !eVoucher.IsUsed;
                if (eVoucher.IsUsed && !eVoucher.UsedTime.HasValue)
                {
                    eVoucher.UsedTime = DateTime.Now;
                }
                else if (!eVoucher.IsUsed)
                {
                    eVoucher.UsedTime = null;
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
            var now = DateTime.Now;
            var stats = new
            {
                total = await _context.EVoucher.CountAsync(),
                used = await _context.EVoucher.CountAsync(e => e.IsUsed),
                unused = await _context.EVoucher.CountAsync(e => !e.IsUsed),
                expired = await _context.EVoucher.CountAsync(e => !e.IsUsed && e.EVoucherType.ValidTo < now),
                types = await _context.EVoucherType.CountAsync()
            };

            return Json(stats);
        }

        // 獲取禮券類型分佈
        [HttpGet]
        public async Task<IActionResult> GetEVoucherTypeDistribution()
        {
            var distribution = await _context.EVoucher
                .GroupBy(e => e.EVoucherType.Name)
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

            var trend = await _context.EVoucher
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

        private bool EVoucherExists(int id)
        {
            return _context.EVoucher.Any(e => e.EVoucherID == id);
        }

        #region EVoucherType Management

        // GET: AdminEVoucher/EVoucherTypes
        public async Task<IActionResult> EVoucherTypes(string searchTerm = "", string status = "", string sortBy = "name", int page = 1, int pageSize = 10)
        {
            var query = _context.EVoucherType.AsQueryable();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(et => et.Name.Contains(searchTerm) || et.Description.Contains(searchTerm));
            }

            // 狀態篩選
            if (!string.IsNullOrEmpty(status))
            {
                var now = DateTime.Now;
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
                var typeId = item.EVoucherTypeId;
                var totalVouchers = await _context.EVoucher.CountAsync(e => e.EVoucherTypeID == typeId);
                var usedVouchers = await _context.EVoucher.CountAsync(e => e.EVoucherTypeID == typeId && e.IsUsed);

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

        // GET: AdminEVoucher/CreateEVoucherType
        public IActionResult CreateEVoucherType()
        {
            return View();
        }

        // POST: AdminEVoucher/CreateEVoucherType
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEVoucherType([Bind("EVoucherTypeId,Name,ValueAmount,ValidFrom,ValidTo,PointsCost,TotalAvailable,Description")] EVoucherType eVoucherType)
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
                if (await _context.EVoucherType.AnyAsync(et => et.Name == eVoucherType.Name))
                {
                    ModelState.AddModelError("Name", "此禮券類型名稱已存在");
                    return View(eVoucherType);
                }

                _context.Add(eVoucherType);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "電子禮券類型建立成功";
                return RedirectToAction(nameof(EVoucherTypes));
            }

            return View(eVoucherType);
        }

        // GET: AdminEVoucher/EditEVoucherType/5
        public async Task<IActionResult> EditEVoucherType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVoucherType = await _context.EVoucherType.FindAsync(id);
            if (eVoucherType == null)
            {
                return NotFound();
            }

            return View(eVoucherType);
        }

        // POST: AdminEVoucher/EditEVoucherType/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEVoucherType(int id, [Bind("EVoucherTypeId,Name,ValueAmount,ValidFrom,ValidTo,PointsCost,TotalAvailable,Description")] EVoucherType eVoucherType)
        {
            if (id != eVoucherType.EVoucherTypeId)
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
                if (await _context.EVoucherType.AnyAsync(et => et.Name == eVoucherType.Name && et.EVoucherTypeId != id))
                {
                    ModelState.AddModelError("Name", "此禮券類型名稱已被其他類型使用");
                    return View(eVoucherType);
                }

                try
                {
                    _context.Update(eVoucherType);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "電子禮券類型更新成功";
                    return RedirectToAction(nameof(EVoucherTypes));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EVoucherTypeExists(eVoucherType.EVoucherTypeId))
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

        // GET: AdminEVoucher/DeleteEVoucherType/5
        public async Task<IActionResult> DeleteEVoucherType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVoucherType = await _context.EVoucherType
                .FirstOrDefaultAsync(m => m.EVoucherTypeId == id);

            if (eVoucherType == null)
            {
                return NotFound();
            }

            // 檢查是否有相關的電子禮券
            var relatedVouchersCount = await _context.EVoucher.CountAsync(e => e.EVoucherTypeID == id);
            ViewBag.RelatedVouchersCount = relatedVouchersCount;

            return View(eVoucherType);
        }

        // POST: AdminEVoucher/DeleteEVoucherType/5
        [HttpPost, ActionName("DeleteEVoucherType")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEVoucherTypeConfirmed(int id)
        {
            // 檢查是否有相關的電子禮券
            var relatedVouchersCount = await _context.EVoucher.CountAsync(e => e.EVoucherTypeID == id);
            if (relatedVouchersCount > 0)
            {
                TempData["ErrorMessage"] = $"無法刪除：此禮券類型仍有 {relatedVouchersCount} 個相關電子禮券";
                return RedirectToAction(nameof(EVoucherTypes));
            }

            var eVoucherType = await _context.EVoucherType.FindAsync(id);
            if (eVoucherType != null)
            {
                _context.EVoucherType.Remove(eVoucherType);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "電子禮券類型刪除成功";
            }

            return RedirectToAction(nameof(EVoucherTypes));
        }

        // GET: AdminEVoucher/EVoucherTypeDetails/5
        public async Task<IActionResult> EVoucherTypeDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVoucherType = await _context.EVoucherType
                .FirstOrDefaultAsync(m => m.EVoucherTypeId == id);

            if (eVoucherType == null)
            {
                return NotFound();
            }

            // 取得使用此類型的電子禮券統計
            var relatedVouchers = await _context.EVoucher
                .Include(e => e.Users)
                .Where(e => e.EVoucherTypeID == id)
                .ToListAsync();

            var now = DateTime.Now;

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
            var eVoucherType = await _context.EVoucherType.FindAsync(id);
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

        // GET: AdminEVoucher/GetEVoucherTypeStats
        [HttpGet]
        public async Task<IActionResult> GetEVoucherTypeStats()
        {
            var now = DateTime.Now;
            var stats = new
            {
                totalTypes = await _context.EVoucherType.CountAsync(),
                activeTypes = await _context.EVoucherType.CountAsync(et => et.ValidFrom <= now && et.ValidTo >= now),
                upcomingTypes = await _context.EVoucherType.CountAsync(et => et.ValidFrom > now),
                expiredTypes = await _context.EVoucherType.CountAsync(et => et.ValidTo < now),
                totalValue = await _context.EVoucherType.SumAsync(et => et.ValueAmount * et.TotalAvailable),
                totalAvailable = await _context.EVoucherType.SumAsync(et => et.TotalAvailable)
            };

            return Json(stats);
        }

        // GET: AdminEVoucher/GetTopEVoucherTypes
        [HttpGet]
        public async Task<IActionResult> GetTopEVoucherTypes(int top = 5)
        {
            var typeStats = new List<object>();

            var types = await _context.EVoucherType
                .OrderByDescending(et => et.TotalAvailable)
                .Take(top)
                .ToListAsync();

            foreach (var type in types)
            {
                var totalVouchers = await _context.EVoucher.CountAsync(e => e.EVoucherTypeID == type.EVoucherTypeId);
                var usedVouchers = await _context.EVoucher.CountAsync(e => e.EVoucherTypeID == type.EVoucherTypeId && e.IsUsed);

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
            var eVoucherType = await _context.EVoucherType.FindAsync(typeId);
            if (eVoucherType == null)
            {
                TempData["ErrorMessage"] = "找不到該禮券類型";
                return RedirectToAction(nameof(EVoucherTypes));
            }

            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "發放數量必須大於 0";
                return RedirectToAction(nameof(EVoucherTypeDetails), new { id = typeId });
            }

            if (eVoucherType.TotalAvailable < quantity * userIds.Count)
            {
                TempData["ErrorMessage"] = $"可用數量不足。需要 {quantity * userIds.Count}，但只有 {eVoucherType.TotalAvailable} 可用";
                return RedirectToAction(nameof(EVoucherTypeDetails), new { id = typeId });
            }

            var issuedCount = 0;
            var random = new Random();

            foreach (var userId in userIds)
            {
                for (int i = 0; i < quantity; i++)
                {
                    var code = $"EV-{eVoucherType.Name.Substring(0, Math.Min(4, eVoucherType.Name.Length)).ToUpper()}-{random.Next(1000, 9999)}-{random.Next(100000, 999999)}";

                    // 確保代碼唯一
                    while (await _context.EVoucher.AnyAsync(e => e.EVoucherCode == code))
                    {
                        code = $"EV-{eVoucherType.Name.Substring(0, Math.Min(4, eVoucherType.Name.Length)).ToUpper()}-{random.Next(1000, 9999)}-{random.Next(100000, 999999)}";
                    }

                    var voucher = new EVoucher
                    {
                        EVoucherTypeID = typeId,
                        UserID = userId,
                        EVoucherCode = code,
                        AcquiredTime = DateTime.Now,
                        IsUsed = false
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
            return RedirectToAction(nameof(EVoucherTypeDetails), new { id = typeId });
        }

        private bool EVoucherTypeExists(int id)
        {
            return _context.EVoucherType.Any(e => e.EVoucherTypeId == id);
        }

        #endregion
    }
}
