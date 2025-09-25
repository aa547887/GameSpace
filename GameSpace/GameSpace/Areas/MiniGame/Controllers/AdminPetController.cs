using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(Policy = "CanPet")] // Requires Pet permission
    public class AdminPetController : Controller
    {
        private readonly IMiniGameAdminService _adminService;
        private readonly IMiniGameAdminAuthService _authService;

        public AdminPetController(IMiniGameAdminService adminService, IMiniGameAdminAuthService authService)
        {
            _adminService = adminService;
            _authService = authService;
        }

        public async Task<IActionResult> Index(PetQueryModel query)
        {
            var model = new AdminPetIndexViewModel
            {
                Pets = await _adminService.GetPetsAsync(query),
                PetSummary = await _adminService.GetPetSummaryAsync(),
                Query = query,
                Sidebar = new SidebarViewModel()
            };
            return View(model);
        }

        public async Task<IActionResult> Rules()
        {
            var model = new AdminPetRulesViewModel
            {
                PetRule = await _adminService.GetPetRuleAsync(),
                Sidebar = new SidebarViewModel()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRules(PetRuleUpdateModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _adminService.UpdatePetRuleAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "寵物規則更新成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "寵物規則更新失敗";
                }
            }
            return RedirectToAction("Rules");
        }

        public async Task<IActionResult> Details(int petId)
        {
            var model = new AdminPetDetailsViewModel
            {
                Pet = await _adminService.GetPetDetailAsync(petId),
                Sidebar = new SidebarViewModel()
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int petId)
        {
            var pet = _adminService.GetPetDetailAsync(petId).Result;
            if (pet == null)
            {
                TempData["ErrorMessage"] = "找不到指定的寵物";
                return RedirectToAction("Index");
            }

            var model = new AdminPetEditViewModel
            {
                Pet = pet,
                Sidebar = new SidebarViewModel()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int petId, PetUpdateModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _adminService.UpdatePetDetailsAsync(petId, model);
                if (success)
                {
                    TempData["SuccessMessage"] = "寵物資料更新成功";
                    return RedirectToAction("Details", new { petId });
                }
                else
                {
                    TempData["ErrorMessage"] = "寵物資料更新失敗";
                }
            }
            return View(new AdminPetEditViewModel { Pet = await _adminService.GetPetDetailAsync(petId), Sidebar = new SidebarViewModel() });
        }

        public async Task<IActionResult> SkinColorChangeLog(PetQueryModel query)
        {
            var model = new AdminPetSkinColorChangeLogViewModel
            {
                SkinColorChangeLogs = await _adminService.GetPetSkinColorChangeLogsAsync(query),
                Query = query,
                Sidebar = new SidebarViewModel()
            };
            return View(model);
        }

        public async Task<IActionResult> BackgroundColorChangeLog(PetQueryModel query)
        {
            var model = new AdminPetBackgroundColorChangeLogViewModel
            {
                BackgroundColorChangeLogs = await _adminService.GetPetBackgroundColorChangeLogsAsync(query),
                Query = query,
                Sidebar = new SidebarViewModel()
            };
            return View(model);
        }
    }
}
