using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        // GET: MiniGame/AdminPet/PetSystemRuleSettings
        public IActionResult PetSystemRuleSettings()
        {
            return View();
        }

        // AJAX endpoint for pet system rule settings
        [HttpGet]
        public async Task<IActionResult> GetPetSystemRuleSettings()
        {
            try
            {
                var settings = await _context.PetSystemRuleSettings
                    .OrderBy(s => s.SettingName)
                    .ToListAsync();

                return Json(new { success = true, data = settings });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: MiniGame/AdminPet/UpdatePetSystemRuleSettings
        [HttpPost]
        public async Task<IActionResult> UpdatePetSystemRuleSettings(int settingId, string settingValue, string description)
        {
            try
            {
                var setting = await _context.PetSystemRuleSettings
                    .FirstOrDefaultAsync(s => s.SettingID == settingId);

                if (setting == null)
                {
                    return Json(new { success = false, message = "找不到該設定" });
                }

                setting.SettingValue = settingValue;
                setting.Description = description;
                setting.UpdatedAt = DateTime.Now;
                setting.UpdatedByManagerId = 1; // TODO: 從認證中獲取管理員ID

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物系統規則更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminPet/IndividualMemberPetSettings
        public IActionResult IndividualMemberPetSettings()
        {
            return View();
        }

        // AJAX endpoint for individual member pet settings
        [HttpGet]
        public async Task<IActionResult> GetIndividualMemberPetSettings(int userId)
        {
            try
            {
                var pet = await _context.Pets
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.User_ID == userId);

                if (pet == null)
                {
                    return Json(new { success = false, message = "該會員沒有寵物" });
                }

                var petData = new
                {
                    pet.PetID,
                    pet.User_ID,
                    pet.User.User_name,
                    pet.User.User_Account,
                    pet.PetName,
                    pet.PetSkin,
                    pet.PetBackground,
                    pet.PetExp,
                    pet.PetLevel,
                    pet.Hunger,
                    pet.Happiness,
                    pet.Health,
                    pet.Energy,
                    pet.Cleanliness,
                    pet.CreatedAt,
                    pet.UpdatedAt
                };

                return Json(new { success = true, data = petData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: MiniGame/AdminPet/UpdateIndividualMemberPetSettings
        [HttpPost]
        public async Task<IActionResult> UpdateIndividualMemberPetSettings(int petId, string petName, string petSkin, string petBackground, int petExp, int petLevel, int hunger, int happiness, int health, int energy, int cleanliness)
        {
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.PetID == petId);

                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到該寵物" });
                }

                pet.PetName = petName;
                pet.PetSkin = petSkin;
                pet.PetBackground = petBackground;
                pet.PetExp = petExp;
                pet.PetLevel = petLevel;
                pet.Hunger = hunger;
                pet.Happiness = happiness;
                pet.Health = health;
                pet.Energy = energy;
                pet.Cleanliness = cleanliness;
                pet.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物設定更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminPet/MemberIndividualPetList
        public IActionResult MemberIndividualPetList()
        {
            return View();
        }

        // AJAX endpoint for member individual pet list
        [HttpGet]
        public async Task<IActionResult> GetMemberIndividualPetList(string searchTerm = "")
        {
            try
            {
                var query = _context.Pets
                    .Include(p => p.User)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p => 
                        p.User.User_name.Contains(searchTerm) || 
                        p.User.User_Account.Contains(searchTerm) ||
                        p.PetName.Contains(searchTerm));
                }

                var pets = await query
                    .Select(p => new
                    {
                        p.PetID,
                        p.User_ID,
                        p.User.User_name,
                        p.User.User_Account,
                        p.PetName,
                        p.PetSkin,
                        p.PetBackground,
                        p.PetExp,
                        p.PetLevel,
                        p.Hunger,
                        p.Happiness,
                        p.Health,
                        p.Energy,
                        p.Cleanliness,
                        p.CreatedAt,
                        p.UpdatedAt
                    })
                    .ToListAsync();

                return Json(new { success = true, data = pets });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminPet/PetAppearanceChangeRecords
        public IActionResult PetAppearanceChangeRecords()
        {
            return View();
        }

        // AJAX endpoint for pet appearance change records
        [HttpGet]
        public async Task<IActionResult> GetPetAppearanceChangeRecords(int? petId = null, string changeType = "")
        {
            try
            {
                var query = _context.PetAppearanceChangeLogs
                    .Include(pacl => pacl.Pet)
                    .ThenInclude(p => p.User)
                    .AsQueryable();

                if (petId.HasValue)
                {
                    query = query.Where(pacl => pacl.PetID == petId.Value);
                }

                if (!string.IsNullOrEmpty(changeType))
                {
                    query = query.Where(pacl => pacl.ChangeType == changeType);
                }

                var records = await query
                    .OrderByDescending(pacl => pacl.ChangeDate)
                    .Select(pacl => new
                    {
                        pacl.LogID,
                        pacl.PetID,
                        pacl.Pet.User.User_name,
                        pacl.Pet.User.User_Account,
                        pacl.Pet.PetName,
                        pacl.ChangeType,
                        pacl.OldValue,
                        pacl.NewValue,
                        pacl.PointsCost,
                        pacl.ChangeDate
                    })
                    .ToListAsync();

                return Json(new { success = true, data = records });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
