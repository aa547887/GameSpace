using System;

namespace GameSpace.Areas.MiniGame.Models;

public class MiniGameRuleUpdateModel
{
    public int MonstersPerLevel { get; set; }
    public decimal MonsterSpeed { get; set; }
    public int DailyGameLimit { get; set; }
    public int BasePointsReward { get; set; }
    public int BaseExpReward { get; set; }
    public string? CouponReward { get; set; }
}
