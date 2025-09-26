using System;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 側邊欄視圖模型
    /// </summary>
    public class SidebarViewModel
    {
        /// <summary>
        /// 當前區域
        /// </summary>
        public string CurrentArea { get; set; } = string.Empty;
        
        /// <summary>
        /// 當前控制器
        /// </summary>
        public string CurrentController { get; set; } = string.Empty;
        
        /// <summary>
        /// 當前動作
        /// </summary>
        public string CurrentAction { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否為管理員
        /// </summary>
        public bool IsAdmin { get; set; }
        
        /// <summary>
        /// 用戶權限
        /// </summary>
        public List<string> UserPermissions { get; set; } = new();
    }

    /// <summary>
    /// 分頁結果
    /// </summary>
    /// <typeparam name="T">項目類型</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// 項目列表
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();
        
        /// <summary>
        /// 當前頁碼
        /// </summary>
        public int Page { get; set; }
        
        /// <summary>
        /// 每頁大小
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// 總數量
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        
        /// <summary>
        /// 是否有上一頁
        /// </summary>
        public bool HasPreviousPage => Page > 1;
        
        /// <summary>
        /// 是否有下一頁
        /// </summary>
        public bool HasNextPage => Page < TotalPages;
        
        /// <summary>
        /// 開始項目索引
        /// </summary>
        public int StartIndex => (Page - 1) * PageSize + 1;
        
        /// <summary>
        /// 結束項目索引
        /// </summary>
        public int EndIndex => Math.Min(Page * PageSize, TotalCount);
        
        /// <summary>
        /// 分頁資訊顯示文字
        /// </summary>
        public string PageInfoDisplay => $"顯示 {StartIndex}-{EndIndex} 項，共 {TotalCount} 項";
    }

    /// <summary>
    /// 電子禮券查詢模型
    /// </summary>
    public class EVoucherQueryModel
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
        /// 頁碼（別名）
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int? UserId { get; set; }
        
        /// <summary>
        /// 電子禮券類型ID
        /// </summary>
        public int? EVoucherTypeId { get; set; }
        
        /// <summary>
        /// 狀態
        /// </summary>
        public string? Status { get; set; }
        
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
    /// 遊戲查詢模型
    /// </summary>
    public class GameQueryModel
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
        /// 頁碼（別名）
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int? UserId { get; set; }
        
        /// <summary>
        /// 遊戲結果
        /// </summary>
        public string? Result { get; set; }
        
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
        public string SortBy { get; set; } = "StartTime";
        
        /// <summary>
        /// 排序方向
        /// </summary>
        public string SortDirection { get; set; } = "desc";
    }

    /// <summary>
    /// 簽到查詢模型
    /// </summary>
    public class SignInQueryModel
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
        /// 頁碼（別名）
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int? UserId { get; set; }
        
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
        public string SortBy { get; set; } = "SignInDate";
        
        /// <summary>
        /// 排序方向
        /// </summary>
        public string SortDirection { get; set; } = "desc";
    }

    /// <summary>
    /// 用戶優惠券讀取模型
    /// </summary>
    public class UserCouponReadModel
    {
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// 用戶名稱
        /// </summary>
        public string UserName { get; set; } = string.Empty;
        
        /// <summary>
        /// 優惠券ID
        /// </summary>
        public int CouponId { get; set; }
        
        /// <summary>
        /// 優惠券類型ID
        /// </summary>
        public int CouponTypeId { get; set; }
        
        /// <summary>
        /// 優惠券名稱
        /// </summary>
        public string CouponName { get; set; } = string.Empty;
        
        /// <summary>
        /// 折扣金額
        /// </summary>
        public int DiscountAmount { get; set; }
        
        /// <summary>
        /// 所需點數
        /// </summary>
        public int RequiredPoints { get; set; }
        
        /// <summary>
        /// 有效期（天）
        /// </summary>
        public int ValidityDays { get; set; }
        
        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// 過期時間
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// 是否已使用
        /// </summary>
        public bool IsUsed { get; set; }
        
        /// <summary>
        /// 使用時間
        /// </summary>
        public DateTime? UsedAt { get; set; }
        
        /// <summary>
        /// 是否過期
        /// </summary>
        public bool IsExpired => DateTime.Now > ExpiresAt;
        
        /// <summary>
        /// 狀態顯示文字
        /// </summary>
        public string StatusDisplay => IsUsed ? "已使用" : IsExpired ? "已過期" : "未使用";
    }

    /// <summary>
    /// 用戶電子禮券讀取模型
    /// </summary>
    public class UserEVoucherReadModel
    {
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// 用戶名稱
        /// </summary>
        public string UserName { get; set; } = string.Empty;
        
        /// <summary>
        /// 電子禮券ID
        /// </summary>
        public int EVoucherId { get; set; }
        
        /// <summary>
        /// 電子禮券類型ID
        /// </summary>
        public int EVoucherTypeId { get; set; }
        
        /// <summary>
        /// 電子禮券名稱
        /// </summary>
        public string EVoucherName { get; set; } = string.Empty;
        
        /// <summary>
        /// 面額
        /// </summary>
        public int FaceValue { get; set; }
        
        /// <summary>
        /// 所需點數
        /// </summary>
        public int RequiredPoints { get; set; }
        
        /// <summary>
        /// 有效期（天）
        /// </summary>
        public int ValidityDays { get; set; }
        
        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// 過期時間
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// 是否已使用
        /// </summary>
        public bool IsUsed { get; set; }
        
        /// <summary>
        /// 使用時間
        /// </summary>
        public DateTime? UsedAt { get; set; }
        
        /// <summary>
        /// 是否過期
        /// </summary>
        public bool IsExpired => DateTime.Now > ExpiresAt;
        
        /// <summary>
        /// 狀態顯示文字
        /// </summary>
        public string StatusDisplay => IsUsed ? "已使用" : IsExpired ? "已過期" : "未使用";
    }

    /// <summary>
    /// 優惠券查詢模型
    /// </summary>
    public class CouponQueryModel
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
        /// 頁碼（別名）
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int? UserId { get; set; }
        
        /// <summary>
        /// 優惠券類型ID
        /// </summary>
        public int? CouponTypeId { get; set; }
        
        /// <summary>
        /// 狀態
        /// </summary>
        public string? Status { get; set; }
        
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
    /// 小遊戲查詢模型
    /// </summary>
    public class MiniGameQueryModel
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
        /// 頁碼（別名）
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int? UserId { get; set; }
        
        /// <summary>
        /// 遊戲結果
        /// </summary>
        public string? Result { get; set; }
        
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
        public string SortBy { get; set; } = "StartTime";
        
        /// <summary>
        /// 排序方向
        /// </summary>
        public string SortDirection { get; set; } = "desc";
    }

    /// <summary>
    /// 寵物查詢模型
    /// </summary>
    public class PetQueryModel
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
        /// 頁碼（別名）
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int? UserId { get; set; }
        
        /// <summary>
        /// 寵物名稱
        /// </summary>
        public string? PetName { get; set; }
        
        /// <summary>
        /// 膚色
        /// </summary>
        public string? SkinColor { get; set; }
        
        /// <summary>
        /// 背景
        /// </summary>
        public string? Background { get; set; }
        
        /// <summary>
        /// 最小等級
        /// </summary>
        public int? MinLevel { get; set; }
        
        /// <summary>
        /// 最大等級
        /// </summary>
        public int? MaxLevel { get; set; }
        
        /// <summary>
        /// 排序方式
        /// </summary>
        public string SortBy { get; set; } = "Name";
        
        /// <summary>
        /// 排序方向
        /// </summary>
        public string SortDirection { get; set; } = "asc";
    }
}
