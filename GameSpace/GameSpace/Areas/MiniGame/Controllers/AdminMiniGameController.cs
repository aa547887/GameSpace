using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Data;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminMiniGameController : Controller
    {
        private readonly GameSpaceContext _context;

        public AdminMiniGameController(GameSpaceContext context)
        {
            _context = context;
        }

        // GET: AdminMiniGame
        public async Task<IActionResult> Index(string searchTerm = "", string gameType = "", string sortBy = "name", int page = 1, int pageSize = 10)
        {
            var query = _context.MiniGame.AsQueryable();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(g => g.GameName.Contains(searchTerm) || 
                                       g.GameDescription.Contains(searchTerm) || 
                                       g.GameType.Contains(searchTerm));
            }

            // 遊戲類型篩選
            if (!string.IsNullOrEmpty(gameType))
            {
                query = query.Where(g => g.GameType == gameType);
            }

            // 排序
            query = sortBy switch
            {
                "type" => query.OrderBy(g => g.GameType),
                "cost" => query.OrderBy(g => g.CostPoints),
                "reward" => query.OrderByDescending(g => g.RewardPoints),
                "plays" => query.OrderByDescending(g => g.MaxPlayCount),
                "created" => query.OrderByDescending(g => g.CreatedAt),
                _ => query.OrderBy(g => g.GameName)
            };

            // 分頁
            var totalCount = await query.CountAsync();
            var miniGames = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new AdminMiniGameIndexViewModel
            {
                MiniGames = new PagedResult<MiniGame>
                {
                    Items = miniGames,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                }
            };

            // 設定 ViewBag 用於搜尋和篩選
            ViewBag.SearchTerm = searchTerm;
            ViewBag.GameType = gameType;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalGames = totalCount;
            ViewBag.ActiveGames = await _context.MiniGame.CountAsync(g => g.IsActive);
            ViewBag.PuzzleGames = await _context.MiniGame.CountAsync(g => g.GameType == "益智");
            ViewBag.ActionGames = await _context.MiniGame.CountAsync(g => g.GameType == "動作");
            ViewBag.StrategyGames = await _context.MiniGame.CountAsync(g => g.GameType == "策略");

            return View(viewModel);
        }

        // GET: AdminMiniGame/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var miniGame = await _context.MiniGame
                .Include(g => g.GamePlayRecords)
                .FirstOrDefaultAsync(m => m.GameId == id);

            if (miniGame == null)
            {
                return NotFound();
            }

            return View(miniGame);
        }

        // GET: AdminMiniGame/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminMiniGame/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminMiniGameCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var miniGame = new MiniGame
                {
                    GameName = model.GameName,
                    GameDescription = model.GameDescription,
                    GameType = model.GameType,
                    CostPoints = model.CostPoints,
                    RewardPoints = model.RewardPoints,
                    MaxPlayCount = model.MaxPlayCount,
                    GameImageUrl = model.GameImageUrl,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now
                };

                _context.Add(miniGame);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "遊戲建立成功";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: AdminMiniGame/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var miniGame = await _context.MiniGame.FindAsync(id);
            if (miniGame == null)
            {
                return NotFound();
            }

            var model = new AdminMiniGameCreateViewModel
            {
                GameName = miniGame.GameName,
                GameDescription = miniGame.GameDescription,
                GameType = miniGame.GameType,
                CostPoints = miniGame.CostPoints,
                RewardPoints = miniGame.RewardPoints,
                MaxPlayCount = miniGame.MaxPlayCount,
                GameImageUrl = miniGame.GameImageUrl,
                IsActive = miniGame.IsActive
            };

            return View(model);
        }

        // POST: AdminMiniGame/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminMiniGameCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var miniGame = await _context.MiniGame.FindAsync(id);
                    if (miniGame == null)
                    {
                        return NotFound();
                    }

                    miniGame.GameName = model.GameName;
                    miniGame.GameDescription = model.GameDescription;
                    miniGame.GameType = model.GameType;
                    miniGame.CostPoints = model.CostPoints;
                    miniGame.RewardPoints = model.RewardPoints;
                    miniGame.MaxPlayCount = model.MaxPlayCount;
                    miniGame.GameImageUrl = model.GameImageUrl;
                    miniGame.IsActive = model.IsActive;

                    _context.Update(miniGame);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "遊戲更新成功";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MiniGameExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(model);
        }

        // GET: AdminMiniGame/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var miniGame = await _context.MiniGame
                .Include(g => g.GamePlayRecords)
                .FirstOrDefaultAsync(m => m.GameId == id);

            if (miniGame == null)
            {
                return NotFound();
            }

            return View(miniGame);
        }

        // POST: AdminMiniGame/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var miniGame = await _context.MiniGame.FindAsync(id);
            if (miniGame != null)
            {
                _context.MiniGame.Remove(miniGame);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "遊戲刪除成功";
            }

            return RedirectToAction(nameof(Index));
        }

        // 切換遊戲狀態
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var miniGame = await _context.MiniGame.FindAsync(id);
            if (miniGame != null)
            {
                miniGame.IsActive = !miniGame.IsActive;
                _context.Update(miniGame);
                await _context.SaveChangesAsync();

                return Json(new { success = true, isActive = miniGame.IsActive });
            }

            return Json(new { success = false });
        }

        // 獲取遊戲統計數據
        [HttpGet]
        public async Task<IActionResult> GetGameStats()
        {
            var stats = new
            {
                total = await _context.MiniGame.CountAsync(),
                active = await _context.MiniGame.CountAsync(g => g.IsActive),
                puzzle = await _context.MiniGame.CountAsync(g => g.GameType == "益智"),
                action = await _context.MiniGame.CountAsync(g => g.GameType == "動作"),
                strategy = await _context.MiniGame.CountAsync(g => g.GameType == "策略"),
                totalPlays = await _context.GamePlayRecord.CountAsync()
            };

            return Json(stats);
        }

        // 獲取遊戲類型分佈
        [HttpGet]
        public async Task<IActionResult> GetGameTypeDistribution()
        {
            var distribution = await _context.MiniGame
                .GroupBy(g => g.GameType)
                .Select(g => new
                {
                    type = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(g => g.count)
                .ToListAsync();

            return Json(distribution);
        }

        // 獲取遊戲遊玩次數統計
        [HttpGet]
        public async Task<IActionResult> GetGamePlayStats()
        {
            var stats = await _context.GamePlayRecord
                .GroupBy(g => g.GameId)
                .Select(g => new
                {
                    gameId = g.Key,
                    gameName = _context.MiniGame.Where(m => m.GameId == g.Key).Select(m => m.GameName).FirstOrDefault(),
                    playCount = g.Count(),
                    totalScore = g.Sum(x => x.Score),
                    avgScore = g.Average(x => x.Score)
                })
                .OrderByDescending(g => g.playCount)
                .Take(10)
                .ToListAsync();

            return Json(stats);
        }

        // 獲取遊戲收益統計
        [HttpGet]
        public async Task<IActionResult> GetGameRevenueStats()
        {
            var stats = await _context.MiniGame
                .Select(g => new
                {
                    gameId = g.GameId,
                    gameName = g.GameName,
                    costPoints = g.CostPoints,
                    rewardPoints = g.RewardPoints,
                    playCount = _context.GamePlayRecord.Count(r => r.GameId == g.GameId),
                    totalCost = g.CostPoints * _context.GamePlayRecord.Count(r => r.GameId == g.GameId),
                    totalReward = g.RewardPoints * _context.GamePlayRecord.Count(r => r.GameId == g.GameId)
                })
                .OrderByDescending(g => g.totalCost)
                .ToListAsync();

            return Json(stats);
        }

        private bool MiniGameExists(int id)
        {
            return _context.MiniGame.Any(e => e.GameId == id);
        }
    }
}
