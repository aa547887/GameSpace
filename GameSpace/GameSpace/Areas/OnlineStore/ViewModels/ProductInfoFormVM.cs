// ★ 新增：IFormFile 需這個命名空間
using GameSpace.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.OnlineStore.ViewModels
{
    // ★ FIX：你的 Validate(...) 想要被 MVC 執行，類別要實作 IValidatableObject
    // 用來承接「新增 / 編輯」表單資料
    // 並透過 DataAnnotations + IValidatableObject 做驗證
    public class ProductInfoFormVM : IValidatableObject
    {
        // =================== 基本資訊（ProductInfo） ===================
        [Display(Name = "商品ID")]
        public int ProductId { get; set; } // 編輯時才有值

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
        public bool IsActive { get; set; } = true;// ★ Info / Detail 同步用這個值


        // ========== Detail（共用）==========
        [Display(Name = "供應商")]
        public int? SupplierIds { get; set; }


        // === Game Detail 欄位（依商品類型）===
        // game

        [Display(Name = "平台代碼")] public int? PlatformId { get; set; }
        [Display(Name = "平台名稱")] public string? PlatformName { get; set; }
        [Display(Name = "遊戲類型")] public string? GameType { get; set; }
        [Display(Name = "下載連結")] public string? DownloadLink { get; set; }

        //========== Non-Game Detail ==========
        [Display(Name = "商品種類(周邊)")] public int? MerchTypeId { get; set; }
        [Display(Name = "數位序號")] public string? DigitalCode { get; set; }
        [Display(Name = "尺寸")] public string? Size { get; set; }
        [Display(Name = "顏色")] public string? Color { get; set; }
        [Display(Name = "重量(Kg)")] public string? Weight { get; set; }
        [Display(Name = "尺寸(長寬高)(cm)")] public string? Dimensions { get; set; }
        [Display(Name = "材質")] public string? Material { get; set; }
        [Display(Name = "明細庫存")][Range(0, int.MaxValue)] public int? StockQuantity { get; set; }


        // ★ 這裡才會被 MVC 呼叫
        // ========== 圖片 ==========
        [Display(Name = "上傳圖片")]
        public List<IFormFile>? Image { get; set; } // input name="Images" multiple

        // 已存在的圖片（編輯 / 詳細顯示用）
        public List<ProductImageVM> ExistingImages { get; set; } = new(); // 編輯時顯示/刪除


        // 這些是顯示用，不給編輯
        // =================== 系統資訊（唯讀） ===================
        [ScaffoldColumn(false)] public int? ProductCreatedBy { get; set; }
        [ScaffoldColumn(false)] public DateTime? ProductCreatedAt { get; set; }
        [ScaffoldColumn(false)] public int? ProductUpdatedBy { get; set; }
        [ScaffoldColumn(false)] public DateTime? ProductUpdatedAt { get; set; }
        public List<string>? ExistingImageUrls { get; set; }

        // =================== 驗證邏輯 ===================
        // 自訂驗證規則（會在 ModelState.IsValid 檢查時觸發）
        public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
        {
            // 共同規則
            if (Price < 0)
                yield return new ValidationResult("價格不可為負數。", new[] { nameof(Price) });

            if (string.IsNullOrWhiteSpace(CurrencyCode))
                yield return new ValidationResult("幣別必填。", new[] { nameof(CurrencyCode) });

            if (ProductType == "game")
            {
                if (!SupplierIds.HasValue)
                    yield return new ValidationResult("遊戲類需選擇供應商", new[] { nameof(SupplierIds) });

                if ((ShipmentQuantity ?? 0) != 0)
                    yield return new ValidationResult("下載型商品庫存請填 0 或留空", new[] { nameof(ShipmentQuantity) });

                if (!PlatformId.HasValue && string.IsNullOrWhiteSpace(PlatformName))
                    yield return new ValidationResult("請填平台或平台ID", new[] { nameof(PlatformId), nameof(PlatformName) });

                if (string.IsNullOrWhiteSpace(DownloadLink))
                    yield return new ValidationResult("請填下載連結", new[] { nameof(DownloadLink) });
            }
            else if (ProductType == "nogame")
            {
                if (!ShipmentQuantity.HasValue)
                    yield return new ValidationResult("非遊戲商品需填寫現貨量", new[] { nameof(ShipmentQuantity) });

                if (!MerchTypeId.HasValue)
                    yield return new ValidationResult("請選擇商品分類", new[] { nameof(MerchTypeId) });

                if (!SupplierIds.HasValue)
                    yield return new ValidationResult("非遊戲類需選擇供應商", new[] { nameof(SupplierIds) });
            }
            else
            {
                yield return new ValidationResult("商品類別必須為 game 或 nogame", new[] { nameof(ProductType) });
            }
        }

    }
        public class ProductImageVM
    {
        public int ImageId { get; set; }
        public string Url { get; set; } = "";
        public string? Alt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // 編輯時刪除用（checkbox）
        public bool Remove { get; set; }
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



