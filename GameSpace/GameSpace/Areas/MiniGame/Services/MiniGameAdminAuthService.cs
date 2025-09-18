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
                .Include(m => m.ManagerRole)
                .ThenInclude(r => r.ManagerRolePermissions)
                .FirstOrDefaultAsync(m => m.ManagerId == managerId);

            if (manager?.ManagerRole?.ManagerRolePermissions == null)
                return false;

            return manager.ManagerRole.ManagerRolePermissions
                .Any(p => p.PermissionName == permission);
        }

        public async Task<List<ManagerRolePermission>> GetManagerPermissionsAsync(int managerId)
        {
            var manager = await _context.ManagerData
                .Include(m => m.ManagerRole)
                .ThenInclude(r => r.ManagerRolePermissions)
                .FirstOrDefaultAsync(m => m.ManagerId == managerId);

            return manager?.ManagerRole?.ManagerRolePermissions ?? new List<ManagerRolePermission>();
        }

        public async Task<ManagerDatum?> GetManagerDatumAsync(int managerId)
        {
            return await _context.ManagerData
                .Include(m => m.ManagerRole)
                .ThenInclude(r => r.ManagerRolePermissions)
                .FirstOrDefaultAsync(m => m.ManagerId == managerId);
        }
    }
}
