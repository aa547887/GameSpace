using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class GroupChat
{
    public int GroupChatId { get; set; }

    public int? GroupId { get; set; }

    public int? SenderId { get; set; }

    public string? GroupChatContent { get; set; }

    public DateTime? SentAt { get; set; }

    public bool IsSent { get; set; }
}
