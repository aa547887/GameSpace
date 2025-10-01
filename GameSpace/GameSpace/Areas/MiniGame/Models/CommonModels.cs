using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 分頁結果
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>
    /// 查詢模型
    /// </summary>
    public class CouponQueryModel
    {
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? CouponName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 電子優惠券查詢模型
    /// </summary>
    public class EVoucherQueryModel
    {
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? EVoucherName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 寵物查詢模型
    /// </summary>
    public class PetQueryModel
    {
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? PetName { get; set; }
        public int? MinLevel { get; set; }
        public int? MaxLevel { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 遊戲查詢模型
    /// </summary>
    public class GameQueryModel
    {
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 電子優惠券創建模型
    /// </summary>
    public class EVoucherCreateModel
    {
        [Required(ErrorMessage = "請選擇用戶")]
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "請選擇優惠券類型")]
        public int EVoucherTypeId { get; set; }
        
        [Required(ErrorMessage = "請輸入數量")]
        [Range(1, 100, ErrorMessage = "數量必須在1-100之間")]
        public int Quantity { get; set; }
    }

    /// <summary>
    /// 簽到規則更新模型
    /// </summary>
    public class SignInRuleUpdateModel
    {
        [Required(ErrorMessage = "請輸入規則名稱")]
        [StringLength(100, ErrorMessage = "規則名稱不能超過100字")]
        public string RuleName { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "描述不能超過500字")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "請輸入積分獎勵")]
        [Range(1, 1000, ErrorMessage = "積分獎勵必須在1-1000之間")]
        public int PointsReward { get; set; }
        
        [Required(ErrorMessage = "請輸入連續天數")]
        [Range(1, 365, ErrorMessage = "連續天數必須在1-365之間")]
        public int ConsecutiveDays { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}

    /// <summary>
    /// 側邊欄視圖模型
    /// </summary>
    public class SidebarViewModel
    {
        public string CurrentPage { get; set; } = string.Empty;
        public List<SidebarItem> MenuItems { get; set; } = new();
    }

    /// <summary>
    /// 側邊欄項目
    /// </summary>
    public class SidebarItem
    {
        public string Text { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 遊戲摘要
    /// </summary>
    public class GameSummary
    {
        public int TotalGames { get; set; }
        public int ActiveGames { get; set; }
        public double AverageScore { get; set; }
    }

    /// <summary>
    /// 遊戲規則讀取模型
    /// </summary>
    public class GameRuleReadModel
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 寵物規則讀取模型
    /// </summary>
    public class PetRuleReadModel
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 寵物規則更新模型
    /// </summary>
    public class PetRulesUpdateModel
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 寵物規則
    /// </summary>
    public class PetRule
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 寵物膚色變更記錄
    /// </summary>
    public class PetSkinColorChangeLog
    {
        public int LogId { get; set; }
        public int PetId { get; set; }
        public string OldColor { get; set; } = string.Empty;
        public string NewColor { get; set; } = string.Empty;
        public DateTime ChangeDate { get; set; }
    }

    /// <summary>
    /// 寵物背景色變更記錄
    /// </summary>
    public class PetBackgroundColorChangeLog
    {
        public int LogId { get; set; }
        public int PetId { get; set; }
        public string OldColor { get; set; } = string.Empty;
        public string NewColor { get; set; } = string.Empty;
        public DateTime ChangeDate { get; set; }
    }
