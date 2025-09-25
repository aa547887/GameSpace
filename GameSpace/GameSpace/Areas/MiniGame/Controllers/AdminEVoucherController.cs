using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Filters;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]
    [MiniGameModulePermission("UserWallet")]
    public class AdminEVoucherController : Controller
    {
        private readonly IMiniGameAdminService _adminService;

        public AdminEVoucherController(IMiniGameAdminService adminService)
        {
            _adminService = adminService;
        }

        // GET: MiniGame/AdminEVoucher
        public async Task<IActionResult> Index(EVoucherQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            var eVouchers = await _adminService.QueryUserEVouchersAsync(query);

            var viewModel = new AdminEVoucherIndexViewModel
            {
                UserEVouchers = eVouchers,
                Query = query
            };

            return View(viewModel);
        }

        // GET: MiniGame/AdminEVoucher/Details/5
        public async Task<IActionResult> Details(int eVoucherId)
        {
            try
            {
                var eVoucher = await _adminService.GetEVoucherDetailsAsync(eVoucherId);
                if (eVoucher == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的電子禮券";
                    return RedirectToAction(nameof(Index));
                }

                return View(eVoucher);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入電子禮券詳情時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: MiniGame/AdminEVoucher/Create
        public async Task<IActionResult> Create()
        {
            var eVoucherTypes = await _adminService.GetEVoucherTypesAsync();
            var viewModel = new AdminEVoucherCreateViewModel
            {
                EVoucherTypes = eVoucherTypes
            };
            return View(viewModel);
        }

        // POST: MiniGame/AdminEVoucher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminEVoucherCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.EVoucherTypes = await _adminService.GetEVoucherTypesAsync();
                return View(model);
            }

            try
            {
                var success = await _adminService.CreateEVoucherAsync(new EVoucherCreateModel
                {
                    EVoucherName = model.EVoucherName,
                    EVoucherCode = model.EVoucherCode,
                    Value = model.Value,
                    Quantity = model.Quantity,
                    ExpiryDate = model.ExpiryDate
                });

                if (success)
                {
                    TempData["SuccessMessage"] = "電子禮券創建成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "電子禮券創建失敗";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"電子禮券創建失敗：{ex.Message}";
            }

            model.EVoucherTypes = await _adminService.GetEVoucherTypesAsync();
            return View(model);
        }

        // GET: MiniGame/AdminEVoucher/Edit/5
        public async Task<IActionResult> Edit(int eVoucherId)
        {
            try
            {
                var eVoucher = await _adminService.GetEVoucherForEditAsync(eVoucherId);
                if (eVoucher == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的電子禮券";
                    return RedirectToAction(nameof(Index));
                }

                var eVoucherTypes = await _adminService.GetEVoucherTypesAsync();
                var viewModel = new AdminEVoucherEditViewModel
                {
                    EVoucherId = eVoucher.EVoucherId,
                    EVoucherName = eVoucher.EVoucherName,
                    EVoucherCode = eVoucher.EVoucherCode,
                    Value = eVoucher.Value,
                    Quantity = eVoucher.Quantity,
                    ExpiryDate = eVoucher.ExpiryDate,
                    EVoucherTypes = eVoucherTypes
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入電子禮券時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: MiniGame/AdminEVoucher/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminEVoucherEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.EVoucherTypes = await _adminService.GetEVoucherTypesAsync();
                return View(model);
            }

            try
            {
                var success = await _adminService.UpdateEVoucherAsync(new EVoucherUpdateModel
                {
                    EVoucherId = model.EVoucherId,
                    EVoucherName = model.EVoucherName,
                    EVoucherCode = model.EVoucherCode,
                    Value = model.Value,
                    Quantity = model.Quantity,
                    ExpiryDate = model.ExpiryDate
                });

                if (success)
                {
                    TempData["SuccessMessage"] = "電子禮券更新成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "電子禮券更新失敗";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"電子禮券更新失敗：{ex.Message}";
            }

            model.EVoucherTypes = await _adminService.GetEVoucherTypesAsync();
            return View(model);
        }

        // POST: MiniGame/AdminEVoucher/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int eVoucherId)
        {
            try
            {
                var success = await _adminService.DeleteEVoucherAsync(eVoucherId);
                if (success)
                {
                    TempData["SuccessMessage"] = "電子禮券刪除成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "電子禮券刪除失敗";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"電子禮券刪除失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: MiniGame/AdminEVoucher/IssueToUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IssueToUser(AdminEVoucherIssueViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "請填寫所有必要欄位";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var success = await _adminService.IssueEVoucherToUserAsync(model.UserId, model.EVoucherId, model.Quantity);

                if (success)
                {
                    TempData["SuccessMessage"] = "電子禮券發放成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "電子禮券發放失敗";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"電子禮券發放失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    // ViewModels
    public class AdminEVoucherIndexViewModel
    {
        public PagedResult<UserEVoucherReadModel> UserEVouchers { get; set; } = new();
        public EVoucherQueryModel Query { get; set; } = new();
        public string Sidebar { get; set; } = "admin";
    }

    public class AdminEVoucherCreateViewModel
    {
        [Required(ErrorMessage = "電子禮券名稱不能為空")]
        [StringLength(100, ErrorMessage = "電子禮券名稱不能超過 100 個字元")]
        public string EVoucherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "電子禮券代碼不能為空")]
        [StringLength(50, ErrorMessage = "電子禮券代碼不能超過 50 個字元")]
        public string EVoucherCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "面額不能為空")]
        [Range(0.01, 999999, ErrorMessage = "面額必須在 0.01-999999 之間")]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "數量不能為空")]
        [Range(1, 999999, ErrorMessage = "數量必須在 1-999999 之間")]
        public int Quantity { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ExpiryDate { get; set; }

        public List<EVoucherTypeReadModel> EVoucherTypes { get; set; } = new();
        public string Sidebar { get; set; } = "admin";
    }

    public class AdminEVoucherEditViewModel
    {
        public int EVoucherId { get; set; }

        [Required(ErrorMessage = "電子禮券名稱不能為空")]
        [StringLength(100, ErrorMessage = "電子禮券名稱不能超過 100 個字元")]
        public string EVoucherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "電子禮券代碼不能為空")]
        [StringLength(50, ErrorMessage = "電子禮券代碼不能超過 50 個字元")]
        public string EVoucherCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "面額不能為空")]
        [Range(0.01, 999999, ErrorMessage = "面額必須在 0.01-999999 之間")]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "數量不能為空")]
        [Range(1, 999999, ErrorMessage = "數量必須在 1-999999 之間")]
        public int Quantity { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ExpiryDate { get; set; }

        public List<EVoucherTypeReadModel> EVoucherTypes { get; set; } = new();
        public string Sidebar { get; set; } = "admin";
    }

    public class AdminEVoucherIssueViewModel
    {
        [Required(ErrorMessage = "請選擇用戶")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "請選擇電子禮券")]
        public int EVoucherId { get; set; }

        [Required(ErrorMessage = "請輸入數量")]
        [Range(1, 999, ErrorMessage = "數量必須在 1-999 之間")]
        public int Quantity { get; set; }
    }
}
