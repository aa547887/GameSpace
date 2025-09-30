using Microsoft.AspNetCore.Mvc;
using GameSpace.Areas.MiniGame.Data;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // 測試資料庫連接
                var userCount = await _context.Users.CountAsync();
                var petCount = await _context.Pets.CountAsync();
                var miniGameCount = await _context.MiniGames.CountAsync();
                var walletCount = await _context.UserWallets.CountAsync();
                var couponCount = await _context.Coupons.CountAsync();
                var eVoucherCount = await _context.EVouchers.CountAsync();
                var managerCount = await _context.ManagerData.CountAsync();

                ViewBag.UserCount = userCount;
                ViewBag.PetCount = petCount;
                ViewBag.MiniGameCount = miniGameCount;
                ViewBag.WalletCount = walletCount;
                ViewBag.CouponCount = couponCount;
                ViewBag.EVoucherCount = eVoucherCount;
                ViewBag.ManagerCount = managerCount;
                ViewBag.ConnectionStatus = "成功連接到資料庫！";

                return View();
            }
            catch (System.Exception ex)
            {
                ViewBag.ConnectionStatus = $"資料庫連接失敗: {ex.Message}";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.UserWallet)
                    .Include(u => u.Pet)
                    .Take(10)
                    .ToListAsync();

                return Json(new { success = true, data = users });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPets()
        {
            try
            {
                var pets = await _context.Pets
                    .Include(p => p.User)
                    .Take(10)
                    .ToListAsync();

                return Json(new { success = true, data = pets });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMiniGames()
        {
            try
            {
                var miniGames = await _context.MiniGames
                    .Include(mg => mg.User)
                    .Include(mg => mg.Pet)
                    .Take(10)
                    .ToListAsync();

                return Json(new { success = true, data = miniGames });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCoupons()
        {
            try
            {
                var coupons = await _context.Coupons
                    .Include(c => c.User)
                    .Include(c => c.CouponType)
                    .Take(10)
                    .ToListAsync();

                return Json(new { success = true, data = coupons });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEVouchers()
        {
            try
            {
                var eVouchers = await _context.EVouchers
                    .Include(ev => ev.User)
                    .Include(ev => ev.EVoucherType)
                    .Take(10)
                    .ToListAsync();

                return Json(new { success = true, data = eVouchers });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetManagers()
        {
            try
            {
                var managers = await _context.ManagerData
                    .Include(md => md.ManagerRoles)
                    .ThenInclude(mr => mr.ManagerRolePermission)
                    .Take(10)
                    .ToListAsync();

                return Json(new { success = true, data = managers });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
