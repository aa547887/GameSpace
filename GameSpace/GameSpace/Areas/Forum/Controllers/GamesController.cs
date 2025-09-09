// GamesController.cs
//using GameSpace.Areas.Admin.Models.ViewModels;
using GameSpace.Areas.Forum.Models.Game;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.Admin.Controllers
{
    
    [Area("Forum")]
    [Route("Forum/[controller]/[action]")]
    public class GamesController : Controller
    {
        private readonly GameSpacedatabaseContext _db;
        public GamesController(GameSpacedatabaseContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? genre)
        {
            var query = _db.Games.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(g => g.Name.Contains(q) || (g.NameZh ?? "").Contains(q));

            if (!string.IsNullOrWhiteSpace(genre))
                query = query.Where(g => g.Genre == genre);

            var list = await query
                .OrderBy(g => g.Name)
                .Select(g => new GameListItemVm
                {
                    GameId = g.GameId,
                    Name = g.Name,
                    NameZh = g.NameZh,
                    Genre = g.Genre,
                    CreatedAt = g.CreatedAt
                })
                .ToListAsync();

            ViewBag.Q = q;
            ViewBag.Genre = genre;
            return View(list);
        }

        [HttpGet]
        public IActionResult Create() => View(new GameEditVm());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GameEditVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var e = new Game
            {
                Name = vm.Name,
                NameZh = vm.NameZh,
                Genre = vm.Genre,
                CreatedAt = DateTime.UtcNow
            };
            _db.Games.Add(e);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var g = await _db.Games.FindAsync(id);
            if (g == null) return NotFound();

            var vm = new GameEditVm
            {
                GameId = g.GameId,
                Name = g.Name,
                NameZh = g.NameZh,
                Genre = g.Genre
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GameEditVm vm)
        {
            if (!vm.GameId.HasValue) return BadRequest();
            var g = await _db.Games.FindAsync(vm.GameId.Value);
            if (g == null) return NotFound();

            g.Name = vm.Name;
            g.NameZh = vm.NameZh;
            g.Genre = vm.Genre;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
