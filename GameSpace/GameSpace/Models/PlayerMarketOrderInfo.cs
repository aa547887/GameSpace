using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class PlayerMarketOrderInfo
{
    public int POrderId { get; set; }

    public int? PProductId { get; set; }

    public int? SellerId { get; set; }

    public int? BuyerId { get; set; }

    public DateTime? POrderDate { get; set; }

    public string? POrderStatus { get; set; }

    public string? PPaymentStatus { get; set; }

    public int? PUnitPrice { get; set; }

    public int? PQuantity { get; set; }

    public int? POrderTotal { get; set; }

    public DateTime? POrderCreatedAt { get; set; }

    public DateTime? POrderUpdatedAt { get; set; }
}
