using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace GameSpace.Areas.MiniGame.Services
{
    public class ManagerService : IManagerService
    {
        private readonly GameSpacedatabaseContext _context;

        public ManagerService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // Manager 基本 CRUD
        public async Task<IEnumerable<Manager>> GetAllManagersAsync(int pageNumber = 1, int pageSize = 50)
        {
            return await _context.Manager
                .OrderByDescending(m => m.Manager_CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Manager?> GetManagerByIdAsync(int managerId)
        {
            return await _context.Manager
                .FirstOrDefaultAsync(m => m.Manager_Id == managerId);
        }

        public async Task<Manager?> GetManagerByAccountAsync(string account)
        {
            return await _context.Manager
                .FirstOrDefaultAsync(m => m.Manager_Account == account);
        }

        public async Task<Manager?> GetManagerByEmailAsync(string email)
        {
            return await _context.Manager
                .FirstOrDefaultAsync(m => m.Manager_Email == email);
        }

        public async Task<bool> CreateManagerAsync(Manager manager, string password)
        {
            try
            {
                manager.Manager_Password = HashPassword(password);
                manager.Manager_CreatedAt = DateTime.UtcNow;
                manager.Manager_EmailConfirmed = false;
                manager.Manager_LockoutEnabled = true;

                _context.Manager.Add(manager);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateManagerAsync(Manager manager)
        {
            try
            {
                _context.Manager.Update(manager);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteManagerAsync(int managerId)
        {
            try
            {
                var manager = await GetManagerByIdAsync(managerId);
                if (manager == null) return false;

                _context.Manager.Remove(manager);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Manager 認證
        public async Task<Manager?> AuthenticateAsync(string account, string password)
        {
            var manager = await GetManagerByAccountAsync(account);
            if (manager == null) return null;

            // 檢查鎖定狀態
            if (manager.Manager_LockoutEnabled &&
                manager.Manager_LockoutEnd.HasValue &&
                manager.Manager_LockoutEnd.Value > DateTime.UtcNow)
            {
                return null;
            }

            // 驗證密碼
            if (!VerifyPassword(password, manager.Manager_Password))
            {
                // 記錄失敗次數
                manager.Manager_AccessFailedCount++;
                if (manager.Manager_AccessFailedCount >= 5)
                {
                    manager.Manager_LockoutEnd = DateTime.UtcNow.AddMinutes(30);
                }
                await _context.SaveChangesAsync();
                return null;
            }

            // 成功登入，重置失敗次數
            manager.Manager_AccessFailedCount = 0;
            manager.Manager_LockoutEnd = null;
            await _context.SaveChangesAsync();

            return manager;
        }

        public async Task<bool> ChangePasswordAsync(int managerId, string oldPassword, string newPassword)
        {
            try
            {
                var manager = await GetManagerByIdAsync(managerId);
                if (manager == null) return false;

                // 驗證舊密碼
                if (!VerifyPassword(oldPassword, manager.Manager_Password))
                    return false;

                manager.Manager_Password = HashPassword(newPassword);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(int managerId, string newPassword)
        {
            try
            {
                var manager = await GetManagerByIdAsync(managerId);
                if (manager == null) return false;

                manager.Manager_Password = HashPassword(newPassword);
                manager.Manager_AccessFailedCount = 0;
                manager.Manager_LockoutEnd = null;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Manager 狀態管理
        public async Task<bool> LockManagerAsync(int managerId, DateTime? lockoutEnd = null)
        {
            try
            {
                var manager = await GetManagerByIdAsync(managerId);
                if (manager == null) return false;

                manager.Manager_LockoutEnabled = true;
                manager.Manager_LockoutEnd = lockoutEnd ?? DateTime.UtcNow.AddDays(30);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UnlockManagerAsync(int managerId)
        {
            try
            {
                var manager = await GetManagerByIdAsync(managerId);
                if (manager == null) return false;

                manager.Manager_LockoutEnabled = false;
                manager.Manager_LockoutEnd = null;
                manager.Manager_AccessFailedCount = 0;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ConfirmEmailAsync(int managerId)
        {
            try
            {
                var manager = await GetManagerByIdAsync(managerId);
                if (manager == null) return false;

                manager.Manager_EmailConfirmed = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Manager 角色管理
        public async Task<IEnumerable<ManagerRole>> GetManagerRolesAsync(int managerId)
        {
            return await _context.ManagerRole
                .Where(r => r.ManagerId == managerId)
                .ToListAsync();
        }

        public async Task<bool> AssignRoleAsync(int managerId, int roleId)
        {
            try
            {
                var existingRole = await _context.ManagerRole
                    .FirstOrDefaultAsync(r => r.ManagerId == managerId && r.RoleId == roleId);

                if (existingRole != null) return true; // 已有角色

                var role = new ManagerRole
                {
                    ManagerId = managerId,
                    RoleId = roleId,
                    AssignedAt = DateTime.UtcNow
                };
                _context.ManagerRole.Add(role);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveRoleAsync(int managerId, int roleId)
        {
            try
            {
                var role = await _context.ManagerRole
                    .FirstOrDefaultAsync(r => r.ManagerId == managerId && r.RoleId == roleId);

                if (role == null) return false;

                _context.ManagerRole.Remove(role);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetManagerPermissionsAsync(int managerId)
        {
            var roles = await GetManagerRolesAsync(managerId);

            // 簡化版：根據角色返回權限
            var permissions = new List<string>();
            foreach (var role in roles)
            {
                // 實際應從 Role-Permission 表查詢
                // 這裡使用簡化邏輯
                if (role.RoleId == 1) // Admin
                {
                    permissions.AddRange(new[] { "ViewUsers", "EditUsers", "DeleteUsers", "ManageSystem" });
                }
                else if (role.RoleId == 2) // Moderator
                {
                    permissions.AddRange(new[] { "ViewUsers", "EditUsers" });
                }
            }

            return permissions.Distinct();
        }

        public async Task<bool> HasPermissionAsync(int managerId, string permission)
        {
            var permissions = await GetManagerPermissionsAsync(managerId);
            return permissions.Contains(permission);
        }

        // Manager 活動日誌
        public async Task<IEnumerable<ManagerActivityLog>> GetManagerActivitiesAsync(int managerId, int pageNumber = 1, int pageSize = 20)
        {
            // 實際需要自訂活動日誌表
            // 這裡返回空列表作為預設實作
            await Task.CompletedTask;
            return new List<ManagerActivityLog>();
        }

        public async Task<bool> LogActivityAsync(int managerId, string action, string details)
        {
            try
            {
                // 實際需要自訂活動日誌表
                // 範例：
                // var log = new ManagerActivityLog
                // {
                //     ManagerId = managerId,
                //     Action = action,
                //     Details = details,
                //     IpAddress = "0.0.0.0",
                //     CreatedAt = DateTime.UtcNow
                // };
                // _context.ManagerActivityLogs.Add(log);
                // await _context.SaveChangesAsync();

                await Task.CompletedTask;
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Manager 統計
        public async Task<int> GetTotalManagersCountAsync()
        {
            return await _context.Manager.CountAsync();
        }

        public async Task<int> GetActiveManagersCountAsync()
        {
            return await _context.Manager.CountAsync(m =>
                !m.Manager_LockoutEnabled ||
                !m.Manager_LockoutEnd.HasValue ||
                m.Manager_LockoutEnd.Value <= DateTime.UtcNow);
        }

        public async Task<Dictionary<string, int>> GetManagerStatsByRoleAsync()
        {
            var roleStats = await _context.ManagerRole
                .GroupBy(r => r.RoleId)
                .Select(g => new { RoleId = g.Key, Count = g.Count() })
                .ToListAsync();

            return roleStats.ToDictionary(
                x => $"Role_{x.RoleId}",
                x => x.Count
            );
        }

        // 密碼處理輔助方法
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }
    }
}
