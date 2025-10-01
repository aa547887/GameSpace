using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Data;
using System.Security.Cryptography;
using System.Text;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminUserController : Controller
    {
        private readonly GameSpaceContext _context;

        public AdminUserController(GameSpaceContext context)
        {
            _context = context;
        }

        // GET: AdminUser
        public async Task<IActionResult> Index(string searchTerm = "", string status = "", string sortBy = "name", int page = 1, int pageSize = 10)
        {
            var query = _context.Users.AsQueryable();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.User_name.Contains(searchTerm) || 
                                       u.User_account.Contains(searchTerm) || 
                                       u.User_email.Contains(searchTerm));
            }

            // 狀態篩選
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "active")
                    query = query.Where(u => u.IsActive);
                else if (status == "inactive")
                    query = query.Where(u => !u.IsActive);
            }

            // 排序
            query = sortBy switch
            {
                "account" => query.OrderBy(u => u.User_account),
                "email" => query.OrderBy(u => u.User_email),
                "created" => query.OrderBy(u => u.User_registration_date),
                "lastLogin" => query.OrderByDescending(u => u.LastLoginAt),
                _ => query.OrderBy(u => u.User_name)
            };

            // 分頁
            var totalCount = await query.CountAsync();
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new AdminUserIndexViewModel
            {
                Users = new PagedResult<Users>
                {
                    Items = users,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                }
            };

            // 設定 ViewBag 用於搜尋和篩選
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalUsers = totalCount;
            ViewBag.ActiveUsers = await _context.Users.CountAsync(u => u.IsActive);
            ViewBag.InactiveUsers = await _context.Users.CountAsync(u => !u.IsActive);

            return View(viewModel);
        }

        // GET: AdminUser/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Wallets)
                .Include(u => u.SignIns)
                .Include(u => u.Pets)
                .Include(u => u.GamePlayRecords)
                .FirstOrDefaultAsync(m => m.User_Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: AdminUser/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminUser/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminUserCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 檢查帳號是否已存在
                if (await _context.Users.AnyAsync(u => u.User_account == model.User_account))
                {
                    ModelState.AddModelError("User_account", "此帳號已存在");
                    return View(model);
                }

                // 檢查電子郵件是否已存在
                if (await _context.Users.AnyAsync(u => u.User_email == model.User_email))
                {
                    ModelState.AddModelError("User_email", "此電子郵件已存在");
                    return View(model);
                }

                var user = new Users
                {
                    User_name = model.User_name,
                    User_account = model.User_account,
                    User_password = HashPassword(model.Password),
                    User_email = model.User_email,
                    User_phone = model.User_phone,
                    User_birthday = model.User_birthday,
                    User_gender = model.User_gender,
                    User_address = model.User_address,
                    IsActive = model.IsActive,
                    User_registration_date = DateTime.Now,
                    LastLoginAt = null
                };

                _context.Add(user);
                await _context.SaveChangesAsync();

                // 建立初始錢包
                var wallet = new Wallet
                {
                    UserId = user.User_Id,
                    Amount = 0,
                    TransactionType = "initial",
                    TransactionDate = DateTime.Now,
                    Description = "初始錢包"
                };
                _context.Add(wallet);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "用戶建立成功";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: AdminUser/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new AdminUserEditViewModel
            {
                User_Id = user.User_Id,
                User_name = user.User_name,
                User_account = user.User_account,
                User_email = user.User_email,
                User_phone = user.User_phone,
                User_birthday = user.User_birthday,
                User_gender = user.User_gender,
                User_address = user.User_address,
                IsActive = user.IsActive
            };

            return View(model);
        }

        // POST: AdminUser/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminUserEditViewModel model)
        {
            if (id != model.User_Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.Users.FindAsync(id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    // 檢查帳號是否已被其他用戶使用
                    if (await _context.Users.AnyAsync(u => u.User_account == model.User_account && u.User_Id != id))
                    {
                        ModelState.AddModelError("User_account", "此帳號已被其他用戶使用");
                        return View(model);
                    }

                    // 檢查電子郵件是否已被其他用戶使用
                    if (await _context.Users.AnyAsync(u => u.User_email == model.User_email && u.User_Id != id))
                    {
                        ModelState.AddModelError("User_email", "此電子郵件已被其他用戶使用");
                        return View(model);
                    }

                    user.User_name = model.User_name;
                    user.User_account = model.User_account;
                    user.User_email = model.User_email;
                    user.User_phone = model.User_phone;
                    user.User_birthday = model.User_birthday;
                    user.User_gender = model.User_gender;
                    user.User_address = model.User_address;
                    user.IsActive = model.IsActive;

                    // 如果提供了新密碼，則更新密碼
                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        if (model.NewPassword != model.ConfirmNewPassword)
                        {
                            ModelState.AddModelError("ConfirmNewPassword", "新密碼和確認密碼不匹配");
                            return View(model);
                        }
                        user.User_password = HashPassword(model.NewPassword);
                    }

                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "用戶更新成功";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(id))
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

        // GET: AdminUser/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Wallets)
                .Include(u => u.SignIns)
                .Include(u => u.Pets)
                .Include(u => u.GamePlayRecords)
                .FirstOrDefaultAsync(m => m.User_Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: AdminUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // 軟刪除：將用戶設為非活躍狀態
                user.IsActive = false;
                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "用戶已停用";
            }

            return RedirectToAction(nameof(Index));
        }

        // 切換用戶狀態
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                _context.Update(user);
                await _context.SaveChangesAsync();

                return Json(new { success = true, isActive = user.IsActive });
            }

            return Json(new { success = false });
        }

        // 重置用戶密碼
        [HttpPost]
        public async Task<IActionResult> ResetPassword(int id, string newPassword)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.User_password = HashPassword(newPassword);
                _context.Update(user);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.User_Id == id);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
