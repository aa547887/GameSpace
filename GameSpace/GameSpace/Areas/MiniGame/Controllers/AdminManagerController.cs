using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using GameSpace.Areas.social_hub.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme)]
    public class AdminManagerController : MiniGameBaseController
    {
        private readonly GameSpacedatabaseContext _context;

        public AdminManagerController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // GET: AdminManager
        public async Task<IActionResult> Index(string searchTerm = "", string status = "", string role = "", string sortBy = "name", int page = 1, int pageSize = 10)
        {
            var query = _context.ManagerData
                .Include(m => m.ManagerRoles)
                .AsQueryable();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(m => (m.ManagerName != null && m.ManagerName.Contains(searchTerm)) ||
                                       (m.ManagerAccount != null && m.ManagerAccount.Contains(searchTerm)) ||
                                       m.ManagerEmail.Contains(searchTerm));
            }

            // 狀態篩選
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "active")
                    query = query.Where(m => !m.ManagerLockoutEnabled || !m.ManagerLockoutEnd.HasValue || m.ManagerLockoutEnd <= DateTime.Now);
                else if (status == "locked")
                    query = query.Where(m => m.ManagerLockoutEnabled && m.ManagerLockoutEnd.HasValue && m.ManagerLockoutEnd > DateTime.Now);
            }

            // 角色篩選
            if (!string.IsNullOrEmpty(role) && int.TryParse(role, out int roleId))
            {
                query = query.Where(m => m.ManagerRoles.Any(mr => mr.ManagerRoleId == roleId));
            }

            // 排序
            query = sortBy switch
            {
                "account" => query.OrderBy(m => m.ManagerAccount),
                "email" => query.OrderBy(m => m.ManagerEmail),
                "created" => query.OrderBy(m => m.AdministratorRegistrationDate),
                _ => query.OrderBy(m => m.ManagerName)
            };

            // 分頁
            var totalCount = await query.CountAsync();
            var managers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 設定 ViewBag 用於搜尋和篩選
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.Role = role;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalManagers = totalCount;
            ViewBag.ActiveManagers = await _context.ManagerData.CountAsync(m => !m.ManagerLockoutEnabled || !m.ManagerLockoutEnd.HasValue || m.ManagerLockoutEnd <= DateTime.Now);
            ViewBag.TotalRoles = await _context.ManagerRolePermissions.CountAsync();
            ViewBag.Managers = managers;
            ViewBag.RoleList = await _context.ManagerRolePermissions.ToListAsync();

            return View();
        }

        // GET: AdminManager/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manager = await _context.ManagerData
                .Include(m => m.ManagerRoles)
                .FirstOrDefaultAsync(m => m.ManagerId == id);

            if (manager == null)
            {
                return NotFound();
            }

            return View(manager);
        }

        // GET: AdminManager/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _context.ManagerRolePermissions.ToListAsync();
            return View();
        }

        // POST: AdminManager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string managerName, string managerAccount, string managerEmail, string password, List<int> roleIds)
        {
            if (ModelState.IsValid)
            {
                // 檢查帳號是否已存在
                if (await _context.ManagerData.AnyAsync(m => m.ManagerAccount == managerAccount))
                {
                    ModelState.AddModelError("managerAccount", "此帳號已存在");
                    ViewBag.Roles = await _context.ManagerRolePermissions.ToListAsync();
                    return View();
                }

                // 檢查電子郵件是否已存在
                if (await _context.ManagerData.AnyAsync(m => m.ManagerEmail == managerEmail))
                {
                    ModelState.AddModelError("managerEmail", "此電子郵件已存在");
                    ViewBag.Roles = await _context.ManagerRolePermissions.ToListAsync();
                    return View();
                }

                var manager = new ManagerDatum
                {
                    ManagerName = managerName,
                    ManagerAccount = managerAccount,
                    ManagerPassword = HashPassword(password),
                    ManagerEmail = managerEmail,
                    AdministratorRegistrationDate = DateTime.Now,
                    ManagerLockoutEnabled = false,
                    ManagerLockoutEnd = null,
                    ManagerEmailConfirmed = false,
                    ManagerAccessFailedCount = 0
                };

                _context.Add(manager);
                await _context.SaveChangesAsync();

                // 分配角色 - 需要使用正確的關聯表
                if (roleIds != null && roleIds.Any())
                {
                    foreach (var roleId in roleIds)
                    {
                        var rolePermission = await _context.ManagerRolePermissions.FindAsync(roleId);
                        if (rolePermission != null)
                        {
                            manager.ManagerRoles.Add(rolePermission);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "管理者建立成功";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = await _context.ManagerRolePermissions.ToListAsync();
            return View();
        }

        // GET: AdminManager/CreateRole
        public async Task<IActionResult> CreateRole()
        {
            ViewBag.Permissions = await _context.ManagerPermission.ToListAsync();
            return View();
        }

        // POST: AdminManager/CreateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(AdminManagerRoleCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = new ManagerRolePermission
                {
                    role_name = model.role_name,
                    role_description = model.role_description,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now
                };

                _context.Add(role);
                await _context.SaveChangesAsync();

                // 分配權限
                if (model.PermissionIds.Any())
                {
                    foreach (var permissionId in model.PermissionIds)
                    {
                        var rolePermission = new ManagerRolePermission
                        {
                            ManagerRole_Id = role.ManagerRole_Id,
                            Permission_Id = permissionId
                        };
                        _context.Add(rolePermission);
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "角色建立成功";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Permissions = await _context.ManagerPermission.ToListAsync();
            return View(model);
        }

        // GET: AdminManager/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manager = await _context.Manager
                .Include(m => m.ManagerRoles)
                .FirstOrDefaultAsync(m => m.Manager_Id == id);

            if (manager == null)
            {
                return NotFound();
            }

            var model = new AdminManagerEditViewModel
            {
                Manager_Id = manager.Manager_Id,
                Manager_Name = manager.Manager_Name,
                Manager_Account = manager.Manager_Account,
                Manager_Email = manager.Manager_Email,
                RoleIds = manager.ManagerRoles.Select(mr => mr.ManagerRole_Id).ToList()
            };

            ViewBag.Roles = await _context.ManagerRolePermission.ToListAsync();
            return View(model);
        }

        // POST: AdminManager/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminManagerEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var manager = await _context.Manager.FindAsync(id);
                    if (manager == null)
                    {
                        return NotFound();
                    }

                    // 檢查帳號是否已被其他管理者使用
                    if (await _context.Manager.AnyAsync(m => m.Manager_Account == model.Manager_Account && m.Manager_Id != id))
                    {
                        ModelState.AddModelError("Manager_Account", "此帳號已被其他管理者使用");
                        ViewBag.Roles = await _context.ManagerRolePermission.ToListAsync();
                        return View(model);
                    }

                    // 檢查電子郵件是否已被其他管理者使用
                    if (await _context.Manager.AnyAsync(m => m.Manager_Email == model.Manager_Email && m.Manager_Id != id))
                    {
                        ModelState.AddModelError("Manager_Email", "此電子郵件已被其他管理者使用");
                        ViewBag.Roles = await _context.ManagerRolePermission.ToListAsync();
                        return View(model);
                    }

                    manager.Manager_Name = model.Manager_Name;
                    manager.Manager_Account = model.Manager_Account;
                    manager.Manager_Email = model.Manager_Email;

                    // 如果提供了新密碼，則更新密碼
                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        if (model.NewPassword != model.ConfirmNewPassword)
                        {
                            ModelState.AddModelError("ConfirmNewPassword", "新密碼和確認密碼不匹配");
                            ViewBag.Roles = await _context.ManagerRolePermission.ToListAsync();
                            return View(model);
                        }
                        manager.Manager_Password = HashPassword(model.NewPassword);
                    }

                    _context.Update(manager);

                    // 更新角色分配
                    var existingRoles = await _context.ManagerRole
                        .Where(mr => mr.Manager_Id == id)
                        .ToListAsync();

                    _context.ManagerRole.RemoveRange(existingRoles);

                    if (model.RoleIds.Any())
                    {
                        foreach (var roleId in model.RoleIds)
                        {
                            var managerRole = new ManagerRole
                            {
                                Manager_Id = id,
                                ManagerRole_Id = roleId
                            };
                            _context.Add(managerRole);
                        }
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "管理者更新成功";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ManagerExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Roles = await _context.ManagerRolePermission.ToListAsync();
            return View(model);
        }

        // GET: AdminManager/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manager = await _context.Manager
                .Include(m => m.ManagerRoles)
                .ThenInclude(mr => mr.ManagerRolePermission)
                .FirstOrDefaultAsync(m => m.Manager_Id == id);

            if (manager == null)
            {
                return NotFound();
            }

            return View(manager);
        }

        // POST: AdminManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var manager = await _context.Manager.FindAsync(id);
            if (manager != null)
            {
                _context.Manager.Remove(manager);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "管理者刪除成功";
            }

            return RedirectToAction(nameof(Index));
        }

        // 切換管理者狀態
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var manager = await _context.Manager.FindAsync(id);
            if (manager != null)
            {
                if (manager.Manager_LockoutEnabled && manager.Manager_LockoutEnd.HasValue && manager.Manager_LockoutEnd > DateTime.Now)
                {
                    // 解鎖
                    manager.Manager_LockoutEnabled = false;
                    manager.Manager_LockoutEnd = null;
                }
                else
                {
                    // 鎖定
                    manager.Manager_LockoutEnabled = true;
                    manager.Manager_LockoutEnd = DateTime.Now.AddDays(30);
                }

                _context.Update(manager);
                await _context.SaveChangesAsync();

                return Json(new { success = true, isLocked = manager.Manager_LockoutEnabled });
            }

            return Json(new { success = false });
        }

        // 重置管理者密碼
        [HttpPost]
        public async Task<IActionResult> ResetPassword(int id, string newPassword)
        {
            var manager = await _context.Manager.FindAsync(id);
            if (manager != null)
            {
                manager.Manager_Password = HashPassword(newPassword);
                _context.Update(manager);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        // 獲取管理者統計數據
        [HttpGet]
        public async Task<IActionResult> GetManagerStats()
        {
            var now = DateTime.Now;
            var stats = new
            {
                total = await _context.Manager.CountAsync(),
                active = await _context.Manager.CountAsync(m => !m.Manager_LockoutEnabled || !m.Manager_LockoutEnd.HasValue || m.Manager_LockoutEnd <= now),
                locked = await _context.Manager.CountAsync(m => m.Manager_LockoutEnabled && m.Manager_LockoutEnd.HasValue && m.Manager_LockoutEnd > now),
                roles = await _context.ManagerRolePermission.CountAsync(),
                todayLogins = await _context.Manager.CountAsync(m => m.LastLoginAt.HasValue && m.LastLoginAt.Value.Date == DateTime.Today)
            };

            return Json(stats);
        }

        // 獲取角色分佈
        [HttpGet]
        public async Task<IActionResult> GetRoleDistribution()
        {
            var distribution = await _context.ManagerRole
                .GroupBy(mr => mr.ManagerRolePermission.role_name)
                .Select(g => new
                {
                    role = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(g => g.count)
                .ToListAsync();

            return Json(distribution);
        }

        // 獲取登入趨勢
        [HttpGet]
        public async Task<IActionResult> GetLoginTrend(int days = 30)
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-days);

            var trend = await _context.Manager
                .Where(m => m.LastLoginAt >= startDate)
                .GroupBy(m => m.LastLoginAt.Value.Date)
                .Select(g => new
                {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    count = g.Count()
                })
                .OrderBy(g => g.date)
                .ToListAsync();

            return Json(trend);
        }

        private bool ManagerExists(int id)
        {
            return _context.Manager.Any(e => e.Manager_Id == id);
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
