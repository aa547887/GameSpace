using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminMiniGameController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminMiniGameController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MiniGame/AdminMiniGame
        public async Task<IActionResult> Index()
        {
            var viewModel = new MiniGameOverviewViewModel();

            // 統計數據
            viewModel.TotalGamesPlayed = await _context.MiniGames.CountAsync();
            viewModel.TodayGamesPlayed = await _context.MiniGames
                .Where(mg => mg.StartTime.Date == DateTime.Today)
                .CountAsync();
            viewModel.WeeklyGamesPlayed = await _context.MiniGames
                .Where(mg => mg.StartTime >= DateTime.Today.AddDays(-7))
                .CountAsync();
            viewModel.MonthlyGamesPlayed = await _context.MiniGames
                .Where(mg => mg.StartTime >= DateTime.Today.AddDays(-30))
                .CountAsync();

            var totalGames = viewModel.TotalGamesPlayed;
            if (totalGames > 0)
            {
                viewModel.TotalWins = await _context.MiniGames
                    .Where(mg => mg.Result == "Win")
                    .CountAsync();
                viewModel.TotalLosses = await _context.MiniGames
                    .Where(mg => mg.Result == "Lose")
                    .CountAsync();
                viewModel.TotalAborted = await _context.MiniGames
                    .Where(mg => mg.Aborted == true)
                    .CountAsync();

                viewModel.WinRate = (decimal)viewModel.TotalWins / totalGames * 100;
            }

            // 遊戲統計
            viewModel.GameStats = await _context.MiniGames
                .Include(mg => mg.User)
                .Include(mg => mg.Pet)
                .OrderByDescending(mg => mg.StartTime)
                .Take(100)
                .Select(mg => new GameStatsViewModel
                {
                    PlayId = mg.PlayID,
                    UserId = mg.UserID,
                    UserName = mg.User.User_name,
                    PetId = mg.PetID,
                    PetName = mg.Pet.PetName,
                    Level = mg.Level,
                    MonsterCount = mg.MonsterCount,
                    SpeedMultiplier = mg.SpeedMultiplier,
                    Result = mg.Result,
                    ExpGained = mg.ExpGained,
                    PointsGained = mg.PointsGained,
                    CouponGained = mg.CouponGained,
                    StartTime = mg.StartTime,
                    EndTime = mg.EndTime,
                    Aborted = mg.Aborted
                })
                .ToListAsync();

            // 用戶遊戲統計
            viewModel.UserGames = await _context.Users
                .Include(u => u.MiniGames)
                .Include(u => u.UserIntroduce)
                .Where(u => u.MiniGames.Any())
                .Select(u => new UserGameViewModel
                {
                    UserId = u.User_ID,
                    UserName = u.User_name,
                    NickName = u.UserIntroduce.User_NickName,
                    TotalGamesPlayed = u.MiniGames.Count(),
                    TotalWins = u.MiniGames.Count(mg => mg.Result == "Win"),
                    TotalLosses = u.MiniGames.Count(mg => mg.Result == "Lose"),
                    TotalAborted = u.MiniGames.Count(mg => mg.Aborted == true),
                    WinRate = u.MiniGames.Any() ? 
                        (decimal)u.MiniGames.Count(mg => mg.Result == "Win") / u.MiniGames.Count() * 100 : 0,
                    TotalPointsGained = u.MiniGames.Sum(mg => mg.PointsGained),
                    TotalExpGained = u.MiniGames.Sum(mg => mg.ExpGained),
                    LastGamePlayed = u.MiniGames.Max(mg => mg.StartTime)
                })
                .OrderByDescending(u => u.TotalGamesPlayed)
                .ToListAsync();

            return View(viewModel);
        }

        // GET: MiniGame/AdminMiniGame/GetMiniGameOverview
        [HttpGet]
        public async Task<IActionResult> GetMiniGameOverview()
        {
            try
            {
                var totalGames = await _context.MiniGames.CountAsync();
                var totalWins = await _context.MiniGames
                    .Where(mg => mg.Result == "Win")
                    .CountAsync();
                var totalLosses = await _context.MiniGames
                    .Where(mg => mg.Result == "Lose")
                    .CountAsync();
                var totalAborted = await _context.MiniGames
                    .Where(mg => mg.Aborted == true)
                    .CountAsync();

                var data = new
                {
                    totalGamesPlayed = totalGames,
                    todayGamesPlayed = await _context.MiniGames
                        .Where(mg => mg.StartTime.Date == DateTime.Today)
                        .CountAsync(),
                    weeklyGamesPlayed = await _context.MiniGames
                        .Where(mg => mg.StartTime >= DateTime.Today.AddDays(-7))
                        .CountAsync(),
                    monthlyGamesPlayed = await _context.MiniGames
                        .Where(mg => mg.StartTime >= DateTime.Today.AddDays(-30))
                        .CountAsync(),
                    totalWins = totalWins,
                    totalLosses = totalLosses,
                    totalAborted = totalAborted,
                    winRate = totalGames > 0 ? (decimal)totalWins / totalGames * 100 : 0,
                    gameStats = await _context.MiniGames
                        .Include(mg => mg.User)
                        .Include(mg => mg.Pet)
                        .OrderByDescending(mg => mg.StartTime)
                        .Take(50)
                        .Select(mg => new
                        {
                            playId = mg.PlayID,
                            userId = mg.UserID,
                            userName = mg.User.User_name,
                            petId = mg.PetID,
                            petName = mg.Pet.PetName,
                            level = mg.Level,
                            monsterCount = mg.MonsterCount,
                            speedMultiplier = mg.SpeedMultiplier,
                            result = mg.Result,
                            expGained = mg.ExpGained,
                            pointsGained = mg.PointsGained,
                            couponGained = mg.CouponGained,
                            startTime = mg.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            endTime = mg.EndTime.HasValue ? mg.EndTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                            aborted = mg.Aborted
                        })
                        .ToListAsync()
                };

                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminMiniGame/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var miniGame = await _context.MiniGames
                .Include(mg => mg.User)
                .Include(mg => mg.Pet)
                .FirstOrDefaultAsync(m => m.PlayID == id);

            if (miniGame == null)
            {
                return NotFound();
            }

            return View(miniGame);
        }

        // POST: MiniGame/AdminMiniGame/AdjustGameResult
        [HttpPost]
        public async Task<IActionResult> AdjustGameResult(int playId, string result, int pointsGained, int expGained, string couponGained)
        {
            try
            {
                var miniGame = await _context.MiniGames
                    .Include(mg => mg.User)
                    .Include(mg => mg.User.UserWallet)
                    .Include(mg => mg.Pet)
                    .FirstOrDefaultAsync(mg => mg.PlayID == playId);

                if (miniGame == null)
                {
                    return Json(new { success = false, message = "找不到遊戲記錄" });
                }

                // 記錄原始值
                var originalPoints = miniGame.PointsGained;
                var originalExp = miniGame.ExpGained;
                var originalCoupon = miniGame.CouponGained;

                // 更新遊戲結果
                miniGame.Result = result;
                miniGame.PointsGained = pointsGained;
                miniGame.ExpGained = expGained;
                miniGame.CouponGained = couponGained ?? "0";
                miniGame.EndTime = DateTime.Now;

                // 調整用戶點數
                var pointDifference = pointsGained - originalPoints;
                if (pointDifference != 0)
                {
                    miniGame.User.UserWallet.User_Point += pointDifference;

                    var walletHistory = new WalletHistory
                    {
                        UserID = miniGame.UserID,
                        ChangeType = "管理員調整遊戲結果",
                        PointsChanged = pointDifference,
                        Description = $"調整遊戲 {miniGame.PlayID} 的點數獎勵",
                        ChangeTime = DateTime.Now
                    };

                    _context.WalletHistories.Add(walletHistory);
                }

                // 調整寵物經驗
                var expDifference = expGained - originalExp;
                if (expDifference != 0 && miniGame.Pet != null)
                {
                    miniGame.Pet.Experience += expDifference;

                    // 檢查是否升級
                    var requiredExp = CalculateRequiredExp(miniGame.Pet.Level);
                    if (miniGame.Pet.Experience >= requiredExp)
                    {
                        miniGame.Pet.Level++;
                        miniGame.Pet.Experience -= requiredExp;
                        miniGame.Pet.LevelUpTime = DateTime.Now;
                        miniGame.Pet.PointsGained_LevelUp = 50;
                        miniGame.Pet.PointsGainedTime_LevelUp = DateTime.Now;

                        // 升級獎勵點數
                        miniGame.User.UserWallet.User_Point += 50;

                        var levelUpHistory = new WalletHistory
                        {
                            UserID = miniGame.UserID,
                            ChangeType = "寵物升級獎勵",
                            PointsChanged = 50,
                            Description = $"寵物 {miniGame.Pet.PetName} 升級到 {miniGame.Pet.Level} 級",
                            ChangeTime = DateTime.Now
                        };

                        _context.WalletHistories.Add(levelUpHistory);
                    }
                }

                // 處理優惠券變更
                if (couponGained != originalCoupon && !string.IsNullOrEmpty(couponGained) && couponGained != "0")
                {
                    var couponType = await _context.CouponTypes
                        .FirstOrDefaultAsync(ct => ct.Name.Contains(couponGained));

                    if (couponType != null)
                    {
                        var coupon = new Coupon
                        {
                            CouponCode = GenerateCouponCode(),
                            CouponTypeID = couponType.CouponTypeID,
                            UserID = miniGame.UserID,
                            IsUsed = false,
                            AcquiredTime = DateTime.Now
                        };

                        _context.Coupons.Add(coupon);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "遊戲結果調整成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: MiniGame/AdminMiniGame/DeleteGame
        [HttpPost]
        public async Task<IActionResult> DeleteGame(int playId)
        {
            try
            {
                var miniGame = await _context.MiniGames
                    .Include(mg => mg.User)
                    .Include(mg => mg.User.UserWallet)
                    .Include(mg => mg.Pet)
                    .FirstOrDefaultAsync(mg => mg.PlayID == playId);

                if (miniGame == null)
                {
                    return Json(new { success = false, message = "找不到遊戲記錄" });
                }

                // 扣除用戶點數
                if (miniGame.PointsGained > 0)
                {
                    miniGame.User.UserWallet.User_Point -= miniGame.PointsGained;

                    var walletHistory = new WalletHistory
                    {
                        UserID = miniGame.UserID,
                        ChangeType = "管理員刪除遊戲記錄",
                        PointsChanged = -miniGame.PointsGained,
                        Description = $"刪除遊戲 {miniGame.PlayID} 的點數獎勵",
                        ChangeTime = DateTime.Now
                    };

                    _context.WalletHistories.Add(walletHistory);
                }

                // 扣除寵物經驗
                if (miniGame.ExpGained > 0 && miniGame.Pet != null)
                {
                    miniGame.Pet.Experience -= miniGame.ExpGained;
                    if (miniGame.Pet.Experience < 0)
                    {
                        miniGame.Pet.Experience = 0;
                    }
                }

                _context.MiniGames.Remove(miniGame);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "遊戲記錄已刪除" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: MiniGame/AdminMiniGame/CreateTestGame
        [HttpPost]
        public async Task<IActionResult> CreateTestGame(int userId, int petId, int level, int monsterCount, decimal speedMultiplier)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserWallet)
                    .Include(u => u.Pet)
                    .FirstOrDefaultAsync(u => u.User_ID == userId);

                if (user == null)
                {
                    return Json(new { success = false, message = "找不到用戶" });
                }

                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.PetID == petId);

                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到寵物" });
                }

                // 創建測試遊戲記錄
                var miniGame = new MiniGame
                {
                    UserID = userId,
                    PetID = petId,
                    Level = level,
                    MonsterCount = monsterCount,
                    SpeedMultiplier = speedMultiplier,
                    Result = "Win",
                    ExpGained = level * 10,
                    ExpGainedTime = DateTime.Now,
                    PointsGained = level * monsterCount * 10,
                    PointsGainedTime = DateTime.Now,
                    CouponGained = "0",
                    CouponGainedTime = DateTime.Now,
                    HungerDelta = -10,
                    MoodDelta = 5,
                    StaminaDelta = -15,
                    CleanlinessDelta = -5,
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddMinutes(5),
                    Aborted = false
                };

                _context.MiniGames.Add(miniGame);

                // 更新用戶點數
                user.UserWallet.User_Point += miniGame.PointsGained;

                // 記錄錢包異動
                var walletHistory = new WalletHistory
                {
                    UserID = userId,
                    ChangeType = "測試遊戲獎勵",
                    PointsChanged = miniGame.PointsGained,
                    Description = $"測試遊戲獎勵: Level {level}, Monsters {monsterCount}",
                    ChangeTime = DateTime.Now
                };

                _context.WalletHistories.Add(walletHistory);

                // 更新寵物經驗
                pet.Experience += miniGame.ExpGained;

                // 檢查是否升級
                var requiredExp = CalculateRequiredExp(pet.Level);
                if (pet.Experience >= requiredExp)
                {
                    pet.Level++;
                    pet.Experience -= requiredExp;
                    pet.LevelUpTime = DateTime.Now;
                    pet.PointsGained_LevelUp = 50;
                    pet.PointsGainedTime_LevelUp = DateTime.Now;

                    // 升級獎勵點數
                    user.UserWallet.User_Point += 50;

                    var levelUpHistory = new WalletHistory
                    {
                        UserID = userId,
                        ChangeType = "寵物升級獎勵",
                        PointsChanged = 50,
                        Description = $"寵物 {pet.PetName} 升級到 {pet.Level} 級",
                        ChangeTime = DateTime.Now
                    };

                    _context.WalletHistories.Add(levelUpHistory);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "測試遊戲創建成功", playId = miniGame.PlayID });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int CalculateRequiredExp(int level)
        {
            // 簡單的經驗值計算公式
            return level * 100;
        }

        private string GenerateCouponCode()
        {
            return "COUPON" + DateTime.Now.Ticks.ToString().Substring(10);
        }
    }
}
