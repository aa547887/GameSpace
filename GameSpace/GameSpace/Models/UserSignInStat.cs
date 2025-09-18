using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class UserSignInStat
{
    public int LogId { get; set; }

    public DateTime SignTime { get; set; }

    public int UserId { get; set; }

    public int PointsGained { get; set; }

    public DateTime PointsGainedTime { get; set; }

    public int ExpGained { get; set; }

    public DateTime ExpGainedTime { get; set; }

    public string CouponGained { get; set; } = null!;

    public DateTime CouponGainedTime { get; set; }

    // 添加缺少的屬性
    public int TotalSignInDays { get; set; } = 0;
    public int ConsecutiveSignInDays { get; set; } = 0;
    public DateTime? LastSignInDate { get; set; }

    public virtual User User { get; set; } = null!;

    public DateTime SignInDate { get; set; }
}
