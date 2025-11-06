using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Infrastructure.Time;
using GameSpace.Areas.MiniGame.Services;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class AdminCouponController : MiniGameBaseController
    {
        private readonly IFuzzySearchService _fuzzySearchService;

        public AdminCouponController(GameSpacedatabaseContext context, IAppClock appClock, IFuzzySearchService fuzzySearchService) : base(context, appClock)
        {
            _fuzzySearchService = fuzzySearchService;
        }

        // GET: AdminCoupon
        public async Task<IActionResult> Index(string searchTerm = "", string status = "", string sortBy = "name", int page = 1, int pageSize = 10)
        {
            var query = _context.Coupons
                .Include(c => c.CouponType)
                .Include(c => c.User)
                .AsQueryable();

            // 模糊搜尋：SearchTerm（聯集OR邏輯，使用 FuzzySearchService）
            var hasSearchTerm = !string.IsNullOrWhiteSpace(searchTerm);

            List<int> matchedCouponIds = new List<int>();
            Dictionary<int, int> couponPriority = new Dictionary<int, int>();

            if (hasSearchTerm)
            {
                var term = searchTerm.Trim();

                // 查詢所有優惠券並使用 FuzzySearchService 計算優先順序
                var allCoupons = await _context.Coupons
                    .Include(c => c.User)
                    .AsNoTracking()
                    .Select(c => new { c.CouponId, c.CouponCode, UserName = c.User != null ? c.User.UserName : "", UserAccount = c.User != null ? c.User.UserAccount : "" })
                    .ToListAsync();

                foreach (var coupon in allCoupons)
                {
                    int priority = 0;

                    // 優惠券代碼精確匹配優先
                    if (coupon.CouponCode.Equals(term, StringComparison.OrdinalIgnoreCase))
                    {
                        priority = 1; // 完全匹配 CouponCode
                    }
                    else if (coupon.CouponCode.StartsWith(term, StringComparison.OrdinalIgnoreCase))
                    {
                        priority = 2; // 開頭匹配
                    }
                    else if (coupon.CouponCode.Contains(term, StringComparison.OrdinalIgnoreCase))
                    {
                        priority = 3; // 包含匹配
                    }

                    // 如果優惠券代碼沒有匹配，嘗試用戶名模糊搜尋
                    if (priority == 0)
                    {
                        priority = _fuzzySearchService.CalculateMatchPriority(
                            term,
                            coupon.UserAccount,
                            coupon.UserName
                        );
                    }

                    // 如果匹配成功（priority > 0），加入結果
                    if (priority > 0)
                    {
                        matchedCouponIds.Add(coupon.CouponId);
                        couponPriority[coupon.CouponId] = priority;
                    }
                }

                query = query.Where(c => matchedCouponIds.Contains(c.CouponId));
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "used")
                    query = query.Where(c => c.IsUsed);
                else if (status == "unused")
                    query = query.Where(c => !c.IsUsed);
            }

            // 計算統計數據（從篩選後的查詢）
            var statsQuery = query;
            var totalCount = await statsQuery.CountAsync();

            // 優先順序排序：先取資料再排序
            var allItems = await query.ToListAsync();
            var items = allItems;

            if (hasSearchTerm)
            {
                // 在記憶體中進行優先順序排序
                var ordered = allItems.OrderBy(c =>
                {
                    // 如果優惠券匹配，返回對應優先順序
                    if (couponPriority.ContainsKey(c.CouponId))
                    {
                        return couponPriority[c.CouponId];
                    }
                    return 99;
                });

                items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                // 沒有搜尋條件時使用預設排序
                items = allItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            var viewModel = new AdminWalletIndexViewModel
            {
                Coupons = new PagedResult<UserCouponModel>
                {
                    Items = items.Select(c => new UserCouponModel
                    {
                        CouponID = c.CouponId,
                        CouponCode = c.CouponCode,
                        UserID = c.UserId,
                        UserName = c.User?.UserName ?? "Unknown",
                        CouponTypeID = c.CouponTypeId,
                        CouponTypeName = c.CouponType?.Name ?? "Unknown",
                        IsUsed = c.IsUsed,
                        AcquiredTime = c.AcquiredTime,
                        UsedTime = c.UsedTime,
                        UsedInOrderID = c.UsedInOrderId
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

            var coupon = await _context.Coupons
                .Include(c => c.CouponType)
                .Include(c => c.User)
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
            return View(new AdminCouponCreateViewModel());
        }

        // POST: AdminCoupon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,CouponTypeId,CouponCode,CouponName,DiscountType,DiscountValue,MinOrderAmount,MaxDiscountAmount,StartDate,EndDate,UsageLimit,IsActive,Description")] AdminCouponCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Coupons.AnyAsync(c => c.CouponCode == model.CouponCode))
                {
                    ModelState.AddModelError("CouponCode", "此優惠券代碼已存在");
                    return View(model);
                }

                // 使用台灣時間
                var nowUtc = _appClock.UtcNow;
                var nowTaiwanTime = _appClock.ToAppTime(nowUtc);

                var coupon = new Coupon
                {
                    CouponCode = model.CouponCode,
                    UserId = model.UserId,
                    CouponTypeId = model.CouponTypeId,
                    IsUsed = false,
                    AcquiredTime = nowTaiwanTime,  // 使用台灣時間
                    UsedTime = null,  // 明確設為 null (剛發放時未使用)
                    UsedInOrderId = null,
                    IsDeleted = false  // 必填字段：未刪除
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

            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }
            
            // 轉換為 SelectListItem 格式
            ViewData["CouponTypeId"] = _context.CouponTypes.Select(ct => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = ct.CouponTypeId.ToString(),
                Text = ct.Name
            }).ToList();
            
            ViewData["UserId"] = _context.Users.Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = u.UserId.ToString(),
                Text = u.UserName
            }).ToList();
            
            return View(coupon);
        }

        // POST: AdminCoupon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CouponId,CouponCode,UserId,CouponTypeId,IsUsed,AcquiredTime,UsedTime,UsedInOrderId")] Coupon coupon)
        {
            if (id != coupon.CouponId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (await _context.Coupons.AnyAsync(c => c.CouponCode == coupon.CouponCode && c.CouponId != id))
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
            
            // 轉換為 SelectListItem 格式
            ViewData["CouponTypeId"] = _context.CouponTypes.Select(ct => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = ct.CouponTypeId.ToString(),
                Text = ct.Name
            }).ToList();
            
            ViewData["UserId"] = _context.Users.Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = u.UserId.ToString(),
                Text = u.UserName
            }).ToList();
            
            return View(coupon);
        }

        // GET: AdminCoupon/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coupon = await _context.Coupons
                .Include(c => c.CouponType)
                .Include(c => c.User)
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
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "優惠券刪除成功";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CouponExists(int id)
        {
            return _context.Coupons.Any(e => e.CouponId == id);
        }

        #region CouponType Management

        // GET: AdminCoupon/CouponTypes
        public async Task<IActionResult> CouponTypes(string searchTerm = "", string discountType = "", string sortBy = "id", int page = 1, int pageSize = 10)
        {
            // 只查詢未刪除的優惠券類型
            var query = _context.CouponTypes.Where(ct => !ct.IsDeleted).AsQueryable();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(ct => ct.Name.Contains(searchTerm) || (ct.Description != null && ct.Description.Contains(searchTerm)));
            }

            // 折扣類型篩選
            if (!string.IsNullOrEmpty(discountType))
            {
                query = query.Where(ct => ct.DiscountType == discountType);
            }

            // 排序 (預設按 ID 升序排列)
            query = sortBy switch
            {
                "name" => query.OrderBy(ct => ct.Name),
                "points" => query.OrderBy(ct => ct.PointsCost),
                "discount" => query.OrderByDescending(ct => ct.DiscountValue),
                "validfrom" => query.OrderBy(ct => ct.ValidFrom),
                _ => query.OrderBy(ct => ct.CouponTypeId) // 預設按 ID 升序
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new
            {
                CouponTypes = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                DiscountType = discountType,
                SortBy = sortBy
            };

            ViewBag.TotalCount = totalCount; // 提供給頁面統計卡片使用
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.DiscountType = discountType;
            ViewBag.SortBy = sortBy;

            return View(viewModel);
        }

        // GET: AdminCoupon/CreateCouponType
        public IActionResult CreateCouponType()
        {
            ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
            return View();
        }

        // POST: AdminCoupon/CreateCouponType
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCouponType([Bind("CouponTypeId,Name,DiscountType,DiscountValue,MinSpend,ValidFrom,ValidTo,PointsCost,Description")] CouponType couponType)
        {
            if (ModelState.IsValid)
            {
                // 驗證邏輯
                if (couponType.ValidFrom >= couponType.ValidTo)
                {
                    ModelState.AddModelError("ValidFrom", "開始日期必須早於結束日期");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                if (couponType.DiscountType == "Percentage" && (couponType.DiscountValue < 0 || couponType.DiscountValue > 100))
                {
                    ModelState.AddModelError("DiscountValue", "百分比折扣必須在 0-100 之間");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                if (couponType.DiscountValue < 0)
                {
                    ModelState.AddModelError("DiscountValue", "折扣值不能為負數");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                if (couponType.MinSpend < 0)
                {
                    ModelState.AddModelError("MinSpend", "最低消費金額不能為負數");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                if (couponType.PointsCost < 0)
                {
                    ModelState.AddModelError("PointsCost", "所需點數不能為負數");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                // 檢查優惠券類型名稱是否重複
                var existingCouponType = await _context.CouponTypes
                    .Where(ct => ct.Name == couponType.Name && !ct.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingCouponType != null)
                {
                    ModelState.AddModelError("Name", $"優惠券類型名稱「{couponType.Name}」已存在，請使用其他名稱");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                _context.Add(couponType);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "優惠券類型建立成功";
                return RedirectToAction(nameof(CouponTypes));
            }

            ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
            return View(couponType);
        }

        // GET: AdminCoupon/EditCouponType/5
        public async Task<IActionResult> EditCouponType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var couponType = await _context.CouponTypes.FindAsync(id);
            if (couponType == null)
            {
                return NotFound();
            }

            ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
            return View(couponType);
        }

        // POST: AdminCoupon/EditCouponType/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCouponType(int id, [Bind("CouponTypeId,Name,DiscountType,DiscountValue,MinSpend,ValidFrom,ValidTo,PointsCost,Description")] CouponType couponType)
        {
            if (id != couponType.CouponTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // 驗證邏輯
                if (couponType.ValidFrom >= couponType.ValidTo)
                {
                    ModelState.AddModelError("ValidFrom", "開始日期必須早於結束日期");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                if (couponType.DiscountType == "Percentage" && (couponType.DiscountValue < 0 || couponType.DiscountValue > 100))
                {
                    ModelState.AddModelError("DiscountValue", "百分比折扣必須在 0-100 之間");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                if (couponType.DiscountValue < 0)
                {
                    ModelState.AddModelError("DiscountValue", "折扣值不能為負數");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                if (couponType.MinSpend < 0)
                {
                    ModelState.AddModelError("MinSpend", "最低消費金額不能為負數");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                if (couponType.PointsCost < 0)
                {
                    ModelState.AddModelError("PointsCost", "所需點數不能為負數");
                    ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
                    return View(couponType);
                }

                try
                {
                    _context.Update(couponType);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "優惠券類型更新成功";
                    return RedirectToAction(nameof(CouponTypes));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CouponTypeExists(couponType.CouponTypeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.DiscountTypes = new List<string> { "Percentage", "FixedAmount" };
            return View(couponType);
        }

        // GET: AdminCoupon/DeleteCouponType/5
        public async Task<IActionResult> DeleteCouponType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var couponType = await _context.CouponTypes
                .FirstOrDefaultAsync(m => m.CouponTypeId == id);

            if (couponType == null)
            {
                return NotFound();
            }

            // 檢查是否有相關的優惠券
            var relatedCouponsCount = await _context.Coupons.CountAsync(c => c.CouponTypeId == id);
            ViewBag.RelatedCouponsCount = relatedCouponsCount;

            return View(couponType);
        }

        // POST: AdminCoupon/DeleteCouponType/5
        // 軟刪除優惠券類型 (設定 IsDeleted = true)
        [HttpPost, ActionName("DeleteCouponType")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCouponTypeConfirmed(int id)
        {
            try
            {
                var couponType = await _context.CouponTypes.FindAsync(id);
                if (couponType == null)
                {
                    TempData["ErrorMessage"] = "找不到要刪除的優惠券類型";
                    return RedirectToAction(nameof(CouponTypes));
                }

                // 軟刪除：設定 IsDeleted = true
                couponType.IsDeleted = true;
                couponType.DeletedAt = _appClock.UtcNow;
                couponType.DeleteReason = "管理員透過後台刪除";

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"優惠券類型「{couponType.Name}」已刪除（軟刪除）";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"刪除失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(CouponTypes));
        }

        // POST: AdminCoupon/DisableCouponType
        // 停用優惠券類型 (軟刪除，與 DeleteCouponType 相同，但訊息不同)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableCouponType(int id)
        {
            try
            {
                var couponType = await _context.CouponTypes.FindAsync(id);
                if (couponType == null)
                {
                    TempData["ErrorMessage"] = "找不到要停用的優惠券類型";
                    return RedirectToAction(nameof(CouponTypes));
                }

                // 軟刪除：設定 IsDeleted = true
                couponType.IsDeleted = true;
                couponType.DeletedAt = _appClock.UtcNow;
                couponType.DeleteReason = "管理員停用";

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"優惠券類型「{couponType.Name}」已停用";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"停用失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(CouponTypes));
        }

        // GET: AdminCoupon/CouponTypeDetails/5
        public async Task<IActionResult> CouponTypeDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var couponType = await _context.CouponTypes
                .FirstOrDefaultAsync(m => m.CouponTypeId == id);

            if (couponType == null)
            {
                return NotFound();
            }

            // 取得使用此類型的優惠券統計
            var relatedCoupons = await _context.Coupons
                .Where(c => c.CouponTypeId == id)
                .ToListAsync();

            ViewBag.TotalCoupons = relatedCoupons.Count;
            ViewBag.UsedCoupons = relatedCoupons.Count(c => c.IsUsed);
            ViewBag.UnusedCoupons = relatedCoupons.Count(c => !c.IsUsed);

            return View(couponType);
        }

        private bool CouponTypeExists(int id)
        {
            return _context.CouponTypes.Any(e => e.CouponTypeId == id);
        }

        #endregion
    }
}

