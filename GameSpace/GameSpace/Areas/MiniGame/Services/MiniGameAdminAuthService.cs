using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class MiniGameAdminAuthService : IMiniGameAdminAuthService
    {
        private readonly GameSpacedatabaseContext _context;

        public MiniGameAdminAuthService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        public async Task<bool> HasPermissionAsync(int managerId, string permission)
        {
            var manager = await _context.ManagerData
                .Include(m => m.ManagerRoles)
                .ThenInclude(r => r.ManagerRolePermission)
                .FirstOrDefaultAsync(m => m.ManagerId == managerId);

            if (manager?.ManagerRoles == null)
                return false;

            return permission switch
            {
                "AdministratorPrivilegesManagement" => manager.ManagerRoles.Any(r => r.ManagerRolePermission?.AdministratorPrivilegesManagement == true),
                "UserStatusManagement" => manager.ManagerRoles.Any(r => r.ManagerRolePermission?.UserStatusManagement == true),
                "ShoppingPermissionManagement" => manager.ManagerRoles.Any(r => r.ManagerRolePermission?.ShoppingPermissionManagement == true),
                "MessagePermissionManagement" => manager.ManagerRoles.Any(r => r.ManagerRolePermission?.MessagePermissionManagement == true),
                "PetRightsManagement" => manager.ManagerRoles.Any(r => r.ManagerRolePermission?.PetRightsManagement == true),
                "CustomerService" => manager.ManagerRoles.Any(r => r.ManagerRolePermission?.CustomerService == true),
                _ => false
            };
        }

        public async Task<List<ManagerRolePermission>> GetManagerPermissionsAsync(int managerId)
        {
            var manager = await _context.ManagerData
                .Include(m => m.ManagerRoles)
                .ThenInclude(r => r.ManagerRolePermission)
                .FirstOrDefaultAsync(m => m.ManagerId == managerId);

            return manager?.ManagerRoles?
                .Where(r => r.ManagerRolePermission != null)
                .Select(r => r.ManagerRolePermission!)
                .ToList() ?? new List<ManagerRolePermission>();
        }

        public async Task<ManagerDatum?> GetManagerDatumAsync(int managerId)
        {
            return await _context.ManagerData
                .Include(m => m.ManagerRoles)
                .ThenInclude(r => r.ManagerRolePermission)
                .FirstOrDefaultAsync(m => m.ManagerId == managerId);
        }
    }
}

