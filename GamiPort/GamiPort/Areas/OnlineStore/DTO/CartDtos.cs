using System.Collections.Generic;

namespace GamiPort.Areas.OnlineStore.DTO
{
	// ─────────────────────────────────────────────────────────────────────────
	// 明細：對應 usp_Cart_GetFull 的 ResultSet #1 欄位
	// 命名維持你目前的蛇形/底線風格，避免大量重構成本
	// ─────────────────────────────────────────────────────────────────────────
	public sealed class CartLineDto
	{
		public int Product_Id { get; set; }
		public string Product_Name { get; set; } = string.Empty;   // [INIT]
		public string Image_Thumb { get; set; } = string.Empty;   // [INIT]
		public decimal Unit_Price { get; set; }
		public int Quantity { get; set; }
		public decimal Line_Subtotal { get; set; }
		public bool Is_Physical { get; set; }
		public decimal Weight_G { get; set; }
		public string Status_Badge { get; set; } = string.Empty;   // [INIT]
		public bool Can_Checkout { get; set; }
		public string Note { get; set; } = string.Empty;   // [INIT]
	}

	// ─────────────────────────────────────────────────────────────────────────
	// 摘要：對應 usp_Cart_GetSummary / usp_Cart_GetFull 的 ResultSet #2 欄位
	// C6 目標：以強型別鎖定欄位契約，避免 Razor/Service 在改名後悄悄壞掉
	// 這裡也先把「優惠折抵/訊息」欄位加進來（即使 SQL 尚未回傳，Service 會給預設值）
	// ─────────────────────────────────────────────────────────────────────────
	public sealed class CartSummaryDto
	{
		public int Item_Count_Total { get; set; }
		public int Item_Count_Physical { get; set; }

		/// <summary>商品小計（全部）</summary>
		public decimal Subtotal { get; set; }

		/// <summary>商品小計（實體）</summary>
		public decimal Subtotal_Physical { get; set; }

		/// <summary>促銷折扣（非優惠券），通常由活動/滿額等規則得出</summary>
		public decimal Discount { get; set; }

		/// <summary>運費（未含折扣）</summary>
		public decimal Shipping_Fee { get; set; }

		/// <summary>應付金額（維持你既有邏輯；此階段不將 Coupon 抵扣卷入）</summary>
		public decimal Grand_Total { get; set; }

		public decimal Total_Weight_G { get; set; }

		/// <summary>規則說明（JSON 陣列字串）；若 SQL 沒給將以 "[]" 補值</summary>
		public string Rule_Notes_Json { get; set; } = "[]";      // [INIT]

		public bool Can_Checkout { get; set; }
		public string Block_Reason { get; set; } = string.Empty; // [INIT]

		/// <summary>
		/// 優惠折抵（負數為折扣；可能為 null）
		/// 目前 SQL 若尚未回傳，Service 會以 0m 補值，Razor 仍會顯示 0
		/// </summary>
		public decimal? CouponDiscount { get; set; }

		/// <summary>
		/// 優惠訊息（灰字說明；可能為 null）
		/// 目前 SQL 若尚未回傳，Service 會以 null 補值，Razor 就不顯示
		/// </summary>
		public string? CouponMessage { get; set; }
	}

	// ─────────────────────────────────────────────────────────────────────────
	// 頁面模型：一次承載兩個結果集（usp_Cart_GetFull）
	// ─────────────────────────────────────────────────────────────────────────
	public sealed class CartVm
	{
		public List<CartLineDto> Lines { get; set; } = new(); // [INIT]
		public CartSummaryDto Summary { get; set; } = new(); // [INIT]
	}

	// ─────────────────────────────────────────────────────────────────────────
	// 舊版相容：可等整個頁面改完後再移除
	// ─────────────────────────────────────────────────────────────────────────
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
