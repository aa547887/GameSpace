using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    public class ShopViewModels
    {
        public class ShopIndexViewModel
        {
            public List<ShopItemViewModel> Items { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class ShopCouponsViewModel
        {
            public List<CouponType> CouponTypes { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class ShopEVouchersViewModel
        {
            public List<EvoucherType> EVoucherTypes { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class ShopOrdersViewModel
        {
            public List<OrderInfo> Orders { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class ShopOrderDetailsViewModel
        {
            public OrderInfo Order { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }
    }

    public class ShopItemViewModel
    {
        public int ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PointsCost { get; set; }
        public string ItemType { get; set; } = string.Empty;
    }

    // 管理員商城管理相關 ViewModels
    public class AdminShopManagementViewModel
    {
        public List<CouponType> CouponTypes { get; set; } = new();
        public List<EvoucherType> EVoucherTypes { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public ShopQueryModel Query { get; set; } = new();
        public ShopStatisticsReadModel Statistics { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class CouponTypeManagementViewModel
    {
        public List<CouponType> CouponTypes { get; set; } = new();
        public CouponTypeCreateModel CreateModel { get; set; } = new();
        public CouponTypeEditModel EditModel { get; set; } = new();
        public CouponQueryModel Query { get; set; } = new();
    }

    public class EVoucherTypeManagementViewModel
    {
        public List<EvoucherType> EVoucherTypes { get; set; } = new();
        public EVoucherTypeCreateModel CreateModel { get; set; } = new();
        public EVoucherTypeEditModel EditModel { get; set; } = new();
        public EVoucherQueryModel Query { get; set; } = new();
    }

    public class CouponTypeCreateModel
    {
        [Required(ErrorMessage = "請輸入優惠券名稱")]
        [StringLength(100, ErrorMessage = "優惠券名稱不能超過100字")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "描述不能超過500字")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "請輸入折扣金額")]
        [Range(1, 10000, ErrorMessage = "折扣金額必須在1-10000之間")]
        public int DiscountAmount { get; set; }
        
        [Required(ErrorMessage = "請輸入所需點數")]
        [Range(1, 10000, ErrorMessage = "所需點數必須在1-10000之間")]
        public int RequiredPoints { get; set; }
        
        [Required(ErrorMessage = "請輸入有效期(天)")]
        [Range(1, 365, ErrorMessage = "有效期必須在1-365天之間")]
        public int ValidityDays { get; set; }
        
        [Required(ErrorMessage = "請選擇是否啟用")]
        public bool IsActive { get; set; } = true;
    }

    public class CouponTypeEditModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "請輸入優惠券名稱")]
        [StringLength(100, ErrorMessage = "優惠券名稱不能超過100字")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "描述不能超過500字")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "請輸入折扣金額")]
        [Range(1, 10000, ErrorMessage = "折扣金額必須在1-10000之間")]
        public int DiscountAmount { get; set; }
        
        [Required(ErrorMessage = "請輸入所需點數")]
        [Range(1, 10000, ErrorMessage = "所需點數必須在1-10000之間")]
        public int RequiredPoints { get; set; }
        
        [Required(ErrorMessage = "請輸入有效期(天)")]
        [Range(1, 365, ErrorMessage = "有效期必須在1-365天之間")]
        public int ValidityDays { get; set; }
        
        [Required(ErrorMessage = "請選擇是否啟用")]
        public bool IsActive { get; set; }
    }

    public class EVoucherTypeCreateModel
    {
        [Required(ErrorMessage = "請輸入電子禮券名稱")]
        [StringLength(100, ErrorMessage = "電子禮券名稱不能超過100字")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "描述不能超過500字")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "請輸入面額")]
        [Range(1, 10000, ErrorMessage = "面額必須在1-10000之間")]
        public int FaceValue { get; set; }
        
        [Required(ErrorMessage = "請輸入所需點數")]
        [Range(1, 10000, ErrorMessage = "所需點數必須在1-10000之間")]
        public int RequiredPoints { get; set; }
        
        [Required(ErrorMessage = "請輸入有效期(天)")]
        [Range(1, 365, ErrorMessage = "有效期必須在1-365天之間")]
        public int ValidityDays { get; set; }
        
        [Required(ErrorMessage = "請選擇是否啟用")]
        public bool IsActive { get; set; } = true;
    }

    public class EVoucherTypeEditModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "請輸入電子禮券名稱")]
        [StringLength(100, ErrorMessage = "電子禮券名稱不能超過100字")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "描述不能超過500字")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "請輸入面額")]
        [Range(1, 10000, ErrorMessage = "面額必須在1-10000之間")]
        public int FaceValue { get; set; }
        
        [Required(ErrorMessage = "請輸入所需點數")]
        [Range(1, 10000, ErrorMessage = "所需點數必須在1-10000之間")]
        public int RequiredPoints { get; set; }
        
        [Required(ErrorMessage = "請輸入有效期(天)")]
        [Range(1, 365, ErrorMessage = "有效期必須在1-365天之間")]
        public int ValidityDays { get; set; }
        
        [Required(ErrorMessage = "請選擇是否啟用")]
        public bool IsActive { get; set; }
    }

    public class ShopQueryModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? ItemType { get; set; }
        public int? MinPoints { get; set; }
        public int? MaxPoints { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ShopStatisticsReadModel
    {
        public int TotalCouponTypes { get; set; }
        public int TotalEVoucherTypes { get; set; }
        public int TotalCouponsIssued { get; set; }
        public int TotalEVouchersIssued { get; set; }
        public int TotalPointsSpent { get; set; }
        public int TotalDiscountGiven { get; set; }
        public List<ShopPopularItemModel> PopularItems { get; set; } = new();
    }

    public class ShopPopularItemModel
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public int SalesCount { get; set; }
        public int TotalPointsEarned { get; set; }
        public double PopularityScore { get; set; }
    }
}
