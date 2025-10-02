using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminCouponController : MiniGameBaseController
    {
        public AdminCouponController(GameSpacedatabaseContext context) : base(context)
        {
        }

        // GET: AdminCoupon
        public async Task<IActionResult> Index(string searchTerm = "", string status = "", string sortBy = "name", int page = 1, int pageSize = 10)
        {
            var query = _context.Coupon
                .Include(c => c.CouponType)
                .Include(c => c.Users)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.CouponCode.Contains(searchTerm) || c.Users.User_name.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "used")
                    query = query.Where(c => c.IsUsed);
                else if (status == "unused")
                    query = query.Where(c => !c.IsUsed);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new AdminWalletIndexViewModel
            {
                Coupons = new PagedResult<UserCouponModel>
                {
                    Items = items.Select(c => new UserCouponModel
                    {
                        CouponID = c.CouponId,
                        CouponCode = c.CouponCode,
                        UserID = c.UserId,
                        UserName = c.Users?.User_name ?? "Unknown",
                        CouponTypeID = c.CouponTypeId,
                        CouponTypeName = c.CouponType?.Name ?? "Unknown",
                        IsUsed = c.IsUsed,
                        AcquiredTime = c.AcquiredTime,
                        UsedTime = c.UsedTime,
                        UsedInOrderID = c.UsedInOrderID
                    }).ToList(),
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                },
                CouponQuery = new CouponQueryModel
                {
                    SearchTerm = searchTerm,
                    Status = status,
                    Page = page,
                    PageSize = pageSize
                }
            };

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
                .Include(c => c.CouponType)
                .Include(c => c.Users)
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
            ViewData["CouponTypeId"] = _context.CouponType.ToList();
            ViewData["UserId"] = _context.Users.ToList();
            return View();
        }

        // POST: AdminCoupon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CouponId,CouponCode,UserId,CouponTypeId,IsUsed,AcquiredTime,UsedTime,UsedInOrderID")] Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Coupon.AnyAsync(c => c.CouponCode == coupon.CouponCode))
                {
                    ModelState.AddModelError("CouponCode", "此優惠券代碼已存在");
                    return View(coupon);
                }

                _context.Add(coupon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "優惠券建立成功";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CouponTypeId"] = _context.CouponType.ToList();
            ViewData["UserId"] = _context.Users.ToList();
            return View(coupon);
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
            ViewData["CouponTypeId"] = _context.CouponType.ToList();
            ViewData["UserId"] = _context.Users.ToList();
            return View(coupon);
        }

        // POST: AdminCoupon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CouponId,CouponCode,UserId,CouponTypeId,IsUsed,AcquiredTime,UsedTime,UsedInOrderID")] Coupon coupon)
        {
            if (id != coupon.CouponId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (await _context.Coupon.AnyAsync(c => c.CouponCode == coupon.CouponCode && c.CouponId != id))
                    {
                        ModelState.AddModelError("CouponCode", "此優惠券代碼已被其他優惠券使用");
                        return View(coupon);
                    }

                    _context.Update(coupon);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "優惠券更新成功";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CouponExists(coupon.CouponId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewData["CouponTypeId"] = _context.CouponType.ToList();
            ViewData["UserId"] = _context.Users.ToList();
            return View(coupon);
        }

        // GET: AdminCoupon/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coupon = await _context.Coupon
                .Include(c => c.CouponType)
                .Include(c => c.Users)
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

        private bool CouponExists(int id)
        {
            return _context.Coupon.Any(e => e.CouponId == id);
        }
    }
}
