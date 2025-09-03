using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class PlayerMarketOrderTradepage
{
    public int POrderTradepageId { get; set; }

    public int? POrderId { get; set; }

    public int? PProductId { get; set; }

    public int? POrderPlatformFee { get; set; }

    public DateTime? SellerTransferredAt { get; set; }

    public DateTime? BuyerReceivedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}
