using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class SignInController : Controller
    {
        private readonly IMiniGameService _miniGameService;
        private readonly GameSpacedatabaseContext _context;

        public SignInController(IMiniGameService miniGameService, GameSpacedatabaseContext context)
        {
            _miniGameService = miniGameService;
            _context = context;
        }

        // 每日簽到頁面
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                // 檢查今日是否已簽到
                var today = DateTime.Today;
                var hasSignedInToday = await _context.UserSignInStats
                    .AnyAsync(s => s.UserId == userId && s.SignTime.Date == today);

                // 獲取連續簽到天數
                var consecutiveDays = await GetConsecutiveSignInDaysAsync(userId);

                // 獲取簽到規則
                var signInRule = await _context.SignInRules.FirstOrDefaultAsync();

                var viewModel = new DailySignInViewModel
                {
                    UserId = userId,
                    HasSignedInToday = hasSignedInToday,
                    ConsecutiveDays = consecutiveDays,
                    SignInRule = signInRule,
                    Today = today
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入簽到頁面時發生錯誤：{ex.Message}";
                return View(new DailySignInViewModel { UserId = GetCurrentUserId() });
            }
        }

        // 執行簽到
        [HttpPost]
        public async Task<IActionResult> SignIn()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new { success = false, message = "請先登入" });
            }

            try
            {
                var today = DateTime.Today;
                
                // 檢查今日是否已簽到
                var hasSignedInToday = await _context.UserSignInStats
                    .AnyAsync(s => s.UserId == userId && s.SignTime.Date == today);

                if (hasSignedInToday)
                {
                    return Json(new { success = false, message = "今日已簽到" });
                }

                // 獲取簽到規則
                var signInRule = await _context.SignInRules.FirstOrDefaultAsync();
                if (signInRule == null)
                {
                    return Json(new { success = false, message = "簽到規則未設定" });
                }

                // 計算獎勵
                var pointsGained = signInRule.DailyPoints;
                var expGained = signInRule.DailyExp;
                var consecutiveDays = await GetConsecutiveSignInDaysAsync(userId) + 1;

                // 檢查連續簽到獎勵
                if (consecutiveDays % 7 == 0) // 每7天額外獎勵
                {
                    pointsGained += signInRule.WeeklyBonusPoints;
                }

                if (consecutiveDays % 30 == 0) // 每30天額外獎勵
                {
                    pointsGained += signInRule.MonthlyBonusPoints;
                }

                // 創建簽到記錄
                var signInRecord = new UserSignInStat
                {
                    UserId = userId,
                    SignTime = DateTime.Now,
                    PointsGained = pointsGained,
                    PointsGainedTime = DateTime.Now,
                    ExpGained = expGained,
                    ExpGainedTime = DateTime.Now,
                    CouponGained = "0",
                    CouponGainedTime = null
                };

                _context.UserSignInStats.Add(signInRecord);

                // 更新用戶點數
                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);
                
                if (wallet != null)
                {
                    wallet.UserPoint += pointsGained;
                }
                else
                {
                    wallet = new UserWallet
                    {
                        UserId = userId,
                        UserPoint = pointsGained
                    };
                    _context.UserWallets.Add(wallet);
                }

                // 更新寵物經驗
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.UserId == userId);
                
                if (pet != null && expGained > 0)
                {
                    pet.Experience += expGained;
                    
                    // 檢查是否升級
                    var requiredExp = pet.Level * 100;
                    if (pet.Experience >= requiredExp)
                    {
                        pet.Level++;
                        pet.Experience -= requiredExp;
                        pet.LevelUpTime = DateTime.Now;
                        
                        // 升級獎勵
                        var levelUpReward = pet.Level * 50;
                        wallet.UserPoint += levelUpReward;
                    }
                }

                // 記錄錢包歷史
                var walletHistory = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "SignIn",
                    PointsChanged = pointsGained,
                    Description = $"每日簽到獲得 {pointsGained} 點",
                    ChangeTime = DateTime.Now
                };

                _context.WalletHistories.Add(walletHistory);

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "簽到成功！",
                    pointsGained = pointsGained,
                    expGained = expGained,
                    consecutiveDays = consecutiveDays,
                    newBalance = wallet.UserPoint
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"簽到失敗：{ex.Message}" });
            }
        }

        // 獲取連續簽到天數
        private async Task<int> GetConsecutiveSignInDaysAsync(int userId)
        {
            var signInRecords = await _context.UserSignInStats
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SignTime)
                .ToListAsync();

            if (!signInRecords.Any()) return 0;

            var consecutiveDays = 0;
            var currentDate = DateTime.Today;

            foreach (var record in signInRecords)
            {
                if (record.SignTime.Date == currentDate)
                {
                    consecutiveDays++;
                    currentDate = currentDate.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return consecutiveDays;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }
    }

    // ViewModels
    public class DailySignInViewModel
    {
        public int UserId { get; set; }
        public bool HasSignedInToday { get; set; }
        public int ConsecutiveDays { get; set; }
        public SignInRule SignInRule { get; set; }
        public DateTime Today { get; set; }
    }
}
