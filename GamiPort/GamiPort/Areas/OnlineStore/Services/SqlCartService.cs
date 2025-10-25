using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // for [Keyless]
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using GamiPort.Areas.OnlineStore.DTO;   // CartLineDto / CartSummaryDto / CartVm / Legacy*
using GamiPort.Models;                  // GameSpacedatabaseContext

namespace GamiPort.Areas.OnlineStore.Services
{
	public class SqlCartService : ICartService
	{
		private readonly GameSpacedatabaseContext _db;
		public SqlCartService(GameSpacedatabaseContext db) => _db = db;

		// 取得或建立 cart_id
		public async Task<Guid> EnsureCartIdAsync(int? userId, Guid? anonymousToken)
		{
			var pUserId = new SqlParameter("@UserId", (object?)userId ?? DBNull.Value);
			var pAnon = new SqlParameter("@AnonymousToken", (object?)anonymousToken ?? DBNull.Value);

			var pOut = new SqlParameter("@CartId", SqlDbType.UniqueIdentifier)
			{
				Direction = ParameterDirection.Output
			};

			await _db.Database.ExecuteSqlRawAsync(
				"EXEC dbo.usp_Cart_Ensure @UserId, @AnonymousToken, @CartId OUTPUT",
				pUserId, pAnon, pOut);

			return (pOut.Value is DBNull) ? Guid.Empty : (Guid)pOut.Value;
		}

		// 加入商品
		public async Task AddAsync(Guid cartId, int productId, int quantity) =>
			await _db.Database.ExecuteSqlRawAsync(
		"EXEC dbo.usp_Cart_AddItem @CartId, @ProductId, @Quantity",
		new SqlParameter("@CartId", cartId),
		new SqlParameter("@ProductId", productId),
		new SqlParameter("@Quantity", quantity));

