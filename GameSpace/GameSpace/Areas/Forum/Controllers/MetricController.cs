// MetricsController.cs
//using GameSpace.Areas.Admin.Models.ViewModels;
using GameSpace.Areas.Forum.Models.Metric;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.Admin.Controllers
{
    [Area("Forum")]
    [Route("Forum/[controller]/[action]")]
    public class MetricsController : Controller
    {
        private readonly GameSpacedatabaseContext _db;
        public MetricsController(GameSpacedatabaseContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Index(int? sourceId, string? q)
        {
            var baseQuery =
                from m in _db.Metrics.AsNoTracking()
                join s in _db.MetricSources.AsNoTracking() on m.SourceId equals s.SourceId
                select new { m, s };

            if (sourceId.HasValue) baseQuery = baseQuery.Where(x => x.s.SourceId == sourceId.Value);
            if (!string.IsNullOrWhiteSpace(q))
                baseQuery = baseQuery.Where(x => x.m.Code.Contains(q) || (x.m.Description ?? "").Contains(q));

            var list = await baseQuery
                .OrderBy(x => x.s.Name).ThenBy(x => x.m.Code)
                .Select(x => new MetricListItemVm
                {
                    MetricId = x.m.MetricId,
                    SourceId = x.s.SourceId,
                    SourceName = x.s.Name,
                    Code = x.m.Code,
                    Unit = x.m.Unit,
                    Description = x.m.Description,
                    IsActive = x.m.IsActive ?? true,
                    CreatedAt = x.m.CreatedAt
                })
                .ToListAsync();

            var sources = await _db.MetricSources.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
            ViewBag.Sources = new SelectList(sources, "SourceId", "Name", sourceId);
            ViewBag.SourceId = sourceId;
            ViewBag.Q = q;
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await FillSources(null);
            return View(new MetricEditVm());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MetricEditVm vm)
        {
            await ValidateMetric(vm);
            if (!ModelState.IsValid)
            {
                await FillSources(vm.SourceId);
                return View(vm);
            }

            var e = new Metric
            {
                SourceId = vm.SourceId,
                Code = vm.Code,
                Unit = vm.Unit,
                Description = vm.Description,
                IsActive = vm.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            _db.Metrics.Add(e);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var m = await _db.Metrics.FindAsync(id);
            if (m == null) return NotFound();

            var vm = new MetricEditVm
            {
                MetricId = m.MetricId,
                SourceId = m.SourceId ?? 0,
                Code = m.Code,
                Unit = m.Unit,
                Description = m.Description,
                IsActive = m.IsActive ?? true
            };
            await FillSources(m.SourceId);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MetricEditVm vm)
        {
            if (!vm.MetricId.HasValue) return BadRequest();
            await ValidateMetric(vm);
            if (!ModelState.IsValid)
            {
                await FillSources(vm.SourceId);
                return View(vm);
            }

            var m = await _db.Metrics.FindAsync(vm.MetricId.Value);
            if (m == null) return NotFound();

            m.SourceId = vm.SourceId;
            m.Code = vm.Code;
            m.Unit = vm.Unit;
            m.Description = vm.Description;
            m.IsActive = vm.IsActive;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var m = await _db.Metrics.FindAsync(id);
            if (m == null) return NotFound();

            m.IsActive = !(m.IsActive ?? true);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task FillSources(int? selected)
        {
            var sources = await _db.MetricSources.AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new { s.SourceId, s.Name })
                .ToListAsync();
            ViewBag.Sources = new SelectList(sources, "SourceId", "Name", selected);
        }

        private async Task ValidateMetric(MetricEditVm vm)
        {
            // (source_id, code) 唯一
            var dup = await _db.Metrics
                .AnyAsync(x => x.SourceId == vm.SourceId && x.Code == vm.Code && x.MetricId != vm.MetricId);
            if (dup) ModelState.AddModelError("", "同一來源下的代碼必須唯一。");
        }
    }
}
