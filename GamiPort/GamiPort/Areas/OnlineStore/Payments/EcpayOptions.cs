namespace GamiPort.Areas.OnlineStore.Payments
{
	public sealed class EcpayOptions
	{
		public string MerchantID { get; set; } = "";
		public string HashKey { get; set; } = "";
		public string HashIV { get; set; } = "";

		public string ReturnUrl { get; set; } = "";
		public string NotifyUrl { get; set; } = "";
		public string ClientBackUrl { get; set; } = "";

		public string AioCheckoutUrl { get; set; } = "https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5";
	}
}
