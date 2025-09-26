using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 商城視圖模型集合
    /// </summary>
    public class ShopViewModels
    {
        /// <summary>
        /// 商城首頁視圖模型
        /// </summary>
        public class ShopIndexViewModel
        {
            /// <summary>
            /// 商品列表
            /// </summary>
            public List<ShopItemViewModel> Items { get; set; } = new();
            
            /// <summary>
            /// 用戶錢包
            /// </summary>
            public UserWallet Wallet { get; set; } = new();
            
            /// <summary>
            /// 發送者ID
            /// </summary>
            public int SenderID { get; set; }
            
            /// <summary>
            /// 商品分類
            /// </summary>
            public List<ShopCategory> Categories { get; set; } = new();
            
            /// <summary>
            /// 熱門商品
            /// </summary>
            public List<ShopItemViewModel> PopularItems { get; set; } = new();
            
            /// <summary>
            /// 推薦商品
            /// </summary>
            public List<ShopItemViewModel> RecommendedItems { get; set; } = new();
        }

        /// <summary>
        /// 商城優惠券視圖模型
        /// </summary>
        public class ShopCouponsViewModel
        {
            /// <summary>
            /// 優惠券類型列表
            /// </summary>
            public List<CouponType> CouponTypes { get; set; } = new();
            
            /// <summary>
            /// 用戶錢包
            /// </summary>
            public UserWallet Wallet { get; set; } = new();
            
            /// <summary>
            /// 發送者ID
            /// </summary>
            public int SenderID { get; set; }
            
            /// <summary>
            /// 用戶優惠券
            /// </summary>
            public List<Coupon> UserCoupons { get; set; } = new();
            
            /// <summary>
            /// 優惠券統計
            /// </summary>
            public CouponStatistics Statistics { get; set; } = new();
        }

        /// <summary>
        /// 商城電子禮券視圖模型
        /// </summary>
        public class ShopEVouchersViewModel
        {
            /// <summary>
            /// 電子禮券類型列表
            /// </summary>
            public List<EvoucherType> EVoucherTypes { get; set; } = new();
            
            /// <summary>
            /// 用戶錢包
            /// </summary>
            public UserWallet Wallet { get; set; } = new();
            
            /// <summary>
            /// 發送者ID
            /// </summary>
            public int SenderID { get; set; }
            
            /// <summary>
            /// 用戶電子禮券
            /// </summary>
            public List<Evoucher> UserEVouchers { get; set; } = new();
            
            /// <summary>
            /// 電子禮券統計
            /// </summary>
            public EVoucherStatistics Statistics { get; set; } = new();
        }

        /// <summary>
        /// 商城訂單視圖模型
        /// </summary>
        public class ShopOrdersViewModel
        {
            /// <summary>
            /// 訂單列表
            /// </summary>
            public List<OrderInfo> Orders { get; set; } = new();
            
            /// <summary>
            /// 用戶錢包
            /// </summary>
            public UserWallet Wallet { get; set; } = new();
            
            /// <summary>
            /// 發送者ID
            /// </summary>
            public int SenderID { get; set; }
            
            /// <summary>
            /// 訂單統計
            /// </summary>
            public OrderStatistics Statistics { get; set; } = new();
        }

        /// <summary>
        /// 商城訂單詳情視圖模型
        /// </summary>
        public class ShopOrderDetailsViewModel
        {
            /// <summary>
            /// 訂單資訊
            /// </summary>
            public OrderInfo Order { get; set; } = new();
            
            /// <summary>
            /// 用戶錢包
            /// </summary>
            public UserWallet Wallet { get; set; } = new();
            
            /// <summary>
            /// 發送者ID
            /// </summary>
            public int SenderID { get; set; }
            
            /// <summary>
            /// 訂單項目
            /// </summary>
            public List<OrderItem> OrderItems { get; set; } = new();
            
            /// <summary>
            /// 訂單狀態歷史
            /// </summary>
            public List<OrderStatusHistory> StatusHistory { get; set; } = new();
        }
    }

    /// <summary>
    /// 商城商品視圖模型
    /// </summary>
    public class ShopItemViewModel
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        public int ItemId { get; set; }
        
        /// <summary>
        /// 商品名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 商品描述
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// 所需點數
        /// </summary>
        public int PointsCost { get; set; }
        
        /// <summary>
        /// 商品類型
        /// </summary>
        public string ItemType { get; set; } = string.Empty;
        
        /// <summary>
        /// 商品圖片URL
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否可用
        /// </summary>
        public bool IsAvailable { get; set; }
        
        /// <summary>
        /// 庫存數量
        /// </summary>
        public int StockQuantity { get; set; }
        
        /// <summary>
        /// 銷售數量
        /// </summary>
        public int SalesCount { get; set; }
        
        /// <summary>
        /// 評分
        /// </summary>
        public double Rating { get; set; }
        
        /// <summary>
        /// 是否為熱門商品
        /// </summary>
        public bool IsPopular { get; set; }
        
        /// <summary>
        /// 是否為推薦商品
        /// </summary>
        public bool IsRecommended { get; set; }
        
        /// <summary>
        /// 折扣百分比
        /// </summary>
        public double DiscountPercentage { get; set; }
        
        /// <summary>
        /// 折扣後價格
        /// </summary>
        public int DiscountedPrice { get; set; }
        
        /// <summary>
        /// 是否已購買
        /// </summary>
        public bool IsPurchased { get; set; }
        
        /// <summary>
        /// 價格顯示文字
        /// </summary>
        public string PriceDisplay => DiscountPercentage > 0 ? $"{DiscountedPrice:N0} 點" : $"{PointsCost:N0} 點";
        
        /// <summary>
        /// 折扣顯示文字
        /// </summary>
        public string DiscountDisplay => DiscountPercentage > 0 ? $"-{DiscountPercentage:P0}" : "";
    }

    /// <summary>
    /// 管理員商城管理視圖模型
    /// </summary>
    public class AdminShopManagementViewModel
    {
        /// <summary>
        /// 優惠券類型列表
        /// </summary>
        public List<CouponType> CouponTypes { get; set; } = new();
        
        /// <summary>
        /// 電子禮券類型列表
        /// </summary>
        public List<EvoucherType> EVoucherTypes { get; set; } = new();
        
        /// <summary>
        /// 用戶列表
        /// </summary>
        public List<User> Users { get; set; } = new();
        
        /// <summary>
        /// 查詢條件
        /// </summary>
        public ShopQueryModel Query { get; set; } = new();
        
        /// <summary>
        /// 統計數據
        /// </summary>
        public ShopStatisticsReadModel Statistics { get; set; } = new();
        
        /// <summary>
        /// 總數量
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// 頁碼
        /// </summary>
        public int PageNumber { get; set; }
        
        /// <summary>
        /// 每頁大小
        /// </summary>
        public int PageSize { get; set; }
    }

    /// <summary>
    /// 優惠券類型管理視圖模型
    /// </summary>
    public class CouponTypeManagementViewModel
    {
        /// <summary>
        /// 優惠券類型列表
        /// </summary>
        public List<CouponType> CouponTypes { get; set; } = new();
        
        /// <summary>
        /// 創建模型
        /// </summary>
        public CouponTypeCreateModel CreateModel { get; set; } = new();
        
        /// <summary>
        /// 編輯模型
        /// </summary>
        public CouponTypeEditModel EditModel { get; set; } = new();
        
        /// <summary>
        /// 查詢條件
        /// </summary>
        public CouponQueryModel Query { get; set; } = new();
        
        /// <summary>
        /// 統計數據
        /// </summary>
        public CouponTypeStatistics Statistics { get; set; } = new();
    }

    /// <summary>
    /// 電子禮券類型管理視圖模型
    /// </summary>
    public class EVoucherTypeManagementViewModel
    {
        /// <summary>
        /// 電子禮券類型列表
        /// </summary>
        public List<EvoucherType> EVoucherTypes { get; set; } = new();
        
        /// <summary>
        /// 創建模型
        /// </summary>
        public EVoucherTypeCreateModel CreateModel { get; set; } = new();
        
        /// <summary>
        /// 編輯模型
        /// </summary>
        public EVoucherTypeEditModel EditModel { get; set; } = new();
        
        /// <summary>
        /// 查詢條件
        /// </summary>
        public EVoucherQueryModel Query { get; set; } = new();
        
        /// <summary>
        /// 統計數據
        /// </summary>
        public EVoucherTypeStatistics Statistics { get; set; } = new();
    }

    /// <summary>
    /// 優惠券類型創建模型
    /// </summary>
    public class CouponTypeCreateModel
    {
        /// <summary>
        /// 優惠券名稱
        /// </summary>
        [Required(ErrorMessage = "請輸入優惠券名稱")]
        [StringLength(100, ErrorMessage = "優惠券名稱不能超過100字")]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500, ErrorMessage = "描述不能超過500字")]
        public string? Description { get; set; }
        
        /// <summary>
        /// 折扣金額
        /// </summary>
        [Required(ErrorMessage = "請輸入折扣金額")]
        [Range(1, 10000, ErrorMessage = "折扣金額必須在1-10000之間")]
        public int DiscountAmount { get; set; }
        
        /// <summary>
        /// 所需點數
        /// </summary>
        [Required(ErrorMessage = "請輸入所需點數")]
        [Range(1, 10000, ErrorMessage = "所需點數必須在1-10000之間")]
        public int RequiredPoints { get; set; }
        
        /// <summary>
        /// 有效期（天）
        /// </summary>
        [Required(ErrorMessage = "請輸入有效期(天)")]
        [Range(1, 365, ErrorMessage = "有效期必須在1-365天之間")]
        public int ValidityDays { get; set; }
        
        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// 最大發行數量
        /// </summary>
        [Range(1, 100000, ErrorMessage = "最大發行數量必須在1-100000之間")]
        public int? MaxQuantity { get; set; }
        
        /// <summary>
        /// 使用條件
        /// </summary>
        [StringLength(200, ErrorMessage = "使用條件不能超過200字")]
        public string? UsageCondition { get; set; }
    }

    /// <summary>
    /// 優惠券類型編輯模型
    /// </summary>
    public class CouponTypeEditModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 優惠券名稱
        /// </summary>
        [Required(ErrorMessage = "請輸入優惠券名稱")]
        [StringLength(100, ErrorMessage = "優惠券名稱不能超過100字")]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500, ErrorMessage = "描述不能超過500字")]
        public string? Description { get; set; }
        
        /// <summary>
        /// 折扣金額
        /// </summary>
        [Required(ErrorMessage = "請輸入折扣金額")]
        [Range(1, 10000, ErrorMessage = "折扣金額必須在1-10000之間")]
        public int DiscountAmount { get; set; }
        
        /// <summary>
        /// 所需點數
        /// </summary>
        [Required(ErrorMessage = "請輸入所需點數")]
        [Range(1, 10000, ErrorMessage = "所需點數必須在1-10000之間")]
        public int RequiredPoints { get; set; }
        
        /// <summary>
        /// 有效期（天）
        /// </summary>
        [Required(ErrorMessage = "請輸入有效期(天)")]
        [Range(1, 365, ErrorMessage = "有效期必須在1-365天之間")]
        public int ValidityDays { get; set; }
        
        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// 最大發行數量
        /// </summary>
        [Range(1, 100000, ErrorMessage = "最大發行數量必須在1-100000之間")]
        public int? MaxQuantity { get; set; }
        
        /// <summary>
        /// 使用條件
        /// </summary>
        [StringLength(200, ErrorMessage = "使用條件不能超過200字")]
        public string? UsageCondition { get; set; }
    }

    /// <summary>
    /// 電子禮券類型創建模型
    /// </summary>
    public class EVoucherTypeCreateModel
    {
        /// <summary>
        /// 電子禮券名稱
        /// </summary>
        [Required(ErrorMessage = "請輸入電子禮券名稱")]
        [StringLength(100, ErrorMessage = "電子禮券名稱不能超過100字")]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500, ErrorMessage = "描述不能超過500字")]
        public string? Description { get; set; }
        
        /// <summary>
        /// 面額
        /// </summary>
        [Required(ErrorMessage = "請輸入面額")]
        [Range(1, 10000, ErrorMessage = "面額必須在1-10000之間")]
        public int FaceValue { get; set; }
        
        /// <summary>
        /// 所需點數
        /// </summary>
        [Required(ErrorMessage = "請輸入所需點數")]
        [Range(1, 10000, ErrorMessage = "所需點數必須在1-10000之間")]
        public int RequiredPoints { get; set; }
        
        /// <summary>
        /// 有效期（天）
        /// </summary>
        [Required(ErrorMessage = "請輸入有效期(天)")]
        [Range(1, 365, ErrorMessage = "有效期必須在1-365天之間")]
        public int ValidityDays { get; set; }
        
        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// 最大發行數量
        /// </summary>
        [Range(1, 100000, ErrorMessage = "最大發行數量必須在1-100000之間")]
        public int? MaxQuantity { get; set; }
        
        /// <summary>
        /// 使用條件
        /// </summary>
        [StringLength(200, ErrorMessage = "使用條件不能超過200字")]
        public string? UsageCondition { get; set; }
    }

    /// <summary>
    /// 電子禮券類型編輯模型
    /// </summary>
    public class EVoucherTypeEditModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 電子禮券名稱
        /// </summary>
        [Required(ErrorMessage = "請輸入電子禮券名稱")]
        [StringLength(100, ErrorMessage = "電子禮券名稱不能超過100字")]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500, ErrorMessage = "描述不能超過500字")]
        public string? Description { get; set; }
        
        /// <summary>
        /// 面額
        /// </summary>
        [Required(ErrorMessage = "請輸入面額")]
        [Range(1, 10000, ErrorMessage = "面額必須在1-10000之間")]
        public int FaceValue { get; set; }
        
        /// <summary>
        /// 所需點數
        /// </summary>
        [Required(ErrorMessage = "請輸入所需點數")]
        [Range(1, 10000, ErrorMessage = "所需點數必須在1-10000之間")]
        public int RequiredPoints { get; set; }
        
        /// <summary>
        /// 有效期（天）
        /// </summary>
        [Required(ErrorMessage = "請輸入有效期(天)")]
        [Range(1, 365, ErrorMessage = "有效期必須在1-365天之間")]
        public int ValidityDays { get; set; }
        
        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// 最大發行數量
        /// </summary>
        [Range(1, 100000, ErrorMessage = "最大發行數量必須在1-100000之間")]
        public int? MaxQuantity { get; set; }
        
        /// <summary>
        /// 使用條件
        /// </summary>
        [StringLength(200, ErrorMessage = "使用條件不能超過200字")]
        public string? UsageCondition { get; set; }
    }

    /// <summary>
    /// 商城查詢模型
    /// </summary>
    public class ShopQueryModel
    {
        /// <summary>
        /// 頁碼
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// 每頁大小
        /// </summary>
        public int PageSize { get; set; } = 10;
        
        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        public string? SearchTerm { get; set; }
        
        /// <summary>
        /// 商品類型
        /// </summary>
        public string? ItemType { get; set; }
        
        /// <summary>
        /// 最小點數
        /// </summary>
        public int? MinPoints { get; set; }
        
        /// <summary>
        /// 最大點數
        /// </summary>
        public int? MaxPoints { get; set; }
        
        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool? IsActive { get; set; }
        
        /// <summary>
        /// 排序方式
        /// </summary>
        public string SortBy { get; set; } = "Name";
        
        /// <summary>
        /// 排序方向
        /// </summary>
        public string SortDirection { get; set; } = "asc";
    }

    /// <summary>
    /// 商城統計讀取模型
    /// </summary>
    public class ShopStatisticsReadModel
    {
        /// <summary>
        /// 總優惠券類型數
        /// </summary>
        public int TotalCouponTypes { get; set; }
        
        /// <summary>
        /// 總電子禮券類型數
        /// </summary>
        public int TotalEVoucherTypes { get; set; }
        
        /// <summary>
        /// 總發行優惠券數
        /// </summary>
        public int TotalCouponsIssued { get; set; }
        
        /// <summary>
        /// 總發行電子禮券數
        /// </summary>
        public int TotalEVouchersIssued { get; set; }
        
        /// <summary>
        /// 總消耗點數
        /// </summary>
        public int TotalPointsSpent { get; set; }
        
        /// <summary>
        /// 總折扣金額
        /// </summary>
        public int TotalDiscountGiven { get; set; }
        
        /// <summary>
        /// 熱門商品
        /// </summary>
        public List<ShopPopularItemModel> PopularItems { get; set; } = new();
        
        /// <summary>
        /// 今日統計
        /// </summary>
        public DailyShopStatistics TodayStats { get; set; } = new();
    }

    /// <summary>
    /// 商城熱門商品模型
    /// </summary>
    public class ShopPopularItemModel
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        public int ItemId { get; set; }
        
        /// <summary>
        /// 商品名稱
        /// </summary>
        public string ItemName { get; set; } = string.Empty;
        
        /// <summary>
        /// 商品類型
        /// </summary>
        public string ItemType { get; set; } = string.Empty;
        
        /// <summary>
        /// 銷售數量
        /// </summary>
        public int SalesCount { get; set; }
        
        /// <summary>
        /// 總獲得點數
        /// </summary>
        public int TotalPointsEarned { get; set; }
        
        /// <summary>
        /// 熱門度分數
        /// </summary>
        public double PopularityScore { get; set; }
        
        /// <summary>
        /// 排名
        /// </summary>
        public int Rank { get; set; }
    }

    /// <summary>
    /// 商城分類
    /// </summary>
    public class ShopCategory
    {
        /// <summary>
        /// 分類ID
        /// </summary>
        public int CategoryId { get; set; }
        
        /// <summary>
        /// 分類名稱
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
        
        /// <summary>
        /// 分類描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 商品數量
        /// </summary>
        public int ItemCount { get; set; }
        
        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 優惠券統計
    /// </summary>
    public class CouponStatistics
    {
        /// <summary>
        /// 總優惠券數
        /// </summary>
        public int TotalCoupons { get; set; }
        
        /// <summary>
        /// 已使用優惠券數
        /// </summary>
        public int UsedCoupons { get; set; }
        
        /// <summary>
        /// 未使用優惠券數
        /// </summary>
        public int UnusedCoupons { get; set; }
        
        /// <summary>
        /// 過期優惠券數
        /// </summary>
        public int ExpiredCoupons { get; set; }
        
        /// <summary>
        /// 使用率
        /// </summary>
        public double UsageRate { get; set; }
        
        /// <summary>
        /// 使用率顯示文字
        /// </summary>
        public string UsageRateDisplay => $"{UsageRate:P1}";
    }

    /// <summary>
    /// 電子禮券統計
    /// </summary>
    public class EVoucherStatistics
    {
        /// <summary>
        /// 總電子禮券數
        /// </summary>
        public int TotalEVouchers { get; set; }
        
        /// <summary>
        /// 已使用電子禮券數
        /// </summary>
        public int UsedEVouchers { get; set; }
        
        /// <summary>
        /// 未使用電子禮券數
        /// </summary>
        public int UnusedEVouchers { get; set; }
        
        /// <summary>
        /// 過期電子禮券數
        /// </summary>
        public int ExpiredEVouchers { get; set; }
        
        /// <summary>
        /// 使用率
        /// </summary>
        public double UsageRate { get; set; }
        
        /// <summary>
        /// 使用率顯示文字
        /// </summary>
        public string UsageRateDisplay => $"{UsageRate:P1}";
    }

    /// <summary>
    /// 訂單統計
    /// </summary>
    public class OrderStatistics
    {
        /// <summary>
        /// 總訂單數
        /// </summary>
        public int TotalOrders { get; set; }
        
        /// <summary>
        /// 已完成訂單數
        /// </summary>
        public int CompletedOrders { get; set; }
        
        /// <summary>
        /// 進行中訂單數
        /// </summary>
        public int PendingOrders { get; set; }
        
        /// <summary>
        /// 已取消訂單數
        /// </summary>
        public int CancelledOrders { get; set; }
        
        /// <summary>
        /// 總金額
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// 平均訂單金額
        /// </summary>
        public decimal AverageOrderAmount { get; set; }
    }

    /// <summary>
    /// 訂單項目
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; }
        
        /// <summary>
        /// 商品名稱
        /// </summary>
        public string ProductName { get; set; } = string.Empty;
        
        /// <summary>
        /// 數量
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// 單價
        /// </summary>
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// 小計
        /// </summary>
        public decimal Subtotal { get; set; }
    }

    /// <summary>
    /// 訂單狀態歷史
    /// </summary>
    public class OrderStatusHistory
    {
        /// <summary>
        /// 歷史ID
        /// </summary>
        public int HistoryId { get; set; }
        
        /// <summary>
        /// 狀態
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// 變更時間
        /// </summary>
        public DateTime ChangedTime { get; set; }
        
        /// <summary>
        /// 變更原因
        /// </summary>
        public string? Reason { get; set; }
        
        /// <summary>
        /// 操作者
        /// </summary>
        public string Operator { get; set; } = string.Empty;
    }

    /// <summary>
    /// 每日商城統計
    /// </summary>
    public class DailyShopStatistics
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// 新增優惠券數
        /// </summary>
        public int NewCoupons { get; set; }
        
        /// <summary>
        /// 新增電子禮券數
        /// </summary>
        public int NewEVouchers { get; set; }
        
        /// <summary>
        /// 消耗點數
        /// </summary>
        public int PointsSpent { get; set; }
        
        /// <summary>
        /// 折扣金額
        /// </summary>
        public int DiscountGiven { get; set; }
    }

    /// <summary>
    /// 優惠券類型統計
    /// </summary>
    public class CouponTypeStatistics
    {
        /// <summary>
        /// 總類型數
        /// </summary>
        public int TotalTypes { get; set; }
        
        /// <summary>
        /// 啟用類型數
        /// </summary>
        public int ActiveTypes { get; set; }
        
        /// <summary>
        /// 總發行數
        /// </summary>
        public int TotalIssued { get; set; }
        
        /// <summary>
        /// 總使用數
        /// </summary>
        public int TotalUsed { get; set; }
        
        /// <summary>
        /// 使用率
        /// </summary>
        public double UsageRate { get; set; }
    }

    /// <summary>
    /// 電子禮券類型統計
    /// </summary>
    public class EVoucherTypeStatistics
    {
        /// <summary>
        /// 總類型數
        /// </summary>
        public int TotalTypes { get; set; }
        
        /// <summary>
        /// 啟用類型數
        /// </summary>
        public int ActiveTypes { get; set; }
        
        /// <summary>
        /// 總發行數
        /// </summary>
        public int TotalIssued { get; set; }
        
        /// <summary>
        /// 總使用數
        /// </summary>
        public int TotalUsed { get; set; }
        
        /// <summary>
        /// 使用率
        /// </summary>
        public double UsageRate { get; set; }
    }
}
