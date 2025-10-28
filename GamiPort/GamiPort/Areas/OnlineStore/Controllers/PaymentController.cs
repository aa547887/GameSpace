	using Microsoft.AspNetCore.Mvc;
	using Microsoft.EntityFrameworkCore;
	using Microsoft.Data.SqlClient;
	using System.Threading.Tasks;
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using GamiPort.Models;
	using GamiPort.Areas.OnlineStore.Payments;

	namespace GamiPort.Areas.OnlineStore.Controllers
	{
		[Area("OnlineStore")]
		public class PaymentController : Controller
		{
			private readonly GameSpacedatabaseContext _db;
			private readonly IPaymentService _pay;

			public PaymentController(GameSpacedatabaseContext db, IPaymentService pay)
			{
				_db = db;
				_pay = pay;
			}

			private static bool IsPaid(string? s)
			{
				if (string.IsNullOrWhiteSpace(s)) return false;
				var t = s.Trim();
				return t == "已付款"
					|| t.Equals("PAID", StringComparison.OrdinalIgnoreCase)
					|| t.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase);
			}

			/// <summary>
			/// Start：建立 INIT 交易 → 回傳自動送出的 ECPay 表單（POST 到 AIO）
			/// </summary>
			[HttpGet]
			public async Task<IActionResult> Start(int id)
			{
				// 1) 取單
				var order = await _db.SoOrderInfoes
					.AsNoTracking()
					.Where(o => o.OrderId == id)
					.Select(o => new {
						o.OrderId,
						o.OrderCode,
						o.GrandTotal,
						o.OrderTotal,
						o.PaymentStatus
					})
					.FirstOrDefaultAsync();

				if (order == null) return RedirectToAction("Step1", "Checkout", new { area = "OnlineStore" });
				if (IsPaid(order.PaymentStatus))
					return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", id });

				decimal amount = ((decimal?)order.GrandTotal) ?? ((decimal?)order.OrderTotal) ?? 0m;


				// 2) 建立一筆 INIT 交易（供日後 Notify 對應）
				var pId = new SqlParameter("@PaymentId", System.Data.SqlDbType.BigInt) { Direction = System.Data.ParameterDirection.Output };
				var pCode = new SqlParameter("@PaymentCode", System.Data.SqlDbType.NVarChar, 14) { Direction = System.Data.ParameterDirection.Output };

				await _db.Database.ExecuteSqlRawAsync(
					"EXEC dbo.usp_Payment_Create @OrderId={0}, @TxnType={1}, @Provider={2}, @Amount={3}, @PaymentId=@PaymentId OUTPUT, @PaymentCode=@PaymentCode OUTPUT",
					id, "PAY", "ECPAY", amount, pId, pCode);

				var paymentCode = (string?)pCode.Value ?? "";

				// 3) 組送往 ECPay 的表單欄位
				// itemName/tradeDesc 先用簡化字串；要列多品項可用 "#" 連接
				var fields = _pay.BuildStartFormFields(
					orderCode: order.OrderCode!,
					orderId: order.OrderId,
					totalAmount: amount,
					itemName: "GamiPort 商品",
					tradeDesc: "GamiPort 訂單付款");

				// 4) 回傳一個自動提交的 HTML 表單（不需要建立 View）
				var actionUrl = _pay.GetGatewayUrl();
				var inputs = string.Join("", fields.Select(kv =>
					$"<input type='hidden' name='{kv.Key}' value='{System.Net.WebUtility.HtmlEncode(kv.Value)}'/>"));

				var html = $@"
	<!doctype html>
	<html><head><meta charset='utf-8'><title>Redirecting...</title></head>
	<body onload='document.forms[0].submit();'>
	  <form method='post' action='{actionUrl}'>
		{inputs}
		<noscript><button type='submit'>前往付款</button></noscript>
	  </form>
	</body></html>";

				return Content(html, "text/html; charset=utf-8");
			}

			/// <summary>
			/// Return：使用者瀏覽器前端被帶回來的同步回傳（不可信）
			/// 這裡只做驗簽與顯示用，實際入帳仍以 Notify 為準。
			/// </summary>
			[HttpPost]
			[IgnoreAntiforgeryToken] // ECPay 不會帶你的 antiforgery
			[Route("/Gateway/Ecpay/Return")]
			public IActionResult EcpayReturn([FromForm] Dictionary<string, string> form)
			{
				// 1) 驗簽（驗失敗直接 400）
				if (!_pay.VerifyCheckMacValue(form)) return BadRequest("Invalid CheckMacValue");

				// 2) 帶回成功頁（僅顯示狀態，實際狀態以 DB 為準）
				//    ECPay 會把 MerchantTradeNo (=你的 order_code) 帶回來
				if (!form.TryGetValue("MerchantTradeNo", out var orderCode) || string.IsNullOrWhiteSpace(orderCode))
					return Content("Return OK (no order code). 您可回訂單頁查看狀態。");

				// 這裡通常需要把 orderCode -> orderId；簡化：請前端自動導回 /Checkout/Success?orderCode=...
				// 你的 Success 現在吃的是 id（int）。我們這邊改以 TempData 提示後導首頁或提示返回會員中心。
				TempData["Msg"] = "已接收第三方同步回傳，實際付款結果以系統內訂單狀態為準。";
				return Redirect("/"); // 若要精準導向，等你之後加上以 orderCode 查 id 的頁面再調整
			}

			/// <summary>
			/// Notify：ECPay 伺服器背景 POST（可信）。在這裡落地入帳。
			/// 規則：驗簽 → 金額一致 → 呼叫 usp_Payment_Reconfirm（冪等 by provider_txn）
			/// </summary>
			[HttpPost]
			[IgnoreAntiforgeryToken]
			[Route("/Gateway/Ecpay/Notify")]
			public async Task<IActionResult> EcpayNotify([FromForm] Dictionary<string, string> form)
			{
				// A) 驗簽
				if (!_pay.VerifyCheckMacValue(form))
					return BadRequest("Invalid CheckMacValue");

				// B) 抽取必要欄位
				form.TryGetValue("MerchantTradeNo", out var orderCode);         // 你的 ORxxxxxxxxxxxx
				form.TryGetValue("TradeAmt", out var amtStr);
				form.TryGetValue("RtnCode", out var rtnCode);                   // 1=成功
				form.TryGetValue("TradeNo", out var providerTxn);               // 綠界交易序號

				if (string.IsNullOrWhiteSpace(orderCode) || string.IsNullOrWhiteSpace(amtStr))
					return BadRequest("Missing fields");

				// C) 查出 order_id 與最新一筆 INIT 交易的 payment_code（或你要的對應策略）
				var orderRow = await _db.SoOrderInfoes
					.AsNoTracking()
					.Where(o => o.OrderCode == orderCode)
					.Select(o => new { o.OrderId, o.GrandTotal, o.OrderTotal, o.PaymentStatus })
					.FirstOrDefaultAsync();

				if (orderRow == null) return BadRequest("Order not found");

				decimal amountAssert = ((decimal?)orderRow.GrandTotal) ?? ((decimal?)orderRow.OrderTotal) ?? 0m;

				if (!decimal.TryParse(amtStr, out var amtFromEcpay))
					return BadRequest("Bad amount");

				// 金額不一致就拒絕（你也可選擇寫 Audit 後回 200）
				if (Math.Round(amountAssert, 0) != Math.Round(amtFromEcpay, 0))
					return BadRequest("Amount mismatch");

				// 取該單最新一筆 INIT 交易（PAY + INIT）
				var paymentCode = await _db.SoPaymentTransactions
					.AsNoTracking()
					.Where(t => t.OrderId == orderRow.OrderId && t.TxnType == "PAY" && t.Status == "INIT")
					.OrderByDescending(t => t.CreatedAt)
					.Select(t => t.PaymentCode)
					.FirstOrDefaultAsync();

				if (string.IsNullOrWhiteSpace(paymentCode))
				{
					// 若沒找到 INIT，可退而求其次找 SUCCESS/FAIL 最近一筆（讓 Reconfirm 能冪等收斂）
					paymentCode = await _db.SoPaymentTransactions
						.AsNoTracking()
						.Where(t => t.OrderId == orderRow.OrderId && t.TxnType == "PAY")
						.OrderByDescending(t => t.CreatedAt)
						.Select(t => t.PaymentCode)
						.FirstOrDefaultAsync();

					if (string.IsNullOrWhiteSpace(paymentCode))
						return BadRequest("No payment transaction to confirm");
				}

				// D) 判斷是否成功
				var isSuccess = string.Equals(rtnCode, "1", StringComparison.OrdinalIgnoreCase);

				// E) 呼叫你的 Reconfirm（冪等、狀態保護、券回補/重佔都在 SP）
				var sql = "EXEC dbo.usp_Payment_Reconfirm @PaymentCode={0}, @IsSuccess={1}, @ProviderTxn={2}, @AmountAssert={3}, @Reason={4}, @ConfirmedAt={5}, @AutoReconsumeCoupon={6}, @AllowFlipFromFail={7}, @DryRun={8}";
				await _db.Database.ExecuteSqlRawAsync(sql,
					paymentCode, isSuccess, (object?)providerTxn ?? DBNull.Value, amountAssert,
					"ECPay Notify", DateTime.UtcNow,
					1, 1, 0);

				// F) 回覆 1|OK（ECPay 需要這個文字才能停止重送）
				return Content("1|OK");
			}

			// ==============================
			// 以下保留你的 Mock (方便離線 demo)
			// ==============================

			[HttpGet]
			public async Task<IActionResult> MockPay(string code, int id)
			{
				var order = await _db.SoOrderInfoes.AsNoTracking()
					.Where(o => o.OrderId == id)
					.Select(o => new { o.OrderId, o.OrderCode, o.GrandTotal, o.PaymentStatus })
					.FirstOrDefaultAsync();

				if (order == null) return RedirectToAction("Step1", "Checkout");

				ViewBag.PaymentCode = code;
				return View(order);
			}

			[HttpPost]
			[ValidateAntiForgeryToken]
			public async Task<IActionResult> MockSuccess(string code, int id)
			{
				var providerTxn = $"MOCK-{code}";
				await _db.Database.ExecuteSqlRawAsync(
					"EXEC dbo.usp_Payment_Confirm @PaymentCode={0}, @ProviderTxn={1}, @IsSuccess={2}, @Note={3}",
					code, providerTxn, true, "內建模擬：成功");

				return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", id });
			}

			[HttpPost]
			[ValidateAntiForgeryToken]
			public async Task<IActionResult> MockFail(string code, int id)
			{
				var providerTxn = $"MOCK-{code}";
				await _db.Database.ExecuteSqlRawAsync(
					"EXEC dbo.usp_Payment_Confirm @PaymentCode={0}, @ProviderTxn={1}, @IsSuccess={2}, @Note={3}",
					code, providerTxn, false, "內建模擬：失敗");

				return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", id });
			}
		}
	}
