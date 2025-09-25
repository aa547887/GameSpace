using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    public class CouponQueryModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }

    public class PetRuleReadModel
    {
        public string RuleName { get; set; } = string.Empty;
        public int LevelUpExp { get; set; }
        public int MaxLevel { get; set; }
        public int ColorChangeCost { get; set; }
        public int BackgroundChangeCost { get; set; }
    }

    public class GameRuleReadModel
    {
        public string RuleName { get; set; } = string.Empty;
        public int DailyLimit { get; set; }
        public int MonsterCount { get; set; }
        public double MonsterSpeed { get; set; }
        public int WinPoints { get; set; }
        public int WinExp { get; set; }
    }

    public class SignInRuleUpdateModel
    {
        public string RuleName { get; set; } = string.Empty;
        public int DailyPoints { get; set; }
        public int WeeklyBonus { get; set; }
        public int MonthlyBonus { get; set; }
    }

    public class PetRuleUpdateModel
    {
        public string RuleName { get; set; } = string.Empty;
        public int LevelUpExp { get; set; }
        public int MaxLevel { get; set; }
        public int ColorChangeCost { get; set; }
        public int BackgroundChangeCost { get; set; }
    }

    public class GameRuleUpdateModel
    {
        public string RuleName { get; set; } = string.Empty;
        public int DailyLimit { get; set; }
        public int MonsterCount { get; set; }
        public double MonsterSpeed { get; set; }
        public int WinPoints { get; set; }
        public int WinExp { get; set; }
    }

    public class SignInRule
    {
        public int Id { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public int DailyPoints { get; set; }
        public int WeeklyBonus { get; set; }
        public int MonthlyBonus { get; set; }
    }

    public class PetRule
    {
        public int Id { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public int LevelUpExp { get; set; }
        public int MaxLevel { get; set; }
        public int ColorChangeCost { get; set; }
        public int BackgroundChangeCost { get; set; }
    }

    public class GameRule
    {
        public int Id { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public int DailyLimit { get; set; }
        public int MonsterCount { get; set; }
        public double MonsterSpeed { get; set; }
        public int WinPoints { get; set; }
        public int WinExp { get; set; }
    }

    public class PetSummary
    {
        public int TotalPets { get; set; }
        public int ActivePets { get; set; }
        public double AverageLevel { get; set; }
    }

    public class GameSummary
    {
        public int TotalGames { get; set; }
        public int CompletedGames { get; set; }
        public double AverageScore { get; set; }
    }

    public class WalletSummary
    {
        public int TotalUsers { get; set; }
        public long TotalPoints { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
    }

    public class PetSkinColorChangeLog
    {
        public int LogId { get; set; }
        public int PetId { get; set; }
        public string OldColor { get; set; } = string.Empty;
        public string NewColor { get; set; } = string.Empty;
        public DateTime ChangeDate { get; set; }
        public int PointsCost { get; set; }
    }

    public class PetBackgroundColorChangeLog
    {
        public int LogId { get; set; }
        public int PetId { get; set; }
        public string OldBackground { get; set; } = string.Empty;
        public string NewBackground { get; set; } = string.Empty;
        public DateTime ChangeDate { get; set; }
        public int PointsCost { get; set; }
    }

    public class WalletTransaction
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class AdminAdjustCouponsViewModel
    {
        public List<GameSpace.Models.CouponType> CouponTypes { get; set; } = new List<GameSpace.Models.CouponType>();
        public List<GameSpace.Models.User> Users { get; set; } = new List<GameSpace.Models.User>();
        public string Action { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int CouponTypeId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class AdminAdjustEVouchersViewModel
    {
        public List<EvoucherType> EVoucherTypes { get; set; } = new List<EvoucherType>();
        public List<GameSpace.Models.User> Users { get; set; } = new List<GameSpace.Models.User>();
        public string Action { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int EVoucherTypeId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class UserPointsReadModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int UserPoint { get; set; }
    }
}
