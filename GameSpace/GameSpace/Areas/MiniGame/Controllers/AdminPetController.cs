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

        // 1. 整體寵物系統規則設定（升級規則/互動增益/可選膚色與所需點數/可選背景與所需點數）
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

        // 2. 會員個別寵物設定手動調整基本資料（寵物名、膚色、背景）
        [HttpGet]
        public async Task<IActionResult> PetSettings(int? userId = null)
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

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
                model.Users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                return View(model);
            }

            try
            {
                await UpdateUserPetSettingsAsync(model.PetSettings);
                TempData["SuccessMessage"] = "寵物設定更新成功！";
                return RedirectToAction(nameof(PetSettings), new { userId = model.SelectedUserId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新失敗：{ex.Message}");
                model.Users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                return View(model);
            }
        }

        // 3. 會員個別寵物清單含查詢（寵物名/膚色/背景/經驗/等級/五大狀態）＋ 換膚／換背景紀錄查詢
        [HttpGet]
        public async Task<IActionResult> PetList(PetQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 20;

            try
            {
                var result = await QueryPetsAsync(query);
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

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

        // 查看寵物詳細信息
        [HttpGet]
        public async Task<IActionResult> PetDetails(int petId)
        {
            try
            {
                var pet = await GetPetDetailsAsync(petId);
                if (pet == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的寵物！";
                    return RedirectToAction(nameof(PetList));
                }

                var changeRecords = await GetPetChangeRecordsAsync(petId);
                var viewModel = new PetDetailsViewModel
                {
                    Pet = pet,
                    ChangeRecords = changeRecords
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查看寵物詳情時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(PetList));
            }
        }

        // 更新寵物設定
        [HttpPost]
        public async Task<IActionResult> UpdatePetSettings([FromBody] PetUpdateModel model)
        {
            try
            {
                await UpdatePetAsync(model);
                return Json(new { success = true, message = "寵物設定更新成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取寵物統計資料
        [HttpGet]
        public async Task<IActionResult> GetPetStats()
        {
            try
            {
                var stats = await GetPetStatisticsAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取用戶寵物清單
        [HttpGet]
        public async Task<IActionResult> GetUserPets(int userId)
        {
            try
            {
                var pets = await GetUserPetsAsync(userId);
                return Json(new { success = true, data = pets });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 導出寵物資料
        [HttpGet]
        public async Task<IActionResult> ExportPetData(PetQueryModel query)
        {
            try
            {
                var pets = await GetAllPetsAsync(query);
                var csv = GeneratePetDataCSV(pets);
                
                var fileName = $"寵物資料_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"導出寵物資料時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(PetList));
            }
        }

        // 私有方法：獲取寵物系統規則
        private async Task<List<PetSystemRuleModel>> GetPetSystemRulesAsync()
        {
            var rules = await _context.PetSystemRules
                .OrderBy(r => r.RuleType)
                .ThenBy(r => r.Level)
                .Select(r => new PetSystemRuleModel
                {
                    Id = r.Id,
                    RuleType = r.RuleType,
                    Level = r.Level,
                    Value = r.Value,
                    Description = r.Description,
                    IsActive = r.IsActive
                })
                .ToListAsync();

            return rules;
        }

        // 私有方法：更新寵物系統規則
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
                    var petRule = new PetSystemRule
                    {
                        RuleType = rule.RuleType,
                        Level = rule.Level,
                        Value = rule.Value,
                        Description = rule.Description,
                        IsActive = rule.IsActive,
                        CreateTime = DateTime.Now,
                        LastUpdateTime = DateTime.Now
                    };
                    _context.PetSystemRules.Add(petRule);
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

        // 私有方法：獲取用戶寵物設定
        private async Task<List<PetSettingModel>> GetUserPetSettingsAsync(int userId)
        {
            var pets = await _context.Pets
                .Where(p => p.UserId == userId)
                .Select(p => new PetSettingModel
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    PetName = p.PetName,
                    SkinColor = p.SkinColor,
                    Background = p.Background,
                    Level = p.Level,
                    Experience = p.Experience,
                    Happiness = p.Happiness,
                    Hunger = p.Hunger,
                    Health = p.Health,
                    Energy = p.Energy,
                    Cleanliness = p.Cleanliness
                })
                .ToListAsync();

            return pets;
        }

        // 私有方法：更新用戶寵物設定
        private async Task UpdateUserPetSettingsAsync(List<PetSettingModel> petSettings)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var setting in petSettings)
                {
                    var pet = await _context.Pets.FindAsync(setting.Id);
                    if (pet != null)
                    {
                        pet.PetName = setting.PetName;
                        pet.SkinColor = setting.SkinColor;
                        pet.Background = setting.Background;
                        pet.Level = setting.Level;
                        pet.Experience = setting.Experience;
                        pet.Happiness = setting.Happiness;
                        pet.Hunger = setting.Hunger;
                        pet.Health = setting.Health;
                        pet.Energy = setting.Energy;
                        pet.Cleanliness = setting.Cleanliness;
                        pet.LastUpdateTime = DateTime.Now;
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

        // 私有方法：查詢寵物
        private async Task<PagedResult<PetModel>> QueryPetsAsync(PetQueryModel query)
        {
            var queryable = _context.Pets
                .Include(p => p.User)
                .AsQueryable();

            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(p => p.UserId == query.UserId.Value);
            }

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(p => 
                    p.PetName.Contains(query.SearchTerm) || 
                    p.User.UserName.Contains(query.SearchTerm) ||
                    p.User.Email.Contains(query.SearchTerm));
            }

            if (query.MinLevel.HasValue)
            {
                queryable = queryable.Where(p => p.Level >= query.MinLevel.Value);
            }

            if (query.MaxLevel.HasValue)
            {
                queryable = queryable.Where(p => p.Level <= query.MaxLevel.Value);
            }

            if (!string.IsNullOrEmpty(query.SkinColor))
            {
                queryable = queryable.Where(p => p.SkinColor == query.SkinColor);
            }

            if (!string.IsNullOrEmpty(query.Background))
            {
                queryable = queryable.Where(p => p.Background == query.Background);
            }

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.Experience)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new PetModel
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = p.User.UserName,
                    Email = p.User.Email,
                    PetName = p.PetName,
                    SkinColor = p.SkinColor,
                    Background = p.Background,
                    Level = p.Level,
                    Experience = p.Experience,
                    Happiness = p.Happiness,
                    Hunger = p.Hunger,
                    Health = p.Health,
                    Energy = p.Energy,
                    Cleanliness = p.Cleanliness,
                    CreateTime = p.CreateTime,
                    LastUpdateTime = p.LastUpdateTime
                })
                .ToListAsync();

            return new PagedResult<PetModel>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        // 私有方法：獲取寵物詳情
        private async Task<PetModel> GetPetDetailsAsync(int petId)
        {
            var pet = await _context.Pets
                .Include(p => p.User)
                .Where(p => p.Id == petId)
                .Select(p => new PetModel
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = p.User.UserName,
                    Email = p.User.Email,
                    PetName = p.PetName,
                    SkinColor = p.SkinColor,
                    Background = p.Background,
                    Level = p.Level,
                    Experience = p.Experience,
                    Happiness = p.Happiness,
                    Hunger = p.Hunger,
                    Health = p.Health,
                    Energy = p.Energy,
                    Cleanliness = p.Cleanliness,
                    CreateTime = p.CreateTime,
                    LastUpdateTime = p.LastUpdateTime
                })
                .FirstOrDefaultAsync();

            return pet;
        }

        // 私有方法：獲取寵物變更紀錄
        private async Task<List<PetChangeRecordModel>> GetPetChangeRecordsAsync(int petId)
        {
            var records = await _context.PetChangeRecords
                .Where(r => r.PetId == petId)
                .OrderByDescending(r => r.ChangeTime)
                .Select(r => new PetChangeRecordModel
                {
                    Id = r.Id,
                    PetId = r.PetId,
                    ChangeType = r.ChangeType,
                    OldValue = r.OldValue,
                    NewValue = r.NewValue,
                    Description = r.Description,
                    ChangeTime = r.ChangeTime
                })
                .ToListAsync();

            return records;
        }

        // 私有方法：更新寵物
        private async Task UpdatePetAsync(PetUpdateModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pet = await _context.Pets.FindAsync(model.Id);
                if (pet == null)
                {
                    throw new Exception("找不到指定的寵物");
                }

                // 記錄變更
                var changeRecord = new PetChangeRecord
                {
                    PetId = model.Id,
                    ChangeType = model.ChangeType,
                    OldValue = model.OldValue,
                    NewValue = model.NewValue,
                    Description = model.Description,
                    ChangeTime = DateTime.Now
                };
                _context.PetChangeRecords.Add(changeRecord);

                // 更新寵物資料
                if (model.PetName != null) pet.PetName = model.PetName;
                if (model.SkinColor != null) pet.SkinColor = model.SkinColor;
                if (model.Background != null) pet.Background = model.Background;
                if (model.Level.HasValue) pet.Level = model.Level.Value;
                if (model.Experience.HasValue) pet.Experience = model.Experience.Value;
                if (model.Happiness.HasValue) pet.Happiness = model.Happiness.Value;
                if (model.Hunger.HasValue) pet.Hunger = model.Hunger.Value;
                if (model.Health.HasValue) pet.Health = model.Health.Value;
                if (model.Energy.HasValue) pet.Energy = model.Energy.Value;
                if (model.Cleanliness.HasValue) pet.Cleanliness = model.Cleanliness.Value;

                pet.LastUpdateTime = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // 私有方法：獲取寵物統計
        private async Task<PetStatsModel> GetPetStatisticsAsync()
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            var stats = new PetStatsModel
            {
                TotalPets = await _context.Pets.CountAsync(),
                ActivePets = await _context.Pets
                    .Where(p => p.LastUpdateTime >= today.AddDays(-7))
                    .CountAsync(),
                AverageLevel = await _context.Pets.AverageAsync(p => p.Level),
                TotalExperience = await _context.Pets.SumAsync(p => p.Experience),
                NewPetsThisMonth = await _context.Pets
                    .Where(p => p.CreateTime >= thisMonth)
                    .CountAsync(),
                TopPets = await _context.Pets
                    .OrderByDescending(p => p.Level)
                    .ThenByDescending(p => p.Experience)
                    .Take(5)
                    .Select(p => new { p.PetName, p.Level, p.Experience, p.User.UserName })
                    .ToListAsync()
            };

            return stats;
        }

        // 私有方法：獲取用戶寵物
        private async Task<List<PetModel>> GetUserPetsAsync(int userId)
        {
            var pets = await _context.Pets
                .Where(p => p.UserId == userId)
                .Select(p => new PetModel
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    PetName = p.PetName,
                    SkinColor = p.SkinColor,
                    Background = p.Background,
                    Level = p.Level,
                    Experience = p.Experience,
                    Happiness = p.Happiness,
                    Hunger = p.Hunger,
                    Health = p.Health,
                    Energy = p.Energy,
                    Cleanliness = p.Cleanliness,
                    CreateTime = p.CreateTime,
                    LastUpdateTime = p.LastUpdateTime
                })
                .ToListAsync();

            return pets;
        }

        // 私有方法：獲取所有寵物（用於導出）
        private async Task<List<PetModel>> GetAllPetsAsync(PetQueryModel query)
        {
            var queryable = _context.Pets
                .Include(p => p.User)
                .AsQueryable();

            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(p => p.UserId == query.UserId.Value);
            }

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(p => 
                    p.PetName.Contains(query.SearchTerm) || 
                    p.User.UserName.Contains(query.SearchTerm) ||
                    p.User.Email.Contains(query.SearchTerm));
            }

            if (query.MinLevel.HasValue)
            {
                queryable = queryable.Where(p => p.Level >= query.MinLevel.Value);
            }

            if (query.MaxLevel.HasValue)
            {
                queryable = queryable.Where(p => p.Level <= query.MaxLevel.Value);
            }

            if (!string.IsNullOrEmpty(query.SkinColor))
            {
                queryable = queryable.Where(p => p.SkinColor == query.SkinColor);
            }

            if (!string.IsNullOrEmpty(query.Background))
            {
                queryable = queryable.Where(p => p.Background == query.Background);
            }

            return await queryable
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.Experience)
                .Select(p => new PetModel
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = p.User.UserName,
                    Email = p.User.Email,
                    PetName = p.PetName,
                    SkinColor = p.SkinColor,
                    Background = p.Background,
                    Level = p.Level,
                    Experience = p.Experience,
                    Happiness = p.Happiness,
                    Hunger = p.Hunger,
                    Health = p.Health,
                    Energy = p.Energy,
                    Cleanliness = p.Cleanliness,
                    CreateTime = p.CreateTime,
                    LastUpdateTime = p.LastUpdateTime
                })
                .ToListAsync();
        }

        // 私有方法：生成寵物資料CSV
        private string GeneratePetDataCSV(List<PetModel> pets)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("寵物ID,用戶ID,用戶名稱,寵物名稱,膚色,背景,等級,經驗值,快樂度,飢餓度,健康度,精力,清潔度,創建時間,最後更新時間");

            foreach (var pet in pets)
            {
                csv.AppendLine($"{pet.Id},{pet.UserId},{pet.UserName},{pet.PetName},{pet.SkinColor},{pet.Background},{pet.Level},{pet.Experience},{pet.Happiness},{pet.Hunger},{pet.Health},{pet.Energy},{pet.Cleanliness},{pet.CreateTime:yyyy-MM-dd HH:mm:ss},{pet.LastUpdateTime:yyyy-MM-dd HH:mm:ss}");
            }

            return csv.ToString();
        }
    }
}
