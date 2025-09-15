// ★ 新增：IFormFile 需這個命名空間
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace GameSpace.Areas.OnlineStore.ViewModels
{
	// ★ FIX：你的 Validate(...) 想要被 MVC 執行，類別要實作 IValidatableObject
	public class ProductInfoFormVM : IValidatableObject
	{
		[Display(Name = "商品ID")]
		public int ProductId { get; set; }

		[Display(Name = "商品名稱")]
		[Required(ErrorMessage = "{0}必填")]
		[StringLength(200, ErrorMessage = "名稱長度不可超過 200 字")]
		public string ProductName { get; set; } = "";

		[Display(Name = "商品類別")]
		[Required(ErrorMessage = "{0}必填")]
		[RegularExpression("^(game|nogame)$", ErrorMessage = "類別必須為 game 或 nogame")]
		public string ProductType { get; set; } = "game";

		[Display(Name = "價格")]
		[Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "{0}必須介於{1}~{2}")]
		public decimal Price { get; set; }

		[Display(Name = "幣別")]
		[Required(ErrorMessage = "{0}必選")]
		[StringLength(10)]
		[RegularExpression("^[A-Z]{3,10}$", ErrorMessage = "幣別需為 3~10 位大寫英文字母")]
		public string CurrencyCode { get; set; } = "TWD";

		[Display(Name = "現貨量")]
		[Range(0, int.MaxValue, ErrorMessage = "{0}不可小於 0")]
		public int? ShipmentQuantity { get; set; }



		[Display(Name = "是否上架")]
		public bool IsActive { get; set; } = true;

		// 這些是顯示用，不給編輯
		[ScaffoldColumn(false)] public int? ProductCreatedBy { get; set; }
		[ScaffoldColumn(false)] public DateTime? ProductCreatedAt { get; set; }
		[ScaffoldColumn(false)] public int? ProductUpdatedBy { get; set; }
		[ScaffoldColumn(false)] public DateTime? ProductUpdatedAt { get; set; }

		// === Detail 欄位（依商品類型）===
		// game
		[Display(Name = "供應商")] public int? SupplierId { get; set; }
		[Display(Name = "平台代碼")] public int? PlatformId { get; set; }
		[Display(Name = "平台名稱")] public string? PlatformName { get; set; }
		[Display(Name = "遊戲類型")] public string? GameType { get; set; }
		[Display(Name = "下載連結")] public string? DownloadLink { get; set; }

		// nogame
		[Display(Name = "商品分類")] public int? MerchTypeId { get; set; }
		[Display(Name = "數位序號")] public string? DigitalCode { get; set; }
		[Display(Name = "尺寸")] public string? Size { get; set; }
		[Display(Name = "顏色")] public string? Color { get; set; }
		[Display(Name = "重量")] public string? Weight { get; set; }
		[Display(Name = "尺寸(長寬高)")] public string? Dimensions { get; set; }
		[Display(Name = "材質")] public string? Material { get; set; }

		// === 圖片上傳 ===
		[Display(Name = "商品圖片")]
		public IFormFile[]? Images { get; set; } // <input type="file" multiple>

		// ★ 這裡才會被 MVC 呼叫
		public IEnumerable<ValidationResult> Validate(ValidationContext _)
		{
			if (ProductType == "nogame")
			{
				if (!ShipmentQuantity.HasValue)
					yield return new ValidationResult("非遊戲商品需填寫現貨量", new[] { nameof(ShipmentQuantity) });
				if (!MerchTypeId.HasValue)
					yield return new ValidationResult("請選擇商品分類", new[] { nameof(MerchTypeId) });
			}
			else if (ProductType == "game")
			{
				if (ShipmentQuantity.HasValue && ShipmentQuantity.Value != 0)
					yield return new ValidationResult("下載型遊戲現貨量請填 0 或留空", new[] { nameof(ShipmentQuantity) });
				if (!SupplierId.HasValue)
					yield return new ValidationResult("遊戲類需選擇供應商", new[] { nameof(SupplierId) });
				if (string.IsNullOrWhiteSpace(DownloadLink))
					yield return new ValidationResult("請填下載連結", new[] { nameof(DownloadLink) });
			}
		}
	}
}


//// 額外顯示用
//[Display(Name = "建立人")]
//public int? ProductCreatedBy { get; set; }

//[Display(Name = "建立時間")]
//public DateTime? ProductCreatedAt { get; set; }

//[Display(Name = "最後修改人")]
//public int? ProductUpdatedBy { get; set; }

//[Display(Name = "最後修改時間")]
//public DateTime? ProductUpdatedAt { get; set; }



