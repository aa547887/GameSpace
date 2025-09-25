using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class MiniGameController : Controller
    {
        private readonly IMiniGameService _miniGameService;
        private readonly GameSpacedatabaseContext _context;

        public MiniGameController(IMiniGameService miniGameService, GameSpacedatabaseContext context)
        {
            _miniGameService = miniGameService;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Pet()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var pet = await _context.Pets
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (pet == null)
            {
                // 創建新寵物
                pet = new GameSpace.Models.Pet
                {
                    UserId = userId,
                    PetName = "我的寵物",
                    Level = 1,
                    Experience = 0,
                    SkinColor = "Default",
                    BackgroundColor = "Default",
                    Hunger = 100,
                    Mood = 100,
                    Stamina = 100,
                    Cleanliness = 100,
                    Health = 100
                };
                _context.Pets.Add(pet);
                await _context.SaveChangesAsync();
            }

            return View(pet);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePetName(int petId, string petName)
        {
            var pet = await _context.Pets.FindAsync(petId);
            if (pet != null)
            {
                pet.PetName = petName;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Pet");
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }
    }
}
