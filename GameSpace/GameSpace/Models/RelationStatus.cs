using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class RelationStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Relation> Relations { get; set; } = new List<Relation>();
}
