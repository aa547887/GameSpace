using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models;

public partial class WalletHistory
{
    public int LogId { get; set; }

    public int UserId { get; set; }

    public string ChangeType { get; set; } = null!;

    public int PointsChanged { get; set; }

    public string? ItemCode { get; set; }

    public string? Description { get; set; }

    public DateTime ChangeTime { get; set; }

    public virtual User User { get; set; } = null!;

    // Alias properties for backward compatibility
    [NotMapped]
    public string ActionType
    {
        get => ChangeType;
        set => ChangeType = value;
    }

    [NotMapped]
    public string TransactionType
    {
        get => ChangeType;
        set => ChangeType = value;
    }

    [NotMapped]
    public int Amount
    {
        get => Math.Abs(PointsChanged);
        set => PointsChanged = value;
    }

    [NotMapped]
    public DateTime TransactionDate
    {
        get => ChangeTime;
        set => ChangeTime = value;
    }
}

