// =========================
// ProductInfoesController
// - 清單(篩選) / 詳細(Modal) / 新增(Modal) / 編輯(Modal) / 下架(軟刪除, Modal) / 異動紀錄(Modal)
// - 延伸：DetailPanel(整合圖片/供應商/明細) / Cards(卡片檢視)
// - 重點觀念：IQueryable 延遲查詢、AsNoTracking 提升清單效能、半開區間日期查詢、避免 Overposting、CSRF 防護、AJAX 回傳 JSON
// =========================

// ★ 重要 using：
//   - System：C# 基本型別/工具（DateTime、Math、各種屬性/介面…）
//   - System.IO：檔案/目錄操作（Path/Directory/FileStream），圖片上傳會用到
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using GameSpace.Models;
using GameSpace.Areas.OnlineStore.ViewModels;

namespace GameSpace.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    public class ProductInfoesController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public ProductInfoesController(GameSpacedatabaseContext context)
        {
            _context = context; // Entity Framework Core 的 DbContext：透過它存取資料表
        }

        // Index → 看全部（清單＋篩選）
        // Details → 看一筆（Modal）
        // Create → 新增（Modal）
        // Edit → 修改（Modal）
        // Delete → 下架（軟刪，Modal）
        // 延伸：AuditLog（異動紀錄 Modal）、ToggleActive、DetailPanel（整合 UI）、Cards（卡片檢視）

        // ============== Index（清單＋篩選）=============
        [HttpGet] // /OnlineStore/ProductInfoes
        public async Task<IActionResult> Index(
            string? keyword,              // 名稱/類別 關鍵字（縮短）
            string? type,                 // 類別
            int? qtyMin, int? qtyMax,     // 存量區間
            string status = "active",     // active | inactive | all
            DateTime? createdFrom = null, // 建立時間 起
            DateTime? createdTo = null,   // 建立時間 迄（右邊採半開區間）
            string? hasLog = null         // yes | no | (null=全部)
        )
        {
            // ★ 觀念：IQueryable + 延遲查詢
            //   把 Where/OrderBy/Select 一直串上去，直到最後 ToListAsync() 才真的送進 DB 執行。
            //   AsNoTracking()：清單頁不用追蹤變更 → 減輕 EF Core 成本（效能更穩）
            var q = _context.ProductInfos.AsNoTracking().AsQueryable();

            // 1) 關鍵字（名稱 or 類別）
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(p => p.ProductName.Contains(keyword) || p.ProductType.Contains(keyword));

            // 2) 類別
            if (!string.IsNullOrWhiteSpace(type))
                q = q.Where(p => p.ProductType == type);

            // 3) 存量區間（ShipmentQuantity 可能為 NULL，要先判斷 HasValue）
            if (qtyMin.HasValue)
                q = q.Where(p => p.ShipmentQuantity.HasValue && p.ShipmentQuantity.Value >= qtyMin.Value);
            if (qtyMax.HasValue)
                q = q.Where(p => p.ShipmentQuantity.HasValue && p.ShipmentQuantity.Value <= qtyMax.Value);

            // 4) 狀態（active / inactive / all）
            if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
            {
                bool isActive = string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);
                q = q.Where(p => p.IsActive == isActive);
            }

            // 5) 建立時間區間：右邊用「半開區間」< 次日 00:00，避開 23:59:59 邊界問題
            if (createdFrom.HasValue)
                q = q.Where(p => p.ProductCreatedAt >= createdFrom.Value);
            if (createdTo.HasValue)
            {
                var end = createdTo.Value.Date.AddDays(1);
                q = q.Where(p => p.ProductCreatedAt < end);
            }

            // 6) 是否有異動紀錄（子查詢 Any()）
            if (hasLog == "yes")
                q = q.Where(p => _context.ProductInfoAuditLogs.Any(a => a.ProductId == p.ProductId));
            else if (hasLog == "no")
                q = q.Where(p => !_context.ProductInfoAuditLogs.Any(a => a.ProductId == p.ProductId));

            // 7) 投影成 ViewModel（只撈畫面需要的欄位 → 減少 DB 傳輸量）
            var rows = await q
                .OrderByDescending(p => p.ProductId) // ★ 修改點：預設以商品ID降冪；前端 DataTables 仍可改排序
                .Select(p => new ProductIndexRowVM
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductType = p.ProductType,
                    Price = p.Price,
                    CurrencyCode = p.CurrencyCode,
                    ShipmentQuantity = p.ShipmentQuantity,
                    IsActive = p.IsActive,
                    ProductCreatedAt = p.ProductCreatedAt,
                    CreatedByManagerId = p.ProductCreatedBy,

                    // 取最後一筆異動（顯示用）
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

            // 下拉選單來源（Distinct 後排序）
            var types = await _context.ProductInfos
                .AsNoTracking()
                .Select(p => p.ProductType)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            // 把目前篩選條件回灌到 View（保留使用者選擇）
            ViewBag.Keyword = keyword;
            ViewBag.Type = type;
            ViewBag.TypeList = types;
            ViewBag.QtyMin = qtyMin;
            ViewBag.QtyMax = qtyMax;
            ViewBag.Status = status;
            ViewBag.CreatedFrom = createdFrom?.ToString("yyyy-MM-dd");
            ViewBag.CreatedTo = createdTo?.ToString("yyyy-MM-dd");
            ViewBag.HasLog = hasLog;

            return View(rows);
        }

        // ============== Details（Modal：顯示單筆）=============
        [HttpGet]
        public async Task<IActionResult> Details(int id) // ★ 參數名稱統一用 id，便於 Url.Action 對應
        {
            // Include()：如果要顯示建立/更新人名稱，可把關聯導覽屬性一起載入
            var p = await _context.ProductInfos
                .Include(x => x.ProductCreatedByNavigation)
                .Include(x => x.ProductUpdatedByNavigation)
                .FirstOrDefaultAsync(x => x.ProductId == id);

            if (p == null) return NotFound();

            var vm = MapToVM(p);
            // Partial + AJAX：只回彈窗內容，比整頁重整快
            return PartialView("_DetailsModal", vm);
        }

        // ============== DetailPanel（延伸：整合圖片/供應商/明細）=============
        [HttpGet]
        public async Task<IActionResult> DetailPanel(int id)
        {
            var p = await _context.ProductInfos.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductId == id);
            if (p == null) return NotFound();

            // 圖片（如果你的 ProductImages 還沒 IsPrimary/SortOrder 欄位 → 先用 ProductimgId 排）
            // TODO：若你的 DbSet 不是 ProductImages，請依你的 Context 調整
            var imgs = await _context.ProductImages.AsNoTracking()
                .Where(x => x.ProductId == id)
                //.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.SortOrder) // 有欄位就改回這行
                .OrderBy(x => x.ProductimgId)                                   // ★ 修改點：沒有欄位先用這個
                .Select(x => new { x.ProductimgUrl, x.ProductimgAltText })
                .ToListAsync();

            string? supplierName = null, merchTypeName = null;

            if (p.ProductType == "game")
            {
                var d = await _context.GameProductDetails.AsNoTracking()
                         .FirstOrDefaultAsync(x => x.ProductId == id);
                if (d != null)
                {
                    // 供應商名稱
                    // TODO：若你的 DbSet 不是 Suppliers，請依 Context 調整
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
            else // 非遊戲
            {
                var d = await _context.OtherProductDetails.AsNoTracking()
                         .FirstOrDefaultAsync(x => x.ProductId == id);
                if (d != null)
                {
                    // TODO：若 DbSet 名稱不同，請調整 Suppliers / MerchTypes
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

            // 沒有對應明細也提供基本面板
            var fallBack = new { Basic = p, Detail = (object?)null, Images = imgs };
            return PartialView("_DetailPanelBasic", fallBack);
        }

        // ============== Cards（卡片檢視 Partial）=============
        [HttpGet]
        public async Task<IActionResult> Cards()
        {
            var list = await _context.ProductInfos.AsNoTracking()
                .OrderByDescending(x => x.ProductId)
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.ProductType,
                    p.Price,
                    p.IsActive,
                    LastChangedAt = _context.ProductInfoAuditLogs
                        .Where(a => a.ProductId == p.ProductId)
                        .OrderByDescending(a => a.ChangedAt)
                        .Select(a => (DateTime?)a.ChangedAt)
                        .FirstOrDefault(),
                    // ★ 如果沒有 IsPrimary/SortOrder，暫時以 ProductimgId 排
                    ImageUrl = _context.ProductImages
                        .Where(i => i.ProductId == p.ProductId)
                        //.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.SortOrder)
                        .OrderBy(i => i.ProductimgId)
                        .Select(i => i.ProductimgUrl)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return PartialView("_CardsGrid", list);
        }

        // ============== Create（Modal：新增）=============
        [HttpGet]
        public IActionResult Create()
        {
            // 預設值（例如幣別、上架）
            var vm = new ProductInfoFormVM
            {
                CurrencyCode = "TWD",
                IsActive = true
            };
            return PartialView("_CreateEditModal", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // ★ CSRF 防護：表單含 AntiForgeryToken，伺服器就會驗證它
        public async Task<IActionResult> Create(ProductInfoFormVM vm)
        {
            // 伺服器端驗證（搭配 DataAnnotations + IValidatableObject）
            if (!ModelState.IsValid)
                return PartialView("_CreateEditModal", vm);

            // 1) 寫 ProductInfo（避免 Overposting：只把允許的欄位寫回）
            var entity = new ProductInfo();
            ApplyFromVM(entity, vm);
            entity.ProductCreatedBy = GetCurrentManagerId();
            entity.ProductCreatedAt = DateTime.Now;

            _context.ProductInfos.Add(entity);
            await _context.SaveChangesAsync(); // ★ 修改點：先存，拿到 ProductId（用於明細/圖片/Log）

            // 2) 依類型寫入對應 Detail（如果你 VM 還沒顯示這些欄位，可先跳過）
            if (vm.ProductType == "game")
            {
                var gd = new GameProductDetail
                {
                    ProductId = entity.ProductId,
                    SupplierId = vm.SupplierId!.Value, // ★ VM 驗證會確保一定有值
                    PlatformId = vm.PlatformId,
                    PlatformName = vm.PlatformName,
                    GameType = vm.GameType,
                    DownloadLink = vm.DownloadLink
                };
                _context.GameProductDetails.Add(gd);
            }
            else // 非遊戲
            {
                var od = new OtherProductDetail
                {
                    ProductId = entity.ProductId,
                    SupplierId = vm.SupplierId!.Value,
                    MerchTypeId = vm.MerchTypeId,
                    DigitalCode = vm.DigitalCode,
                    Size = vm.Size,
                    Color = vm.Color,
                    Weight = vm.Weight,
                    Dimensions = vm.Dimensions,
                    Material = vm.Material,
                    StockQuantity = vm.ShipmentQuantity ?? 0
                };
                _context.OtherProductDetails.Add(od);
            }

            // 3) 圖片上傳（示範：存入 wwwroot/uploads/products/{id}/，DB 存相對 URL）
            if (vm.Images is { Length: > 0 })
            {
                var root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products", entity.ProductId.ToString());
                Directory.CreateDirectory(root); // 若目錄不存在會建立

                int sort = 0;
                foreach (var file in vm.Images)
                {
                    if (file.Length <= 0) continue;

                    // 用 時間戳+原檔名 避免重名覆蓋
                    var fname = $"{DateTime.Now:yyyyMMddHHmmssfff}_{Path.GetFileName(file.FileName)}";
                    var fullpath = Path.Combine(root, fname);
                    using (var fs = new FileStream(fullpath, FileMode.Create))
                    {
                        await file.CopyToAsync(fs);
                    }

                    var relUrl = $"/uploads/products/{entity.ProductId}/{fname}";

                    // TODO：若 DbSet 名稱不是 ProductImages，請調整
                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductId = entity.ProductId,
                        ProductimgUrl = relUrl,
                        ProductimgAltText = entity.ProductName,
                        ProductimgUpdatedAt = DateTime.Now
                        // IsPrimary = (sort == 0),   // 你的 DB 有這欄再打開
                        // SortOrder = sort++
                    });

                    // ★ 圖片新增也記錄到 AuditLog（你之前提出的需求）
                    _context.ProductInfoAuditLogs.Add(new ProductInfoAuditLog
                    {
                        ProductId = entity.ProductId,
                        ActionType = "UPDATE",
                        FieldName = "image:add", // 你指定的格式：image:add / image:remove
                        OldValue = null,
                        NewValue = relUrl,
                        ManagerId = entity.ProductCreatedBy,
                        ChangedAt = DateTime.Now
                    });

                    sort++;
                }
            }

            // 4) 寫入 AuditLog：CREATE（用摘要即可）
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

            await _context.SaveChangesAsync(); // ★ 統一存檔（Detail/Images/Logs）

            // 回 JSON：前端會顯示 toast；也可即時插入新列或直接重整
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
                    // 新增當下尚無最後異動
                    lastChangedText = (string?)null,
                    lastChangedRaw = (string?)null,
                    lastManagerId = (int?)null
                }
            });
        }

        // ============== Edit（Modal：編輯）=============
        [HttpGet]
        public async Task<IActionResult> Edit(int id) // ★ 一律用 id（和 Url.Action 對應）
        {
            var p = await _context.ProductInfos.FindAsync(id);
            if (p == null) return NotFound();

            var vm = MapToVM(p);
            return PartialView("_CreateEditModal", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductInfoFormVM vm)
        {
            if (!ModelState.IsValid)
                return PartialView("_CreateEditModal", vm);

            var p = await _context.ProductInfos.FindAsync(vm.ProductId);
            if (p == null) return NotFound();

            // ★ 先保留舊值（寫入 AuditLog 用）
            var old = new { p.ProductName, p.Price, p.ShipmentQuantity, p.IsActive };

            // 寫回允許修改的欄位（避免 overposting）
            ApplyFromVM(p, vm);
            p.ProductUpdatedBy = GetCurrentManagerId();
            p.ProductUpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // 寫入 AuditLog（UPDATE）
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

            // 回 JSON：前端（Index.cshtml）會不重整更新該列
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
                    // 最後異動欄位（顯示用＋data-*）
                    lastChangedText = log.ChangedAt.ToString("yyyy/MM/dd tt hh:mm"),
                    lastChangedRaw = log.ChangedAt.ToString("yyyy/MM/dd HH:mm:ss"),
                    lastManagerId = log.ManagerId
                }
            });
        }

        // ============== Delete（Modal：下架＝軟刪）=============
        [HttpGet]
        public async Task<IActionResult> Delete(int id) // ★ 與 View 的 Url.Action 統一用 id
        {
            var p = await _context.ProductInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductId == id);
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
            // ↑ 隱藏欄位名稱若是 productId（不是 id），用 FromForm 指定名稱
            var p = await _context.ProductInfos.FindAsync(id);
            if (p == null) return NotFound();

            var oldActive = p.IsActive;

            // 軟刪：把 is_active 設為 0（資料保留、可還原）
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

        // ============== AuditLog（Modal：顯示完整異動紀錄）=============
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

            var p = await _context.ProductInfos.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductId == id);
            ViewBag.ProductName = p?.ProductName ?? $"#{id}";

            return PartialView("_AuditLogModal", logs);
        }

        // ============== 切換上/下架（不換頁）=============
        [HttpPost]
        [ValidateAntiForgeryToken] // ★ 前端用 fetch header: RequestVerificationToken 送過來
        public async Task<IActionResult> ToggleActive(int id)
        {
            var p = await _context.ProductInfos.FindAsync(id);
            if (p == null) return NotFound();

            var oldActive = p.IsActive;
            p.IsActive = !p.IsActive; // 切換
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

        // ============== Helpers（小工具）=============
        private int GetCurrentManagerId()
        {
            // TODO：從登入資訊取得實際 manager_id
            // 目前先用假資料方便開發
            return 1;
        }

        private static ProductInfoFormVM MapToVM(ProductInfo p) => new ProductInfoFormVM
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            ProductType = p.ProductType,
            Price = p.Price,
            CurrencyCode = p.CurrencyCode,
            ShipmentQuantity = p.ShipmentQuantity,
            IsActive = p.IsActive,
            ProductCreatedBy = p.ProductCreatedBy,
            ProductCreatedAt = p.ProductCreatedAt,
            ProductUpdatedBy = p.ProductUpdatedBy,
            ProductUpdatedAt = p.ProductUpdatedAt
        };

        // 把 VM 的可編輯欄位寫回 Entity（避免 overposting）
        private static void ApplyFromVM(ProductInfo entity, ProductInfoFormVM vm)
        {
            entity.ProductName = vm.ProductName.Trim();
            entity.ProductType = vm.ProductType;
            entity.Price = vm.Price;
            entity.CurrencyCode = vm.CurrencyCode;
            entity.ShipmentQuantity = vm.ShipmentQuantity;
            entity.IsActive = vm.IsActive;
        }

        private bool ProductInfoExists(int id)
            => _context.ProductInfos.Any(e => e.ProductId == id);
     // -----------（以下保留：舊版 Index 寫法供參考）-----------
        // 你的舊版 Index 我留在專案裡當範例即可；現在的版本用 DataTables 前端分頁/排序，
        // 後端只做篩選與初始排序，維持簡單與效能。}


    }
}
