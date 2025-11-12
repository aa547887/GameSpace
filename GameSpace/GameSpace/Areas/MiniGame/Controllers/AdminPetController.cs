using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class AdminPetController : MiniGameBaseController
    {
        private readonly IPetService _petService;
        private readonly IPetRulesService _petRulesService;
        private readonly IPetQueryService _petQueryService;
        private readonly IPetMutationService _petMutationService;
        private readonly IFuzzySearchService _fuzzySearchService;
        private readonly ILogger<AdminPetController> _logger;

        public AdminPetController(
            GameSpacedatabaseContext context,
            IPetService petService,
            IPetRulesService petRulesService,
            IPetQueryService petQueryService,
            IPetMutationService petMutationService,
            IFuzzySearchService fuzzySearchService,
            ILogger<AdminPetController> logger)
            : base(context)
        {
            _petService = petService;
            _petRulesService = petRulesService;
            _petQueryService = petQueryService;
            _petMutationService = petMutationService;
            _fuzzySearchService = fuzzySearchService;
            _logger = logger;
        }

        // GET: AdminPet
        // Note: Pet entity only has basic properties (PetName, Level, Experience, etc.), not PetType/Description/Rarity
        public async Task<IActionResult> Index(string searchTerm = "", string sortBy = "name", int page = 1, int pageSize = 10)
        {
            var pets = await _petService.GetAllPetsAsync();

            // 模糊搜尋：SearchTerm（聯集OR邏輯，使用 FuzzySearchService）
            var hasSearchTerm = !string.IsNullOrWhiteSpace(searchTerm);
            Dictionary<int, int> petPriority = new Dictionary<int, int>();

            if (hasSearchTerm)
            {
                var term = searchTerm.Trim();
                var matchedPets = new List<GameSpace.Models.Pet>();

                foreach (var pet in pets)
                {
                    int priority = 0;

                    // 寵物ID精確匹配優先
                    if (pet.PetId.ToString().Equals(term, StringComparison.OrdinalIgnoreCase))
                    {
                        priority = 1; // 完全匹配 PetId
                    }
                    else if (pet.PetId.ToString().Contains(term))
                    {
                        priority = 2; // 部分匹配 PetId
                    }

                    // 如果ID沒有匹配，檢查UserId
                    if (priority == 0 && int.TryParse(term, out int userId))
                    {
                        if (pet.UserId == userId)
                        {
                            priority = 1; // 完全匹配 UserId
                        }
                        else if (pet.UserId.ToString().Contains(term))
                        {
                            priority = 2; // 部分匹配 UserId
                        }
                    }

                    // 如果還沒匹配，使用模糊搜尋寵物名稱
                    if (priority == 0)
                    {
                        priority = _fuzzySearchService.CalculateMatchPriority(
                            term,
                            pet.PetName ?? ""
                        );
                    }

                    // 如果匹配成功（priority > 0），加入結果
                    if (priority > 0)
                    {
                        matchedPets.Add(pet);
                        petPriority[pet.PetId] = priority;
                    }
                }

                pets = matchedPets;
            }

            // 分頁前計算總數
            var totalCount = pets.Count();

            // 優先順序排序
            if (hasSearchTerm)
            {
                // 按照優先順序排序
                var ordered = pets.OrderBy(p => petPriority.ContainsKey(p.PetId) ? petPriority[p.PetId] : 99);

                // 次要排序
                pets = sortBy switch
                {
                    "level" => ordered.ThenByDescending(p => p.Level),
                    "exp" => ordered.ThenByDescending(p => p.Experience),
                    "health" => ordered.ThenByDescending(p => p.Health),
                    _ => ordered.ThenBy(p => p.PetName)
                };
            }
            else
            {
                // 沒有搜尋條件時使用預設排序
                pets = sortBy switch
                {
                    "level" => pets.OrderByDescending(p => p.Level),
                    "exp" => pets.OrderByDescending(p => p.Experience),
                    "health" => pets.OrderByDescending(p => p.Health),
                    _ => pets.OrderBy(p => p.PetName)
                };
            }

            // 分頁
            var pagedPets = pets
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new AdminPetIndexViewModel
            {
                Pets = new PagedResult<GameSpace.Models.Pet>
                {
                    Items = pagedPets,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                }
            };

            // 設定 ViewBag 用於搜尋和篩選
            // 修正：統計應該從篩選後的資料計算，不是重新查詢全表
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalPets = totalCount;
            ViewBag.HighLevelPets = pets.Count(p => p.Level >= 10);
            ViewBag.HealthyPets = pets.Count(p => p.Health >= 80);
            ViewBag.AverageLevel = pets.Any() ? pets.Average(p => (double)p.Level) : 0;
            ViewBag.AverageHealth = pets.Any() ? pets.Average(p => (double)p.Health) : 0;

            return View(viewModel);
        }

        // GET: AdminPet/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _petService.GetPetByIdAsync(id.Value);

            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }

        // GET: AdminPet/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminPet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminPetCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var pet = new GameSpace.Models.Pet
                {
                    UserId = model.UserId,
                    PetName = model.PetName,
                    SkinColor = model.SkinColor,
                    SkinColorChangedTime = DateTime.Now,
                    BackgroundColor = model.BackgroundColor,
                    BackgroundColorChangedTime = DateTime.Now,
                    // 初始化寵物狀態
                    Level = 1,
                    LevelUpTime = DateTime.Now,
                    Experience = 0,
                    Hunger = 100,
                    Mood = 100,
                    Stamina = 100,
                    Cleanliness = 100,
                    Health = 100,
                    PointsChangedSkinColor = 0,
                    PointsChangedBackgroundColor = 0,
                    PointsGainedLevelUp = 0,
                    PointsGainedTimeLevelUp = DateTime.Now
                };

                var result = await _petService.CreatePetAsync(pet);

                if (result)
                {
                    TempData["SuccessMessage"] = "寵物建立成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "建立寵物失敗");
                }
            }

            return View(model);
        }

        // GET: AdminPet/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _petService.GetPetByIdAsync(id.Value);
            if (pet == null)
            {
                return NotFound();
            }

            var model = new AdminPetCreateViewModel
            {
                UserId = pet.UserId,
                PetName = pet.PetName,
                SkinColor = pet.SkinColor,
                BackgroundColor = pet.BackgroundColor
            };

            return View(model);
        }

        // POST: AdminPet/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminPetCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var pet = await _petService.GetPetByIdAsync(id);
                if (pet == null)
                {
                    return NotFound();
                }

                pet.UserId = model.UserId;
                pet.PetName = model.PetName;
                pet.SkinColor = model.SkinColor;
                pet.BackgroundColor = model.BackgroundColor;

                var result = await _petService.UpdatePetAsync(pet);

                if (result)
                {
                    TempData["SuccessMessage"] = "寵物更新成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "更新寵物失敗");
                }
            }

            return View(model);
        }

        // GET: AdminPet/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _petService.GetPetByIdAsync(id.Value);

            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }

        // POST: AdminPet/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _petService.DeletePetAsync(id);

            if (result)
            {
                TempData["SuccessMessage"] = "寵物刪除成功";
            }
            else
            {
                TempData["ErrorMessage"] = "寵物刪除失敗";
            }

            return RedirectToAction(nameof(Index));
        }

        // 切換寵物狀態 - Pet entity doesn't have IsActive property, removed this functionality
        // [HttpPost]
        // public async Task<IActionResult> ToggleStatus(int id)
        // Note: Pet entity doesn't have IsActive property - this method is disabled

        // 獲取寵物統計數據
        [HttpGet]
        public async Task<IActionResult> GetPetStats()
        {
            var allPets = await _petService.GetAllPetsAsync();
            var stats = new
            {
                total = allPets.Count(),
                highLevel = allPets.Count(p => p.Level >= 10),
                healthy = allPets.Count(p => p.Health >= 80),
                hungry = allPets.Count(p => p.Hunger < 30),
                avgLevel = allPets.Any() ? allPets.Average(p => (double)p.Level) : 0,
                avgHealth = allPets.Any() ? allPets.Average(p => (double)p.Health) : 0
            };

            return Json(stats);
        }

        // 獲取寵物等級分佈
        [HttpGet]
        public async Task<IActionResult> GetPetRarityDistribution()
        {
            var allPets = await _petService.GetAllPetsAsync();

            var distribution = allPets
                .GroupBy(p => p.Level / 10 * 10) // Group by 10-level ranges (0-9, 10-19, etc.)
                .Select(g => new
                {
                    levelRange = $"Level {g.Key}-{g.Key + 9}",
                    count = g.Count()
                })
                .ToList();

            return Json(distribution);
        }

        // 獲取寵物皮膚顏色分佈
        [HttpGet]
        public async Task<IActionResult> GetPetTypeDistribution()
        {
            var allPets = await _petService.GetAllPetsAsync();

            var distribution = allPets
                .GroupBy(p => p.SkinColor)
                .Select(g => new
                {
                    skinColor = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(g => g.count)
                .ToList();

            return Json(distribution);
        }

        // 獲取寵物健康度統計
        [HttpGet]
        public async Task<IActionResult> GetPetDropRateStats()
        {
            var allPets = await _petService.GetAllPetsAsync();

            var stats = new
            {
                avgHealth = allPets.Any() ? allPets.Average(p => (double)p.Health) : 0,
                avgHunger = allPets.Any() ? allPets.Average(p => (double)p.Hunger) : 0,
                avgMood = allPets.Any() ? allPets.Average(p => (double)p.Mood) : 0,
                avgStamina = allPets.Any() ? allPets.Average(p => (double)p.Stamina) : 0,
                avgCleanliness = allPets.Any() ? allPets.Average(p => (double)p.Cleanliness) : 0
            };

            return Json(stats);
        }

        // 獲取寵物顏色選項
        [HttpGet]
        public async Task<IActionResult> GetPetColorOptions()
        {
            try
            {
                var options = await _petService.GetAvailableColorsAsync();
                return Json(new { success = true, data = options });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 新增寵物顏色選項
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPetColorOption(string colorName, string colorValue, int displayOrder)
        {
            try
            {
                if (string.IsNullOrEmpty(colorName) || string.IsNullOrEmpty(colorValue))
                {
                    return Json(new { success = false, message = "顏色名稱和顏色值不能為空" });
                }

                var option = new PetColorOption
                {
                    ColorName = colorName,
                    ColorValue = colorValue,
                    DisplayOrder = displayOrder,
                    IsActive = true
                };

                var result = await _petRulesService.CreateColorOptionAsync(option);

                if (result)
                {
                    return Json(new { success = true, message = "寵物顏色選項新增成功" });
                }
                else
                {
                    return Json(new { success = false, message = "新增失敗" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 更新寵物顏色選項
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePetColorOption(int id, string colorName, string colorValue, int displayOrder, bool isActive)
        {
            try
            {
                var options = await _petRulesService.GetAllColorOptionsAsync();
                var option = options.FirstOrDefault(o => o.ColorOptionId == id);

                if (option == null)
                {
                    return Json(new { success = false, message = "找不到指定的顏色選項" });
                }

                option.ColorName = colorName;
                option.ColorValue = colorValue;
                option.DisplayOrder = displayOrder;
                option.IsActive = isActive;

                var result = await _petRulesService.UpdateColorOptionAsync(option);

                if (result)
                {
                    return Json(new { success = true, message = "寵物顏色選項更新成功" });
                }
                else
                {
                    return Json(new { success = false, message = "更新失敗" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 刪除寵物顏色選項
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePetColorOption(int id)
        {
            try
            {
                var result = await _petRulesService.DeleteColorOptionAsync(id);

                if (result)
                {
                    return Json(new { success = true, message = "寵物顏色選項刪除成功" });
                }
                else
                {
                    return Json(new { success = false, message = "刪除失敗" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取寵物背景選項
        [HttpGet]
        public async Task<IActionResult> GetPetBackgroundOptions()
        {
            try
            {
                var options = await _petService.GetAvailableBackgroundsAsync();
                return Json(new { success = true, data = options });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取升級規則
        [HttpGet]
        public async Task<IActionResult> GetPetLevelUpRules()
        {
            try
            {
                var rules = await _petRulesService.GetAllLevelUpRulesAsync();
                return Json(new { success = true, data = rules });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 更新升級規則
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePetLevelUpRules(List<GameSpace.Areas.MiniGame.Models.PetLevelUpRule> rules)
        {
            try
            {
                if (rules == null || !rules.Any())
                {
                    return Json(new { success = false, message = "升級規則不能為空" });
                }

                // 更新每個規則
                foreach (var rule in rules)
                {
                    await _petRulesService.UpdateLevelUpRuleAsync(rule);
                }

                return Json(new { success = true, message = "寵物升級規則更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取互動獎勵規則
        [HttpGet]
        public async Task<IActionResult> GetPetInteractionBonusRules()
        {
            try
            {
                var rules = await _petRulesService.GetAllInteractionRulesAsync();
                return Json(new { success = true, data = rules });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 更新互動獎勵規則
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePetInteractionBonusRules(List<GameSpace.Areas.MiniGame.Models.PetInteractionBonusRules> rules)
        {
            try
            {
                if (rules == null || !rules.Any())
                {
                    return Json(new { success = false, message = "互動狀態增益規則不能為空" });
                }

                // 更新每個規則
                foreach (var rule in rules)
                {
                    await _petRulesService.UpdateInteractionRuleAsync(rule);
                }

                return Json(new { success = true, message = "寵物互動狀態增益規則更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取換色成本
        [HttpGet]
        public async Task<IActionResult> GetPetColorChangeCost()
        {
            try
            {
                // 使用預設等級1的成本
                var cost = await _petRulesService.GetSkinColorCostForLevelAsync(1);
                return Json(new { success = true, data = new { pointsRequired = cost } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 更新換色成本
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePetColorChangeCost(int pointsRequired)
        {
            try
            {
                if (pointsRequired < 0)
                {
                    return Json(new { success = false, message = "所需點數不能為負數" });
                }

                // 更新等級1的換色成本設定
                var settings = await _petRulesService.GetAllSkinColorSettingsAsync();
                var setting = settings.FirstOrDefault(s => s.RequiredLevel == 1);

                if (setting != null)
                {
                    setting.PointCost = pointsRequired;
                    await _petRulesService.UpdateSkinColorSettingAsync(setting);
                }

                return Json(new { success = true, message = "寵物換色所需點數設定成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取換背景成本
        [HttpGet]
        public async Task<IActionResult> GetPetBackgroundChangeCost()
        {
            try
            {
                var cost = await _petRulesService.GetBackgroundCostForLevelAsync(1);
                return Json(new { success = true, data = new { pointsRequired = cost } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 更新換背景成本
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePetBackgroundChangeCost(int pointsRequired)
        {
            try
            {
                if (pointsRequired < 0)
                {
                    return Json(new { success = false, message = "所需點數不能為負數" });
                }

                var settings = await _petRulesService.GetAllBackgroundSettingsAsync();
                var setting = settings.FirstOrDefault(s => s.RequiredLevel == 1);

                if (setting != null)
                {
                    setting.PointCost = pointsRequired;
                    await _petRulesService.UpdateBackgroundSettingAsync(setting);
                }

                return Json(new { success = true, message = "寵物換背景所需點數設定成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: AdminPet/ColorChangeHistory
        /// <summary>
        /// 取得寵物膚色變更歷史記錄
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ColorChangeHistory(PetColorChangeHistoryQueryModel query)
        {
            try
            {
                // 驗證查詢參數
                if (query.PageNumber < 1)
                {
                    query.PageNumber = 1;
                }

                if (query.PageSize < 1 || query.PageSize > 100)
                {
                    query.PageSize = 20;
                }

                // 如果有結束日期但沒有開始日期，設定預設開始日期為30天前
                if (query.EndDate.HasValue && !query.StartDate.HasValue)
                {
                    query.StartDate = query.EndDate.Value.AddDays(-30);
                }

                // 呼叫服務層取得歷史記錄
                var result = await _petQueryService.GetColorChangeHistoryAsync(query);

                // 將查詢參數傳遞到 ViewBag 以便視圖使用
                ViewBag.UserId = query.UserId;
                ViewBag.PetId = query.PetId;
                ViewBag.StartDate = query.StartDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = query.EndDate?.ToString("yyyy-MM-dd");
                ViewBag.PageNumber = query.PageNumber;
                ViewBag.PageSize = query.PageSize;

                return View(result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入膚色變更歷史記錄時發生錯誤：{ex.Message}";
                return View(new PetColorChangeHistoryPagedResult
                {
                    Items = new List<ColorChangeHistoryItemViewModel>(),
                    TotalCount = 0,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalPages = 0
                });
            }
        }

        // GET: AdminPet/BackgroundChangeHistory
        /// <summary>
        /// 取得寵物背景變更歷史記錄
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BackgroundChangeHistory(PetBackgroundChangeHistoryQueryModel query)
        {
            try
            {
                // 驗證查詢參數
                if (query.PageNumber < 1)
                {
                    query.PageNumber = 1;
                }

                if (query.PageSize < 1 || query.PageSize > 100)
                {
                    query.PageSize = 20;
                }

                // 如果有結束日期但沒有開始日期，設定預設開始日期為30天前
                if (query.EndDate.HasValue && !query.StartDate.HasValue)
                {
                    query.StartDate = query.EndDate.Value.AddDays(-30);
                }

                // 呼叫服務層取得歷史記錄
                var result = await _petQueryService.GetBackgroundChangeHistoryAsync(query);

                // 將查詢參數傳遞到 ViewBag 以便視圖使用
                ViewBag.UserId = query.UserId;
                ViewBag.PetId = query.PetId;
                ViewBag.StartDate = query.StartDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = query.EndDate?.ToString("yyyy-MM-dd");
                ViewBag.PageNumber = query.PageNumber;
                ViewBag.PageSize = query.PageSize;

                return View(result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入背景變更歷史記錄時發生錯誤：{ex.Message}";
                return View(new PetBackgroundChangeHistoryPagedResult
                {
                    Items = new List<BackgroundChangeHistoryItemViewModel>(),
                    TotalCount = 0,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalPages = 0
                });
            }
        }

        // GET: AdminPet/QueryPets
        /// <summary>
        /// 查詢寵物清單（支援篩選、排序、分頁）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> QueryPets(PetAdminListQueryModel query)
        {
            try
            {
                // 驗證並標準化查詢參數
                if (query.PageNumber < 1)
                {
                    query.PageNumber = 1;
                }

                if (query.PageSize < 1 || query.PageSize > 100)
                {
                    query.PageSize = 20;
                }

                // 標準化排序參數
                if (string.IsNullOrWhiteSpace(query.SortBy))
                {
                    query.SortBy = "level_desc";
                }

                // 呼叫服務層執行查詢
                var result = await _petQueryService.GetPetListAsync(query);

                // 將查詢參數傳遞到 ViewBag 以便視圖使用
                ViewBag.UserId = query.UserId;
                ViewBag.PetName = query.PetName;
                ViewBag.SkinColor = query.SkinColor;
                ViewBag.BackgroundColor = query.BackgroundColor;
                ViewBag.SearchTerm = query.SearchTerm;
                ViewBag.SortBy = query.SortBy;
                ViewBag.PageNumber = query.PageNumber;
                ViewBag.PageSize = query.PageSize;

                // 從資料庫讀取所有膚色選項（直接從 SQL Server 讀取，不可硬編碼）
                var skinColors = await _context.PetSkinColorCostSettings
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.ColorName)
                    .Select(s => new { Code = s.ColorCode, Name = s.ColorName })
                    .ToListAsync();
                ViewBag.SkinColors = skinColors;

                // 從資料庫讀取所有背景選項（直接從 SQL Server 讀取，不可硬編碼）
                var backgroundColors = await _context.PetBackgroundCostSettings
                    .Where(b => !b.IsDeleted)
                    .OrderBy(b => b.BackgroundName)
                    .Select(b => new { Code = b.BackgroundCode, Name = b.BackgroundName })
                    .ToListAsync();
                ViewBag.BackgroundColors = backgroundColors;

                // 修正：計算統計資訊應該從所有篩選結果計算，不是只從當前分頁
                // 使用模糊搜尋來計算統計值
                if (result.TotalCount > 0)
                {
                    // 重新構建相同的篩選條件但不分頁，用於計算統計
                    var allPets = await _context.Pets
                        .Include(p => p.User)
                        .AsNoTracking()
                        .ToListAsync();

                    // 應用模糊搜尋篩選條件
                    var hasUserId = query.UserId.HasValue;
                    var hasSearchTerm = !string.IsNullOrWhiteSpace(query.SearchTerm);
                    var hasExtraPetName = !string.IsNullOrWhiteSpace(query.PetName);

                    List<int> matchedPetIds = new List<int>();

                    if (hasUserId || hasSearchTerm || hasExtraPetName)
                    {
                        foreach (var pet in allPets)
                        {
                            bool matched = false;

                            // UserId 匹配
                            if (hasUserId && pet.UserId == query.UserId.Value)
                            {
                                matched = true;
                            }

                            // SearchTerm 模糊搜尋（使用 FuzzySearchService）
                            if (!matched && hasSearchTerm)
                            {
                                var term = query.SearchTerm.Trim();
                                var userName = pet.User?.UserName ?? "";
                                var petName = pet.PetName ?? "";

                                int priority = _fuzzySearchService.CalculateMatchPriority(term, userName, petName);
                                if (priority > 0)
                                {
                                    matched = true;
                                }
                            }

                            // PetName 模糊搜尋
                            if (!matched && hasExtraPetName)
                            {
                                var term = query.PetName.Trim();
                                int priority = _fuzzySearchService.CalculateMatchPriority(term, pet.PetName ?? "");
                                if (priority > 0)
                                {
                                    matched = true;
                                }
                            }

                            if (matched)
                            {
                                matchedPetIds.Add(pet.PetId);
                            }
                        }

                        allPets = allPets.Where(p => matchedPetIds.Contains(p.PetId)).ToList();
                    }

                    // SkinColor 篩選
                    if (!string.IsNullOrWhiteSpace(query.SkinColor))
                    {
                        allPets = allPets.Where(p => p.SkinColor != null && p.SkinColor.Contains(query.SkinColor.Trim())).ToList();
                    }

                    // BackgroundColor 篩選
                    if (!string.IsNullOrWhiteSpace(query.BackgroundColor))
                    {
                        allPets = allPets.Where(p => p.BackgroundColor != null && p.BackgroundColor.Contains(query.BackgroundColor.Trim())).ToList();
                    }

                    // 計算統計值（從所有篩選結果）
                    ViewBag.TotalPets = result.TotalCount;
                    ViewBag.HealthyPets = allPets.Count(p => p.Health >= 80);
                    ViewBag.AverageLevel = allPets.Any() ? allPets.Average(p => (double)p.Level) : 0;
                    ViewBag.MaxPetLevel = allPets.Any() ? allPets.Max(p => p.Level) : 0;
                }
                else
                {
                    ViewBag.TotalPets = 0;
                    ViewBag.HealthyPets = 0;
                    ViewBag.AverageLevel = 0;
                    ViewBag.MaxPetLevel = 0;
                }

                return View(result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢寵物資料時發生錯誤：{ex.Message}";
                return View(new PetAdminListPagedResult
                {
                    Items = new List<PetAdminListItemViewModel>(),
                    TotalCount = 0,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalPages = 0
                });
            }
        }

        // GET: AdminPet/PetDetail/5
        /// <summary>
        /// 顯示寵物詳細資料
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PetDetail(int id)
        {
            try
            {
                // 驗證參數
                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "無效的寵物 ID";
                    return RedirectToAction(nameof(QueryPets));
                }

                // 呼叫服務層取得寵物詳細資料
                var petDetail = await _petQueryService.GetPetDetailAsync(id);

                // 檢查寵物是否存在
                if (petDetail == null)
                {
                    TempData["ErrorMessage"] = $"找不到 ID 為 {id} 的寵物";
                    return RedirectToAction(nameof(QueryPets));
                }

                // 將寵物 ID 傳遞到 ViewBag 以便視圖使用
                ViewBag.PetId = id;

                return View(petDetail);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入寵物詳細資料時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(QueryPets));
            }
        }

        // GET: AdminPet/IndividualSettings/5
        /// <summary>
        /// 顯示個別寵物設定編輯表單（名稱、膚色、背景）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> IndividualSettings(int? petId)
        {
            if (!petId.HasValue || petId <= 0)
            {
                // 顯示寵物選擇頁面而不是重定向
                try
                {
                    var pets = await _petService.GetAllPetsAsync();

                    // 載入背景設定以供顯示
                    var backgrounds = await _context.PetBackgroundCostSettings
                        .AsNoTracking()
                        .ToDictionaryAsync(b => b.BackgroundCode, b => b.BackgroundName);
                    ViewBag.Backgrounds = backgrounds;

                    return View("PetSelection", pets);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"載入寵物列表時發生錯誤：{ex.Message}";
                    return RedirectToAction(nameof(QueryPets));
                }
            }

            var pet = await _petService.GetPetByIdAsync(petId.Value);
            if (pet == null)
            {
                TempData["ErrorMessage"] = $"找不到寵物 ID: {petId.Value}";
                return RedirectToAction(nameof(Index));
            }

            // 取得用戶資訊
            var user = await _context.Users.FindAsync(pet.UserId);

            var model = new PetUpdateModel
            {
                PetId = pet.PetId,
                PetName = pet.PetName,
                SkinColor = pet.SkinColor,
                BackgroundColor = pet.BackgroundColor,
                Level = pet.Level,
                Experience = pet.Experience,
                Hunger = pet.Hunger,
                Mood = pet.Mood,
                Stamina = pet.Stamina,
                Cleanliness = pet.Cleanliness,
                Health = pet.Health
            };

            ViewBag.PetId = petId;
            ViewBag.UserId = pet.UserId;
            ViewBag.UserName = user?.UserName ?? "未知";
            ViewBag.CurrentPetName = pet.PetName;

            // 從資料庫讀取所有膚色選項（直接從 SQL Server 讀取，不可硬編碼）
            var skinColors = await _context.PetSkinColorCostSettings
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.ColorName)
                .Select(s => new { Code = s.ColorCode, Name = s.ColorName })
                .ToListAsync();
            ViewBag.SkinColors = skinColors;

            // 從資料庫讀取所有背景選項（直接從 SQL Server 讀取，不可硬編碼）
            var backgroundColors = await _context.PetBackgroundCostSettings
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.BackgroundName)
                .Select(b => new { Code = b.BackgroundCode, Name = b.BackgroundName })
                .ToListAsync();
            ViewBag.BackgroundColors = backgroundColors;

            // 獲取當前膚色的名稱用於顯示
            var currentSkinColor = await _context.PetSkinColorCostSettings
                .Where(s => !s.IsDeleted && s.ColorCode == pet.SkinColor)
                .Select(s => s.ColorName)
                .FirstOrDefaultAsync();
            ViewBag.CurrentSkinColorName = currentSkinColor ?? "未知";

            // 獲取當前背景的名稱用於顯示
            var currentBackgroundColor = await _context.PetBackgroundCostSettings
                .Where(b => !b.IsDeleted && b.BackgroundCode == pet.BackgroundColor)
                .Select(b => b.BackgroundName)
                .FirstOrDefaultAsync();
            ViewBag.CurrentBackgroundColorName = currentBackgroundColor ?? "未知";

            return View(model);
        }

        // POST: AdminPet/EditPet
        /// <summary>
        /// 儲存個別寵物變更（名稱、膚色、背景）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPet(PetUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "輸入資料驗證失敗，請檢查表單內容";
                return RedirectToAction(nameof(IndividualSettings), new { petId = model.PetId });
            }

            var pet = await _petService.GetPetByIdAsync(model.PetId);
            if (pet == null)
            {
                TempData["ErrorMessage"] = $"找不到寵物 ID: {model.PetId}";
                return RedirectToAction(nameof(Index));
            }

            var operatorId = 0;

            var hasNameChange = !string.IsNullOrWhiteSpace(model.PetName) && model.PetName != pet.PetName;
            var hasSkinColorChange = !string.IsNullOrWhiteSpace(model.SkinColor) && model.SkinColor != pet.SkinColor;
            var hasBackgroundColorChange = !string.IsNullOrWhiteSpace(model.BackgroundColor) && model.BackgroundColor != pet.BackgroundColor;

            var successMessages = new List<string>();
            var errorMessages = new List<string>();

            if (hasNameChange)
            {
                var nameInputModel = new PetBasicInfoInputModel
                {
                    PetId = model.PetId,
                    PetName = model.PetName,
                    UserId = pet.UserId
                };

                var nameResult = await _petMutationService.UpdatePetBasicInfoAsync(model.PetId, nameInputModel, operatorId);

                if (nameResult.Success)
                {
                    successMessages.Add($"寵物名稱已更新為「{model.PetName}」");
                }
                else
                {
                    errorMessages.Add($"名稱更新失敗：{nameResult.Message}");
                }
            }

            if (hasSkinColorChange || hasBackgroundColorChange)
            {
                var appearanceInputModel = new PetAppearanceInputModel
                {
                    PetId = model.PetId,
                    SkinColor = hasSkinColorChange ? model.SkinColor : pet.SkinColor,
                    BackgroundColor = hasBackgroundColorChange ? model.BackgroundColor : pet.BackgroundColor,
                    PointsCost = 0,
                    Description = "管理員調整寵物外觀"
                };

                var appearanceResult = await _petMutationService.UpdatePetAppearanceAsync(model.PetId, appearanceInputModel, operatorId);

                if (appearanceResult.Success)
                {
                    if (hasSkinColorChange && hasBackgroundColorChange)
                    {
                        successMessages.Add($"膚色和背景已更新");
                    }
                    else if (hasSkinColorChange)
                    {
                        successMessages.Add($"膚色已更新為 {model.SkinColor}");
                    }
                    else if (hasBackgroundColorChange)
                    {
                        successMessages.Add($"背景已更新為 {model.BackgroundColor}");
                    }
                }
                else
                {
                    errorMessages.Add($"外觀更新失敗：{appearanceResult.Message}");
                }
            }

            if (!hasNameChange && !hasSkinColorChange && !hasBackgroundColorChange)
            {
                TempData["InfoMessage"] = "沒有變更任何資料";
                return RedirectToAction(nameof(IndividualSettings), new { petId = model.PetId });
            }

            if (successMessages.Any())
            {
                TempData["SuccessMessage"] = string.Join("；", successMessages);
            }

            if (errorMessages.Any())
            {
                TempData["ErrorMessage"] = string.Join("；", errorMessages);
            }

            return RedirectToAction(nameof(IndividualSettings), new { petId = model.PetId });
        }

        // GET: AdminPet/SystemRules
        /// <summary>
        /// 顯示寵物系統規則設定頁面
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SystemRules()
        {
            try
            {
                // 從 SystemSettings 讀取現有設定
                // 注意：實際的互動獎勵使用 Pet.Interaction.* 設定，而非 Pet.*Bonus
                var feedBonus = await GetSystemSettingIntAsync("Pet.Interaction.Feed.HungerIncrease", 10);
                var cleanBonus = await GetSystemSettingIntAsync("Pet.Interaction.Bath.CleanlinessIncrease", 10);
                var playBonus = await GetSystemSettingIntAsync("Pet.Interaction.Coax.MoodIncrease", 10);
                var sleepBonus = await GetSystemSettingIntAsync("Pet.Interaction.Rest.StaminaIncrease", 10);

                var model = new PetSystemRulesInputModel
                {
                    LevelUpExpBase = await GetSystemSettingIntAsync("Pet.LevelUpExpBase", 100),
                    LevelUpFormula = "Level 1-10: 40×level+60; 11-100: 0.8×level²+380; ≥101: 285.69×1.06^level",
                    FeedBonus = feedBonus,
                    CleanBonus = cleanBonus,
                    PlayBonus = playBonus,
                    SleepBonus = sleepBonus,
                    ExpBonus = await GetSystemSettingIntAsync("Pet.ExpBonus", 1),
                    ColorChangePoints = await GetSystemSettingIntAsync("Pet.ColorChange.PointsCost", 2000),
                    BackgroundChangePoints = await GetSystemSettingIntAsync("Pet.BackgroundChange.PointsCost", 1000),
                    DailyDecayHunger = await GetSystemSettingIntAsync("Pet.DailyDecay.Hunger", 20),
                    DailyDecayMood = await GetSystemSettingIntAsync("Pet.DailyDecay.Mood", 30),
                    DailyDecayStamina = await GetSystemSettingIntAsync("Pet.DailyDecay.Stamina", 10),
                    DailyDecayCleanliness = await GetSystemSettingIntAsync("Pet.DailyDecay.Cleanliness", 20),
                    AvailableColors = await GetSystemSettingAsync("Pet.AvailableColors", "#FFFFFF,#FFD700,#FF6B6B,#4ECDC4,#45B7D1,#FFA07A,#98D8C8,#F7DC6F,#BB8FCE,#85C1E2"),
                    AvailableBackgrounds = await GetSystemSettingAsync("Pet.AvailableBackgrounds", "#FFFFFF,#F0F0F0,#E8F5E9,#E3F2FD,#FFF3E0,#FCE4EC,#F3E5F5,#E0F2F1,#FFF8E1,#EFEBE9")
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入系統規則設定時發生錯誤：{ex.Message}";
                return View(new PetSystemRulesInputModel());
            }
        }

        // POST: AdminPet/UpdateSystemRules
        /// <summary>
        /// 更新寵物系統規則設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSystemRules(PetSystemRulesInputModel model)
        {
            try
            {
                // 驗證 ModelState
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "輸入資料驗證失敗，請檢查所有欄位";
                    return View("SystemRules", model);
                }

                // 獲取當前管理員ID（如果沒有則使用0作為系統管理員）
                var managerId = GetCurrentManagerId() ?? 0;

                // 呼叫 PetMutationService 更新系統規則
                var result = await _petMutationService.UpdatePetSystemRulesAsync(model, managerId);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;

                    // 記錄操作日誌
                    await LogOperationAsync(
                        "UpdatePetSystemRules",
                        $"更新寵物系統規則：升級基礎經驗={model.LevelUpExpBase}, 餵食獎勵={model.FeedBonus}, 清潔獎勵={model.CleanBonus}, 玩耍獎勵={model.PlayBonus}, 哄睡獎勵={model.SleepBonus}, 經驗值倍數={model.ExpBonus}, 換色點數={model.ColorChangePoints}, 換背景點數={model.BackgroundChangePoints}"
                    );

                    return RedirectToAction(nameof(SystemRules));
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                    return View("SystemRules", model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"更新系統規則時發生錯誤：{ex.Message}";
                return View("SystemRules", model);
            }
        }

        // ==================== AJAX 端點 (用於 QueryPets 頁面) ====================

        /// <summary>
        /// AJAX: 搜尋寵物（用於 QueryPets 頁面的動態搜尋）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchPets(int? userId, string userName, string petName, string petType,
            int? minLevel, int? maxLevel, string color, string sortBy, string sortOrder)
        {
            try
            {
                // 構建查詢模型
                var query = new PetAdminListQueryModel
                {
                    UserId = userId,
                    SearchTerm = userName,
                    PetName = petName,
                    SkinColor = color,
                    SortBy = sortBy ?? "level_desc",
                    PageNumber = 1,
                    PageSize = 100  // AJAX 搜尋返回更多結果
                };

                // 執行查詢
                var result = await _petQueryService.GetPetListAsync(query);

                // 構建返回數據
                var data = result.Items.Select(p => new
                {
                    petId = p.PetId,
                    userId = p.UserId,
                    userName = p.UserName ?? "未知",
                    petName = p.PetName,
                    petType = "slime",  // Pet entity 沒有 PetType，使用預設值
                    level = p.Level,
                    status = DetermineStatus(p.Health, p.Hunger, p.Mood),
                    color = p.SkinColor ?? "default"
                }).ToList();

                // 計算統計資訊
                // 健康寵物: Health >= 20
                // 需照顧寵物: Health < 20
                var statistics = new
                {
                    totalPets = result.TotalCount,
                    activePets = result.Items.Count(p => p.Health >= 20),
                    avgLevel = result.Items.Any() ? result.Items.Average(p => (double)p.Level) : 0,
                    needCarePets = result.Items.Count(p => p.Health < 20)
                };

                return Json(new { success = true, data, statistics });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"搜尋失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// AJAX: 獲取寵物詳細資訊
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPetDetail(int petId)
        {
            try
            {
                if (petId <= 0)
                {
                    return Json(new { success = false, message = "無效的寵物 ID" });
                }

                var pet = await _petService.GetPetByIdAsync(petId);
                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到該寵物" });
                }

                // 獲取用戶資訊
                var user = await _context.Users.FindAsync(pet.UserId);

                var data = new
                {
                    userId = pet.UserId,
                    userName = user?.UserName ?? "未知",
                    petName = pet.PetName,
                    petType = "slime",  // Pet entity 沒有 PetType
                    level = pet.Level,
                    hp = pet.Health,
                    attack = 0,  // Pet entity 沒有 Attack 屬性
                    defense = 0,  // Pet entity 沒有 Defense 屬性
                    speed = 0,  // Pet entity 沒有 Speed 屬性
                    status = DetermineStatus(pet.Health, pet.Hunger, pet.Mood),
                    color = pet.SkinColor,
                    size = "medium",  // Pet entity 沒有 Size 屬性
                    specialEffects = pet.BackgroundColor,
                    createTime = pet.LevelUpTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    lastUpdate = pet.LevelUpTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    lastFeed = pet.LevelUpTime.ToString("yyyy-MM-dd HH:mm:ss")
                };

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"獲取寵物詳情失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// AJAX: 調整寵物（主要用於調整等級）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustPet(int petId, int newLevel)
        {
            try
            {
                if (petId <= 0)
                {
                    return Json(new { success = false, message = "無效的寵物 ID" });
                }

                if (newLevel < 1 || newLevel > 100)
                {
                    return Json(new { success = false, message = "等級必須在 1-100 之間" });
                }

                var pet = await _petService.GetPetByIdAsync(petId);
                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到該寵物" });
                }

                // 獲取當前管理員ID
                var managerId = GetCurrentManagerId() ?? 0;

                // 使用 PetMutationService 調整寵物等級
                var statsInput = new PetStatsInputModel
                {
                    PetId = petId,
                    Level = newLevel,
                    Experience = 0,  // 設置為該等級的起始經驗值
                    Hunger = pet.Hunger,
                    Happiness = pet.Mood,  // Map Mood to Happiness
                    Mood = pet.Mood,
                    Stamina = pet.Stamina,
                    Cleanliness = pet.Cleanliness,
                    Health = pet.Health
                };

                var result = await _petMutationService.UpdatePetStatsAsync(petId, statsInput, managerId);

                if (result.Success)
                {
                    // 記錄操作日誌
                    await LogOperationAsync(
                        "AdjustPet",
                        $"調整寵物 {pet.PetName} (ID:{petId}) 等級從 {pet.Level} 到 {newLevel}"
                    );

                    return Json(new { success = true, message = $"寵物等級已調整為 {newLevel}" });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"調整寵物失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// AJAX: 刪除寵物
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePet(int petId)
        {
            try
            {
                if (petId <= 0)
                {
                    return Json(new { success = false, message = "無效的寵物 ID" });
                }

                var pet = await _petService.GetPetByIdAsync(petId);
                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到該寵物" });
                }

                var petName = pet.PetName;
                var userId = pet.UserId;

                // 刪除寵物
                var result = await _petService.DeletePetAsync(petId);

                if (result)
                {
                    // 記錄操作日誌
                    await LogOperationAsync(
                        "DeletePet",
                        $"刪除寵物 {petName} (ID:{petId}, UserId:{userId})"
                    );

                    return Json(new { success = true, message = "寵物已成功刪除" });
                }
                else
                {
                    return Json(new { success = false, message = "刪除寵物失敗" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"刪除寵物失敗：{ex.Message}" });
            }
        }


        // ==================== 私有輔助方法 ====================

        /// <summary>
        /// 根據寵物屬性判斷狀態
        /// </summary>
        private string DetermineStatus(int health, int hunger, int mood)
        {
            if (health <= 0) return "死亡";
            if (health < 30 || hunger < 20 || mood < 20) return "生病";
            if (hunger < 50) return "睡眠";
            return "活躍";
        }

        // ==================== 膚色與背景管理 API ====================

        /// <summary>
        /// 獲取所有膚色選項
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSkinColors()
        {
            try
            {
                var colors = await _context.PetSkinColorCostSettings
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.DisplayOrder)
                    .Select(s => new
                    {
                        s.SettingId,
                        s.ColorCode,
                        s.ColorName,
                        s.PointsCost,
                        s.ColorHex,
                        s.IsActive,
                        s.DisplayOrder
                    })
                    .ToListAsync();

                return Json(colors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取膚色選項失敗");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 獲取所有背景選項
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBackgrounds()
        {
            try
            {
                var backgrounds = await _context.PetBackgroundCostSettings
                    .Where(b => !b.IsDeleted)
                    .OrderBy(b => b.DisplayOrder)
                    .Select(b => new
                    {
                        b.SettingId,
                        b.BackgroundCode,
                        b.BackgroundName,
                        b.PointsCost,
                        b.IsActive,
                        b.DisplayOrder
                    })
                    .ToListAsync();

                return Json(backgrounds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取背景選項失敗");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 新增膚色選項
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddSkinColor(string colorCode, string colorName, int pointsCost)
        {
            try
            {
                var maxOrder = await _context.PetSkinColorCostSettings
                    .Where(s => !s.IsDeleted)
                    .MaxAsync(s => (int?)s.DisplayOrder) ?? 0;

                var setting = new PetSkinColorCostSetting
                {
                    ColorCode = colorCode,
                    ColorName = colorName,
                    ColorHex = colorCode,
                    PointsCost = pointsCost,
                    Rarity = "Common",
                    IsActive = true,
                    DisplayOrder = maxOrder + 1,
                    IsFree = pointsCost == 0,
                    IsLimitedEdition = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PetSkinColorCostSettings.Add(setting);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "膚色新增成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增膚色失敗");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 新增背景選項
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddBackground(string backgroundCode, string backgroundName, int pointsCost)
        {
            try
            {
                var maxOrder = await _context.PetBackgroundCostSettings
                    .Where(b => !b.IsDeleted)
                    .MaxAsync(b => (int?)b.DisplayOrder) ?? 0;

                var setting = new PetBackgroundCostSetting
                {
                    BackgroundCode = backgroundCode,
                    BackgroundName = backgroundName,
                    PointsCost = pointsCost,
                    Rarity = "Common",
                    IsActive = true,
                    DisplayOrder = maxOrder + 1,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PetBackgroundCostSettings.Add(setting);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "背景新增成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增背景失敗");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 更新膚色所需點數
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateSkinColorPoints(int settingId, int pointsCost)
        {
            try
            {
                var setting = await _context.PetSkinColorCostSettings.FindAsync(settingId);
                if (setting == null || setting.IsDeleted)
                {
                    return Json(new { success = false, message = "找不到膚色設定" });
                }

                setting.PointsCost = pointsCost;
                setting.IsFree = pointsCost == 0;
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新膚色點數失敗");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 更新背景所需點數
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateBackgroundPoints(int settingId, int pointsCost)
        {
            try
            {
                var setting = await _context.PetBackgroundCostSettings.FindAsync(settingId);
                if (setting == null || setting.IsDeleted)
                {
                    return Json(new { success = false, message = "找不到背景設定" });
                }

                setting.PointsCost = pointsCost;
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新背景點數失敗");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 切換膚色啟用狀態
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleSkinColorActive(int settingId, bool isActive)
        {
            try
            {
                var setting = await _context.PetSkinColorCostSettings.FindAsync(settingId);
                if (setting == null || setting.IsDeleted)
                {
                    return Json(new { success = false, message = "找不到膚色設定" });
                }

                setting.IsActive = isActive;
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換膚色狀態失敗");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 切換背景啟用狀態
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleBackgroundActive(int settingId, bool isActive)
        {
            try
            {
                var setting = await _context.PetBackgroundCostSettings.FindAsync(settingId);
                if (setting == null || setting.IsDeleted)
                {
                    return Json(new { success = false, message = "找不到背景設定" });
                }

                setting.IsActive = isActive;
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換背景狀態失敗");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 刪除膚色選項（軟刪除）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteSkinColor(int settingId)
        {
            try
            {
                var setting = await _context.PetSkinColorCostSettings.FindAsync(settingId);
                if (setting == null || setting.IsDeleted)
                {
                    return Json(new { success = false, message = "找不到膚色設定" });
                }

                setting.IsDeleted = true;
                setting.DeletedAt = DateTime.UtcNow;
                setting.DeletedBy = GetCurrentManagerId();

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "膚色刪除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除膚色失敗");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 刪除背景選項（軟刪除）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteBackground(int settingId)
        {
            try
            {
                var setting = await _context.PetBackgroundCostSettings.FindAsync(settingId);
                if (setting == null || setting.IsDeleted)
                {
                    return Json(new { success = false, message = "找不到背景設定" });
                }

                setting.IsDeleted = true;
                setting.DeletedAt = DateTime.UtcNow;
                setting.DeletedBy = GetCurrentManagerId();

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "背景刪除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除背景失敗");
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}

