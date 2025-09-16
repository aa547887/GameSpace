// =========================
// ProductInfoesController
// - 清單(篩選) / 詳細(Modal) / 新增(Modal) / 編輯(Modal) / 下架(軟刪除, Modal) / 異動紀錄(Modal)
// - 延伸：DetailPanel(整合圖片/供應商/明細) / Cards(卡片檢視)
// - 觀念：IQueryable 延遲查詢、AsNoTracking、半開區間日期查詢、避免 Overposting、CSRF、防止重送、AJAX JSON
// =========================

// ★ 重要 using：
//   System：C# 基本型別/工具（DateTime、介面、屬性…）
//   System.IO：檔案/目錄操作（Path/Directory/FileStream），圖片上傳會用到
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;                               // ★ EF 實體（ProductInfo 等）
using GameSpace.Areas.OnlineStore.ViewModels;         // ★ 你的 ViewModel

namespace GameSpace.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    public class ProductInfoesController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public ProductInfoesController(GameSpacedatabaseContext context)
        {
            _context = context; // EF Core DbContext
        }

        // ============== Index（清單＋篩選）=============
        [HttpGet] // /OnlineStore/ProductInfoes
        public async Task<IActionResult> Index(
            string? keyword,
            string? type,
            int? qtyMin, int? qtyMax,
            string status = "active",           // active | inactive | all
            DateTime? createdFrom = null,
            DateTime? createdTo = null,
            string? hasLog = null               // yes | no | null(全部)
        )
        {
            var q = _context.ProductInfos.AsNoTracking().AsQueryable();

            // 1) 關鍵字（名稱 or 類別）
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(p => p.ProductName.Contains(keyword) || p.ProductType.Contains(keyword));

            // 2) 類別
            if (!string.IsNullOrWhiteSpace(type))
                q = q.Where(p => p.ProductType == type);

            // 3) 存量（ShipmentQuantity 可能為 NULL）
            if (qtyMin.HasValue)
                q = q.Where(p => p.ShipmentQuantity.HasValue && p.ShipmentQuantity.Value >= qtyMin.Value);
            if (qtyMax.HasValue)
                q = q.Where(p => p.ShipmentQuantity.HasValue && p.ShipmentQuantity.Value <= qtyMax.Value);

            // 4) 狀態
            if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
            {
                bool isActive = string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);
                q = q.Where(p => p.IsActive == isActive);
            }

            // 5) 建立時間（右邊半開區間）
            if (createdFrom.HasValue)
                q = q.Where(p => p.ProductCreatedAt >= createdFrom.Value);
            if (createdTo.HasValue)
            {
                var end = createdTo.Value.Date.AddDays(1);
                q = q.Where(p => p.ProductCreatedAt < end);
            }

            // 6) 是否有異動紀錄
            if (hasLog == "yes")
                q = q.Where(p => _context.ProductInfoAuditLogs.Any(a => a.ProductId == p.ProductId));
            else if (hasLog == "no")
                q = q.Where(p => !_context.ProductInfoAuditLogs.Any(a => a.ProductId == p.ProductId));

            // 7) 投影成 VM（只撈畫面需要欄位）
            var rows = await q
                .OrderByDescending(p => p.ProductId)
                .Select(p => new ProductIndexRowVM
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,

                    // ★ 修改點：你的實體欄位名稱是 ProductCode1，用它取商品編號
                    ProductCode = _context.ProductCodes
                        .Where(c => c.ProductId == p.ProductId)
                        .Select(c => c.ProductCode1)
                        .FirstOrDefault(),

                    // ★ 修改點：排序碼先嘗試在 DB 端轉（若資料有非數字會在下方再保險一次）
                    ProductCodeSort = _context.ProductCodes
                        .Where(c => c.ProductId == p.ProductId && c.ProductCode1.Length > 2)
                        .Select(c => (int?)Convert.ToInt32(c.ProductCode1.Substring(2)))
                        .FirstOrDefault(),

                    ProductType = p.ProductType,
                    Price = p.Price,
                    CurrencyCode = p.CurrencyCode,
                    ShipmentQuantity = p.ShipmentQuantity,
                    IsActive = p.IsActive,
                    ProductCreatedAt = p.ProductCreatedAt,
                    CreatedByManagerId = p.ProductCreatedBy,

                    LastLog = _context.ProductInfoAuditLogs
                        .Where(a => a.ProductId == p.ProductId)
                        .OrderByDescending(a => a.ChangedAt)
                        .Select(a => new LastLogDto
                        {
                            LogId = a.LogId,
                            ManagerId = a.ManagerId,
                            ChangedAt = a.ChangedAt
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            // ★ 修改點（保險）：若 DB 端轉型失敗，再在記憶體端計算一次排序碼
            //之後再跑一次 TryParse 來修正可能的例外值 
            foreach (var r in rows)

            {
                if (!r.ProductCodeSort.HasValue &&
                    !string.IsNullOrEmpty(r.ProductCode) &&
                    r.ProductCode.Length > 2 &&
                    int.TryParse(r.ProductCode.Substring(2), out var n))
                {
                    r.ProductCodeSort = n;
                }
            }

            // 類別下拉
            var types = await _context.ProductInfos.AsNoTracking()
                .Select(p => p.ProductType).Distinct().OrderBy(s => s).ToListAsync();

            // 回灌篩選
            ViewBag.Keyword = keyword;
            ViewBag.Type = type;
            ViewBag.TypeList = types;
            ViewBag.QtyMin = qtyMin;
            ViewBag.QtyMax = qtyMax;
            ViewBag.Status = status;
            ViewBag.CreatedFrom = createdFrom?.ToString("yyyy-MM-dd");
            ViewBag.CreatedTo = createdTo?.ToString("yyyy-MM-dd");
            ViewBag.HasLog = hasLog;

            // TODO(5) 可選：這裡未實作 Detail 的進階篩選（平台、周邊分類等），之後可再補
            return View(rows);
        }

        // ============== Details（Modal：顯示單筆）=============
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var p = await _context.ProductInfos
                .Include(x => x.ProductCreatedByNavigation)
                .Include(x => x.ProductUpdatedByNavigation)
                .FirstOrDefaultAsync(x => x.ProductId == id);

            if (p == null) return NotFound();
            return PartialView("_DetailsModal", MapToVM(p));
        }

        // ============== DetailPanel（整合圖片/供應商/明細）=============
        [HttpGet]
        public async Task<IActionResult> DetailPanel(int id)
        {
            var p = await _context.ProductInfos.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductId == id);
            if (p == null) return NotFound();

            // 圖片（若尚未加 IsPrimary/SortOrder，先用 id 排）
            var imgs = await _context.ProductImages.AsNoTracking()
                .Where(x => x.ProductId == id)
                .OrderBy(x => x.ProductimgId) // ★ 暫時用主鍵排序
                .Select(x => new { x.ProductimgUrl, x.ProductimgAltText })
                .ToListAsync();

            string? supplierName = null, merchTypeName = null;

            if (p.ProductType == "game")
            {
                var d = await _context.GameProductDetails.AsNoTracking()
                             .FirstOrDefaultAsync(x => x.ProductId == id);
                if (d != null)
                {
                    supplierName = await _context.Suppliers
                        .Where(s => s.SupplierId == d.SupplierId)
                        .Select(s => s.SupplierName)
                        .FirstOrDefaultAsync();

                    var vm = new
                    {
                        Basic = p,
                        Detail = new { d.PlatformId, d.PlatformName, d.GameType, d.DownloadLink, SupplierName = supplierName },
                        Images = imgs
                    };
                    return PartialView("_DetailPanelGame", vm);
                }
            }
            else
            {
                var d = await _context.OtherProductDetails.AsNoTracking()
                             .FirstOrDefaultAsync(x => x.ProductId == id);
                if (d != null)
                {
                    supplierName = await _context.Suppliers
                        .Where(s => s.SupplierId == d.SupplierId)
                        .Select(s => s.SupplierName)
                        .FirstOrDefaultAsync();

                    merchTypeName = await _context.MerchTypes
                        .Where(m => m.MerchTypeId == d.MerchTypeId)
                        .Select(m => m.MerchTypeName)
                        .FirstOrDefaultAsync();

                    var vm = new
                    {
                        Basic = p,
                        Detail = new { d.Size, d.Color, d.Weight, d.Dimensions, d.Material, d.DigitalCode, SupplierName = supplierName, MerchTypeName = merchTypeName },
                        Images = imgs
                    };
                    return PartialView("_DetailPanelOther", vm);
                }
            }

            var fallBack = new { Basic = p, Detail = (object?)null, Images = imgs };
            return PartialView("_DetailPanelBasic", fallBack);
        }

        // ============== Cards（卡片檢視 Partial）=============
        [HttpGet]
        public async Task<IActionResult> Cards(string? keyword, string? type, int page = 1, int pageSize = 12)
        {
            var q = _context.ProductInfos.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(p => p.ProductName.Contains(keyword) || p.ProductType.Contains(keyword));
            if (!string.IsNullOrWhiteSpace(type))
                q = q.Where(p => p.ProductType == type);

            var total = await q.CountAsync();
            page = Math.Max(1, page);
            pageSize = Math.Max(1, pageSize);

            var list = await q
                .OrderByDescending(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.ProductType,
                    p.Price,
                    p.IsActive,
                    ProductCode = _context.ProductCodes
                        .Where(c => c.ProductId == p.ProductId)
                        .Select(c => c.ProductCode1)
                        .FirstOrDefault(),
                    LastChangedAt = _context.ProductInfoAuditLogs
                        .Where(a => a.ProductId == p.ProductId)
                        .OrderByDescending(a => a.ChangedAt)
                        .Select(a => (DateTime?)a.ChangedAt)
                        .FirstOrDefault(),
                    ImageUrl = _context.ProductImages
                        .Where(i => i.ProductId == p.ProductId)
                        .OrderBy(i => i.ProductimgId) // 沒 IsPrimary/SortOrder 先用 id
                        .Select(i => i.ProductimgUrl)
                        .FirstOrDefault()
                })
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;

            return PartialView("_CardsGrid", list);
        }

        // ============== Create（Modal：新增）=============
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new ProductInfoFormVM { CurrencyCode = "TWD", IsActive = true };
            return PartialView("_CreateEditModal", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductInfoFormVM vm)
        {
            if (!ModelState.IsValid)
                return PartialView("_CreateEditModal", vm);

            try
            {
                // 1) 先寫 ProductInfo
                var entity = new ProductInfo();
                ApplyFromVM(entity, vm);
                entity.ProductCreatedBy = GetCurrentManagerId(); // ★ 假登入 ID（下方方法）
                entity.ProductCreatedAt = DateTime.Now;

                _context.ProductInfos.Add(entity);
                await _context.SaveChangesAsync();               // ★ 先存拿 ProductId

                // 2) 寫對應 Detail（注意：你的 EF 類別若是複數，請改成對應名稱）
                if (vm.ProductType == "game")
                {
                    _context.GameProductDetails.Add(new GameProductDetail
                    {
                        ProductId = entity.ProductId,
                        SupplierId = vm.SupplierIds!.Value,
                        PlatformId = vm.PlatformId,
                        PlatformName = vm.PlatformName,
                        GameType = vm.GameType,
                        DownloadLink = vm.DownloadLink,
                        IsActive = true
                    });
                }
                else
                {
                    _context.OtherProductDetails.Add(new OtherProductDetail
                    {
                        ProductId = entity.ProductId,
                        SupplierId = vm.SupplierIds!.Value,
                        MerchTypeId = vm.MerchTypeId,
                        DigitalCode = vm.DigitalCode,
                        Size = vm.Size,
                        Color = vm.Color,
                        Weight = vm.Weight,
                        Dimensions = vm.Dimensions,
                        Material = vm.Material,
                        StockQuantity = vm.ShipmentQuantity ?? 0,
                        IsActive = true
                    });
                }

                // 3) 圖片上傳（wwwroot/uploads/products/{id}/）
                if (vm.Image != null && vm.Image.Count > 0)
                {
                    var root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products", entity.ProductId.ToString());
                    Directory.CreateDirectory(root);

                    foreach (var file in vm.Image)
                    {
                        if (file.Length <= 0) continue;

                        var fname = $"{DateTime.Now:yyyyMMddHHmmssfff}_{Path.GetFileName(file.FileName)}";
                        var full = Path.Combine(root, fname);
                        using (var fs = new FileStream(full, FileMode.Create))
                            await file.CopyToAsync(fs);

                        var url = $"/uploads/products/{entity.ProductId}/{fname}";
                        _context.ProductImages.Add(new ProductImage
                        {
                            ProductId = entity.ProductId,
                            ProductimgUrl = url,
                            ProductimgAltText = entity.ProductName,
                            ProductimgUpdatedAt = DateTime.Now
                        });

                        // ★ 記錄圖片異動
                        _context.ProductInfoAuditLogs.Add(new ProductInfoAuditLog
                        {
                            ProductId = entity.ProductId,
                            ActionType = "UPDATE",
                            FieldName = "image:add",
                            OldValue = null,
                            NewValue = url,
                            ManagerId = entity.ProductCreatedBy,
                            ChangedAt = DateTime.Now
                        });
                    }
                }

                // 4) CREATE 紀錄
                _context.ProductInfoAuditLogs.Add(new ProductInfoAuditLog
                {
                    ProductId = entity.ProductId,
                    ActionType = "CREATE",
                    FieldName = "(all)",
                    OldValue = null,
                    NewValue = $"Name={entity.ProductName}, Price={entity.Price}, Type={entity.ProductType}",
                    ManagerId = entity.ProductCreatedBy,
                    ChangedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();

                return Json(new
                {
                    ok = true,
                    msg = $"「{entity.ProductName}」已新增！",
                    created = new
                    {
                        id = entity.ProductId,
                        name = entity.ProductName,
                        type = entity.ProductType,
                        priceN0 = entity.Price.ToString("N0"),
                        qty = entity.ShipmentQuantity,
                        active = entity.IsActive,
                        createdText = entity.ProductCreatedAt.ToString("yyyy/MM/dd tt hh:mm"),
                        createdRaw = entity.ProductCreatedAt.ToString("yyyy/MM/dd HH:mm:ss"),
                        createdByManager = entity.ProductCreatedBy,
                        lastChangedText = (string?)null,
                        lastChangedRaw = (string?)null,
                        lastManagerId = (int?)null
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, msg = $"新增失敗：{ex.Message}" });
            }
        }

        // ============== Edit（Modal：編輯）=============
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _context.ProductInfos.FindAsync(id);
            if (p == null) return NotFound();
            return PartialView("_CreateEditModal", MapToVM(p));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductInfoFormVM vm)
        {
            if (!ModelState.IsValid)
                return PartialView("_CreateEditModal", vm);

            var p = await _context.ProductInfos.FindAsync(vm.ProductId);
            if (p == null) return NotFound();

            var old = new { p.ProductName, p.Price, p.ShipmentQuantity, p.IsActive };

            ApplyFromVM(p, vm);
            p.ProductUpdatedBy = GetCurrentManagerId();
            p.ProductUpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var log = new ProductInfoAuditLog
            {
                ProductId = p.ProductId,
                ActionType = "UPDATE",
                FieldName = "(mixed)",
                OldValue = $"Name={old.ProductName}, Price={old.Price}, Qty={old.ShipmentQuantity}, Active={old.IsActive}",
                NewValue = $"Name={p.ProductName}, Price={p.Price}, Qty={p.ShipmentQuantity}, Active={p.IsActive}",
                ManagerId = p.ProductUpdatedBy,
                ChangedAt = DateTime.Now
            };
            _context.ProductInfoAuditLogs.Add(log);
            await _context.SaveChangesAsync();

            return Json(new
            {
                ok = true,
                msg = $"「{p.ProductName}」的商品清單資訊已修改完成 *-* !",
                updated = new
                {
                    id = p.ProductId,
                    name = p.ProductName,
                    type = p.ProductType,
                    priceN0 = p.Price.ToString("N0"),
                    qty = p.ShipmentQuantity,
                    active = p.IsActive,
                    lastChangedText = log.ChangedAt.ToString("yyyy/MM/dd tt hh:mm"),
                    lastChangedRaw = log.ChangedAt.ToString("yyyy/MM/dd HH:mm:ss"),
                    lastManagerId = log.ManagerId
                }
            });
        }

        // ============== Delete（Modal：下架＝軟刪）=============
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _context.ProductInfos.AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == id);
            if (p == null) return NotFound();

            var vm = new ProductInfoFormVM
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductType = p.ProductType,
                Price = p.Price,
                IsActive = p.IsActive
            };
            return PartialView("_DeleteModal", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([FromForm(Name = "productId")] int id)
        {
            var p = await _context.ProductInfos.FindAsync(id);
            if (p == null) return NotFound();

            var oldActive = p.IsActive;
            p.IsActive = false;
            p.ProductUpdatedBy = GetCurrentManagerId();
            p.ProductUpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            _context.ProductInfoAuditLogs.Add(new ProductInfoAuditLog
            {
                ProductId = p.ProductId,
                ActionType = "UPDATE",
                FieldName = "is_active",
                OldValue = oldActive ? "1" : "0",
                NewValue = "0",
                ManagerId = p.ProductUpdatedBy,
                ChangedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return Json(new { ok = true, msg = $"「{p.ProductName}」已下架（軟刪除）。", id = p.ProductId, active = false, softDeleted = true });
        }

        // ============== AuditLog（Modal）=============
        [HttpGet]
        public async Task<IActionResult> AuditLog(int id)
        {
            var logs = await _context.ProductInfoAuditLogs
                .Where(a => a.ProductId == id)
                .OrderByDescending(a => a.ChangedAt)
                .Select(a => new ProductInfoAuditLogRowVM
                {
                    LogId = a.LogId,
                    ActionType = a.ActionType,
                    FieldName = a.FieldName,
                    OldValue = a.OldValue,
                    NewValue = a.NewValue,
                    ManagerId = a.ManagerId,
                    ChangedAt = a.ChangedAt
                })
                .ToListAsync();

            ViewBag.ProductId = id;
            var p = await _context.ProductInfos.AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == id);
            ViewBag.ProductName = p?.ProductName ?? $"#{id}";
            return PartialView("_AuditLogModal", logs);
        }

        // ============== ToggleActive（AJAX）=============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var p = await _context.ProductInfos.FindAsync(id);
            if (p == null) return NotFound();

            var oldActive = p.IsActive;
            p.IsActive = !p.IsActive;
            p.ProductUpdatedBy = GetCurrentManagerId();
            p.ProductUpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            _context.ProductInfoAuditLogs.Add(new ProductInfoAuditLog
            {
                ProductId = p.ProductId,
                ActionType = "UPDATE",
                FieldName = "is_active",
                OldValue = oldActive ? "1" : "0",
                NewValue = p.IsActive ? "1" : "0",
                ManagerId = p.ProductUpdatedBy,
                ChangedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            var msg = $"「{p.ProductName}」已{(p.IsActive ? "上架" : "下架")}。";
            return Json(new { ok = true, msg, active = p.IsActive });
        }

        // ============== Helpers ==============
        // ★ 先用固定的 Manager_Id（請確認 DB 中確實存在這個 id）
        private int GetCurrentManagerId() => 30000060;

        private ProductInfoFormVM MapToVM(ProductInfo e)
        {
            var vm = new ProductInfoFormVM
            {
                ProductId = e.ProductId,
                ProductName = e.ProductName,
                ProductType = e.ProductType,
                Price = e.Price,
                CurrencyCode = e.CurrencyCode,
                ShipmentQuantity = e.ShipmentQuantity,
                IsActive = e.IsActive,
                ProductCreatedAt = e.ProductCreatedAt,
                ProductCreatedBy = e.ProductCreatedBy,
                ProductUpdatedAt = e.ProductUpdatedAt,
                ProductUpdatedBy = e.ProductUpdatedBy
            };
            return vm;
        }

        // 只寫回允許欄位（避免 overposting）
        private void ApplyFromVM(ProductInfo e, ProductInfoFormVM vm)
        {
            e.ProductName = vm.ProductName.Trim();
            e.ProductType = vm.ProductType;
            e.Price = vm.Price;                         // ★ 修改點：VM 的 Price 是 decimal（非 nullable），直接指定
            e.CurrencyCode = vm.CurrencyCode;
            e.ShipmentQuantity = vm.ShipmentQuantity;   // ★ 修改點：保留 null（game 可空），不要強制 0
            e.IsActive = vm.IsActive;
        }

       
        //private bool ProductInfoExists(int id) => _context.ProductInfos.Any(e => e.ProductId == id);
    }
}
