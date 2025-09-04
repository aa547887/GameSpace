using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Pet
{
    public int PetId { get; set; }

    public int UserId { get; set; }

    public string PetName { get; set; } = null!;

    public int Level { get; set; }

    public DateTime LevelUpTime { get; set; }

    public int Experience { get; set; }

    public int Hunger { get; set; }

    public int Mood { get; set; }

    public int Stamina { get; set; }

    public int Cleanliness { get; set; }

    public int Health { get; set; }

    public string SkinColor { get; set; } = null!;

    public DateTime ColorChangedTime { get; set; }

    public string BackgroundColor { get; set; } = null!;

    public DateTime BackgroundColorChangedTime { get; set; }

    public int PointsChanged { get; set; }

    public DateTime PointsChangedTime { get; set; }

    public virtual ICollection<MiniGame> MiniGames { get; set; } = new List<MiniGame>();

    public virtual User User { get; set; } = null!;
}
