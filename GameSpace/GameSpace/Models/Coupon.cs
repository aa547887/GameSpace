using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Coupon
{
    public int CouponId { get; set; }

    public string CouponCode { get; set; } = null!;

    public int CouponTypeId { get; set; }

    public int UserId { get; set; }

    public bool IsUsed { get; set; }

    public DateTime AcquiredTime { get; set; }

    public DateTime? UsedTime { get; set; }

    public int? UsedInOrderId { get; set; }

    public virtual OrderInfo? UsedInOrder { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual CouponType CouponType { get; set; } = null!;

    public DateTime CreatedDate { get; set; }
    
    // 新增的屬性
    public string UserName { get; set; } = string.Empty;
    public string CouponName { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
    public int Id { get; set; }
}
