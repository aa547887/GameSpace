// Areas/OnlineStore/Controllers/EcpayController.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GamiPort.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

			var mt = form.GetValueOrDefault("MerchantTradeNo");
			var orderCode = ExtractOrderCode(mt);
			if (string.IsNullOrWhiteSpace(orderCode))
				return RedirectToAction("OnePage", "Checkout", new { area = "OnlineStore" });

			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", orderCode });
		}

		[HttpGet("Return")]
		public IActionResult ReturnGet()
		{
			var mt = Request.Query["MerchantTradeNo"].ToString();
			var orderCode = ExtractOrderCode(mt);
			_logger.LogInformation("[ECPay Return/GET] MerchantTradeNo={code} -> orderCode={oc}", mt, orderCode);

			if (string.IsNullOrWhiteSpace(orderCode))
				return RedirectToAction("OnePage", "Checkout", new { area = "OnlineStore" });

			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", orderCode });
		}

		[HttpPost("OrderResult")]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> OrderResultPost([FromForm] Dictionary<string, string> form)
		{
			_logger.LogInformation("[ECPay OrderResult/POST] {raw}", Dump(form));
			try
			{
				if (Verify(form)) await TryMarkPaidAsync(form);
				else _logger.LogWarning("[ECPay OrderResult/POST] CheckMacValue verify failed.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[ECPay OrderResult/POST] TryMarkPaidAsync error");
			}

			var mt = form.GetValueOrDefault("MerchantTradeNo");
			var orderCode = ExtractOrderCode(mt);
			if (string.IsNullOrWhiteSpace(orderCode))
				return RedirectToAction("OnePage", "Checkout", new { area = "OnlineStore" });

			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", orderCode });
		}

		[HttpGet("OrderResult")]
		public IActionResult OrderResultGet()
		{
			var mt = Request.Query["MerchantTradeNo"].ToString();
			var orderCode = ExtractOrderCode(mt);
			_logger.LogInformation("[ECPay OrderResult/GET] MerchantTradeNo={code} -> orderCode={oc}", mt, orderCode);

			if (string.IsNullOrWhiteSpace(orderCode))
				return RedirectToAction("OnePage", "Checkout", new { area = "OnlineStore" });

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
				var mt = data.GetValueOrDefault("MerchantTradeNo");
				var orderCode = ExtractOrderCode(mt);
				var payDateStr = data.GetValueOrDefault("PaymentDate");

				if (rtnCode != "1" || string.IsNullOrWhiteSpace(orderCode)) return false;

				var order = await _db.SoOrderInfoes.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
				if (order == null) return false;

				if (!int.TryParse(tradeAmtStr, out var paidAmt)) return false;

				// ★ 非 nullable decimal：直接計算折抵後金額
				var expect = order.Subtotal + order.ShippingFee - order.DiscountTotal;
				var expectAmt = (int)Math.Round(expect, MidpointRounding.AwayFromZero);

				if (paidAmt != expectAmt) return false;

				if (!DateTime.TryParse(payDateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
					dt = DateTime.Now;

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
				clone, key: "pwFHCqoQZGmho4w6", iv: "EkRm7iFT261dpevs"); // 3002607

			return string.Equals(local, recv, StringComparison.OrdinalIgnoreCase);
		}

		private static string ExtractOrderCode(string? merchantTradeNo)
		{
			if (string.IsNullOrWhiteSpace(merchantTradeNo)) return "";
			var s = merchantTradeNo.Trim();
			if (s.Length >= 4 && (s[0] == 'D' || s[0] == 'P')) return s.Substring(1, s.Length - 1 - 2);
			return s;
		}
	}
}
