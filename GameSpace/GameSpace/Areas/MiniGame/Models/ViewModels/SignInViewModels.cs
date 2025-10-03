﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 簽到規則讀取模型
    /// </summary>
    public class SignInRuleReadModel
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ConsecutiveDays { get; set; }
        public int PointsReward { get; set; }
        public int ExpReward { get; set; }
        public int? CouponTypeId { get; set; }
        public string? CouponTypeName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 簽到記錄讀取模型
    /// </summary>
    public class SignInRecordReadModel
    {
        public int RecordId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime SignInDate { get; set; }
        public int ConsecutiveDays { get; set; }
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public int? CouponGained { get; set; }
        public string? CouponCode { get; set; }
        public string RewardDescription { get; set; } = string.Empty;
    }

    /// <summary>
    /// 簽到統計查詢模型
    /// </summary>
    public class SignInStatsQueryModel
    {
        public int? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MinConsecutiveDays { get; set; }
        public int? MaxConsecutiveDays { get; set; }
        public string? SortBy { get; set; } = "SignInDate";
        public bool Descending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 簽到統計摘要
    /// </summary>
    public class SignInStatsSummary
    {
        public int TotalSignIns { get; set; }
        public int UniqueUsers { get; set; }
        public int TotalPointsDistributed { get; set; }
        public int TotalExpDistributed { get; set; }
        public int TotalCouponsDistributed { get; set; }
        public int AverageConsecutiveDays { get; set; }
        public int MaxConsecutiveDays { get; set; }
        public DateTime? LastSignInDate { get; set; }
        public Dictionary<int, int> ConsecutiveDaysDistribution { get; set; } = new();
        public Dictionary<string, int> DailySignInTrend { get; set; } = new();

        // Additional properties for compatibility
        public int TodaySignInCount { get; set; }
        public int ThisWeekSignInCount { get; set; }
        public int ThisMonthSignInCount { get; set; }
        public int PerfectAttendanceCount { get; set; }
        public int TotalPointsGranted { get => TotalPointsDistributed; set => TotalPointsDistributed = value; }
        public int TotalExpGranted { get => TotalExpDistributed; set => TotalExpDistributed = value; }
        public int TotalCouponsGranted { get => TotalCouponsDistributed; set => TotalCouponsDistributed = value; }
    }

    /// <summary>
    /// 簽到規則模型（用於建立/更新）
    /// </summary>
    public class SignInRulesModel
    {
        public int? RuleId { get; set; }

        [Required(ErrorMessage = "規則名稱為必填")]
        [StringLength(100, ErrorMessage = "規則名稱長度不可超過 100 字元")]
        public string RuleName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "描述長度不可超過 500 字元")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "連續天數為必填")]
        [Range(1, 365, ErrorMessage = "連續天數必須在 1-365 之間")]
        public int ConsecutiveDays { get; set; }

        [Required(ErrorMessage = "點數獎勵為必填")]
        [Range(0, 10000, ErrorMessage = "點數獎勵必須在 0-10000 之間")]
        public int PointsReward { get; set; }

        [Range(0, 10000, ErrorMessage = "經驗值獎勵必須在 0-10000 之間")]
        public int ExpReward { get; set; }

        public int? CouponTypeId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// 簽到統計索引 ViewModel
    /// </summary>
    public class AdminSignInStatsIndexViewModel
    {
        public SignInStatsSummary Summary { get; set; } = new();
        public List<SignInRecordReadModel> RecentSignIns { get; set; } = new();
        public List<SignInRuleReadModel> ActiveRules { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }

    /// <summary>
    /// 簽到規則更新模型
    /// </summary>
    public class SignInRuleUpdateModel
    {
        [Required(ErrorMessage = "規則ID為必填")]
        public int RuleId { get; set; }

        [Required(ErrorMessage = "規則名稱為必填")]
        [StringLength(100, ErrorMessage = "規則名稱長度不可超過 100 字元")]
        public string RuleName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "描述長度不可超過 500 字元")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "連續天數為必填")]
        [Range(1, 365, ErrorMessage = "連續天數必須在 1-365 之間")]
        public int ConsecutiveDays { get; set; }

        [Required(ErrorMessage = "點數獎勵為必填")]
        [Range(0, 10000, ErrorMessage = "點數獎勵必須在 0-10000 之間")]
        public int PointsReward { get; set; }

        [Range(0, 10000, ErrorMessage = "經驗值獎勵必須在 0-10000 之間")]
        public int ExpReward { get; set; }

        public int? CouponTypeId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 簽到規則配置 ViewModel (用於 Rules.cshtml 頁面)
    /// </summary>
    public class SignInRuleConfigViewModel
    {
        public SignInRuleConfig SignInRule { get; set; } = new();
    }

    /// <summary>
    /// 簽到規則配置模型
    /// </summary>
    public class SignInRuleConfig
    {
        [Required(ErrorMessage = "每日簽到點數為必填")]
        [Range(0, 10000, ErrorMessage = "每日簽到點數必須在 0-10000 之間")]
        public int DailyPoints { get; set; }

        [Required(ErrorMessage = "週獎勵點數為必填")]
        [Range(0, 10000, ErrorMessage = "週獎勵點數必須在 0-10000 之間")]
        public int WeeklyBonusPoints { get; set; }

        [Required(ErrorMessage = "月獎勵點數為必填")]
        [Range(0, 10000, ErrorMessage = "月獎勵點數必須在 0-10000 之間")]
        public int MonthlyBonusPoints { get; set; }

        [Required(ErrorMessage = "連續簽到天數為必填")]
        [Range(1, 365, ErrorMessage = "連續簽到天數必須在 1-365 之間")]
        public int ConsecutiveDays { get; set; }

        [StringLength(500, ErrorMessage = "規則描述長度不可超過 500 字元")]
        public string? Description { get; set; }
    }
}

