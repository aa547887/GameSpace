using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models;

public partial class UserWallet
{
    public int UserId { get; set; }

    public int UserPoint { get; set; }

    // Alias properties for backward compatibility
    public int User_Id
    {
        get => UserId;
        set => UserId = value;
    }

    public int User_Point
    {
        get => UserPoint;
        set => UserPoint = value;
    }

    [NotMapped]
    public int Points
    {
        get => UserPoint;
        set => UserPoint = value;
    }

    public virtual User User { get; set; } = null!;
}

