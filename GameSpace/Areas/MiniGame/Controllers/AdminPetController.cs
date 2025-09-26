using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Route("MiniGame/[controller]")]
    public class AdminPetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminPetController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminPetViewModel
            {
                TotalPets = await _context.Pet.CountAsync(),
                ActivePets = await _context.Pet.Where(p => p.Health > 0).CountAsync(),
                HighLevelPets = await _context.Pet.Where(p => p.Level >= 10).CountAsync(),
                PetsNeedingCare = await _context.Pet
                    .Where(p => p.Hunger > 80 || p.Health < 20)
                    .CountAsync()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<JsonResult> GetPetOverview()
        {
            try
            {
                var data = new
                {
                    totalPets = await _context.Pet.CountAsync(),
                    activePets = await _context.Pet.Where(p => p.Health > 0).CountAsync(),
                    averageLevel = await _context.Pet.AverageAsync(p => (double)p.Level),
                    petsNeedingCare = await _context.Pet
                        .Where(p => p.Hunger > 80 || p.Health < 20)
                        .CountAsync(),
                    petData = await GetPetDataAsync()
                };

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetPetData(int page = 1, int pageSize = 50)
        {
            try
            {
                var pets = await _context.Pet
                    .Include(p => p.User)
                    .OrderByDescending(p => p.Level)
                    .ThenByDescending(p => p.Experience)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        petId = p.PetID,
                        petName = p.PetName,
                        userId = p.UserID,
                        userName = p.User.User_name,
                        level = p.Level,
                        experience = p.Experience,
                        hunger = p.Hunger,
                        mood = p.Mood,
                        stamina = p.Stamina,
                        cleanliness = p.Cleanliness,
                        health = p.Health,
                        skinColor = p.SkinColor,
                        backgroundColor = p.BackgroundColor,
                        lastLevelUp = p.LevelUpTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        status = p.Health > 0 ? "健康" : "需要照護"
                    })
                    .ToListAsync();

                return Json(new { success = true, data = pets });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> UpdatePetAttributes(int petId, int hunger, int mood, int stamina, int cleanliness, int health)
        {
            try
            {
                var pet = await _context.Pet.FindAsync(petId);
                if (pet == null)
                {
                    return Json(new { success = false, message = "寵物不存在" });
                }

                // Validate attribute ranges (0-100)
                if (hunger < 0 || hunger > 100 || mood < 0 || mood > 100 || 
                    stamina < 0 || stamina > 100 || cleanliness < 0 || cleanliness > 100 || 
                    health < 0 || health > 100)
                {
                    return Json(new { success = false, message = "屬性值必須在 0-100 之間" });
                }

                pet.Hunger = hunger;
                pet.Mood = mood;
                pet.Stamina = stamina;
                pet.Cleanliness = cleanliness;
                pet.Health = health;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物屬性更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> AdjustPetLevel(int petId, int newLevel)
        {
            try
            {
                var pet = await _context.Pet.Include(p => p.User).FirstOrDefaultAsync(p => p.PetID == petId);
                if (pet == null)
                {
                    return Json(new { success = false, message = "寵物不存在" });
                }

                if (newLevel < 1 || newLevel > 100)
                {
                    return Json(new { success = false, message = "等級必須在 1-100 之間" });
                }

                var oldLevel = pet.Level;
                pet.Level = newLevel;
                pet.LevelUpTime = DateTime.UtcNow;

                // Award points for level up if increasing
                if (newLevel > oldLevel)
                {
                    var pointsAwarded = (newLevel - oldLevel) * 50; // 50 points per level
                    pet.PointsGained_LevelUp = pointsAwarded;
                    pet.PointsGainedTime_LevelUp = DateTime.UtcNow;

                    // Update user wallet
                    var wallet = await _context.User_Wallet.FirstOrDefaultAsync(w => w.User_Id == pet.UserID);
                    if (wallet != null)
                    {
                        wallet.User_Point += pointsAwarded;

                        // Add wallet history
                        var history = new WalletHistory
                        {
                            UserID = pet.UserID,
                            ChangeType = "Pet",
                            PointsChanged = pointsAwarded,
                            Description = $"寵物 {pet.PetName} 升級獎勵 (Lv.{oldLevel} → Lv.{newLevel})",
                            ChangeTime = DateTime.UtcNow
                        };
                        _context.WalletHistory.Add(history);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"寵物等級調整成功 (Lv.{oldLevel} → Lv.{newLevel})" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetPetDetails(int petId)
        {
            try
            {
                var pet = await _context.Pet
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PetID == petId);

                if (pet == null)
                {
                    return Json(new { success = false, message = "寵物不存在" });
                }

                // Get recent games played by this pet
                var recentGames = await _context.MiniGame
                    .Where(g => g.PetID == petId)
                    .OrderByDescending(g => g.StartTime)
                    .Take(10)
                    .Select(g => new
                    {
                        playId = g.PlayID,
                        level = g.Level,
                        result = g.Result,
                        expGained = g.ExpGained,
                        pointsGained = g.PointsGained,
                        startTime = g.StartTime.ToString("yyyy-MM-dd HH:mm:ss")
                    })
                    .ToListAsync();

                var data = new
                {
                    petId = pet.PetID,
                    petName = pet.PetName,
                    userId = pet.UserID,
                    userName = pet.User.User_name,
                    level = pet.Level,
                    experience = pet.Experience,
                    attributes = new
                    {
                        hunger = pet.Hunger,
                        mood = pet.Mood,
                        stamina = pet.Stamina,
                        cleanliness = pet.Cleanliness,
                        health = pet.Health
                    },
                    appearance = new
                    {
                        skinColor = pet.SkinColor,
                        backgroundColor = pet.BackgroundColor,
                        lastSkinChange = pet.SkinColorChangedTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        lastBackgroundChange = pet.BackgroundColorChangedTime.ToString("yyyy-MM-dd HH:mm:ss")
                    },
                    stats = new
                    {
                        lastLevelUp = pet.LevelUpTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        pointsFromLevelUp = pet.PointsGained_LevelUp,
                        pointsSpentOnSkin = pet.PointsChanged_SkinColor,
                        pointsSpentOnBackground = pet.PointsChanged_BackgroundColor
                    },
                    recentGames = recentGames
                };

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ResetPetAttributes(int petId)
        {
            try
            {
                var pet = await _context.Pet.FindAsync(petId);
                if (pet == null)
                {
                    return Json(new { success = false, message = "寵物不存在" });
                }

                // Reset to healthy defaults
                pet.Hunger = 50;
                pet.Mood = 70;
                pet.Stamina = 80;
                pet.Cleanliness = 90;
                pet.Health = 100;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物屬性已重置為健康狀態" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<List<object>> GetPetDataAsync()
        {
            return await _context.Pet
                .Include(p => p.User)
                .OrderByDescending(p => p.Level)
                .Take(20)
                .Select(p => new
                {
                    petId = p.PetID,
                    petName = p.PetName,
                    userId = p.UserID,
                    userName = p.User.User_name,
                    level = p.Level,
                    experience = p.Experience,
                    hunger = p.Hunger,
                    mood = p.Mood,
                    stamina = p.Stamina,
                    cleanliness = p.Cleanliness,
                    health = p.Health,
                    status = p.Health > 0 ? "健康" : "需要照護"
                })
                .Cast<object>()
                .ToListAsync();
        }
    }
}
