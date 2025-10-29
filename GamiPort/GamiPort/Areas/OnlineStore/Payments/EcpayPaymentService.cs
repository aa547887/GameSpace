// Areas/OnlineStore/Services/EcpayPaymentService.cs
using System.Text;
using System.Security.Cryptography;

namespace GamiPort.Areas.OnlineStore.Payments
{
	public sealed class EcpayPaymentService
	{
		private readonly IHttpContextAccessor _http;

		// 用你自己的 Sandbox 參數
		private const string MerchantID = "2000132";
		private const string HashKey = "5294y06JbISpM5x9";
		private const string HashIV = "v77hoKGq4kWxNNIS";
		private const string GatewayUrl = "https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5";

		public EcpayPaymentService(IHttpContextAccessor http) => _http = http;

		public (string action, Dictionary<string, string> fields) BuildCreditRequest(
			string orderCode, decimal amount, string itemName,
			string returnPath, string orderResultPath, string clientBackPath)
		{
			// 產生三個「完整網址」（一定要完整含 http(s) 與網域，localhost 要用外網隧道）
			var req = _http.HttpContext!.Request;
			string Abs(string path) => new Uri(new Uri($"{req.Scheme}://{req.Host}"), path).ToString();

			var fields = new Dictionary<string, string>
			{
				["MerchantID"] = MerchantID,
				["MerchantTradeNo"] = orderCode,                                       // 你的 12~20 碼唯一單號
				["MerchantTradeDate"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
				["PaymentType"] = "aio",
				["TotalAmount"] = ((int)Math.Round(amount)).ToString(),
				["TradeDesc"] = "GamiPort 訂單",
				["ItemName"] = itemName,                                        // 例：「遊戲幣x1#滑鼠x2」
				["ReturnURL"] = Abs(returnPath),                                 // 例：/Ecpay/Return
				["OrderResultURL"] = Abs(orderResultPath),                            // 例：/OnlineStore/Checkout/Success
				["ClientBackURL"] = Abs(clientBackPath),                             // 例：/OnlineStore/Checkout/Review
				["ChoosePayment"] = "Credit",
				["EncryptType"] = "1"
			};

			// 依綠界規格產生 CheckMacValue
			fields["CheckMacValue"] = MakeCheckMacValue(fields, HashKey, HashIV);

			return (GatewayUrl, fields);
		}

		public static string MakeCheckMacValue(IDictionary<string, string> dict, string key, string iv)
		{
			// 1) 依 Key 排序、2) URL encode、3) 前後加 HashKey/HashIV、4) SHA256、5) 全大寫十六進位
			var ordered = dict.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
							  .Select(p => $"{p.Key}={p.Value}");
			var raw = $"HashKey={key}&{string.Join("&", ordered)}&HashIV={iv}";
			var urlEncoded = System.Web.HttpUtility.UrlEncode(raw).ToLower(); // e.g. .NET 8 可用 WebUtility +自處理
			using var sha = SHA256.Create();
			var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(urlEncoded));
			return string.Concat(hash.Select(b => b.ToString("X2")));
		}
	}
}
