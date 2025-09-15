using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using GameSpace.Areas.MemberManagement.Models;

namespace GameSpace.Areas.MemberManagement.Controllers
{
    [Area("MemberManagement")]
    public class ManagerDataController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public ManagerDataController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // GET: MemberManagement/ManagerData
        public async Task<IActionResult> Index()
        {
            var gameSpacedatabaseContext = _context.ManagerData.Select(m => new ManagerDatumVM
			{ ManagerAccessFailedCount = m.ManagerAccessFailedCount,
              ManagerAccount = m.ManagerAccount,
              ManagerEmail = m.ManagerEmail,
              ManagerEmailConfirmed = m.ManagerEmailConfirmed,
              ManagerId = m.ManagerId,
              ManagerLockoutEnabled = m.ManagerLockoutEnabled,
              ManagerLockoutEnd = m.ManagerLockoutEnd,
              ManagerName = m.ManagerName,
              ManagerPassword = m.ManagerPassword,
              AdministratorRegistrationDate = m.AdministratorRegistrationDate
			}
            ).ToListAsync();
			return View(await gameSpacedatabaseContext);
		}

        // GET: MemberManagement/ManagerData/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var managerDatum = await _context.ManagerData
                .FirstOrDefaultAsync(m => m.ManagerId == id);
            if (managerDatum == null)
            {
                return NotFound();
            }

            return View(managerDatum);
        }

        // GET: MemberManagement/ManagerData/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MemberManagement/ManagerData/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ManagerId,ManagerName,ManagerAccount,ManagerPassword,AdministratorRegistrationDate,ManagerEmail,ManagerEmailConfirmed,ManagerAccessFailedCount,ManagerLockoutEnabled,ManagerLockoutEnd")] ManagerDatum managerDatum)
        {
            if (ModelState.IsValid)
            {
                _context.Add(managerDatum);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(managerDatum);
        }

        // GET: MemberManagement/ManagerData/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var managerDatum = await _context.ManagerData.FindAsync(id);
            if (managerDatum == null)
            {
                return NotFound();
            }
            return View(managerDatum);
        }

        // POST: MemberManagement/ManagerData/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ManagerId,ManagerName,ManagerAccount,ManagerPassword,AdministratorRegistrationDate,ManagerEmail,ManagerEmailConfirmed,ManagerAccessFailedCount,ManagerLockoutEnabled,ManagerLockoutEnd")] ManagerDatum managerDatum)
        {
            if (id != managerDatum.ManagerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(managerDatum);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ManagerDatumExists(managerDatum.ManagerId))
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
            return View(managerDatum);
        }

        // GET: MemberManagement/ManagerData/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var managerDatum = await _context.ManagerData
                .FirstOrDefaultAsync(m => m.ManagerId == id);
            if (managerDatum == null)
            {
                return NotFound();
            }

            return View(managerDatum);
        }

        // POST: MemberManagement/ManagerData/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var managerDatum = await _context.ManagerData.FindAsync(id);
            if (managerDatum != null)
            {
                _context.ManagerData.Remove(managerDatum);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ManagerDatumExists(int id)
        {
            return _context.ManagerData.Any(e => e.ManagerId == id);
        }
    }
}
