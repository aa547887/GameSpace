// Areas/OnlineStore/Controllers/EcpayController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;                 // EF Core Async
using Microsoft.Data.SqlClient;                     // SqlParameter
using GamiPort.Models;                              // GameSpacedatabaseContext
using System.Data;   // 為了 SqlDbType.NVarChar, Size=-1


namespace GamiPort.Areas.OnlineStore.Controllers
{
	[Area("OnlineStore")]
	[Route("Ecpay")]
	public sealed class EcpayController : Controller
	{
		private readonly ILogger<EcpayController> _logger;
		private readonly GameSpacedatabaseContext _db;

		public EcpayController(ILogger<EcpayController> logger, GameSpacedatabaseContext db)
		{
			_logger = logger;
			_db = db;
		}

		/* -------------------------------------------------------
		 *  1) Return：前台回傳（GET/POST 都吃）
		 *     DEV：為了示範方便，也會標記付款與寫交易
		 *     PROD：只導頁，不改狀態（以 Notify 為準）
		 * -----------------------------------------------------*/

		[HttpPost("Return")]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> FrontReturnPost([FromForm] Dictionary<string, string> form)
			=> await HandleFrontReturnCoreAsync(form, "POST");

		[HttpGet("Return")]
		public async Task<IActionResult> FrontReturnGet()
		{
			var q = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
			return await HandleFrontReturnCoreAsync(q, "GET");
		}

		private async Task<IActionResult> HandleFrontReturnCoreAsync(Dictionary<string, string> data, string via)
		{
			_logger.LogInformation("[ECPay Return/{Via}] {raw}", via, Dump(data));

			// POST 驗簽；GET 直接放行
			if (string.Equals(via, "POST", StringComparison.OrdinalIgnoreCase))
				if (!Verify(data)) return Content("0|CheckMacValue Fail");

			var fallbackOid = Request.Query.ContainsKey("oid")
				? int.Parse(Request.Query["oid"])
				: (int?)null;

#if DEBUG
			// DEV：Return 也嘗試標記付款、記帳
			return await MarkPaidAndGotoSuccessAsync(data, fallbackOid, phase: "Return");
#else
			// PROD：Return 僅導頁，不做狀態更新
			var orderCode = data.GetValueOrDefault("MerchantTradeNo");
			return RedirectToAction("Success", "Checkout",
				new { area = "OnlineStore", orderCode });
#endif
		}

		/* -------------------------------------------------------
		 *  2) OrderResult：橘色測試鍵（GET/POST 都吃）
		 * -----------------------------------------------------*/

		[HttpPost("OrderResult")]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> OrderResultPost([FromForm] Dictionary<string, string> form)
			=> await HandleOrderResultCoreAsync(form, "POST");

		[HttpGet("OrderResult")]
		public async Task<IActionResult> OrderResultGet()
		{
			var q = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
			return await HandleOrderResultCoreAsync(q, "GET");
		}

		private async Task<IActionResult> HandleOrderResultCoreAsync(Dictionary<string, string> data, string via)
		{
			_logger.LogInformation("[ECPay OrderResult/{Via}] {raw}", via, Dump(data));

			if (string.Equals(via, "POST", StringComparison.OrdinalIgnoreCase))
				if (!Verify(data)) return Content("0|CheckMacValue Fail");

			var fallbackOid = Request.Query.ContainsKey("oid")
				? int.Parse(Request.Query["oid"])
				: (int?)null;

#if DEBUG
			return await MarkPaidAndGotoSuccessAsync(data, fallbackOid, phase: "OrderResult");
#else
			var orderCode = data.GetValueOrDefault("MerchantTradeNo");
			return RedirectToAction("Success", "Checkout",
				new { area = "OnlineStore", orderCode });
#endif
		}

		/* -------------------------------------------------------
		 *  3) Notify：伺服器回呼（正式唯一可信來源）
		 *     冪等：以 provider='ECP' + provider_txn(TradeNo) Upsert
		 * -----------------------------------------------------*/

