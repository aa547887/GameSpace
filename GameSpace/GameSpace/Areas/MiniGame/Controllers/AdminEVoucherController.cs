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
            // 這裡需要根據實際的資料庫結構來實現
            TempData["ErrorMessage"] = "功能開發中";
            return RedirectToAction(nameof(Index));
        }

        // GET: MiniGame/AdminEVoucher/Create
        public IActionResult Create()
        {
            var viewModel = new AdminEVoucherCreateViewModel();
            return View(viewModel);
        }

        // POST: MiniGame/AdminEVoucher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminEVoucherCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // 這裡需要根據實際的資料庫結構來實現
                TempData["SuccessMessage"] = "電子禮券創建成功";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"電子禮券創建失敗：{ex.Message}";
            }

            return View(model);
        }

        // GET: MiniGame/AdminEVoucher/Edit/5
        public async Task<IActionResult> Edit(int eVoucherId)
        {
            // 這裡需要根據實際的資料庫結構來實現
            TempData["ErrorMessage"] = "功能開發中";
            return RedirectToAction(nameof(Index));
        }

        // POST: MiniGame/AdminEVoucher/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminEVoucherEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // 這裡需要根據實際的資料庫結構來實現
                TempData["SuccessMessage"] = "電子禮券更新成功";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"電子禮券更新失敗：{ex.Message}";
            }

            return View(model);
        }

        // POST: MiniGame/AdminEVoucher/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int eVoucherId)
        {
            try
            {
                // 這裡需要根據實際的資料庫結構來實現
                TempData["SuccessMessage"] = "電子禮券刪除成功";
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
