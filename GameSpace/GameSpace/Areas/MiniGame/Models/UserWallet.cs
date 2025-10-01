using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 使用者錢包表
    /// </summary>
    [Table("User_Wallet")]
    public class UserWallet
    {
        [Key]
        [Column("User_Id")]
        public int UserId { get; set; }

        [Column("User_Point")]
        public int UserPoint { get; set; } = 0;

        // 導航屬性
        public virtual ICollection<WalletHistory> WalletHistories { get; set; } = new List<WalletHistory>();
    }
}
