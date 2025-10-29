using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GamiPort.Areas.OnlineStore.DTO;
using GamiPort.Models; // GameSpacedatabaseContext

namespace GamiPort.Areas.OnlineStore.Services
{
	/// <summary>
	/// 以 EF Core 由資料庫撈出下拉清單；僅作唯讀查詢，不做商業邏輯
	/// </summary>
	public sealed class SqlLookupService : ILookupService
	{
		private readonly GameSpacedatabaseContext _db;
		public SqlLookupService(GameSpacedatabaseContext db) => _db = db;

		// =========================
		// 配送方式（清單 + 存在性）
		// =========================
		public async Task<IReadOnlyList<ShipMethodDto>> GetShipMethodsAsync()
		{
			var rows = await _db.SoShipMethods
								.AsNoTracking()
								// .OrderBy(x => x.DisplayOrder) // 你的實體沒有此欄，移除
								.OrderBy(x => x.ShipMethodId)   // 以主鍵排序（或換成 .OrderBy(x => x.MethodName)）
								.Select(x => new ShipMethodDto
								{
									ShipMethodId = x.ShipMethodId,
									MethodName = x.MethodName
								})
								.ToListAsync();
			return rows;
		}

		/// <summary>
		/// 檢查指定配送方式是否存在（給伺端驗證用）
		/// </summary>
		public async Task<bool> ShipMethodExistsAsync(int shipMethodId)
			=> await _db.SoShipMethods
						.AsNoTracking()
						.AnyAsync(x => x.ShipMethodId == shipMethodId);

		// =========================
		// 付款方式（清單 + 存在性）
		// =========================
		public async Task<IReadOnlyList<PayMethodDto>> GetPayMethodsAsync()
		{
			var rows = await _db.SoPayMethods
				.AsNoTracking()
				.Where(x => x.IsEnabled)              // ← 只取啟用
				.OrderBy(x => x.SortOrder)            // ← 依排序欄位
				.Select(x => new PayMethodDto
				{
					PayMethodId = x.PayMethodId,
					MethodName = x.MethodName
				})
				.ToListAsync();
			return rows;
		}


		/// <summary>
		/// 檢查指定付款方式是否存在（給伺端驗證用）
		/// </summary>
		public async Task<bool> PayMethodExistsAsync(int payMethodId)
	=> await _db.SoPayMethods
		.AsNoTracking()
		.AnyAsync(x => x.PayMethodId == payMethodId && x.IsEnabled);
	}
}
