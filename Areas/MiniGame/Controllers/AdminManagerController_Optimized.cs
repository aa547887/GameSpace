using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminManagerController_Optimized : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminManagerController_Optimized(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MiniGame/AdminManager
        public async Task<IActionResult> Index()
        {
            var games = await _context.MiniGames
                .Include(g => g.Category)
                .Include(g => g.User)
                .ToListAsync();
            
            return View(games);
        }

        // GET: MiniGame/AdminManager/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var miniGame = await _context.MiniGames
                .Include(g => g.Category)
                .Include(g => g.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (miniGame == null)
            {
                return NotFound();
            }

            return View(miniGame);
        }

        // GET: MiniGame/AdminManager/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Categories, "Id", "Name");
            ViewData["UserId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        // POST: MiniGame/AdminManager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,CategoryId,UserId,IsActive,CreatedAt,UpdatedAt")] MiniGame miniGame)
        {
            if (ModelState.IsValid)
            {
                _context.Add(miniGame);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Categories, "Id", "Name", miniGame.CategoryId);
            ViewData["UserId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Users, "Id", "UserName", miniGame.UserId);
            return View(miniGame);
        }

        // GET: MiniGame/AdminManager/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var miniGame = await _context.MiniGames.FindAsync(id);
            if (miniGame == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Categories, "Id", "Name", miniGame.CategoryId);
            ViewData["UserId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Users, "Id", "UserName", miniGame.UserId);
            return View(miniGame);
        }

        // POST: MiniGame/AdminManager/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,CategoryId,UserId,IsActive,CreatedAt,UpdatedAt")] MiniGame miniGame)
        {
            if (id != miniGame.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(miniGame);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MiniGameExists(miniGame.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Categories, "Id", "Name", miniGame.CategoryId);
            ViewData["UserId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Users, "Id", "UserName", miniGame.UserId);
            return View(miniGame);
        }

        // GET: MiniGame/AdminManager/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var miniGame = await _context.MiniGames
                .Include(g => g.Category)
                .Include(g => g.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (miniGame == null)
            {
                return NotFound();
            }

            return View(miniGame);
        }

        // POST: MiniGame/AdminManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var miniGame = await _context.MiniGames.FindAsync(id);
            _context.MiniGames.Remove(miniGame);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MiniGameExists(int id)
        {
            return _context.MiniGames.Any(e => e.Id == id);
        }
    }
}
