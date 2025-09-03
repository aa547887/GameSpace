using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class OrderInfo
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? OrderStatus { get; set; }

    public string? PaymentStatus { get; set; }

    public decimal? OrderTotal { get; set; }

    public DateTime? PaymentAt { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}
