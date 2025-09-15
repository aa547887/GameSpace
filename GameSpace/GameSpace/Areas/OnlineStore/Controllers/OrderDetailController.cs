using GameSpace.Areas.OnlineStore.ViewModels;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GameSpace.Areas.OnlineStore.ViewModels.OrderDetailViewModels;

namespace GameSpace.Areas.OnlineStore.Controllers
{
	[Area("OnlineStore")]
	public class OrderDetailController : Controller
	{
		private readonly GameSpacedatabaseContext _dbContext;// ① 這是你的 EF Core DbContext

		// ② 透過相依性注入拿到 DbContext 實例
		public OrderDetailController(GameSpacedatabaseContext dbContext)
		{
			_dbContext = dbContext;
		}
		#region 取得訂單明細

		// GET: /OnlineStore/Orders/Detail/123
		[HttpGet]
		public async Task<IActionResult> Detail(int id) // ③ id = OrderInfo.order_id
		{
			// ④ 從 OrderInfo 主表查一筆，投影到 ViewModel
			var vm = await _dbContext.OrderInfos
				.AsNoTracking()
				.Where(o => o.OrderId == id) // ⑤ o 代表 OrderInfo 的一列資料
				.Select(o => new OrderDetailViewModels
				{
					// ====== 一行一行來，左邊=ViewModel 欄位，右邊=資料來源 ======
					OrderCode = o.OrderCode,     // 來源：OrderInfo.OrderCode（o 的屬性）
					UserId = o.UserId,        // 來源：OrderInfo.UserId
					OrderTotal = o.OrderTotal,    // 來源：OrderInfo.OrderTotal
					OrderStatus = o.OrderStatus,   // 來源：OrderInfo.OrderStatus
					OrderDate = o.OrderDate,     // 來源：OrderInfo.OrderDate

					//付款
					PaymentCode = _dbContext.PaymentTransactions
						.Where(p => p.OrderId == o.OrderId
									&& p.TxnType == "PAY"
									&& p.Status == "SUCCESS")
						.OrderByDescending(p => p.CreatedAt)     // 或改成 p.PaymentAt（看你欄位）
						.Select(p => p.PaymentCode)              // 若沒有 PaymentCode，先用 p.PaymentId 或 p.ProviderTxn
						.FirstOrDefault(),

					PaymentAt = _dbContext.PaymentTransactions
						.Where(p => p.OrderId == o.OrderId
									&& p.TxnType == "PAY"
									&& p.Status == "SUCCESS")
						.OrderByDescending(p => p.CreatedAt)
						.Select(p => (DateTime?)p.CreatedAt)     // 沒資料會回 null → 建議 ViewModel 用 DateTime?
						.FirstOrDefault(),

					PaymentStatus = _dbContext.PaymentTransactions
						.Where(p => p.OrderId == o.OrderId)
						.OrderByDescending(p => p.CreatedAt)
						.Select(p => p.Status)
						.FirstOrDefault() ?? o.PaymentStatus,     // 完全沒交易 → 退回主表欄位

					//出貨
					// ✅ 出貨單號（沒有欄位就先用 shipment_id 或 provider 的單號代打）
					ShipmentCode = _dbContext.Shipments
						.Where(s => s.OrderId == o.OrderId)
						.OrderByDescending(s => s.ShippedAt)      // 或 .OrderByDescending(s => s.ShippedAt)
						.Select(s => s.ShipmentCode)              // 若沒有 ShipmentCode 欄位，改成 s.ShipmentId 或 s.ProviderShipmentNo
						.FirstOrDefault(),                        // 建議 ViewModel 用 long?，避免沒資料顯示 0

					// ✅ 出貨狀態（你 ViewModel 取名是 Status＝出貨狀態）
					Status = _dbContext.Shipments
						.Where(s => s.OrderId == o.OrderId)
						.OrderByDescending(s => s.ShippedAt)
						.Select(s => s.Status)                    // 來源：Shipments.Status
						.FirstOrDefault() ?? "未出貨",             // 沒任何出貨紀錄 → 顯示「未出貨」

					// ✅ 實際出貨時間（如果你 ViewModel 有 ShippedAt，建議新增）
					ShippedAt = _dbContext.Shipments
						.Where(s => s.OrderId == o.OrderId && s.ShippedAt != null)
						.OrderByDescending(s => s.ShippedAt)
						.Select(s => (DateTime?)s.ShippedAt)      // 轉成可空，沒有就回 null
						.FirstOrDefault(),


					//地址
					// === 收件地址：1:1，所以不需要任何排序，直接取這筆訂單的地址 ===
					Recipient = _dbContext.OrderAddresses
						.Where(a => a.OrderId == o.OrderId)      // 來源表：OrderAddresses，條件：a.OrderId = 這筆訂單 o.OrderId
						.Select(a => a.Recipient)                // 來源欄位：OrderAddresses.Recipient
						.FirstOrDefault() ?? "",                 // VM 是非可空字串，沒資料給空字串

					Phone = _dbContext.OrderAddresses
						.Where(a => a.OrderId == o.OrderId)
						.Select(a => a.Phone)                    // 來源欄位：OrderAddresses.Phone
						.FirstOrDefault() ?? "",

					Zipcode = _dbContext.OrderAddresses
						.Where(a => a.OrderId == o.OrderId)
						.Select(a => a.Zipcode)                  // 來源欄位：OrderAddresses.Zipcode
						.FirstOrDefault() ?? "",

					Address1 = _dbContext.OrderAddresses
						.Where(a => a.OrderId == o.OrderId)
						.Select(a => a.Address1)                 // 來源欄位：OrderAddresses.Address1
						.FirstOrDefault() ?? "",

					Address2 = _dbContext.OrderAddresses
						.Where(a => a.OrderId == o.OrderId)
						.Select(a => a.Address2)                 // 來源欄位：OrderAddresses.Address2（可為 null）
						.FirstOrDefault(),                       // VM 是 string?，可直接拿 null

					City = _dbContext.OrderAddresses
						.Where(a => a.OrderId == o.OrderId)
						.Select(a => a.City)                     // 來源欄位：OrderAddresses.City
						.FirstOrDefault() ?? "",

					Country = _dbContext.OrderAddresses
						.Where(a => a.OrderId == o.OrderId)
						.Select(a => a.Country)                  // 來源欄位：OrderAddresses.Country
						.FirstOrDefault() ?? "",
				})
				.FirstOrDefaultAsync();

			if (vm == null) return NotFound(); // ⑥ 查不到就回 404

			// 取出這筆訂單的所有明細
			var items = await _dbContext.OrderItems
				.AsNoTracking()
				.Where(oi => oi.OrderId == id)                         // 關聯條件：OrderItems.OrderId = 這筆訂單
				.Select(oi => new OrderItemRowVM
				{
					ProductId = oi.ProductId,                         // 來源：OrderItems.ProductId
					ProductName = _dbContext.ProductInfos              // 來源：ProductInfos.ProductName（用子查詢取名稱）
						.Where(p => p.ProductId == oi.ProductId)
						.Select(p => p.ProductName)
						.FirstOrDefault() ?? $"#{oi.ProductId}",       // 若找不到名稱就顯示 #ProductId
					ProductCode = _dbContext.ProductCodes
						.Where(pc => pc.ProductId == oi.ProductId)
						.Select(pc => pc.ProductCode1                   /* scaffold 若生 ProductCode1 就改這個 */)
						.FirstOrDefault(),
					Quantity = oi.Quantity,                          // 來源：OrderItems.Quantity
					UnitPrice = oi.UnitPrice,                         // 來源：OrderItems.UnitPrice
					Subtotal = oi.UnitPrice * oi.Quantity            // 來源：計算（或改用 oi.Subtotal）
				})
				.ToListAsync();

			vm.Items = items;

			return View("OrderDetail", vm); // ⑦ 把 ViewModel 丟給你現成的 OrderDetail.cshtml
		}
		#endregion

