using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 使用者資料表
    /// </summary>
    [Table("Users")]
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int User_ID { get; set; }

        [Required]
        [StringLength(30)]
        public string User_name { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string User_Account { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string User_Password { get; set; } = string.Empty;

        public bool User_EmailConfirmed { get; set; } = false;

        public bool User_PhoneNumberConfirmed { get; set; } = false;

        public bool User_TwoFactorEnabled { get; set; } = false;

        public int User_AccessFailedCount { get; set; } = 0;

        public bool User_LockoutEnabled { get; set; } = true;

        public DateTime? User_LockoutEnd { get; set; }
    }
}