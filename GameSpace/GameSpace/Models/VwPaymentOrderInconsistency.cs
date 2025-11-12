using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class VwPaymentOrderInconsistency
{
    public string PaymentCode { get; set; } = null!;

    public int OrderId { get; set; }

    public string TxnStatus { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? ProviderTxn { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public string OrderStatus { get; set; } = null!;

    public string? CouponCode { get; set; }
}
