using System;
using System.Collections.Generic;

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

    public virtual User User { get; set; } = null!;
}

