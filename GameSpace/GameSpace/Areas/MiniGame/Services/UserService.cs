using GameSpace.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class UserService : IUserService
    {
        private readonly GameSpacedatabaseContext _context;

        public UserService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // 使用者基本 CRUD
        public async Task<IEnumerable<Users>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 50)
        {
            return await _context.Users
                .OrderByDescending(u => u.User_CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Users?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.User_Id == userId);
        }

        public async Task<Users?> GetUserByAccountAsync(string account)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.User_Account == account);
        }

        public async Task<Users?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.User_Email == email);
        }

        public async Task<bool> CreateUserAsync(Users user)
        {
            try
            {
                user.User_CreatedAt = DateTime.UtcNow;
                user.User_Status = "Active";
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // 自動建立錢包
                var wallet = new User_Wallet
                {
                    User_Id = user.User_Id,
                    User_Point = 0
                };
                _context.User_Wallet.Add(wallet);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(Users user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null) return false;

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 使用者狀態管理
        public async Task<bool> ActivateUserAsync(int userId)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null) return false;

                user.User_Status = "Active";
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null) return false;

                user.User_Status = "Inactive";
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LockUserAsync(int userId, DateTime? lockoutEnd = null)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null) return false;

                user.User_Status = "Locked";
                user.User_LockoutEnd = lockoutEnd ?? DateTime.UtcNow.AddDays(30);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UnlockUserAsync(int userId)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null) return false;

                user.User_Status = "Active";
                user.User_LockoutEnd = null;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 使用者搜尋與篩選
        public async Task<IEnumerable<Users>> SearchUsersAsync(string searchTerm)
        {
            return await _context.Users
                .Where(u => u.User_Account.Contains(searchTerm) ||
                           u.User_Email.Contains(searchTerm) ||
                           u.User_Name.Contains(searchTerm))
                .OrderByDescending(u => u.User_CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Users>> GetActiveUsersAsync()
        {
            return await _context.Users
                .Where(u => u.User_Status == "Active")
                .OrderByDescending(u => u.User_CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Users>> GetInactiveUsersAsync()
        {
            return await _context.Users
                .Where(u => u.User_Status == "Inactive")
                .OrderByDescending(u => u.User_CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Users>> GetLockedUsersAsync()
        {
            return await _context.Users
                .Where(u => u.User_Status == "Locked")
                .OrderByDescending(u => u.User_LockoutEnd)
                .ToListAsync();
        }

        // 使用者統計
        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            return await _context.Users.CountAsync(u => u.User_Status == "Active");
        }

        public async Task<int> GetNewUsersCountAsync(DateTime since)
        {
            return await _context.Users.CountAsync(u => u.User_CreatedAt >= since);
        }

        public async Task<Dictionary<string, int>> GetUserStatsByDateAsync(int days = 30)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var users = await _context.Users
                .Where(u => u.User_CreatedAt >= startDate)
                .GroupBy(u => u.User_CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            return users.ToDictionary(
                x => x.Date.ToString("yyyy-MM-dd"),
                x => x.Count
            );
        }

        // 使用者權限
        public async Task<bool> GrantRightAsync(int userId, string rightName)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null) return false;

                // 檢查是否已有此權限
                var existingRight = await _context.User_Right
                    .FirstOrDefaultAsync(r => r.User_Id == userId && r.User_RightName == rightName);

                if (existingRight != null) return true; // 已有權限

                var right = new User_Right
                {
                    User_Id = userId,
                    User_RightName = rightName,
                    User_GrantedAt = DateTime.UtcNow
                };
                _context.User_Right.Add(right);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RevokeRightAsync(int userId, string rightName)
        {
            try
            {
                var right = await _context.User_Right
                    .FirstOrDefaultAsync(r => r.User_Id == userId && r.User_RightName == rightName);

                if (right == null) return false;

                _context.User_Right.Remove(right);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetUserRightsAsync(int userId)
        {
            return await _context.User_Right
                .Where(r => r.User_Id == userId)
                .Select(r => r.User_RightName)
                .ToListAsync();
        }

        public async Task<bool> HasRightAsync(int userId, string rightName)
        {
            return await _context.User_Right
                .AnyAsync(r => r.User_Id == userId && r.User_RightName == rightName);
        }
    }
}
