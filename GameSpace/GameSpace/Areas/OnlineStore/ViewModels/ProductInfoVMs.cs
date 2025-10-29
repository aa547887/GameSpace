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
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "TWD";

        // 對應 S_ProductInfo.safety_stock
        public int ShipmentQuantity { get; set; }

        // 由 publish/unpublish/is_deleted 推得；控制器計算完放進來
        public bool IsActive { get; set; }

        // 對應 created_at（清單可顯示建立日或排序用）
        public DateTime ProductCreatedAt { get; set; }
    }

    // =========================
    // 2) Cards 卡片清單用（展示）
    // =========================
    public class ProductCardVM
    {
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "TWD";
        public bool IsActive { get; set; }
        public string? CoverUrl { get; set; }

        // Game
        public string? PlatformName { get; set; }
        public string? GameType { get; set; }

        // Other
        public string? MerchTypeName { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? SupplierName { get; set; }
    }

    // =========================
    // 3) 建立/編輯 用（右側面板 / Modal）
    // =========================
    public class ProductInfoFormVM
    {
        public int ProductId { get; set; }


        // "game" / "notgame"
        public string? ProductType { get; set; }

        public decimal Price { get; set; }


        public int? SupplierId { get; set; }

        public int? PlatformId { get; set; }

        // 控制器允許用名稱找 PlatformId
        public string? PlatformName { get; set; }

        public string? DownloadLink { get; set; }
        public string? GameProductDescription { get; set; }

        public string? DigitalCode { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Weight { get; set; }
        public string? Dimensions { get; set; }
        public string? Material { get; set; }
        public string? OtherProductDescription { get; set; }


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


