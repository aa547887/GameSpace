using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;
using GameSpace.Areas.social_hub.Services;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	public class MutesController : Controller
	{
		// 欄位
		private readonly GameSpace.Models.GameSpacedatabaseContext _context;
		private readonly GameSpace.Areas.social_hub.Services.IMuteFilter _muteFilter;

		// 建構子
		public MutesController(
			GameSpace.Models.GameSpacedatabaseContext context,
			GameSpace.Areas.social_hub.Services.IMuteFilter muteFilter)
		{
			_context = context;
			_muteFilter = muteFilter;
		}


		// GET: social_hub/Mutes
		public async Task<IActionResult> Index()
		{
			var list = await _context.Mutes
				.AsNoTracking()
				.OrderByDescending(m => m.MuteId)
				.ToListAsync();
			return View(list);
		}

		// GET: social_hub/Mutes/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();

			var mute = await _context.Mutes
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.MuteId == id.Value);

			if (mute == null) return NotFound();

			return View(mute);
		}

		// GET: social_hub/Mutes/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: social_hub/Mutes/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("MuteName,IsActive")] Mute mute)
		{
			if (!ModelState.IsValid) return View(mute);

			// 若你的 Mute 模型有 CreatedAt 或 ManagerId，可在這裡補上：
			// mute.CreatedAt = DateTime.UtcNow;
			// mute.ManagerId = 當前管理員ID;

			_context.Add(mute);
			await _context.SaveChangesAsync();

			// 詞庫更新後，立即刷新快取中的過濾規則
			await _muteFilter.RefreshAsync();

			TempData["Msg"] = "已新增詞彙並刷新過濾規則。";
			return RedirectToAction(nameof(Index));
		}

		// GET: social_hub/Mutes/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			var mute = await _context.Mutes.FindAsync(id.Value);
			if (mute == null) return NotFound();

			return View(mute);
		}

		// POST: social_hub/Mutes/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("MuteId,MuteName,IsActive")] Mute mute)
		{
			if (id != mute.MuteId) return NotFound();
			if (!ModelState.IsValid) return View(mute);

			try
			{
				_context.Update(mute);
				await _context.SaveChangesAsync();

				await _muteFilter.RefreshAsync();

				TempData["Msg"] = "已更新詞彙並刷新過濾規則。";
				return RedirectToAction(nameof(Index));
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!await _context.Mutes.AnyAsync(e => e.MuteId == id))
					return NotFound();
				throw;
			}
		}

		// GET: social_hub/Mutes/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return NotFound();

			var mute = await _context.Mutes
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.MuteId == id.Value);

			if (mute == null) return NotFound();

			return View(mute);
		}

		// POST: social_hub/Mutes/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var mute = await _context.Mutes.FirstOrDefaultAsync(m => m.MuteId == id);
			if (mute != null)
			{
				_context.Mutes.Remove(mute);
				await _context.SaveChangesAsync();

				await _muteFilter.RefreshAsync();

				TempData["Msg"] = "已刪除詞彙並刷新過濾規則。";
			}
			return RedirectToAction(nameof(Index));
		}
	}
}
