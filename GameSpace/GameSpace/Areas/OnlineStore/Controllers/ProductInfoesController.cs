using GameSpace.Areas.OnlineStore.ViewModels;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    public class ProductInfoesController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public ProductInfoesController(GameSpacedatabaseContext context)
        {
            _context = context; //Entity Framework Core (DbContext)存取資料表
        }

        //Index → 看全部    //Details → 看一筆	  //Create → 新增		//Edit → 修改		//Delete → 下架

        [HttpGet]// /OnlineStore/ProductInfos
        public async Task<IActionResult> Index(
        string? keyword,                 // 名稱/類別 關鍵字（縮短）
        string? type,                    // 類別
        int? qtyMin, int? qtyMax,        // 存量區間
        string status = "active",        // active | inactive | all
        DateTime? createdFrom = null,    // 建立時間 起
        DateTime? createdTo = null,      // 建立時間 迄
        string? hasLog = null)          // yes | no | (null=全部)

        {
            var q = _context.ProductInfos.AsNoTracking().AsQueryable();

            // 關鍵字：名稱 or 類別
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(p => p.ProductName.Contains(keyword) || p.ProductType.Contains(keyword));

            // 類別
            if (!string.IsNullOrWhiteSpace(type))
                q = q.Where(p => p.ProductType == type);

            // 存量區間（NULL 不參與）
            if (qtyMin.HasValue) q = q.Where(p => p.ShipmentQuantity.HasValue && p.ShipmentQuantity.Value >= qtyMin.Value);
            if (qtyMax.HasValue) q = q.Where(p => p.ShipmentQuantity.HasValue && p.ShipmentQuantity.Value <= qtyMax.Value);

            // 狀態
            if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
            {
                bool isActive = string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);
                q = q.Where(p => p.IsActive == isActive);
            }

            // 建立時間區間（右邊含當日）
            if (createdFrom.HasValue) q = q.Where(p => p.ProductCreatedAt >= createdFrom.Value);
            if (createdTo.HasValue)
            {
                var end = createdTo.Value.Date.AddDays(1);
                q = q.Where(p => p.ProductCreatedAt < end);
            }

            // 是否有異動紀錄
            if (hasLog == "yes")
            {
                q = q.Where(p => _context.ProductInfoAuditLogs.Any(a => a.ProductId == p.ProductId));
            }
            else if (hasLog == "no")
            {

                q = q.Where(p => !_context.ProductInfoAuditLogs.Any(a => a.ProductId == p.ProductId));
            }

         var rows = await q
        .OrderByDescending(p => p.ProductCreatedAt) // 初始排序（DataTables 前端可再改）
        .Select(p => new GameSpace.Areas.OnlineStore.ViewModels.ProductIndexRowVM
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            ProductType = p.ProductType,
            Price = p.Price,
            ShipmentQuantity = p.ShipmentQuantity,
            IsActive = p.IsActive,
            ProductCreatedAt = p.ProductCreatedAt,
            CreatedByManagerId = p.ProductCreatedBy,
            LastLog = _context.ProductInfoAuditLogs
                .Where(a => a.ProductId == p.ProductId)
                .OrderByDescending(a => a.ChangedAt)
                .Select(a => new GameSpace.Areas.OnlineStore.ViewModels.LastLogDto
                {
                    LogId = a.LogId,
                    ManagerId = a.ManagerId,
                    ChangedAt = a.ChangedAt
                })
                .FirstOrDefault()
        })
        .ToListAsync();

            // 下拉選單來源
            var types = await _context.ProductInfos.AsNoTracking()
                            .Select(p => p.ProductType).Distinct().OrderBy(s => s).ToListAsync();

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

        //舊版index		//[HttpGet]
        //public async Task<IActionResult> Index(
        //	string? search,            // 名稱/類別 關鍵字
        //	string? type,              // 類別精準篩選
        //	int? qtyMin,               // 存量下限
        //	int? qtyMax,               // 存量上限
        //	string status = "active",  // active | inactive | all
        //	DateTime? createdFrom = null,
        //	DateTime? createdTo = null,
        //	string? hasLog = null,
        //	string? sort = "created_desc",
        //	int page = 1,
        //	int pageSize = 10)
        //{
        //	var q = _context.ProductInfos.AsNoTracking().AsQueryable();

        //          // 1) 關鍵字 (名稱 or 類別)
        //          if (!string.IsNullOrWhiteSpace(search))
        //              q = q.Where(p => p.ProductName.Contains(search) ||
        //                               p.ProductType.Contains(search));

        //          // 2) 類別
        //          if (!string.IsNullOrWhiteSpace(type))
        //              q = q.Where(p => p.ProductType == type);

        //          // 3) 存量區間（只針對有存量的商品；NULL 視為不參與）
        //          if (qtyMin.HasValue)
        //		q = q.Where(p => p.ShipmentQuantity.HasValue && p.ShipmentQuantity.Value >= qtyMin.Value);
        //	if (qtyMax.HasValue)
        //		q = q.Where(p => p.ShipmentQuantity.HasValue && p.ShipmentQuantity.Value <= qtyMax.Value);

        //	// 4) 狀態
        //	if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        //	{
        //		bool isActive = string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);
        //		q = q.Where(p => p.IsActive == isActive);
        //	}

        //	// 5) 建立時間區間（createdTo 取當日 23:59:59）
        //	if (createdFrom.HasValue)
        //		q = q.Where(p => p.ProductCreatedAt >= createdFrom.Value);
        //	if (createdTo.HasValue)
        //	{
        //		var end = createdTo.Value.Date.AddDays(1); // < 次日 00:00
        //		q = q.Where(p => p.ProductCreatedAt < end);
        //	}
        //	// 6. 是否有異動紀錄
        //	if (hasLog == "yes")
        //		q = q.Where(p => _context.ProductInfoAuditLogs.Any(a => a.ProductId == p.ProductId));
        //	else if (hasLog == "no")
        //		q = q.Where(p => !_context.ProductInfoAuditLogs.Any(a => a.ProductId == p.ProductId));


        //	// 6) 排序
        //	q = sort switch
        //	{
        //		"name_asc" => q.OrderBy(p => p.ProductName),
        //		"name_desc" => q.OrderByDescending(p => p.ProductName),
        //		"type_asc" => q.OrderBy(p => p.ProductType),
        //		"type_desc" => q.OrderByDescending(p => p.ProductType),
        //		"price_asc" => q.OrderBy(p => p.Price),
        //		"price_desc" => q.OrderByDescending(p => p.Price),
        //		"qty_asc" => q.OrderBy(p => p.ShipmentQuantity),
        //		"qty_desc" => q.OrderByDescending(p => p.ShipmentQuantity),
        //		"status_asc" => q.OrderBy(p => p.IsActive),
        //		"status_desc" => q.OrderByDescending(p => p.IsActive),
        //		"created_asc" => q.OrderBy(p => p.ProductCreatedAt),
        //		"created_desc" => q.OrderByDescending(p => p.ProductCreatedAt),
        //		"updated_asc" => q.OrderBy(p => p.ProductUpdatedAt),
        //		"updated_desc" => q.OrderByDescending(p => p.ProductUpdatedAt),
        //		_ => q.OrderByDescending(p => p.ProductCreatedAt)
        //	};

        //	// 7) 總數 + 分頁
        //	var totalCount = await q.CountAsync();

        //	// ✅ pageSize=0 表示全部
        //	if (pageSize <= 0) pageSize = totalCount > 0 ? totalCount : 1;
        //	var rows = await q
        //		.Skip((page - 1) * pageSize)
        //		.Take(pageSize)
        //		.Select(p => new ProductIndexRowVM
        //		{
        //			ProductId = p.ProductId,
        //			ProductName = p.ProductName,
        //			ProductType = p.ProductType,
        //			Price = p.Price,
        //			ShipmentQuantity = p.ShipmentQuantity,
        //			IsActive = p.IsActive,
        //			ProductCreatedAt = p.ProductCreatedAt,
        //			CreatedByManagerId = p.ProductCreatedBy,
        //			//ProductUpdatedAt = p.ProductUpdatedAt,


        //			// 🔎 取最後一筆異動

        //                  LastLog = _context.ProductInfoAuditLogs
        //                      .Where(a => a.ProductId == p.ProductId)
        //                      .OrderByDescending(a => a.ChangedAt)
        //                      .Select(a => new LastLogDto
        //                      {
        //					LogId = a.LogId,
        //					ManagerId = a.ManagerId,
        //					ChangedAt = a.ChangedAt
        //                      })
        //                      .FirstOrDefault()
        //              })
        //		.ToListAsync();

        //	// 類別下拉
        //	var types = await _context.ProductInfos.AsNoTracking()
        //		.Select(p => p.ProductType)
        //		.Distinct()
        //		.OrderBy(s => s)
        //		.ToListAsync();

        //	ViewBag.TotalCount = totalCount;
        //	ViewBag.Page = page;
        //	ViewBag.PageSize = pageSize;
        //	ViewBag.Search = search;
        //	ViewBag.Type = type;
        //	ViewBag.TypeList = types;
        //	ViewBag.QtyMin = qtyMin;
        //	ViewBag.QtyMax = qtyMax;
        //	ViewBag.Status = status;
        //	ViewBag.CreatedFrom = createdFrom?.ToString("yyyy-MM-dd");
        //          ViewBag.CreatedTo = createdTo?.ToString("yyyy-MM-dd");
        //	ViewBag.HasLog = hasLog;
        //	ViewBag.Sort = sort;

        //	return View(rows);
        //}


        //public async Task<IActionResult> Index()// GET: OnlineStore/ProductInfoes  //自動生成版
        //{
        //    var gameSpacedatabaseContext = _context.ProductInfos
        //        .Include(p => p.ProductCreatedByNavigation)   //載入同時紀錄誰建立商品
        //        .Include(p => p.ProductUpdatedByNavigation);  //載入同時紀錄誰最後更新商品
        //    return View(await gameSpacedatabaseContext.ToListAsync()); //非同步轉換成 List執行查詢並取得所有資料
        //}

        [HttpGet]// ===== Details (Modal) =====
        public async Task<IActionResult> Details(int id)
        {
            var p = await _context.ProductInfos
                         .Include(x => x.ProductCreatedByNavigation)
                         .Include(x => x.ProductUpdatedByNavigation)
                         .FirstOrDefaultAsync(x => x.ProductId == id);
            if (p == null) return NotFound();

            var vm = MapToVM(p);
            return PartialView("_DetailsModal", vm); //以 Modal + AJAX 方式運作GET 動作回傳 PartialView（只渲染 Modal 內容）
        }


        //舊版Detail GET: OnlineStore/ProductInfoes/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //	if (id == null)
        //	{
        //		return NotFound();
        //	}

        //	var productInfo = await _context.ProductInfos
        //		.Include(p => p.ProductCreatedByNavigation)
        //		.Include(p => p.ProductUpdatedByNavigation)
        //		.FirstOrDefaultAsync(m => m.ProductId == id);
        //	if (productInfo == null)
        //	{
        //		return NotFound();
        //	}

        //	return View(productInfo);
        //}

        [HttpGet]// ===== Create (Modal) =====
        public IActionResult Create()
        {
            var vm = new ProductInfoFormVM
            {
                CurrencyCode = "TWD",
                IsActive = true
            };
            return PartialView("_CreateEditModal", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductInfoFormVM vm)
        {
            if (!ModelState.IsValid)
                return PartialView("_CreateEditModal", vm);

            var entity = new ProductInfo
            {
                ProductName = vm.ProductName,
                ProductType = vm.ProductType,
                Price = vm.Price,
                CurrencyCode = vm.CurrencyCode,
                ShipmentQuantity = vm.ShipmentQuantity,
                IsActive = vm.IsActive,
                ProductCreatedBy = GetCurrentManagerId(),
                ProductCreatedAt = DateTime.Now
            };

            _context.ProductInfos.Add(entity);
            await _context.SaveChangesAsync();

            // 寫入 AuditLog：CREATE
            _context.ProductInfoAuditLogs.Add(new ProductInfoAuditLog
            {
                ProductId = entity.ProductId,
                ActionType = "CREATE",
                FieldName = "(all)",
                OldValue = null,
                NewValue = $"Name={entity.ProductName}, Price={entity.Price}",
                ManagerId = entity.ProductCreatedBy,
                ChangedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return Json(new { ok = true });
        }


        // 舊版Create GET: OnlineStore/ProductInfoes/Create
        //public IActionResult Create()
        //{
        //	ViewData["ProductCreatedBy"] = new SelectList(_context.ManagerData, "ManagerId", "ManagerId");
        //	ViewData["ProductUpdatedBy"] = new SelectList(_context.ManagerData, "ManagerId", "ManagerId");
        //	return View();
        //}

        //// POST: OnlineStore/ProductInfoes/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("ProductId,ProductName,ProductType,Price,CurrencyCode,ShipmentQuantity,ProductCreatedBy,ProductCreatedAt,ProductUpdatedBy,ProductUpdatedAt,IsActive")] ProductInfo productInfo)
        //{
        //	if (ModelState.IsValid)
        //	{
        //		_context.Add(productInfo);
        //		await _context.SaveChangesAsync();
        //		return RedirectToAction(nameof(Index));
        //	}
        //	ViewData["ProductCreatedBy"] = new SelectList(_context.ManagerData, "ManagerId", "ManagerId", productInfo.ProductCreatedBy);
        //	ViewData["ProductUpdatedBy"] = new SelectList(_context.ManagerData, "ManagerId", "ManagerId", productInfo.ProductUpdatedBy);
        //	return View(productInfo);
        //}

        // GET: OnlineStore/ProductInfoes/Edit/5

        [HttpGet] // ===== Edit (Modal)：成功回 JSON + 訊息  =====
        public async Task<IActionResult> Edit(int id)
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

            // 舊值（摘要）
            var old = new { p.ProductName, p.Price, p.ShipmentQuantity, p.IsActive };

            // 更新欄位
            p.ProductName = vm.ProductName;
            p.ProductType = vm.ProductType;
            p.Price = vm.Price;
            p.CurrencyCode = vm.CurrencyCode;
            p.ShipmentQuantity = vm.ShipmentQuantity;
            p.IsActive = vm.IsActive;
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

            // ✅ 回傳更新後要顯示的資料（用於不重整更新該列）
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
                    // 最後異動欄位：顯示用與 data-* 用各一個
                    lastChangedText = log.ChangedAt.ToString("yyyy/MM/dd tt hh:mm"),
                    lastChangedRaw = log.ChangedAt.ToString("yyyy/MM/dd HH:mm:ss"),
                    lastManagerId = log.ManagerId
                }
            });
        }


        //舊版Edit public async Task<IActionResult> Edit(int? id)
        //{
        //	if (id == null)
        //	{
        //		return NotFound();
        //	}

        //	var productInfo = await _context.ProductInfos.FindAsync(id);
        //	if (productInfo == null)
        //	{
        //		return NotFound();
        //	}
        //	ViewData["ProductCreatedBy"] = new SelectList(_context.ManagerData, "ManagerId", "ManagerId", productInfo.ProductCreatedBy);
        //	ViewData["ProductUpdatedBy"] = new SelectList(_context.ManagerData, "ManagerId", "ManagerId", productInfo.ProductUpdatedBy);
        //	return View(productInfo);
        //}

        //// POST: OnlineStore/ProductInfoes/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,ProductType,Price,CurrencyCode,ShipmentQuantity,ProductCreatedBy,ProductCreatedAt,ProductUpdatedBy,ProductUpdatedAt,IsActive")] ProductInfo productInfo)
        //{
        //	if (id != productInfo.ProductId)
        //	{
        //		return NotFound();
        //	}

        //	if (ModelState.IsValid)
        //	{
        //		try
        //		{
        //			_context.Update(productInfo);
        //			await _context.SaveChangesAsync();
        //		}
        //		catch (DbUpdateConcurrencyException)
        //		{
        //			if (!ProductInfoExists(productInfo.ProductId))
        //			{
        //				return NotFound();
        //			}
        //			else
        //			{
        //				throw;
        //			}
        //		}
        //		return RedirectToAction(nameof(Index));
        //	}
        //	ViewData["ProductCreatedBy"] = new SelectList(_context.ManagerData, "ManagerId", "ManagerId", productInfo.ProductCreatedBy);
        //	ViewData["ProductUpdatedBy"] = new SelectList(_context.ManagerData, "ManagerId", "ManagerId", productInfo.ProductUpdatedBy);
        //	return View(productInfo);
        //}

        // 舊版Delete GET: OnlineStore/ProductInfoes/Delete/5

        [HttpGet] // Delete(GET)：仍顯示確認 Modal（軟刪文案）
        public async Task<IActionResult> Delete(int productid)
        {
            var p = await _context.ProductInfos
                       .AsNoTracking()
                       .FirstOrDefaultAsync(x => x.ProductId == productid);
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

        [HttpPost]//DeleteConfirmed：軟刪 is_active=0 + AuditLog + 回傳訊息

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int productid)
        {
            var p = await _context.ProductInfos.FindAsync(productid);
            if (p == null) return NotFound();

            // 軟刪：下架即可：is_active = 0
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

            return Json(new { ok = true, msg = $"「{p.ProductName}」已下架（軟刪除）。" });
        }

        //舊版public async Task<IActionResult> Delete(int? id)
        //{
        //	if (id == null)
        //	{
        //		return NotFound();
        //	}

        //	var productInfo = await _context.ProductInfos
        //		.Include(p => p.ProductCreatedByNavigation)
        //		.Include(p => p.ProductUpdatedByNavigation)
        //		.FirstOrDefaultAsync(m => m.ProductId == id);
        //	if (productInfo == null)
        //	{
        //		return NotFound();
        //	}

        //	return View(productInfo);
        //}

        //// POST: OnlineStore/ProductInfoes/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //	var productInfo = await _context.ProductInfos.FindAsync(id);
        //	if (productInfo != null)
        //	{
        //		_context.ProductInfos.Remove(productInfo);
        //	}

        //	await _context.SaveChangesAsync();
        //	return RedirectToAction(nameof(Index));
        //}

        [HttpGet]// AuditLog (GET Modal)：指定 productId 的完整異動紀錄 
        public async Task<IActionResult> AuditLog(int id)
        {
            var logs = await _context.ProductInfoAuditLogs
                .Where(a => a.ProductId == id)
                .OrderByDescending(a => a.ChangedAt)
                .Select(a => new GameSpace.Areas.OnlineStore.ViewModels.ProductInfoAuditLogRowVM
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
            // 也可帶商品名
            var p = await _context.ProductInfos.AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == id);
            ViewBag.ProductName = p?.ProductName ?? $"#{id}";

            return PartialView("_AuditLogModal", logs);
        }

        private int GetCurrentManagerId()// ===== Helpers =====
        {
            // TODO: 從登入資訊取得實際 manager_id
            // 先用假資料方便開發
            return 1;
        }
        // POST: OnlineStore/ProductInfoes/ToggleActive/5
        //GetCurrentManagerId() 用你前面放的 helper。
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        private bool ProductInfoExists(int id)
        {
            return _context.ProductInfos.Any(e => e.ProductId == id);
        }

        //	//帶入ProudctDetails
        //	// GET: /OnlineStore/ProductInfo?view=card&...
        //	public async Task<IActionResult> Index(
        //		string view = "list", string q = null,
        //		int? supplierId = null, string platformName = null, string gameType = null,
        //		int? merchTypeId = null, string color = null, string size = null, string material = null,
        //		int page = 1, int pageSize = 12)
        //	{
        //		// base: ProductInfo + 左連 Game/Other + 對應 Supplier / MerchType
        //		var baseQuery =
        //			from p in _context.ProductInfos.AsNoTracking()
        //			join g0 in _context.GameProductDetails.AsNoTracking()
        //				on p.ProductId equals g0.ProductId into gj
        //			from g in gj.DefaultIfEmpty()
        //			join o0 in _context.OtherProductDetails.AsNoTracking()
        //				on p.ProductId equals o0.ProductId into oj
        //			from o in oj.DefaultIfEmpty()
        //			join sg0 in _context.Suppliers.AsNoTracking()
        //				on g.SupplierId equals sg0.SupplierId into sg
        //			from sGame in sg.DefaultIfEmpty()
        //			join so0 in _context.Suppliers.AsNoTracking()
        //				on o.SupplierId equals so0.SupplierId into so
        //			from sOther in so.DefaultIfEmpty()
        //			join mt0 in _context.MerchTypes.AsNoTracking()
        //				on o.MerchTypeId equals mt0.MerchTypeId into mtj
        //			from mt in mtj.DefaultIfEmpty()
        //			select new
        //			{
        //				p,
        //				g,
        //				o,
        //				SupplierIdGame = (int?)g.SupplierId,
        //				SupplierIdOther = (int?)o.SupplierId,
        //				SupplierNameGame = sGame != null ? sGame.SupplierName : null,
        //				SupplierNameOther = sOther != null ? sOther.SupplierName : null,
        //				MerchTypeId = (int?)o.MerchTypeId,
        //				MerchTypeName = mt != null ? mt.MerchTypeName : null
        //			};

        //		// 關鍵字（名稱/型別/描述/平台/規格）
        //		if (!string.IsNullOrWhiteSpace(q))
        //		{
        //			var kw = q.Trim();
        //			baseQuery = baseQuery.Where(x =>
        //				x.p.ProductName.Contains(kw) ||
        //				x.p.ProductType.Contains(kw) ||
        //				(x.g != null && (x.g.ProductName.Contains(kw) ||
        //								 x.g.ProductDescription.Contains(kw) ||
        //								 x.g.PlatformName.Contains(kw) ||
        //								 x.g.GameType.Contains(kw))) ||
        //				(x.o != null && (x.o.ProductName.Contains(kw) ||
        //								 x.o.ProductDescription.Contains(kw) ||
        //								 x.o.Color.Contains(kw) ||
        //								 x.o.Size.Contains(kw) ||
        //								 x.o.Material.Contains(kw))));
        //		}

        //		// 進階篩選（只有卡片時 UI 會露出，但後端兩種模式都支援）
        //		if (supplierId.HasValue)
        //			baseQuery = baseQuery.Where(x => x.SupplierIdGame == supplierId || x.SupplierIdOther == supplierId);

        //		if (!string.IsNullOrWhiteSpace(platformName))
        //			baseQuery = baseQuery.Where(x => x.g != null && x.g.PlatformName.Contains(platformName));

        //		if (!string.IsNullOrWhiteSpace(gameType))
        //			baseQuery = baseQuery.Where(x => x.g != null && x.g.GameType.Contains(gameType));

        //		if (merchTypeId.HasValue)
        //			baseQuery = baseQuery.Where(x => x.MerchTypeId == merchTypeId);

        //		if (!string.IsNullOrWhiteSpace(color))
        //			baseQuery = baseQuery.Where(x => x.o != null && x.o.Color.Contains(color));

        //		if (!string.IsNullOrWhiteSpace(size))
        //			baseQuery = baseQuery.Where(x => x.o != null && x.o.Size.Contains(size));

        //		if (!string.IsNullOrWhiteSpace(material))
        //			baseQuery = baseQuery.Where(x => x.o != null && x.o.Material.Contains(material));

        //		// 總數 + 分頁
        //		if (page < 1) page = 1;
        //		if (pageSize <= 0) pageSize = 12;

        //		var totalCount = await baseQuery.CountAsync();
        //		var skip = (page - 1) * pageSize;

        //		var pageRows = await baseQuery
        //			.OrderBy(x => x.p.ProductName)
        //			.Skip(skip).Take(pageSize)
        //			.Select(x => new ProductInfoListItemVM
        //			{
        //				ProductId = x.p.ProductId,
        //				ProductName = x.p.ProductName,
        //				ProductType = x.p.ProductType,
        //				Price = x.p.Price,
        //				CurrencyCode = x.p.CurrencyCode,

        //				SupplierName = x.SupplierNameGame ?? x.SupplierNameOther,
        //				Description = x.g != null ? x.g.ProductDescription : x.o.ProductDescription,

        //				PlatformName = x.g != null ? x.g.PlatformName : null,
        //				GameType = x.g != null ? x.g.GameType : null,

        //				MerchTypeName = x.MerchTypeName,
        //				Color = x.o != null ? x.o.Color : null,
        //				Size = x.o != null ? x.o.Size : null,
        //				Material = x.o != null ? x.o.Material : null
        //			})
        //			.ToListAsync();

        //		// 第二段查詢：把當頁每個 ProductId 的第一張圖抓回來（避免 EF set/projection 問題）
        //		var ids = pageRows.Select(r => r.ProductId).ToList();
        //		var imgMap = await _context.ProductImages
        //			.Where(i => ids.Contains(i.ProductId))
        //			.GroupBy(i => i.ProductId)
        //			.Select(g => new
        //			{
        //				ProductId = g.Key,
        //				ImageUrl = g.OrderBy(i => i.ProductimgId).Select(i => i.ProductimgUrl).FirstOrDefault(),
        //				ImageAlt = g.OrderBy(i => i.ProductimgId).Select(i => i.ProductimgAltText).FirstOrDefault()
        //			})
        //			.ToDictionaryAsync(x => x.ProductId, x => (x.ImageUrl, x.ImageAlt));

        //		foreach (var row in pageRows)
        //			if (imgMap.TryGetValue(row.ProductId, out var img))
        //			{ row.ImageUrl = img.ImageUrl; row.ImageAlt = img.ImageAlt; }

        //		// 下拉資料
        //		var supplierOpts = await _context.Suppliers
        //			.AsNoTracking()
        //			.OrderBy(s => s.SupplierName)
        //			.Select(s => new SelectListItem(s.SupplierName, s.SupplierId.ToString()))
        //			.ToListAsync();

        //		var merchTypeOpts = await _context.MerchTypes
        //			.AsNoTracking()
        //			.OrderBy(m => m.MerchTypeName)
        //			.Select(m => new SelectListItem(m.MerchTypeName, m.MerchTypeId.ToString()))
        //			.ToListAsync();

        //		var vm = new ProductInfoIndexVM
        //		{
        //			ViewMode = (view == "card") ? "card" : "list",
        //			Q = q,
        //			SupplierId = supplierId,
        //			PlatformName = platformName,
        //			GameType = gameType,
        //			MerchTypeId = merchTypeId,
        //			Color = color,
        //			Size = size,
        //			Material = material,
        //			Page = page,
        //			PageSize = pageSize,
        //			TotalCount = totalCount,
        //			Items = pageRows,
        //			SupplierOptions = supplierOpts,
        //			MerchTypeOptions = merchTypeOpts
        //		};

        //		return View(vm);
        //	}

        //	// GET: /OnlineStore/ProductInfo/Details/5
        //	public async Task<IActionResult> Detailss(int id)
        //	{
        //		var dto = await (
        //			from p in _context.ProductInfos.AsNoTracking().Where(x => x.ProductId == id)
        //			join g0 in _context.GameProductDetails.AsNoTracking()
        //				on p.ProductId equals g0.ProductId into gj
        //			from g in gj.DefaultIfEmpty()
        //			join o0 in _context.OtherProductDetails.AsNoTracking()
        //				on p.ProductId equals o0.ProductId into oj
        //			from o in oj.DefaultIfEmpty()
        //			join sg0 in _context.Suppliers.AsNoTracking()
        //				on g.SupplierId equals sg0.SupplierId into sg
        //			from sGame in sg.DefaultIfEmpty()
        //			join so0 in _context.Suppliers.AsNoTracking()
        //				on o.SupplierId equals so0.SupplierId into so
        //			from sOther in so.DefaultIfEmpty()
        //			join mt0 in _context.MerchTypes.AsNoTracking()
        //				on o.MerchTypeId equals mt0.MerchTypeId into mtj
        //			from mt in mtj.DefaultIfEmpty()
        //			select new ProductInfoDetailVM
        //			{
        //				ProductId = p.ProductId,
        //				ProductName = p.ProductName,
        //				ProductType = p.ProductType,
        //				Price = p.Price,
        //				CurrencyCode = p.CurrencyCode,

        //				SupplierName = sGame != null ? sGame.SupplierName : sOther.SupplierName,
        //				Description = g != null ? g.ProductDescription : o.ProductDescription,

        //				PlatformName = g != null ? g.PlatformName : null,
        //				GameType = g != null ? g.GameType : null,
        //				DownloadLink = g != null ? g.DownloadLink : null,

        //				MerchTypeName = mt != null ? mt.MerchTypeName : null,
        //				Color = o != null ? o.Color : null,
        //				Size = o != null ? o.Size : null,
        //				Material = o != null ? o.Material : null
        //			})
        //			.FirstOrDefaultAsync();

        //		if (dto == null) return NotFound();

        //		var img = await _context.ProductImages.AsNoTracking()
        //			.Where(i => i.ProductId == id)
        //			.OrderBy(i => i.ProductimgId)
        //			.Select(i => new { i.ProductimgUrl, i.ProductimgAltText })
        //			.FirstOrDefaultAsync();

        //		if (img != null)
        //		{ dto.ImageUrl = img.ProductimgUrl; dto.ImageAlt = img.ProductimgAltText; }

        //		return View(dto);
        //	}
    }
}

