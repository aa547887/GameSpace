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
    public class PetController : Controller
    {
        private readonly IMiniGameService _miniGameService;
        private readonly GameSpacedatabaseContext _context;

        public PetController(IMiniGameService miniGameService, GameSpacedatabaseContext context)
        {
            _miniGameService = miniGameService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var pet = await _context.Pets
                .FirstOrDefaultAsync(p => p.UserId == userId);

            return View(pet);
        }

        [HttpPost]
        public async Task<IActionResult> Feed(int petId)
        {
            var pet = await _context.Pets.FindAsync(petId);
            if (pet != null)
            {
                pet.Hunger = Math.Min(100, pet.Hunger + 20);
                pet.Mood = Math.Min(100, pet.Mood + 10);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Play(int petId)
        {
            var pet = await _context.Pets.FindAsync(petId);
            if (pet != null)
            {
                pet.Mood = Math.Min(100, pet.Mood + 30);
                pet.Stamina = Math.Max(0, pet.Stamina - 20);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Sleep(int petId)
        {
            var pet = await _context.Pets.FindAsync(petId);
            if (pet != null)
            {
                pet.Stamina = Math.Min(100, pet.Stamina + 50);
                pet.Health = Math.Min(100, pet.Health + 10);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeSkinColor(int petId, string skinColor)
        {
            var pet = await _context.Pets.FindAsync(petId);
            if (pet != null)
            {
                pet.SkinColor = skinColor;
                pet.SkinColorChangedTime = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeBackgroundColor(int petId, string backgroundColor)
        {
            var pet = await _context.Pets.FindAsync(petId);
            if (pet != null)
            {
                pet.BackgroundColor = backgroundColor;
                pet.BackgroundColorChangedTime = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
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
