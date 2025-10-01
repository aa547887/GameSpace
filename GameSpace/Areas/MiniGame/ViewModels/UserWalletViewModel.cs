using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.ViewModels
{
    /// <summary>
    /// 會員錢包管理 ViewModel
    /// </summary>
    public class UserWalletViewModel
    {
        public int UserID { get; set; }
        
        [Display(Name = "會員帳號")]
        public string UserAccount { get; set; } = string.Empty;
        
        [Display(Name = "會員姓名")]
        public string UserName { get; set; } = string.Empty;
        
        [Display(Name = "當前點數")]
        public int CurrentPoints { get; set; }
        
        [Display(Name = "調整點數")]
        [Range(-10000, 10000, ErrorMessage = "點數調整範圍為 -10000 到 10000")]
        public int? AdjustPoints { get; set; }
        
        [Display(Name = "調整原因")]
        [StringLength(200, ErrorMessage = "調整原因不能超過200字")]
        public string? AdjustReason { get; set; }
        
        public List<WalletHistoryViewModel> WalletHistory { get; set; } = new();
        public List<CouponViewModel> UserCoupons { get; set; } = new();
        public List<EVoucherViewModel> UserEVouchers { get; set; } = new();
    }

    public class WalletHistoryViewModel
    {
        public int HistoryID { get; set; }
        public DateTime ChangeTime { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public int ChangeAmount { get; set; }
        public string? Description { get; set; }
    }

    public class CouponViewModel
    {
        public int CouponID { get; set; }
        public string CouponCode { get; set; } = string.Empty;
        public string CouponTypeName { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
    }

    public class EVoucherViewModel
    {
        public int EVoucherID { get; set; }
        public string EVoucherCode { get; set; } = string.Empty;
        public string EVoucherTypeName { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
    }
}