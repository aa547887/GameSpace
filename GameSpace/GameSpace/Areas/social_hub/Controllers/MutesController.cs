using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;

namespace GameSpace.Areas.social_hub.Controllers
{
    [Area("social_hub")]
    public class MutesController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public MutesController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // GET: social_hub/Mutes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Mutes.ToListAsync());
        }

        // GET: social_hub/Mutes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mute = await _context.Mutes
                .FirstOrDefaultAsync(m => m.MuteId == id);
            if (mute == null)
            {
                return NotFound();
            }

            return View(mute);
        }

        // GET: social_hub/Mutes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: social_hub/Mutes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MuteId,MuteName,CreatedAt,IsActive,ManagerId")] Mute mute)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mute);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mute);
        }

        // GET: social_hub/Mutes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mute = await _context.Mutes.FindAsync(id);
            if (mute == null)
            {
                return NotFound();
            }
            return View(mute);
        }

        // POST: social_hub/Mutes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MuteId,MuteName,CreatedAt,IsActive,ManagerId")] Mute mute)
        {
            if (id != mute.MuteId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mute);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MuteExists(mute.MuteId))
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
            return View(mute);
        }

        // GET: social_hub/Mutes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mute = await _context.Mutes
                .FirstOrDefaultAsync(m => m.MuteId == id);
            if (mute == null)
            {
                return NotFound();
            }

            return View(mute);
        }

        // POST: social_hub/Mutes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mute = await _context.Mutes.FindAsync(id);
            if (mute != null)
            {
                _context.Mutes.Remove(mute);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MuteExists(int id)
        {
            return _context.Mutes.Any(e => e.MuteId == id);
        }
    }
}
