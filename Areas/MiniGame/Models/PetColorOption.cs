using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 寵物顏色選項模型
    /// </summary>
    public class PetColorOption
    {
        /// <summary>
        /// 顏色選項ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 顏色名稱
        /// </summary>
        [Required(ErrorMessage = "顏色名稱為必填項目")]
        [StringLength(50, ErrorMessage = "顏色名稱長度不能超過50個字元")]
        public string ColorName { get; set; } = string.Empty;

        /// <summary>
        /// 顏色代碼 (HEX格式)
        /// </summary>
        [Required(ErrorMessage = "顏色代碼為必填項目")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "顏色代碼格式不正確，請使用HEX格式 (例: #FF0000)")]
        public string ColorCode { get; set; } = string.Empty;

        /// <summary>
        /// 顏色描述
        /// </summary>
        [StringLength(200, ErrorMessage = "顏色描述長度不能超過200個字元")]
        public string? Description { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 排序順序
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
