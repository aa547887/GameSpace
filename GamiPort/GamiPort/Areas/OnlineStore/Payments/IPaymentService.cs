using System.Collections.Generic;

namespace GamiPort.Areas.OnlineStore.Payments
{
	/// <summary>
	/// 金流對接的抽象介面（本案先實作 ECPay）
	/// </summary>
	public interface IPaymentService
	{
		/// <summary>
		/// 組出送往金流頁面需要的表單欄位（含 CheckMacValue）
		/// </summary>
		Dictionary<string, string> BuildStartFormFields(
			string orderCode,
			int orderId,
			decimal totalAmount,
			string itemName,
			string tradeDesc);

		/// <summary>
		/// 驗證 ECPay 回傳（Return/Notify）的 CheckMacValue 是否正確
		/// </summary>
		bool VerifyCheckMacValue(Dictionary<string, string> form);

		/// <summary>
		/// 取得 AIO 付款頁 URL
		/// </summary>
		string GetGatewayUrl();
	}
}
