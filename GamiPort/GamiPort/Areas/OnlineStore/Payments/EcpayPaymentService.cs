// Areas/OnlineStore/Payments/EcpayPaymentService.cs
using System.Text;
using System.Security.Cryptography;

namespace GamiPort.Areas.OnlineStore.Payments
{
	public sealed class EcpayPaymentService
	{
		private readonly IHttpContextAccessor _http;

		private const string MerchantID = "3002607";
		private const string HashKey = "pwFHCqoQZGmho4w6";
		private const string HashIV = "EkRm7iFT261dpevs";
		private const string GatewayUrl = "https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5";

		public EcpayPaymentService(IHttpContextAccessor http) => _http = http;

		public (string action, Dictionary<string, string> fields) BuildCreditRequest(
			string orderCode, decimal amount, string itemName,
			string returnPath, string orderResultPath, string clientBackPath)
		{
			var req = _http.HttpContext!.Request;
			string Abs(string path) => new Uri(new Uri($"{req.Scheme}://{req.Host}"), path).ToString();

			var fields = new Dictionary<string, string>
			{
				["MerchantID"] = MerchantID,
				["MerchantTradeNo"] = orderCode,
				["MerchantTradeDate"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
				["PaymentType"] = "aio",
				["TotalAmount"] = ((int)Math.Round(amount)).ToString(),
				["TradeDesc"] = "GamiPort 訂單",
				["ItemName"] = itemName,
				["ReturnURL"] = Abs(returnPath),
				["OrderResultURL"] = Abs(orderResultPath),
				["ClientBackURL"] = Abs(clientBackPath),
				["ChoosePayment"] = "Credit",
				["EncryptType"] = "1"
			};

			fields["CheckMacValue"] = MakeCheckMacValue(fields, HashKey, HashIV);
			return (GatewayUrl, fields);
		}

		public static string MakeCheckMacValue(IDictionary<string, string> dict, string key, string iv)
		{
			var ordered = dict.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
							  .Select(p => $"{p.Key}={p.Value}");
			var raw = $"HashKey={key}&{string.Join("&", ordered)}&HashIV={iv}";
			var urlEncoded = System.Web.HttpUtility.UrlEncode(raw).ToLower();
			using var sha = SHA256.Create();
			var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(urlEncoded));
			return string.Concat(hash.Select(b => b.ToString("X2")));
		}
	}
}
