using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Evoucher
{
    public int EvoucherId { get; set; }

    public string EvoucherCode { get; set; } = null!;

    public int EvoucherTypeId { get; set; }

    public int UserId { get; set; }

    public bool IsUsed { get; set; }

    public DateTime AcquiredTime { get; set; }

    public DateTime? UsedTime { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual EvoucherType EVoucherType { get; set; } = null!;

    public int EVoucherTypeId { get; set; }

    public DateTime CreatedDate { get; set; }
    public string EVoucherName { get; set; } = string.Empty;
    public decimal Amount { get; set; } = 0;
    public DateTime ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Id { get; set; }
}
