using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 視圖模型集合
    /// </summary>
    public class ViewModels
    {
        /// <summary>
        /// 管理員錢包首頁視圖模型
        /// </summary>
        public class AdminWalletIndexViewModel
        {
            /// <summary>
            /// 用戶錢包列表
            /// </summary>
            public List<UserWallet> UserPoints { get; set; } = new();
            
            /// <summary>
            /// 用戶列表
            /// </summary>
            public List<User> Users { get; set; } = new();
            
            /// <summary>
            /// 查詢條件
            /// </summary>
            public CouponQueryModel Query { get; set; } = new();
            
            /// <summary>
            /// 總數量
            /// </summary>
            public int TotalCount { get; set; }
            
            /// <summary>
            /// 頁碼
            /// </summary>
            public int PageNumber { get; set; }
            
            /// <summary>
            /// 每頁大小
            /// </summary>
            public int PageSize { get; set; }
            
            /// <summary>
            /// 分頁結果
            /// </summary>
            public PagedResult<UserWallet> PagedResults { get; set; } = new();
            
            /// <summary>
            /// 統計數據
            /// </summary>
            public WalletStatisticsReadModel Statistics { get; set; } = new();
        }

        /// <summary>
        /// 發放點數視圖模型
        /// </summary>
        public class GrantPointsViewModel
        {
            /// <summary>
            /// 用戶ID
            /// </summary>
            [Required(ErrorMessage = "請選擇用戶")]
            public int UserId { get; set; }
            
            /// <summary>
            /// 點數
            /// </summary>
            [Required(ErrorMessage = "請輸入點數")]
            [Range(1, 999999, ErrorMessage = "點數必須在1-999999之間")]
            public int Points { get; set; }
            
            /// <summary>
            /// 原因
            /// </summary>
            [Required(ErrorMessage = "請輸入原因")]
            [StringLength(500, ErrorMessage = "原因不能超過500字")]
            public string Reason { get; set; } = string.Empty;
            
            /// <summary>
            /// 用戶列表
            /// </summary>
            public List<User> Users { get; set; } = new();
            
            /// <summary>
            /// 發放類型
            /// </summary>
            [Required(ErrorMessage = "請選擇發放類型")]
            public string GrantType { get; set; } = "Bonus";
            
            /// <summary>
            /// 發放說明
            /// </summary>
            [StringLength(200, ErrorMessage = "發放說明不能超過200字")]
            public string? Description { get; set; }
        }

        /// <summary>
        /// 管理員優惠券首頁視圖模型
        /// </summary>
        public class AdminCouponIndexViewModel
        {
            /// <summary>
            /// 優惠券列表
            /// </summary>
            public List<UserCouponReadModel> Coupons { get; set; } = new();
            
            /// <summary>
            /// 用戶列表
            /// </summary>
            public List<User> Users { get; set; } = new();
            
            /// <summary>
            /// 查詢條件
            /// </summary>
            public CouponQueryModel Query { get; set; } = new();
            
            /// <summary>
            /// 總數量
            /// </summary>
            public int TotalCount { get; set; }
            
            /// <summary>
            /// 頁碼
            /// </summary>
            public int PageNumber { get; set; }
            
            /// <summary>
            /// 每頁大小
            /// </summary>
            public int PageSize { get; set; }
            
            /// <summary>
            /// 分頁結果
            /// </summary>
            public PagedResult<UserCouponReadModel> PagedResults { get; set; } = new();
            
            /// <summary>
            /// 統計數據
            /// </summary>
            public CouponStatistics Statistics { get; set; } = new();
        }

        /// <summary>
        /// 發放優惠券視圖模型
        /// </summary>
        public class GrantCouponsViewModel
        {
            /// <summary>
            /// 用戶ID
            /// </summary>
            [Required(ErrorMessage = "請選擇用戶")]
            public int UserId { get; set; }
            
            /// <summary>
            /// 優惠券類型ID
            /// </summary>
            [Required(ErrorMessage = "請選擇優惠券類型")]
            public int CouponTypeId { get; set; }
            
            /// <summary>
            /// 發放數量
            /// </summary>
            [Required(ErrorMessage = "請輸入發放數量")]
            [Range(1, 100, ErrorMessage = "發放數量必須在1-100之間")]
            public int Quantity { get; set; }
            
            /// <summary>
            /// 發放原因
            /// </summary>
            [Required(ErrorMessage = "請輸入發放原因")]
            [StringLength(500, ErrorMessage = "發放原因不能超過500字")]
            public string Reason { get; set; } = string.Empty;
            
            /// <summary>
            /// 用戶列表
            /// </summary>
            public List<User> Users { get; set; } = new();
            
            /// <summary>
            /// 優惠券類型列表
            /// </summary>
            public List<CouponType> CouponTypes { get; set; } = new();
            
            /// <summary>
            /// 發放說明
            /// </summary>
            [StringLength(200, ErrorMessage = "發放說明不能超過200字")]
            public string? Description { get; set; }
        }

        /// <summary>
        /// 管理員電子禮券首頁視圖模型
        /// </summary>
        public class AdminEVoucherIndexViewModel
        {
            /// <summary>
            /// 電子禮券列表
            /// </summary>
            public List<UserEVoucherReadModel> EVouchers { get; set; } = new();
            
            /// <summary>
            /// 用戶列表
            /// </summary>
            public List<User> Users { get; set; } = new();
            
            /// <summary>
            /// 查詢條件
            /// </summary>
            public EVoucherQueryModel Query { get; set; } = new();
            
            /// <summary>
            /// 總數量
            /// </summary>
            public int TotalCount { get; set; }
            
            /// <summary>
            /// 頁碼
            /// </summary>
            public int PageNumber { get; set; }
            
            /// <summary>
            /// 每頁大小
            /// </summary>
            public int PageSize { get; set; }
            
            /// <summary>
            /// 分頁結果
            /// </summary>
            public PagedResult<UserEVoucherReadModel> PagedResults { get; set; } = new();
            
            /// <summary>
            /// 統計數據
            /// </summary>
            public EVoucherStatistics Statistics { get; set; } = new();
        }

        /// <summary>
        /// 發放電子禮券視圖模型
        /// </summary>
        public class GrantEVouchersViewModel
        {
            /// <summary>
            /// 用戶ID
            /// </summary>
            [Required(ErrorMessage = "請選擇用戶")]
            public int UserId { get; set; }
            
            /// <summary>
            /// 電子禮券類型ID
            /// </summary>
            [Required(ErrorMessage = "請選擇電子禮券類型")]
            public int EVoucherTypeId { get; set; }
            
            /// <summary>
            /// 發放數量
            /// </summary>
            [Required(ErrorMessage = "請輸入發放數量")]
            [Range(1, 100, ErrorMessage = "發放數量必須在1-100之間")]
            public int Quantity { get; set; }
            
            /// <summary>
            /// 發放原因
            /// </summary>
            [Required(ErrorMessage = "請輸入發放原因")]
            [StringLength(500, ErrorMessage = "發放原因不能超過500字")]
            public string Reason { get; set; } = string.Empty;
            
            /// <summary>
            /// 用戶列表
            /// </summary>
            public List<User> Users { get; set; } = new();
            
            /// <summary>
            /// 電子禮券類型列表
            /// </summary>
            public List<EvoucherType> EVoucherTypes { get; set; } = new();
            
            /// <summary>
            /// 發放說明
            /// </summary>
            [StringLength(200, ErrorMessage = "發放說明不能超過200字")]
            public string? Description { get; set; }
        }

        /// <summary>
        /// 管理員簽到首頁視圖模型
        /// </summary>
        public class AdminSignInIndexViewModel
        {
            /// <summary>
            /// 簽到記錄列表
            /// </summary>
            public List<UserSignInStat> SignInRecords { get; set; } = new();
            
            /// <summary>
            /// 用戶列表
            /// </summary>
            public List<User> Users { get; set; } = new();
            
            /// <summary>
            /// 查詢條件
            /// </summary>
            public SignInQueryModel Query { get; set; } = new();
            
            /// <summary>
            /// 總數量
            /// </summary>
            public int TotalCount { get; set; }
            
            /// <summary>
            /// 頁碼
            /// </summary>
            public int PageNumber { get; set; }
            
            /// <summary>
            /// 每頁大小
            /// </summary>
            public int PageSize { get; set; }
            
            /// <summary>
            /// 分頁結果
            /// </summary>
            public PagedResult<UserSignInStat> PagedResults { get; set; } = new();
            
            /// <summary>
            /// 統計數據
            /// </summary>
            public SignInStatisticsReadModel Statistics { get; set; } = new();
        }

        /// <summary>
        /// 管理員小遊戲首頁視圖模型
        /// </summary>
        public class AdminMiniGameIndexViewModel
        {
            /// <summary>
            /// 小遊戲記錄列表
            /// </summary>
            public List<MiniGame> GameRecords { get; set; } = new();
            
            /// <summary>
            /// 用戶列表
            /// </summary>
            public List<User> Users { get; set; } = new();
            
            /// <summary>
            /// 查詢條件
            /// </summary>
            public MiniGameQueryModel Query { get; set; } = new();
            
            /// <summary>
            /// 總數量
            /// </summary>
            public int TotalCount { get; set; }
            
            /// <summary>
            /// 頁碼
            /// </summary>
            public int PageNumber { get; set; }
            
            /// <summary>
            /// 每頁大小
            /// </summary>
            public int PageSize { get; set; }
            
            /// <summary>
            /// 分頁結果
            /// </summary>
            public PagedResult<MiniGame> PagedResults { get; set; } = new();
            
            /// <summary>
            /// 統計數據
            /// </summary>
            public MiniGameStatisticsReadModel Statistics { get; set; } = new();
        }

        /// <summary>
        /// 管理員寵物首頁視圖模型
        /// </summary>
        public class AdminPetIndexViewModel
        {
            /// <summary>
            /// 寵物列表
            /// </summary>
            public List<Pet> Pets { get; set; } = new();
            
            /// <summary>
            /// 用戶列表
            /// </summary>
            public List<User> Users { get; set; } = new();
            
            /// <summary>
            /// 查詢條件
            /// </summary>
            public PetQueryModel Query { get; set; } = new();
            
            /// <summary>
            /// 總數量
            /// </summary>
            public int TotalCount { get; set; }
            
            /// <summary>
            /// 頁碼
            /// </summary>
            public int PageNumber { get; set; }
            
            /// <summary>
            /// 每頁大小
            /// </summary>
            public int PageSize { get; set; }
            
            /// <summary>
            /// 分頁結果
            /// </summary>
            public PagedResult<Pet> PagedResults { get; set; } = new();
            
            /// <summary>
            /// 統計數據
            /// </summary>
            public PetStatisticsReadModel Statistics { get; set; } = new();
        }

        /// <summary>
        /// 管理員寵物編輯視圖模型
        /// </summary>
        public class AdminPetEditViewModel
        {
            /// <summary>
            /// 寵物資料
            /// </summary>
            public Pet Pet { get; set; } = new();
            
            /// <summary>
            /// 編輯模型
            /// </summary>
            public PetEditModel EditModel { get; set; } = new();
            
            /// <summary>
            /// 可用膚色選項
            /// </summary>
            public List<PetSkinOption> AvailableSkins { get; set; } = new();
            
            /// <summary>
            /// 可用背景選項
            /// </summary>
            public List<PetBackgroundOption> AvailableBackgrounds { get; set; } = new();
        }

        /// <summary>
        /// 管理員寵物規則視圖模型
        /// </summary>
        public class AdminPetRulesViewModel
        {
            /// <summary>
            /// 寵物規則
            /// </summary>
            public PetRuleReadModel PetRule { get; set; } = new();
            
            /// <summary>
            /// 更新模型
            /// </summary>
            public PetRulesUpdateModel UpdateModel { get; set; } = new();
            
            /// <summary>
            /// 規則歷史
            /// </summary>
            public List<PetRule> RuleHistory { get; set; } = new();
            
            /// <summary>
            /// 規則統計
            /// </summary>
            public PetRuleStatistics RuleStatistics { get; set; } = new();
        }

        /// <summary>
        /// 管理員小遊戲規則視圖模型
        /// </summary>
        public class AdminMiniGameRulesViewModel
        {
            /// <summary>
            /// 小遊戲規則
            /// </summary>
            public GameRuleReadModel GameRule { get; set; } = new();
            
            /// <summary>
            /// 更新模型
            /// </summary>
            public MiniGameRulesUpdateModel UpdateModel { get; set; } = new();
            
            /// <summary>
            /// 規則歷史
            /// </summary>
            public List<GameRule> RuleHistory { get; set; } = new();
            
            /// <summary>
            /// 規則統計
            /// </summary>
            public GameRuleStatistics RuleStatistics { get; set; } = new();
        }

        /// <summary>
        /// 管理員簽到規則視圖模型
        /// </summary>
        public class AdminSignInRulesViewModel
        {
            /// <summary>
            /// 簽到規則
            /// </summary>
            public SignInRuleReadModel SignInRule { get; set; } = new();
            
            /// <summary>
            /// 更新模型
            /// </summary>
            public SignInRulesUpdateModel UpdateModel { get; set; } = new();
            
            /// <summary>
            /// 規則歷史
            /// </summary>
            public List<SignInRule> RuleHistory { get; set; } = new();
            
            /// <summary>
            /// 規則統計
            /// </summary>
            public SignInRuleStatistics RuleStatistics { get; set; } = new();
        }
    }
}
