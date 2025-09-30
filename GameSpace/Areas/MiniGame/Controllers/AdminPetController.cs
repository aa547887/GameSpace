using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
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

        // GET: MiniGame/AdminPet
        public async Task<IActionResult> Index()
        {
            var viewModel = new PetOverviewViewModel();

            // 統計數據
            viewModel.TotalPets = await _context.Pets.CountAsync();
            viewModel.ActivePets = await _context.Pets
                .Where(p => p.Pet_Status == "Active")
                .CountAsync();
            viewModel.MaxLevelPets = await _context.Pets
                .Where(p => p.Pet_Level >= 10)
                .CountAsync();
            viewModel.TotalAppearanceChanges = await _context.PetAppearanceChangeLogs.CountAsync();

            // 寵物等級分布
            viewModel.LevelDistribution = await _context.Pets
                .GroupBy(p => p.Pet_Level)
                .Select(g => new PetLevelDistributionViewModel
                {
                    Level = g.Key,
                    Count = g.Count()
                })
                .OrderBy(d => d.Level)
                .ToListAsync();

            // 最近創建的寵物
            viewModel.RecentPets = await _context.Pets
                .Include(p => p.User)
                .OrderByDescending(p => p.Pet_CreatedAt)
                .Take(20)
                .Select(p => new PetViewModel
                {
                    PetID = p.PetID,
                    UserID = p.UserID,
                    UserName = p.User.User_name,
                    Pet_Name = p.Pet_Name,
                    Pet_Level = p.Pet_Level,
                    Pet_Exp = p.Pet_Exp,
                    Pet_Status = p.Pet_Status,
                    Pet_Skin = p.Pet_Skin,
                    Pet_Background = p.Pet_Background,
                    Pet_CreatedAt = p.Pet_CreatedAt
                })
                .ToListAsync();

            return View(viewModel);
        }

        // GET: MiniGame/AdminPet/PetSystemRuleSettings
        public async Task<IActionResult> PetSystemRuleSettings()
        {
            var settings = await _context.PetSystemRuleSettings
                .OrderBy(s => s.SettingID)
                .ToListAsync();

            return View(settings);
        }

        // POST: MiniGame/AdminPet/UpdatePetSystemRule
        [HttpPost]
        public async Task<IActionResult> UpdatePetSystemRule([FromBody] UpdatePetSystemRuleRequest request)
        {
            try
            {
                var setting = await _context.PetSystemRuleSettings
                    .FirstOrDefaultAsync(s => s.SettingID == request.SettingID);

                if (setting == null)
                {
                    return Json(new { success = false, message = "設定不存在" });
                }

                setting.SettingValue = request.SettingValue;
                setting.Description = request.Description;
                setting.UpdatedAt = DateTime.UtcNow;
                setting.UpdatedByManagerId = GetCurrentManagerId();

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物系統規則更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"更新失敗: {ex.Message}" });
            }
        }

        // POST: MiniGame/AdminPet/CreatePetSystemRule
        [HttpPost]
        public async Task<IActionResult> CreatePetSystemRule([FromBody] CreatePetSystemRuleRequest request)
        {
            try
            {
                var setting = new PetSystemRuleSettings
                {
                    SettingName = request.SettingName,
                    SettingValue = request.SettingValue,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedByManagerId = GetCurrentManagerId()
                };

                _context.PetSystemRuleSettings.Add(setting);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物系統規則創建成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"創建失敗: {ex.Message}" });
            }
        }

        // GET: MiniGame/AdminPet/MemberPetSettings
        public async Task<IActionResult> MemberPetSettings(int? page, string searchTerm)
        {
            var pageSize = 20;
            var pageNumber = page ?? 1;

            var query = _context.Pets
                .Include(p => p.User)
                .AsQueryable();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.User.User_name.Contains(searchTerm) ||
                                       p.User.User_Account.Contains(searchTerm) ||
                                       p.Pet_Name.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var pets = await query
                .OrderByDescending(p => p.Pet_CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PetViewModel
                {
                    PetID = p.PetID,
                    UserID = p.UserID,
                    UserName = p.User.User_name,
                    UserAccount = p.User.User_Account,
                    Pet_Name = p.Pet_Name,
                    Pet_Level = p.Pet_Level,
                    Pet_Exp = p.Pet_Exp,
                    Pet_Status = p.Pet_Status,
                    Pet_Skin = p.Pet_Skin,
                    Pet_Background = p.Pet_Background,
                    Pet_CreatedAt = p.Pet_CreatedAt,
                    Pet_Hunger = p.Pet_Hunger,
                    Pet_Happiness = p.Pet_Happiness,
                    Pet_Health = p.Pet_Health,
                    Pet_Sleepiness = p.Pet_Sleepiness,
                    Pet_Cleanliness = p.Pet_Cleanliness
                })
                .ToListAsync();

            var viewModel = new MemberPetSettingsViewModel
            {
                Pets = pets,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                TotalCount = totalCount,
                SearchTerm = searchTerm
            };

            return View(viewModel);
        }

        // GET: MiniGame/AdminPet/MemberPetDetail/{petId}
        public async Task<IActionResult> MemberPetDetail(int petId)
        {
            var pet = await _context.Pets
                .Include(p => p.User)
                .Include(p => p.User.UserIntroduce)
                .Include(p => p.PetAppearanceChangeLogs)
                .FirstOrDefaultAsync(p => p.PetID == petId);

            if (pet == null)
            {
                return NotFound();
            }

            var viewModel = new MemberPetDetailViewModel
            {
                PetID = pet.PetID,
                UserID = pet.UserID,
                UserName = pet.User.User_name,
                UserAccount = pet.User.User_Account,
                NickName = pet.User.UserIntroduce?.User_NickName ?? "",
                Pet_Name = pet.Pet_Name,
                Pet_Level = pet.Pet_Level,
                Pet_Exp = pet.Pet_Exp,
                Pet_Status = pet.Pet_Status,
                Pet_Skin = pet.Pet_Skin,
                Pet_Background = pet.Pet_Background,
                Pet_CreatedAt = pet.Pet_CreatedAt,
                Pet_Hunger = pet.Pet_Hunger,
                Pet_Happiness = pet.Pet_Happiness,
                Pet_Health = pet.Pet_Health,
                Pet_Sleepiness = pet.Pet_Sleepiness,
                Pet_Cleanliness = pet.Pet_Cleanliness,
                AppearanceChangeLogs = pet.PetAppearanceChangeLogs?
                    .OrderByDescending(log => log.ChangeTime)
                    .Select(log => new PetAppearanceChangeLogViewModel
                    {
                        LogID = log.LogID,
                        PetID = log.PetID,
                        ChangeType = log.ChangeType,
                        OldValue = log.OldValue,
                        NewValue = log.NewValue,
                        ChangeTime = log.ChangeTime,
                        Cost = log.Cost
                    })
                    .ToList() ?? new List<PetAppearanceChangeLogViewModel>()
            };

            return View(viewModel);
        }

        // POST: MiniGame/AdminPet/UpdatePetSettings
        [HttpPost]
        public async Task<IActionResult> UpdatePetSettings([FromBody] UpdatePetSettingsRequest request)
        {
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.PetID == request.PetID);

                if (pet == null)
                {
                    return Json(new { success = false, message = "寵物不存在" });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 記錄變更
                    var hasChanges = false;
                    var changeLogs = new List<PetAppearanceChangeLog>();

                    if (pet.Pet_Name != request.Pet_Name)
                    {
                        changeLogs.Add(new PetAppearanceChangeLog
                        {
                            PetID = pet.PetID,
                            ChangeType = "Name",
                            OldValue = pet.Pet_Name,
                            NewValue = request.Pet_Name,
                            ChangeTime = DateTime.UtcNow,
                            Cost = 0
                        });
                        pet.Pet_Name = request.Pet_Name;
                        hasChanges = true;
                    }

                    if (pet.Pet_Skin != request.Pet_Skin)
                    {
                        changeLogs.Add(new PetAppearanceChangeLog
                        {
                            PetID = pet.PetID,
                            ChangeType = "Skin",
                            OldValue = pet.Pet_Skin,
                            NewValue = request.Pet_Skin,
                            ChangeTime = DateTime.UtcNow,
                            Cost = 0 // 管理員調整不扣點數
                        });
                        pet.Pet_Skin = request.Pet_Skin;
                        hasChanges = true;
                    }

                    if (pet.Pet_Background != request.Pet_Background)
                    {
                        changeLogs.Add(new PetAppearanceChangeLog
                        {
                            PetID = pet.PetID,
                            ChangeType = "Background",
                            OldValue = pet.Pet_Background,
                            NewValue = request.Pet_Background,
                            ChangeTime = DateTime.UtcNow,
                            Cost = 0 // 管理員調整不扣點數
                        });
                        pet.Pet_Background = request.Pet_Background;
                        hasChanges = true;
                    }

                    if (hasChanges)
                    {
                        _context.PetAppearanceChangeLogs.AddRange(changeLogs);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "寵物設定更新成功" });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"更新失敗: {ex.Message}" });
            }
        }

        // GET: MiniGame/AdminPet/PetAppearanceChangeLogs
        public async Task<IActionResult> PetAppearanceChangeLogs(int? page, string searchTerm, string changeType)
        {
            var pageSize = 20;
            var pageNumber = page ?? 1;

            var query = _context.PetAppearanceChangeLogs
                .Include(log => log.Pet)
                    .ThenInclude(p => p.User)
                .AsQueryable();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(log => log.Pet.User.User_name.Contains(searchTerm) ||
                                          log.Pet.User.User_Account.Contains(searchTerm) ||
                                          log.Pet.Pet_Name.Contains(searchTerm));
            }

            // 變更類型篩選
            if (!string.IsNullOrEmpty(changeType))
            {
                query = query.Where(log => log.ChangeType == changeType);
            }

            var totalCount = await query.CountAsync();
            var logs = await query
                .OrderByDescending(log => log.ChangeTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(log => new PetAppearanceChangeLogViewModel
                {
                    LogID = log.LogID,
                    PetID = log.PetID,
                    PetName = log.Pet.Pet_Name,
                    UserID = log.Pet.UserID,
                    UserName = log.Pet.User.User_name,
                    ChangeType = log.ChangeType,
                    OldValue = log.OldValue,
                    NewValue = log.NewValue,
                    ChangeTime = log.ChangeTime,
                    Cost = log.Cost
                })
                .ToListAsync();

            var viewModel = new PetAppearanceChangeLogsViewModel
            {
                ChangeLogs = logs,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                TotalCount = totalCount,
                SearchTerm = searchTerm,
                ChangeType = changeType
            };

            return View(viewModel);
        }

        // GET: MiniGame/AdminPet/PetStatistics
        public async Task<IActionResult> PetStatistics()
        {
            var viewModel = new PetStatisticsViewModel();

            // 寵物狀態統計
            viewModel.StatusStats = await _context.Pets
                .GroupBy(p => p.Pet_Status)
                .Select(g => new PetStatusStatViewModel
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // 膚色統計
            viewModel.SkinStats = await _context.Pets
                .GroupBy(p => p.Pet_Skin)
                .Select(g => new PetSkinStatViewModel
                {
                    Skin = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // 背景統計
            viewModel.BackgroundStats = await _context.Pets
                .GroupBy(p => p.Pet_Background)
                .Select(g => new PetBackgroundStatViewModel
                {
                    Background = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // 變更記錄統計
            viewModel.ChangeLogStats = await _context.PetAppearanceChangeLogs
                .GroupBy(log => log.ChangeType)
                .Select(g => new PetChangeLogStatViewModel
                {
                    ChangeType = g.Key,
                    Count = g.Count(),
                    TotalCost = g.Sum(log => log.Cost)
                })
                .ToListAsync();

            return View(viewModel);
        }

        // 輔助方法
        private int GetCurrentManagerId()
        {
            // 這裡應該從當前登入的管理員中獲取ID
            // 實際應用中需要從Claims或Session中獲取
            return 1; // 暫時返回1，實際應用中需要修改
        }
    }

    // 請求模型
    public class UpdatePetSystemRuleRequest
    {
        public int SettingID { get; set; }
        public string SettingValue { get; set; }
        public string Description { get; set; }
    }

    public class CreatePetSystemRuleRequest
    {
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
        public string Description { get; set; }
    }

    public class UpdatePetSettingsRequest
    {
        public int PetID { get; set; }
        public string Pet_Name { get; set; }
        public string Pet_Skin { get; set; }
        public string Pet_Background { get; set; }
    }
}