		[HttpPost("Notify")]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> ServerNotify([FromForm] Dictionary<string, string> form)
		{
			_logger.LogInformation("[ECPay Notify] {raw}", Dump(form));
			if (!Verify(form)) return Content("0|CheckMacValue Fail");

			var rtnCode = form.GetValueOrDefault("RtnCode");            // "1" 成功
			var tradeAmtStr = form.GetValueOrDefault("TradeAmt");           // 金額(整數)
			var merchantTradeNo = form.GetValueOrDefault("MerchantTradeNo");    // 你的 order_code
			var tradeNo = form.GetValueOrDefault("TradeNo");            // 綠界交易序號
			var payDate = form.GetValueOrDefault("PaymentDate");        // yyyy/MM/dd HH:mm:ss
			var rawJson = Dump(form);

			if (rtnCode != "1") return Content("1|OK");

			var order = await _db.SoOrderInfoes.FirstOrDefaultAsync(o => o.OrderCode == merchantTradeNo);
			if (order == null) return Content("1|OK");

			// 金額比對（以你表的 GrandTotal 為準）
			if (!int.TryParse(tradeAmtStr, out var amt)) return Content("1|OK");
			var expectAmt = (int)Math.Round(order.GrandTotal ?? 0m);
			if (amt != expectAmt) return Content("1|OK");

			// Upsert 付款交易（冪等）
			var confirmedAt = TryParseTwTime(payDate);
			var payCode = await UpsertPaymentTxnAsync(
				orderId: order.OrderId,
				amount: order.GrandTotal ?? 0m,
				providerTxn: tradeNo,
				status: "SUCCESS",
				confirmedAt: confirmedAt,
				note: "ECPay Notify",
				raw: rawJson
			);

			// 稽核（每次都記）
			await AppendPaymentAuditAsync(payCode, tradeNo, order.OrderId,
				phase: "Notify", action: "Confirm", result: "SUCCESS",
				message: "ECPay 伺服器回呼成功", raw: rawJson);

			// 只留歷史紀錄（不變更狀態值，避免 CHECK）
			await AppendOrderStatusHistoryAsync(order.OrderId,
				fromStatus: order.OrderStatus ?? "未出貨",
				toStatus: order.OrderStatus ?? "未出貨",
				note: "付款完成（狀態不變，僅留痕）");

			// 更新主檔（冪等）
			order.PaymentStatus = "已付款";
			if (confirmedAt.HasValue) order.PaymentAt = confirmedAt.Value;
			await _db.SaveChangesAsync();

			return Content("1|OK");
		}

		/* -------------------------------------------------------
		 *  Shared：DEV 便利用，回 Success 之前幫你把單子寫好
		 * -----------------------------------------------------*/

		private async Task<IActionResult> MarkPaidAndGotoSuccessAsync(
			Dictionary<string, string> data, int? fallbackOid, string phase)
		{
			var rtnCode = data.GetValueOrDefault("RtnCode");
			var tradeAmtStr = data.GetValueOrDefault("TradeAmt");
			var merchantTradeNo = data.GetValueOrDefault("MerchantTradeNo");
			var tradeNo = data.GetValueOrDefault("TradeNo");
			var payDate = data.GetValueOrDefault("PaymentDate");
			var rawJson = Dump(data);

			// 找 order_id：優先用我們 querystring 的 oid，沒有就用 order_code 兜回
			var id = fallbackOid ?? 0;
			if (id <= 0 && !string.IsNullOrEmpty(merchantTradeNo))
			{
				id = await _db.SoOrderInfoes
					.Where(o => o.OrderCode == merchantTradeNo)
					.Select(o => o.OrderId)
					.FirstOrDefaultAsync();
			}
			if (id <= 0)
				return RedirectToAction("Step1", "Checkout", new { area = "OnlineStore" });

#if DEBUG
			if (rtnCode == "1" && int.TryParse(tradeAmtStr, out var amt))
			{
				var order = await _db.SoOrderInfoes.FirstOrDefaultAsync(o => o.OrderId == id);
				if (order != null)
				{
					var expect = (int)Math.Round(order.GrandTotal ?? 0m);
					if (expect == amt)
					{
						var confirmedAt = TryParseTwTime(payDate);

						// Upsert（同 TradeNo 重送會落在 UPDATE）
						var payCode = await UpsertPaymentTxnAsync(
							orderId: order.OrderId,
							amount: order.GrandTotal ?? 0m,
							providerTxn: tradeNo,
							status: "SUCCESS",
							confirmedAt: confirmedAt,
							note: $"ECPay {phase}",
							raw: rawJson
						);

						await AppendPaymentAuditAsync(payCode, tradeNo, order.OrderId,
							phase: phase, action: "Confirm", result: "SUCCESS",
							message: $"本機 {phase} 標記已付款", raw: rawJson);

						await AppendOrderStatusHistoryAsync(order.OrderId,
							fromStatus: order.OrderStatus ?? "未出貨",
							toStatus: order.OrderStatus ?? "未出貨",
							note: "付款完成（狀態不變，僅留痕）");

						order.PaymentStatus = "已付款";
						if (confirmedAt.HasValue) order.PaymentAt = confirmedAt.Value;
						await _db.SaveChangesAsync();
					}
				}
			}
#endif
			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", id });
		}

