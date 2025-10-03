using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models;

public partial class MiniGame
{
    public int PlayId { get; set; }

    public int UserId { get; set; }

    public int PetId { get; set; }

    public int Level { get; set; }

    public int MonsterCount { get; set; }

    public decimal SpeedMultiplier { get; set; }

    public string Result { get; set; } = null!;

    public int ExpGained { get; set; }

    public DateTime ExpGainedTime { get; set; }

    public int PointsGained { get; set; }

    public DateTime PointsGainedTime { get; set; }

    public string CouponGained { get; set; } = null!;

    public DateTime CouponGainedTime { get; set; }

    public int HungerDelta { get; set; }

    public int MoodDelta { get; set; }

    public int StaminaDelta { get; set; }

    public int CleanlinessDelta { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public bool Aborted { get; set; }

    public virtual User User { get; set; } = null!;

    // Property aliases for compatibility with service layer
    [NotMapped]
    public int GameID { get => PlayId; set => PlayId = value; }

    [NotMapped]
    public int UserID { get => UserId; set => UserId = value; }

    [NotMapped]
    public int PetID { get => PetId; set => PetId = value; }

    [NotMapped]
    public string? GameType { get; set; }

    [NotMapped]
    public string? GameResult { get => Result; set => Result = value ?? string.Empty; }

    [NotMapped]
    public int PointsEarned { get => PointsGained; set => PointsGained = value; }

    [NotMapped]
    public int PetExpEarned { get => ExpGained; set => ExpGained = value; }

    [NotMapped]
    public int? CouponEarned
    {
        get => string.IsNullOrEmpty(CouponGained) ? null : int.TryParse(CouponGained, out var val) ? val : null;
        set => CouponGained = value?.ToString() ?? string.Empty;
    }

    [NotMapped]
    public string? SessionID { get; set; }
}
