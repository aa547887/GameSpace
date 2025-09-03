using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int SourceId { get; set; }

    public int ActionId { get; set; }

    public int SenderId { get; set; }

    public int? SenderManagerId { get; set; }

    public string? NotificationTitle { get; set; }

    public string? NotificationMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? GroupId { get; set; }
}