		[HttpGet]
		public IActionResult Search(string? q, string scope = "all")
		{
			if (string.IsNullOrWhiteSpace(q))
			{
				ViewBag.Q = "";
				return View(new List<OrderInfo>()); // 先回傳空清單
			}

			// 只示範：當數字 → 當 order_id / order_code 查
			// 之後再教你擴充模糊查
			IQueryable<OrderInfo> query = _dbContext.OrderInfos.AsNoTracking();
			switch (scope)
			{
				case "order_id":
					if (int.TryParse(q, out var id))
						query = query.Where(o => o.OrderId == id);
					else
					    query = query.Where(o => false);
						break;
				case "order_code":
					if (long.TryParse(q, out var code))
						query = query.Where(o => o.OrderCode == code);
					else
						query = query.Where(o => false);
					break;
				default: // "all"：先嘗試 code，再嘗試 id（誰符合就出現誰）
					bool hasCode = long.TryParse(q, out var codeAll);
					bool hasId = int.TryParse(q, out var idAll);

					if (hasCode || hasId)
					{
						query = query.Where(o =>
							(hasCode && o.OrderCode == codeAll) ||
							(hasId && o.OrderId == idAll));
					}
					else
					{
						query = query.Where(o => false);
					}
					break;
			}

			var results = query
				.OrderByDescending(o => o.OrderDate)
				.Take(50) // 先限 50 筆
				.ToList();

			ViewBag.Q = q;
			return View(results);
		}

	}
}

