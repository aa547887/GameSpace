using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    public class WalletViewModels
    {
        public class WalletIndexViewModel
        {
            public List<UserWallet> Wallets { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class WalletDetailsViewModel
        {
            public UserWallet Wallet { get; set; } = new();
            public List<WalletHistory> History { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class WalletCouponsViewModel
        {
            public List<Coupon> Coupons { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class WalletEVouchersViewModel
        {
            public List<Evoucher> EVouchers { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class AvailableCouponsViewModel
        {
            public List<CouponType> CouponTypes { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class AvailableEVouchersViewModel
        {
            public List<EvoucherType> EVoucherTypes { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }
    }

    // 管理員錢包管理相關 ViewModels
    public class AdminWalletManagementViewModel
    {
        public List<UserWallet> UserWallets { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public WalletQueryModel Query { get; set; } = new();
        public WalletStatisticsReadModel Statistics { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class WalletTransactionManagementViewModel
    {
        public List<WalletTransaction> Transactions { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public WalletHistoryQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class PointsGrantViewModel
    {
        [Required(ErrorMessage = "請選擇用戶")]
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "請輸入點數")]
        [Range(1, 999999, ErrorMessage = "點數必須在1-999999之間")]
        public int Points { get; set; }
        
        [Required(ErrorMessage = "請輸入原因")]
        [StringLength(500, ErrorMessage = "原因不能超過500字")]
        public string Reason { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "請選擇交易類型")]
        public string TransactionType { get; set; } = string.Empty;
        
        public List<User> Users { get; set; } = new();
        public List<string> TransactionTypes { get; set; } = new();
    }

    public class CouponGrantViewModel
    {
        [Required(ErrorMessage = "請選擇用戶")]
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "請選擇優惠券類型")]
        public int CouponTypeId { get; set; }
        
        [Required(ErrorMessage = "請輸入數量")]
        [Range(1, 100, ErrorMessage = "數量必須在1-100之間")]
        public int Quantity { get; set; }
        
        [Required(ErrorMessage = "請輸入原因")]
        [StringLength(500, ErrorMessage = "原因不能超過500字")]
        public string Reason { get; set; } = string.Empty;
        
        public List<User> Users { get; set; } = new();
        public List<CouponType> CouponTypes { get; set; } = new();
    }

    public class EVoucherGrantViewModel
    {
        [Required(ErrorMessage = "請選擇用戶")]
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "請選擇電子禮券類型")]
        public int EVoucherTypeId { get; set; }
        
        [Required(ErrorMessage = "請輸入數量")]
        [Range(1, 100, ErrorMessage = "數量必須在1-100之間")]
        public int Quantity { get; set; }
        
        [Required(ErrorMessage = "請輸入原因")]
        [StringLength(500, ErrorMessage = "原因不能超過500字")]
        public string Reason { get; set; } = string.Empty;
        
        public List<User> Users { get; set; } = new();
        public List<EvoucherType> EVoucherTypes { get; set; } = new();
    }

    public class WalletTransactionDetailViewModel
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int Amount { get; set; }
        public int Balance { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string? ReferenceId { get; set; }
        public string? Source { get; set; }
    }

    public class WalletSummaryViewModel
    {
        public int TotalUsers { get; set; }
        public long TotalPoints { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AveragePointsPerUser { get; set; }
        public List<WalletTransactionTypeModel> TransactionTypes { get; set; } = new();
        public List<TopUserReadModel> TopUsers { get; set; } = new();
    }
}
