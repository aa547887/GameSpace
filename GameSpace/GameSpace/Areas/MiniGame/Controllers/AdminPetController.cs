using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Data;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminPetController : Controller
    {
        private readonly MiniGameDbContext _context;

        public AdminPetController(MiniGameDbContext context)
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

        private bool PetExists(int id)
        {
            return _context.Pet.Any(e => e.PetId == id);
        }
    }
}
