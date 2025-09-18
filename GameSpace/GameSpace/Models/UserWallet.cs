using System;

public partial class UserWallet
{
    public int UserId { get; set; }
    public int UserPoint { get; set; }
    
    // 新增的屬性
    public string UserName { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public int CurrentPoints { get; set; }
    public int TotalEarnedPoints { get; set; }
    public int TotalSpentPoints { get; set; }
    
    public virtual GameSpace.Models.User User { get; set; } = null!;
}
