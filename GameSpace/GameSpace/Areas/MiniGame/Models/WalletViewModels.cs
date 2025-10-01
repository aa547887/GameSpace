using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 錢包視圖模型集合
    /// </summary>
    public class WalletViewModels
    {
        /// <summary>
        /// 錢包統計信息
        /// </summary>
        public class WalletStatisticsReadModel
        {
            public int TotalUsers { get; set; }
            public int UsersWithPoints { get; set; }
            public long TotalPoints { get; set; }
            public long TotalCoupons { get; set; }
            public long TotalEVouchers { get; set; }
        }

        /// <summary>
        /// 錢包交易記錄
        /// </summary>
        public class WalletTransaction
        {
            public int TransactionId { get; set; }
            public int UserId { get; set; }
            public string TransactionType { get; set; } = string.Empty;
            public int Amount { get; set; }
            public string Description { get; set; } = string.Empty;
            public DateTime TransactionTime { get; set; }
        }
    }
}
