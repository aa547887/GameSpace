using Microsoft.AspNetCore.Mvc;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Models;
using System.Security.Cryptography;
using System.Text;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminUserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IWalletService _walletService;

        public AdminUserController(IUserService userService, IWalletService walletService)
        {
            _userService = userService;
            _walletService = walletService;
        }

        // GET: AdminUser
        public async Task<IActionResult> Index(string searchTerm = "", string status = "", string sortBy = "name", int page = 1, int pageSize = 10)
        {
            IEnumerable<Users> users;

            // 狀態篩選
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "active")
                    users = await _userService.GetActiveUsersAsync();
                else if (status == "inactive")
                    users = await _userService.GetInactiveUsersAsync();
                else
                    users = await _userService.GetAllUsersAsync(1, 10000);
            }
            else
            {
                users = await _userService.GetAllUsersAsync(1, 10000);
            }

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(u => u.User_Name.Contains(searchTerm) ||
                                        u.User_Account.Contains(searchTerm) ||
                                        u.User_Email.Contains(searchTerm));
            }

            // 排序
            users = sortBy switch
            {
                "account" => users.OrderBy(u => u.User_Account),
                "email" => users.OrderBy(u => u.User_Email),
                "created" => users.OrderBy(u => u.User_CreatedAt),
                "lastLogin" => users.OrderByDescending(u => u.User_LockoutEnd),
                _ => users.OrderBy(u => u.User_Name)
            };

            // 分頁
            var totalCount = users.Count();
            var pagedUsers = users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new AdminUserIndexViewModel
            {
                Users = new PagedResult<Users>
                {
                    Items = pagedUsers,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                }
            };

            // 設定 ViewBag 用於搜尋和篩選
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalUsers = await _userService.GetTotalUsersCountAsync();
            ViewBag.ActiveUsers = await _userService.GetActiveUsersCountAsync();
            ViewBag.InactiveUsers = ViewBag.TotalUsers - ViewBag.ActiveUsers;

            return View(viewModel);
        }

        // GET: AdminUser/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userService.GetUserByIdAsync(id.Value);

            if (user == null)
            {
                return NotFound();
            }

            // 取得額外資訊
            var summary = await _walletService.GetPointsSummaryAsync(id.Value);
            ViewBag.WalletSummary = summary;

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
                var existingAccount = await _userService.GetUserByAccountAsync(model.User_account);
                if (existingAccount != null)
                {
                    ModelState.AddModelError("User_account", "此帳號已存在");
                    return View(model);
                }

                // 檢查電子郵件是否已存在
                var existingEmail = await _userService.GetUserByEmailAsync(model.User_email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("User_email", "此電子郵件已存在");
                    return View(model);
                }

                var user = new Users
                {
                    User_Name = model.User_name,
                    User_Account = model.User_account,
                    User_Password = HashPassword(model.Password),
                    User_Email = model.User_email,
                    User_Phone = model.User_phone,
                    User_Birthday = model.User_birthday,
                    User_Gender = model.User_gender,
                    User_Address = model.User_address,
                    User_Status = model.IsActive ? "Active" : "Inactive",
                    User_CreatedAt = DateTime.UtcNow
                };

                var result = await _userService.CreateUserAsync(user);

                if (result)
                {
                    TempData["SuccessMessage"] = "用戶建立成功（已自動建立錢包）";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "建立用戶失敗");
                }
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

            var user = await _userService.GetUserByIdAsync(id.Value);
            if (user == null)
            {
                return NotFound();
            }

            var model = new AdminUserEditViewModel
            {
                User_Id = user.User_Id,
                User_name = user.User_Name,
                User_account = user.User_Account,
                User_email = user.User_Email,
                User_phone = user.User_Phone,
                User_birthday = user.User_Birthday,
                User_gender = user.User_Gender,
                User_address = user.User_Address,
                IsActive = user.User_Status == "Active"
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
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // 檢查帳號是否已被其他用戶使用
                var existingAccount = await _userService.GetUserByAccountAsync(model.User_account);
                if (existingAccount != null && existingAccount.User_Id != id)
                {
                    ModelState.AddModelError("User_account", "此帳號已被其他用戶使用");
                    return View(model);
                }

                // 檢查電子郵件是否已被其他用戶使用
                var existingEmail = await _userService.GetUserByEmailAsync(model.User_email);
                if (existingEmail != null && existingEmail.User_Id != id)
                {
                    ModelState.AddModelError("User_email", "此電子郵件已被其他用戶使用");
                    return View(model);
                }

                user.User_Name = model.User_name;
                user.User_Account = model.User_account;
                user.User_Email = model.User_email;
                user.User_Phone = model.User_phone;
                user.User_Birthday = model.User_birthday;
                user.User_Gender = model.User_gender;
                user.User_Address = model.User_address;
                user.User_Status = model.IsActive ? "Active" : "Inactive";

                // 如果提供了新密碼，則更新密碼
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    if (model.NewPassword != model.ConfirmNewPassword)
                    {
                        ModelState.AddModelError("ConfirmNewPassword", "新密碼和確認密碼不匹配");
                        return View(model);
                    }
                    user.User_Password = HashPassword(model.NewPassword);
                }

                var result = await _userService.UpdateUserAsync(user);

                if (result)
                {
                    TempData["SuccessMessage"] = "用戶更新成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "更新用戶失敗");
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

            var user = await _userService.GetUserByIdAsync(id.Value);

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
            // 軟刪除：將用戶設為非活躍狀態
            var result = await _userService.DeactivateUserAsync(id);

            if (result)
            {
                TempData["SuccessMessage"] = "用戶已停用";
            }
            else
            {
                TempData["ErrorMessage"] = "停用用戶失敗";
            }

            return RedirectToAction(nameof(Index));
        }

        // 切換用戶狀態
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user != null)
            {
                bool result;
                if (user.User_Status == "Active")
                {
                    result = await _userService.DeactivateUserAsync(id);
                }
                else
                {
                    result = await _userService.ActivateUserAsync(id);
                }

                if (result)
                {
                    var updatedUser = await _userService.GetUserByIdAsync(id);
                    return Json(new { success = true, isActive = updatedUser.User_Status == "Active" });
                }
            }

            return Json(new { success = false });
        }

        // 重置用戶密碼
        [HttpPost]
        public async Task<IActionResult> ResetPassword(int id, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
            {
                return Json(new { success = false, message = "新密碼不能為空" });
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user != null)
            {
                user.User_Password = HashPassword(newPassword);
                var result = await _userService.UpdateUserAsync(user);

                if (result)
                {
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
        }

        // 鎖定用戶
        [HttpPost]
        public async Task<IActionResult> LockUser(int id, int? days = null)
        {
            DateTime? lockoutEnd = days.HasValue ? DateTime.UtcNow.AddDays(days.Value) : null;
            var result = await _userService.LockUserAsync(id, lockoutEnd);

            if (result)
            {
                return Json(new { success = true, message = "用戶已鎖定" });
            }

            return Json(new { success = false, message = "鎖定失敗" });
        }

        // 解鎖用戶
        [HttpPost]
        public async Task<IActionResult> UnlockUser(int id)
        {
            var result = await _userService.UnlockUserAsync(id);

            if (result)
            {
                return Json(new { success = true, message = "用戶已解鎖" });
            }

            return Json(new { success = false, message = "解鎖失敗" });
        }

        // 獲取用戶統計
        [HttpGet]
        public async Task<IActionResult> GetUserStats(int userId)
        {
            var summary = await _walletService.GetPointsSummaryAsync(userId);
            return Json(summary);
        }

        // 搜尋用戶
        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return Json(new List<object>());
            }

            var users = await _userService.SearchUsersAsync(term);
            var result = users.Take(10).Select(u => new
            {
                id = u.User_Id,
                name = u.User_Name,
                account = u.User_Account,
                email = u.User_Email
            });

            return Json(result);
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
