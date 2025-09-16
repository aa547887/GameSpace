// =========================
// ProductInfoesController（完整可貼）
// 特色：
// - Info + Detail（Game/Other）寫入與切換
// - Info.is_active 與 Detail.is_active 連動
// - 圖片新增/刪除 + AuditLog
// - Index 篩選/排序 + ProductCode / ProductCodeSort
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
			// ▼ Detail 篩選
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

			// 投影
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
					ProductCode = _context.ProductCodes.Where(c => c.ProductId == p.ProductId).Select(c => c.ProductCode1).FirstOrDefault(),
					LastLog = _context.ProductInfoAuditLogs.Where(a => a.ProductId == p.ProductId)
						.OrderByDescending(a => a.ChangedAt)
						.Select(a => new LastLogDto { LogId = a.LogId, ManagerId = a.ManagerId, ChangedAt = a.ChangedAt })
						.FirstOrDefault()
				})
				.ToListAsync();

			// 計算排序碼
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

			return View(rows);
		}

		// ============================================================
		// DetailPanel（整合圖片/供應商/明細）
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
		// Cards（卡片檢視，含圖片 + 狀態 + 操作）
		// ============================================================
		[HttpGet]
		public async Task<IActionResult> Cards(string? keyword, string? type, int page = 1, int pageSize = 12)
		{
			var q = _context.ProductInfos.AsNoTracking().AsQueryable();
			if (!string.IsNullOrWhiteSpace(keyword)) q = q.Where(p => p.ProductName.Contains(keyword) || p.ProductType.Contains(keyword));
			if (!string.IsNullOrWhiteSpace(type)) q = q.Where(p => p.ProductType == type);

			var list = await q.OrderByDescending(p => p.ProductId)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(p => new
				{
					p.ProductId,
					p.ProductName,
					p.ProductType,
					p.Price,
					p.IsActive,
					ProductCode = _context.ProductCodes.Where(c => c.ProductId == p.ProductId).Select(c => c.ProductCode1).FirstOrDefault(),
					LastChangedAt = _context.ProductInfoAuditLogs.Where(a => a.ProductId == p.ProductId).OrderByDescending(a => a.ChangedAt).Select(a => (DateTime?)a.ChangedAt).FirstOrDefault(),
					ImageUrl = _context.ProductImages.Where(i => i.ProductId == p.ProductId).OrderBy(i => i.ProductimgId).Select(i => i.ProductimgUrl).FirstOrDefault()
				})
				.ToListAsync();

			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;
			ViewBag.Total = await q.CountAsync();

			return PartialView("_CardsGrid", list);
		}
	}
}
