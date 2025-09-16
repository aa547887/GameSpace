using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GameSpace.Areas.OnlineStore.ViewModels
{
	/// <summary>
	/// Create/Edit 共用的表單 VM
	/// - 統一欄位名稱：SupplierId（單數）、Images（多檔上傳）
	/// - 新增 ExistingImages（支援編輯時顯示舊圖＋勾選刪除）
	/// - 將 ProductCreatedAt / UpdatedAt 設為 nullable，支援 Razor 的 ?.ToString(...)
	/// - Weight 為 string（符合你的 DB nvarchar(50)）
	/// </summary>
	public class ProductInfoFormVM : IValidatableObject
	{
		// ========== 基本 Info ==========
		public int ProductId { get; set; }

		[Display(Name = "商品名稱")]
		[Required(ErrorMessage = "請輸入商品名稱")]
		[StringLength(200)]
		public string ProductName { get; set; } = "";

		/// <summary>game / nogame</summary>
		[Display(Name = "種類")]
		[Required]
		public string ProductType { get; set; } = "game";

		[Display(Name = "價格")]
		[Range(0, 999999999, ErrorMessage = "價格不得為負數")]
		public decimal Price { get; set; }

		[Display(Name = "幣別")]
		[StringLength(10)]
		public string CurrencyCode { get; set; } = "TWD";

		[Display(Name = "清單存量(Info)")]
		public int? ShipmentQuantity { get; set; }

		[Display(Name = "上架")]
		public bool IsActive { get; set; } = true;

		// ========== Detail 共用 ==========
		[Display(Name = "供應商")]
		public int? SupplierId { get; set; }     // ★ 單數：和控制器一致

		// ========== Game 專用 ==========
		[Display(Name = "平台 Id")]
		public int? PlatformId { get; set; }

		[Display(Name = "平台名稱")]
		[StringLength(100)]
		public string? PlatformName { get; set; }

		[Display(Name = "遊戲類型")]
		[StringLength(200)]
		public string? GameType { get; set; }

		[Display(Name = "下載連結")]
		[StringLength(500)]
		public string? DownloadLink { get; set; }

		[Display(Name = "商品描述（遊戲）")]
		public string? GameProductDescription { get; set; }

		// ========== Non-game 專用 ==========
		[Display(Name = "周邊分類")]
		public int? MerchTypeId { get; set; }

		[Display(Name = "數位序號")]
		[StringLength(100)]
		public string? DigitalCode { get; set; }

		[Display(Name = "尺寸")][StringLength(50)] public string? Size { get; set; }
		[Display(Name = "顏色")][StringLength(50)] public string? Color { get; set; }

		/// <summary>字串型別，符合 DB nvarchar(50)</summary>
		[Display(Name = "重量")]
		[StringLength(50)]
		public string? Weight { get; set; }

		[Display(Name = "尺寸(長寬高)")]
		[StringLength(100)]
		public string? Dimensions { get; set; }

		[Display(Name = "材質")]
		[StringLength(50)]
		public string? Material { get; set; }

		[Display(Name = "庫存(Detail)")]
		public int? StockQuantity { get; set; }

		[Display(Name = "商品描述（周邊）")]
		public string? OtherProductDescription { get; set; }

		// ========== 圖片上傳 / 舊圖 ==========
		/// <summary>多檔上傳</summary>
		[Display(Name = "上傳圖片")]
		public IFormFile[]? Images { get; set; }

		/// <summary>編輯時顯示舊圖 + 是否刪除</summary>
		public List<ExistingImageItem>? ExistingImages { get; set; }

		public class ExistingImageItem
		{
			public int ImageId { get; set; }      // 對應 ProductImage.ProductimgId
			public string Url { get; set; } = "";
			public string? Alt { get; set; }
			public bool Remove { get; set; }       // 是否要刪除
		}

		// ========== 系統資訊（唯讀顯示用） ==========
		public DateTime? ProductCreatedAt { get; set; }   // 設 nullable 讓 Razor 可用 ?.
		public int? ProductCreatedBy { get; set; }
		public DateTime? ProductUpdatedAt { get; set; }   // 本來就可能為 null
		public int? ProductUpdatedBy { get; set; }

		// ========== 跨欄位驗證 ==========
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (ProductType == "game")
			{
				if (!SupplierId.HasValue)
					yield return new ValidationResult("遊戲類需選擇供應商", new[] { nameof(SupplierId) });
				// ShipmentQuantity 對 game 不是必填，允許 null
			}
			else if (ProductType == "nogame")
			{
				if (!SupplierId.HasValue)
					yield return new ValidationResult("周邊類需選擇供應商", new[] { nameof(SupplierId) });
				if (!MerchTypeId.HasValue)
					yield return new ValidationResult("周邊類需選擇分類", new[] { nameof(MerchTypeId) });
				if (!StockQuantity.HasValue || StockQuantity.Value < 0)
					yield return new ValidationResult("周邊庫存需填 ≥ 0", new[] { nameof(StockQuantity) });
			}
		}
	}
}
