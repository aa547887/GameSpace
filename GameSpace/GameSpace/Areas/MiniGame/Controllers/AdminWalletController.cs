using Microsoft.AspNetCore.Mvc;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Data;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminWalletController : MiniGameBaseController
    {
        private readonly IWalletService _walletService;
        private readonly IUserService _userService;

        public AdminWalletController(
            GameSpacedatabaseContext context,
            IWalletService walletService,
            IUserService userService) : base(context)
        {
            _walletService = walletService;
            _userService = userService;
        }

        // GET: AdminWallet
        public async Task<IActionResult> Index(string searchTerm = "", string changeType = "", string sortBy = "date", int page = 1, int pageSize = 10)
        {
            // 獲取所有歷史記錄
            var allHistory = await _walletService.GetAllHistoryAsync(1, 10000); // 簡化：取大量數據

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                allHistory = allHistory.Where(w => w.Description.Contains(searchTerm));
            }

            // 交易類型篩選
            if (!string.IsNullOrEmpty(changeType))
            {
                allHistory = allHistory.Where(w => w.ChangeType == changeType);
            }

            // 排序
            allHistory = sortBy switch
            {
                "amount" => allHistory.OrderByDescending(w => Math.Abs(w.PointsChanged)),
                "type" => allHistory.OrderBy(w => w.ChangeType),
                _ => allHistory.OrderByDescending(w => w.ChangeTime)
            };

            // 分頁
            var totalCount = allHistory.Count();
            var pagedHistory = allHistory
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new AdminWalletIndexViewModel
            {
                Wallets = new PagedResult<Wallet>
                {
                    Items = pagedHistory.Select(h => new Wallet
                    {
                        WalletId = h.HistoryID,
                        UserId = h.UserID,
                        Amount = Math.Abs(h.PointsChanged),
                        TransactionType = h.PointsChanged > 0 ? "earn" : "spend",
                        TransactionDate = h.ChangeTime,
                        Description = h.Description
                    }).ToList(),
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                }
            };

            // 統計數據
            ViewBag.SearchTerm = searchTerm;
            ViewBag.TransactionType = changeType;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalTransactions = totalCount;
            ViewBag.TotalEarned = allHistory.Where(h => h.PointsChanged > 0).Sum(h => h.PointsChanged);
            ViewBag.TotalSpent = Math.Abs(allHistory.Where(h => h.PointsChanged < 0).Sum(h => h.PointsChanged));
            ViewBag.TotalUsers = await _userService.GetTotalUsersCountAsync();

            return View(viewModel);
        }

        // GET: AdminWallet/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var history = await _walletService.GetHistoryDetailAsync(id.Value);

            if (history == null)
            {
                return NotFound();
            }

            // 轉換為 Wallet 格式顯示
            var wallet = new Wallet
            {
                WalletId = history.HistoryID,
                UserId = history.UserID,
                Amount = Math.Abs(history.PointsChanged),
                TransactionType = history.PointsChanged > 0 ? "earn" : "spend",
                TransactionDate = history.ChangeTime,
                Description = history.Description
            };

            return View(wallet);
        }

        // GET: AdminWallet/Transaction
        public async Task<IActionResult> Transaction(int? userId)
        {
            if (userId == null)
            {
                return NotFound();
            }

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            var summary = await _walletService.GetPointsSummaryAsync(userId.Value);

            var model = new AdminWalletTransactionViewModel
            {
                UserId = user.User_Id,
                UserName = user.User_Name,
                CurrentPoints = summary["CurrentPoints"]
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
                bool result;
                if (model.TransactionType == "earn")
                {
                    result = await _walletService.AddPointsAsync(
                        model.UserId,
                        model.TransactionAmount,
                        model.Description
                    );
                }
                else
                {
                    result = await _walletService.DeductPointsAsync(
                        model.UserId,
                        model.TransactionAmount,
                        model.Description
                    );
                }

                if (result)
                {
                    TempData["SuccessMessage"] = "點數交易成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "交易失敗（點數不足或其他錯誤）");
                }
            }

            // 重新載入用戶資訊
            var user = await _userService.GetUserByIdAsync(model.UserId);
            if (user != null)
            {
                var summary = await _walletService.GetPointsSummaryAsync(model.UserId);
                model.UserName = user.User_Name;
                model.CurrentPoints = summary["CurrentPoints"];
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

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            var walletHistory = await _walletService.GetWalletHistoryAsync(userId.Value, 1, 100);
            var summary = await _walletService.GetPointsSummaryAsync(userId.Value);

            // 轉換為 Wallet 格式
            var wallets = walletHistory.Select(h => new Wallet
            {
                WalletId = h.HistoryID,
                UserId = h.UserID,
                Amount = Math.Abs(h.PointsChanged),
                TransactionType = h.PointsChanged > 0 ? "earn" : "spend",
                TransactionDate = h.ChangeTime,
                Description = h.Description
            }).ToList();

            ViewBag.User = user;
            ViewBag.CurrentPoints = summary["CurrentPoints"];

            return View(wallets);
        }

        // 獲取用戶點數統計
        [HttpGet]
        public async Task<IActionResult> GetUserPoints(int userId)
        {
            var summary = await _walletService.GetPointsSummaryAsync(userId);

            return Json(new
            {
                currentPoints = summary["CurrentPoints"],
                earnedPoints = summary["TotalEarned"],
                spentPoints = summary["TotalSpent"]
            });
        }

        // 獲取點數統計圖表數據
        [HttpGet]
        public async Task<IActionResult> GetPointsChartData(int days = 30)
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-days);

            var allHistory = await _walletService.GetAllHistoryAsync(1, 10000);
            var historyInRange = allHistory.Where(h => h.ChangeTime.Date >= startDate && h.ChangeTime.Date <= endDate);

            var data = new List<object>();

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var dayHistory = historyInRange.Where(h => h.ChangeTime.Date == date);

                var earned = dayHistory.Where(h => h.PointsChanged > 0).Sum(h => h.PointsChanged);
                var spent = Math.Abs(dayHistory.Where(h => h.PointsChanged < 0).Sum(h => h.PointsChanged));

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
            var allHistory = await _walletService.GetAllHistoryAsync(1, 10000);

            var stats = allHistory
                .GroupBy(h => h.ChangeType)
                .Select(g => new
                {
                    type = g.Key,
                    count = g.Count(),
                    totalAmount = Math.Abs(g.Sum(h => h.PointsChanged))
                })
                .ToList();

            return Json(stats);
        }

        // 獲取用戶點數排行榜
        [HttpGet]
        public async Task<IActionResult> GetPointsLeaderboard(int top = 10)
        {
            var users = await _userService.GetAllUsersAsync(1, 1000);
            var leaderboard = new List<object>();

            foreach (var user in users.Take(top * 2)) // 取多一點以防有些用戶沒有錢包
            {
                var summary = await _walletService.GetPointsSummaryAsync(user.User_Id);
                leaderboard.Add(new
                {
                    userId = user.User_Id,
                    userName = user.User_Name,
                    userAccount = user.User_Account,
                    currentPoints = summary["CurrentPoints"]
                });
            }

            var result = leaderboard
                .OrderByDescending(u => ((dynamic)u).currentPoints)
                .Take(top)
                .ToList();

            return Json(result);
        }
    }
}
