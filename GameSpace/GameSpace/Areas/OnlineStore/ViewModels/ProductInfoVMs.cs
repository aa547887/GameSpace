using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GameSpace.Areas.OnlineStore.ViewModels
{
    // =========================
    // 1) Index 表格列用（Info Only）
    // =========================
    public class ProductIndexRowVM
    {
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; } = string.Empty;

        // "game" / "notgame"
        [Required]
        public string ProductType { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "TWD";

        // 對應 S_ProductInfo.safety_stock
        public int ShipmentQuantity { get; set; }

        // 由 publish/unpublish/is_deleted 推得；控制器計算完放進來
        public bool IsActive { get; set; }

        // 對應 created_at（清單可顯示建立日或排序用）
        public DateTime ProductCreatedAt { get; set; }

        // 對應 S_ProductCodes.ProductCode（由觸發器產生）
        public string ProductCode { get; set; } = string.Empty;

        // 主要圖片（S_ProductImages.IsPrimary=1）給列表縮圖/檢視
        public string CoverUrl { get; set; } = string.Empty;

        // （可選）純數字排序值：若你要依代碼尾碼排序可用
        public long? ProductCodeSort { get; set; }
    }

    // =========================
    // 2) Cards 卡片清單用（展示）
    // =========================
    public class ProductCardVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "TWD";
        public bool IsActive { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }

        // Game
        public string? PlatformName { get; set; }
        public string? GameType { get; set; }

        // Other
        public string? MerchTypeName { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }

        // 共用
        public string? SupplierName { get; set; }
    }

    // =========================
    // 3) 建立/編輯 用（右側面板 / Modal）
    // =========================
    public class ProductInfoFormVM
    {
        public int ProductId { get; set; }

        // -------- 主表 S_ProductInfo --------
        [Required(ErrorMessage = "商品名稱為必填")]
        public string ProductName { get; set; } = string.Empty;

        // "game" / "notgame"
        [Required(ErrorMessage = "請選擇類別")]
        public string? ProductType { get; set; }

        [Required, Range(0, 999_999_999, ErrorMessage = "價格須為非負數")]
        public decimal Price { get; set; }

        [Required]
        public string CurrencyCode { get; set; } = "TWD";

        /// <summary>對應 S_ProductInfo.safety_stock</summary>
        public int? ShipmentQuantity { get; set; }

        /// <summary>畫面勾選上下架（實際由 publish/unpublish 決定）</summary>
        public bool IsActive { get; set; } = true;

        // （唯讀顯示）建立/更新資訊 – 供刪除/審核視圖顯示
        public DateTime? ProductCreatedAt { get; set; }
        public int? ProductCreatedBy { get; set; }
        public DateTime? ProductUpdatedAt { get; set; }
        public int? ProductUpdatedBy { get; set; }

        // -------- 共用（兩種明細都會用到）--------
        // 兩個 Detail 的 supplier_id 皆 NOT NULL；控制器會驗證 HasValue
        public int? SupplierId { get; set; }

        // -------- 遊戲明細 S_GameProductDetails --------
        // DB 可為 NULL → int?
        public int? PlatformId { get; set; }

        // 控制器允許用名稱找 PlatformId
        public string? PlatformName { get; set; }

        public string? DownloadLink { get; set; }
        public string? GameProductDescription { get; set; }

        // -------- 周邊明細 S_OtherProductDetails --------
        public int? MerchTypeId { get; set; }     // 可為 NULL
        public string? DigitalCode { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Weight { get; set; }
        public string? Dimensions { get; set; }
        public string? Material { get; set; }
        public string? OtherProductDescription { get; set; }

        // -------- 圖片（單張上傳 + 既有圖管理）--------
        public IFormFile? Image { get; set; } // 目前控制器示範單張；可自行擴充 List<IFormFile>

        /// <summary>編輯時顯示既有圖片（移除/調整排序/主圖）</summary>
        public List<ExistingImageVM>? ExistingImages { get; set; }
    }

    public class ExistingImageVM
    {
        public int ImageId { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; } = 0;

        /// <summary>勾選刪除此圖片</summary>
        public bool Remove { get; set; }
    }
}
