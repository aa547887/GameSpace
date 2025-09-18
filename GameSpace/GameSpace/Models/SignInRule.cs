using System;

namespace GameSpace.Models;

public partial class SignInRule
{
    public int RuleId { get; set; }
    public string RuleName { get; set; } = null!;
    public int DayNumber { get; set; }
    public int PointsReward { get; set; }
    public int ExpReward { get; set; }
    public string? CouponReward { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    public int DailyPoints { get; set; }

    public int WeeklyBonus { get; set; }

    public int MonthlyBonus { get; set; }
}
