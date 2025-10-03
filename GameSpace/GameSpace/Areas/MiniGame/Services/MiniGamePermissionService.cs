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
                var manager = await _context.Managers
                    .Include(m => m.ManagerRoles)
                    .ThenInclude(mr => mr.ManagerRole)
                    .FirstOrDefaultAsync(m => m.ManagerId == managerId);

                if (manager == null || !manager.IsActive) return false;

                return permission switch
                {
                    "UserStatusManagement" => manager.ManagerRoles.Any(r => r.ManagerRole?.UserStatusManagement == true),
                    "PetRightsManagement" => manager.ManagerRoles.Any(r => r.ManagerRole?.PetRightsManagement == true),
                    "ShoppingPermissionManagement" => manager.ManagerRoles.Any(r => r.ManagerRole?.ShoppingPermissionManagement == true),
                    "MessagePermissionManagement" => manager.ManagerRoles.Any(r => r.ManagerRole?.MessagePermissionManagement == true),
                    "CustomerService" => manager.ManagerRoles.Any(r => r.ManagerRole?.CustomerService == true),
                    "AdministratorPrivilegesManagement" => manager.ManagerRoles.Any(r => r.ManagerRole?.AdministratorPrivilegesManagement == true),
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
                var manager = await _context.Managers
                    .Include(m => m.ManagerRoles)
                    .ThenInclude(mr => mr.ManagerRole)
                    .FirstOrDefaultAsync(m => m.ManagerId == managerId);

                if (manager == null || !manager.IsActive) return permissions;

                if (manager.ManagerRoles.Any(r => r.ManagerRole?.AdministratorPrivilegesManagement == true))
                    permissions.Add("AdministratorPrivilegesManagement");
                if (manager.ManagerRoles.Any(r => r.ManagerRole?.UserStatusManagement == true))
                    permissions.Add("UserStatusManagement");
                if (manager.ManagerRoles.Any(r => r.ManagerRole?.ShoppingPermissionManagement == true))
                    permissions.Add("ShoppingPermissionManagement");
                if (manager.ManagerRoles.Any(r => r.ManagerRole?.MessagePermissionManagement == true))
                    permissions.Add("MessagePermissionManagement");
                if (manager.ManagerRoles.Any(r => r.ManagerRole?.PetRightsManagement == true))
                    permissions.Add("PetRightsManagement");
                if (manager.ManagerRoles.Any(r => r.ManagerRole?.CustomerService == true))
                    permissions.Add("CustomerService");
            }
            catch
            {
                // 記錄錯誤但不拋出異常
            }

            return permissions;
        }

        public async Task<ManagerRoleInfo?> GetManagerRoleInfoAsync(int managerId)
        {
            try
            {
                var manager = await _context.Managers
                    .Include(m => m.ManagerRoles)
                    .ThenInclude(mr => mr.ManagerRole)
                    .FirstOrDefaultAsync(m => m.ManagerId == managerId);

                if (manager == null) return null;

                var permissions = await GetManagerPermissionsAsync(managerId);
                var roleName = manager.ManagerRoles.FirstOrDefault()?.ManagerRole?.RoleName ?? "Unknown";

                return new ManagerRoleInfo
                {
                    ManagerId = manager.ManagerId,
                    ManagerName = manager.ManagerName ?? string.Empty,
                    ManagerAccount = manager.ManagerAccount ?? string.Empty,
                    RoleName = roleName,
                    Permissions = permissions,
                    IsActive = manager.IsActive,
                    LastLoginTime = manager.LastLoginTime
                };
            }
            catch
            {
                return null;
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
                    .ThenInclude(mr => mr.ManagerRolePermission)
                    .FirstOrDefaultAsync(m => m.Manager_Id == managerId);

                if (manager == null) return false;

                return manager.ManagerRoles.Any(mr =>
                    mr.ManagerRolePermission?.UserStatusManagement == true);
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
                    .ThenInclude(mr => mr.ManagerRolePermission)
                    .FirstOrDefaultAsync(m => m.Manager_Id == managerId);

                if (manager == null) return false;

                return manager.ManagerRoles.Any(mr =>
                    mr.ManagerRolePermission?.Pet_Rights_Management == true);
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
                    .ThenInclude(mr => mr.ManagerRolePermission)
                    .FirstOrDefaultAsync(m => m.Manager_Id == managerId);

                if (manager == null) return false;

                return manager.ManagerRoles.Any(mr =>
                    mr.ManagerRolePermission?.ShoppingPermissionManagement == true);
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
                    .ThenInclude(mr => mr.ManagerRolePermission)
                    .FirstOrDefaultAsync(m => m.Manager_Id == managerId);

                if (manager == null) return false;

                return manager.ManagerRoles.Any(mr =>
                    mr.ManagerRolePermission?.MessagePermissionManagement == true);
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
                    .ThenInclude(mr => mr.ManagerRolePermission)
                    .FirstOrDefaultAsync(m => m.Manager_Id == managerId);

                if (manager == null) return false;

                return manager.ManagerRoles.Any(mr =>
                    mr.ManagerRolePermission?.customer_service == true);
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
                    .ThenInclude(mr => mr.ManagerRolePermission)
                    .FirstOrDefaultAsync(m => m.Manager_Id == managerId);

                if (manager == null) return false;

                return manager.ManagerRoles.Any(mr =>
                    mr.ManagerRolePermission?.AdministratorPrivilegesManagement == true);
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
                if (model.IsActive.HasValue) right.IsActive = model.IsActive.Value;
                if (model.RightLevel.HasValue) right.RightLevel = model.RightLevel.Value;
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

        public async Task<PermissionStatistics> GetPermissionStatisticsAsync()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var usersWithRights = await _context.UserRights
                    .Select(ur => ur.UserId)
                    .Distinct()
                    .CountAsync();

                var allRights = await _context.UserRights.ToListAsync();
                var now = DateTime.UtcNow;

                var activeRights = allRights.Count(r => r.IsActive && (r.ExpiresAt == null || r.ExpiresAt > now));
                var expiredRights = allRights.Count(r => r.ExpiresAt != null && r.ExpiresAt <= now);

                var rightsByType = allRights
                    .GroupBy(r => r.RightType)
                    .ToDictionary(g => g.Key, g => g.Count());

                var rightsByLevel = allRights
                    .GroupBy(r => r.RightLevel)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count());

                return new PermissionStatistics
                {
                    TotalUsers = totalUsers,
                    UsersWithRights = usersWithRights,
                    TotalRights = allRights.Count,
                    ActiveRights = activeRights,
                    ExpiredRights = expiredRights,
                    RightsByType = rightsByType,
                    RightsByLevel = rightsByLevel
                };
            }
            catch
            {
                return new PermissionStatistics();
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
                        LogId = log.OperationLogId,
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
    }
}


