using System;
using System.Collections.Generic;

namespace GameSpace.Areas.MiniGame.Models
{
    // 電子禮券Token管理 ViewModel
    public class EVoucherTokenManagementViewModel
    {
        public int TotalTokens { get; set; }
        public int ActiveTokens { get; set; }
        public int ExpiredTokens { get; set; }
        public int RevokedTokens { get; set; }
        public List<EVoucherTokenViewModel> EVoucherTokens { get; set; } = new List<EVoucherTokenViewModel>();
    }

    public class EVoucherTokenViewModel
    {
        public int TokenID { get; set; }
        public int EVoucherID { get; set; }
        public string EVoucherCode { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsExpired { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
    }

    // 電子禮券核銷記錄管理 ViewModel
    public class EVoucherRedeemLogManagementViewModel
    {
        public int TotalRedeemLogs { get; set; }
        public int ApprovedLogs { get; set; }
        public int RejectedLogs { get; set; }
        public int ExpiredLogs { get; set; }
        public int AlreadyUsedLogs { get; set; }
        public List<EVoucherRedeemLogViewModel> EVoucherRedeemLogs { get; set; } = new List<EVoucherRedeemLogViewModel>();
    }

    public class EVoucherRedeemLogViewModel
    {
        public int RedeemID { get; set; }
        public int EVoucherID { get; set; }
        public string EVoucherCode { get; set; }
        public int? TokenID { get; set; }
        public string Token { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public DateTime ScannedAt { get; set; }
        public string Status { get; set; }
        public string StatusDescription { get; set; }
    }

    // 簽到規則設定管理 ViewModel
    public class SignInRuleSettingsManagementViewModel
    {
        public int TotalSettings { get; set; }
        public int ActiveSettings { get; set; }
        public List<SignInRuleSettingsViewModel> SignInRuleSettings { get; set; } = new List<SignInRuleSettingsViewModel>();
    }

    public class SignInRuleSettingsViewModel
    {
        public int SettingID { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CreatedByManagerId { get; set; }
        public string CreatedByManagerName { get; set; }
        public int? UpdatedByManagerId { get; set; }
        public string UpdatedByManagerName { get; set; }
    }

    // 寵物系統規則設定管理 ViewModel
    public class PetSystemRuleSettingsManagementViewModel
    {
        public int TotalSettings { get; set; }
        public int ActiveSettings { get; set; }
        public List<PetSystemRuleSettingsViewModel> PetSystemRuleSettings { get; set; } = new List<PetSystemRuleSettingsViewModel>();
    }

    public class PetSystemRuleSettingsViewModel
    {
        public int SettingID { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CreatedByManagerId { get; set; }
        public string CreatedByManagerName { get; set; }
        public int? UpdatedByManagerId { get; set; }
        public string UpdatedByManagerName { get; set; }
    }

    // 小遊戲規則設定管理 ViewModel
    public class MiniGameRuleSettingsManagementViewModel
    {
        public int TotalSettings { get; set; }
        public int ActiveSettings { get; set; }
        public List<MiniGameRuleSettingsViewModel> MiniGameRuleSettings { get; set; } = new List<MiniGameRuleSettingsViewModel>();
    }

    public class MiniGameRuleSettingsViewModel
    {
        public int SettingID { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CreatedByManagerId { get; set; }
        public string CreatedByManagerName { get; set; }
        public int? UpdatedByManagerId { get; set; }
        public string UpdatedByManagerName { get; set; }
    }

    // 寵物外觀變更記錄管理 ViewModel
    public class PetAppearanceChangeLogManagementViewModel
    {
        public int TotalChangeLogs { get; set; }
        public int SkinColorChanges { get; set; }
        public int BackgroundColorChanges { get; set; }
        public int TotalPointsSpent { get; set; }
        public List<PetAppearanceChangeLogViewModel> PetAppearanceChangeLogs { get; set; } = new List<PetAppearanceChangeLogViewModel>();
    }

    public class PetAppearanceChangeLogViewModel
    {
        public int LogID { get; set; }
        public int PetID { get; set; }
        public string PetName { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string ChangeType { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public int PointsCost { get; set; }
        public DateTime ChangeTime { get; set; }
    }

    // 會員錢包詳細管理 ViewModel (優化版)
    public class MemberWalletDetailViewModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public int CurrentPoints { get; set; }
        public int TotalCoupons { get; set; }
        public int ActiveCoupons { get; set; }
        public int UsedCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public int ActiveEVouchers { get; set; }
        public int UsedEVouchers { get; set; }
        public DateTime LastActivity { get; set; }
        public List<WalletHistoryDetailViewModel> RecentWalletHistory { get; set; } = new List<WalletHistoryDetailViewModel>();
        public List<CouponDetailViewModel> UserCoupons { get; set; } = new List<CouponDetailViewModel>();
        public List<EVoucherDetailViewModel> UserEVouchers { get; set; } = new List<EVoucherDetailViewModel>();
    }

    public class WalletHistoryDetailViewModel
    {
        public int LogID { get; set; }
        public string ChangeType { get; set; }
        public int PointsChanged { get; set; }
        public string ItemCode { get; set; }
        public string Description { get; set; }
        public DateTime ChangeTime { get; set; }
        public string Status { get; set; }
    }

    public class CouponDetailViewModel
    {
        public int CouponID { get; set; }
        public string CouponCode { get; set; }
        public string CouponTypeName { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinSpend { get; set; }
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
        public bool IsExpired { get; set; }
    }

    public class EVoucherDetailViewModel
    {
        public int EVoucherID { get; set; }
        public string EVoucherCode { get; set; }
        public string EVoucherTypeName { get; set; }
        public decimal ValueAmount { get; set; }
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
        public bool IsExpired { get; set; }
    }

    // 會員簽到詳細管理 ViewModel (優化版)
    public class MemberSignInDetailViewModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public int TotalSignIns { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime LastSignIn { get; set; }
        public DateTime LastActivity { get; set; }
        public List<SignInStatsDetailViewModel> RecentSignIns { get; set; } = new List<SignInStatsDetailViewModel>();
    }

    public class SignInStatsDetailViewModel
    {
        public int LogID { get; set; }
        public DateTime SignTime { get; set; }
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public string CouponGained { get; set; }
        public int StreakDays { get; set; }
        public string RewardDescription { get; set; }
    }

    // 會員寵物詳細管理 ViewModel (優化版)
    public class MemberPetDetailViewModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public int PetID { get; set; }
        public string PetName { get; set; }
        public int PetLevel { get; set; }
        public int PetExperience { get; set; }
        public int Hunger { get; set; }
        public int Mood { get; set; }
        public int Stamina { get; set; }
        public int Cleanliness { get; set; }
        public int Health { get; set; }
        public string SkinColor { get; set; }
        public string BackgroundColor { get; set; }
        public DateTime PetCreated { get; set; }
        public DateTime LastActivity { get; set; }
        public List<PetAppearanceChangeLogViewModel> AppearanceChangeHistory { get; set; } = new List<PetAppearanceChangeLogViewModel>();
    }

    // 會員遊戲詳細管理 ViewModel (優化版)
    public class MemberGameDetailViewModel
    {
        public int UserID { get; set; }
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
        public List<GameStatsDetailViewModel> RecentGames { get; set; } = new List<GameStatsDetailViewModel>();
    }

    public class GameStatsDetailViewModel
    {
        public int PlayID { get; set; }
        public int PetID { get; set; }
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
        public string GameDuration { get; set; }
    }
}
