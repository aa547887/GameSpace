using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 用戶銷售資訊模型
    /// </summary>
    [Table("UserSalesInformation")]
    public class UserSalesInformation
    {
        /// <summary>
        /// 用戶銷售資訊ID
        /// </summary>
        [Key]
        public int UserSalesInformationId { get; set; }

        /// <summary>
        /// 用戶ID
        /// </summary>
        [Required(ErrorMessage = "用戶ID為必填欄位")]
        public int UserId { get; set; }

        /// <summary>
        /// 公司名稱
        /// </summary>
        [StringLength(200, ErrorMessage = "公司名稱不能超過200字")]
        public string? CompanyName { get; set; }

        /// <summary>
        /// 聯絡人
        /// </summary>
        [StringLength(100, ErrorMessage = "聯絡人不能超過100字")]
        public string? ContactPerson { get; set; }

        /// <summary>
        /// 電話號碼
        /// </summary>
        [StringLength(20, ErrorMessage = "電話號碼不能超過20字")]
        [Phone(ErrorMessage = "請輸入有效的電話號碼")]
        public string? Phone { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        [StringLength(200, ErrorMessage = "地址不能超過200字")]
        public string? Address { get; set; }

        /// <summary>
        /// 業務類型
        /// </summary>
        [StringLength(100, ErrorMessage = "業務類型不能超過100字")]
        public string? BusinessType { get; set; }

        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 電子郵件
        /// </summary>
        [StringLength(100, ErrorMessage = "電子郵件不能超過100字")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
        public string? Email { get; set; }

        /// <summary>
        /// 傳真號碼
        /// </summary>
        [StringLength(20, ErrorMessage = "傳真號碼不能超過20字")]
        public string? Fax { get; set; }

        /// <summary>
        /// 網站
        /// </summary>
        [StringLength(200, ErrorMessage = "網站不能超過200字")]
        [Url(ErrorMessage = "請輸入有效的網站地址")]
        public string? Website { get; set; }

        /// <summary>
        /// 統一編號
        /// </summary>
        [StringLength(20, ErrorMessage = "統一編號不能超過20字")]
        public string? TaxId { get; set; }

        /// <summary>
        /// 營業登記證號
        /// </summary>
        [StringLength(50, ErrorMessage = "營業登記證號不能超過50字")]
        public string? BusinessLicense { get; set; }

        /// <summary>
        /// 營業額
        /// </summary>
        public decimal? AnnualRevenue { get; set; }

        /// <summary>
        /// 員工人數
        /// </summary>
        public int? EmployeeCount { get; set; }

        /// <summary>
        /// 成立日期
        /// </summary>
        public DateTime? EstablishedDate { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [StringLength(500, ErrorMessage = "備註不能超過500字")]
        public string? Notes { get; set; }

        /// <summary>
        /// 更新者ID
        /// </summary>
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// 創建時間顯示文字
        /// </summary>
        public string CreatedAtDisplay => CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");

        /// <summary>
        /// 更新時間顯示文字
        /// </summary>
        public string UpdatedAtDisplay => UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "未更新";

        /// <summary>
        /// 業務類型顯示文字
        /// </summary>
        public string BusinessTypeDisplay => BusinessType ?? "未設定";

        /// <summary>
        /// 公司名稱顯示文字
        /// </summary>
        public string CompanyNameDisplay => CompanyName ?? "未設定";

        /// <summary>
        /// 聯絡人顯示文字
        /// </summary>
        public string ContactPersonDisplay => ContactPerson ?? "未設定";

        /// <summary>
        /// 電話號碼顯示文字
        /// </summary>
        public string PhoneDisplay => Phone ?? "未設定";

        /// <summary>
        /// 地址顯示文字
        /// </summary>
        public string AddressDisplay => Address ?? "未設定";

        /// <summary>
        /// 電子郵件顯示文字
        /// </summary>
        public string EmailDisplay => Email ?? "未設定";

        /// <summary>
        /// 網站顯示文字
        /// </summary>
        public string WebsiteDisplay => Website ?? "未設定";

        /// <summary>
        /// 統一編號顯示文字
        /// </summary>
        public string TaxIdDisplay => TaxId ?? "未設定";

        /// <summary>
        /// 營業登記證號顯示文字
        /// </summary>
        public string BusinessLicenseDisplay => BusinessLicense ?? "未設定";

        /// <summary>
        /// 營業額顯示文字
        /// </summary>
        public string AnnualRevenueDisplay => AnnualRevenue?.ToString("N0") ?? "未設定";

        /// <summary>
        /// 員工人數顯示文字
        /// </summary>
        public string EmployeeCountDisplay => EmployeeCount?.ToString() ?? "未設定";

        /// <summary>
        /// 成立日期顯示文字
        /// </summary>
        public string EstablishedDateDisplay => EstablishedDate?.ToString("yyyy-MM-dd") ?? "未設定";

        /// <summary>
        /// 是否完整填寫
        /// </summary>
        public bool IsComplete => !string.IsNullOrEmpty(CompanyName) &&
                                 !string.IsNullOrEmpty(ContactPerson) &&
                                 !string.IsNullOrEmpty(Phone) &&
                                 !string.IsNullOrEmpty(Address) &&
                                 !string.IsNullOrEmpty(BusinessType);

        /// <summary>
        /// 完整度百分比
        /// </summary>
        public double CompletionPercentage
        {
            get
            {
                var fields = new[] { CompanyName, ContactPerson, Phone, Address, BusinessType, Email, Website, TaxId, BusinessLicense };
                var filledFields = fields.Count(f => !string.IsNullOrEmpty(f));
                return (double)filledFields / fields.Length * 100;
            }
        }

        /// <summary>
        /// 完整度顯示文字
        /// </summary>
        public string CompletionPercentageDisplay => $"{CompletionPercentage:F0}%";

        /// <summary>
        /// 完整度顏色類別
        /// </summary>
        public string CompletionColorClass => CompletionPercentage switch
        {
            >= 80 => "text-success",
            >= 60 => "text-warning",
            >= 40 => "text-info",
            _ => "text-danger"
        };

        // 導航屬性
        /// <summary>
        /// 關聯的用戶
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// 關聯的更新者
        /// </summary>
        [ForeignKey("UpdatedBy")]
        public virtual User? UpdatedByUser { get; set; }
    }

    /// <summary>
    /// 用戶銷售資訊查詢模型
    /// </summary>
    public class UserSalesInformationQueryModel
    {
        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        public string SearchTerm { get; set; } = string.Empty;

        /// <summary>
        /// 頁碼
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每頁大小
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 用戶ID
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// 業務類型
        /// </summary>
        public string? BusinessType { get; set; }

        /// <summary>
        /// 是否完整填寫
        /// </summary>
        public bool? IsComplete { get; set; }

        /// <summary>
        /// 開始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 結束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 排序方式
        /// </summary>
        public string SortBy { get; set; } = "CreatedAt";

        /// <summary>
        /// 排序方向
        /// </summary>
        public string SortDirection { get; set; } = "desc";
    }

    /// <summary>
    /// 用戶銷售資訊統計模型
    /// </summary>
    public class UserSalesInformationStatistics
    {
        /// <summary>
        /// 總銷售資訊數
        /// </summary>
        public int TotalSalesInfo { get; set; }

        /// <summary>
        /// 完整填寫數
        /// </summary>
        public int CompleteSalesInfo { get; set; }

        /// <summary>
        /// 不完整填寫數
        /// </summary>
        public int IncompleteSalesInfo { get; set; }

        /// <summary>
        /// 各業務類型統計
        /// </summary>
        public Dictionary<string, int> SalesInfoByBusinessType { get; set; } = new();

        /// <summary>
        /// 完整度統計
        /// </summary>
        public Dictionary<string, int> CompletionLevels { get; set; } = new();

        /// <summary>
        /// 完整度比例
        /// </summary>
        public double CompletionRatio => TotalSalesInfo > 0 ? (double)CompleteSalesInfo / TotalSalesInfo * 100 : 0;

        /// <summary>
        /// 完整度比例顯示文字
        /// </summary>
        public string CompletionRatioDisplay => $"{CompletionRatio:F1}%";
    }
}
