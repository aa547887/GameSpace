using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IUserService
    {
        Task<IEnumerable<Users>> GetAllUsersAsync();
        Task<Users?> GetUserByIdAsync(int userId);
        Task<Users?> GetUserByAccountAsync(string account);
        Task<bool> CreateUserAsync(Users user);
        Task<bool> UpdateUserAsync(Users user);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> UserExistsAsync(int userId);
        Task<bool> UserAccountExistsAsync(string account);
        Task<IEnumerable<Users>> SearchUsersAsync(string searchTerm);
        Task<int> GetTotalUsersCountAsync();
    }
}
