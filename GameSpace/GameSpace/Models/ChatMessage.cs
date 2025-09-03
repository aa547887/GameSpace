using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class ChatMessage
{
    public int MessageId { get; set; }

    public int? ManagerId { get; set; }

    public int SenderId { get; set; }

    public int? ReceiverId { get; set; }

    public string ChatContent { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }

    public bool IsSent { get; set; }
}
