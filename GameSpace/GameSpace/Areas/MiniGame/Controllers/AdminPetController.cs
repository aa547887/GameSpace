using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public class AdminPetController : MiniGameBaseController
    {
        private readonly IMiniGameAdminService _adminService;

        public AdminPetController(GameSpacedatabaseContext context, IMiniGameAdminService adminService) : base(context)
        {
            _adminService = adminService;
        }

        // 整體寵物系統規則設定
        [HttpGet]
        public async Task<IActionResult> PetSystemRules()
        {
            try
            {
                var rules = await GetPetSystemRulesAsync();
                var viewModel = new PetSystemRulesViewModel
                {
                    Rules = rules
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入寵物系統規則時發生錯誤：{ex.Message}";
                return View(new PetSystemRulesViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> PetSystemRules(PetSystemRulesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Rules = await GetPetSystemRulesAsync();
                return View(model);
            }

            try
            {
                await UpdatePetSystemRulesAsync(model.Rules);
                TempData["SuccessMessage"] = "寵物系統規則更新成功！";
                return RedirectToAction(nameof(PetSystemRules));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新失敗：{ex.Message}");
                model.Rules = await GetPetSystemRulesAsync();
                return View(model);
            }
        }

        // 會員個別寵物設定手動調整基本資料
        [HttpGet]
        public async Task<IActionResult> PetSettings(int? userId = null)
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                var petSettings = new List<PetSettingModel>();

                if (userId.HasValue)
                {
                    petSettings = await GetUserPetSettingsAsync(userId.Value);
                }

                var viewModel = new PetSettingsViewModel
                {
                    Users = users,
                    PetSettings = petSettings,
                    SelectedUserId = userId
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入寵物設定時發生錯誤：{ex.Message}";
                return View(new PetSettingsViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> PetSettings(PetSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Users = await _context.Users.ToListAsync();
                return View(model);
            }

            try
            {
                await UpdatePetSettingsAsync(model.PetSettings);
                TempData["SuccessMessage"] = "寵物設定更新成功！";
                return RedirectToAction(nameof(PetSettings), new { userId = model.SelectedUserId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新失敗：{ex.Message}");
                model.Users = await _context.Users.ToListAsync();
                return View(model);
            }
        }

        // 會員個別寵物清單含查詢
        public async Task<IActionResult> PetList(PetQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            try
            {
                var result = await QueryPetsAsync(query);
                var users = await _context.Users.ToListAsync();

                var viewModel = new AdminPetListViewModel
                {
                    Pets = result.Items,
                    Users = users,
                    Query = query,
                    TotalCount = result.TotalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢寵物清單時發生錯誤：{ex.Message}";
                return View(new AdminPetListViewModel());
            }
        }

        // 換膚／換背景紀錄查詢
        public async Task<IActionResult> PetChangeHistory(PetChangeHistoryQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            try
            {
                var result = await QueryPetChangeHistoryAsync(query);
                var users = await _context.Users.ToListAsync();

                var viewModel = new AdminPetChangeHistoryViewModel
                {
                    ChangeHistories = result.Items,
                    Users = users,
                    Query = query,
                    TotalCount = result.TotalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢寵物變更紀錄時發生錯誤：{ex.Message}";
                return View(new AdminPetChangeHistoryViewModel());
            }
        }

        // AJAX API 方法
        [HttpGet]
        public async Task<IActionResult> GetUserPets(int userId)
        {
            try
            {
                var pets = await _context.Pets
                    .Where(p => p.UserId == userId)
                    .Select(p => new
                    {
                        petId = p.PetId,
                        petName = p.PetName,
                        skinColor = p.SkinColor,
                        background = p.Background,
                        level = p.Level,
                        experience = p.Experience,
                        happiness = p.Happiness,
                        hunger = p.Hunger,
                        cleanliness = p.Cleanliness,
                        energy = p.Energy,
                        health = p.Health
                    })
                    .ToListAsync();

                return Json(new { success = true, pets = pets });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPetDetails(int petId)
        {
            try
            {
                var pet = await _context.Pets
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PetId == petId);

                if (pet == null)
                    return Json(new { success = false, message = "找不到指定的寵物" });

                var petDetails = new
                {
                    petId = pet.PetId,
                    petName = pet.PetName,
                    userId = pet.UserId,
                    userName = pet.User.UserName,
                    skinColor = pet.SkinColor,
                    background = pet.Background,
                    level = pet.Level,
                    experience = pet.Experience,
                    happiness = pet.Happiness,
                    hunger = pet.Hunger,
                    cleanliness = pet.Cleanliness,
                    energy = pet.Energy,
                    health = pet.Health,
                    createdTime = pet.CreatedTime
                };

                return Json(new { success = true, data = petDetails });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePetSettings(UpdatePetSettingsModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "請填寫所有必要欄位" });

            try
            {
                await UpdatePetSettingsAsync(model);
                return Json(new { success = true, message = "寵物設定更新成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPetExperience(int petId, int experience)
        {
            try
            {
                await AddPetExperienceAsync(petId, experience);
                return Json(new { success = true, message = "寵物經驗添加成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPetStats(int petId)
        {
            try
            {
                await ResetPetStatsAsync(petId);
                return Json(new { success = true, message = "寵物狀態重置成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 私有方法
        private async Task<List<PetSystemRuleModel>> GetPetSystemRulesAsync()
        {
            var rules = await _context.PetSystemRules
                .OrderBy(r => r.Level)
                .Select(r => new PetSystemRuleModel
                {
                    RuleId = r.RuleId,
                    Level = r.Level,
                    RequiredExp = r.RequiredExp,
                    MaxHappiness = r.MaxHappiness,
                    MaxHunger = r.MaxHunger,
                    MaxCleanliness = r.MaxCleanliness,
                    MaxEnergy = r.MaxEnergy,
                    MaxHealth = r.MaxHealth,
                    FeedGain = r.FeedGain,
                    PlayGain = r.PlayGain,
                    BathGain = r.BathGain,
                    SleepGain = r.SleepGain
                })
                .ToListAsync();

            // 如果沒有規則，創建預設規則
            if (!rules.Any())
            {
                rules = CreateDefaultPetSystemRules();
                await SaveDefaultPetSystemRulesAsync(rules);
            }

            return rules;
        }

        private async Task UpdatePetSystemRulesAsync(List<PetSystemRuleModel> rules)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 清除現有規則
                var existingRules = await _context.PetSystemRules.ToListAsync();
                _context.PetSystemRules.RemoveRange(existingRules);

                // 添加新規則
                foreach (var rule in rules)
                {
                    var petSystemRule = new PetSystemRule
                    {
                        Level = rule.Level,
                        RequiredExp = rule.RequiredExp,
                        MaxHappiness = rule.MaxHappiness,
                        MaxHunger = rule.MaxHunger,
                        MaxCleanliness = rule.MaxCleanliness,
                        MaxEnergy = rule.MaxEnergy,
                        MaxHealth = rule.MaxHealth,
                        FeedGain = rule.FeedGain,
                        PlayGain = rule.PlayGain,
                        BathGain = rule.BathGain,
                        SleepGain = rule.SleepGain
                    };
                    _context.PetSystemRules.Add(petSystemRule);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<List<PetSettingModel>> GetUserPetSettingsAsync(int userId)
        {
            var pets = await _context.Pets
                .Where(p => p.UserId == userId)
                .Select(p => new PetSettingModel
                {
                    PetId = p.PetId,
                    PetName = p.PetName,
                    SkinColor = p.SkinColor,
                    Background = p.Background,
                    Level = p.Level,
                    Experience = p.Experience,
                    Happiness = p.Happiness,
                    Hunger = p.Hunger,
                    Cleanliness = p.Cleanliness,
                    Energy = p.Energy,
                    Health = p.Health
                })
                .ToListAsync();

            return pets;
        }

        private async Task UpdatePetSettingsAsync(List<PetSettingModel> petSettings)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var setting in petSettings)
                {
                    var pet = await _context.Pets.FindAsync(setting.PetId);
                    if (pet != null)
                    {
                        pet.PetName = setting.PetName;
                        pet.SkinColor = setting.SkinColor;
                        pet.Background = setting.Background;
                        pet.Level = setting.Level;
                        pet.Experience = setting.Experience;
                        pet.Happiness = setting.Happiness;
                        pet.Hunger = setting.Hunger;
                        pet.Cleanliness = setting.Cleanliness;
                        pet.Energy = setting.Energy;
                        pet.Health = setting.Health;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task UpdatePetSettingsAsync(UpdatePetSettingsModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pet = await _context.Pets.FindAsync(model.PetId);
                if (pet == null)
                    throw new Exception("找不到指定的寵物");

                pet.PetName = model.PetName;
                pet.SkinColor = model.SkinColor;
                pet.Background = model.Background;
                pet.Level = model.Level;
                pet.Experience = model.Experience;
                pet.Happiness = model.Happiness;
                pet.Hunger = model.Hunger;
                pet.Cleanliness = model.Cleanliness;
                pet.Energy = model.Energy;
                pet.Health = model.Health;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<PagedResult<PetModel>> QueryPetsAsync(PetQueryModel query)
        {
            var queryable = _context.Pets
                .Include(p => p.User)
                .AsQueryable();

            if (query.UserId.HasValue)
                queryable = queryable.Where(p => p.UserId == query.UserId.Value);

            if (!string.IsNullOrEmpty(query.UserName))
                queryable = queryable.Where(p => p.User.UserName.Contains(query.UserName));

            if (!string.IsNullOrEmpty(query.PetName))
                queryable = queryable.Where(p => p.PetName.Contains(query.PetName));

            if (query.MinLevel.HasValue)
                queryable = queryable.Where(p => p.Level >= query.MinLevel.Value);

            if (query.MaxLevel.HasValue)
                queryable = queryable.Where(p => p.Level <= query.MaxLevel.Value);

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.Experience)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new PetModel
                {
                    PetId = p.PetId,
                    UserId = p.UserId,
                    UserName = p.User.UserName,
                    PetName = p.PetName,
                    SkinColor = p.SkinColor,
                    Background = p.Background,
                    Level = p.Level,
                    Experience = p.Experience,
                    Happiness = p.Happiness,
                    Hunger = p.Hunger,
                    Cleanliness = p.Cleanliness,
                    Energy = p.Energy,
                    Health = p.Health,
                    CreatedTime = p.CreatedTime
                })
                .ToListAsync();

            return new PagedResult<PetModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        private async Task<PagedResult<PetChangeHistoryModel>> QueryPetChangeHistoryAsync(PetChangeHistoryQueryModel query)
        {
            var queryable = _context.PetChangeHistories
                .Include(h => h.Pet)
                .Include(h => h.Pet.User)
                .AsQueryable();

            if (query.UserId.HasValue)
                queryable = queryable.Where(h => h.Pet.UserId == query.UserId.Value);

            if (!string.IsNullOrEmpty(query.UserName))
                queryable = queryable.Where(h => h.Pet.User.UserName.Contains(query.UserName));

            if (!string.IsNullOrEmpty(query.ChangeType))
                queryable = queryable.Where(h => h.ChangeType == query.ChangeType);

            if (query.StartDate.HasValue)
                queryable = queryable.Where(h => h.ChangeTime >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                queryable = queryable.Where(h => h.ChangeTime <= query.EndDate.Value);

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(h => h.ChangeTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(h => new PetChangeHistoryModel
                {
                    HistoryId = h.HistoryId,
                    PetId = h.PetId,
                    PetName = h.Pet.PetName,
                    UserId = h.Pet.UserId,
                    UserName = h.Pet.User.UserName,
                    ChangeType = h.ChangeType,
                    OldValue = h.OldValue,
                    NewValue = h.NewValue,
                    ChangeTime = h.ChangeTime,
                    Description = h.Description
                })
                .ToListAsync();

            return new PagedResult<PetChangeHistoryModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        private async Task AddPetExperienceAsync(int petId, int experience)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pet = await _context.Pets.FindAsync(petId);
                if (pet == null)
                    throw new Exception("找不到指定的寵物");

                pet.Experience += experience;

                // 檢查是否升級
                var rules = await _context.PetSystemRules
                    .Where(r => r.Level > pet.Level)
                    .OrderBy(r => r.Level)
                    .FirstOrDefaultAsync();

                if (rules != null && pet.Experience >= rules.RequiredExp)
                {
                    pet.Level = rules.Level;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task ResetPetStatsAsync(int petId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pet = await _context.Pets.FindAsync(petId);
                if (pet == null)
                    throw new Exception("找不到指定的寵物");

                pet.Happiness = 100;
                pet.Hunger = 100;
                pet.Cleanliness = 100;
                pet.Energy = 100;
                pet.Health = 100;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private List<PetSystemRuleModel> CreateDefaultPetSystemRules()
        {
            return new List<PetSystemRuleModel>
            {
                new() { Level = 1, RequiredExp = 0, MaxHappiness = 100, MaxHunger = 100, MaxCleanliness = 100, MaxEnergy = 100, MaxHealth = 100, FeedGain = 20, PlayGain = 15, BathGain = 25, SleepGain = 30 },
                new() { Level = 2, RequiredExp = 100, MaxHappiness = 120, MaxHunger = 120, MaxCleanliness = 120, MaxEnergy = 120, MaxHealth = 120, FeedGain = 25, PlayGain = 20, BathGain = 30, SleepGain = 35 },
                new() { Level = 3, RequiredExp = 250, MaxHappiness = 150, MaxHunger = 150, MaxCleanliness = 150, MaxEnergy = 150, MaxHealth = 150, FeedGain = 30, PlayGain = 25, BathGain = 35, SleepGain = 40 },
                new() { Level = 4, RequiredExp = 500, MaxHappiness = 200, MaxHunger = 200, MaxCleanliness = 200, MaxEnergy = 200, MaxHealth = 200, FeedGain = 35, PlayGain = 30, BathGain = 40, SleepGain = 45 },
                new() { Level = 5, RequiredExp = 1000, MaxHappiness = 300, MaxHunger = 300, MaxCleanliness = 300, MaxEnergy = 300, MaxHealth = 300, FeedGain = 40, PlayGain = 35, BathGain = 45, SleepGain = 50 }
            };
        }

        private async Task SaveDefaultPetSystemRulesAsync(List<PetSystemRuleModel> rules)
        {
            foreach (var rule in rules)
            {
                var petSystemRule = new PetSystemRule
                {
                    Level = rule.Level,
                    RequiredExp = rule.RequiredExp,
                    MaxHappiness = rule.MaxHappiness,
                    MaxHunger = rule.MaxHunger,
                    MaxCleanliness = rule.MaxCleanliness,
                    MaxEnergy = rule.MaxEnergy,
                    MaxHealth = rule.MaxHealth,
                    FeedGain = rule.FeedGain,
                    PlayGain = rule.PlayGain,
                    BathGain = rule.BathGain,
                    SleepGain = rule.SleepGain
                };
                _context.PetSystemRules.Add(petSystemRule);
            }
            await _context.SaveChangesAsync();
        }
    }

    // ViewModels
    public class PetSystemRulesViewModel
    {
        public List<PetSystemRuleModel> Rules { get; set; } = new();
    }

    public class PetSettingsViewModel
    {
        public List<User> Users { get; set; } = new();
        public List<PetSettingModel> PetSettings { get; set; } = new();
        public int? SelectedUserId { get; set; }
    }

    public class PetQueryModel
    {
        public int? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public int? MinLevel { get; set; }
        public int? MaxLevel { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PetChangeHistoryQueryModel
    {
        public int? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AdminPetListViewModel
    {
        public List<PetModel> Pets { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public PetQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class AdminPetChangeHistoryViewModel
    {
        public List<PetChangeHistoryModel> ChangeHistories { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public PetChangeHistoryQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class PetSystemRuleModel
    {
        public int RuleId { get; set; }
        public int Level { get; set; }
        public int RequiredExp { get; set; }
        public int MaxHappiness { get; set; }
        public int MaxHunger { get; set; }
        public int MaxCleanliness { get; set; }
        public int MaxEnergy { get; set; }
        public int MaxHealth { get; set; }
        public int FeedGain { get; set; }
        public int PlayGain { get; set; }
        public int BathGain { get; set; }
        public int SleepGain { get; set; }
    }

    public class PetSettingModel
    {
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string SkinColor { get; set; } = string.Empty;
        public string Background { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Happiness { get; set; }
        public int Hunger { get; set; }
        public int Cleanliness { get; set; }
        public int Energy { get; set; }
        public int Health { get; set; }
    }

    public class UpdatePetSettingsModel
    {
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string SkinColor { get; set; } = string.Empty;
        public string Background { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Happiness { get; set; }
        public int Hunger { get; set; }
        public int Cleanliness { get; set; }
        public int Energy { get; set; }
        public int Health { get; set; }
    }

    public class PetModel
    {
        public int PetId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public string SkinColor { get; set; } = string.Empty;
        public string Background { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Happiness { get; set; }
        public int Hunger { get; set; }
        public int Cleanliness { get; set; }
        public int Energy { get; set; }
        public int Health { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class PetChangeHistoryModel
    {
        public int HistoryId { get; set; }
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public DateTime ChangeTime { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
