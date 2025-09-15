using GameSpace.Areas.MemberManagement.Models;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq; 
using System.Threading.Tasks;




namespace GameSpace.Areas.MemberManagement.Controllers
{
    [Area("MemberManagement")]
    public class UserIntroducesController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public UserIntroducesController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // GET: MemberManagement/UserIntroduces
        public async Task<IActionResult> Index()
        {
            var gameSpacedatabaseContext = _context.UserIntroduces.Select(u => new UserIntroduceVM
            {
                UserId = u.UserId,
                UserNickName = u.UserNickName,
                Gender = u.Gender,
                IdNumber = u.IdNumber,
                Cellphone = u.Cellphone,
                Email = u.Email,
                Address = u.Address,
                DateOfBirth = u.DateOfBirth,
                CreateAccount = u.CreateAccount,
                UserPicture = u.UserPicture,
                UserIntroduce1 = u.UserIntroduce1,
                User = u.User
            }).ToListAsync();
            return View(await gameSpacedatabaseContext);
        }

        // GET: MemberManagement/UserIntroduces/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userIntroduce = await _context.UserIntroduces
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (userIntroduce == null)
            {
                return NotFound();
            }

            return View(userIntroduce);
        }

        // GET: MemberManagement/UserIntroduces/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: MemberManagement/UserIntroduces/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,UserNickName,Gender,IdNumber,Cellphone,Email,Address,DateOfBirth,CreateAccount,UserPicture,UserIntroduce1")] UserIntroduce userIntroduce)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userIntroduce);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", userIntroduce.UserId);
            return View(userIntroduce);
        }

        // GET: MemberManagement/UserIntroduces/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userIntroduce = await _context.UserIntroduces.FindAsync(id);
            if (userIntroduce == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", userIntroduce.UserId);
            return View(userIntroduce);
        }

        // POST: MemberManagement/UserIntroduces/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,UserNickName,Gender,IdNumber,Cellphone,Email,Address,DateOfBirth,CreateAccount,UserPicture,UserIntroduce1")] UserIntroduce userIntroduce)
        {
            if (id != userIntroduce.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userIntroduce);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserIntroduceExists(userIntroduce.UserId))
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
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", userIntroduce.UserId);
            return View(userIntroduce);
        }

        // GET: MemberManagement/UserIntroduces/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userIntroduce = await _context.UserIntroduces
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (userIntroduce == null)
            {
                return NotFound();
            }

            return View(userIntroduce);
        }

        // POST: MemberManagement/UserIntroduces/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userIntroduce = await _context.UserIntroduces.FindAsync(id);
            if (userIntroduce != null)
            {
                _context.UserIntroduces.Remove(userIntroduce);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserIntroduceExists(int id)
        {
            return _context.UserIntroduces.Any(e => e.UserId == id);
        }
    }
}
