using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ClosedXML.Excel;

namespace GameSpace.Areas.Forum.Controllers
{
    [Area("Forum")]
    [Route("Forum/[controller]/[action]")]
    public class ReportsController : Controller
    {
        private readonly GameSpacedatabaseContext _db;
        public ReportsController(GameSpacedatabaseContext db) => _db = db;

        // 查詢頁（表單）
        [HttpGet]
        public async Task<IActionResult> History(int? gameId, DateOnly? from, DateOnly? to)
        {
            // 遊戲清單 for 下拉
            ViewBag.Games = await _db.Games.AsNoTracking()
                .OrderBy(g => g.Name)
                .Select(g => new { g.GameId, g.Name })
                .ToListAsync();

            // 有參數就查，沒參數就先給空
            if (gameId.HasValue && from.HasValue && to.HasValue)
            {
                var rows = await QueryDailyIndexAsync(gameId.Value, from.Value, to.Value);
                

                ViewBag.ExportRoute = new Dictionary<string, string>
                {
                    ["gameId"] = gameId.Value.ToString(),
                    ["from"] = from.Value.ToString("yyyy-MM-dd"),
                    ["to"] = to.Value.ToString("yyyy-MM-dd")
                };
                return View(rows);
            }
            // 沒查詢就給空 Route，View 會判斷不顯示匯出鈕
            ViewBag.ExportRoute = null;
            return View(new List<DailyIndexVm>());
        }

        // 匯出 CSV
        [HttpGet]
        public async Task<IActionResult> ExportCsv(int gameId, DateOnly from, DateOnly to)
        {
            var rows = await QueryDailyIndexAsync(gameId, from, to);

            var sb = new StringBuilder();
            sb.AppendLine("date,game,Index");
            foreach (var r in rows)
                sb.AppendLine($"{r.Date:yyyy-MM-dd},{Quote(r.GameName)},{r.IndexValue:0.####}");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"index_{gameId}_{from:yyyyMMdd}_{to:yyyyMMdd}.csv");

            static string Quote(string s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";
        }

        // 匯出 Excel
        [HttpGet]
        public async Task<IActionResult> ExportExcel(int gameId, DateOnly from, DateOnly to)
        {
            var rows = await QueryDailyIndexAsync(gameId, from, to);

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Index");
            ws.Cell(1, 1).Value = "Date";
            ws.Cell(1, 2).Value = "Game";
            ws.Cell(1, 3).Value = "Index";

            for (int i = 0; i < rows.Count; i++)
            {
                ws.Cell(i + 2, 1).Value = rows[i].Date.ToDateTime(TimeOnly.MinValue);
                ws.Cell(i + 2, 1).Style.DateFormat.Format = "yyyy-mm-dd";
                ws.Cell(i + 2, 2).Value = rows[i].GameName;
                ws.Cell(i + 2, 3).Value = rows[i].IndexValue;
            }
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var bytes = ms.ToArray();
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"index_{gameId}_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx");
        }

        // 取得某遊戲在日期區間內的每日 Index（等權重）
        private async Task<List<DailyIndexVm>> QueryDailyIndexAsync(int gameId, DateOnly from, DateOnly to)
        {
            var dates = EachDate(from, to).ToList();
            var game = await _db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.GameId == gameId);
            var gameName = game?.Name ?? $"#{gameId}";

            // 啟用中的 metrics（等權重）
            var activeMetricIds = await _db.Metrics
                .Where(m => (m.IsActive ?? true))
                .Select(m => m.MetricId)
                .ToListAsync();

            var result = new List<DailyIndexVm>();
            if (activeMetricIds.Count == 0) return result;

            foreach (var d in dates)
            {
                // 取當日數據（只取啟用指標）
                var rows = await _db.GameMetricDailies
                    .Where(x => x.Date == d
                                && x.GameId.HasValue && x.MetricId.HasValue
                                && activeMetricIds.Contains(x.MetricId.Value))
                    .Select(x => new { GameId = x.GameId.Value, MetricId = x.MetricId.Value, x.Value })
                    .ToListAsync();

                if (rows.Count == 0) { result.Add(new DailyIndexVm { Date = d, GameName = gameName, IndexValue = 0m }); continue; }

                // 各指標最大值 → 標準化
                var maxByMetric = rows.GroupBy(r => r.MetricId)
                                      .ToDictionary(g => g.Key, g => g.Max(x => x.Value == 0 ? 1 : x.Value));

                // 取該遊戲的值
                var mine = rows.Where(r => r.GameId == gameId).ToList();
                if (mine.Count == 0) { result.Add(new DailyIndexVm { Date = d, GameName = gameName, IndexValue = 0m }); continue; }

                double sum = 0d; int used = 0;
                foreach (var r in mine)
                {
                    if (!maxByMetric.TryGetValue(r.MetricId, out var max)) continue;
                    var norm = max == 0 ? 0 : (double)(r.Value / max);
                    sum += norm; used++;
                }
                var index = (used == 0) ? 0d : sum / activeMetricIds.Count; // 等權重平均（分母用啟用指標數）
                result.Add(new DailyIndexVm { Date = d, GameName = gameName, IndexValue = (decimal)Math.Round(index, 4) });
            }

            return result;
        }

        private static IEnumerable<DateOnly> EachDate(DateOnly from, DateOnly to)
        {
            for (var d = from; d <= to; d = d.AddDays(1)) yield return d;
        }

        public class DailyIndexVm
        {
            public DateOnly Date { get; set; }
            public string GameName { get; set; } = "";
            public decimal IndexValue { get; set; }
        }
      
    }
}
