using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class MiniGameBaseController : Controller
    {
        protected readonly GameSpacedatabaseContext _context;
        protected readonly IMiniGameAdminService _adminService;

        public MiniGameBaseController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        public MiniGameBaseController(GameSpacedatabaseContext context, IMiniGameAdminService adminService) : this(context)
        {
            _adminService = adminService;
        }

        // 獲取當前用戶ID
        protected int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        // 獲取當前用戶
        protected async Task<GameSpace.Models.User> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return null;

            return await _context.Users.FindAsync(userId);
        }

        // 獲取用戶錢包
        protected async Task<UserWallet> GetUserWalletAsync(int userId)
        {
            return await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        // 獲取用戶寵物
        protected async Task<GameSpace.Models.Pet> GetUserPetAsync(int userId)
        {
            return await _context.Pets
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        // 檢查是否可以玩遊戲
        protected async Task<bool> CanPlayGameAsync(int userId)
        {
            var pet = await GetUserPetAsync(userId);
            if (pet == null) return false;

            // 檢查寵物健康狀態
            return pet.Health > 0 && pet.Hunger > 0 && pet.Happiness > 0 && pet.Energy > 0;
        }

        // 更新用戶點數
        protected async Task UpdateUserPointsAsync(int userId, int points)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var wallet = await GetUserWalletAsync(userId);
                if (wallet == null)
                {
                    wallet = new UserWallet { UserId = userId, UserPoint = 0 };
                    _context.UserWallets.Add(wallet);
                }

                wallet.UserPoint = Math.Max(0, wallet.UserPoint + points);

                // 記錄交易歷史
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Point",
                    PointsChanged = points,
                    Description = points > 0 ? $"獲得 {points} 點" : $"消耗 {Math.Abs(points)} 點",
                    ChangeTime = DateTime.Now
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // 更新寵物經驗
        protected async Task UpdatePetExperienceAsync(int petId, int exp)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pet = await _context.Pets.FindAsync(petId);
                if (pet != null)
                {
                    pet.Experience += exp;
                    
                    // 檢查是否升級
                    var requiredExp = CalculateRequiredExp(pet.Level);
                    if (pet.Experience >= requiredExp)
                    {
                        pet.Level++;
                        pet.Experience -= requiredExp;
                        
                        // 升級獎勵
                        var levelUpReward = pet.Level * 50;
                        await UpdateUserPointsAsync(pet.UserId, levelUpReward);
                    }
                    
                    await _context.SaveChangesAsync();
                }
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // 記錄遊戲遊玩
        protected async Task LogGamePlayAsync(int userId, int miniGameId, bool isWin, int pointsGained, int expGained)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var gameRecord = new MiniGame
                {
                    UserId = userId,
                    PetId = await GetUserPetIdAsync(userId),
                    Level = 1, // 預設關卡等級
                    MonsterCount = 3, // 預設怪物數量
                    SpeedMultiplier = 1.0m, // 預設速度倍率
                    Result = isWin ? "Win" : "Lose",
                    ExpGained = expGained,
                    PointsGained = pointsGained,
                    StartTime = DateTime.Now.AddMinutes(-5), // 假設遊戲進行5分鐘
                    EndTime = DateTime.Now,
                    Aborted = false
                };

                _context.MiniGames.Add(gameRecord);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // 檢查是否為管理員
        protected bool IsAdmin()
        {
            return User.IsInRole("Admin") || User.HasClaim("Role", "Admin");
        }

        // 返回404錯誤
        protected IActionResult Return404()
        {
            return NotFound();
        }

        // 返回JSON成功響應
        protected IActionResult JsonSuccess(object data = null, string message = "操作成功")
        {
            return Json(new { success = true, data = data, message = message });
        }

        // 返回JSON錯誤響應
        protected IActionResult JsonError(string message = "操作失敗")
        {
            return Json(new { success = false, message = message });
        }

        // 記錄錯誤日誌
        protected async Task LogErrorAsync(string message, Exception exception = null)
        {
            try
            {
                var errorLog = new ErrorLog
                {
                    Level = "Error",
                    Message = message,
                    Exception = exception?.ToString(),
                    Timestamp = DateTime.Now,
                    Source = "MiniGameArea"
                };

                _context.ErrorLogs.Add(errorLog);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // 如果記錄錯誤日誌失敗，不拋出異常
            }
        }

        // 記錄資訊日誌
        protected async Task LogInfoAsync(string message)
        {
            try
            {
                var infoLog = new ErrorLog
                {
                    Level = "Information",
                    Message = message,
                    Timestamp = DateTime.Now,
                    Source = "MiniGameArea"
                };

                _context.ErrorLogs.Add(infoLog);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // 如果記錄資訊日誌失敗，不拋出異常
            }
        }

        // 驗證用戶權限
        protected async Task<bool> ValidateUserPermissionAsync(int userId, string permission)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                // 這裡可以添加更複雜的權限驗證邏輯
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 獲取分頁結果
        protected PagedResult<T> CreatePagedResult<T>(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // 計算分頁偏移量
        protected int CalculateSkip(int pageNumber, int pageSize)
        {
            return (pageNumber - 1) * pageSize;
        }

        // 驗證分頁參數
        protected void ValidatePagination(ref int pageNumber, ref int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // 限制最大頁面大小
        }

        // 格式化日期時間
        protected string FormatDateTime(DateTime? dateTime)
        {
            return dateTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
        }

        // 格式化金額
        protected string FormatAmount(decimal amount)
        {
            return amount.ToString("N2");
        }

        // 生成隨機代碼
        protected string GenerateRandomCode(string prefix, int length = 8)
        {
            var random = new Random();
            var code = prefix + random.Next(10000000, 99999999).ToString();
            return code;
        }

        // 檢查字串是否為空或空白
        protected bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        // 安全地轉換字串為整數
        protected int SafeParseInt(string value, int defaultValue = 0)
        {
            if (int.TryParse(value, out int result))
                return result;
            return defaultValue;
        }

        // 安全地轉換字串為小數
        protected decimal SafeParseDecimal(string value, decimal defaultValue = 0)
        {
            if (decimal.TryParse(value, out decimal result))
                return result;
            return defaultValue;
        }

        // 私有方法
        private async Task<int> GetUserPetIdAsync(int userId)
        {
            var pet = await GetUserPetAsync(userId);
            return pet?.PetId ?? 0;
        }

        private int CalculateRequiredExp(int level)
        {
            // 簡單的經驗值計算公式：每級需要 level * 100 經驗
            return level * 100;
        }
    }

    // 分頁結果類
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
