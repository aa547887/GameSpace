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
    public class AdminEVoucherController : MiniGameBaseController
    {
        private readonly IMiniGameAdminService _adminService;

        public AdminEVoucherController(GameSpacedatabaseContext context, IMiniGameAdminService adminService) : base(context)
        {
            _adminService = adminService;
        }

        // 電子禮券類型管理
        public async Task<IActionResult> Index(EVoucherTypeQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            try
            {
                var result = await QueryEVoucherTypesAsync(query);
                var viewModel = new AdminEVoucherTypeListViewModel
                {
                    EVoucherTypes = result.Items,
                    Query = query,
                    TotalCount = result.TotalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢電子禮券類型時發生錯誤：{ex.Message}";
                return View(new AdminEVoucherTypeListViewModel());
            }
        }

        // 新增電子禮券類型
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var viewModel = new CreateEVoucherTypeViewModel();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入新增電子禮券類型頁面時發生錯誤：{ex.Message}";
                return View(new CreateEVoucherTypeViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEVoucherTypeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await CreateEVoucherTypeAsync(model);
                TempData["SuccessMessage"] = "電子禮券類型創建成功！";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"創建失敗：{ex.Message}");
                return View(model);
            }
        }

        // 編輯電子禮券類型
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var eVoucherType = await GetEVoucherTypeByIdAsync(id);
                if (eVoucherType == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的電子禮券類型";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new EditEVoucherTypeViewModel
                {
                    EVoucherTypeId = eVoucherType.EVoucherTypeId,
                    Name = eVoucherType.Name,
                    Description = eVoucherType.Description,
                    ValueAmount = eVoucherType.ValueAmount,
                    IsActive = eVoucherType.IsActive,
                    ValidFrom = eVoucherType.ValidFrom,
                    ValidTo = eVoucherType.ValidTo
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入編輯電子禮券類型頁面時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditEVoucherTypeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await UpdateEVoucherTypeAsync(model);
                TempData["SuccessMessage"] = "電子禮券類型更新成功！";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新失敗：{ex.Message}");
                return View(model);
            }
        }

        // 刪除電子禮券類型
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await DeleteEVoucherTypeAsync(id);
                return Json(new { success = true, message = "電子禮券類型刪除成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 切換電子禮券類型狀態
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                await ToggleEVoucherTypeStatusAsync(id);
                return Json(new { success = true, message = "電子禮券類型狀態切換成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取電子禮券類型詳情
        [HttpGet]
        public async Task<IActionResult> GetDetails(int id)
        {
            try
            {
                var eVoucherType = await GetEVoucherTypeByIdAsync(id);
                if (eVoucherType == null)
                    return Json(new { success = false, message = "找不到指定的電子禮券類型" });

                var details = new
                {
                    eVoucherTypeId = eVoucherType.EVoucherTypeId,
                    name = eVoucherType.Name,
                    description = eVoucherType.Description,
                    valueAmount = eVoucherType.ValueAmount,
                    isActive = eVoucherType.IsActive,
                    validFrom = eVoucherType.ValidFrom,
                    validTo = eVoucherType.ValidTo,
                    createdTime = eVoucherType.CreatedTime
                };

                return Json(new { success = true, data = details });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 私有方法
        private async Task<PagedResult<EVoucherTypeModel>> QueryEVoucherTypesAsync(EVoucherTypeQueryModel query)
        {
            var queryable = _context.EVoucherTypes.AsQueryable();

            if (!string.IsNullOrEmpty(query.Name))
                queryable = queryable.Where(e => e.Name.Contains(query.Name));

            if (query.IsActive.HasValue)
                queryable = queryable.Where(e => e.IsActive == query.IsActive.Value);

            if (query.MinValueAmount.HasValue)
                queryable = queryable.Where(e => e.ValueAmount >= query.MinValueAmount.Value);

            if (query.MaxValueAmount.HasValue)
                queryable = queryable.Where(e => e.ValueAmount <= query.MaxValueAmount.Value);

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(e => e.CreatedTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(e => new EVoucherTypeModel
                {
                    EVoucherTypeId = e.EVoucherTypeId,
                    Name = e.Name,
                    Description = e.Description,
                    ValueAmount = e.ValueAmount,
                    IsActive = e.IsActive,
                    ValidFrom = e.ValidFrom,
                    ValidTo = e.ValidTo,
                    CreatedTime = e.CreatedTime
                })
                .ToListAsync();

            return new PagedResult<EVoucherTypeModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        private async Task<EVoucherTypeModel> GetEVoucherTypeByIdAsync(int id)
        {
            var eVoucherType = await _context.EVoucherTypes.FindAsync(id);
            if (eVoucherType == null)
                return null;

            return new EVoucherTypeModel
            {
                EVoucherTypeId = eVoucherType.EVoucherTypeId,
                Name = eVoucherType.Name,
                Description = eVoucherType.Description,
                ValueAmount = eVoucherType.ValueAmount,
                IsActive = eVoucherType.IsActive,
                ValidFrom = eVoucherType.ValidFrom,
                ValidTo = eVoucherType.ValidTo,
                CreatedTime = eVoucherType.CreatedTime
            };
        }

        private async Task CreateEVoucherTypeAsync(CreateEVoucherTypeViewModel model)
        {
            var eVoucherType = new EVoucherType
            {
                Name = model.Name,
                Description = model.Description,
                ValueAmount = model.ValueAmount,
                IsActive = model.IsActive,
                ValidFrom = model.ValidFrom,
                ValidTo = model.ValidTo,
                CreatedTime = DateTime.Now
            };

            _context.EVoucherTypes.Add(eVoucherType);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateEVoucherTypeAsync(EditEVoucherTypeViewModel model)
        {
            var eVoucherType = await _context.EVoucherTypes.FindAsync(model.EVoucherTypeId);
            if (eVoucherType == null)
                throw new Exception("找不到指定的電子禮券類型");

            eVoucherType.Name = model.Name;
            eVoucherType.Description = model.Description;
            eVoucherType.ValueAmount = model.ValueAmount;
            eVoucherType.IsActive = model.IsActive;
            eVoucherType.ValidFrom = model.ValidFrom;
            eVoucherType.ValidTo = model.ValidTo;

            await _context.SaveChangesAsync();
        }

        private async Task DeleteEVoucherTypeAsync(int id)
        {
            var eVoucherType = await _context.EVoucherTypes.FindAsync(id);
            if (eVoucherType == null)
                throw new Exception("找不到指定的電子禮券類型");

            _context.EVoucherTypes.Remove(eVoucherType);
            await _context.SaveChangesAsync();
        }

        private async Task ToggleEVoucherTypeStatusAsync(int id)
        {
            var eVoucherType = await _context.EVoucherTypes.FindAsync(id);
            if (eVoucherType == null)
                throw new Exception("找不到指定的電子禮券類型");

            eVoucherType.IsActive = !eVoucherType.IsActive;
            await _context.SaveChangesAsync();
        }
    }

    // ViewModels
    public class EVoucherTypeQueryModel
    {
        public string Name { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public decimal? MinValueAmount { get; set; }
        public decimal? MaxValueAmount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AdminEVoucherTypeListViewModel
    {
        public List<EVoucherTypeModel> EVoucherTypes { get; set; } = new();
        public EVoucherTypeQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class CreateEVoucherTypeViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal ValueAmount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime ValidFrom { get; set; } = DateTime.Now;
        public DateTime? ValidTo { get; set; }
    }

    public class EditEVoucherTypeViewModel
    {
        public int EVoucherTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal ValueAmount { get; set; }
        public bool IsActive { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }

    public class EVoucherTypeModel
    {
        public int EVoucherTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal ValueAmount { get; set; }
        public bool IsActive { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
