using System.Collections.Generic;

namespace GamiPort.Areas.OnlineStore.DTO
{
	// 一列明細（給 View 顯示）
	public record CartItemDto(
		long ItemId,
		int ProductId,
		string ProductName,
		decimal UnitPrice,
		int Quantity,
		decimal LineSubtotal);

	// 整體摘要（View 的 @model）
	public record CartSummaryDto(
		IReadOnlyList<CartItemDto> Items,
		int TotalQty,
		decimal Subtotal);
}