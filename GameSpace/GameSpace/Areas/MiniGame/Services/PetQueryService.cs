using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物查詢服務實作
    /// </summary>
    public class PetQueryService : IPetQueryService
    {
        private readonly GameSpacedatabaseContext _context;

        public PetQueryService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得會員寵物清單（分頁）
        /// </summary>
        public async Task<PetAdminListPagedResult> GetPetListAsync(PetAdminListQueryModel query)
        {
            var dbQuery = _context.Pets
                .Include(p => p.User)
                .AsNoTracking()
                .AsQueryable();

            // === 修改：會員ID、搜尋關鍵字（用戶名OR寵物名稱）、額外寵物名稱條件採用聯集（OR）查詢 ===
            // 優先順序：會員ID > 搜尋關鍵字(用戶名/寵物名稱) > 額外寵物名稱條件
            var hasUserId = query.UserId.HasValue;
            var hasSearchTerm = !string.IsNullOrWhiteSpace(query.SearchTerm);
            var hasExtraPetName = !string.IsNullOrWhiteSpace(query.PetName);

            if (hasUserId || hasSearchTerm || hasExtraPetName)
            {
                // 使用 OR 查詢，並透過排序實現優先順序
                dbQuery = dbQuery.Where(p =>
                    (hasUserId && p.UserId == query.UserId.Value) ||
                    (hasSearchTerm && (p.User.UserName.Contains(query.SearchTerm.Trim()) || p.PetName.Contains(query.SearchTerm.Trim()))) ||
                    (hasExtraPetName && p.PetName.Contains(query.PetName.Trim()))
                );
            }


            // 篩選條件：膚色（修改為模糊查詢）
            if (!string.IsNullOrWhiteSpace(query.SkinColor))
            {
                dbQuery = dbQuery.Where(p => p.SkinColor != null && p.SkinColor.Contains(query.SkinColor.Trim()));
            }

            // 篩選條件：背景（修改為模糊查詢）
            if (!string.IsNullOrWhiteSpace(query.BackgroundColor))
            {
                dbQuery = dbQuery.Where(p => p.BackgroundColor != null && p.BackgroundColor.Contains(query.BackgroundColor.Trim()));
            }

            // === 排序：實現優先順序（會員ID > 搜尋關鍵字(用戶名/寵物名稱) > 額外寵物名稱條件）===
            // 支援新的組合型排序方式如 "level_asc", "level_desc" 等
            // 當有搜尋條件時，優先顯示符合優先順序高的結果
            if (hasUserId || hasSearchTerm || hasExtraPetName)
            {
                // 優先順序排序：先按符合條件的優先級排序
                IOrderedQueryable<Pet> priorityOrdered = dbQuery.OrderBy(p =>
                    hasUserId && p.UserId == query.UserId.Value ? 1 :
                    hasSearchTerm && (p.User.UserName.Contains(query.SearchTerm.Trim()) || p.PetName.Contains(query.SearchTerm.Trim())) ? 2 :
                    hasExtraPetName && p.PetName.Contains(query.PetName.Trim()) ? 3 : 4
                );

                // 接著按使用者指定的排序方式進行次要排序（支援組合型格式如 "level_asc"）
                dbQuery = (query.SortBy ?? "level_desc").ToLower() switch
                {
                    "level_asc" => priorityOrdered.ThenBy(p => p.Level),
                    "level_desc" => priorityOrdered.ThenByDescending(p => p.Level),
                    "exp_asc" => priorityOrdered.ThenBy(p => p.Experience),
                    "exp_desc" => priorityOrdered.ThenByDescending(p => p.Experience),
                    "health_asc" => priorityOrdered.ThenBy(p => p.Health),
                    "health_desc" => priorityOrdered.ThenByDescending(p => p.Health),
                    "petname_asc" => priorityOrdered.ThenBy(p => p.PetName),
                    "petname_desc" => priorityOrdered.ThenByDescending(p => p.PetName),
                    "userid_asc" => priorityOrdered.ThenBy(p => p.UserId),
                    "userid_desc" => priorityOrdered.ThenByDescending(p => p.UserId),
                    _ => priorityOrdered.ThenByDescending(p => p.Level)
                };
            }
            else
            {
                // 沒有搜尋條件時，使用一般排序（支援組合型格式如 "level_asc"）
                dbQuery = (query.SortBy ?? "level_desc").ToLower() switch
                {
                    "level_asc" => dbQuery.OrderBy(p => p.Level),
                    "level_desc" => dbQuery.OrderByDescending(p => p.Level),
                    "exp_asc" => dbQuery.OrderBy(p => p.Experience),
                    "exp_desc" => dbQuery.OrderByDescending(p => p.Experience),
                    "health_asc" => dbQuery.OrderBy(p => p.Health),
                    "health_desc" => dbQuery.OrderByDescending(p => p.Health),
                    "petname_asc" => dbQuery.OrderBy(p => p.PetName),
                    "petname_desc" => dbQuery.OrderByDescending(p => p.PetName),
                    "userid_asc" => dbQuery.OrderBy(p => p.UserId),
                    "userid_desc" => dbQuery.OrderByDescending(p => p.UserId),
                    _ => dbQuery.OrderByDescending(p => p.Level)
                };
            }

            // 計算總數
            var totalCount = await dbQuery.CountAsync();

            // 分頁
            var items = await dbQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new PetAdminListItemViewModel
                {
                    PetId = p.PetId,
                    UserId = p.UserId,
                    UserName = p.User.UserName,
                    PetName = p.PetName,
                    Level = p.Level,
                    Experience = p.Experience,
                    SkinColor = p.SkinColor,
                    BackgroundColor = p.BackgroundColor,
                    Health = p.Health,
                    Hunger = p.Hunger,
                    Mood = p.Mood,
                    Stamina = p.Stamina,
                    Cleanliness = p.Cleanliness
                })
                .ToListAsync();

            return new PetAdminListPagedResult
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
            };
        }

        /// <summary>
        /// 取得單一寵物詳細資料
        /// </summary>
        public async Task<PetAdminDetailViewModel?> GetPetDetailAsync(int petId)
        {
            var pet = await _context.Pets
                .Include(p => p.User)
                    .ThenInclude(u => u.UserIntroduce)
                .AsNoTracking()
                .Where(p => p.PetId == petId)
                .Select(p => new PetAdminDetailViewModel
                {
                    PetId = p.PetId,
                    UserId = p.UserId,
                    UserName = p.User.UserName,
                    UserEmail = p.User.UserIntroduce != null ? p.User.UserIntroduce.Email : string.Empty,
                    PetName = p.PetName,
                    Level = p.Level,
                    LevelUpTime = p.LevelUpTime,
                    Experience = p.Experience,
                    Health = p.Health,
                    Hunger = p.Hunger,
                    Mood = p.Mood,
                    Stamina = p.Stamina,
                    Cleanliness = p.Cleanliness,
                    SkinColor = p.SkinColor,
                    SkinColorChangedTime = p.SkinColorChangedTime,
                    BackgroundColor = p.BackgroundColor,
                    BackgroundColorChangedTime = p.BackgroundColorChangedTime,
                    PointsChangedSkinColor = p.PointsChangedSkinColor,
                    PointsChangedBackgroundColor = p.PointsChangedBackgroundColor,
                    PointsGainedLevelUp = p.PointsGainedLevelUp,
                    PointsGainedTimeLevelUp = p.PointsGainedTimeLevelUp
                })
                .FirstOrDefaultAsync();

            return pet;
        }

        /// <summary>
        /// 取得寵物膚色更換歷史記錄（分頁）
        /// </summary>
        public async Task<PetColorChangeHistoryPagedResult> GetColorChangeHistoryAsync(PetColorChangeHistoryQueryModel query)
        {
            var dbQuery = _context.WalletHistories
                .Include(wh => wh.User)
                .AsNoTracking()
                .Where(wh => wh.ChangeType == "PetSkinColorChange")
                .AsQueryable();

            // 查詢條件邏輯：
            // 1. 當兩個條件都為空時，顯示全部結果（不加篩選）
            // 2. 當兩個條件都有值時，優先使用會員ID（忽略寵物ID）
            // 3. 當只有一個條件有值時，使用該條件
            // 4. 所有查詢都支持模糊查詢
            bool hasUserId = query.UserId.HasValue;
            bool hasPetId = query.PetId.HasValue;

            if (hasUserId || hasPetId)
            {
                if (hasUserId && hasPetId)
                {
                    // 兩個都有值：優先會員ID（忽略寵物ID）
                    var userIdStr = query.UserId.Value.ToString();
                    dbQuery = dbQuery.Where(wh => wh.UserId.ToString().Contains(userIdStr));
                }
                else if (hasUserId)
                {
                    // 只有會員ID：模糊查詢
                    var userIdStr = query.UserId.Value.ToString();
                    dbQuery = dbQuery.Where(wh => wh.UserId.ToString().Contains(userIdStr));
                }
                else if (hasPetId)
                {
                    // 只有寵物ID：模糊查詢（從 ItemCode 中解析）
                    var petIdStr = query.PetId.Value.ToString();
                    dbQuery = dbQuery.Where(wh => wh.ItemCode != null && wh.ItemCode.Contains(petIdStr));
                }
            }
            // 如果兩個條件都為空，不添加任何篩選條件，顯示全部結果

            // 篩選條件：日期範圍
            if (query.StartDate.HasValue)
            {
                dbQuery = dbQuery.Where(wh => wh.ChangeTime >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                var endDate = query.EndDate.Value.Date.AddDays(1);
                dbQuery = dbQuery.Where(wh => wh.ChangeTime < endDate);
            }

            // 排序（預設為時間倒序）
            dbQuery = query.SortOrder.ToLower() == "asc"
                ? dbQuery.OrderBy(wh => wh.ChangeTime)
                : dbQuery.OrderByDescending(wh => wh.ChangeTime);

            // 計算總數
            var totalCount = await dbQuery.CountAsync();

            // 分頁並轉換為 ViewModel
            var items = await dbQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(wh => new ColorChangeHistoryItemViewModel
                {
                    LogId = wh.LogId,
                    UserId = wh.UserId,
                    UserName = wh.User.UserName,
                    PetId = ExtractPetIdFromItemCode(wh.ItemCode),
                    OldColor = ExtractOldColorFromDescription(wh.Description),
                    NewColor = ExtractNewColorFromDescription(wh.Description),
                    PointsCost = Math.Abs(wh.PointsChanged),
                    ChangedAt = wh.ChangeTime,
                    Description = wh.Description
                })
                .ToListAsync();

            // 補充寵物名稱
            var petIds = items.Where(i => i.PetId.HasValue).Select(i => i.PetId!.Value).Distinct().ToList();
            if (petIds.Any())
            {
                var petNames = await _context.Pets
                    .AsNoTracking()
                    .Where(p => petIds.Contains(p.PetId))
                    .ToDictionaryAsync(p => p.PetId, p => p.PetName);

                foreach (var item in items)
                {
                    if (item.PetId.HasValue && petNames.ContainsKey(item.PetId.Value))
                    {
                        item.PetName = petNames[item.PetId.Value];
                    }
                }
            }

            return new PetColorChangeHistoryPagedResult
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
            };
        }

        /// <summary>
        /// 取得寵物背景更換歷史記錄（分頁）
        /// </summary>
        public async Task<PetBackgroundChangeHistoryPagedResult> GetBackgroundChangeHistoryAsync(PetBackgroundChangeHistoryQueryModel query)
        {
            var dbQuery = _context.WalletHistories
                .Include(wh => wh.User)
                .AsNoTracking()
                .Where(wh => wh.ChangeType == "PetBackgroundChange")
                .AsQueryable();

            // 查詢條件邏輯：
            // 1. 當兩個條件都為空時，顯示全部結果（不加篩選）
            // 2. 當兩個條件都有值時，優先使用會員ID（忽略寵物ID）
            // 3. 當只有一個條件有值時，使用該條件
            // 4. 所有查詢都支持模糊查詢
            bool hasUserId = query.UserId.HasValue;
            bool hasPetId = query.PetId.HasValue;

            if (hasUserId || hasPetId)
            {
                if (hasUserId && hasPetId)
                {
                    // 兩個都有值：優先會員ID（忽略寵物ID）
                    var userIdStr = query.UserId.Value.ToString();
                    dbQuery = dbQuery.Where(wh => wh.UserId.ToString().Contains(userIdStr));
                }
                else if (hasUserId)
                {
                    // 只有會員ID：模糊查詢
                    var userIdStr = query.UserId.Value.ToString();
                    dbQuery = dbQuery.Where(wh => wh.UserId.ToString().Contains(userIdStr));
                }
                else if (hasPetId)
                {
                    // 只有寵物ID：模糊查詢（從 ItemCode 中解析）
                    var petIdStr = query.PetId.Value.ToString();
                    dbQuery = dbQuery.Where(wh => wh.ItemCode != null && wh.ItemCode.Contains(petIdStr));
                }
            }
            // 如果兩個條件都為空，不添加任何篩選條件，顯示全部結果

            // 篩選條件：日期範圍
            if (query.StartDate.HasValue)
            {
                dbQuery = dbQuery.Where(wh => wh.ChangeTime >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                var endDate = query.EndDate.Value.Date.AddDays(1);
                dbQuery = dbQuery.Where(wh => wh.ChangeTime < endDate);
            }

            // 排序（預設為時間倒序）
            dbQuery = query.SortOrder.ToLower() == "asc"
                ? dbQuery.OrderBy(wh => wh.ChangeTime)
                : dbQuery.OrderByDescending(wh => wh.ChangeTime);

            // 計算總數
            var totalCount = await dbQuery.CountAsync();

            // 分頁並轉換為 ViewModel
            var items = await dbQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(wh => new BackgroundChangeHistoryItemViewModel
                {
                    LogId = wh.LogId,
                    UserId = wh.UserId,
                    UserName = wh.User.UserName,
                    PetId = ExtractPetIdFromItemCode(wh.ItemCode),
                    OldBackground = ExtractOldBackgroundFromDescription(wh.Description),
                    NewBackground = ExtractNewBackgroundFromDescription(wh.Description),
                    PointsCost = Math.Abs(wh.PointsChanged),
                    ChangedAt = wh.ChangeTime,
                    Description = wh.Description
                })
                .ToListAsync();

            // 補充寵物名稱
            var petIds = items.Where(i => i.PetId.HasValue).Select(i => i.PetId!.Value).Distinct().ToList();
            if (petIds.Any())
            {
                var petNames = await _context.Pets
                    .AsNoTracking()
                    .Where(p => petIds.Contains(p.PetId))
                    .ToDictionaryAsync(p => p.PetId, p => p.PetName);

                foreach (var item in items)
                {
                    if (item.PetId.HasValue && petNames.ContainsKey(item.PetId.Value))
                    {
                        item.PetName = petNames[item.PetId.Value];
                    }
                }
            }

            return new PetBackgroundChangeHistoryPagedResult
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
            };
        }

        #region 輔助方法

        /// <summary>
        /// 從 ItemCode 解析寵物 ID
        /// 格式範例："Pet:123" 或 "Pet:123:Color"
        /// </summary>
        private static int? ExtractPetIdFromItemCode(string? itemCode)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
                return null;

            var parts = itemCode.Split(':');
            if (parts.Length >= 2 && parts[0] == "Pet" && int.TryParse(parts[1], out int petId))
            {
                return petId;
            }

            return null;
        }

        /// <summary>
        /// 從 Description 解析舊膚色
        /// 格式範例："膚色更換：#FFFFFF → #000000"
        /// </summary>
        private static string ExtractOldColorFromDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return string.Empty;

            var parts = description.Split('→');
            if (parts.Length >= 2)
            {
                var oldPart = parts[0].Trim();
                var colonIndex = oldPart.IndexOf('：');
                if (colonIndex >= 0)
                {
                    return oldPart.Substring(colonIndex + 1).Trim();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 從 Description 解析新膚色
        /// 格式範例："膚色更換：#FFFFFF → #000000"
        /// </summary>
        private static string ExtractNewColorFromDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return string.Empty;

            var parts = description.Split('→');
            if (parts.Length >= 2)
            {
                return parts[1].Trim();
            }

            return string.Empty;
        }

        /// <summary>
        /// 從 Description 解析舊背景
        /// 格式範例："背景更換：Forest → Ocean"
        /// </summary>
        private static string ExtractOldBackgroundFromDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return string.Empty;

            var parts = description.Split('→');
            if (parts.Length >= 2)
            {
                var oldPart = parts[0].Trim();
                var colonIndex = oldPart.IndexOf('：');
                if (colonIndex >= 0)
                {
                    return oldPart.Substring(colonIndex + 1).Trim();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 從 Description 解析新背景
        /// 格式範例："背景更換：Forest → Ocean"
        /// </summary>
        private static string ExtractNewBackgroundFromDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return string.Empty;

            var parts = description.Split('→');
            if (parts.Length >= 2)
            {
                return parts[1].Trim();
            }

            return string.Empty;
        }

        #endregion
    }
}
