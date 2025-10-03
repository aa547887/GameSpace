﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 錢包查詢模型
    /// </summary>
    public class WalletQueryModel
    {
        public int? UserId { get; set; }
        public string? ChangeType { get; set; } // "Point", "Coupon", "EVoucher"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MinAmount { get; set; }
        public int? MaxAmount { get; set; }
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "ChangeTime";
        public bool Descending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 錢包統計模型
    /// </summary>
    public class WalletStatisticsModel
    {
        public int TotalTransactions { get; set; }
        public int TotalPointsDistributed { get; set; }
        public int TotalPointsSpent { get; set; }
        public int CurrentTotalPoints { get; set; }
        public int TotalCouponsDistributed { get; set; }
        public int TotalEVouchersDistributed { get; set; }
        public Dictionary<string, int> TransactionTypeDistribution { get; set; } = new();
        public Dictionary<string, int> DailyTransactionTrend { get; set; } = new();
    }

    /// <summary>
    /// 用戶錢包摘要
    /// </summary>
    public class UserWalletSummaryModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int CurrentPoints { get; set; }
        public int TotalPointsEarned { get; set; }
        public int TotalPointsSpent { get; set; }
        public int TotalCoupons { get; set; }
        public int UnusedCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public int UnusedEVouchers { get; set; }
        public DateTime? LastTransaction { get; set; }
    }

    /// <summary>
    /// 錢包摘要模型
    /// </summary>
    public class WalletSummary
    {
        public int TotalPoints { get; set; }
        public int PointsEarned { get; set; }
        public int PointsSpent { get; set; }
        public int AvailableCoupons { get; set; }
        public int UsedCoupons { get; set; }
        public int AvailableEVouchers { get; set; }
        public int UsedEVouchers { get; set; }
        public DateTime? LastTransactionTime { get; set; }
        public int TransactionCount { get; set; }

        // Additional properties for compatibility
        public int TotalUsers { get; set; }
    }

    /// <summary>
    /// 錢包交易模型
    /// </summary>
    public class WalletTransaction
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty; // Point, Coupon, EVoucher
        public int PointsChanged { get; set; }
        public int PointsAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime ChangeTime { get; set; }
        public string? RelatedCouponCode { get; set; }
        public string? RelatedEVoucherCode { get; set; }
    }
}

