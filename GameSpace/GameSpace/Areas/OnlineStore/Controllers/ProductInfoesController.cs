// =========================
// ProductInfoesController（完整可貼）
// 特色：
// - Info + Detail（Game/Other）寫入與切換
// - Info.is_active 與 Detail.is_active 連動
// - 圖片新增/刪除 + AuditLog
// - Index 篩選/排序 + ProductCode / ProductCodeSort
// - 新增：InfoModal / DetailModal（兩種詳細彈窗）
// - 新增：Strips（寬條清單資料）
// - 調整：Cards/Strips 以商品代碼中的數字排序（無代碼者排最後）
// =========================

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using GameSpace.Areas.OnlineStore.ViewModels;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace GameSpace.Areas.OnlineStore.Controllers
{
	[Area("OnlineStore")]
	public class ProductInfoesController : Controller
	{
		private readonly GameSpacedatabaseContext _context;
		public ProductInfoesController(GameSpacedatabaseContext context) => _context = context;

		// ============================================================
		// Index（清單＋篩選）
		// ============================================================
		[HttpGet]
		public async Task<IActionResult> Index(
			string? keyword, string? type,
			int? qtyMin, int? qtyMax,
			string status = "active",
			DateTime? createdFrom = null, DateTime? createdTo = null,
			string? hasLog = null,
			// ▼ Detail 篩選（目前僅在寬條/卡片模式用；表格模式仍只看 Info）
			int? supplierId = null, int? platformId = null, int? merchTypeId = null,
			string? gameType = null, string? size = null, string? color = null, string? material = null,
			int? dqtyMin = null, int? dqtyMax = null
		)
		{
			var q = _context.ProductInfos.AsNoTracking().AsQueryable();

			if (!string.IsNullOrWhiteSpace(keyword))
				q = q.Where(p => p.ProductName.Contains(keyword) || p.ProductType.Contains(keyword));

			if (!string.IsNullOrWhiteSpace(type))
				q = q.Where(p => p.ProductType == type);

			if (qtyMin.HasValue) q = q.Where(p => p.ShipmentQuantity >= qtyMin);
			if (qtyMax.HasValue) q = q.Where(p => p.ShipmentQuantity <= qtyMax);

			if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
			{
				bool isActive = string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);
				q = q.Where(p => p.IsActive == isActive);
			}

			if (createdFrom.HasValue) q = q.Where(p => p.ProductCreatedAt >= createdFrom.Value);
			if (createdTo.HasValue)
			{
				var end = createdTo.Value.Date.AddDays(1);
				q = q.Where(p => p.ProductCreatedAt < end);
			}

			if (hasLog == "yes")
				q = q.Where(p => _context.ProductInfoAuditLogs.Any(a => a.ProductId == p.ProductId));
			else if (hasLog == "no")
				q = q.Where(p => !_context.ProductInfoAuditLogs.Any(a => a.ProductId == p.ProductId));

			// 投影（表格模式只顯示 Info）
			var rows = await q
				.OrderByDescending(p => p.ProductId)
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
					ProductCode = _context.ProductCodes
						.Where(c => c.ProductId == p.ProductId)
						.Select(c => c.ProductCode1)
						.FirstOrDefault(),
					LastLog = _context.ProductInfoAuditLogs.Where(a => a.ProductId == p.ProductId)
						.OrderByDescending(a => a.ChangedAt)
						.Select(a => new LastLogDto { LogId = a.LogId, ManagerId = a.ManagerId, ChangedAt = a.ChangedAt })
						.FirstOrDefault()
				})
				.ToListAsync();

			// 計算排序碼（表格由 DataTables data-order 使用）
			foreach (var r in rows)
			{
				if (!string.IsNullOrWhiteSpace(r.ProductCode))
				{
					var s = r.ProductCode.Trim();
					var digits = new string(s.SkipWhile(ch => !char.IsDigit(ch)).TakeWhile(char.IsDigit).ToArray());
					if (int.TryParse(digits, out var n)) r.ProductCodeSort = n;
				}
			}

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

			// 供進階篩選（寬條/卡片會用到）
			ViewBag.SupplierId = supplierId;
			ViewBag.PlatformId = platformId;
			ViewBag.MerchTypeId = merchTypeId;
			ViewBag.GameType = gameType;
			ViewBag.Size = size; ViewBag.Color = color; ViewBag.Material = material;
			ViewBag.DQtyMin = dqtyMin; ViewBag.DQtyMax = dqtyMax;

			ViewBag.SupplierList = await _context.Suppliers.AsNoTracking()
				.OrderBy(s => s.SupplierName)
				.Select(s => new { s.SupplierId, s.SupplierName })
				.ToListAsync();

			// 你若有平台主檔可替換，暫時用 game detail 取 distinct
			ViewBag.PlatformList = await _context.GameProductDetails.AsNoTracking()
				.Where(g => g.PlatformId != null)
				.Select(g => new { g.PlatformId, g.PlatformName })
				.Distinct()
				.OrderBy(g => g.PlatformName)
				.ToListAsync();

			ViewBag.MerchTypeList = await _context.MerchTypes.AsNoTracking()
				.OrderBy(m => m.MerchTypeName)
				.Select(m => new { m.MerchTypeId, m.MerchTypeName })
				.ToListAsync();

			return View(rows);
		}

		// ============================================================
		// Info 詳細彈窗（Info-only）→ _InfoModal.cshtml
		// ============================================================
		[HttpGet]
		public async Task<IActionResult> InfoModal(int id)
		{
			var p = await _context.ProductInfos
				.Include(x => x.ProductCreatedByNavigation)
				.Include(x => x.ProductUpdatedByNavigation)
				.AsNoTracking()
				.FirstOrDefaultAsync(x => x.ProductId == id);
			if (p == null) return NotFound();

			return PartialView("_InfoModal", MapToFormVM(p));
		}

		// ============================================================
		// Detail 詳細彈窗（Game / NoGame 自動切）→ _DetailModal.cshtml
		// ============================================================
		[HttpGet]
		public async Task<IActionResult> DetailModal(int id)
		{
			var p = await _context.ProductInfos.AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == id);
			if (p == null) return NotFound();

			var vm = new ProductDetailModalVM
			{
				ProductId = p.ProductId,
				ProductName = p.ProductName,
				ProductType = p.ProductType,
				IsActive = p.IsActive,
				Price = p.Price,
				ShipmentQuantity = p.ShipmentQuantity,
				ProductCreatedAt = p.ProductCreatedAt,
				ProductCreatedBy = p.ProductCreatedBy,
				ProductUpdatedAt = p.ProductUpdatedAt,
				ProductUpdatedBy = p.ProductUpdatedBy,
				Images = await _context.ProductImages.AsNoTracking()
					.Where(i => i.ProductId == id)
					.OrderBy(i => i.ProductimgId)
					.Select(i => new ProductDetailModalVM.ImageVM { Id = i.ProductimgId, Url = i.ProductimgUrl, Alt = i.ProductimgAltText })
					.ToListAsync()
			};

			if (p.ProductType == "game")
			{
				var d = await _context.GameProductDetails.AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == id);
				if (d != null)
				{
					vm.SupplierName = await _context.Suppliers.Where(s => s.SupplierId == d.SupplierId).Select(s => s.SupplierName).FirstOrDefaultAsync();
					vm.PlatformId = d.PlatformId;
					vm.PlatformName = d.PlatformName;
					vm.GameType = d.GameType;
					vm.DownloadLink = d.DownloadLink;
					vm.GameDescription = d.ProductDescription;
				}
			}
			else
			{
				var d = await _context.OtherProductDetails.AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == id);
				if (d != null)
				{
					vm.SupplierName = await _context.Suppliers.Where(s => s.SupplierId == d.SupplierId).Select(s => s.SupplierName).FirstOrDefaultAsync();
					vm.MerchTypeId = d.MerchTypeId;
					vm.MerchTypeName = await _context.MerchTypes.Where(m => m.MerchTypeId == d.MerchTypeId).Select(m => m.MerchTypeName).FirstOrDefaultAsync();
					vm.DigitalCode = d.DigitalCode;
					vm.Size = d.Size;
					vm.Color = d.Color;
					vm.Weight = d.Weight;
					vm.Dimensions = d.Dimensions;
					vm.Material = d.Material;
					vm.StockQuantity = d.StockQuantity;
					vm.OtherDescription = d.ProductDescription;
				}
			}

			return PartialView("_DetailModal", vm);
		}

		// ============================================================
		// DetailPanel（舊：整合圖片/供應商/明細）— 仍保留供既有按鈕/頁面使用
		// ============================================================
		[HttpGet]
		public async Task<IActionResult> DetailPanel(int id)
		{
			var p = await _context.ProductInfos.AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == id);
			if (p == null) return NotFound();

			var imgs = await _context.ProductImages.AsNoTracking()
				.Where(x => x.ProductId == id)
				.OrderBy(x => x.ProductimgId)
				.Select(x => new { x.ProductimgUrl, x.ProductimgAltText })
				.ToListAsync();

			if (p.ProductType == "game")
			{
				var d = await _context.GameProductDetails.AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == id);
				if (d != null)
				{
					var supplierName = await _context.Suppliers.Where(s => s.SupplierId == d.SupplierId).Select(s => s.SupplierName).FirstOrDefaultAsync();
					return PartialView("_DetailPanelGame", new { Basic = p, Detail = d, SupplierName = supplierName, Images = imgs });
				}
			}
			else
			{
				var d = await _context.OtherProductDetails.AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == id);
				if (d != null)
				{
					var supplierName = await _context.Suppliers.Where(s => s.SupplierId == d.SupplierId).Select(s => s.SupplierName).FirstOrDefaultAsync();
					var merchTypeName = await _context.MerchTypes.Where(m => m.MerchTypeId == d.MerchTypeId).Select(m => m.MerchTypeName).FirstOrDefaultAsync();
					return PartialView("_DetailPanelOther", new { Basic = p, Detail = d, SupplierName = supplierName, MerchTypeName = merchTypeName, Images = imgs });
				}
			}

			return PartialView("_DetailPanelBasic", new { Basic = p, Detail = (object?)null, Images = imgs });
		}

		// ============================================================
		// Cards（卡片檢視，支援進階篩選）→ 以商品代碼數字排序
		// ============================================================
		[HttpGet]
		public async Task<IActionResult> Cards(
			string? keyword, string? type, int page = 1, int pageSize = 12,
			int? supplierId = null, int? platformId = null, int? merchTypeId = null,
			string? gameType = null
		)
		{
			var q = _context.ProductInfos.AsNoTracking().AsQueryable();
			if (!string.IsNullOrWhiteSpace(keyword)) q = q.Where(p => p.ProductName.Contains(keyword) || p.ProductType.Contains(keyword));
			if (!string.IsNullOrWhiteSpace(type)) q = q.Where(p => p.ProductType == type);

			// 進階條件（用子查詢過濾）
			if (supplierId.HasValue)
				q = q.Where(p =>
					_context.GameProductDetails.Any(g => g.ProductId == p.ProductId && g.SupplierId == supplierId) ||
					_context.OtherProductDetails.Any(o => o.ProductId == p.ProductId && o.SupplierId == supplierId));

			if (platformId.HasValue)
				q = q.Where(p => _context.GameProductDetails.Any(g => g.ProductId == p.ProductId && g.PlatformId == platformId));

			if (merchTypeId.HasValue)
				q = q.Where(p => _context.OtherProductDetails.Any(o => o.ProductId == p.ProductId && o.MerchTypeId == merchTypeId));

			if (!string.IsNullOrWhiteSpace(gameType))
				q = q.Where(p => _context.GameProductDetails.Any(g => g.ProductId == p.ProductId && g.GameType.Contains(gameType)));

			var list = await q.Select(p => new
			{
				p.ProductId,
				p.ProductName,
				p.ProductType,
				p.Price,
				p.IsActive,
				ProductCode = _context.ProductCodes.Where(c => c.ProductId == p.ProductId).Select(c => c.ProductCode1).FirstOrDefault(),
				LastChangedAt = _context.ProductInfoAuditLogs.Where(a => a.ProductId == p.ProductId).OrderByDescending(a => a.ChangedAt).Select(a => (DateTime?)a.ChangedAt).FirstOrDefault(),
				ImageUrl = _context.ProductImages.Where(i => i.ProductId == p.ProductId).OrderBy(i => i.ProductimgId).Select(i => i.ProductimgUrl).FirstOrDefault()
			}).ToListAsync();

			list = list.OrderBy(x =>
			{
				if (string.IsNullOrWhiteSpace(x.ProductCode)) return int.MaxValue;
				var s = new string(x.ProductCode.Trim().SkipWhile(ch => !char.IsDigit(ch)).TakeWhile(char.IsDigit).ToArray());
				return int.TryParse(s, out var n) ? n : int.MaxValue;
			}).ToList();

			var total = list.Count;
			page = Math.Max(1, page);
			pageSize = Math.Max(1, pageSize);
			var pageList = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;
			ViewBag.Total = total;

			return PartialView("_CardsGrid", pageList);
		}

		// ============================================================
		// Strips（寬條清單，支援進階篩選）→ 以商品代碼數字排序
		// ============================================================
		[HttpGet]
		public async Task<IActionResult> Strips(
			string? keyword, string? type, int page = 1, int pageSize = 12,
			int? supplierId = null, int? platformId = null, int? merchTypeId = null,
			string? gameType = null
		)
		{
			var q = _context.ProductInfos.AsNoTracking().AsQueryable();
			if (!string.IsNullOrWhiteSpace(keyword)) q = q.Where(p => p.ProductName.Contains(keyword) || p.ProductType.Contains(keyword));
			if (!string.IsNullOrWhiteSpace(type)) q = q.Where(p => p.ProductType == type);

			if (supplierId.HasValue)
				q = q.Where(p =>
					_context.GameProductDetails.Any(g => g.ProductId == p.ProductId && g.SupplierId == supplierId) ||
					_context.OtherProductDetails.Any(o => o.ProductId == p.ProductId && o.SupplierId == supplierId));

			if (platformId.HasValue)
				q = q.Where(p => _context.GameProductDetails.Any(g => g.ProductId == p.ProductId && g.PlatformId == platformId));

			if (merchTypeId.HasValue)
				q = q.Where(p => _context.OtherProductDetails.Any(o => o.ProductId == p.ProductId && o.MerchTypeId == merchTypeId));

			if (!string.IsNullOrWhiteSpace(gameType))
				q = q.Where(p => _context.GameProductDetails.Any(g => g.ProductId == p.ProductId && g.GameType.Contains(gameType)));

			var list = await q.Select(p => new
			{
				p.ProductId,
				p.ProductName,
				p.ProductType,
				p.Price,
				p.IsActive,
				p.ShipmentQuantity,
				p.ProductCreatedAt,
				ProductCode = _context.ProductCodes.Where(c => c.ProductId == p.ProductId).Select(c => c.ProductCode1).FirstOrDefault(),
				LastChangedAt = _context.ProductInfoAuditLogs.Where(a => a.ProductId == p.ProductId).OrderByDescending(a => a.ChangedAt).Select(a => (DateTime?)a.ChangedAt).FirstOrDefault(),
				ImageUrl = _context.ProductImages.Where(i => i.ProductId == p.ProductId).OrderBy(i => i.ProductimgId).Select(i => i.ProductimgUrl).FirstOrDefault()
			}).ToListAsync();

			list = list.OrderBy(x =>
			{
				if (string.IsNullOrWhiteSpace(x.ProductCode)) return int.MaxValue;
				var s = new string(x.ProductCode.Trim().SkipWhile(ch => !char.IsDigit(ch)).TakeWhile(char.IsDigit).ToArray());
				return int.TryParse(s, out var n) ? n : int.MaxValue;
			}).ToList();

			var total = list.Count;
			page = Math.Max(1, page);
			pageSize = Math.Max(1, pageSize);
			var pageList = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

			ViewBag.Page = page; ViewBag.PageSize = pageSize; ViewBag.Total = total;
			return PartialView("_StripsList", pageList);
		}

		// ============================================================
		// Create（Modal）
		// ============================================================
		[HttpGet]
		public IActionResult Create()
		{
			var vm = new ProductInfoFormVM { CurrencyCode = "TWD", IsActive = true, ProductType = "game" };
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
				if (!vm.SupplierId.HasValue) return Json(new { ok = false, msg = "請選擇供應商。" });
				if (vm.ProductType == "nogame" && !vm.MerchTypeId.HasValue) return Json(new { ok = false, msg = "非遊戲類需選擇周邊分類。" });

				var entity = new ProductInfo();
				ApplyFromVM(entity, vm);
				entity.ProductCreatedBy = GetCurrentManagerId();
				entity.ProductCreatedAt = DateTime.Now;

				_context.ProductInfos.Add(entity);
				await _context.SaveChangesAsync(); // 先拿 ProductId

				if (vm.ProductType == "game")
				{
					_context.GameProductDetails.Add(new GameProductDetail
					{
						ProductId = entity.ProductId,
						SupplierId = vm.SupplierId!.Value,
						PlatformId = vm.PlatformId,
						PlatformName = vm.PlatformName,
						GameType = vm.GameType,
						DownloadLink = vm.DownloadLink,
						ProductDescription = vm.GameProductDescription,
						IsActive = vm.IsActive
					});
				}
				else
				{
					_context.OtherProductDetails.Add(new OtherProductDetail
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
						StockQuantity = vm.StockQuantity ?? 0,
						ProductDescription = vm.OtherProductDescription,
						IsActive = vm.IsActive
					});
				}

				// 圖片新增
				await SaveNewImagesAsync(entity.ProductId, entity.ProductName, entity.ProductCreatedBy, vm.Images);

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

		// ============================================================
		// Edit（Modal）
		// ============================================================
		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var p = await _context.ProductInfos.FindAsync(id);
			if (p == null) return NotFound();
			return PartialView("_CreateEditModal", MapToFormVM(p));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(ProductInfoFormVM vm)
		{
			if (!ModelState.IsValid)
				return PartialView("_CreateEditModal", vm);

			var p = await _context.ProductInfos.FindAsync(vm.ProductId);
			if (p == null) return NotFound();

			var old = new { p.ProductName, p.Price, p.ShipmentQuantity, p.IsActive, p.ProductType };
			var oldType = p.ProductType;

			ApplyFromVM(p, vm);
			p.ProductUpdatedBy = GetCurrentManagerId();
			p.ProductUpdatedAt = DateTime.Now;

			// 類型切換處理：刪舊建新
			if (oldType != vm.ProductType)
			{
				if (oldType == "game")
				{
					var g = await _context.GameProductDetails.FirstOrDefaultAsync(x => x.ProductId == p.ProductId);
					if (g != null) _context.GameProductDetails.Remove(g);
				}
				else
				{
					var o = await _context.OtherProductDetails.FirstOrDefaultAsync(x => x.ProductId == p.ProductId);
					if (o != null) _context.OtherProductDetails.Remove(o);
				}

				if (vm.ProductType == "game")
				{
					_context.GameProductDetails.Add(new GameProductDetail
					{
						ProductId = p.ProductId,
						SupplierId = vm.SupplierId!.Value,
						PlatformId = vm.PlatformId,
						PlatformName = vm.PlatformName,
						GameType = vm.GameType,
						DownloadLink = vm.DownloadLink,
						ProductDescription = vm.GameProductDescription,
						IsActive = vm.IsActive
					});
				}
				else
				{
					_context.OtherProductDetails.Add(new OtherProductDetail
					{
						ProductId = p.ProductId,
						SupplierId = vm.SupplierId!.Value,
						MerchTypeId = vm.MerchTypeId,
						DigitalCode = vm.DigitalCode,
						Size = vm.Size,
						Color = vm.Color,
						Weight = vm.Weight,
						Dimensions = vm.Dimensions,
						Material = vm.Material,
						StockQuantity = vm.StockQuantity ?? 0,
						ProductDescription = vm.OtherProductDescription,
						IsActive = vm.IsActive
					});
				}
			}
			else
			{
				// 類型未變：更新 Detail
				if (vm.ProductType == "game")
				{
					var d = await _context.GameProductDetails.FirstOrDefaultAsync(x => x.ProductId == p.ProductId);
					if (d == null) { d = new GameProductDetail { ProductId = p.ProductId }; _context.GameProductDetails.Add(d); }
					d.SupplierId = vm.SupplierId!.Value;
					d.PlatformId = vm.PlatformId;
					d.PlatformName = vm.PlatformName;
					d.GameType = vm.GameType;
					d.DownloadLink = vm.DownloadLink;
					d.ProductDescription = vm.GameProductDescription;
					d.IsActive = vm.IsActive;
				}
				else
				{
					var d = await _context.OtherProductDetails.FirstOrDefaultAsync(x => x.ProductId == p.ProductId);
					if (d == null) { d = new OtherProductDetail { ProductId = p.ProductId }; _context.OtherProductDetails.Add(d); }
					d.SupplierId = vm.SupplierId!.Value;
					d.MerchTypeId = vm.MerchTypeId;
					d.DigitalCode = vm.DigitalCode;
					d.Size = vm.Size;
					d.Color = vm.Color;
					d.Weight = vm.Weight;
					d.Dimensions = vm.Dimensions;
					d.Material = vm.Material;
					d.StockQuantity = vm.StockQuantity ?? d.StockQuantity;
					d.ProductDescription = vm.OtherProductDescription;
					d.IsActive = vm.IsActive;
				}
			}

			// 刪除舊圖（勾選 Remove 的）
			if (vm.ExistingImages != null && vm.ExistingImages.Count > 0)
			{
				var idsToRemove = vm.ExistingImages.Where(x => x.Remove).Select(x => x.ImageId).ToList();
				if (idsToRemove.Count > 0)
				{
					var imgs = await _context.ProductImages.Where(i => i.ProductId == p.ProductId && idsToRemove.Contains(i.ProductimgId)).ToListAsync();
					foreach (var img in imgs)
					{
						_context.ProductImages.Remove(img);
						_context.ProductInfoAuditLogs.Add(new ProductInfoAuditLog
						{
							ProductId = p.ProductId,
							ActionType = "UPDATE",
							FieldName = "image:remove",
							OldValue = img.ProductimgUrl,
							NewValue = null,
							ManagerId = p.ProductUpdatedBy,
							ChangedAt = DateTime.Now
						});
					}
				}
			}

			// 新增新上傳圖
			await SaveNewImagesAsync(p.ProductId, p.ProductName, p.ProductUpdatedBy, vm.Images);

			await _context.SaveChangesAsync();

			var log = new ProductInfoAuditLog
			{
				ProductId = p.ProductId,
				ActionType = "UPDATE",
				FieldName = "(mixed)",
				OldValue = $"Name={old.ProductName}, Price={old.Price}, Qty={old.ShipmentQuantity}, Active={old.IsActive}, Type={old.ProductType}",
				NewValue = $"Name={p.ProductName}, Price={p.Price}, Qty={p.ShipmentQuantity}, Active={p.IsActive}, Type={p.ProductType}",
				ManagerId = p.ProductUpdatedBy,
				ChangedAt = DateTime.Now
			};
			_context.ProductInfoAuditLogs.Add(log);
			await _context.SaveChangesAsync();

			return Json(new
			{
				ok = true,
				msg = $"「{p.ProductName}」的商品資訊已修改完成！",
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

		// ============================================================
		// Delete（軟刪）
		// ============================================================
		[HttpGet]
		public async Task<IActionResult> Delete(int id)
		{
			var p = await _context.ProductInfos.AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == id);
			if (p == null) return NotFound();

			var vm = new ProductInfoFormVM { ProductId = p.ProductId, ProductName = p.ProductName, ProductType = p.ProductType, Price = p.Price, IsActive = p.IsActive };
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

			// 連動 Detail
			if (p.ProductType == "game")
			{
				var g = await _context.GameProductDetails.FirstOrDefaultAsync(x => x.ProductId == id);
				if (g != null) g.IsActive = false;
			}
			else
			{
				var o = await _context.OtherProductDetails.FirstOrDefaultAsync(x => x.ProductId == id);
				if (o != null) o.IsActive = false;
			}

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

		// ============================================================
		// AuditLog（Modal）
		// ============================================================
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

		// ============================================================
		// ToggleActive（Info/Detail 連動）
		// ============================================================
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

			if (p.ProductType == "game")
			{
				var g = await _context.GameProductDetails.FirstOrDefaultAsync(x => x.ProductId == id);
				if (g != null) g.IsActive = p.IsActive;
			}
			else
			{
				var o = await _context.OtherProductDetails.FirstOrDefaultAsync(x => x.ProductId == id);
				if (o != null) o.IsActive = p.IsActive;
			}

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

		// ============================================================
		// Helpers
		// ============================================================
		private int GetCurrentManagerId() => 30000060; // ★ 請確定 DB 真的有這個 ManagerData

		private void ApplyFromVM(ProductInfo e, ProductInfoFormVM vm)
		{
			e.ProductName = vm.ProductName.Trim();
			e.ProductType = vm.ProductType;   // "game"/"nogame"
			e.Price = vm.Price;
			e.CurrencyCode = vm.CurrencyCode;
			e.ShipmentQuantity = vm.ShipmentQuantity;
			e.IsActive = vm.IsActive;
		}

		private ProductInfoFormVM MapToFormVM(ProductInfo e)
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
				ProductUpdatedBy = e.ProductUpdatedBy,
				ExistingImages = _context.ProductImages
					.Where(i => i.ProductId == e.ProductId)
					.OrderBy(i => i.ProductimgId)
					.Select(i => new ProductInfoFormVM.ExistingImageItem
					{
						ImageId = i.ProductimgId,
						Url = i.ProductimgUrl,
						Alt = i.ProductimgAltText
					})
					.ToList()
			};

			if (e.ProductType == "game")
			{
				var d = _context.GameProductDetails.AsNoTracking().FirstOrDefault(x => x.ProductId == e.ProductId);
				if (d != null)
				{
					vm.SupplierId = d.SupplierId;
					vm.PlatformId = d.PlatformId;
					vm.PlatformName = d.PlatformName;
					vm.GameType = d.GameType;
					vm.DownloadLink = d.DownloadLink;
					vm.GameProductDescription = d.ProductDescription;
				}
			}
			else
			{
				var d = _context.OtherProductDetails.AsNoTracking().FirstOrDefault(x => x.ProductId == e.ProductId);
				if (d != null)
				{
					vm.SupplierId = d.SupplierId;
					vm.MerchTypeId = d.MerchTypeId;
					vm.DigitalCode = d.DigitalCode;
					vm.Size = d.Size;
					vm.Color = d.Color;
					vm.Weight = d.Weight; // string
					vm.Dimensions = d.Dimensions;
					vm.Material = d.Material;
					vm.StockQuantity = d.StockQuantity;
					vm.OtherProductDescription = d.ProductDescription;
				}
			}

			return vm;
		}

		/// <summary>儲存新上傳圖片 + 寫入 AuditLog</summary>
		private async Task SaveNewImagesAsync(int productId, string productName, int? managerId, IFormFile[]? files)
		{
			if (files is not { Length: > 0 }) return;

			var root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products", productId.ToString());
			Directory.CreateDirectory(root);

			foreach (var file in files)
			{
				if (file == null || file.Length <= 0) continue;

				var fname = $"{DateTime.Now:yyyyMMddHHmmssfff}_{Path.GetFileName(file.FileName)}";
				var full = Path.Combine(root, fname);
				using (var fs = new FileStream(full, FileMode.Create))
					await file.CopyToAsync(fs);

				var url = $"/uploads/products/{productId}/{fname}";
				_context.ProductImages.Add(new ProductImage
				{
					ProductId = productId,
					ProductimgUrl = url,
					ProductimgAltText = productName,
					ProductimgUpdatedAt = DateTime.Now
				});

				_context.ProductInfoAuditLogs.Add(new ProductInfoAuditLog
				{
					ProductId = productId,
					ActionType = "UPDATE",
					FieldName = "image:add",
					OldValue = null,
					NewValue = url,
					ManagerId = managerId,
					ChangedAt = DateTime.Now
				});
			}
		}
	}
}
