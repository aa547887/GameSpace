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
}
