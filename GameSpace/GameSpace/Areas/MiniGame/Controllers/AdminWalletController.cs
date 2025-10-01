using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Data;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminWalletController : Controller
    {
        private readonly GameSpaceContext _context;

        public AdminWalletController(GameSpaceContext context)
        {
            _context = context;
        }

        // GET: AdminWallet
        public async Task<IActionResult> Index(string searchTerm = "", string transactionType = "", string sortBy = "date", int page = 1, int pageSize = 10)
        {
            var query = _context.Wallet
                .Include(w => w.Users)
                .AsQueryable();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(w => w.Users.User_name.Contains(searchTerm) || 
                                       w.Users.User_account.Contains(searchTerm) || 
                                       w.Description.Contains(searchTerm));
            }

            // 交易類型篩選
            if (!string.IsNullOrEmpty(transactionType))
            {
                query = query.Where(w => w.TransactionType == transactionType);
            }

            // 排序
            query = sortBy switch
            {
                "amount" => query.OrderByDescending(w => w.Amount),
                "user" => query.OrderBy(w => w.Users.User_name),
                "type" => query.OrderBy(w => w.TransactionType),
                _ => query.OrderByDescending(w => w.TransactionDate)
            };

            // 分頁
            var totalCount = await query.CountAsync();
            var wallets = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new AdminWalletIndexViewModel
            {
                Wallets = new PagedResult<Wallet>
                {
                    Items = wallets,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                }
            };

            // 設定 ViewBag 用於搜尋和篩選
            ViewBag.SearchTerm = searchTerm;
            ViewBag.TransactionType = transactionType;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalTransactions = totalCount;
            ViewBag.TotalEarned = await _context.Wallet.Where(w => w.TransactionType == "earn").SumAsync(w => w.Amount);
            ViewBag.TotalSpent = await _context.Wallet.Where(w => w.TransactionType == "spend").SumAsync(w => w.Amount);
            ViewBag.TotalUsers = await _context.Users.CountAsync();

            return View(viewModel);
        }

        // GET: AdminWallet/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wallet = await _context.Wallet
                .Include(w => w.Users)
                .FirstOrDefaultAsync(m => m.WalletId == id);

            if (wallet == null)
            {
                return NotFound();
            }

            return View(wallet);
        }

        // GET: AdminWallet/Transaction
        public async Task<IActionResult> Transaction(int? userId)
        {
            if (userId == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var currentPoints = await _context.Wallet
                .Where(w => w.UserId == userId)
                .SumAsync(w => w.TransactionType == "earn" ? w.Amount : -w.Amount);

            var model = new AdminWalletTransactionViewModel
            {
                UserId = user.User_Id,
                UserName = user.User_name,
                CurrentPoints = currentPoints
            };

            return View(model);
        }

        // POST: AdminWallet/Transaction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transaction(AdminWalletTransactionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var wallet = new Wallet
                {
                    UserId = model.UserId,
                    Amount = model.TransactionAmount,
                    TransactionType = model.TransactionType,
                    TransactionDate = DateTime.Now,
                    Description = model.Description
                };

                _context.Add(wallet);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "點數交易成功";
                return RedirectToAction(nameof(Index));
            }

            // 重新載入用戶資訊
            var user = await _context.Users.FindAsync(model.UserId);
            if (user != null)
            {
                var currentPoints = await _context.Wallet
                    .Where(w => w.UserId == model.UserId)
                    .SumAsync(w => w.TransactionType == "earn" ? w.Amount : -w.Amount);

                model.UserName = user.User_name;
                model.CurrentPoints = currentPoints;
            }

            return View(model);
        }

        // GET: AdminWallet/UserWallet/5
        public async Task<IActionResult> UserWallet(int? userId)
        {
            if (userId == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var wallets = await _context.Wallet
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.TransactionDate)
                .ToListAsync();

            var currentPoints = wallets.Sum(w => w.TransactionType == "earn" ? w.Amount : -w.Amount);

            ViewBag.User = user;
            ViewBag.CurrentPoints = currentPoints;

            return View(wallets);
        }

        // 獲取用戶點數統計
        [HttpGet]
        public async Task<IActionResult> GetUserPoints(int userId)
        {
            var currentPoints = await _context.Wallet
                .Where(w => w.UserId == userId)
                .SumAsync(w => w.TransactionType == "earn" ? w.Amount : -w.Amount);

            var earnedPoints = await _context.Wallet
                .Where(w => w.UserId == userId && w.TransactionType == "earn")
                .SumAsync(w => w.Amount);

            var spentPoints = await _context.Wallet
                .Where(w => w.UserId == userId && w.TransactionType == "spend")
                .SumAsync(w => w.Amount);

            return Json(new
            {
                currentPoints,
                earnedPoints,
                spentPoints
            });
        }

        // 獲取點數統計圖表數據
        [HttpGet]
        public async Task<IActionResult> GetPointsChartData(int days = 30)
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-days);

            var data = new List<object>();

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var earned = await _context.Wallet
                    .Where(w => w.TransactionDate.Date == date && w.TransactionType == "earn")
                    .SumAsync(w => w.Amount);
                var spent = await _context.Wallet
                    .Where(w => w.TransactionDate.Date == date && w.TransactionType == "spend")
                    .SumAsync(w => w.Amount);

                data.Add(new
                {
                    date = date.ToString("yyyy-MM-dd"),
                    earned,
                    spent,
                    net = earned - spent
                });
            }

            return Json(data);
        }

        // 獲取交易類型統計
        [HttpGet]
        public async Task<IActionResult> GetTransactionTypeStats()
        {
            var stats = await _context.Wallet
                .GroupBy(w => w.TransactionType)
                .Select(g => new
                {
                    type = g.Key,
                    count = g.Count(),
                    totalAmount = g.Sum(w => w.Amount)
                })
                .ToListAsync();

            return Json(stats);
        }

        // 獲取用戶點數排行榜
        [HttpGet]
        public async Task<IActionResult> GetPointsLeaderboard(int top = 10)
        {
            var leaderboard = await _context.Users
                .Select(u => new
                {
                    userId = u.User_Id,
                    userName = u.User_name,
                    userAccount = u.User_account,
                    currentPoints = _context.Wallet
                        .Where(w => w.UserId == u.User_Id)
                        .Sum(w => w.TransactionType == "earn" ? w.Amount : -w.Amount)
                })
                .OrderByDescending(u => u.currentPoints)
                .Take(top)
                .ToListAsync();

            return Json(leaderboard);
        }
    }
}
