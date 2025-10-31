// Areas/OnlineStore/Controllers/EcpayController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using GamiPort.Models;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace GamiPort.Areas.OnlineStore.Controllers
{
	[Area("OnlineStore")]
	[Route("Ecpay")]
	public sealed class EcpayController : Controller
	{
		private readonly ILogger<EcpayController> _logger;
		private readonly GameSpacedatabaseContext _db;
		private readonly IConfiguration _cfg;

		public EcpayController(ILogger<EcpayController> logger, GameSpacedatabaseContext db, IConfiguration cfg)
		{
			_logger = logger;
			_db = db;
			_cfg = cfg;
		}

		// =============== 開發端點（只改主檔；不寫交易/稽核/歷史） ===============
		private bool DevEnabled() => _cfg.GetValue<bool>("PaymentDev:EnableSkipFlow");
		private bool DevSecretOk(string? s) => !string.IsNullOrWhiteSpace(s) && string.Equals(s, _cfg["PaymentDev:Secret"], StringComparison.Ordinal);

		[HttpPost("Dev/PaySuccess")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DevPaySuccess([FromForm] string orderCode, [FromForm] string secret)
		{
			if (!DevEnabled()) return Forbid();
			if (!DevSecretOk(secret)) return Unauthorized();
			if (string.IsNullOrWhiteSpace(orderCode) || orderCode.Length != 14 || !orderCode.StartsWith("OR")) return BadRequest("orderCode 格式不正確");

			var order = await _db.SoOrderInfoes.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
			if (order == null) return BadRequest("orderCode 找不到訂單");

			order.PaymentStatus = "已付款";
			order.PaymentAt = DateTime.UtcNow;
			await _db.SaveChangesAsync();

			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", orderCode });
		}

		[HttpPost("Dev/PayFail")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DevPayFail([FromForm] string orderCode, [FromForm] string secret)
		{
			if (!DevEnabled()) return Forbid();
			if (!DevSecretOk(secret)) return Unauthorized();
			if (string.IsNullOrWhiteSpace(orderCode) || orderCode.Length != 14 || !orderCode.StartsWith("OR")) return BadRequest("orderCode 格式不正確");

			// 失敗：不改主檔，只導回 Review
			return RedirectToAction("Review", "Checkout", new { area = "OnlineStore", orderCode });
		}

		[HttpPost("Dev/Refund")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DevRefund([FromForm] string orderCode, [FromForm] string secret)
		{
			if (!DevEnabled()) return Forbid();
			if (!DevSecretOk(secret)) return Unauthorized();
			if (string.IsNullOrWhiteSpace(orderCode) || orderCode.Length != 14 || !orderCode.StartsWith("OR")) return BadRequest("orderCode 格式不正確");

			var order = await _db.SoOrderInfoes.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
			if (order == null) return BadRequest("orderCode 找不到訂單");

			order.PaymentStatus = "已退款";
			await _db.SaveChangesAsync();

			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", orderCode });
		}

		// =================== 真實金流（Return/OrderResult 只導頁） ===================
		[HttpPost("Return")]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> ReturnPost([FromForm] Dictionary<string, string> form)
		{
			_logger.LogInformation("[ECPay Return/POST] {raw}", Dump(form));
			if (!Verify(form)) return Content("0|CheckMacValue Fail");

			// ★ 本機 Demo 關鍵：在 Return POST（3D 驗證完成）就直接入帳
			await TryMarkPaidAsync(form);

			var orderCode = form.GetValueOrDefault("MerchantTradeNo");
			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", orderCode });
		}

		[HttpGet("Return")]
		public IActionResult ReturnGet()
		{
			// GET 通常無法驗簽，僅作導頁
			var orderCode = Request.Query["MerchantTradeNo"].ToString();
			_logger.LogInformation("[ECPay Return/GET] MerchantTradeNo={code}", orderCode);
			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", orderCode });
		}

		private async Task<IActionResult> HandleReturnCoreAsync(Dictionary<string, string> data, string via)
		{
			_logger.LogInformation("[ECPay Return/{Via}] {raw}", via, Dump(data));
			if (string.Equals(via, "POST", StringComparison.OrdinalIgnoreCase))
				if (!Verify(data)) return Content("0|CheckMacValue Fail");

			var orderCode = data.GetValueOrDefault("MerchantTradeNo");
			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", orderCode });
		}

		[HttpPost("OrderResult")]
		[IgnoreAntiforgeryToken]
		public IActionResult OrderResultPost([FromForm] Dictionary<string, string> form)
		{
			_logger.LogInformation("[ECPay OrderResult/POST] {raw}", Dump(form));
			// 不重複落帳：讓 Return(POST)/Notify 處理
			var orderCode = form.GetValueOrDefault("MerchantTradeNo");
			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", orderCode });
		}

		[HttpGet("OrderResult")]
		public IActionResult OrderResultGet()
		{
			var orderCode = Request.Query["MerchantTradeNo"].ToString();
			_logger.LogInformation("[ECPay OrderResult/GET] MerchantTradeNo={code}", orderCode);
			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", orderCode });
		}


		private async Task<IActionResult> HandleOrderResultCoreAsync(Dictionary<string, string> data, string via)
		{
			_logger.LogInformation("[ECPay OrderResult/{Via}] {raw}", via, Dump(data));
			if (string.Equals(via, "POST", StringComparison.OrdinalIgnoreCase))
				if (!Verify(data)) return Content("0|CheckMacValue Fail");

			var orderCode = data.GetValueOrDefault("MerchantTradeNo");
			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", orderCode });
		}

		// =================== 真實落帳：Notify（只改主檔） ===================
		[HttpPost("Notify")]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> ServerNotify([FromForm] Dictionary<string, string> form)
		{
			_logger.LogInformation("[ECPay Notify] {raw}", Dump(form));
			if (!Verify(form)) return Content("0|CheckMacValue Fail");

			await TryMarkPaidAsync(form);
			return Content("1|OK");
		}

		// ============== 共用：把訂單標記為已付款（含金額/時間檢查） ==============
		private async Task<bool> TryMarkPaidAsync(Dictionary<string, string> data)
		{
			try
			{
				// 必要欄位
				var rtnCode = data.GetValueOrDefault("RtnCode");
				var tradeAmtStr = data.GetValueOrDefault("TradeAmt");
				var merchantTradeNo = data.GetValueOrDefault("MerchantTradeNo");
				var payDateStr = data.GetValueOrDefault("PaymentDate"); // 可能如 "2025/10/29 14:30:00"

				if (rtnCode != "1" || string.IsNullOrWhiteSpace(merchantTradeNo))
					return false;

				var order = await _db.SoOrderInfoes.FirstOrDefaultAsync(o => o.OrderCode == merchantTradeNo);
				if (order == null) return false;

				if (!int.TryParse(tradeAmtStr, out var paidAmt)) return false;
				var expectAmt = (int)Math.Round(order.GrandTotal ?? 0m);
				if (paidAmt != expectAmt) return false;

				// 解析 PaymentDate（允許幾種常見格式）
				DateTime dt;
				if (!DateTime.TryParse(payDateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
				{
					// 萬一沒有時間也不擋，改記 UTC-Now
					dt = DateTime.UtcNow;
				}

				// ★ 正式寫入
				order.PaymentStatus = "已付款";
				order.PaymentAt = dt.ToUniversalTime();
				await _db.SaveChangesAsync();

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "TryMarkPaidAsync error");
				return false;
			}
		}

		// Utils（保持你原本的 Verify，但注意用 3002607 的 Key/IV）
		private static string Dump(Dictionary<string, string> form)
			=> string.Join(", ", form.Select(kv => $"{kv.Key}={kv.Value}"));

		private static bool Verify(Dictionary<string, string> form)
		{
			var recv = form.GetValueOrDefault("CheckMacValue")?.ToUpperInvariant();
			if (string.IsNullOrEmpty(recv)) return false;

			var clone = form.Where(kv => !string.Equals(kv.Key, "CheckMacValue", StringComparison.OrdinalIgnoreCase))
							.ToDictionary(kv => kv.Key, kv => kv.Value);

			var local = GamiPort.Areas.OnlineStore.Payments.EcpayPaymentService.MakeCheckMacValue(
				clone, key: "pwFHCqoQZGmho4w6", iv: "EkRm7iFT261dpevs"); // ★ 3002607 的金鑰

			return string.Equals(local, recv, StringComparison.OrdinalIgnoreCase);
		}
	}
}
