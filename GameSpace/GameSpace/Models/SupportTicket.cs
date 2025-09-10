using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SupportTicket
{
    public int TicketId { get; set; }

    public int UserId { get; set; }

    public int AssignedManagerId { get; set; }

    public bool? IsClosed { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public string? StatusText { get; set; }
}
