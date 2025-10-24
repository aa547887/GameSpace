using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GamiPort.Models; // GameSpacedatabaseContext

namespace GamiPort.Areas.OnlineStore.Services
{
	public class SqlLookupService : ILookupService
	{
		private readonly GameSpacedatabaseContext _db;
		public SqlLookupService(GameSpacedatabaseContext db) => _db = db;

		public async Task<bool> ShipMethodExistsAsync(int shipMethodId)
		{
			// 目前你的 SoShipMethod 不含 IsEnabled → 只檢查存在性
			return await _db.SoShipMethods
				.AsNoTracking()
				.AnyAsync(m => m.ShipMethodId == shipMethodId);
			// 若未來加了 IsEnabled 欄位，再改成：
			// .AnyAsync(m => m.ShipMethodId == shipMethodId && m.IsEnabled);
		}

		public async Task<bool> PayMethodExistsAsync(int payMethodId)
		{
			// SO_PayMethods 我們剛建立時就有 is_enabled → 這裡保留
			return await _db.SoPayMethods
				.AsNoTracking()
				.AnyAsync(p => p.PayMethodId == payMethodId && p.IsEnabled);
		}
	}
}
