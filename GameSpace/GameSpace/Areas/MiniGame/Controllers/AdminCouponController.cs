using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public class AdminCouponController : MiniGameBaseController
    {
        private readonly IMiniGameAdminService _adminService;

        public AdminCouponController(GameSpacedatabaseContext context, IMiniGameAdminService adminService) : base(context)
        {
            _adminService = adminService;
        }

        // 優惠券類型管理
        public async Task<IActionResult> Index(CouponTypeQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            try
            {
                var result = await QueryCouponTypesAsync(query);
                var viewModel = new AdminCouponTypeListViewModel
                {
                    CouponTypes = result.Items,
                    Query = query,
                    TotalCount = result.TotalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢優惠券類型時發生錯誤：{ex.Message}";
                return View(new AdminCouponTypeListViewModel());
            }
        }

        // 新增優惠券類型
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var viewModel = new CreateCouponTypeViewModel();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入新增優惠券類型頁面時發生錯誤：{ex.Message}";
                return View(new CreateCouponTypeViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCouponTypeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await CreateCouponTypeAsync(model);
                TempData["SuccessMessage"] = "優惠券類型創建成功！";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"創建失敗：{ex.Message}");
                return View(model);
            }
        }

        // 編輯優惠券類型
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var couponType = await GetCouponTypeByIdAsync(id);
                if (couponType == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的優惠券類型";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new EditCouponTypeViewModel
                {
                    CouponTypeId = couponType.CouponTypeId,
                    Name = couponType.Name,
                    Description = couponType.Description,
                    DiscountType = couponType.DiscountType,
                    DiscountValue = couponType.DiscountValue,
                    MinOrderAmount = couponType.MinOrderAmount,
                    MaxDiscountAmount = couponType.MaxDiscountAmount,
                    IsActive = couponType.IsActive,
                    ValidFrom = couponType.ValidFrom,
                    ValidTo = couponType.ValidTo
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入編輯優惠券類型頁面時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditCouponTypeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await UpdateCouponTypeAsync(model);
                TempData["SuccessMessage"] = "優惠券類型更新成功！";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新失敗：{ex.Message}");
                return View(model);
            }
        }

        // 刪除優惠券類型
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await DeleteCouponTypeAsync(id);
                return Json(new { success = true, message = "優惠券類型刪除成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 切換優惠券類型狀態
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                await ToggleCouponTypeStatusAsync(id);
                return Json(new { success = true, message = "優惠券類型狀態切換成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取優惠券類型詳情
        [HttpGet]
        public async Task<IActionResult> GetDetails(int id)
        {
            try
            {
                var couponType = await GetCouponTypeByIdAsync(id);
                if (couponType == null)
                    return Json(new { success = false, message = "找不到指定的優惠券類型" });

                var details = new
                {
                    couponTypeId = couponType.CouponTypeId,
                    name = couponType.Name,
                    description = couponType.Description,
                    discountType = couponType.DiscountType,
                    discountValue = couponType.DiscountValue,
                    minOrderAmount = couponType.MinOrderAmount,
                    maxDiscountAmount = couponType.MaxDiscountAmount,
                    isActive = couponType.IsActive,
                    validFrom = couponType.ValidFrom,
                    validTo = couponType.ValidTo,
                    createdTime = couponType.CreatedTime
                };

                return Json(new { success = true, data = details });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 私有方法
        private async Task<PagedResult<CouponTypeModel>> QueryCouponTypesAsync(CouponTypeQueryModel query)
        {
            var queryable = _context.CouponTypes.AsQueryable();

            if (!string.IsNullOrEmpty(query.Name))
                queryable = queryable.Where(c => c.Name.Contains(query.Name));

            if (query.IsActive.HasValue)
                queryable = queryable.Where(c => c.IsActive == query.IsActive.Value);

            if (query.DiscountType != null)
                queryable = queryable.Where(c => c.DiscountType == query.DiscountType);

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(c => c.CreatedTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new CouponTypeModel
                {
                    CouponTypeId = c.CouponTypeId,
                    Name = c.Name,
                    Description = c.Description,
                    DiscountType = c.DiscountType,
                    DiscountValue = c.DiscountValue,
                    MinOrderAmount = c.MinOrderAmount,
                    MaxDiscountAmount = c.MaxDiscountAmount,
                    IsActive = c.IsActive,
                    ValidFrom = c.ValidFrom,
                    ValidTo = c.ValidTo,
                    CreatedTime = c.CreatedTime
                })
                .ToListAsync();

            return new PagedResult<CouponTypeModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        private async Task<CouponTypeModel> GetCouponTypeByIdAsync(int id)
        {
            var couponType = await _context.CouponTypes.FindAsync(id);
            if (couponType == null)
                return null;

            return new CouponTypeModel
            {
                CouponTypeId = couponType.CouponTypeId,
                Name = couponType.Name,
                Description = couponType.Description,
                DiscountType = couponType.DiscountType,
                DiscountValue = couponType.DiscountValue,
                MinOrderAmount = couponType.MinOrderAmount,
                MaxDiscountAmount = couponType.MaxDiscountAmount,
                IsActive = couponType.IsActive,
                ValidFrom = couponType.ValidFrom,
                ValidTo = couponType.ValidTo,
                CreatedTime = couponType.CreatedTime
            };
        }

        private async Task CreateCouponTypeAsync(CreateCouponTypeViewModel model)
        {
            var couponType = new CouponType
            {
                Name = model.Name,
                Description = model.Description,
                DiscountType = model.DiscountType,
                DiscountValue = model.DiscountValue,
                MinOrderAmount = model.MinOrderAmount,
                MaxDiscountAmount = model.MaxDiscountAmount,
                IsActive = model.IsActive,
                ValidFrom = model.ValidFrom,
                ValidTo = model.ValidTo,
                CreatedTime = DateTime.Now
            };

            _context.CouponTypes.Add(couponType);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateCouponTypeAsync(EditCouponTypeViewModel model)
        {
            var couponType = await _context.CouponTypes.FindAsync(model.CouponTypeId);
            if (couponType == null)
                throw new Exception("找不到指定的優惠券類型");

            couponType.Name = model.Name;
            couponType.Description = model.Description;
            couponType.DiscountType = model.DiscountType;
            couponType.DiscountValue = model.DiscountValue;
            couponType.MinOrderAmount = model.MinOrderAmount;
            couponType.MaxDiscountAmount = model.MaxDiscountAmount;
            couponType.IsActive = model.IsActive;
            couponType.ValidFrom = model.ValidFrom;
            couponType.ValidTo = model.ValidTo;

            await _context.SaveChangesAsync();
        }

        private async Task DeleteCouponTypeAsync(int id)
        {
            var couponType = await _context.CouponTypes.FindAsync(id);
            if (couponType == null)
                throw new Exception("找不到指定的優惠券類型");

            _context.CouponTypes.Remove(couponType);
            await _context.SaveChangesAsync();
        }

        private async Task ToggleCouponTypeStatusAsync(int id)
        {
            var couponType = await _context.CouponTypes.FindAsync(id);
            if (couponType == null)
                throw new Exception("找不到指定的優惠券類型");

            couponType.IsActive = !couponType.IsActive;
            await _context.SaveChangesAsync();
        }
    }

    // ViewModels
    public class CouponTypeQueryModel
    {
        public string Name { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public string DiscountType { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AdminCouponTypeListViewModel
    {
        public List<CouponTypeModel> CouponTypes { get; set; } = new();
        public CouponTypeQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class CreateCouponTypeViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime ValidFrom { get; set; } = DateTime.Now;
        public DateTime? ValidTo { get; set; }
    }

    public class EditCouponTypeViewModel
    {
        public int CouponTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public bool IsActive { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }

    public class CouponTypeModel
    {
        public int CouponTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public bool IsActive { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
