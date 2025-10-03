using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme)]
    public class AdminPetController : MiniGameBaseController
    {
        private readonly IPetService _petService;
        private readonly IPetRulesService _petRulesService;

        public AdminPetController(
            GameSpacedatabaseContext context,
            IPetService petService,
            IPetRulesService petRulesService)
            : base(context)
        {
            _petService = petService;
            _petRulesService = petRulesService;
        }

        // GET: AdminPet
        public async Task<IActionResult> Index(string searchTerm = "", string rarity = "", string sortBy = "name", int page = 1, int pageSize = 10)
        {
            var pets = await _petService.GetAllPetsAsync();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                pets = pets.Where(p => p.PetName.Contains(searchTerm) ||
                                      p.PetType.Contains(searchTerm) ||
                                      p.PetDescription.Contains(searchTerm));
            }

            // 稀有度篩選
            if (!string.IsNullOrEmpty(rarity))
            {
                pets = pets.Where(p => p.Rarity == rarity);
            }

            // 排序
            pets = sortBy switch
            {
                "type" => pets.OrderBy(p => p.PetType),
                "rarity" => pets.OrderBy(p => p.Rarity),
                "rate" => pets.OrderByDescending(p => p.DropRate),
                "created" => pets.OrderByDescending(p => p.CreatedAt),
                _ => pets.OrderBy(p => p.PetName)
            };

            // 分頁
            var totalCount = pets.Count();
            var pagedPets = pets
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new AdminPetIndexViewModel
            {
                Pets = new PagedResult<Pet>
                {
                    Items = pagedPets,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                }
            };

            // 設定 ViewBag 用於搜尋和篩選
            var allPets = await _petService.GetAllPetsAsync();
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Rarity = rarity;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalPets = allPets.Count();
            ViewBag.CommonPets = allPets.Count(p => p.Rarity == "普通");
            ViewBag.RarePets = allPets.Count(p => p.Rarity == "稀有");
            ViewBag.EpicPets = allPets.Count(p => p.Rarity == "史詩");
            ViewBag.LegendaryPets = allPets.Count(p => p.Rarity == "傳說");

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
                var pet = new Pet
                {
                    PetName = model.PetName,
                    PetDescription = model.PetDescription,
                    PetType = model.PetType,
                    Rarity = model.Rarity,
                    DropRate = model.DropRate,
                    PetImageUrl = model.PetImageUrl,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    // 初始化寵物狀態
                    Level = 1,
                    Experience = 0,
                    Hunger = 100,
                    Mood = 100,
                    Stamina = 100,
                    Cleanliness = 100,
                    Health = 100
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
                PetName = pet.PetName,
                PetDescription = pet.PetDescription,
                PetType = pet.PetType,
                Rarity = pet.Rarity,
                DropRate = pet.DropRate,
                PetImageUrl = pet.PetImageUrl,
                IsActive = pet.IsActive
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

                pet.PetName = model.PetName;
                pet.PetDescription = model.PetDescription;
                pet.PetType = model.PetType;
                pet.Rarity = model.Rarity;
                pet.DropRate = model.DropRate;
                pet.PetImageUrl = model.PetImageUrl;
                pet.IsActive = model.IsActive;

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

        // 切換寵物狀態
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet != null)
            {
                pet.IsActive = !pet.IsActive;
                var result = await _petService.UpdatePetAsync(pet);

                if (result)
                {
                    return Json(new { success = true, isActive = pet.IsActive });
                }
            }

            return Json(new { success = false });
        }

        // 獲取寵物統計數據
        [HttpGet]
        public async Task<IActionResult> GetPetStats()
        {
            var allPets = await _petService.GetAllPetsAsync();
            var stats = new
            {
                total = allPets.Count(),
                active = allPets.Count(p => p.IsActive),
                common = allPets.Count(p => p.Rarity == "普通"),
                rare = allPets.Count(p => p.Rarity == "稀有"),
                epic = allPets.Count(p => p.Rarity == "史詩"),
                legendary = allPets.Count(p => p.Rarity == "傳說")
            };

            return Json(stats);
        }

        // 獲取寵物稀有度分佈
        [HttpGet]
        public async Task<IActionResult> GetPetRarityDistribution()
        {
            var allPets = await _petService.GetAllPetsAsync();
            var total = allPets.Count();

            var distribution = allPets
                .GroupBy(p => p.Rarity)
                .Select(g => new
                {
                    rarity = g.Key,
                    count = g.Count(),
                    percentage = total > 0 ? (double)g.Count() / total * 100 : 0
                })
                .ToList();

            return Json(distribution);
        }

        // 獲取寵物類型分佈
        [HttpGet]
        public async Task<IActionResult> GetPetTypeDistribution()
        {
            var allPets = await _petService.GetAllPetsAsync();

            var distribution = allPets
                .GroupBy(p => p.PetType)
                .Select(g => new
                {
                    type = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(g => g.count)
                .ToList();

            return Json(distribution);
        }

        // 獲取寵物獲得率統計
        [HttpGet]
        public async Task<IActionResult> GetPetDropRateStats()
        {
            var allPets = await _petService.GetAllPetsAsync();

            var stats = allPets
                .GroupBy(p => p.Rarity)
                .Select(g => new
                {
                    rarity = g.Key,
                    avgDropRate = g.Average(p => p.DropRate),
                    minDropRate = g.Min(p => p.DropRate),
                    maxDropRate = g.Max(p => p.DropRate)
                })
                .ToList();

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
        public async Task<IActionResult> UpdatePetColorOption(int id, string colorName, string colorValue, int displayOrder, bool isActive)
        {
            try
            {
                var options = await _petRulesService.GetAllColorOptionsAsync();
                var option = options.FirstOrDefault(o => o.OptionID == id);

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
        public async Task<IActionResult> UpdatePetLevelUpRules(List<PetLevelUpRule> rules)
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
        public async Task<IActionResult> UpdatePetInteractionBonusRules(List<PetInteractionBonusRules> rules)
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
                var setting = settings.FirstOrDefault(s => s.Level == 1);

                if (setting != null)
                {
                    setting.PointsCost = pointsRequired;
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
        public async Task<IActionResult> UpdatePetBackgroundChangeCost(int pointsRequired)
        {
            try
            {
                if (pointsRequired < 0)
                {
                    return Json(new { success = false, message = "所需點數不能為負數" });
                }

                var settings = await _petRulesService.GetAllBackgroundSettingsAsync();
                var setting = settings.FirstOrDefault(s => s.Level == 1);

                if (setting != null)
                {
                    setting.PointsCost = pointsRequired;
                    await _petRulesService.UpdateBackgroundSettingAsync(setting);
                }

                return Json(new { success = true, message = "寵物換背景所需點數設定成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

