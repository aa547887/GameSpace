using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using System.Security.Claims;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public abstract class MiniGameBaseController : Controller
    {
        protected readonly GameSpacedatabaseContext _context;
        protected readonly IMiniGameAdminService _adminService;

        protected MiniGameBaseController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        protected MiniGameBaseController(GameSpacedatabaseContext context, IMiniGameAdminService adminService) : this(context)
        {
            _adminService = adminService;
        }

        // 獲取當前管理員ID
        protected int? GetCurrentManagerId()
        {
            var managerIdClaim = User.FindFirst("ManagerId");
            if (managerIdClaim != null && int.TryParse(managerIdClaim.Value, out int managerId))
            {
                return managerId;
            }
            return null;
        }

        // 獲取當前管理員信息
        protected async Task<Manager> GetCurrentManagerAsync()
        {
            var managerId = GetCurrentManagerId();
            if (managerId.HasValue)
            {
                return await _context.Managers.FindAsync(managerId.Value);
            }
            return null;
        }

        // 檢查管理員權限
        protected async Task<bool> HasPermissionAsync(string permission)
        {
            var manager = await GetCurrentManagerAsync();
            if (manager == null) return false;

            return manager.Role switch
            {
                "SuperAdmin" => true,
                "Admin" => permission switch
                {
                    "MiniGame.View" => true,
                    "MiniGame.Edit" => true,
                    "MiniGame.Delete" => true,
                    "User.View" => true,
                    "User.Edit" => true,
                    "Wallet.View" => true,
                    "Wallet.Edit" => true,
                    "Pet.View" => true,
                    "Pet.Edit" => true,
                    "Game.View" => true,
                    "Game.Edit" => true,
                    "Coupon.View" => true,
                    "Coupon.Edit" => true,
                    "EVoucher.View" => true,
                    "EVoucher.Edit" => true,
                    _ => false
                },
                "Manager" => permission switch
                {
                    "MiniGame.View" => true,
                    "User.View" => true,
                    "Wallet.View" => true,
                    "Pet.View" => true,
                    "Game.View" => true,
                    "Coupon.View" => true,
                    "EVoucher.View" => true,
                    _ => false
                },
                _ => false
            };
        }

        // 記錄操作日誌
        protected async Task LogOperationAsync(string operation, string details, int? targetUserId = null)
        {
            try
            {
                var managerId = GetCurrentManagerId();
                var log = new AdminOperationLog
                {
                    ManagerId = managerId,
                    Operation = operation,
                    Details = details,
                    TargetUserId = targetUserId,
                    OperationTime = DateTime.Now,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                _context.AdminOperationLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // 記錄日誌失敗不影響主要功能
            }
        }

        // 驗證用戶權限
        protected async Task<bool> ValidateUserPermissionAsync(int userId, string permission)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                // 檢查用戶狀態
                if (!user.IsActive) return false;

                // 檢查管理員權限
                var hasPermission = await HasPermissionAsync(permission);
                if (!hasPermission) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        // 獲取分頁結果
        protected PagedResult<T> CreatePagedResult<T>(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // 計算分頁信息
        protected (int totalPages, int startPage, int endPage) CalculatePagination(int totalCount, int pageNumber, int pageSize)
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var startPage = Math.Max(1, pageNumber - 2);
            var endPage = Math.Min(totalPages, pageNumber + 2);
            
            return (totalPages, startPage, endPage);
        }

        // 生成隨機代碼
        protected string GenerateRandomCode(string prefix, int length = 8)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var code = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return $"{prefix}{code}";
        }

        // 驗證日期範圍
        protected bool IsValidDateRange(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue || !endDate.HasValue) return true;
            return startDate.Value <= endDate.Value;
        }

        // 格式化日期顯示
        protected string FormatDate(DateTime? date)
        {
            return date?.ToString("yyyy-MM-dd HH:mm:ss") ?? "未設定";
        }

        // 檢查字符串是否為空或空白
        protected bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        // 安全地轉換字符串為整數
        protected int? SafeParseInt(string value)
        {
            if (int.TryParse(value, out int result))
                return result;
            return null;
        }

        // 安全地轉換字符串為布爾值
        protected bool? SafeParseBool(string value)
        {
            if (bool.TryParse(value, out bool result))
                return result;
            return null;
        }

        // 獲取客戶端IP地址
        protected string GetClientIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress))
            {
                var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    ipAddress = forwardedFor.Split(',')[0].Trim();
                }
            }
            return ipAddress ?? "Unknown";
        }

        // 驗證文件上傳
        protected bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;
            
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            return allowedExtensions.Contains(extension);
        }

        // 獲取文件大小描述
        protected string GetFileSizeDescription(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        // 驗證電子郵件格式
        protected bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // 驗證手機號碼格式
        protected bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return false;
            
            // 簡單的手機號碼驗證（台灣格式）
            var cleanNumber = phoneNumber.Replace("-", "").Replace(" ", "");
            return System.Text.RegularExpressions.Regex.IsMatch(cleanNumber, @"^09\d{8}$");
        }

        // 生成QR碼內容
        protected string GenerateQRCodeContent(string type, string code)
        {
            return $"{type}:{code}:{DateTime.Now:yyyyMMddHHmmss}";
        }

        // 驗證QR碼內容
        protected (bool isValid, string type, string code, DateTime? timestamp) ValidateQRCodeContent(string content)
        {
            try
            {
                var parts = content.Split(':');
                if (parts.Length != 3) return (false, null, null, null);
                
                var type = parts[0];
                var code = parts[1];
                var timestampStr = parts[2];
                
                if (DateTime.TryParseExact(timestampStr, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime timestamp))
                {
                    return (true, type, code, timestamp);
                }
                
                return (false, null, null, null);
            }
            catch
            {
                return (false, null, null, null);
            }
        }

        // 檢查操作是否在允許的時間範圍內
        protected bool IsOperationAllowed(DateTime? operationTime, int maxHours = 24)
        {
            if (!operationTime.HasValue) return false;
            return DateTime.Now.Subtract(operationTime.Value).TotalHours <= maxHours;
        }

        // 獲取系統設定值
        protected async Task<string> GetSystemSettingAsync(string key, string defaultValue = "")
        {
            try
            {
                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.Key == key);
                return setting?.Value ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        // 設定系統設定值
        protected async Task<bool> SetSystemSettingAsync(string key, string value)
        {
            try
            {
                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.Key == key);
                
                if (setting == null)
                {
                    setting = new SystemSetting
                    {
                        Key = key,
                        Value = value,
                        CreatedTime = DateTime.Now
                    };
                    _context.SystemSettings.Add(setting);
                }
                else
                {
                    setting.Value = value;
                    setting.UpdatedTime = DateTime.Now;
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    // 分頁結果類
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
