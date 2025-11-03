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

		[HttpPost("Return")]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> ReturnPost([FromForm] Dictionary<string, string> form)
		{
			_logger.LogInformation("[ECPay Return/POST] {raw}", Dump(form));
			if (!Verify(form)) return Content("0|CheckMacValue Fail");
			await TryMarkPaidAsync(form);
			var orderCode = form.GetValueOrDefault("MerchantTradeNo");
			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", orderCode });
		}

		[HttpGet("Return")]
		public IActionResult ReturnGet()
		{
			var orderCode = Request.Query["MerchantTradeNo"].ToString();
			_logger.LogInformation("[ECPay Return/GET] MerchantTradeNo={code}", orderCode);
			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", orderCode });
		}

		// Areas/OnlineStore/Controllers/EcpayController.cs
		[HttpPost("OrderResult")]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> OrderResultPost([FromForm] Dictionary<string, string> form)
		{
			_logger.LogInformation("[ECPay OrderResult/POST] {raw}", Dump(form));

			// ★ 本機測試很常只收到這支，因此同樣做驗簽＋嘗試入庫（與 Return/Notify 一致）
			try
			{
				if (Verify(form))
				{
					// 會檢查 RtnCode==1、金額一致、時間解析，並把 PaymentStatus/PaymentAt 寫入
					await TryMarkPaidAsync(form);
				}
				else
				{
					_logger.LogWarning("[ECPay OrderResult/POST] CheckMacValue verify failed.");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[ECPay OrderResult/POST] TryMarkPaidAsync error");
				// 不擋前端導回，但會保留未付款狀態；使用者可以在成功頁按「重新付款」
			}

			var orderCode = form.GetValueOrDefault("MerchantTradeNo");
			if (string.IsNullOrWhiteSpace(orderCode))
				return RedirectToAction("OnePage", "Checkout", new { area = "OnlineStore" });

			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", orderCode });
		}

		[HttpGet("OrderResult")]
		public IActionResult OrderResultGet()
		{
			var orderCode = Request.Query["MerchantTradeNo"].ToString();
			_logger.LogInformation("[ECPay OrderResult/GET] MerchantTradeNo={code}", orderCode);
			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", orderCode });
		}

		[HttpPost("Notify")]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> ServerNotify([FromForm] Dictionary<string, string> form)
		{
			_logger.LogInformation("[ECPay Notify] {raw}", Dump(form));
			if (!Verify(form)) return Content("0|CheckMacValue Fail");
			await TryMarkPaidAsync(form);
			return Content("1|OK");
		}

		private async Task<bool> TryMarkPaidAsync(Dictionary<string, string> data)
		{
			try
			{
				var rtnCode = data.GetValueOrDefault("RtnCode");
				var tradeAmtStr = data.GetValueOrDefault("TradeAmt");
				var merchantTradeNo = data.GetValueOrDefault("MerchantTradeNo");
				var payDateStr = data.GetValueOrDefault("PaymentDate");

				if (rtnCode != "1" || string.IsNullOrWhiteSpace(merchantTradeNo))
					return false;

				var order = await _db.SoOrderInfoes.FirstOrDefaultAsync(o => o.OrderCode == merchantTradeNo);
				if (order == null) return false;

				if (!int.TryParse(tradeAmtStr, out var paidAmt)) return false;
				var expectAmt = (int)Math.Round(order.GrandTotal ?? 0m);
				if (paidAmt != expectAmt) return false;

				if (!DateTime.TryParse(payDateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
					dt = DateTime.UtcNow;

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

		private static string Dump(Dictionary<string, string> form)
			=> string.Join(", ", form.Select(kv => $"{kv.Key}={kv.Value}"));

		private static bool Verify(Dictionary<string, string> form)
		{
			var recv = form.GetValueOrDefault("CheckMacValue")?.ToUpperInvariant();
			if (string.IsNullOrEmpty(recv)) return false;

			var clone = form.Where(kv => !string.Equals(kv.Key, "CheckMacValue", StringComparison.OrdinalIgnoreCase))
							.ToDictionary(kv => kv.Key, kv => kv.Value);

			var local = GamiPort.Areas.OnlineStore.Payments.EcpayPaymentService.MakeCheckMacValue(
				clone, key: "pwFHCqoQZGmho4w6", iv: "EkRm7iFT261dpevs"); // 3002607 金鑰

			return string.Equals(local, recv, StringComparison.OrdinalIgnoreCase);
		}
	}
}
