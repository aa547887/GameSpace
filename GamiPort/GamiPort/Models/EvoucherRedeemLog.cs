﻿using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class EvoucherRedeemLog
{
    public int RedeemId { get; set; }

    public int EvoucherId { get; set; }

    public int? TokenId { get; set; }

    public int UserId { get; set; }

    public DateTime ScannedAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual Evoucher Evoucher { get; set; } = null!;

    public virtual EvoucherToken? Token { get; set; }

    public virtual User User { get; set; } = null!;
}
