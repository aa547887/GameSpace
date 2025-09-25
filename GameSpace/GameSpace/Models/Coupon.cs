﻿using System;
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
}
