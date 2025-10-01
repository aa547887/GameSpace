using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    // 分頁結果模型
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    // 基礎查詢模型
    public class BaseQueryModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageNumberSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }

    // 優惠券查詢模型
    public class CouponQueryModel : BaseQueryModel
    {
        public int? UserId { get; set; }
        public int? CouponTypeId { get; set; }
        public bool? IsUsed { get; set; }
        public DateTime? AcquiredFrom { get; set; }
        public DateTime? AcquiredTo { get; set; }
    }

    // 電子券查詢模型
    public class EVoucherQueryModel : BaseQueryModel
    {
        public int? UserId { get; set; }
        public int? EVoucherTypeId { get; set; }
        public bool? IsUsed { get; set; }
        public DateTime? AcquiredFrom { get; set; }
        public DateTime? AcquiredTo { get; set; }
    }

    // 寵物查詢模型
    public class PetQueryModel : BaseQueryModel
    {
        public int? UserId { get; set; }
        public int? MinLevel { get; set; }
        public int? MaxLevel { get; set; }
        public string? SkinColor { get; set; }
        public string? BackgroundColor { get; set; }
    }

    // 遊戲記錄查詢模型
    public class GameQueryModel : BaseQueryModel
    {
        public int? UserId { get; set; }
        public int? PetId { get; set; }
        public string? GameType { get; set; }
        public string? Result { get; set; }
        public DateTime? StartFrom { get; set; }
        public DateTime? StartTo { get; set; }
    }

    // 錢包歷史查詢模型
    public class WalletHistoryQueryModel : BaseQueryModel
    {
        public int? UserId { get; set; }
        public string? ChangeType { get; set; }
        public DateTime? ChangeFrom { get; set; }
        public DateTime? ChangeTo { get; set; }
    }

    // 簽到記錄查詢模型
    public class SignInQueryModel : BaseQueryModel
    {
        public int? UserId { get; set; }
        public DateTime? SignFrom { get; set; }
        public DateTime? SignTo { get; set; }
    }

    // 管理員查詢模型
    public class ManagerQueryModel : BaseQueryModel
    {
        public int? RoleId { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? RegistrationFrom { get; set; }
        public DateTime? RegistrationTo { get; set; }
    }

    // 用戶查詢模型
    public class UserQueryModel : BaseQueryModel
    {
        public bool? EmailConfirmed { get; set; }
        public bool? LockoutEnabled { get; set; }
        public DateTime? RegistrationFrom { get; set; }
        public DateTime? RegistrationTo { get; set; }
    }
}
