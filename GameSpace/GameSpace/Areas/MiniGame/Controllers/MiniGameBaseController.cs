using Microsoft.AspNetCore.Mvc;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class MiniGameBaseController : Controller
    {
        protected readonly GameSpacedatabaseContext _context;

        public MiniGameBaseController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        protected int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        protected async Task<GameSpace.Models.User> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return null;

            return await _context.Users.FindAsync(userId);
        }

        protected async Task<UserWallet> GetUserWalletAsync(int userId)
        {
            return await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        protected async Task<GameSpace.Models.Pet> GetUserPetAsync(int userId)
        {
            return await _context.Pets
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        protected async Task<bool> CanPlayGameAsync(int userId)
        {
            var pet = await GetUserPetAsync(userId);
            if (pet == null) return false;

            // 檢查寵物健康狀態
            return pet.Health > 0 && pet.Hunger > 0 && pet.Mood > 0 && pet.Stamina > 0;
        }

        protected async Task UpdateUserPointsAsync(int userId, int points)
        {
            var wallet = await GetUserWalletAsync(userId);
            if (wallet != null)
            {
                wallet.UserPoint = Math.Max(0, wallet.UserPoint + points);
                await _context.SaveChangesAsync();

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
            }
        }

        protected async Task UpdatePetExperienceAsync(int petId, int exp)
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
                    pet.LevelUpTime = DateTime.Now;
                    
                    // 升級獎勵
                    var levelUpReward = pet.Level * 50;
                    await UpdateUserPointsAsync(pet.UserId, levelUpReward);
                }
                
                await _context.SaveChangesAsync();
            }
        }

        protected async Task LogGamePlayAsync(int userId, int miniGameId, bool isWin, int pointsGained, int expGained)
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
        }

        protected bool IsAdmin()
        {
            return User.IsInRole("Admin") || User.HasClaim("Role", "Admin");
        }

        protected IActionResult Return404()
        {
            return NotFound();
        }

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
}
