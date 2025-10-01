using Microsoft.AspNetCore.Mvc;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.ViewModels;
using GameSpace.Areas.MiniGame.Filters;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 寵物管理控制器
    /// </summary>
    [Area("MiniGame")]
    [MiniGameAdminAuthorize("Pet_Rights_Management")]
    public class PetController : Controller
    {
        private readonly IPetService _petService;
        private readonly ILogger<PetController> _logger;

        public PetController(IPetService petService, ILogger<PetController> logger)
        {
            _petService = petService;
            _logger = logger;
        }

        /// <summary>
        /// 寵物管理首頁
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            try
            {
                var pets = await _petService.GetAllPetsAsync(page, pageSize);
                
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = 1; // 實際實作時需要計算總頁數

                return View(pets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得寵物列表失敗");
                TempData["ErrorMessage"] = "取得寵物列表失敗";
                return View(new List<PetViewModel>());
            }
        }

        /// <summary>
        /// 寵物詳情
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int petId)
        {
            try
            {
                // 這裡需要根據 PetID 找到對應的 UserID
                // 暫時使用假資料，實際實作時需要查詢資料庫
                var pet = new PetViewModel
                {
                    PetID = petId,
                    UserID = 1,
                    UserAccount = "test@example.com",
                    UserName = "測試會員",
                    PetName = "測試寵物",
                    PetType = "貓咪",
                    PetLevel = 1,
                    PetExp = 0,
                    PetSkin = "default",
                    PetBackground = "default",
                    Hunger = 100,
                    Happiness = 100,
                    Health = 100,
                    Energy = 100,
                    Cleanliness = 100,
                    CreatedAt = DateTime.Now
                };

                return View(pet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得寵物詳情失敗，PetID: {PetId}", petId);
                TempData["ErrorMessage"] = "取得寵物詳情失敗";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 建立新寵物
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create(int userId)
        {
            try
            {
                var pet = new PetViewModel
                {
                    UserID = userId,
                    PetLevel = 1,
                    PetExp = 0,
                    PetSkin = "default",
                    PetBackground = "default",
                    Hunger = 100,
                    Happiness = 100,
                    Health = 100,
                    Energy = 100,
                    Cleanliness = 100
                };

                return View(pet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "顯示建立寵物頁面失敗，UserID: {UserId}", userId);
                TempData["ErrorMessage"] = "顯示建立寵物頁面失敗";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 建立新寵物
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetViewModel pet)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(pet);
                }

                var success = await _petService.CreatePetAsync(pet);
                if (success)
                {
                    TempData["SuccessMessage"] = "成功建立寵物";
                    return RedirectToAction(nameof(Details), new { petId = pet.PetID });
                }
                else
                {
                    TempData["ErrorMessage"] = "建立寵物失敗";
                    return View(pet);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立寵物失敗，UserID: {UserId}", pet.UserID);
                TempData["ErrorMessage"] = "建立寵物失敗";
                return View(pet);
            }
        }

        /// <summary>
        /// 編輯寵物
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int petId)
        {
            try
            {
                var pet = await _petService.GetUserPetAsync(1); // 實際實作時需要根據 PetID 查詢
                if (pet == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的寵物";
                    return RedirectToAction(nameof(Index));
                }

                return View(pet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "顯示編輯寵物頁面失敗，PetID: {PetId}", petId);
                TempData["ErrorMessage"] = "顯示編輯寵物頁面失敗";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 更新寵物
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PetViewModel pet)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(pet);
                }

                var success = await _petService.UpdatePetAsync(pet);
                if (success)
                {
                    TempData["SuccessMessage"] = "成功更新寵物資訊";
                    return RedirectToAction(nameof(Details), new { petId = pet.PetID });
                }
                else
                {
                    TempData["ErrorMessage"] = "更新寵物資訊失敗";
                    return View(pet);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物資訊失敗，PetID: {PetId}", pet.PetID);
                TempData["ErrorMessage"] = "更新寵物資訊失敗";
                return View(pet);
            }
        }

        /// <summary>
        /// 刪除寵物
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int petId)
        {
            try
            {
                var success = await _petService.DeletePetAsync(petId);
                if (success)
                {
                    TempData["SuccessMessage"] = "成功刪除寵物";
                }
                else
                {
                    TempData["ErrorMessage"] = "刪除寵物失敗";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除寵物失敗，PetID: {PetId}", petId);
                TempData["ErrorMessage"] = "刪除寵物失敗";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 餵食寵物
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Feed(int petId)
        {
            try
            {
                var success = await _petService.FeedPetAsync(petId);
                if (success)
                {
                    TempData["SuccessMessage"] = "成功餵食寵物";
                }
                else
                {
                    TempData["ErrorMessage"] = "餵食寵物失敗";
                }

                return RedirectToAction(nameof(Details), new { petId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "餵食寵物失敗，PetID: {PetId}", petId);
                TempData["ErrorMessage"] = "餵食寵物失敗";
                return RedirectToAction(nameof(Details), new { petId });
            }
        }

        /// <summary>
        /// 與寵物玩耍
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Play(int petId)
        {
            try
            {
                var success = await _petService.PlayWithPetAsync(petId);
                if (success)
                {
                    TempData["SuccessMessage"] = "成功與寵物玩耍";
                }
                else
                {
                    TempData["ErrorMessage"] = "與寵物玩耍失敗";
                }

                return RedirectToAction(nameof(Details), new { petId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "與寵物玩耍失敗，PetID: {PetId}", petId);
                TempData["ErrorMessage"] = "與寵物玩耍失敗";
                return RedirectToAction(nameof(Details), new { petId });
            }
        }

        /// <summary>
        /// 幫寵物洗澡
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Bathe(int petId)
        {
            try
            {
                var success = await _petService.BathePetAsync(petId);
                if (success)
                {
                    TempData["SuccessMessage"] = "成功幫寵物洗澡";
                }
                else
                {
                    TempData["ErrorMessage"] = "幫寵物洗澡失敗";
                }

                return RedirectToAction(nameof(Details), new { petId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "幫寵物洗澡失敗，PetID: {PetId}", petId);
                TempData["ErrorMessage"] = "幫寵物洗澡失敗";
                return RedirectToAction(nameof(Details), new { petId });
            }
        }

        /// <summary>
        /// 讓寵物睡覺
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sleep(int petId)
        {
            try
            {
                var success = await _petService.LetPetSleepAsync(petId);
                if (success)
                {
                    TempData["SuccessMessage"] = "成功讓寵物睡覺";
                }
                else
                {
                    TempData["ErrorMessage"] = "讓寵物睡覺失敗";
                }

                return RedirectToAction(nameof(Details), new { petId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "讓寵物睡覺失敗，PetID: {PetId}", petId);
                TempData["ErrorMessage"] = "讓寵物睡覺失敗";
                return RedirectToAction(nameof(Details), new { petId });
            }
        }

        /// <summary>
        /// 寵物規則設定
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Rules()
        {
            try
            {
                var rules = await _petService.GetPetRulesAsync();
                return View(rules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得寵物規則設定失敗");
                TempData["ErrorMessage"] = "取得寵物規則設定失敗";
                return View(new PetRulesViewModel());
            }
        }

        /// <summary>
        /// 更新寵物規則設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRules(PetRulesViewModel rules)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Rules", rules);
                }

                var success = await _petService.UpdatePetRulesAsync(rules);
                if (success)
                {
                    TempData["SuccessMessage"] = "成功更新寵物規則設定";
                }
                else
                {
                    TempData["ErrorMessage"] = "更新寵物規則設定失敗";
                }

                return RedirectToAction(nameof(Rules));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物規則設定失敗");
                TempData["ErrorMessage"] = "更新寵物規則設定失敗";
                return View("Rules", rules);
            }
        }
    }
}