		// 為舊頁面保留的總覽（可移除）
		public async Task<LegacyCartSummaryDto> GetAsync(Guid cartId)
		{
			var rows = await _db.Set<CartItemRow>()
				.FromSqlRaw("EXEC dbo.usp_Cart_Get @CartId={0}", cartId)
				.AsNoTracking()
				.ToListAsync();

			var items = rows.Select(x =>
				new LegacyCartItemDto(x.item_id, x.product_id, x.product_name, x.unit_price, x.quantity, x.line_subtotal)
			).ToList();

			var sum = await _db.Set<CartSumRow>().FromSqlRaw(@"
                SELECT ISNULL(SUM(quantity),0) AS total_qty,
                       ISNULL(SUM(unit_price*quantity),0) AS subtotal
                FROM dbo.SO_CartItems WHERE cart_id = {0}", cartId)
				.AsNoTracking()
				.FirstAsync();

			return new LegacyCartSummaryDto(items, sum.total_qty, sum.subtotal);
		}

		// 更新數量（0 代表移除）
		public Task UpdateQtyAsync(Guid cartId, int productId, int qty) =>
			_db.Database.ExecuteSqlRawAsync(
				"EXEC dbo.usp_Cart_UpdateQty @CartId={0}, @ProductId={1}, @Quantity={2}",
				cartId, productId, qty);

		// 刪除單一品項（你庫裡的名字是 usp_Cart_RemoveItem）
		public async Task RemoveAsync(Guid cartId, int productId)
		{
			try
			{
				await _db.Database.ExecuteSqlRawAsync(
					"EXEC dbo.usp_Cart_RemoveItem @CartId={0}, @ProductId={1}",
					cartId, productId);
			}
			catch (Microsoft.Data.SqlClient.SqlException ex)
				when (ex.Message.Contains("Could not find stored procedure", StringComparison.OrdinalIgnoreCase)
				   || ex.Message.Contains("找不到預存程序"))
			{
				// 後援：用 UpdateQty=0 模擬刪除
				await _db.Database.ExecuteSqlRawAsync(
					"EXEC dbo.usp_Cart_UpdateQty @CartId={0}, @ProductId={1}, @Quantity={2}",
					cartId, productId, 0);
			}
		}

		// 清空（你庫裡叫 usp_Cart_Clear；再加一個保險後援）
		public async Task ClearAsync(Guid cartId)
		{
			try
			{
				await _db.Database.ExecuteSqlRawAsync("EXEC dbo.usp_Cart_Clear @CartId={0}", cartId);
			}
			catch (Microsoft.Data.SqlClient.SqlException ex)
				when (ex.Message.Contains("Could not find stored procedure", StringComparison.OrdinalIgnoreCase)
				   || ex.Message.Contains("找不到預存程序"))
			{
				// 後援：直接清資料表
				await _db.Database.ExecuteSqlRawAsync("DELETE FROM dbo.SO_CartItems WHERE cart_id = {0}", cartId);
			}
		}

		// [CHANGED] 新版摘要（給 Navbar 徽章 / 即時總計）
		public async Task<CartSummaryDto> GetSummaryAsync(Guid cartId, int shipMethodId, string destZip, string? couponCode = null)
		{
			await using var conn = _db.Database.GetDbConnection();
			if (conn.State != ConnectionState.Open) await conn.OpenAsync();

			await using var cmd = conn.CreateCommand();
			cmd.CommandText = "dbo.usp_Cart_GetSummary";
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add(new SqlParameter("@CartId", SqlDbType.UniqueIdentifier) { Value = cartId });
			cmd.Parameters.Add(new SqlParameter("@ShipMethodId", SqlDbType.Int) { Value = shipMethodId });
			cmd.Parameters.Add(new SqlParameter("@DestZip", SqlDbType.NVarChar, 10) { Value = (object?)destZip ?? DBNull.Value });
			cmd.Parameters.Add(new SqlParameter("@CouponCode", SqlDbType.NVarChar, 50) { Value = (object?)couponCode ?? DBNull.Value });

			var s = new CartSummaryDto(); // ← 先宣告容器，再去讀資料

			await using var reader = await cmd.ExecuteReaderAsync();
			if (await reader.ReadAsync())
			{
				// 讀常用欄位（皆有容錯別名）
				s.Item_Count_Total = GetInt32Or(reader, new[] { "item_count_total", "item_count" }, 0);
				s.Item_Count_Physical = GetInt32Or(reader, new[] { "item_count_physical" }, 0);
				s.Subtotal = GetDecimalOr(reader, new[] { "subtotal", "subtotal_all" }, 0m);
				s.Subtotal_Physical = GetDecimalOr(reader, new[] { "subtotal_physical" }, 0m);
				s.Discount = GetDecimalOr(reader, new[] { "discount", "discount_amount" }, 0m);
				s.Shipping_Fee = GetDecimalOr(reader, new[] { "shipping_fee" }, 0m);
				s.Grand_Total = GetDecimalOr(reader, new[] { "grand_total" }, 0m);
				s.Total_Weight_G = GetDecimalOr(reader, new[] { "total_weight_g", "total_weight" }, 0m);

				// 規則備註：先吃 JSON 欄位，沒有再退回單一字串欄位
				var notesJson = GetStringOr(reader, new[] { "rule_notes_json" }, null);
				if (!string.IsNullOrWhiteSpace(notesJson))
					s.Rule_Notes_Json = notesJson!;
				else
				{
					var notes = GetStringOr(reader, new[] { "rule_notes" }, null);
					s.Rule_Notes_Json = string.IsNullOrWhiteSpace(notes) ? "[]"
						: JsonSerializer.Serialize(new[] { notes });
				}

				s.Can_Checkout = GetBoolOr(reader, new[] { "can_checkout" }, true);
				s.Block_Reason = GetStringOr(reader, new[] { "block_reason" }, null);

				// ✅ 新增：優惠欄位（若 SQL 無此欄，fallback=0/null，不會拋例外）
				s.CouponDiscount = GetDecimalOr(reader,
					new[] { "coupon_discount", "coupondiscount", "CouponDiscount" }, 0m);

				s.CouponMessage = GetStringOr(reader,
					new[] { "coupon_message", "couponmessage", "CouponMessage" }, null);
			}

			return s;
		}

		// 取回兩個結果集（Lines + Summary）
		public async Task<CartVm> GetFullAsync(Guid cartId, int shipMethodId, string destZip, string? couponCode = null)
		{
			await using var conn = _db.Database.GetDbConnection();
			if (conn.State != ConnectionState.Open) await conn.OpenAsync();

			await using var cmd = conn.CreateCommand();
			cmd.CommandText = "dbo.usp_Cart_GetFull";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@CartId", SqlDbType.UniqueIdentifier) { Value = cartId });
			cmd.Parameters.Add(new SqlParameter("@ShipMethodId", SqlDbType.Int) { Value = shipMethodId });
			cmd.Parameters.Add(new SqlParameter("@DestZip", SqlDbType.NVarChar, 10) { Value = (object?)destZip ?? DBNull.Value });
			cmd.Parameters.Add(new SqlParameter("@CouponCode", SqlDbType.NVarChar, 50) { Value = (object?)couponCode ?? DBNull.Value });

			var lines = new List<CartLineDto>();
			var summary = new CartSummaryDto();

			await using var reader = await cmd.ExecuteReaderAsync();

			// ResultSet #1：Lines
			while (await reader.ReadAsync())
			{
				lines.Add(new CartLineDto
				{
					Product_Id = GetInt32(reader, "product_id"),
					Product_Name = GetString(reader, "product_name") ?? "",
					Image_Thumb = GetString(reader, "image_thumb"),
					Unit_Price = GetDecimal(reader, "unit_price"),
					Quantity = GetInt32(reader, "quantity"),
					Line_Subtotal = GetDecimal(reader, "line_subtotal"),
					Is_Physical = GetBool(reader, "is_physical"),
					Weight_G = GetDecimalOrZero(reader, "weight_g"),
					Status_Badge = GetString(reader, "status_badge"),
					Can_Checkout = GetBool(reader, "can_checkout"),
					Note = GetString(reader, "note")
				});
			}

			// ResultSet #2：Summary
			if (await reader.NextResultAsync() && await reader.ReadAsync())
			{
				summary.Item_Count_Total = GetInt32Or(reader, new[] { "item_count_total", "item_count" }, 0);
				summary.Item_Count_Physical = GetInt32Or(reader, new[] { "item_count_physical" }, 0);
				summary.Subtotal = GetDecimalOr(reader, new[] { "subtotal", "subtotal_all" }, 0m);
				summary.Subtotal_Physical = GetDecimalOr(reader, new[] { "subtotal_physical" }, 0m);
				summary.Discount = GetDecimalOr(reader, new[] { "discount", "discount_amount" }, 0m);
				summary.Shipping_Fee = GetDecimalOr(reader, new[] { "shipping_fee" }, 0m);
				summary.Grand_Total = GetDecimalOr(reader, new[] { "grand_total" }, 0m);
				summary.Total_Weight_G = GetDecimalOr(reader, new[] { "total_weight_g", "total_weight" }, 0m);

				var notesJson = GetStringOr(reader, new[] { "rule_notes_json" }, null);
				if (!string.IsNullOrWhiteSpace(notesJson))
					summary.Rule_Notes_Json = notesJson!;
				else
				{
					var notes = GetStringOr(reader, new[] { "rule_notes" }, null);
					summary.Rule_Notes_Json = string.IsNullOrWhiteSpace(notes) ? "[]"
						: JsonSerializer.Serialize(new[] { notes });
				}

				summary.Can_Checkout = GetBoolOr(reader, new[] { "can_checkout" }, true);
				summary.Block_Reason = GetStringOr(reader, new[] { "block_reason" }, null);

				// ✅ 新增：優惠欄位（若 SQL 無此欄，fallback=0/null，不會拋例外）
				summary.CouponDiscount = GetDecimalOr(reader,
					new[] { "coupon_discount", "coupondiscount", "CouponDiscount" }, 0m);

				summary.CouponMessage = GetStringOr(reader,
					new[] { "coupon_message", "couponmessage", "CouponMessage" }, null);
			}

			return new CartVm { Lines = lines, Summary = summary };
		}

		#region Keyless 容器（給舊 GetAsync 用）
		[Keyless]
		public class CartItemRow
		{
			public long item_id { get; set; }
			public int product_id { get; set; }
			public string product_name { get; set; } = "";
			public decimal unit_price { get; set; }
			public int quantity { get; set; }
			public decimal line_subtotal { get; set; }
		}

		[Keyless]
		public class CartSumRow
		{
			public int total_qty { get; set; }
			public decimal subtotal { get; set; }
		}
		#endregion

		#region DataReader helpers（容錯找欄位 + 型別安全）
		private static bool TryGetOrdinal(IDataRecord r, string name, out int ordinal)
		{
			for (int i = 0; i < r.FieldCount; i++)
				if (string.Equals(r.GetName(i), name, StringComparison.OrdinalIgnoreCase))
				{ ordinal = i; return true; }
			ordinal = -1; return false;
		}
		private static int GetOrdinalOr(IDataRecord r, string[] names)
		{
			foreach (var n in names) if (TryGetOrdinal(r, n, out int idx)) return idx;
			return -1;
		}

		private static string? GetString(IDataRecord r, string name)
			=> TryGetOrdinal(r, name, out var i) && !r.IsDBNull(i) ? r.GetString(i) : null;

		private static string? GetStringOr(IDataRecord r, string[] names, string? fallback)
		{
			var i = GetOrdinalOr(r, names);
			return (i >= 0 && !r.IsDBNull(i)) ? r.GetString(i) : fallback;
		}

		private static int GetInt32(IDataRecord r, string name)
			=> TryGetOrdinal(r, name, out var i) && !r.IsDBNull(i) ? r.GetInt32(i) : 0;

		private static int GetInt32Or(IDataRecord r, string[] names, int fallback)
		{
			var i = GetOrdinalOr(r, names);
			return (i >= 0) ? (r.IsDBNull(i) ? fallback : r.GetInt32(i)) : fallback;
		}

		private static bool GetBool(IDataRecord r, string name)
			=> TryGetOrdinal(r, name, out var i) && !r.IsDBNull(i) && r.GetBoolean(i);

		private static bool GetBoolOr(IDataRecord r, string[] names, bool fallback)
		{
			var i = GetOrdinalOr(r, names);
			return (i >= 0) ? (!r.IsDBNull(i) && r.GetBoolean(i)) : fallback;
		}

		private static decimal GetDecimal(IDataRecord r, string name)
			=> TryGetOrdinal(r, name, out var i) && !r.IsDBNull(i) ? r.GetDecimal(i) : 0m;

		private static decimal GetDecimalOr(IDataRecord r, string[] names, decimal fallback)
		{
			var i = GetOrdinalOr(r, names);
			return (i >= 0) ? (r.IsDBNull(i) ? fallback : r.GetDecimal(i)) : fallback;
		}

		private static decimal GetDecimalOrZero(IDataRecord r, string name)
			=> GetDecimal(r, name);
		#endregion
	}
}
