using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SoPaymentAudit
{
    public long AuditId { get; set; }

    public DateTime HappenedAt { get; set; }

    public string PaymentCode { get; set; } = null!;

    public string? ProviderTxn { get; set; }

    public int? OrderId { get; set; }

    public string Phase { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string Result { get; set; } = null!;

    public string? Message { get; set; }
}
