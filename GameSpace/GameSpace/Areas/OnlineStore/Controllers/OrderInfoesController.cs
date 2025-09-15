using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;

namespace GameSpace.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    public class OrderInfoesController : Controller
    {
        private readonly GameSpacedatabaseContext _dbContext;

        public OrderInfoesController(GameSpacedatabaseContext dbContext)
        {
			_dbContext = dbContext;
		}

        // GET: OnlineStore/OrderInfoes
        public async Task<IActionResult> Index()
        {
            return View(await _dbContext.OrderInfos.ToListAsync());
        }

        // GET: OnlineStore/OrderInfoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderInfo = await _dbContext.OrderInfos
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (orderInfo == null)
            {
                return NotFound();
            }

            return View(orderInfo);
        }

        // GET: OnlineStore/OrderInfoes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: OnlineStore/OrderInfoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,OrderCode,UserId,OrderDate,OrderStatus,PaymentStatus,OrderTotal,PaymentAt,ShippedAt,CompletedAt")] OrderInfo orderInfo)
        {
            if (ModelState.IsValid)
            {
				_dbContext.Add(orderInfo);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(orderInfo);
        }

        // GET: OnlineStore/OrderInfoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderInfo = await _dbContext.OrderInfos.FindAsync(id);
            if (orderInfo == null)
            {
                return NotFound();
            }
            return View(orderInfo);
        }

        // POST: OnlineStore/OrderInfoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,OrderCode,UserId,OrderDate,OrderStatus,PaymentStatus,OrderTotal,PaymentAt,ShippedAt,CompletedAt")] OrderInfo orderInfo)
        {
            if (id != orderInfo.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
					_dbContext.Update(orderInfo);
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderInfoExists(orderInfo.OrderId))
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
            return View(orderInfo);
        }

        // GET: OnlineStore/OrderInfoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderInfo = await _dbContext.OrderInfos
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (orderInfo == null)
            {
                return NotFound();
            }

            return View(orderInfo);
        }

        // POST: OnlineStore/OrderInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderInfo = await _dbContext.OrderInfos.FindAsync(id);
            if (orderInfo != null)
            {
				_dbContext.OrderInfos.Remove(orderInfo);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderInfoExists(int id)
        {
            return _dbContext.OrderInfos.Any(e => e.OrderId == id);
        }
    }
}
