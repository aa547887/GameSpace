using System;
using System.Collections.Generic;

namespace GameSpace.Areas.MiniGame.Models
{
    // 管理員儀表板 ViewModel
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalGamesPlayed { get; set; }
        public int NewSignInsToday { get; set; }
        public int TotalPets { get; set; }
        public int ActivePets { get; set; }
        public int TotalPointsIssued { get; set; }
        public int TotalCouponsIssued { get; set; }
        public int TotalEVouchersIssued { get; set; }
        public List<ActivityLogViewModel> RecentActivity { get; set; } = new List<ActivityLogViewModel>();
        public List<UserStatsViewModel> TopUsers { get; set; } = new List<UserStatsViewModel>();
    }

    public class ActivityLogViewModel
    {
        public DateTime Timestamp { get; set; }
        public string Module { get; set; }
        public string Operation { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
        public string Details { get; set; }
    }

    public class UserStatsViewModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public int TotalPoints { get; set; }
        public int SignInCount { get; set; }
        public int GamesPlayed { get; set; }
        public int PetLevel { get; set; }
        public DateTime LastActive { get; set; }
    }

    // 錢包管理 ViewModel
    public class WalletOverviewViewModel
    {
        public int TotalPoints { get; set; }
        public int TodayPointChanges { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public int ActiveCoupons { get; set; }
        public int ActiveEVouchers { get; set; }
        public List<WalletHistoryViewModel> WalletHistory { get; set; } = new List<WalletHistoryViewModel>();
        public List<UserWalletViewModel> UserWallets { get; set; } = new List<UserWalletViewModel>();
    }

    public class WalletHistoryViewModel
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string ChangeType { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
        public DateTime ChangeTime { get; set; }
        public string Status { get; set; }
    }

    public class UserWalletViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public int CurrentPoints { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public DateTime LastActivity { get; set; }
    }

    // 簽到管理 ViewModel
    public class SignInOverviewViewModel
    {
        public int TodaySignIns { get; set; }
        public int WeeklySignIns { get; set; }
        public int MonthlySignIns { get; set; }
        public int TotalSignIns { get; set; }
        public int ActiveStreaks { get; set; }
        public int LongestStreak { get; set; }
        public List<SignInStatsViewModel> RecentSignIns { get; set; } = new List<SignInStatsViewModel>();
        public List<UserSignInViewModel> UserSignIns { get; set; } = new List<UserSignInViewModel>();
    }

    public class SignInStatsViewModel
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime SignTime { get; set; }
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public string CouponGained { get; set; }
        public int StreakDays { get; set; }
    }

    public class UserSignInViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public int TotalSignIns { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime LastSignIn { get; set; }
        public DateTime LastActivity { get; set; }
    }

    // 寵物管理 ViewModel
    public class PetOverviewViewModel
    {
        public int TotalPets { get; set; }
        public int ActivePets { get; set; }
        public int AverageLevel { get; set; }
        public int HighestLevel { get; set; }
        public int TotalExperience { get; set; }
        public List<PetStatsViewModel> PetStats { get; set; } = new List<PetStatsViewModel>();
        public List<UserPetViewModel> UserPets { get; set; } = new List<UserPetViewModel>();
    }

    public class PetStatsViewModel
    {
        public int PetId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string PetName { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Hunger { get; set; }
        public int Mood { get; set; }
        public int Stamina { get; set; }
        public int Cleanliness { get; set; }
        public int Health { get; set; }
        public string SkinColor { get; set; }
        public string BackgroundColor { get; set; }
        public DateTime LastActivity { get; set; }
    }

    public class UserPetViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public int PetId { get; set; }
        public string PetName { get; set; }
        public int PetLevel { get; set; }
        public int PetExperience { get; set; }
        public DateTime PetCreated { get; set; }
        public DateTime LastActivity { get; set; }
    }

    // 小遊戲管理 ViewModel
    public class MiniGameOverviewViewModel
    {
        public int TotalGamesPlayed { get; set; }
        public int TodayGamesPlayed { get; set; }
        public int WeeklyGamesPlayed { get; set; }
        public int MonthlyGamesPlayed { get; set; }
        public int TotalWins { get; set; }
        public int TotalLosses { get; set; }
        public int TotalAborted { get; set; }
        public decimal WinRate { get; set; }
        public List<GameStatsViewModel> GameStats { get; set; } = new List<GameStatsViewModel>();
        public List<UserGameViewModel> UserGames { get; set; } = new List<UserGameViewModel>();
    }

    public class GameStatsViewModel
    {
        public int PlayId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int PetId { get; set; }
        public string PetName { get; set; }
        public int Level { get; set; }
        public int MonsterCount { get; set; }
        public decimal SpeedMultiplier { get; set; }
        public string Result { get; set; }
        public int ExpGained { get; set; }
        public int PointsGained { get; set; }
        public string CouponGained { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Aborted { get; set; }
    }

    public class UserGameViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public int TotalGamesPlayed { get; set; }
        public int TotalWins { get; set; }
        public int TotalLosses { get; set; }
        public int TotalAborted { get; set; }
        public decimal WinRate { get; set; }
        public int TotalPointsGained { get; set; }
        public int TotalExpGained { get; set; }
        public DateTime LastGamePlayed { get; set; }
    }

    // 優惠券管理 ViewModel
    public class CouponManagementViewModel
    {
        public int TotalCouponTypes { get; set; }
        public int ActiveCouponTypes { get; set; }
        public int TotalCouponsIssued { get; set; }
        public int ActiveCoupons { get; set; }
        public int UsedCoupons { get; set; }
        public int ExpiredCoupons { get; set; }
        public List<CouponTypeViewModel> CouponTypes { get; set; } = new List<CouponTypeViewModel>();
        public List<CouponViewModel> Coupons { get; set; } = new List<CouponViewModel>();
    }

    public class CouponTypeViewModel
    {
        public int CouponTypeId { get; set; }
        public string Name { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinSpend { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int PointsCost { get; set; }
        public string Description { get; set; }
        public int TotalIssued { get; set; }
        public int TotalUsed { get; set; }
        public bool IsActive { get; set; }
    }

    public class CouponViewModel
    {
        public int CouponId { get; set; }
        public string CouponCode { get; set; }
        public int CouponTypeId { get; set; }
        public string CouponTypeName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
        public int? UsedInOrderId { get; set; }
        public bool IsExpired { get; set; }
    }

    // 電子禮券管理 ViewModel
    public class EVoucherManagementViewModel
    {
        public int TotalEVoucherTypes { get; set; }
        public int ActiveEVoucherTypes { get; set; }
        public int TotalEVouchersIssued { get; set; }
        public int ActiveEVouchers { get; set; }
        public int UsedEVouchers { get; set; }
        public int ExpiredEVouchers { get; set; }
        public List<EVoucherTypeViewModel> EVoucherTypes { get; set; } = new List<EVoucherTypeViewModel>();
        public List<EVoucherViewModel> EVouchers { get; set; } = new List<EVoucherViewModel>();
    }

    public class EVoucherTypeViewModel
    {
        public int EVoucherTypeId { get; set; }
        public string Name { get; set; }
        public decimal ValueAmount { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int PointsCost { get; set; }
        public int TotalAvailable { get; set; }
        public string Description { get; set; }
        public int TotalIssued { get; set; }
        public int TotalUsed { get; set; }
        public bool IsActive { get; set; }
    }

    public class EVoucherViewModel
    {
        public int EVoucherId { get; set; }
        public string EVoucherCode { get; set; }
        public int EVoucherTypeId { get; set; }
        public string EVoucherTypeName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
        public bool IsExpired { get; set; }
    }
}
