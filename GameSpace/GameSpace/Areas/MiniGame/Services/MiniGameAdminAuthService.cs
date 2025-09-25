using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class MiniGameAdminAuthService
    {
        private readonly GameSpacedatabaseContext _context;

        public MiniGameAdminAuthService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        public async Task<bool> HasPermissionAsync(int managerId, string permission)
        {
            var manager = await _context.ManagerDatum
                .Include(m => m.ManagerRoles)
                .ThenInclude(r => r.ManagerRolePermission)
                .FirstOrDefaultAsync(m => m.ManagerId == managerId);

            if (manager?.ManagerRoles == null)
                return false;

            return manager.ManagerRoles
                .Any(r => r.ManagerRolePermission.PermissionName == permission);
        }

        public async Task<List<ManagerRolePermission>> GetManagerPermissionsAsync(int managerId)
        {
            var manager = await _context.ManagerDatum
                .Include(m => m.ManagerRoles)
                .ThenInclude(r => r.ManagerRolePermission)
                .FirstOrDefaultAsync(m => m.ManagerId == managerId);

            return manager?.ManagerRoles?.Select(r => r.ManagerRolePermission).ToList() ?? new List<ManagerRolePermission>();
        }

        public async Task<ManagerDatum?> GetManagerDatumAsync(int managerId)
        {
            return await _context.ManagerDatum
                .Include(m => m.ManagerRoles)
                .ThenInclude(r => r.ManagerRolePermission)
                .FirstOrDefaultAsync(m => m.ManagerId == managerId);
        }
    }
}
