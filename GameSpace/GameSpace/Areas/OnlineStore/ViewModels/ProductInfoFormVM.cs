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



	public class ProductInfoFormVM
	{
		// ========= 基本資料 (Info) =========
		public int ProductId { get; set; }

		[Display(Name = "商品名稱"), Required, StringLength(200)]
		public string ProductName { get; set; } = "";

		/// <summary>game / nogame</summary>
		[Display(Name = "類別"), Required, StringLength(200)]
		public string ProductType { get; set; } = "game";

		[Display(Name = "售價"), Range(0, 999999999)]
		public decimal Price { get; set; }

		[Display(Name = "幣別"), StringLength(10)]
		public string CurrencyCode { get; set; } = "TWD";

		[Display(Name = "存量(Info)")]
		public int? ShipmentQuantity { get; set; }

		[Display(Name = "上架")]
		public bool IsActive { get; set; } = true;

		// 顯示用（非必填）
		public string? ProductCode { get; set; }
		public DateTime ProductCreatedAt { get; set; }
		public int? ProductCreatedBy { get; set; }
		public DateTime? ProductUpdatedAt { get; set; }
		public int? ProductUpdatedBy { get; set; }

		// ========= 共同：供應商 =========
		[Display(Name = "供應商")]
		public int? SupplierId { get; set; }

		// ========= Game 專用欄位 =========
		[Display(Name = "平台ID")]
		public int? PlatformId { get; set; }

		[Display(Name = "平台名稱")]
		public string? PlatformName { get; set; }

		[Display(Name = "遊戲種類")]
		public string? GameType { get; set; }

		[Display(Name = "下載連結")]
		public string? DownloadLink { get; set; }

		[Display(Name = "描述( Game )")]
		public string? GameProductDescription { get; set; }

		// ========= Non-Game 專用欄位 =========
		[Display(Name = "分類")]
		public int? MerchTypeId { get; set; }

		[Display(Name = "數位代碼")]
		public string? DigitalCode { get; set; }

		[Display(Name = "尺寸")]
		public string? Size { get; set; }

		[Display(Name = "顏色")]
		public string? Color { get; set; }

		[Display(Name = "重量")]
		public string? Weight { get; set; }

		[Display(Name = "尺寸(mm/cm)")]
		public string? Dimensions { get; set; }

		[Display(Name = "材質")]
		public string? Material { get; set; }

		[Display(Name = "庫存(Detail)")]
		public int? StockQuantity { get; set; }

		[Display(Name = "描述( Non-Game )")]
		public string? OtherProductDescription { get; set; }

		// ========= 圖片（ImgBB/外部連結） =========
		/// <summary>
		/// ★ ImgBB / 外部 URL 多筆：前端每新增一筆就 append 一個
		/// &lt;input type="hidden" name="NewImageUrls" value="..." /&gt;
		/// 建議使用 List 以確保 MVC 穩定綁定。
		/// </summary>
		[Display(Name = "圖片(外部連結)")]
		public List<string>? NewImageUrls { get; set; }

		// ========= 圖片上傳 / 舊圖 =========
		/// <summary>（如未使用直接對 ImgBB 上傳，可保留不使用）</summary>
		[Display(Name = "上傳圖片")]
		public IFormFile[]? Images { get; set; }

		/// <summary>編輯時顯示舊圖 + 是否刪除</summary>
		public List<ExistingImageItem> ExistingImages { get; set; } = new();

		public class ExistingImageItem
		{
			public int ImageId { get; set; }                 // 對應 ProductImage.ProductimgId
			public string Url { get; set; } = "";
			public string? Alt { get; set; }
			public bool Remove { get; set; } = false;        // 是否要刪除
		}

		// ========= 便利屬性 =========
		public bool IsCreate => ProductId == 0;
		public bool IsGame => string.Equals(ProductType, "game", StringComparison.OrdinalIgnoreCase);

		//// ========== 系統資訊（唯讀顯示用） ==========
		//public DateTime? ProductCreatedAt { get; set; }   // 設 nullable 讓 Razor 可用 ?.
		//public int? ProductCreatedBy { get; set; }
		//public DateTime? ProductUpdatedAt { get; set; }   // 本來就可能為 null
		//public int? ProductUpdatedBy { get; set; }

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