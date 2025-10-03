using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GameSpace.Areas.MiniGame.Services
{
    public class UserWalletService : IUserWalletService
    {
        private readonly GameSpacedatabaseContext _context;

        public UserWalletService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<UserWallet>> GetUserWalletsAsync(WalletQueryModel query)
        {
            var queryable = _context.UserWallets.AsQueryable();

            // 使用者ID篩選
            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(w => w.UserId == query.UserId.Value);
            }

            // 點數範圍篩選（使用 MinAmount 和 MaxAmount，不是 MinPoints/MaxPoints）
            if (query.MinAmount.HasValue)
                queryable = queryable.Where(w => w.UserPoint >= query.MinAmount.Value);
            if (query.MaxAmount.HasValue)
                queryable = queryable.Where(w => w.UserPoint <= query.MaxAmount.Value);

            var totalCount = await queryable.CountAsync();

            // 排序
            if (query.Descending)
                queryable = queryable.OrderByDescending(w => w.UserPoint);
            else
                queryable = queryable.OrderBy(w => w.UserPoint);

            var items = await queryable
                .Include(w => w.User)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<UserWallet>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<UserWallet> GetUserWalletAsync(int userId)
        {
            return await _context.UserWallets
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<bool> UpdateUserPointsAsync(int userId, int points, string description)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var wallet = await _context.UserWallets.FindAsync(userId);
                if (wallet == null) return false;

                var oldPoints = wallet.UserPoint;
                wallet.UserPoint += points;

                // 記錄交易歷史
                var history = new WalletHistory
                {
                    UserId = userId,
                    PointsChanged = points,
                    ChangeType = "Point",
                    ChangeTime = DateTime.Now,
                    Description = description,
                    ItemCode = null
                };

                _context.WalletHistories.Add(history);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> IssueCouponAsync(int userId, int couponTypeId, string description)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var couponType = await _context.CouponTypes.FindAsync(couponTypeId);
                if (couponType == null) return false;

                var coupon = new Coupon
                {
                    CouponCode = GenerateCouponCode(),
                    CouponTypeId = couponTypeId,
                    UserId = userId,
                    IsUsed = false,
                    AcquiredTime = DateTime.Now,
                    UsedTime = null,
                    UsedInOrderId = null
                };

                _context.Coupons.Add(coupon);

                // 記錄交易歷史
                var history = new WalletHistory
                {
                    UserId = userId,
                    PointsChanged = 1,
                    ChangeType = "Coupon",
                    ChangeTime = DateTime.Now,
                    Description = description,
                    ItemCode = coupon.CouponId
                };

                _context.WalletHistories.Add(history);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> IssueEVoucherAsync(int userId, int evoucherTypeId, string description)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var evoucherType = await _context.EVoucherTypes.FindAsync(evoucherTypeId);
                if (evoucherType == null) return false;

                var evoucher = new EVoucher
                {
                    EVoucherCode = GenerateEVoucherCode(evoucherType.Name),
                    EVoucherTypeId = evoucherTypeId,
                    UserId = userId,
                    IsUsed = false,
                    AcquiredTime = DateTime.Now,
                    UsedTime = null
                };

                _context.EVouchers.Add(evoucher);

                // 記錄交易歷史
                var history = new WalletHistory
                {
                    UserId = userId,
                    PointsChanged = 1,
                    ChangeType = "EVoucher",
                    ChangeTime = DateTime.Now,
                    Description = description,
                    ItemCode = evoucher.EVoucherId
                };

                _context.WalletHistories.Add(history);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<PagedResult<WalletHistory>> GetWalletHistoryAsync(WalletHistoryQueryModel query)
        {
            try
            {
                var queryable = _context.WalletHistories.AsQueryable();

                // 用戶篩選
                if (query.UserId.HasValue)
                    queryable = queryable.Where(h => h.UserId == query.UserId.Value);

                // 交易類型篩選
                if (!string.IsNullOrEmpty(query.ChangeType))
                    queryable = queryable.Where(h => h.ChangeType == query.ChangeType);

                // 日期範圍篩選
                if (query.StartDate.HasValue)
                    queryable = queryable.Where(h => h.ChangeTime >= query.StartDate.Value);
                if (query.EndDate.HasValue)
                    queryable = queryable.Where(h => h.ChangeTime <= query.EndDate.Value);

                var totalCount = await queryable.CountAsync();

                // 排序
                if (query.Descending)
                    queryable = queryable.OrderByDescending(h => h.ChangeTime);
                else
                    queryable = queryable.OrderBy(h => h.ChangeTime);

                var items = await queryable
                    .Include(h => h.User)
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                return new PagedResult<WalletHistory>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };
            }
            catch (Exception)
            {
                return new PagedResult<WalletHistory>
                {
                    Items = new List<WalletHistory>(),
                    TotalCount = 0,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };
            }
        }

        public async Task<bool> ExportWalletHistoryAsync(WalletHistoryQueryModel query, string filePath)
        {
            try
            {
                // 取得所有符合條件的資料（不分頁）
                var queryable = _context.WalletHistories.AsQueryable();

                // 用戶篩選
                if (query.UserId.HasValue)
                    queryable = queryable.Where(h => h.UserId == query.UserId.Value);

                // 交易類型篩選
                if (!string.IsNullOrEmpty(query.ChangeType))
                    queryable = queryable.Where(h => h.ChangeType == query.ChangeType);

                // 日期範圍篩選
                if (query.StartDate.HasValue)
                    queryable = queryable.Where(h => h.ChangeTime >= query.StartDate.Value);
                if (query.EndDate.HasValue)
                    queryable = queryable.Where(h => h.ChangeTime <= query.EndDate.Value);

                // 排序
                if (query.Descending)
                    queryable = queryable.OrderByDescending(h => h.ChangeTime);
                else
                    queryable = queryable.OrderBy(h => h.ChangeTime);

                var allData = await queryable
                    .Include(h => h.User)
                    .ToListAsync();

                // 根據副檔名決定匯出格式
                var extension = Path.GetExtension(filePath).ToLowerInvariant();

                if (extension == ".csv")
                {
                    await ExportToCsvAsync(allData, filePath);
                }
                else if (extension == ".txt")
                {
                    await ExportToTextAsync(allData, filePath);
                }
                else
                {
                    // 預設使用 CSV 格式
                    await ExportToCsvAsync(allData, filePath);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task ExportToCsvAsync(List<WalletHistory> data, string filePath)
        {
            var sb = new StringBuilder();

            // CSV 標題行（繁體中文）
            sb.AppendLine("歷史ID,用戶ID,用戶名稱,變更類型,變更金額,點數變更,變更時間,描述,相關ID,項目代碼");

            foreach (var record in data)
            {
                var userName = record.User?.UserName ?? "未知用戶";
                var changeAmount = record.PointsChanged?.ToString() ?? "0";
                var pointsChanged = record.PointsChanged?.ToString() ?? "0";
                var relatedId = record.ItemCode?.ToString() ?? "";
                var itemCode = record.ItemCode ?? "";
                var description = EscapeCsvField(record.Description ?? "");

                sb.AppendLine($"{record.LogId},{record.UserId},{EscapeCsvField(userName)},{record.ChangeType},{changeAmount},{pointsChanged},{record.ChangeTime:yyyy-MM-dd HH:mm:ss},{description},{relatedId},{itemCode}");
            }

            // 使用 UTF-8 with BOM 以支援 Excel 正確顯示中文
            var encoding = new UTF8Encoding(true);
            await File.WriteAllTextAsync(filePath, sb.ToString(), encoding);
        }

        private async Task ExportToTextAsync(List<WalletHistory> data, string filePath)
        {
            var sb = new StringBuilder();

            sb.AppendLine("========================================");
            sb.AppendLine("錢包歷史記錄匯出");
            sb.AppendLine($"匯出時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"總記錄數: {data.Count}");
            sb.AppendLine("========================================");
            sb.AppendLine();

            foreach (var record in data)
            {
                var userName = record.User?.UserName ?? "未知用戶";

                sb.AppendLine($"歷史ID: {record.LogId}");
                sb.AppendLine($"用戶: {userName} (ID: {record.UserId})");
                sb.AppendLine($"變更類型: {record.ChangeType}");
                sb.AppendLine($"變更金額: {record.PointsChanged ?? 0}");
                sb.AppendLine($"點數變更: {record.PointsChanged ?? 0}");
                sb.AppendLine($"變更時間: {record.ChangeTime:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"描述: {record.Description ?? "無"}");

                if (!string.IsNullOrEmpty(record.ItemCode))
                    sb.AppendLine($"相關ID: {record.ItemCode}");

                if (!string.IsNullOrEmpty(record.ItemCode))
                    sb.AppendLine($"項目代碼: {record.ItemCode}");

                sb.AppendLine("----------------------------------------");
                sb.AppendLine();
            }

            var encoding = new UTF8Encoding(true);
            await File.WriteAllTextAsync(filePath, sb.ToString(), encoding);
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            // 如果欄位包含逗號、雙引號或換行符號，需要用雙引號包圍並轉義雙引號
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }

        private string GenerateCouponCode()
        {
            var now = DateTime.Now;
            var random = new Random();
            var randomCode = random.Next(100000, 999999);
            return $"CPN-{now:yyyyMM}-{randomCode}";
        }

        private string GenerateEVoucherCode(string typeName)
        {
            var typeCode = GetTypeCode(typeName);
            var random = new Random();
            var randomCode = random.Next(1000, 9999);
            var numberCode = random.Next(100000, 999999);
            return $"EV-{typeCode}-{randomCode}-{numberCode}";
        }

        private string GetTypeCode(string typeName)
        {
            return typeName.ToUpper() switch
            {
                "現金券" => "CASH",
                "電影券" => "MOVIE",
                "美食券" => "FOOD",
                "加油券" => "GAS",
                "商店券" => "STORE",
                "咖啡券" => "COFFEE",
                _ => "CASH"
            };
        }
    }
}


