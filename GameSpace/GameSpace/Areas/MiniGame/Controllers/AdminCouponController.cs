using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Data;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminCouponController : Controller
    {
        private readonly GameSpaceContext _context;

        public AdminCouponController(GameSpaceContext context)
        {
            _context = context;
        }

        // GET: AdminCoupon
        public async Task<IActionResult> Index(string searchTerm = "", string status = "", string sortBy = "name", int page = 1, int pageSize = 10)
        {
            var query = _context.Coupon.AsQueryable();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.CouponName.Contains(searchTerm) || 
                                       c.CouponCode.Contains(searchTerm) || 
                                       c.Description.Contains(searchTerm));
            }

            // 狀態篩選
            if (!string.IsNullOrEmpty(status))
            {
                var now = DateTime.Now;
                query = status switch
                {
                    "active" => query.Where(c => c.IsActive && c.StartDate <= now && c.EndDate >= now),
                    "expired" => query.Where(c => c.EndDate < now),
                    "upcoming" => query.Where(c => c.StartDate > now),
                    "inactive" => query.Where(c => !c.IsActive),
                    _ => query
                };
            }

            // 排序
            query = sortBy switch
            {
                "code" => query.OrderBy(c => c.CouponCode),
                "discount" => query.OrderByDescending(c => c.DiscountValue),
                "start" => query.OrderByDescending(c => c.StartDate),
                "end" => query.OrderByDescending(c => c.EndDate),
                "usage" => query.OrderByDescending(c => c.UsageCount),
                _ => query.OrderBy(c => c.CouponName)
            };

            // 分頁
            var totalCount = await query.CountAsync();
            var coupons = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new AdminCouponIndexViewModel
            {
                Coupons = new PagedResult<Coupon>
                {
                    Items = coupons,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                }
            };

            // 設定 ViewBag 用於搜尋和篩選
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalCoupons = totalCount;
            ViewBag.ActiveCoupons = await _context.Coupon.CountAsync(c => c.IsActive && c.StartDate <= DateTime.Now && c.EndDate >= DateTime.Now);
            ViewBag.ExpiredCoupons = await _context.Coupon.CountAsync(c => c.EndDate < DateTime.Now);
            ViewBag.UpcomingCoupons = await _context.Coupon.CountAsync(c => c.StartDate > DateTime.Now);

            return View(viewModel);
        }

        // GET: AdminCoupon/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coupon = await _context.Coupon
                .Include(c => c.CouponUsages)
                .FirstOrDefaultAsync(m => m.CouponId == id);

            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }

        // GET: AdminCoupon/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminCoupon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCouponCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 檢查優惠券代碼是否已存在
                if (await _context.Coupon.AnyAsync(c => c.CouponCode == model.CouponCode))
                {
                    ModelState.AddModelError("CouponCode", "此優惠券代碼已存在");
                    return View(model);
                }

                var coupon = new Coupon
                {
                    CouponName = model.CouponName,
                    CouponCode = model.CouponCode,
                    DiscountType = model.DiscountType,
                    DiscountValue = model.DiscountValue,
                    MinOrderAmount = model.MinOrderAmount,
                    MaxDiscountAmount = model.MaxDiscountAmount,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    UsageLimit = model.UsageLimit,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now,
                    UsageCount = 0
                };

                _context.Add(coupon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "優惠券建立成功";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: AdminCoupon/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coupon = await _context.Coupon.FindAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }

            var model = new AdminCouponCreateViewModel
            {
                CouponName = coupon.CouponName,
                CouponCode = coupon.CouponCode,
                DiscountType = coupon.DiscountType,
                DiscountValue = coupon.DiscountValue,
                MinOrderAmount = coupon.MinOrderAmount,
                MaxDiscountAmount = coupon.MaxDiscountAmount,
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate,
                UsageLimit = coupon.UsageLimit,
                Description = coupon.Description,
                IsActive = coupon.IsActive
            };

            return View(model);
        }

        // POST: AdminCoupon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminCouponCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var coupon = await _context.Coupon.FindAsync(id);
                    if (coupon == null)
                    {
                        return NotFound();
                    }

                    // 檢查優惠券代碼是否已被其他優惠券使用
                    if (await _context.Coupon.AnyAsync(c => c.CouponCode == model.CouponCode && c.CouponId != id))
                    {
                        ModelState.AddModelError("CouponCode", "此優惠券代碼已被其他優惠券使用");
                        return View(model);
                    }

                    coupon.CouponName = model.CouponName;
                    coupon.CouponCode = model.CouponCode;
                    coupon.DiscountType = model.DiscountType;
                    coupon.DiscountValue = model.DiscountValue;
                    coupon.MinOrderAmount = model.MinOrderAmount;
                    coupon.MaxDiscountAmount = model.MaxDiscountAmount;
                    coupon.StartDate = model.StartDate;
                    coupon.EndDate = model.EndDate;
                    coupon.UsageLimit = model.UsageLimit;
                    coupon.Description = model.Description;
                    coupon.IsActive = model.IsActive;

                    _context.Update(coupon);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "優惠券更新成功";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CouponExists(id))
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

        // GET: AdminCoupon/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coupon = await _context.Coupon
                .Include(c => c.CouponUsages)
                .FirstOrDefaultAsync(m => m.CouponId == id);

            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }

        // POST: AdminCoupon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coupon = await _context.Coupon.FindAsync(id);
            if (coupon != null)
            {
                _context.Coupon.Remove(coupon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "優惠券刪除成功";
            }

            return RedirectToAction(nameof(Index));
        }

        // 切換優惠券狀態
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var coupon = await _context.Coupon.FindAsync(id);
            if (coupon != null)
            {
                coupon.IsActive = !coupon.IsActive;
                _context.Update(coupon);
                await _context.SaveChangesAsync();

                return Json(new { success = true, isActive = coupon.IsActive });
            }

            return Json(new { success = false });
        }

        // 獲取優惠券統計數據
        [HttpGet]
        public async Task<IActionResult> GetCouponStats()
        {
            var now = DateTime.Now;
            var stats = new
            {
                total = await _context.Coupon.CountAsync(),
                active = await _context.Coupon.CountAsync(c => c.IsActive && c.StartDate <= now && c.EndDate >= now),
                expired = await _context.Coupon.CountAsync(c => c.EndDate < now),
                upcoming = await _context.Coupon.CountAsync(c => c.StartDate > now),
                totalUsage = await _context.Coupon.SumAsync(c => c.UsageCount)
            };

            return Json(stats);
        }

        // 獲取優惠券使用趨勢
        [HttpGet]
        public async Task<IActionResult> GetCouponUsageTrend(int days = 30)
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-days);

            var trend = await _context.CouponUsage
                .Where(cu => cu.UsedAt >= startDate)
                .GroupBy(cu => cu.UsedAt.Date)
                .Select(g => new
                {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    count = g.Count()
                })
                .OrderBy(g => g.date)
                .ToListAsync();

            return Json(trend);
        }

        // 獲取優惠券類型分佈
        [HttpGet]
        public async Task<IActionResult> GetCouponTypeDistribution()
        {
            var distribution = await _context.Coupon
                .GroupBy(c => c.DiscountType)
                .Select(g => new
                {
                    type = g.Key,
                    count = g.Count()
                })
                .ToListAsync();

            return Json(distribution);
        }

        // 獲取最受歡迎的優惠券
        [HttpGet]
        public async Task<IActionResult> GetPopularCoupons(int top = 10)
        {
            var popular = await _context.Coupon
                .OrderByDescending(c => c.UsageCount)
                .Take(top)
                .Select(c => new
                {
                    couponId = c.CouponId,
                    couponName = c.CouponName,
                    couponCode = c.CouponCode,
                    usageCount = c.UsageCount,
                    usageLimit = c.UsageLimit,
                    usageRate = (double)c.UsageCount / c.UsageLimit * 100
                })
                .ToListAsync();

            return Json(popular);
        }

        private bool CouponExists(int id)
        {
            return _context.Coupon.Any(e => e.CouponId == id);
        }
    }
}
