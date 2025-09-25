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
    public class AdminCouponController : Controller
    {
        private readonly IMiniGameAdminService _adminService;

        public AdminCouponController(IMiniGameAdminService adminService)
        {
            _adminService = adminService;
        }

        // GET: MiniGame/AdminCoupon
        public async Task<IActionResult> Index(CouponQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            var coupons = await _adminService.QueryUserCouponsAsync(query);

            var viewModel = new AdminCouponIndexViewModel
            {
                UserCoupons = coupons,
                Query = query
            };

            return View(viewModel);
        }

        // GET: MiniGame/AdminCoupon/Details/5
        public async Task<IActionResult> Details(int couponId)
        {
            try
            {
                var coupon = await _adminService.GetCouponDetailsAsync(couponId);
                if (coupon == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的優惠券";
                    return RedirectToAction(nameof(Index));
                }

                return View(coupon);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入優惠券詳情時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: MiniGame/AdminCoupon/Create
        public async Task<IActionResult> Create()
        {
            var couponTypes = await _adminService.GetCouponTypesAsync();
            var viewModel = new AdminCouponCreateViewModel
            {
                CouponTypes = couponTypes
            };
            return View(viewModel);
        }

        // POST: MiniGame/AdminCoupon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCouponCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.CouponTypes = await _adminService.GetCouponTypesAsync();
                return View(model);
            }

            try
            {
                var success = await _adminService.CreateCouponAsync(new CouponCreateModel
                {
                    CouponName = model.CouponName,
                    CouponCode = model.CouponCode,
                    DiscountValue = model.DiscountValue,
                    DiscountType = model.DiscountType,
                    Quantity = model.Quantity,
                    ExpiryDate = model.ExpiryDate
                });

                if (success)
                {
                    TempData["SuccessMessage"] = "優惠券創建成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "優惠券創建失敗";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"優惠券創建失敗：{ex.Message}";
            }

            model.CouponTypes = await _adminService.GetCouponTypesAsync();
            return View(model);
        }

        // GET: MiniGame/AdminCoupon/Edit/5
        public async Task<IActionResult> Edit(int couponId)
        {
            try
            {
                var coupon = await _adminService.GetCouponForEditAsync(couponId);
                if (coupon == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的優惠券";
                    return RedirectToAction(nameof(Index));
                }

                var couponTypes = await _adminService.GetCouponTypesAsync();
                var viewModel = new AdminCouponEditViewModel
                {
                    CouponId = coupon.CouponId,
                    CouponName = coupon.CouponName,
                    CouponCode = coupon.CouponCode,
                    DiscountValue = coupon.DiscountValue,
                    DiscountType = coupon.DiscountType,
                    Quantity = coupon.Quantity,
                    ExpiryDate = coupon.ExpiryDate,
                    CouponTypes = couponTypes
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入優惠券時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: MiniGame/AdminCoupon/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminCouponEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.CouponTypes = await _adminService.GetCouponTypesAsync();
                return View(model);
            }

            try
            {
                var success = await _adminService.UpdateCouponAsync(new CouponUpdateModel
                {
                    CouponId = model.CouponId,
                    CouponName = model.CouponName,
                    CouponCode = model.CouponCode,
                    DiscountValue = model.DiscountValue,
                    DiscountType = model.DiscountType,
                    Quantity = model.Quantity,
                    ExpiryDate = model.ExpiryDate
                });

                if (success)
                {
                    TempData["SuccessMessage"] = "優惠券更新成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "優惠券更新失敗";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"優惠券更新失敗：{ex.Message}";
            }

            model.CouponTypes = await _adminService.GetCouponTypesAsync();
            return View(model);
        }

        // POST: MiniGame/AdminCoupon/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int couponId)
        {
            try
            {
                var success = await _adminService.DeleteCouponAsync(couponId);
                if (success)
                {
                    TempData["SuccessMessage"] = "優惠券刪除成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "優惠券刪除失敗";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"優惠券刪除失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: MiniGame/AdminCoupon/IssueToUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IssueToUser(AdminCouponIssueViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "請填寫所有必要欄位";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var success = await _adminService.IssueCouponToUserAsync(model.UserId, model.CouponId, model.Quantity);

                if (success)
                {
                    TempData["SuccessMessage"] = "優惠券發放成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "優惠券發放失敗";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"優惠券發放失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    // ViewModels
    public class AdminCouponIndexViewModel
    {
        public PagedResult<UserCouponReadModel> UserCoupons { get; set; } = new();
        public CouponQueryModel Query { get; set; } = new();
        public string Sidebar { get; set; } = "admin";
    }

    public class AdminCouponCreateViewModel
    {
        [Required(ErrorMessage = "優惠券名稱不能為空")]
        [StringLength(100, ErrorMessage = "優惠券名稱不能超過 100 個字元")]
        public string CouponName { get; set; } = string.Empty;

        [Required(ErrorMessage = "優惠券代碼不能為空")]
        [StringLength(50, ErrorMessage = "優惠券代碼不能超過 50 個字元")]
        public string CouponCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "折扣值不能為空")]
        [Range(0.01, 999999, ErrorMessage = "折扣值必須在 0.01-999999 之間")]
        public decimal DiscountValue { get; set; }

        [Required(ErrorMessage = "折扣類型不能為空")]
        public string DiscountType { get; set; } = "Percentage";

        [Required(ErrorMessage = "數量不能為空")]
        [Range(1, 999999, ErrorMessage = "數量必須在 1-999999 之間")]
        public int Quantity { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ExpiryDate { get; set; }

        public List<CouponTypeReadModel> CouponTypes { get; set; } = new();
        public string Sidebar { get; set; } = "admin";
    }

    public class AdminCouponEditViewModel
    {
        public int CouponId { get; set; }

        [Required(ErrorMessage = "優惠券名稱不能為空")]
        [StringLength(100, ErrorMessage = "優惠券名稱不能超過 100 個字元")]
        public string CouponName { get; set; } = string.Empty;

        [Required(ErrorMessage = "優惠券代碼不能為空")]
        [StringLength(50, ErrorMessage = "優惠券代碼不能超過 50 個字元")]
        public string CouponCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "折扣值不能為空")]
        [Range(0.01, 999999, ErrorMessage = "折扣值必須在 0.01-999999 之間")]
        public decimal DiscountValue { get; set; }

        [Required(ErrorMessage = "折扣類型不能為空")]
        public string DiscountType { get; set; } = "Percentage";

        [Required(ErrorMessage = "數量不能為空")]
        [Range(1, 999999, ErrorMessage = "數量必須在 1-999999 之間")]
        public int Quantity { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ExpiryDate { get; set; }

        public List<CouponTypeReadModel> CouponTypes { get; set; } = new();
        public string Sidebar { get; set; } = "admin";
    }

    public class AdminCouponIssueViewModel
    {
        [Required(ErrorMessage = "請選擇用戶")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "請選擇優惠券")]
        public int CouponId { get; set; }

        [Required(ErrorMessage = "請輸入數量")]
        [Range(1, 999, ErrorMessage = "數量必須在 1-999 之間")]
        public int Quantity { get; set; }
    }
}
