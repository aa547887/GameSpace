using System.ComponentModel.DataAnnotations;

namespace Areas.MiniGame.Models
{
    /// <summary>
    /// 每日遊戲次數限制設定表單模型
    /// </summary>
    public class DailyGameLimitSettingFormViewModel
    {
        /// <summary>
        /// 設定ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 設定名稱
        /// </summary>
        [Required(ErrorMessage = "設定名稱為必填項目")]
        [StringLength(100, ErrorMessage = "設定名稱長度不能超過100個字符")]
        [Display(Name = "設定名稱")]
        public string SettingName { get; set; } = string.Empty;

        /// <summary>
        /// 每日遊戲次數限制
        /// </summary>
        [Required(ErrorMessage = "每日限制次數為必填項目")]
        [Range(1, 100, ErrorMessage = "每日限制次數必須在1-100之間")]
        [Display(Name = "每日限制次數")]
        public int DailyLimit { get; set; } = 3;

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Display(Name = "是否啟用")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 設定描述
        /// </summary>
        [StringLength(500, ErrorMessage = "設定描述長度不能超過500個字符")]
        [Display(Name = "設定描述")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// 每日遊戲次數限制設定列表模型
    /// </summary>
    public class DailyGameLimitSettingListViewModel
    {
        /// <summary>
        /// 設定ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 設定名稱
        /// </summary>
        public string SettingName { get; set; } = string.Empty;

        /// <summary>
        /// 每日遊戲次數限制
        /// </summary>
        public int DailyLimit { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 設定描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
