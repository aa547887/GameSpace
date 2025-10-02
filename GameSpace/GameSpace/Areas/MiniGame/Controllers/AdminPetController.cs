using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Data;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminPetController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public AdminPetController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // GET: AdminPet
        public async Task<IActionResult> Index(string searchTerm = "", string rarity = "", string sortBy = "name", int page = 1, int pageSize = 10)
        {
            var query = _context.Pet.AsQueryable();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.PetName.Contains(searchTerm) || 
                                       p.PetType.Contains(searchTerm) || 
                                       p.PetDescription.Contains(searchTerm));
            }

            // 稀有度篩選
            if (!string.IsNullOrEmpty(rarity))
            {
                query = query.Where(p => p.Rarity == rarity);
            }

            // 排序
            query = sortBy switch
            {
                "type" => query.OrderBy(p => p.PetType),
                "rarity" => query.OrderBy(p => p.Rarity),
                "rate" => query.OrderByDescending(p => p.DropRate),
                "created" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.PetName)
            };

            // 分頁
            var totalCount = await query.CountAsync();
            var pets = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new AdminPetIndexViewModel
            {
                Pets = new PagedResult<Pet>
                {
                    Items = pets,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                }
            };

            // 設定 ViewBag 用於搜尋和篩選
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Rarity = rarity;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalPets = totalCount;
            ViewBag.CommonPets = await _context.Pet.CountAsync(p => p.Rarity == "普通");
            ViewBag.RarePets = await _context.Pet.CountAsync(p => p.Rarity == "稀有");
            ViewBag.EpicPets = await _context.Pet.CountAsync(p => p.Rarity == "史詩");
            ViewBag.LegendaryPets = await _context.Pet.CountAsync(p => p.Rarity == "傳說");

            return View(viewModel);
        }

        // GET: AdminPet/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _context.Pet
                .Include(p => p.Users)
                .FirstOrDefaultAsync(m => m.PetId == id);

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
                    CreatedAt = DateTime.Now
                };

                _context.Add(pet);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "寵物建立成功";
                return RedirectToAction(nameof(Index));
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

            var pet = await _context.Pet.FindAsync(id);
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
                try
                {
                    var pet = await _context.Pet.FindAsync(id);
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

                    _context.Update(pet);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "寵物更新成功";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PetExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
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

            var pet = await _context.Pet
                .Include(p => p.Users)
                .FirstOrDefaultAsync(m => m.PetId == id);

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
            var pet = await _context.Pet.FindAsync(id);
            if (pet != null)
            {
                _context.Pet.Remove(pet);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "寵物刪除成功";
            }

            return RedirectToAction(nameof(Index));
        }

        // 切換寵物狀態
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var pet = await _context.Pet.FindAsync(id);
            if (pet != null)
            {
                pet.IsActive = !pet.IsActive;
                _context.Update(pet);
                await _context.SaveChangesAsync();

                return Json(new { success = true, isActive = pet.IsActive });
            }

            return Json(new { success = false });
        }

        // 獲取寵物統計數據
        [HttpGet]
        public async Task<IActionResult> GetPetStats()
        {
            var stats = new
            {
                total = await _context.Pet.CountAsync(),
                active = await _context.Pet.CountAsync(p => p.IsActive),
                common = await _context.Pet.CountAsync(p => p.Rarity == "普通"),
                rare = await _context.Pet.CountAsync(p => p.Rarity == "稀有"),
                epic = await _context.Pet.CountAsync(p => p.Rarity == "史詩"),
                legendary = await _context.Pet.CountAsync(p => p.Rarity == "傳說")
            };

            return Json(stats);
        }

        // 獲取寵物稀有度分佈
        [HttpGet]
        public async Task<IActionResult> GetPetRarityDistribution()
        {
            var distribution = await _context.Pet
                .GroupBy(p => p.Rarity)
                .Select(g => new
                {
                    rarity = g.Key,
                    count = g.Count(),
                    percentage = (double)g.Count() / _context.Pet.Count() * 100
                })
                .ToListAsync();

            return Json(distribution);
        }

        // 獲取寵物類型分佈
        [HttpGet]
        public async Task<IActionResult> GetPetTypeDistribution()
        {
            var distribution = await _context.Pet
                .GroupBy(p => p.PetType)
                .Select(g => new
                {
                    type = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(g => g.count)
                .ToListAsync();

            return Json(distribution);
        }

        // 獲取寵物獲得率統計
        [HttpGet]
        public async Task<IActionResult> GetPetDropRateStats()
        {
            var stats = await _context.Pet
                .GroupBy(p => p.Rarity)
                .Select(g => new
                {
                    rarity = g.Key,
                    avgDropRate = g.Average(p => p.DropRate),
                    minDropRate = g.Min(p => p.DropRate),
                    maxDropRate = g.Max(p => p.DropRate)
                })
                .ToListAsync();

            return Json(stats);
        }

        // 新增：寵物換色所需點數設定
        [HttpGet]
        public async Task<IActionResult> GetPetColorChangeCost()
        {
            try
            {
                var cost = await _context.PetColorChangeCosts.FirstOrDefaultAsync();
                if (cost == null)
                {
                    // 如果沒有設定，返回預設值
                    return Json(new { success = true, data = new { pointsRequired = 100 } });
                }
                
                return Json(new { success = true, data = new { pointsRequired = cost.PointsRequired } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePetColorChangeCost(int pointsRequired)
        {
            try
            {
                if (pointsRequired < 0)
                {
                    return Json(new { success = false, message = "所需點數不能為負數" });
                }

                var cost = await _context.PetColorChangeCosts.FirstOrDefaultAsync();
                if (cost == null)
                {
                    cost = new PetColorChangeCost { PointsRequired = pointsRequired };
                    _context.PetColorChangeCosts.Add(cost);
                }
                else
                {
                    cost.PointsRequired = pointsRequired;
                    _context.PetColorChangeCosts.Update(cost);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "寵物換色所需點數設定成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 新增：寵物換背景所需點數設定
        [HttpGet]
        public async Task<IActionResult> GetPetBackgroundChangeCost()
        {
            try
            {
                var cost = await _context.PetBackgroundChangeCosts.FirstOrDefaultAsync();
                if (cost == null)
                {
                    // 如果沒有設定，返回預設值
                    return Json(new { success = true, data = new { pointsRequired = 150 } });
                }
                
                return Json(new { success = true, data = new { pointsRequired = cost.PointsRequired } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePetBackgroundChangeCost(int pointsRequired)
        {
            try
            {
                if (pointsRequired < 0)
                {
                    return Json(new { success = false, message = "所需點數不能為負數" });
                }

                var cost = await _context.PetBackgroundChangeCosts.FirstOrDefaultAsync();
                if (cost == null)
                {
                    cost = new PetBackgroundChangeCost { PointsRequired = pointsRequired };
                    _context.PetBackgroundChangeCosts.Add(cost);
                }
                else
                {
                    cost.PointsRequired = pointsRequired;
                    _context.PetBackgroundChangeCosts.Update(cost);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "寵物換背景所需點數設定成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 新增：寵物顏色選項管理
        [HttpGet]
        public async Task<IActionResult> GetPetColorOptions()
        {
            try
            {
                var options = await _context.PetColorOptions
                    .OrderBy(o => o.DisplayOrder)
                    .ToListAsync();
                
                return Json(new { success = true, data = options });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

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

                _context.PetColorOptions.Add(option);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物顏色選項新增成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePetColorOption(int id, string colorName, string colorValue, int displayOrder, bool isActive)
        {
            try
            {
                var option = await _context.PetColorOptions.FindAsync(id);
                if (option == null)
                {
                    return Json(new { success = false, message = "找不到指定的顏色選項" });
                }

                option.ColorName = colorName;
                option.ColorValue = colorValue;
                option.DisplayOrder = displayOrder;
                option.IsActive = isActive;

                _context.PetColorOptions.Update(option);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物顏色選項更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeletePetColorOption(int id)
        {
            try
            {
                var option = await _context.PetColorOptions.FindAsync(id);
                if (option == null)
                {
                    return Json(new { success = false, message = "找不到指定的顏色選項" });
                }

                _context.PetColorOptions.Remove(option);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物顏色選項刪除成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 新增：寵物背景選項管理
        [HttpGet]
        public async Task<IActionResult> GetPetBackgroundOptions()
        {
            try
            {
                var options = await _context.PetBackgroundOptions
                    .OrderBy(o => o.DisplayOrder)
                    .ToListAsync();
                
                return Json(new { success = true, data = options });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPetBackgroundOption(string backgroundName, string backgroundValue, int displayOrder)
        {
            try
            {
                if (string.IsNullOrEmpty(backgroundName) || string.IsNullOrEmpty(backgroundValue))
                {
                    return Json(new { success = false, message = "背景名稱和背景值不能為空" });
                }

                var option = new PetBackgroundOption
                {
                    BackgroundName = backgroundName,
                    BackgroundValue = backgroundValue,
                    DisplayOrder = displayOrder,
                    IsActive = true
                };

                _context.PetBackgroundOptions.Add(option);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物背景選項新增成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePetBackgroundOption(int id, string backgroundName, string backgroundValue, int displayOrder, bool isActive)
        {
            try
            {
                var option = await _context.PetBackgroundOptions.FindAsync(id);
                if (option == null)
                {
                    return Json(new { success = false, message = "找不到指定的背景選項" });
                }

                option.BackgroundName = backgroundName;
                option.BackgroundValue = backgroundValue;
                option.DisplayOrder = displayOrder;
                option.IsActive = isActive;

                _context.PetBackgroundOptions.Update(option);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物背景選項更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeletePetBackgroundOption(int id)
        {
            try
            {
                var option = await _context.PetBackgroundOptions.FindAsync(id);
                if (option == null)
                {
                    return Json(new { success = false, message = "找不到指定的背景選項" });
                }

                _context.PetBackgroundOptions.Remove(option);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物背景選項刪除成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 新增：升級規則詳細設定
        [HttpGet]
        public async Task<IActionResult> GetPetLevelUpRules()
        {
            try
            {
                var rules = await _context.PetLevelUpRules
                    .OrderBy(r => r.Level)
                    .ToListAsync();
                
                return Json(new { success = true, data = rules });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePetLevelUpRules(List<PetLevelUpRule> rules)
        {
            try
            {
                if (rules == null || !rules.Any())
                {
                    return Json(new { success = false, message = "升級規則不能為空" });
                }

                // 清除現有規則
                var existingRules = await _context.PetLevelUpRules.ToListAsync();
                _context.PetLevelUpRules.RemoveRange(existingRules);

                // 新增新規則
                _context.PetLevelUpRules.AddRange(rules);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物升級規則更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 新增：互動狀態增益規則設定
        [HttpGet]
        public async Task<IActionResult> GetPetInteractionBonusRules()
        {
            try
            {
                var rules = await _context.PetInteractionBonusRules
                    .OrderBy(r => r.InteractionType)
                    .ToListAsync();
                
                return Json(new { success = true, data = rules });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePetInteractionBonusRules(List<PetInteractionBonusRule> rules)
        {
            try
            {
                if (rules == null || !rules.Any())
                {
                    return Json(new { success = false, message = "互動狀態增益規則不能為空" });
                }

                // 清除現有規則
                var existingRules = await _context.PetInteractionBonusRules.ToListAsync();
                _context.PetInteractionBonusRules.RemoveRange(existingRules);

                // 新增新規則
                _context.PetInteractionBonusRules.AddRange(rules);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "寵物互動狀態增益規則更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private bool PetExists(int id)
        {
            return _context.Pet.Any(e => e.PetId == id);
        }
    }
}
