using System.Collections.Generic;

//namespace GamiPort.Areas.OnlineStore.DTO
//{
//	// 一列明細（給 View 顯示）
//	public record CartItemDto(
//		long ItemId,
//		int ProductId,
//		string ProductName,
//		decimal UnitPrice,
//		int Quantity,
//		decimal LineSubtotal);

//	// 整體摘要（View 的 @model）
//	public record CartSummaryDto(
//		IReadOnlyList<CartItemDto> Items,
//		int TotalQty,
//		decimal Subtotal);
//}
namespace GamiPort.Areas.OnlineStore.DTO
{
	// 明細：對應 usp_Cart_GetLines 的欄位
	public sealed class CartLineDto
	{
		public int Product_Id { get; set; }
		public string Product_Name { get; set; }
		public string Image_Thumb { get; set; }
		public decimal Unit_Price { get; set; }
		public int Quantity { get; set; }
		public decimal Line_Subtotal { get; set; }
		public bool Is_Physical { get; set; }
		public decimal Weight_G { get; set; }
		public string Status_Badge { get; set; }
		public bool Can_Checkout { get; set; }
		public string Note { get; set; }
	}

	// 摘要：對應 usp_Cart_GetSummary 的欄位（你剛提到的這一組）
	public sealed class CartSummaryDto
	{
		public int Item_Count_Total { get; set; }
		public int Item_Count_Physical { get; set; }
		public decimal Subtotal { get; set; }
		public decimal Subtotal_Physical { get; set; }
		public decimal Discount { get; set; }
		public decimal Shipping_Fee { get; set; }
		public decimal Grand_Total { get; set; }
		public decimal Total_Weight_G { get; set; }
		public string Rule_Notes_Json { get; set; }
		public bool Can_Checkout { get; set; }
		public string Block_Reason { get; set; }
	}

	// 頁面模型：一次承載兩個結果集（usp_Cart_GetFull）
	public sealed class CartVm
	{
		public List<CartLineDto> Lines { get; set; } = new();
		public CartSummaryDto Summary { get; set; } = new();
	}

	// 先保留舊的，改名避免衝突（改好所有呼叫點再刪）
	// [Obsolete("Use CartVm/CartSummaryDto + CartLineDto instead.")]
	public record LegacyCartSummaryDto(
		System.Collections.Generic.IReadOnlyList<LegacyCartItemDto> Items,
		int TotalQty,
		decimal Subtotal);

	public record LegacyCartItemDto(
		long ItemId,
		int ProductId,
		string ProductName,
		decimal UnitPrice,
		int Quantity,
		decimal LineSubtotal);
}
