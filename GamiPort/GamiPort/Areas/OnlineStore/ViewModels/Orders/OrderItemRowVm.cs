// 檔案：Areas/OnlineStore/ViewModels/Orders/OrderItemRowVm.cs
namespace GamiPort.Areas.OnlineStore.ViewModels
{
	public sealed class OrderItemRowVm
	{
		public string ProductCode { get; set; } = "";
		public string ProductName { get; set; } = "";
		public decimal UnitPrice { get; set; }
		public int Quantity { get; set; }
		public decimal Subtotal { get; set; }
	}
}
