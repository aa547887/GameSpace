// Areas/OnlineStore/Services/SqlLookupService.cs
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;    // ★ 新增
using GamiPort.Areas.OnlineStore.DTO;
using GamiPort.Models; // GameSpacedatabaseContext

namespace GamiPort.Areas.OnlineStore.Services
{
	/// 以 EF Core 撈下拉清單；若偵測不到連線字串，改走 ADO 備援，避免
	/// "The ConnectionString property has not been initialized." 例外。
	public sealed class SqlLookupService : ILookupService
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly string _conn; // ★ 備援連線字串

		public SqlLookupService(GameSpacedatabaseContext db, IConfiguration cfg)
		{
			_db = db;
			// 先向 EF 拿，目前 DI 正常時這裡會有值；否則退回 appsettings
			var fromEf = _db.Database.GetConnectionString();
			var fromCfg = cfg.GetConnectionString("GameSpace");
			_conn = string.IsNullOrWhiteSpace(fromEf) ? fromCfg : fromEf;
		}

		private bool UseAdoFallback => string.IsNullOrWhiteSpace(_db.Database.GetConnectionString());

		// =========================
		// 配送方式（清單 + 存在性）
		// =========================
		// 1) 取得配送方式清單（改用 ADO）
		// 1) 取得配送方式清單
		public async Task<IReadOnlyList<ShipMethodDto>> GetShipMethodsAsync()
		{
			var list = new List<ShipMethodDto>();
			await using var conn = new SqlConnection(_conn);
			await conn.OpenAsync();
			await using var cmd = conn.CreateCommand();
			cmd.CommandText = @"
SELECT 
    ship_method_id AS ShipMethodId,
    method_name   AS MethodName
FROM dbo.SO_ShipMethods
WHERE is_active = 1
ORDER BY ship_method_id";
			await using var rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
			while (await rdr.ReadAsync())
			{
				list.Add(new ShipMethodDto
				{
					ShipMethodId = rdr.GetInt32(0),
					MethodName = rdr.IsDBNull(1) ? "" : rdr.GetString(1)
				});
			}
			return list;
		}

		// 2) 驗證配送方式存在
		public async Task<bool> ShipMethodExistsAsync(int shipMethodId)
		{
			await using var conn = new SqlConnection(_conn);
			await conn.OpenAsync();
			await using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT COUNT(1) FROM dbo.SO_ShipMethods WHERE ship_method_id=@id AND is_active=1";
			cmd.Parameters.Add(new SqlParameter("@id", shipMethodId));
			var n = (int)await cmd.ExecuteScalarAsync();
			return n > 0;
		}

		// =========================
		// 付款方式（清單 + 存在性）
		// =========================
		// 3) 取得付款方式清單
		public async Task<IReadOnlyList<PayMethodDto>> GetPayMethodsAsync()
		{
			var list = new List<PayMethodDto>();
			await using var conn = new SqlConnection(_conn);
			await conn.OpenAsync();
			await using var cmd = conn.CreateCommand();
			cmd.CommandText = @"
SELECT
    pay_method_id AS PayMethodId,
    method_name   AS MethodName
FROM dbo.SO_PayMethods
WHERE is_enabled = 1
ORDER BY sort_order, pay_method_id";
			await using var rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
			while (await rdr.ReadAsync())
			{
				list.Add(new PayMethodDto
				{
					PayMethodId = rdr.GetInt32(0),
					MethodName = rdr.IsDBNull(1) ? "" : rdr.GetString(1)
				});
			}
			return list;
		}

		// 4) 驗證付款方式存在
		public async Task<bool> PayMethodExistsAsync(int payMethodId)
		{
			await using var conn = new SqlConnection(_conn);
			await conn.OpenAsync();
			await using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT COUNT(1) FROM dbo.SO_PayMethods WHERE pay_method_id=@id AND is_enabled=1";
			cmd.Parameters.Add(new SqlParameter("@id", payMethodId));
			var n = (int)await cmd.ExecuteScalarAsync();
			return n > 0;
		}
	}
}
