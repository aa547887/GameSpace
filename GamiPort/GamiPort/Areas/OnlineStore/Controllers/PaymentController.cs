using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using GamiPort.Models;

namespace GamiPort.Areas.OnlineStore.Controllers
{
	[Area("OnlineStore")]
	public class PaymentController : Controller
	{
		private readonly GameSpacedatabaseContext _db;

		public PaymentController(GameSpacedatabaseContext db) => _db = db;

		private static bool IsPaid(string? s)
		{
			if (string.IsNullOrWhiteSpace(s)) return false;
			var t = s.Trim();
			return t == "已付款"                       // 你的 DB 現行值（中文）
				|| t.Equals("PAID", System.StringComparison.OrdinalIgnoreCase)
				|| t.Equals("SUCCESS", System.StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>建立付款交易並導向「模擬付款」畫面</summary>
		[HttpGet]
		public async Task<IActionResult> Start(int id)
		{
			var order = await _db.SoOrderInfoes
				.AsNoTracking()
				.FirstOrDefaultAsync(o => o.OrderId == id);

			if (order == null) return RedirectToAction("Step1", "Checkout");
			if (IsPaid(order.PaymentStatus))
				return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", id });

			// 建立付款交易
			var pId = new SqlParameter("@PaymentId", System.Data.SqlDbType.BigInt) { Direction = System.Data.ParameterDirection.Output };
			var pCode = new SqlParameter("@PaymentCode", System.Data.SqlDbType.NVarChar, 14) { Direction = System.Data.ParameterDirection.Output };

			await _db.Database.ExecuteSqlRawAsync(
				"EXEC dbo.usp_Payment_Create @OrderId={0}, @TxnType={1}, @Provider={2}, @Amount={3}, @PaymentId=@PaymentId OUTPUT, @PaymentCode=@PaymentCode OUTPUT",
				id, "PAY", "ECP", order.GrandTotal ?? order.OrderTotal, pId, pCode);

			var paymentCode = (string?)pCode.Value ?? "";
			return RedirectToAction(nameof(MockPay), new { code = paymentCode, id });
		}

		/// <summary>內建模擬付款頁</summary>
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

		/// <summary>模擬：付款成功</summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> MockSuccess(string code, int id)
		{
			var providerTxn = $"MOCK-{code}"; // ← 自然唯一，符合你的唯一索引
			await _db.Database.ExecuteSqlRawAsync(
				"EXEC dbo.usp_Payment_Confirm @PaymentCode={0}, @ProviderTxn={1}, @IsSuccess={2}, @Note={3}",
				code, providerTxn, true, "內建模擬：成功");

			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", id });
		}

		/// <summary>模擬：付款失敗</summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> MockFail(string code, int id)
		{
			var providerTxn = $"MOCK-{code}"; // 失敗同樣傳唯一字串或可傳 NULL
			await _db.Database.ExecuteSqlRawAsync(
				"EXEC dbo.usp_Payment_Confirm @PaymentCode={0}, @ProviderTxn={1}, @IsSuccess={2}, @Note={3}",
				code, providerTxn, false, "內建模擬：失敗");

			// 失敗你也可以導回 MockPay；這裡先維持回 Success 方便檢視
			return RedirectToAction("Success", "Checkout", new { area = "OnlineStore", id });
		}
	}
}
