// MetricsController.cs
//using GameSpace.Areas.Admin.Models.ViewModels;
using GameSpace.Areas.Forum.Models.Metric;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GameSpace.Areas.Admin.Controllers
{
    [Area("Forum")]
    [Route("Forum/[controller]/[action]")]
    public class MetricsController : Controller
    {
        private readonly GameSpacedatabaseContext _db;
        public MetricsController(GameSpacedatabaseContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Index(int? sourceId, string? q, DateOnly? date)
        {
            // 目標日期（預設今天 UTC 的日）
            var target = date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

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
                    IsActive = x.m.IsActive ?? true,   // 你原本就是 nullable
                    CreatedAt = x.m.CreatedAt
                })
                .ToListAsync();

            var sources = await _db.MetricSources.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
            ViewBag.Sources = new SelectList(sources, "SourceId", "Name", sourceId);
            ViewBag.SourceId = sourceId;
            ViewBag.Q = q;

            // ➌ 下方排行榜（等權重、僅啟用指標）
            var top10 = await BuildLeaderboardAsync(target);

            var vm = new MetricsIndexVm
            {
                TargetDate = target,
                List = list,
                Top10 = top10
            };
            return View(vm);
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

        // ➊ 新增 VM：頁面總模型（上：清單｜下：Top10）
        public class MetricsIndexVm
        {
            public DateOnly TargetDate { get; set; }
            public List<MetricListItemVm> List { get; set; } = new();   // 你原本的 List VM
            public List<LeaderboardRowVm> Top10 { get; set; } = new();  // 下方排行榜
        }

        // ➋ 新增 VM：排行榜每列
        public class LeaderboardRowVm
        {
            public int Rank { get; set; }
            public int GameId { get; set; }
            public string GameName { get; set; } = "";
            public double Score { get; set; }           // 0~1 之間（等權重平均）
        }

        private async Task<List<LeaderboardRowVm>> BuildLeaderboardAsync(DateOnly date)
        {
            // 1) 取啟用中的指標 IDs（int）
            var activeMetricIds = await _db.Metrics
                .Where(m => (m.IsActive ?? true))
                .Select(m => m.MetricId)               // metrics.metric_id 是 int（PK）
                .ToListAsync();

            if (activeMetricIds.Count == 0) return new();

            // 2) 取當日數據：先把 nullable 過濾掉再投影成非 nullable，才不會跟 List<int> 打架
            var rows = await _db.GameMetricDailies
                .Where(d => d.Date == date
                            && d.GameId.HasValue
                            && d.MetricId.HasValue
                            && activeMetricIds.Contains(d.MetricId.Value))
                .Select(d => new
                {
                    GameId = d.GameId.Value,
                    MetricId = d.MetricId.Value,
                    d.Value
                })
                .ToListAsync();

            if (rows.Count == 0) return new();

            // 3) 每個指標的「當日最大值」→ 用來做 0~1 標準化
            var maxByMetric = rows
                .GroupBy(r => r.MetricId)
                .ToDictionary(g => g.Key, g => g.Max(x => x.Value == 0 ? 1 : x.Value));

            // 4) 等權重平均（每個啟用指標視為 weight=1）
            var scoreByGame = rows
                .GroupBy(r => r.GameId)
                .Select(g => new
                {
                    GameId = g.Key,
                    Score = g.Sum(x =>
                    {
                        var max = maxByMetric[x.MetricId];
                        return max == 0 ? 0 : (double)(x.Value / max);
                    }) / activeMetricIds.Count
                })
                .OrderByDescending(x => x.Score)
                .Take(10)
                .ToList();

            // 5) 補遊戲名稱（不用 GetValueOrDefault，多數框架都支援 TryGetValue）
            var gameIds = scoreByGame.Select(x => x.GameId).ToList();
            var names = await _db.Games
                .Where(g => gameIds.Contains(g.GameId))
                .ToDictionaryAsync(g => g.GameId, g => g.Name);

            return scoreByGame.Select((x, i) => new LeaderboardRowVm
            {
                Rank = i + 1,
                GameId = x.GameId,                                    // 這裡現在是 int → 不會再 CS0266
                GameName = names.TryGetValue(x.GameId, out var n) ? n : $"#{x.GameId}", // 取不到就顯示 #ID
                Score = Math.Round(x.Score, 4)
            }).ToList();
        }



    }
}
