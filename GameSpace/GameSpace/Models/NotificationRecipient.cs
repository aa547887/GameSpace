using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class NotificationRecipient
{
    public int RecipientId { get; set; }

    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }
}