		/* -------------------------------------------------------
		 *  Core helpers
		 * -----------------------------------------------------*/

		private static DateTime? TryParseTwTime(string? s)
			=> DateTime.TryParse(s, out var dt) ? dt : (DateTime?)null;

		// 產生 14 碼：PT + yyyyMMddHHmm
		private static string NewPayCode(DateTime? now = null)
		{
			var t = now ?? DateTime.Now;
			return "PT" + t.ToString("yyyyMMddHHmm"); // 2 + 12 = 14
		}

		/// <summary>
		/// 以 provider='ECP' + provider_txn 做冪等 Upsert。
		/// - 若已存在：UPDATE status/confirmed_at/note/raw，不更換 payment_code
		/// - 若不存在：INSERT 並產生 14 碼 payment_code
		/// 回傳：該交易的 payment_code（舊的或新的）
		/// </summary>
		private async Task<string> UpsertPaymentTxnAsync(
			int orderId, decimal amount, string? providerTxn, string status,
			DateTime? confirmedAt, string note, string raw)
		{
			// 1) 先查是否已有這個 provider_txn（與唯一索引一致）
			var existedCode = await _db.SoPaymentTransactions
				.Where(p => p.Provider == "ECP" && p.ProviderTxn == providerTxn)
				.Select(p => p.PaymentCode)
				.FirstOrDefaultAsync();

			if (!string.IsNullOrEmpty(existedCode))
			{
				// 2) 已存在 → 做 UPDATE（冪等）
				var sqlUpd = @"
UPDATE dbo.SO_PaymentTransactions
   SET status       = LEFT(@status,30),
       confirmed_at = @cat,
       note         = LEFT(@note,200),
       raw_payload  = @raw
 WHERE provider     = N'ECP'
   AND provider_txn = LEFT(@ptx,100);";

				await _db.Database.ExecuteSqlRawAsync(sqlUpd, new[]
				{
	new SqlParameter("@status", status ?? "SUCCESS"),
	new SqlParameter("@cat",    (object?)confirmedAt ?? DBNull.Value),
	new SqlParameter("@note",   note ?? string.Empty),
    // 指定 NVARCHAR(MAX)；Size = -1 代表 MAX
    new SqlParameter("@raw",    (object?)raw ?? DBNull.Value) { SqlDbType = SqlDbType.NVarChar, Size = -1 },
	new SqlParameter("@ptx",    (object?)providerTxn ?? DBNull.Value),
});

				return existedCode!;
			}
			else
			{
				// 3) 不存在 → INSERT（第一次寫入）
				var code = NewPayCode();
				var sqlIns = @"
INSERT INTO dbo.SO_PaymentTransactions
  (payment_code, order_id, txn_type, amount, provider, provider_txn, status, created_at, confirmed_at, note, meta, raw_payload)
VALUES
  (LEFT(@code,14), @oid, N'PAY', @amt, N'ECP', LEFT(@ptx,100), LEFT(@st,30), SYSUTCDATETIME(), @cat, LEFT(@note,200), NULL, @raw);";

				try
				{
					await _db.Database.ExecuteSqlRawAsync(sqlIns, new[]
					{
						new SqlParameter("@code", code),
						new SqlParameter("@oid",  orderId),
						new SqlParameter("@amt",  amount),
						new SqlParameter("@ptx",  (object?)providerTxn ?? DBNull.Value),
						new SqlParameter("@st",   status ?? "SUCCESS"),
						new SqlParameter("@cat",  (object?)confirmedAt ?? DBNull.Value),
						new SqlParameter("@note", note ?? ""),
						new SqlParameter("@raw",  raw ?? ""),
					});
					return code;
				}
				catch (DbUpdateException ex) when (
					ex.InnerException?.Message?.Contains("payment_code", StringComparison.OrdinalIgnoreCase) == true)
				{
					// 極少數同分鐘撞碼 → 尾端加兩碼退避一次
					var fallback = (code + new Random().Next(10, 99).ToString()).Substring(0, 14);
					await _db.Database.ExecuteSqlRawAsync(sqlIns, new[]
					{
						new SqlParameter("@code", fallback),
						new SqlParameter("@oid",  orderId),
						new SqlParameter("@amt",  amount),
						new SqlParameter("@ptx",  (object?)providerTxn ?? DBNull.Value),
						new SqlParameter("@st",   status ?? "SUCCESS"),
						new SqlParameter("@cat",  (object?)confirmedAt ?? DBNull.Value),
						new SqlParameter("@note", note ?? ""),
						new SqlParameter("@raw",  raw ?? ""),
					});
					return fallback;
				}
			}
		}

