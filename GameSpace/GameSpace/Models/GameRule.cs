using System;

namespace GameSpace.Models
{
    public partial class GameRule
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public int DailyLimit { get; set; }
        public int MonsterCount { get; set; }
        public double MonsterSpeed { get; set; }
        public int WinPoints { get; set; }
        public int WinExp { get; set; }
    }
}
