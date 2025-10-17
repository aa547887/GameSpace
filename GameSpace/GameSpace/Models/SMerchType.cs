using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class SMerchType
{
    public int MerchTypeId { get; set; }

    public string MerchTypeName { get; set; } = null!;

    public virtual ICollection<SOtherProductDetail> SOtherProductDetails { get; set; } = new List<SOtherProductDetail>();
}
