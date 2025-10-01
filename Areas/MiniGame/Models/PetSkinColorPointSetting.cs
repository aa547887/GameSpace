using System.ComponentModel.DataAnnotations;

namespace Areas.MiniGame.Models
{
    /// <summary>
    /// 寵物換色所需點數設定模型
    /// </summary>
    public class PetSkinColorPointSetting
    {
        /// <summary>
        /// 設定ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 寵物等級
        /// </summary>
        [Required(ErrorMessage = "寵物等級為必填項目")]
        [Range(1, 100, ErrorMessage = "寵物等級必須在1-100之間")]
        public int PetLevel { get; set; }

        /// <summary>
        /// 換色所需點數
        /// </summary>
        [Required(ErrorMessage = "換色所需點數為必填項目")]
        [Range(1, 10000, ErrorMessage = "換色所需點數必須在1-10000之間")]
        public int RequiredPoints { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 建立者ID
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// 更新者ID
        /// </summary>
        public int UpdatedBy { get; set; }
    }
}