		// 插入 SO_PaymentAudit（每次都寫入一行稽核）
		private async Task AppendPaymentAuditAsync(
			string paymentCode, string? providerTxn, int orderId,
			string phase, string action, string result, string message, string raw)
		{
			var sql = @"
INSERT INTO dbo.SO_PaymentAudit
    (happened_at, payment_code, provider_txn, order_id, phase, action, result, message)
VALUES
    (SYSUTCDATETIME(), LEFT(@code,14), LEFT(@ptxn,100), @oid, LEFT(@phase,30), LEFT(@action,30), LEFT(@result,30), LEFT(@msg,1000));";

			// 為避免在 C# 端組過長字串，直接把拼好的字串交給 SQL 的 LEFT 截斷
			var msg = string.IsNullOrWhiteSpace(message) ? raw ?? "" : $"{message} | {raw}";
			await _db.Database.ExecuteSqlRawAsync(sql, new[]
			{
				new SqlParameter("@code",  paymentCode),
				new SqlParameter("@ptxn",  (object?)providerTxn ?? DBNull.Value),
				new SqlParameter("@oid",   orderId),
				new SqlParameter("@phase", phase ?? ""),
				new SqlParameter("@action",action ?? ""),
				new SqlParameter("@result",result ?? ""),
				new SqlParameter("@msg",   msg ?? "")
			});
		}

		// 插入 SO_OrderStatusHistory（若 from == to 則跳過，避免觸發 CHECK）
		private async Task AppendOrderStatusHistoryAsync(
			int orderId, string? fromStatus, string? toStatus, string note)
		{
			// 讀目前訂單狀態，補齊空值
			var curr = await _db.SoOrderInfoes
				.Where(o => o.OrderId == orderId)
				.Select(o => o.OrderStatus)
				.FirstOrDefaultAsync();

			var from = string.IsNullOrWhiteSpace(fromStatus) ? (curr ?? "未出貨") : fromStatus;
			var to = string.IsNullOrWhiteSpace(toStatus) ? (curr ?? "未出貨") : toStatus;

			// ★ 關鍵：相同就不寫入（避免違反 CK_SO_OrderStatusHistory_FromTo_Different）
			if (string.Equals(from, to, StringComparison.Ordinal))
				return;

			var sql = @"
INSERT INTO dbo.SO_OrderStatusHistory
    (order_id, from_status, to_status, changed_by, changed_at, note)
VALUES
    (@oid, LEFT(@from,30), LEFT(@to,30), NULL, @t, LEFT(@note,200));";

			await _db.Database.ExecuteSqlRawAsync(sql, new[]
			{
		new Microsoft.Data.SqlClient.SqlParameter("@oid", orderId),
		new Microsoft.Data.SqlClient.SqlParameter("@from", from),
		new Microsoft.Data.SqlClient.SqlParameter("@to",   to),
		new Microsoft.Data.SqlClient.SqlParameter("@t",    DateTime.UtcNow),
		new Microsoft.Data.SqlClient.SqlParameter("@note", note ?? string.Empty),
	});
		}


		/* -------------------------------------------------------
		 *  Utils
		 * -----------------------------------------------------*/

		private static string Dump(Dictionary<string, string> form)
			=> string.Join(", ", form.Select(kv => $"{kv.Key}={kv.Value}"));

		// 綠界檢核
		private static bool Verify(Dictionary<string, string> form)
		{
			var recv = form.GetValueOrDefault("CheckMacValue")?.ToUpperInvariant();
			if (string.IsNullOrEmpty(recv)) return false;

			var clone = form
				.Where(kv => !string.Equals(kv.Key, "CheckMacValue", StringComparison.OrdinalIgnoreCase))
				.ToDictionary(kv => kv.Key, kv => kv.Value);

			var local = GamiPort.Areas.OnlineStore.Payments.EcpayPaymentService.MakeCheckMacValue(
				clone, key: "5294y06JbISpM5x9", iv: "v77hoKGq4kWxNNIS");

			return string.Equals(local, recv, StringComparison.OrdinalIgnoreCase);
		}
	}
}
