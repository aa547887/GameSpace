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
        /// 管理員權限首頁視圖模型
        /// </summary>
        public class AdminPermissionIndexViewModel
        {
            /// <summary>
            /// 管理員角色信息
            /// </summary>
            public ManagerRoleInfo? ManagerRoleInfo { get; set; }
            
            /// <summary>
            /// 權限統計
            /// </summary>
            public PermissionStatistics Statistics { get; set; } = new();
        }

        /// <summary>
        /// 用戶權限管理首頁視圖模型
        /// </summary>
        public class UserRightsIndexViewModel
        {
            /// <summary>
            /// 用戶權限分頁結果
            /// </summary>
            public PagedResult<UserRight> UserRights { get; set; } = new();
            
            /// <summary>
            /// 用戶列表
            /// </summary>
            public List<User> Users { get; set; } = new();
            
            /// <summary>
            /// 權限類型列表
            /// </summary>
            public List<RightTypeInfo> RightTypes { get; set; } = new();
            
            /// <summary>
            /// 查詢條件
            /// </summary>
            public UserRightQueryModel Query { get; set; } = new();
        }

        /// <summary>
        /// 權限操作日誌視圖模型
        /// </summary>
        public class PermissionLogsIndexViewModel
        {
            /// <summary>
            /// 操作日誌分頁結果
            /// </summary>
            public PagedResult<PermissionOperationLog> Logs { get; set; } = new();
            
            /// <summary>
            /// 管理員列表
            /// </summary>
            public List<Manager> Managers { get; set; } = new();
            
            /// <summary>
            /// 查詢條件
            /// </summary>
            public PermissionLogQueryModel Query { get; set; } = new();
        }
    }
}
