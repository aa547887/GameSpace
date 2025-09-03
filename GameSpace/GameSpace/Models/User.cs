using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string UserAccount { get; set; } = null!;

    public string UserPassword { get; set; } = null!;

    public bool UserEmailConfirmed { get; set; }

    public bool UserPhoneNumberConfirmed { get; set; }

    public bool UserTwoFactorEnabled { get; set; }

    public int UserAccessFailedCount { get; set; }

    public bool UserLockoutEnabled { get; set; }

    public DateTime? UserLockoutEnd { get; set; }
}
