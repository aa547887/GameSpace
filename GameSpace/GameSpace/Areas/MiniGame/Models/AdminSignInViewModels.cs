using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    public class AdminSignInRulesViewModel
    {
        public SignInRuleReadModel SignInRule { get; set; } = new();
        public SidebarViewModel Sidebar { get; set; } = new();
    }

    public class AdminSignInUserHistoryViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<SignInRecordReadModel> SignInHistory { get; set; } = new();
        public SidebarViewModel Sidebar { get; set; } = new();
    }

    public class SignInRuleReadModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PointsReward { get; set; }
        public int ConsecutiveDays { get; set; }
        public bool IsActive { get; set; }
    }

    public class SignInRecordReadModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime SignInDate { get; set; }
        public int PointsEarned { get; set; }
        public int ConsecutiveDays { get; set; }
    }
}
