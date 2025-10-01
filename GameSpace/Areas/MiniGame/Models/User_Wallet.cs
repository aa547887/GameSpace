using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 使用者錢包表
    /// </summary>
    [Table("User_Wallet")]
    public class User_Wallet
    {
        [Key]
        public int User_Id { get; set; }

        public int User_Point { get; set; } = 0;

        // 導航屬性
        [ForeignKey("User_Id")]
        public virtual Users Users { get; set; } = null!;
    }
}