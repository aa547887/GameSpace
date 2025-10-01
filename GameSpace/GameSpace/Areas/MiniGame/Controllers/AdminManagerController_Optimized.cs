using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 管理員管理控制器 - 完整CRUD操作
    /// </summary>
    [Area(""MiniGame"")]
    [Authorize(AuthenticationSchemes = ""AdminCookie"", Policy = ""AdminOnly"")]
    public class AdminManagerController : MiniGameBaseController
    {
        public AdminManagerController(
            MiniGameDbContext context, 
            IMiniGameAdminService adminService,
            IMiniGamePermissionService permissionService,
            ILogger<AdminManagerController> logger) 
            : base(context, adminService, permissionService, logger)
        {
        }

        /// <summary>
        /// 管理員列表頁面
        /// </summary>
        public async Task<IActionResult> Index(
            string searchTerm = "", 
            string status = "", 
            string role = "", 
            string sortBy = ""name"", 
            int page = 1, 
            int pageSize = 10)
        {
            try
            {
                // 檢查權限
                if (!await CheckPermissionAsync(""AdministratorPrivilegesManagement""))
                {
                    TempData[""ErrorMessage""] = ""您沒有權限訪問此功能"";
                    return RedirectToAction(""Index"", ""Admin"");
                }

                var query = _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .ThenInclude(mr => mr.ManagerRolePermission)
                    .AsQueryable();

                // 搜尋功能
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(m => m.Manager_Name.Contains(searchTerm) || 
                                           m.Manager_Account.Contains(searchTerm) || 
                                           m.Manager_Email.Contains(searchTerm));
                }

                // 狀態篩選
                if (!string.IsNullOrEmpty(status))
                {
                    switch (status.ToLower())
                    {
                        case ""active"":
                            query = query.Where(m => !m.Manager_LockoutEnabled || 
                                                   !m.Manager_LockoutEnd.HasValue || 
                                                   m.Manager_LockoutEnd <= DateTime.Now);
                            break;
                        case ""locked"":
                            query = query.Where(m => m.Manager_LockoutEnabled && 
                                                   m.Manager_LockoutEnd.HasValue && 
                                                   m.Manager_LockoutEnd > DateTime.Now);
                            break;
                        case ""unconfirmed"":
                            query = query.Where(m => !m.Manager_EmailConfirmed);
                            break;
                    }
                }

                // 角色篩選
                if (!string.IsNullOrEmpty(role) && int.TryParse(role, out int roleId))
                {
                    query = query.Where(m => m.ManagerRoles.Any(mr => mr.ManagerRole_Id == roleId));
                }

                // 排序
                query = sortBy.ToLower() switch
                {
                    ""email"" => query.OrderBy(m => m.Manager_Email),
                    ""date"" => query.OrderByDescending(m => m.Administrator_registration_date),
                    ""account"" => query.OrderBy(m => m.Manager_Account),
                    _ => query.OrderBy(m => m.Manager_Name)
                };

                // 分頁
                var totalCount = await query.CountAsync();
                var managers = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m => new ManagerListViewModel
                    {
                        Manager_Id = m.Manager_Id,
                        Manager_Name = m.Manager_Name,
                        Manager_Account = m.Manager_Account,
                        Manager_Email = m.Manager_Email,
                        Manager_EmailConfirmed = m.Manager_EmailConfirmed,
                        Manager_LockoutEnabled = m.Manager_LockoutEnabled,
                        Manager_LockoutEnd = m.Manager_LockoutEnd,
                        Administrator_registration_date = m.Administrator_registration_date,
                        Roles = m.ManagerRoles.Select(mr => mr.ManagerRolePermission.role_name).ToList()
                    })
                    .ToListAsync();

                var viewModel = new AdminManagerIndexViewModel
                {
                    Managers = managers,
                    SearchTerm = searchTerm,
                    Status = status,
                    Role = role,
                    SortBy = sortBy,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                // 載入角色選項
                viewModel.RoleOptions = await _context.ManagerRolePermissions
                    .Select(rp => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = rp.ManagerRole_Id.ToString(),
                        Text = rp.role_name
                    })
                    .ToListAsync();

                LogOperation(""查看管理員列表"", $""搜尋: {searchTerm}, 狀態: {status}, 角色: {role}"");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleError(ex, ""載入管理員列表"");
            }
        }

        /// <summary>
        /// 管理員詳情頁面
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (!await CheckPermissionAsync(""AdministratorPrivilegesManagement""))
                {
                    TempData[""ErrorMessage""] = ""您沒有權限訪問此功能"";
                    return RedirectToAction(""Index"");
                }

                var manager = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .ThenInclude(mr => mr.ManagerRolePermission)
                    .FirstOrDefaultAsync(m => m.Manager_Id == id);

                if (manager == null)
                {
                    TempData[""ErrorMessage""] = ""找不到指定的管理員"";
                    return RedirectToAction(""Index"");
                }

                var viewModel = new ManagerDetailsViewModel
                {
                    Manager_Id = manager.Manager_Id,
                    Manager_Name = manager.Manager_Name,
                    Manager_Account = manager.Manager_Account,
                    Manager_Email = manager.Manager_Email,
                    Manager_EmailConfirmed = manager.Manager_EmailConfirmed,
                    Manager_AccessFailedCount = manager.Manager_AccessFailedCount,
                    Manager_LockoutEnabled = manager.Manager_LockoutEnabled,
                    Manager_LockoutEnd = manager.Manager_LockoutEnd,
                    Administrator_registration_date = manager.Administrator_registration_date,
                    Roles = manager.ManagerRoles.Select(mr => new RoleInfo
                    {
                        RoleId = mr.ManagerRolePermission.ManagerRole_Id,
                        RoleName = mr.ManagerRolePermission.role_name,
                        Permissions = GetRolePermissions(mr.ManagerRolePermission)
                    }).ToList()
                };

                LogOperation(""查看管理員詳情"", $""管理員ID: {id}"");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleError(ex, ""載入管理員詳情"");
            }
        }

        /// <summary>
        /// 新增管理員頁面
        /// </summary>
        public async Task<IActionResult> Create()
        {
            try
            {
                if (!await CheckPermissionAsync(""AdministratorPrivilegesManagement""))
                {
                    TempData[""ErrorMessage""] = ""您沒有權限訪問此功能"";
                    return RedirectToAction(""Index"");
                }

                var viewModel = new ManagerCreateViewModel();
                viewModel.RoleOptions = await _context.ManagerRolePermissions
                    .Select(rp => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = rp.ManagerRole_Id.ToString(),
                        Text = rp.role_name
                    })
                    .ToListAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleError(ex, ""載入新增管理員頁面"");
            }
        }

        /// <summary>
        /// 新增管理員處理
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ManagerCreateViewModel model)
        {
            try
            {
                if (!await CheckPermissionAsync(""AdministratorPrivilegesManagement""))
                {
                    TempData[""ErrorMessage""] = ""您沒有權限訪問此功能"";
                    return RedirectToAction(""Index"");
                }

                if (!ModelState.IsValid)
                {
                    model.RoleOptions = await _context.ManagerRolePermissions
                        .Select(rp => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Value = rp.ManagerRole_Id.ToString(),
                            Text = rp.role_name
                        })
                        .ToListAsync();
                    return View(model);
                }

                // 檢查帳號是否已存在
                if (await _context.ManagerData.AnyAsync(m => m.Manager_Account == model.Manager_Account))
                {
                    ModelState.AddModelError(""Manager_Account"", ""此帳號已存在"");
                    model.RoleOptions = await _context.ManagerRolePermissions
                        .Select(rp => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Value = rp.ManagerRole_Id.ToString(),
                            Text = rp.role_name
                        })
                        .ToListAsync();
                    return View(model);
                }

                // 檢查Email是否已存在
                if (await _context.ManagerData.AnyAsync(m => m.Manager_Email == model.Manager_Email))
                {
                    ModelState.AddModelError(""Manager_Email"", ""此電子郵件已存在"");
                    model.RoleOptions = await _context.ManagerRolePermissions
                        .Select(rp => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Value = rp.ManagerRole_Id.ToString(),
                            Text = rp.role_name
                        })
                        .ToListAsync();
                    return View(model);
                }

                // 創建管理員
                var manager = new ManagerData
                {
                    Manager_Name = model.Manager_Name,
                    Manager_Account = model.Manager_Account,
                    Manager_Password = HashPassword(model.Manager_Password),
                    Manager_Email = model.Manager_Email,
                    Manager_EmailConfirmed = false,
                    Manager_AccessFailedCount = 0,
                    Manager_LockoutEnabled = true,
                    Administrator_registration_date = DateTime.UtcNow
                };

                _context.ManagerData.Add(manager);
                await _context.SaveChangesAsync();

                // 分配角色
                if (model.SelectedRoleIds != null && model.SelectedRoleIds.Any())
                {
                    foreach (var roleId in model.SelectedRoleIds)
                    {
                        var managerRole = new ManagerRole
                        {
                            Manager_Id = manager.Manager_Id,
                            ManagerRole_Id = roleId
                        };
                        _context.ManagerRoles.Add(managerRole);
                    }
                    await _context.SaveChangesAsync();
                }

                LogOperation(""新增管理員"", $""管理員: {manager.Manager_Name} ({manager.Manager_Account})"");
                TempData[""SuccessMessage""] = ""管理員新增成功"";
                return RedirectToAction(""Index"");
            }
            catch (Exception ex)
            {
                return HandleError(ex, ""新增管理員"");
            }
        }

        /// <summary>
        /// 編輯管理員頁面
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                if (!await CheckPermissionAsync(""AdministratorPrivilegesManagement""))
                {
                    TempData[""ErrorMessage""] = ""您沒有權限訪問此功能"";
                    return RedirectToAction(""Index"");
                }

                var manager = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.Manager_Id == id);

                if (manager == null)
                {
                    TempData[""ErrorMessage""] = ""找不到指定的管理員"";
                    return RedirectToAction(""Index"");
                }

                var viewModel = new ManagerEditViewModel
                {
                    Manager_Id = manager.Manager_Id,
                    Manager_Name = manager.Manager_Name,
                    Manager_Account = manager.Manager_Account,
                    Manager_Email = manager.Manager_Email,
                    Manager_EmailConfirmed = manager.Manager_EmailConfirmed,
                    Manager_LockoutEnabled = manager.Manager_LockoutEnabled,
                    SelectedRoleIds = manager.ManagerRoles.Select(mr => mr.ManagerRole_Id).ToList()
                };

                viewModel.RoleOptions = await _context.ManagerRolePermissions
                    .Select(rp => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = rp.ManagerRole_Id.ToString(),
                        Text = rp.role_name
                    })
                    .ToListAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleError(ex, ""載入編輯管理員頁面"");
            }
        }

        /// <summary>
        /// 編輯管理員處理
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ManagerEditViewModel model)
        {
            try
            {
                if (!await CheckPermissionAsync(""AdministratorPrivilegesManagement""))
                {
                    TempData[""ErrorMessage""] = ""您沒有權限訪問此功能"";
                    return RedirectToAction(""Index"");
                }

                if (!ModelState.IsValid)
                {
                    model.RoleOptions = await _context.ManagerRolePermissions
                        .Select(rp => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Value = rp.ManagerRole_Id.ToString(),
                            Text = rp.role_name
                        })
                        .ToListAsync();
                    return View(model);
                }

                var manager = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.Manager_Id == model.Manager_Id);

                if (manager == null)
                {
                    TempData[""ErrorMessage""] = ""找不到指定的管理員"";
                    return RedirectToAction(""Index"");
                }

                // 檢查帳號是否已被其他管理員使用
                if (await _context.ManagerData.AnyAsync(m => m.Manager_Account == model.Manager_Account && m.Manager_Id != model.Manager_Id))
                {
                    ModelState.AddModelError(""Manager_Account"", ""此帳號已被其他管理員使用"");
                    model.RoleOptions = await _context.ManagerRolePermissions
                        .Select(rp => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Value = rp.ManagerRole_Id.ToString(),
                            Text = rp.role_name
                        })
                        .ToListAsync();
                    return View(model);
                }

                // 檢查Email是否已被其他管理員使用
                if (await _context.ManagerData.AnyAsync(m => m.Manager_Email == model.Manager_Email && m.Manager_Id != model.Manager_Id))
                {
                    ModelState.AddModelError(""Manager_Email"", ""此電子郵件已被其他管理員使用"");
                    model.RoleOptions = await _context.ManagerRolePermissions
                        .Select(rp => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Value = rp.ManagerRole_Id.ToString(),
                            Text = rp.role_name
                        })
                        .ToListAsync();
                    return View(model);
                }

                // 更新管理員資料
                manager.Manager_Name = model.Manager_Name;
                manager.Manager_Account = model.Manager_Account;
                manager.Manager_Email = model.Manager_Email;
                manager.Manager_EmailConfirmed = model.Manager_EmailConfirmed;
                manager.Manager_LockoutEnabled = model.Manager_LockoutEnabled;

                // 更新密碼（如果提供）
                if (!string.IsNullOrEmpty(model.Manager_Password))
                {
                    manager.Manager_Password = HashPassword(model.Manager_Password);
                }

                // 更新角色
                var existingRoles = manager.ManagerRoles.ToList();
                _context.ManagerRoles.RemoveRange(existingRoles);

                if (model.SelectedRoleIds != null && model.SelectedRoleIds.Any())
                {
                    foreach (var roleId in model.SelectedRoleIds)
                    {
                        var managerRole = new ManagerRole
                        {
                            Manager_Id = manager.Manager_Id,
                            ManagerRole_Id = roleId
                        };
                        _context.ManagerRoles.Add(managerRole);
                    }
                }

                await _context.SaveChangesAsync();

                LogOperation(""編輯管理員"", $""管理員: {manager.Manager_Name} ({manager.Manager_Account})"");
                TempData[""SuccessMessage""] = ""管理員更新成功"";
                return RedirectToAction(""Index"");
            }
            catch (Exception ex)
            {
                return HandleError(ex, ""編輯管理員"");
            }
        }

        /// <summary>
        /// 刪除管理員確認頁面
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (!await CheckPermissionAsync(""AdministratorPrivilegesManagement""))
                {
                    TempData[""ErrorMessage""] = ""您沒有權限訪問此功能"";
                    return RedirectToAction(""Index"");
                }

                var manager = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .ThenInclude(mr => mr.ManagerRolePermission)
                    .FirstOrDefaultAsync(m => m.Manager_Id == id);

                if (manager == null)
                {
                    TempData[""ErrorMessage""] = ""找不到指定的管理員"";
                    return RedirectToAction(""Index"");
                }

                var viewModel = new ManagerDeleteViewModel
                {
                    Manager_Id = manager.Manager_Id,
                    Manager_Name = manager.Manager_Name,
                    Manager_Account = manager.Manager_Account,
                    Manager_Email = manager.Manager_Email,
                    Administrator_registration_date = manager.Administrator_registration_date,
                    Roles = manager.ManagerRoles.Select(mr => mr.ManagerRolePermission.role_name).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleError(ex, ""載入刪除管理員頁面"");
            }
        }

        /// <summary>
        /// 刪除管理員處理
        /// </summary>
        [HttpPost, ActionName(""Delete"")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (!await CheckPermissionAsync(""AdministratorPrivilegesManagement""))
                {
                    TempData[""ErrorMessage""] = ""您沒有權限訪問此功能"";
                    return RedirectToAction(""Index"");
                }

                var manager = await _context.ManagerData
                    .Include(m => m.ManagerRoles)
                    .FirstOrDefaultAsync(m => m.Manager_Id == id);

                if (manager == null)
                {
                    TempData[""ErrorMessage""] = ""找不到指定的管理員"";
                    return RedirectToAction(""Index"");
                }

                // 檢查是否為當前登入的管理員
                var currentManagerId = GetCurrentManagerId();
                if (currentManagerId == id)
                {
                    TempData[""ErrorMessage""] = ""無法刪除自己的帳號"";
                    return RedirectToAction(""Index"");
                }

                // 刪除相關的角色關聯
                _context.ManagerRoles.RemoveRange(manager.ManagerRoles);
                
                // 刪除管理員
                _context.ManagerData.Remove(manager);
                await _context.SaveChangesAsync();

                LogOperation(""刪除管理員"", $""管理員: {manager.Manager_Name} ({manager.Manager_Account})"");
                TempData[""SuccessMessage""] = ""管理員刪除成功"";
                return RedirectToAction(""Index"");
            }
            catch (Exception ex)
            {
                return HandleError(ex, ""刪除管理員"");
            }
        }

        /// <summary>
        /// 重置管理員密碼
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ResetPassword(int id, string newPassword)
        {
            try
            {
                if (!await CheckPermissionAsync(""AdministratorPrivilegesManagement""))
                {
                    return Json(new { success = false, message = ""您沒有權限執行此操作"" });
                }

                var manager = await _context.ManagerData.FindAsync(id);
                if (manager == null)
                {
                    return Json(new { success = false, message = ""找不到指定的管理員"" });
                }

                manager.Manager_Password = HashPassword(newPassword);
                manager.Manager_AccessFailedCount = 0;
                manager.Manager_LockoutEnd = null;

                await _context.SaveChangesAsync();

                LogOperation(""重置管理員密碼"", $""管理員: {manager.Manager_Name} ({manager.Manager_Account})"");
                return Json(new { success = true, message = ""密碼重置成功"" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ""重置管理員密碼時發生錯誤"");
                return Json(new { success = false, message = ""重置密碼時發生錯誤"" });
            }
        }

        /// <summary>
        /// 鎖定/解鎖管理員
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleLock(int id)
        {
            try
            {
                if (!await CheckPermissionAsync(""AdministratorPrivilegesManagement""))
                {
                    return Json(new { success = false, message = ""您沒有權限執行此操作"" });
                }

                var manager = await _context.ManagerData.FindAsync(id);
                if (manager == null)
                {
                    return Json(new { success = false, message = ""找不到指定的管理員"" });
                }

                // 檢查是否為當前登入的管理員
                var currentManagerId = GetCurrentManagerId();
                if (currentManagerId == id)
                {
                    return Json(new { success = false, message = ""無法鎖定自己的帳號"" });
                }

                if (manager.Manager_LockoutEnabled && manager.Manager_LockoutEnd.HasValue && manager.Manager_LockoutEnd > DateTime.Now)
                {
                    // 解鎖
                    manager.Manager_LockoutEnd = null;
                    manager.Manager_AccessFailedCount = 0;
                    LogOperation(""解鎖管理員"", $""管理員: {manager.Manager_Name} ({manager.Manager_Account})"");
                }
                else
                {
                    // 鎖定
                    manager.Manager_LockoutEnd = DateTime.Now.AddDays(30); // 鎖定30天
                    LogOperation(""鎖定管理員"", $""管理員: {manager.Manager_Name} ({manager.Manager_Account})"");
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = ""操作成功"" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ""鎖定/解鎖管理員時發生錯誤"");
                return Json(new { success = false, message = ""操作時發生錯誤"" });
            }
        }

        /// <summary>
        /// 密碼雜湊
        /// </summary>
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// 獲取角色權限列表
        /// </summary>
        private List<string> GetRolePermissions(ManagerRolePermission role)
        {
            var permissions = new List<string>();
            
            if (role.AdministratorPrivilegesManagement) permissions.Add(""管理者平台管理"");
            if (role.UserStatusManagement) permissions.Add(""使用者狀態管理"");
            if (role.ShoppingPermissionManagement) permissions.Add(""購物權限管理"");
            if (role.MessagePermissionManagement) permissions.Add(""訊息權限管理"");
            if (role.Pet_Rights_Management) permissions.Add(""寵物權限管理"");
            if (role.customer_service) permissions.Add(""客服權限"");
            
            return permissions;
        }
    }
}
