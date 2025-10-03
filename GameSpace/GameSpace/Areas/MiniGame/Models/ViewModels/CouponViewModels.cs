using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 用戶優惠券讀取模型
    /// </summary>
    public class UserCouponReadModel
    {
        public int CouponId { get; set; }
        public string CouponCode { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int CouponTypeId { get; set; }
        public string CouponTypeName { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? MinimumPurchase { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsUsed { get; set; }
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Now;
        public string Status
        {
            get
            {
                if (IsUsed) return "已使用";
                if (IsExpired) return "已過期";
                return "未使用";
            }
        }
    }

    /// <summary>
    /// 優惠券統計模型
    /// </summary>
    public class CouponStatisticsModel
    {
        public int TotalCoupons { get; set; }
        public int UsedCoupons { get; set; }
        public int UnusedCoupons { get; set; }
        public int ExpiredCoupons { get; set; }
        public int TotalCouponTypes { get; set; }
        public decimal TotalDiscountValue { get; set; }
        public Dictionary<string, int> CouponTypeDistribution { get; set; } = new();
        public Dictionary<string, int> UsageTrend { get; set; } = new();
    }

    /// <summary>
    /// 優惠券查詢模型
    /// </summary>
    public class CouponQueryModel
    {
        public int? UserId { get; set; }
        public int? CouponTypeId { get; set; }
        public string? Status { get; set; } // "used", "unused", "expired"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "AcquiredTime";
        public bool Descending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

