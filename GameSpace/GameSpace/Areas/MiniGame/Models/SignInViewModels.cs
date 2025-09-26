using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    public class SignInViewModels
    {
        public class SignInIndexViewModel
        {
            public bool HasSignedInToday { get; set; }
            public int ConsecutiveDays { get; set; }
            public int MonthSignInCount { get; set; }
            public UserSignInStat? TodaySignIn { get; set; }
            public List<SignInReward> SignInRewards { get; set; } = new();
        }

        public class SignInHistoryViewModel
        {
            public List<UserSignInStat> SignInHistory { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class SignInStatsViewModel
        {
            public int TotalSignIns { get; set; }
            public int ConsecutiveDays { get; set; }
            public int TotalPoints { get; set; }
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }
    }

    public class SignInReward
    {
        public int Day { get; set; }
        public int Points { get; set; }
        public int Experience { get; set; }
        public string? BonusReward { get; set; }
    }

    // 管理員簽到管理相關 ViewModels
    public class AdminSignInManagementViewModel
    {
        public List<UserSignInStat> SignInStats { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public SignInQueryModel Query { get; set; } = new();
        public SignInStatisticsReadModel Statistics { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class SignInRuleManagementViewModel
    {
        public SignInRuleReadModel CurrentRule { get; set; } = new();
        public SignInRulesUpdateModel UpdateModel { get; set; } = new();
        public List<SignInRule> RuleHistory { get; set; } = new();
    }

    public class UserSignInDetailViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public List<UserSignInStat> SignInHistory { get; set; } = new();
        public SignInStatisticsReadModel UserStats { get; set; } = new();
        public List<SignInReward> AvailableRewards { get; set; } = new();
    }

    public class SignInCalendarViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<SignInCalendarDayModel> Days { get; set; } = new();
        public int TotalSignIns { get; set; }
        public int ConsecutiveDays { get; set; }
    }

    public class SignInCalendarDayModel
    {
        public int Day { get; set; }
        public bool HasSignedIn { get; set; }
        public DateTime SignInTime { get; set; }
        public int PointsEarned { get; set; }
        public bool IsToday { get; set; }
        public bool IsFuture { get; set; }
    }

    public class SignInRewardClaimViewModel
    {
        [Required(ErrorMessage = "請選擇獎勵")]
        public int RewardId { get; set; }
        
        [Required(ErrorMessage = "請確認領取")]
        public bool ConfirmClaim { get; set; }
        
        public List<SignInReward> AvailableRewards { get; set; } = new();
        public int UserId { get; set; }
        public int ConsecutiveDays { get; set; }
    }
}
