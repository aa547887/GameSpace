using System;

namespace GameSpace.Models
{
    public partial class PetRule
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public int LevelUpExp { get; set; }
        public int MaxLevel { get; set; }
        public int ColorChangeCost { get; set; }
        public int BackgroundChangeCost { get; set; }
    }
}
