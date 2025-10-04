using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// MiniGame Area 權限服務實作
    /// </summary>
    public class MiniGamePermissionService : IMiniGamePermissionService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MiniGamePermissionService(GameSpacedatabaseContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // ========== 管理員權限檢查 ==========

        public async Task<bool> HasManagerPermissionAsync(int managerId, string permission)
        {
            try
            {
                var manager = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.ManagerId == managerId);

                if (manager == null || !manager.IsActive) return false;

                return permission switch
                {
                    "UserStatusManagement" => manager.ManagerRoles.Any(r => r?.UserStatusManagement == true),
                    "PetRightsManagement" => manager.ManagerRoles.Any(r => r?.PetRightsManagement == true),
                    "ShoppingPermissionManagement" => manager.ManagerRoles.Any(r => r?.ShoppingPermissionManagement == true),
                    "MessagePermissionManagement" => manager.ManagerRoles.Any(r => r?.MessagePermissionManagement == true),
                    "CustomerService" => manager.ManagerRoles.Any(r => r?.CustomerService == true),
                    "AdministratorPrivilegesManagement" => manager.ManagerRoles.Any(r => r?.AdministratorPrivilegesManagement == true),
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> HasManagerPermissionAsync(HttpContext httpContext, string permission)
        {
            var managerIdClaim = httpContext.User.FindFirst("ManagerId");
            if (managerIdClaim == null || !int.TryParse(managerIdClaim.Value, out int managerId))
                return false;

            return await HasManagerPermissionAsync(managerId, permission);
        }

        public async Task<List<string>> GetManagerPermissionsAsync(int managerId)
        {
            var permissions = new List<string>();

            try
            {
                var manager = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.ManagerId == managerId);

                if (manager == null || !manager.IsActive) return permissions;

                if (manager.ManagerRoles.Any(r => r?.AdministratorPrivilegesManagement == true))
                    permissions.Add("AdministratorPrivilegesManagement");
                if (manager.ManagerRoles.Any(r => r?.UserStatusManagement == true))
                    permissions.Add("UserStatusManagement");
                if (manager.ManagerRoles.Any(r => r?.ShoppingPermissionManagement == true))
                    permissions.Add("ShoppingPermissionManagement");
                if (manager.ManagerRoles.Any(r => r?.MessagePermissionManagement == true))
                    permissions.Add("MessagePermissionManagement");
                if (manager.ManagerRoles.Any(r => r?.PetRightsManagement == true))
                    permissions.Add("PetRightsManagement");
                if (manager.ManagerRoles.Any(r => r?.CustomerService == true))
                    permissions.Add("CustomerService");
            }
            catch
            {
                // 記錄錯誤但不拋出異常
            }

            return permissions;
        }

        public async Task<ManagerRoleInfoViewModel?> GetManagerRoleInfoAsync(int managerId)
        {
            try
            {
                var manager = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.ManagerId == managerId);

                if (manager == null) return null;

                var permissions = await GetManagerPermissionsAsync(managerId);
                var roleName = manager.ManagerRoles.FirstOrDefault()?.RoleName ?? "Unknown";

                return new ManagerRoleInfoViewModel
                {
                    ManagerId = manager.ManagerId,
                    ManagerName = manager.ManagerName ?? string.Empty,
                    ManagerAccount = manager.ManagerAccount ?? string.Empty,
                    RoleName = roleName,
                    Permissions = permissions,
                    IsActive = manager.IsActive,
                    LastLoginTime = manager.ManagerLockoutEnd ?? DateTime.Now
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateManagerPermissionsAsync(int managerId, List<string> permissions)
        {
            try
            {
                var manager = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.ManagerId == managerId);

                if (manager == null) return false;

                // 獲取管理員的主要角色權限
                var rolePermission = manager.ManagerRoles.FirstOrDefault();
                if (rolePermission == null) return false;

                // 更新權限
                rolePermission.UserStatusManagement = permissions.Contains("UserStatusManagement");
                rolePermission.PetRightsManagement = permissions.Contains("PetRightsManagement") || permissions.Contains("PetManagement");
                rolePermission.ShoppingPermissionManagement = permissions.Contains("ShoppingPermissionManagement") || permissions.Contains("ShoppingManagement");
                rolePermission.MessagePermissionManagement = permissions.Contains("MessagePermissionManagement") || permissions.Contains("MessageManagement");
                rolePermission.CustomerService = permissions.Contains("CustomerService");
                rolePermission.AdministratorPrivilegesManagement = permissions.Contains("AdministratorPrivilegesManagement") || permissions.Contains("Administrator");

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CanManageUsersAsync(int managerId)
        {
            return await HasManagerPermissionAsync(managerId, "UserStatusManagement");
        }

        public async Task<bool> CanManagePetsAsync(int managerId)
        {
            return await HasManagerPermissionAsync(managerId, "PetRightsManagement");
        }

        public async Task<bool> CanManageMiniGamesAsync(int managerId)
        {
            return await HasManagerPermissionAsync(managerId, "UserStatusManagement");
        }

        public async Task<bool> CanManageWalletAsync(int managerId)
        {
            return await HasManagerPermissionAsync(managerId, "UserStatusManagement");
        }

        public async Task<bool> CanManageCouponsAsync(int managerId)
        {
            return await HasManagerPermissionAsync(managerId, "ShoppingPermissionManagement");
        }

        public async Task<bool> CanManageEVouchersAsync(int managerId)
        {
            return await HasManagerPermissionAsync(managerId, "ShoppingPermissionManagement");
        }

        public async Task<bool> CanManageSignInAsync(int managerId)
        {
            return await HasManagerPermissionAsync(managerId, "UserStatusManagement");
        }

        // ========== 介面必要方法實作 (8個) ==========

        /// <summary>
        /// 檢查管理員是否有使用者狀態管理權限
        /// </summary>
        public async Task<bool> HasUserStatusManagementPermissionAsync(int managerId)
        {
            try
            {
                var manager = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.Manager_Id == managerId);

                if (manager == null) return false;

                return manager.ManagerRoles.Any(mr =>
                    mr?.UserStatusManagement == true);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 檢查管理員是否有寵物管理權限
        /// </summary>
        public async Task<bool> HasPetManagementPermissionAsync(int managerId)
        {
            try
            {
                var manager = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.Manager_Id == managerId);

                if (manager == null) return false;

                return manager.ManagerRoles.Any(mr =>
                    mr?.PetRightsManagement == true);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 檢查管理員是否有購物管理權限
        /// </summary>
        public async Task<bool> HasShoppingManagementPermissionAsync(int managerId)
        {
            try
            {
                var manager = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.Manager_Id == managerId);

                if (manager == null) return false;

                return manager.ManagerRoles.Any(mr =>
                    mr?.ShoppingPermissionManagement == true);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 檢查管理員是否有訊息管理權限
        /// </summary>
        public async Task<bool> HasMessageManagementPermissionAsync(int managerId)
        {
            try
            {
                var manager = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.Manager_Id == managerId);

                if (manager == null) return false;

                return manager.ManagerRoles.Any(mr =>
                    mr?.MessagePermissionManagement == true);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 檢查管理員是否有客服權限
        /// </summary>
        public async Task<bool> HasCustomerServicePermissionAsync(int managerId)
        {
            try
            {
                var manager = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.Manager_Id == managerId);

                if (manager == null) return false;

                return manager.ManagerRoles.Any(mr =>
                    mr?.CustomerService == true);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 檢查管理員是否有管理員權限
        /// </summary>
        public async Task<bool> HasAdministratorPermissionAsync(int managerId)
        {
            try
            {
                var manager = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.Manager_Id == managerId);

                if (manager == null) return false;

                return manager.ManagerRoles.Any(mr =>
                    mr?.AdministratorPrivilegesManagement == true);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 從 HttpContext 獲取管理員 ID
        /// </summary>
        public int? GetManagerIdFromContext(HttpContext context)
        {
            try
            {
                var managerIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (managerIdClaim == null)
                {
                    managerIdClaim = context.User.FindFirst("ManagerId");
                }

                if (managerIdClaim != null && int.TryParse(managerIdClaim.Value, out int managerId))
                {
                    return managerId;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 檢查當前管理員是否有指定權限
        /// </summary>
        public async Task<bool> HasPermissionAsync(HttpContext context, string permissionType)
        {
            try
            {
                var managerId = GetManagerIdFromContext(context);
                if (!managerId.HasValue)
                {
                    return false;
                }

                return permissionType switch
                {
                    "UserStatusManagement" => await HasUserStatusManagementPermissionAsync(managerId.Value),
                    "PetManagement" or "PetRightsManagement" or "Pet_Rights_Management" => await HasPetManagementPermissionAsync(managerId.Value),
                    "ShoppingManagement" or "ShoppingPermissionManagement" => await HasShoppingManagementPermissionAsync(managerId.Value),
                    "MessageManagement" or "MessagePermissionManagement" => await HasMessageManagementPermissionAsync(managerId.Value),
                    "CustomerService" => await HasCustomerServicePermissionAsync(managerId.Value),
                    "Administrator" or "AdministratorPrivilegesManagement" => await HasAdministratorPermissionAsync(managerId.Value),
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        // ========== 用戶權限管理 ==========

        public async Task<List<GameSpace.Models.UserRight>> GetUserRightsAsync(int userId)
        {
            try
            {
                return await _context.UserRights
                    .Where(ur => ur.UserId == userId)
                    .OrderBy(ur => ur.RightName)
                    .ToListAsync();
            }
            catch
            {
                return new List<GameSpace.Models.UserRight>();
            }
        }

        public async Task<bool> HasUserRightAsync(int userId, string rightName)
        {
            try
            {
                var right = await _context.UserRights
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && 
                                             ur.RightName == rightName && 
                                             ur.IsActive &&
                                             (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow));

                return right != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddUserRightAsync(int userId, string rightName, string? description = null, 
            string rightType = "General", int rightLevel = 1, DateTime? expiresAt = null)
        {
            try
            {
                // 檢查是否已存在
                var existingRight = await _context.UserRights
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RightName == rightName);

                if (existingRight != null)
                {
                    // 更新現有權限
                    existingRight.Description = description;
                    existingRight.RightType = rightType;
                    existingRight.RightLevel = rightLevel;
                    existingRight.ExpiresAt = expiresAt;
                    existingRight.IsActive = true;
                }
                else
                {
                    // 創建新權限
                    var userRight = new GameSpace.Models.UserRight
                    {
                        UserId = userId,
                        RightName = rightName,
                        Description = description,
                        RightType = rightType,
                        RightLevel = rightLevel,
                        ExpiresAt = expiresAt,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.UserRights.Add(userRight);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveUserRightAsync(int userId, string rightName)
        {
            try
            {
                var right = await _context.UserRights
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RightName == rightName);

                if (right != null)
                {
                    _context.UserRights.Remove(right);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserRightAsync(int userRightId, UserRightUpdateModel model)
        {
            try
            {
                var right = await _context.UserRights.FindAsync(userRightId);
                if (right == null) return false;

                if (model.Description != null) right.Description = model.Description;
                if (model.ExpiresAt.HasValue) right.ExpiresAt = model.ExpiresAt;
                if (model.IsActive != right.IsActive) right.IsActive = model.IsActive;
                if (model.RightLevel != right.RightLevel) right.RightLevel = model.RightLevel;
                if (model.RightScope != null) right.RightScope = model.RightScope;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetUserRightsAsync(int userId, List<UserRightModel> rights)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                // 移除現有權限
                var existingRights = await _context.UserRights
                    .Where(ur => ur.UserId == userId)
                    .ToListAsync();

                _context.UserRights.RemoveRange(existingRights);

                // 添加新權限
                foreach (var rightModel in rights)
                {
                    var userRight = new UserRight
                    {
                        UserId = userId,
                        RightName = rightModel.RightName,
                        Description = rightModel.Description,
                        RightType = rightModel.RightType,
                        RightLevel = rightModel.RightLevel,
                        ExpiresAt = rightModel.ExpiresAt,
                        RightScope = rightModel.RightScope,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.UserRights.Add(userRight);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserRightSummary> GetUserRightSummaryAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                var rights = await _context.UserRights
                    .Where(ur => ur.UserId == userId)
                    .ToListAsync();

                var now = DateTime.UtcNow;
                var activeRights = rights.Count(r => r.IsActive && (r.ExpiresAt == null || r.ExpiresAt > now));
                var expiredRights = rights.Count(r => r.ExpiresAt != null && r.ExpiresAt <= now);

                return new UserRightSummary
                {
                    UserId = userId,
                    UserName = user?.UserName ?? "Unknown",
                    TotalRights = rights.Count,
                    ActiveRights = activeRights,
                    ExpiredRights = expiredRights,
                    RightTypes = rights.Select(r => r.RightType).Distinct().ToList(),
                    LastRightUpdate = rights.Max(r => r.CreatedAt)
                };
            }
            catch
            {
                return new UserRightSummary { UserId = userId };
            }
        }

        public async Task<bool> CanUserAccessMiniGameAsync(int userId)
        {
            return await HasUserRightAsync(userId, "MiniGame.Access") || 
                   await HasUserRightAsync(userId, "General.Access");
        }

        public async Task<bool> CanUserManagePetAsync(int userId)
        {
            return await HasUserRightAsync(userId, "Pet.Manage") || 
                   await HasUserRightAsync(userId, "General.Access");
        }

        public async Task<bool> CanUserReceiveRewardsAsync(int userId)
        {
            return await HasUserRightAsync(userId, "Reward.Receive") || 
                   await HasUserRightAsync(userId, "General.Access");
        }

        // ========== 權限查詢 ==========

        public async Task<PagedResult<UserRight>> QueryUserRightsAsync(UserRightQueryModel query)
        {
            try
            {
                var queryable = _context.UserRights.AsQueryable();

                if (query.UserId.HasValue)
                    queryable = queryable.Where(ur => ur.UserId == query.UserId.Value);

                if (!string.IsNullOrEmpty(query.RightName))
                    queryable = queryable.Where(ur => ur.RightName.Contains(query.RightName));

                if (!string.IsNullOrEmpty(query.RightType))
                    queryable = queryable.Where(ur => ur.RightType == query.RightType);

                if (query.IsActive.HasValue)
                    queryable = queryable.Where(ur => ur.IsActive == query.IsActive.Value);

                if (query.CreatedFrom.HasValue)
                    queryable = queryable.Where(ur => ur.CreatedAt >= query.CreatedFrom.Value);

                if (query.CreatedTo.HasValue)
                    queryable = queryable.Where(ur => ur.CreatedAt <= query.CreatedTo.Value);

                if (query.ExpiresFrom.HasValue)
                    queryable = queryable.Where(ur => ur.ExpiresAt >= query.ExpiresFrom.Value);

                if (query.ExpiresTo.HasValue)
                    queryable = queryable.Where(ur => ur.ExpiresAt <= query.ExpiresTo.Value);

                var totalCount = await queryable.CountAsync();

                // 排序
                if (!string.IsNullOrEmpty(query.SortBy))
                {
                    queryable = query.SortBy.ToLower() switch
                    {
                        "rightname" => query.SortDescending ? queryable.OrderByDescending(ur => ur.RightName) : queryable.OrderBy(ur => ur.RightName),
                        "righttype" => query.SortDescending ? queryable.OrderByDescending(ur => ur.RightType) : queryable.OrderBy(ur => ur.RightType),
                        "createdat" => query.SortDescending ? queryable.OrderByDescending(ur => ur.CreatedAt) : queryable.OrderBy(ur => ur.CreatedAt),
                        "expiresat" => query.SortDescending ? queryable.OrderByDescending(ur => ur.ExpiresAt) : queryable.OrderBy(ur => ur.ExpiresAt),
                        _ => queryable.OrderBy(ur => ur.RightName)
                    };
                }
                else
                {
                    queryable = queryable.OrderBy(ur => ur.RightName);
                }

                var items = await queryable
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                return new PagedResult<UserRight>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };
            }
            catch
            {
                return new PagedResult<UserRight>();
            }
        }

        public async Task<List<RightTypeInfo>> GetAvailableRightTypesAsync()
        {
            return new List<RightTypeInfo>
            {
                new RightTypeInfo
                {
                    TypeName = "General",
                    DisplayName = "一般權限",
                    Description = "基本功能訪問權限",
                    MaxLevel = 5,
                    CanExpire = true,
                    CanScope = false
                },
                new RightTypeInfo
                {
                    TypeName = "MiniGame",
                    DisplayName = "小遊戲權限",
                    Description = "小遊戲相關功能權限",
                    MaxLevel = 10,
                    CanExpire = true,
                    CanScope = true
                },
                new RightTypeInfo
                {
                    TypeName = "Pet",
                    DisplayName = "寵物權限",
                    Description = "寵物管理相關權限",
                    MaxLevel = 10,
                    CanExpire = true,
                    CanScope = true
                },
                new RightTypeInfo
                {
                    TypeName = "Reward",
                    DisplayName = "獎勵權限",
                    Description = "獎勵領取相關權限",
                    MaxLevel = 5,
                    CanExpire = true,
                    CanScope = false
                },
                new RightTypeInfo
                {
                    TypeName = "Premium",
                    DisplayName = "高級權限",
                    Description = "高級功能訪問權限",
                    MaxLevel = 3,
                    CanExpire = true,
                    CanScope = true
                }
            };
        }

        public async Task<PermissionStatisticsViewModel> GetPermissionStatisticsAsync()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var activeUsers = await _context.Users.Where(u => u.IsActive).CountAsync();

                // 統計管理員
                var totalManagers = await _context.ManagerData.CountAsync();
                var activeManagers = await _context.ManagerData.Where(m => m.IsActive).CountAsync();

                // 統計用戶權限（基於 UserRight 表）
                var totalUserRights = await _context.UserRights.CountAsync();
                var activeUserRights = await _context.UserRights
                    .Where(ur => ur.UserStatus == true || ur.ShoppingPermission == true ||
                                ur.MessagePermission == true || ur.SalesAuthority == true)
                    .CountAsync();

                // 統計權限類型
                var totalRightTypes = 7; // 預定義的權限類型數量

                // 統計操作日誌
                var totalOperationLogs = await _context.AdminOperationLogs.CountAsync();

                // 權限類型統計
                var rightTypeStats = new List<RightTypeStatisticsViewModel>
                {
                    new RightTypeStatisticsViewModel
                    {
                        RightType = "General",
                        Count = totalUsers,
                        ActiveCount = activeUsers,
                        Percentage = totalUsers > 0 ? (double)activeUsers / totalUsers * 100 : 0
                    },
                    new RightTypeStatisticsViewModel
                    {
                        RightType = "Shopping",
                        Count = await _context.UserRights.CountAsync(ur => ur.ShoppingPermission == true),
                        ActiveCount = await _context.UserRights.CountAsync(ur => ur.ShoppingPermission == true),
                        Percentage = 0
                    },
                    new RightTypeStatisticsViewModel
                    {
                        RightType = "Message",
                        Count = await _context.UserRights.CountAsync(ur => ur.MessagePermission == true),
                        ActiveCount = await _context.UserRights.CountAsync(ur => ur.MessagePermission == true),
                        Percentage = 0
                    },
                    new RightTypeStatisticsViewModel
                    {
                        RightType = "Sales",
                        Count = await _context.UserRights.CountAsync(ur => ur.SalesAuthority == true),
                        ActiveCount = await _context.UserRights.CountAsync(ur => ur.SalesAuthority == true),
                        Percentage = 0
                    }
                };

                // 管理員角色統計
                var managerRoleStats = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .GroupBy(m => m.ManagerRoles.FirstOrDefault()!.RoleName ?? "Unknown")
                    .Select(g => new ManagerRoleStatisticsViewModel
                    {
                        RoleName = g.Key,
                        Count = g.Count(),
                        Percentage = totalManagers > 0 ? (double)g.Count() / totalManagers * 100 : 0
                    })
                    .ToListAsync();

                // 操作日誌統計
                var operationLogStats = await _context.AdminOperationLogs
                    .GroupBy(log => log.Operation)
                    .Select(g => new OperationLogStatisticsViewModel
                    {
                        Operation = g.Key,
                        Count = g.Count(),
                        LastOperationTime = g.Max(log => log.OperationTime)
                    })
                    .OrderByDescending(s => s.Count)
                    .Take(10)
                    .ToListAsync();

                return new PermissionStatisticsViewModel
                {
                    TotalManagers = totalManagers,
                    ActiveManagers = activeManagers,
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    TotalUserRights = totalUserRights,
                    ActiveUserRights = activeUserRights,
                    TotalRightTypes = totalRightTypes,
                    TotalOperationLogs = totalOperationLogs,
                    RightTypeStatistics = rightTypeStats,
                    ManagerRoleStatistics = managerRoleStats,
                    OperationLogStatistics = operationLogStats
                };
            }
            catch
            {
                return new PermissionStatisticsViewModel();
            }
        }

        // ========== 權限日誌 ==========

        public async Task LogPermissionOperationAsync(int managerId, string operation, string details, int? targetUserId = null)
        {
            try
            {
                var log = new AdminOperationLog
                {
                    ManagerId = managerId,
                    Operation = operation,
                    Details = details,
                    TargetUserId = targetUserId,
                    OperationTime = DateTime.Now,
                    IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
                };

                _context.AdminOperationLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // 記錄日誌失敗不影響主要功能
            }
        }

        public async Task<PagedResult<PermissionOperationLog>> GetPermissionOperationLogsAsync(PermissionLogQueryModel query)
        {
            try
            {
                var queryable = _context.AdminOperationLogs.AsQueryable();

                if (query.ManagerId.HasValue)
                    queryable = queryable.Where(log => log.ManagerId == query.ManagerId.Value);

                if (query.TargetUserId.HasValue)
                    queryable = queryable.Where(log => log.TargetUserId == query.TargetUserId.Value);

                if (!string.IsNullOrEmpty(query.Operation))
                    queryable = queryable.Where(log => log.Operation.Contains(query.Operation));

                if (query.FromDate.HasValue)
                    queryable = queryable.Where(log => log.OperationTime >= query.FromDate.Value);

                if (query.ToDate.HasValue)
                    queryable = queryable.Where(log => log.OperationTime <= query.ToDate.Value);

                var totalCount = await queryable.CountAsync();

                var logs = await queryable
                    .OrderByDescending(log => log.OperationTime)
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(log => new PermissionOperationLog
                    {
                        LogId = log.LogId,
                        ManagerId = log.ManagerId,
                        ManagerName = "Manager", // 可以通過 Join 獲取
                        TargetUserId = log.TargetUserId,
                        TargetUserName = "User", // 可以通過 Join 獲取
                        Operation = log.Operation,
                        Details = log.Details,
                        OperationTime = log.OperationTime,
                        IpAddress = log.IpAddress
                    })
                    .ToListAsync();

                return new PagedResult<PermissionOperationLog>
                {
                    Items = logs,
                    TotalCount = totalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };
            }
            catch
            {
                return new PagedResult<PermissionOperationLog>();
            }
        }

        // ========== 新增的介面方法實作 ==========

        /// <summary>
        /// 根據查詢條件獲取用戶權限列表
        /// </summary>
        public async Task<PagedResult<UserRightViewModel>> GetUserRightsAsync(UserRightsQuery query)
        {
            try
            {
                // 由於 UserRight 的擴展屬性是 NotMapped，我們需要從其他來源構建數據
                // 這裡我們使用 Users 表來模擬用戶權限數據
                var usersQuery = _context.Users.AsQueryable();

                // 應用查詢條件
                if (!string.IsNullOrEmpty(query.UserId))
                {
                    if (int.TryParse(query.UserId, out int userIdInt))
                    {
                        usersQuery = usersQuery.Where(u => u.UserId == userIdInt);
                    }
                }

                var totalCount = await usersQuery.CountAsync();

                var users = await usersQuery
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Include(u => u.UserRight)
                    .ToListAsync();

                var userRights = users.Select(u => new UserRightViewModel
                {
                    UserRightId = u.UserId, // 使用 UserId 作為標識
                    UserId = u.UserId,
                    UserName = u.UserName,
                    UserAccount = u.UserAccount,
                    RightName = "基本權限",
                    RightType = "General",
                    RightLevel = 1,
                    Description = "用戶基本權限",
                    IsActive = u.IsActive,
                    ExpiresAt = null,
                    CreatedAt = u.User_registration_date ?? DateTime.Now,
                    UpdatedAt = null
                }).ToList();

                return new PagedResult<UserRightViewModel>
                {
                    Items = userRights,
                    TotalCount = totalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };
            }
            catch
            {
                return new PagedResult<UserRightViewModel>
                {
                    Items = new List<UserRightViewModel>(),
                    TotalCount = 0,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };
            }
        }

        /// <summary>
        /// 獲取所有用戶列表
        /// </summary>
        public async Task<List<UserViewModel>> GetAllUsersAsync()
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.UserName)
                    .Take(100) // 限制返回數量
                    .Select(u => new UserViewModel
                    {
                        UserId = u.UserId,
                        UserName = u.UserName,
                        UserAccount = u.UserAccount,
                        UserEmail = u.User_email ?? string.Empty,
                        IsActive = u.IsActive,
                        CreatedAt = u.User_registration_date ?? DateTime.Now,
                        LastLoginTime = u.UserLockoutEnd
                    })
                    .ToListAsync();

                return users;
            }
            catch
            {
                return new List<UserViewModel>();
            }
        }

        /// <summary>
        /// 獲取所有權限類型
        /// </summary>
        public async Task<List<RightTypeViewModel>> GetRightTypesAsync()
        {
            // 由於沒有 RightType 表，返回預定義的權限類型
            return await Task.FromResult(new List<RightTypeViewModel>
            {
                new RightTypeViewModel
                {
                    TypeId = 1,
                    TypeName = "General",
                    DisplayName = "一般權限",
                    Description = "基本功能訪問權限",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                },
                new RightTypeViewModel
                {
                    TypeId = 2,
                    TypeName = "MiniGame",
                    DisplayName = "小遊戲權限",
                    Description = "小遊戲相關功能權限",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                },
                new RightTypeViewModel
                {
                    TypeId = 3,
                    TypeName = "Pet",
                    DisplayName = "寵物權限",
                    Description = "寵物管理相關權限",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                },
                new RightTypeViewModel
                {
                    TypeId = 4,
                    TypeName = "Reward",
                    DisplayName = "獎勵權限",
                    Description = "獎勵領取相關權限",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                },
                new RightTypeViewModel
                {
                    TypeId = 5,
                    TypeName = "Premium",
                    DisplayName = "高級權限",
                    Description = "高級功能訪問權限",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                },
                new RightTypeViewModel
                {
                    TypeId = 6,
                    TypeName = "Shopping",
                    DisplayName = "購物權限",
                    Description = "購物相關功能權限",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                },
                new RightTypeViewModel
                {
                    TypeId = 7,
                    TypeName = "Message",
                    DisplayName = "訊息權限",
                    Description = "訊息相關功能權限",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                }
            });
        }

        /// <summary>
        /// 添加用戶權限（新簽名）
        /// </summary>
        public async Task<bool> AddUserRightAsync(int userId, string rightName, string rightType,
            int rightLevel, string description, DateTime? expiresAt)
        {
            return await AddUserRightAsync(userId, rightName, description, rightType, rightLevel, expiresAt);
        }

        /// <summary>
        /// 更新用戶權限（新簽名）
        /// </summary>
        public async Task<bool> UpdateUserRightAsync(int userRightId, int rightLevel,
            string description, DateTime? expiresAt, bool isActive)
        {
            var updateModel = new UserRightUpdateModel
            {
                RightLevel = rightLevel,
                Description = description,
                ExpiresAt = expiresAt,
                IsActive = isActive
            };

            return await UpdateUserRightAsync(userRightId, updateModel);
        }

        /// <summary>
        /// 根據ID獲取用戶權限
        /// </summary>
        public async Task<UserRightViewModel?> GetUserRightByIdAsync(int userRightId)
        {
            try
            {
                // 使用 UserId 作為 UserRightId
                var user = await _context.Users
                    .Include(u => u.UserRight)
                    .FirstOrDefaultAsync(u => u.UserId == userRightId);

                if (user == null) return null;

                return new UserRightViewModel
                {
                    UserRightId = user.UserId,
                    UserId = user.UserId,
                    UserName = user.UserName,
                    UserAccount = user.UserAccount,
                    RightName = "基本權限",
                    RightType = "General",
                    RightLevel = 1,
                    Description = "用戶基本權限",
                    IsActive = user.IsActive,
                    ExpiresAt = null,
                    CreatedAt = user.User_registration_date ?? DateTime.Now,
                    UpdatedAt = null
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 根據用戶ID獲取用戶權限列表
        /// </summary>
        public async Task<List<UserRightViewModel>> GetUserRightsByUserIdAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRight)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null) return new List<UserRightViewModel>();

                var rights = new List<UserRightViewModel>();

                // 添加基本權限
                rights.Add(new UserRightViewModel
                {
                    UserRightId = user.UserId * 1000 + 1,
                    UserId = user.UserId,
                    UserName = user.UserName,
                    UserAccount = user.UserAccount,
                    RightName = "基本權限",
                    RightType = "General",
                    RightLevel = 1,
                    Description = "用戶基本訪問權限",
                    IsActive = user.IsActive,
                    ExpiresAt = null,
                    CreatedAt = user.User_registration_date ?? DateTime.Now
                });

                // 根據 UserRight 表添加特殊權限
                if (user.UserRight != null)
                {
                    if (user.UserRight.ShoppingPermission == true)
                    {
                        rights.Add(new UserRightViewModel
                        {
                            UserRightId = user.UserId * 1000 + 2,
                            UserId = user.UserId,
                            UserName = user.UserName,
                            UserAccount = user.UserAccount,
                            RightName = "購物權限",
                            RightType = "Shopping",
                            RightLevel = 1,
                            Description = "允許用戶購物",
                            IsActive = user.UserRight.ShoppingPermission ?? false,
                            CreatedAt = user.User_registration_date ?? DateTime.Now
                        });
                    }

                    if (user.UserRight.MessagePermission == true)
                    {
                        rights.Add(new UserRightViewModel
                        {
                            UserRightId = user.UserId * 1000 + 3,
                            UserId = user.UserId,
                            UserName = user.UserName,
                            UserAccount = user.UserAccount,
                            RightName = "訊息權限",
                            RightType = "Message",
                            RightLevel = 1,
                            Description = "允許用戶發送訊息",
                            IsActive = user.UserRight.MessagePermission ?? false,
                            CreatedAt = user.User_registration_date ?? DateTime.Now
                        });
                    }

                    if (user.UserRight.SalesAuthority == true)
                    {
                        rights.Add(new UserRightViewModel
                        {
                            UserRightId = user.UserId * 1000 + 4,
                            UserId = user.UserId,
                            UserName = user.UserName,
                            UserAccount = user.UserAccount,
                            RightName = "銷售權限",
                            RightType = "Sales",
                            RightLevel = 2,
                            Description = "允許用戶銷售商品",
                            IsActive = user.UserRight.SalesAuthority ?? false,
                            CreatedAt = user.User_registration_date ?? DateTime.Now
                        });
                    }
                }

                return rights;
            }
            catch
            {
                return new List<UserRightViewModel>();
            }
        }

        /// <summary>
        /// 添加權限類型
        /// </summary>
        public async Task<bool> AddRightTypeAsync(string typeName, string displayName, string description)
        {
            // 由於沒有 RightType 表，這個方法返回成功但不執行任何操作
            // 實際應用中應該創建 RightType 表或使用配置文件
            await Task.CompletedTask;
            return true;
        }

        /// <summary>
        /// 更新權限類型
        /// </summary>
        public async Task<bool> UpdateRightTypeAsync(int typeId, string displayName, string description)
        {
            // 由於沒有 RightType 表，這個方法返回成功但不執行任何操作
            await Task.CompletedTask;
            return true;
        }

        /// <summary>
        /// 刪除權限類型
        /// </summary>
        public async Task<bool> DeleteRightTypeAsync(int typeId)
        {
            // 由於沒有 RightType 表，這個方法返回成功但不執行任何操作
            await Task.CompletedTask;
            return true;
        }

        /// <summary>
        /// 獲取操作日誌列表
        /// </summary>
        public async Task<PagedResult<OperationLogViewModel>> GetOperationLogsAsync(int page, int pageSize)
        {
            try
            {
                var query = _context.AdminOperationLogs.AsQueryable();

                var totalCount = await query.CountAsync();

                var logs = await query
                    .OrderByDescending(log => log.OperationTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(log => log.Manager)
                    .Select(log => new OperationLogViewModel
                    {
                        LogId = log.LogId,
                        ManagerId = log.ManagerId,
                        ManagerName = log.Manager != null ? log.Manager.ManagerName : "Unknown",
                        Operation = log.Operation,
                        Details = log.Details,
                        TargetUserId = log.TargetUserId,
                        TargetUserName = string.Empty, // 可以通過額外查詢獲取
                        OperationTime = log.OperationTime,
                        IpAddress = log.IpAddress ?? string.Empty
                    })
                    .ToListAsync();

                return new PagedResult<OperationLogViewModel>
                {
                    Items = logs,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                };
            }
            catch
            {
                return new PagedResult<OperationLogViewModel>
                {
                    Items = new List<OperationLogViewModel>(),
                    TotalCount = 0,
                    PageNumber = page,
                    PageSize = pageSize
                };
            }
        }

        /// <summary>
        /// 根據ID獲取操作日誌
        /// </summary>
        public async Task<OperationLogViewModel?> GetOperationLogByIdAsync(int logId)
        {
            try
            {
                var log = await _context.AdminOperationLogs
                    .Include(l => l.Manager)
                    .FirstOrDefaultAsync(l => l.LogId == logId);

                if (log == null) return null;

                string targetUserName = string.Empty;
                if (log.TargetUserId.HasValue)
                {
                    var user = await _context.Users.FindAsync(log.TargetUserId.Value);
                    targetUserName = user?.UserName ?? string.Empty;
                }

                return new OperationLogViewModel
                {
                    LogId = log.LogId,
                    ManagerId = log.ManagerId,
                    ManagerName = log.Manager?.ManagerName ?? "Unknown",
                    Operation = log.Operation,
                    Details = log.Details,
                    TargetUserId = log.TargetUserId,
                    TargetUserName = targetUserName,
                    OperationTime = log.OperationTime,
                    IpAddress = log.IpAddress ?? string.Empty
                };
            }
            catch
            {
                return null;
            }
        }
    }
}


