using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Data;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminEVoucherController : Controller
    {
        private readonly GameSpaceContext _context;

        public AdminEVoucherController(GameSpaceContext context)
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
    }
}
