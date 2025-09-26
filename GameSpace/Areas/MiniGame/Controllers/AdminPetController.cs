using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminPetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminPetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MiniGame/AdminPet
        public async Task<IActionResult> Index()
        {
            var viewModel = new PetOverviewViewModel();

            // 統計數據
            viewModel.TotalPets = await _context.Pets.CountAsync();
            viewModel.ActivePets = await _context.Pets
                .Where(p => p.Health > 0)
                .CountAsync();
            viewModel.AverageLevel = await _context.Pets
                .AverageAsync(p => p.Level);
            viewModel.HighestLevel = await _context.Pets
                .MaxAsync(p => p.Level);
            viewModel.TotalExperience = await _context.Pets
                .SumAsync(p => p.Experience);

            // 寵物統計
            viewModel.PetStats = await _context.Pets
                .Include(p => p.User)
                .Include(p => p.User.UserIntroduce)
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.Experience)
                .Select(p => new PetStatsViewModel
                {
                    PetId = p.PetID,
                    UserId = p.UserID,
                    UserName = p.User.User_name,
                    PetName = p.PetName,
                    Level = p.Level,
                    Experience = p.Experience,
                    Hunger = p.Hunger,
                    Mood = p.Mood,
                    Stamina = p.Stamina,
                    Cleanliness = p.Cleanliness,
                    Health = p.Health,
                    SkinColor = p.SkinColor,
                    BackgroundColor = p.BackgroundColor,
                    LastActivity = p.LevelUpTime
                })
                .ToListAsync();

            // 用戶寵物列表
            viewModel.UserPets = await _context.Users
                .Include(u => u.Pet)
                .Include(u => u.UserIntroduce)
                .Where(u => u.Pet != null)
                .OrderByDescending(u => u.Pet.Level)
                .ThenByDescending(u => u.Pet.Experience)
                .Select(u => new UserPetViewModel
                {
                    UserId = u.User_ID,
                    UserName = u.User_name,
                    NickName = u.UserIntroduce.User_NickName,
                    PetId = u.Pet.PetID,
                    PetName = u.Pet.PetName,
                    PetLevel = u.Pet.Level,
                    PetExperience = u.Pet.Experience,
                    PetCreated = u.Pet.LevelUpTime,
                    LastActivity = u.Pet.LevelUpTime
                })
                .ToListAsync();

            return View(viewModel);
        }

        // GET: MiniGame/AdminPet/GetPetOverview
        [HttpGet]
        public async Task<IActionResult> GetPetOverview()
        {
            try
            {
                var data = new
                {
                    totalPets = await _context.Pets.CountAsync(),
                    activePets = await _context.Pets
                        .Where(p => p.Health > 0)
                        .CountAsync(),
                    averageLevel = await _context.Pets
                        .AverageAsync(p => p.Level),
                    highestLevel = await _context.Pets
                        .MaxAsync(p => p.Level),
                    totalExperience = await _context.Pets
                        .SumAsync(p => p.Experience),
                    petStats = await _context.Pets
                        .Include(p => p.User)
                        .Include(p => p.User.UserIntroduce)
                        .OrderByDescending(p => p.Level)
                        .ThenByDescending(p => p.Experience)
                        .Take(50)
                        .Select(p => new
                        {
                            petId = p.PetID,
                            userId = p.UserID,
                            userName = p.User.User_name,
                            petName = p.PetName,
                            level = p.Level,
                            experience = p.Experience,
                            hunger = p.Hunger,
                            mood = p.Mood,
                            stamina = p.Stamina,
                            cleanliness = p.Cleanliness,
                            health = p.Health,
                            skinColor = p.SkinColor,
                            backgroundColor = p.BackgroundColor,
                            lastActivity = p.LevelUpTime.ToString("yyyy-MM-dd HH:mm:ss")
                        })
                        .ToListAsync()
                };

                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminPet/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _context.Pets
                .Include(p => p.User)
                .Include(p => p.User.UserIntroduce)
                .Include(p => p.MiniGames)
                .FirstOrDefaultAsync(m => m.PetID == id);

            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }

        // POST: MiniGame/AdminPet/AdjustPetAttributes
        [HttpPost]
        public async Task<IActionResult> AdjustPetAttributes(int petId, int hunger, int mood, int stamina, int cleanliness, int health)
        {
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.PetID == petId);

                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到寵物" });
                }

                // 調整屬性值
                pet.Hunger = Math.Max(0, Math.Min(100, hunger));
                pet.Mood = Math.Max(0, Math.Min(100, mood));
                pet.Stamina = Math.Max(0, Math.Min(100, stamina));
                pet.Cleanliness = Math.Max(0, Math.Min(100, cleanliness));
                pet.Health = Math.Max(0, Math.Min(100, health));

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物屬性調整成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: MiniGame/AdminPet/LevelUpPet
        [HttpPost]
        public async Task<IActionResult> LevelUpPet(int petId)
        {
            try
            {
                var pet = await _context.Pets
                    .Include(p => p.User)
                    .Include(p => p.User.UserWallet)
                    .FirstOrDefaultAsync(p => p.PetID == petId);

                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到寵物" });
                }

                // 升級寵物
                pet.Level++;
                pet.LevelUpTime = DateTime.Now;
                pet.PointsGained_LevelUp = 50;
                pet.PointsGainedTime_LevelUp = DateTime.Now;

                // 給用戶獎勵點數
                pet.User.UserWallet.User_Point += 50;

                // 記錄錢包異動
                var walletHistory = new WalletHistory
                {
                    UserID = pet.UserID,
                    ChangeType = "寵物升級獎勵",
                    PointsChanged = 50,
                    Description = $"寵物 {pet.PetName} 升級到 {pet.Level} 級",
                    ChangeTime = DateTime.Now
                };

                _context.WalletHistories.Add(walletHistory);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物升級成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: MiniGame/AdminPet/AddExperience
        [HttpPost]
        public async Task<IActionResult> AddExperience(int petId, int experience)
        {
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.PetID == petId);

                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到寵物" });
                }

                pet.Experience += experience;

                // 檢查是否升級
                var requiredExp = CalculateRequiredExp(pet.Level);
                if (pet.Experience >= requiredExp)
                {
                    pet.Level++;
                    pet.Experience -= requiredExp;
                    pet.LevelUpTime = DateTime.Now;
                    pet.PointsGained_LevelUp = 50;
                    pet.PointsGainedTime_LevelUp = DateTime.Now;

                    // 給用戶獎勵點數
                    var user = await _context.Users
                        .Include(u => u.UserWallet)
                        .FirstOrDefaultAsync(u => u.User_ID == pet.UserID);

                    if (user != null)
                    {
                        user.UserWallet.User_Point += 50;

                        var walletHistory = new WalletHistory
                        {
                            UserID = pet.UserID,
                            ChangeType = "寵物升級獎勵",
                            PointsChanged = 50,
                            Description = $"寵物 {pet.PetName} 升級到 {pet.Level} 級",
                            ChangeTime = DateTime.Now
                        };

                        _context.WalletHistories.Add(walletHistory);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "經驗值增加成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: MiniGame/AdminPet/ChangePetSkin
        [HttpPost]
        public async Task<IActionResult> ChangePetSkin(int petId, string skinColor, string backgroundColor)
        {
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.PetID == petId);

                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到寵物" });
                }

                pet.SkinColor = skinColor;
                pet.SkinColorChangedTime = DateTime.Now;
                pet.BackgroundColor = backgroundColor;
                pet.BackgroundColorChangedTime = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物外觀更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: MiniGame/AdminPet/ResetPet
        [HttpPost]
        public async Task<IActionResult> ResetPet(int petId)
        {
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.PetID == petId);

                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到寵物" });
                }

                // 重置寵物屬性
                pet.Level = 1;
                pet.Experience = 0;
                pet.Hunger = 50;
                pet.Mood = 50;
                pet.Stamina = 50;
                pet.Cleanliness = 50;
                pet.Health = 100;
                pet.LevelUpTime = DateTime.Now;
                pet.SkinColor = "#00FF00";
                pet.SkinColorChangedTime = DateTime.Now;
                pet.BackgroundColor = "default";
                pet.BackgroundColorChangedTime = DateTime.Now;
                pet.PointsChanged_SkinColor = 0;
                pet.PointsChanged_BackgroundColor = 0;
                pet.PointsGained_LevelUp = 0;
                pet.PointsGainedTime_LevelUp = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物重置成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int CalculateRequiredExp(int level)
        {
            // 簡單的經驗值計算公式
            return level * 100;
        }
    }
}
