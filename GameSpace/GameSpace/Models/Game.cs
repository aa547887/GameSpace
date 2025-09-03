using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Game
{
    public int GameId { get; set; }

    public string? Name { get; set; }

    public string? Genre { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? NameZh { get; set; }
}
