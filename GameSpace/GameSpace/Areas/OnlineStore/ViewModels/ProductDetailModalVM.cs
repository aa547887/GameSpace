using System;
using System.Collections.Generic;

namespace GameSpace.Areas.OnlineStore.ViewModels
{
	/// <summary>商品 Detail（Game / NoGame）彈窗 VM：兩種類型共用，一些欄位會是 null。</summary>
	public class ProductDetailModalVM
	{
		// 基本
		public int ProductId { get; set; }
		public string ProductName { get; set; } = "";
		public string ProductType { get; set; } = ""; // "game" / "nogame"
		public bool IsActive { get; set; }
		public decimal Price { get; set; }
		public int? ShipmentQuantity { get; set; }

		// Info 的時間與人員（方便彈窗顯示）
		public DateTime ProductCreatedAt { get; set; }
		public int? ProductCreatedBy { get; set; }
		public DateTime? ProductUpdatedAt { get; set; }
		public int? ProductUpdatedBy { get; set; }

		// 共通（由 Detail 來）
		public string? SupplierName { get; set; }

		// ---- game 專屬 ----
		public int? PlatformId { get; set; }
		public string? PlatformName { get; set; }
		public string? GameType { get; set; }
		public string? DownloadLink { get; set; }
		public string? GameDescription { get; set; }

		// ---- nogame 專屬 ----
		public int? MerchTypeId { get; set; }
		public string? MerchTypeName { get; set; }
		public string? DigitalCode { get; set; }
		public string? Size { get; set; }
		public string? Color { get; set; }
		public string? Weight { get; set; }
		public string? Dimensions { get; set; }
		public string? Material { get; set; }
		public int? StockQuantity { get; set; }
		public string? OtherDescription { get; set; }

		// 圖片（小縮圖）
		public List<ImageVM> Images { get; set; } = new();
		public class ImageVM
		{
			public int Id { get; set; }
			public string Url { get; set; } = "";
			public string? Alt { get; set; }
		}
	}
}
