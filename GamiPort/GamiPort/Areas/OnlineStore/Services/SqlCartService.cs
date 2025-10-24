using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using GamiPort.Areas.OnlineStore.DTO;   // << 這個 namespace 有你的 CartLineDto/CartSummaryDto/CartVm
using GamiPort.Models;                  // DbContext

namespace GamiPort.Areas.OnlineStore.Services
{
	public class SqlCartService : ICartService
	{
		private readonly GameSpacedatabaseContext _db;
		public SqlCartService(GameSpacedatabaseContext db) => _db = db;

		// 取得或建立 cart_id
		public async Task<Guid> EnsureCartIdAsync(int? userId, Guid? anonymousToken)
		{
			var outParam = new SqlParameter("@OutCartId", SqlDbType.UniqueIdentifier)
			{ Direction = ParameterDirection.Output };

			await _db.Database.ExecuteSqlRawAsync(
				"EXEC dbo.usp_Cart_Ensure @UserId={0}, @AnonymousToken={1}, @OutCartId=@OutCartId OUTPUT",
				userId, (object?)anonymousToken ?? DBNull.Value, outParam);

			return (Guid)outParam.Value;
		}

		// 加入商品
		public Task AddAsync(Guid cartId, int productId, int quantity) =>
			_db.Database.ExecuteSqlRawAsync(
				"EXEC dbo.usp_Cart_AddItem @CartId={0}, @ProductId={1}, @Quantity={2}",
				cartId, productId, quantity);

		// 舊版總覽：為了相容舊頁面（使用 Legacy* 型別）
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
				SELECT ISNULL(SUM(quantity),0) total_qty,
					   ISNULL(SUM(unit_price*quantity),0) subtotal
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

		// 移除
		public Task RemoveAsync(Guid cartId, int productId) =>
			_db.Database.ExecuteSqlRawAsync(
				"EXEC dbo.usp_Cart_RemoveItem @CartId={0}, @ProductId={1}",
				cartId, productId);

		// 清空
		public Task ClearAsync(Guid cartId) =>
			_db.Database.ExecuteSqlRawAsync("EXEC dbo.usp_Cart_Clear @CartId={0}", cartId);

		// ✅ 一次取回兩個結果集（Lines + Summary），對齊你 DTO 的底線命名
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

			// ResultSet #1：Lines（usp_Cart_GetLines）
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

			// ResultSet #2：Summary（usp_Cart_GetSummary，內部可用 iTVF）
			if (await reader.NextResultAsync() && await reader.ReadAsync())
			{
				// DB 欄位名稱依你的 SP/iTVF；右邊是對應到你 C# 的底線命名屬性
				summary.Item_Count_Total = GetInt32Or(reader, new[] { "item_count_total", "item_count" }, 0);
				summary.Item_Count_Physical = GetInt32Or(reader, new[] { "item_count_physical" }, 0);

				summary.Subtotal = GetDecimalOr(reader, new[] { "subtotal", "subtotal_all" }, 0m);
				summary.Subtotal_Physical = GetDecimalOr(reader, new[] { "subtotal_physical" }, 0m);
				summary.Discount = GetDecimalOr(reader, new[] { "discount", "discount_amount" }, 0m);

				summary.Shipping_Fee = GetDecimalOr(reader, new[] { "shipping_fee" }, 0m);
				summary.Grand_Total = GetDecimalOr(reader, new[] { "grand_total" }, 0m);
				summary.Total_Weight_G = GetDecimalOr(reader, new[] { "total_weight_g", "total_weight" }, 0m);

				// rule notes：支援 rule_notes_json（JSON 陣列字串）或 rule_notes（單行字串）
				var notesJson = GetStringOr(reader, new[] { "rule_notes_json" }, null);
				if (!string.IsNullOrWhiteSpace(notesJson))
					summary.Rule_Notes_Json = notesJson!;
				else
				{
					var notes = GetStringOr(reader, new[] { "rule_notes" }, null);
					if (!string.IsNullOrWhiteSpace(notes))
						summary.Rule_Notes_Json = JsonSerializer.Serialize(new[] { notes });
					else
						summary.Rule_Notes_Json = "[]";
				}

				summary.Can_Checkout = GetBoolOr(reader, new[] { "can_checkout" }, true);
				summary.Block_Reason = GetStringOr(reader, new[] { "block_reason" }, null);
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
			=> TryGetOrdinal(r, name, out var i) ? r.GetInt32(i) : 0;

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
			=> TryGetOrdinal(r, name, out var i) ? r.GetDecimal(i) : 0m;

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
