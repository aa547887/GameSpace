// 建議放 Areas/OnlineStore/Payments/EcpaySettings.cs
public class EcpaySettings
{
	public string MerchantID { get; set; } = "";
	public string HashKey { get; set; } = "";
	public string HashIV { get; set; } = "";
	public string ReturnURL { get; set; } = "";
	public string OrderResultURL { get; set; } = "";
	public string ClientBackURL { get; set; } = "";
}
