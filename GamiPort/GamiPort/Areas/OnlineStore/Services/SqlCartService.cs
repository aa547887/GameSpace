using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using GamiPort.Areas.OnlineStore.DTO;
using GamiPort.Models; // ← 你的 DbContext 在這個命名空間

namespace GamiPort.Areas.OnlineStore.Services
{
	public class SqlCartService : ICartService
	{
		private readonly GameSpacedatabaseContext _db;
		public SqlCartService(GameSpacedatabaseContext db) => _db = db;

		public async Task<Guid> EnsureCartIdAsync(int? userId, Guid? anonymousToken)
		{
			var outParam = new SqlParameter("@OutCartId", System.Data.SqlDbType.UniqueIdentifier)
			{ Direction = System.Data.ParameterDirection.Output };

			await _db.Database.ExecuteSqlRawAsync(
				"EXEC dbo.usp_Cart_Ensure @UserId={0}, @AnonymousToken={1}, @OutCartId=@OutCartId OUTPUT",
				userId, (object?)anonymousToken ?? DBNull.Value, outParam);

			return (Guid)outParam.Value;
		}

		public Task AddAsync(Guid cartId, int productId, int quantity) =>
			_db.Database.ExecuteSqlRawAsync(
				"EXEC dbo.usp_Cart_AddItem @CartId={0}, @ProductId={1}, @Quantity={2}",
				cartId, productId, quantity);

		public async Task<CartSummaryDto> GetAsync(Guid cartId)
		{
			// 第一個結果集：明細
			var items = await _db.Set<CartItemRow>()
				.FromSqlRaw("EXEC dbo.usp_Cart_Get @CartId={0}", cartId)
				.ToListAsync();

			// 彙總（EF 讀多結果集不方便，這裡用第二次查詢簡化）
			var sum = await _db.Set<CartSumRow>().FromSqlRaw(@"
                SELECT ISNULL(SUM(quantity),0) total_qty,
                       ISNULL(SUM(unit_price*quantity),0) subtotal
                FROM dbo.SO_CartItems WHERE cart_id = {0}", cartId).FirstAsync();

			var dtos = items.Select(x =>
				new CartItemDto(x.item_id, x.product_id, x.product_name, x.unit_price, x.quantity, x.line_subtotal)
			).ToList();

			return new CartSummaryDto(dtos, sum.total_qty, sum.subtotal);
		}

		public Task UpdateQtyAsync(Guid cartId, int productId, int qty) =>
			_db.Database.ExecuteSqlRawAsync(
				"EXEC dbo.usp_Cart_UpdateQty @CartId={0}, @ProductId={1}, @Quantity={2}",
				cartId, productId, qty);

		public Task RemoveAsync(Guid cartId, int productId) =>
			_db.Database.ExecuteSqlRawAsync(
				"EXEC dbo.usp_Cart_RemoveItem @CartId={0}, @ProductId={1}",
				cartId, productId);

		public Task ClearAsync(Guid cartId) =>
			_db.Database.ExecuteSqlRawAsync("EXEC dbo.usp_Cart_Clear @CartId={0}", cartId);
	}

	// ★ 這兩個是「結果集容器」（Keyless）
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
}
