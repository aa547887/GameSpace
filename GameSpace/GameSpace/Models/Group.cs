using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Group
{
    public int GroupId { get; set; }

    public string? GroupName { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }
}
