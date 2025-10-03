using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class EVoucherTypeService : IEVoucherTypeService
    {
        private readonly GameSpacedatabaseContext _context;

        public EVoucherTypeService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // EvoucherType 基本 CRUD
        public async Task<IEnumerable<EvoucherType>> GetAllEVoucherTypesAsync()
        {
            return await _context.EvoucherTypes
                .OrderByDescending(evt => evt.CreatedAt)
                .ToListAsync();
        }

        public async Task<EvoucherType?> GetEVoucherTypeByIdAsync(int evoucherTypeId)
        {
            return await _context.EvoucherTypes
                .FirstOrDefaultAsync(evt => evt.EVoucherTypeID == evoucherTypeId);
        }

        public async Task<bool> CreateEVoucherTypeAsync(EvoucherType evoucherType)
        {
            try
            {
                evoucherType.CreatedAt = DateTime.UtcNow;
                evoucherType.IsActive = true;
                _context.EvoucherTypes.Add(evoucherType);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateEVoucherTypeAsync(EvoucherType evoucherType)
        {
            try
            {
                evoucherType.UpdatedAt = DateTime.UtcNow;
                _context.EvoucherTypes.Update(evoucherType);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteEVoucherTypeAsync(int evoucherTypeId)
        {
            try
            {
                var evoucherType = await GetEVoucherTypeByIdAsync(evoucherTypeId);
                if (evoucherType == null) return false;

                _context.EvoucherTypes.Remove(evoucherType);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // EvoucherType 狀態管理
        public async Task<bool> ActivateEVoucherTypeAsync(int evoucherTypeId)
        {
            try
            {
                var evoucherType = await GetEVoucherTypeByIdAsync(evoucherTypeId);
                if (evoucherType == null) return false;

                evoucherType.IsActive = true;
                evoucherType.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeactivateEVoucherTypeAsync(int evoucherTypeId)
        {
            try
            {
                var evoucherType = await GetEVoucherTypeByIdAsync(evoucherTypeId);
                if (evoucherType == null) return false;

                evoucherType.IsActive = false;
                evoucherType.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // EvoucherType 查詢
        public async Task<IEnumerable<EvoucherType>> GetActiveEVoucherTypesAsync()
        {
            return await _context.EvoucherTypes
                .Where(evt => evt.IsActive)
                .OrderBy(evt => evt.EVoucherTypeName)
                .ToListAsync();
        }

        public async Task<IEnumerable<EvoucherType>> GetEVoucherTypesByValueRangeAsync(decimal minValue, decimal maxValue)
        {
            return await _context.EvoucherTypes
                .Where(evt => evt.Value >= minValue && evt.Value <= maxValue)
                .OrderBy(evt => evt.Value)
                .ToListAsync();
        }

        public async Task<IEnumerable<EvoucherType>> GetAvailableEVoucherTypesAsync()
        {
            return await _context.EvoucherTypes
                .Where(evt => evt.IsActive && evt.Stock > 0)
                .OrderBy(evt => evt.Value)
                .ToListAsync();
        }

        // EvoucherType 庫存管理
        public async Task<bool> IncreaseStockAsync(int evoucherTypeId, int amount)
        {
            try
            {
                var evoucherType = await GetEVoucherTypeByIdAsync(evoucherTypeId);
                if (evoucherType == null) return false;

                evoucherType.Stock += amount;
                evoucherType.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DecreaseStockAsync(int evoucherTypeId, int amount)
        {
            try
            {
                var evoucherType = await GetEVoucherTypeByIdAsync(evoucherTypeId);
                if (evoucherType == null || evoucherType.Stock < amount) return false;

                evoucherType.Stock -= amount;
                evoucherType.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetRemainingStockAsync(int evoucherTypeId)
        {
            var evoucherType = await GetEVoucherTypeByIdAsync(evoucherTypeId);
            return evoucherType?.Stock ?? 0;
        }

        public async Task<bool> IsStockAvailableAsync(int evoucherTypeId)
        {
            var evoucherType = await GetEVoucherTypeByIdAsync(evoucherTypeId);
            return evoucherType != null && evoucherType.IsActive && evoucherType.Stock > 0;
        }

        // EvoucherType 統計
        public async Task<int> GetTotalEVoucherTypesCountAsync()
        {
            return await _context.EvoucherTypes.CountAsync();
        }

        public async Task<int> GetActiveEVoucherTypesCountAsync()
        {
            return await _context.EvoucherTypes.CountAsync(evt => evt.IsActive);
        }

        public async Task<Dictionary<string, int>> GetEVoucherTypesDistributionAsync()
        {
            var types = await _context.EvoucherTypes.ToListAsync();
            var distribution = new Dictionary<string, int>();

            // 按價值範圍分組
            distribution["0-100"] = types.Count(t => t.Value < 100);
            distribution["100-500"] = types.Count(t => t.Value >= 100 && t.Value < 500);
            distribution["500-1000"] = types.Count(t => t.Value >= 500 && t.Value < 1000);
            distribution["1000+"] = types.Count(t => t.Value >= 1000);

            return distribution;
        }

        public async Task<IEnumerable<EVoucherTypeUsageStats>> GetEVoucherTypeUsageStatsAsync()
        {
            var evoucherTypes = await _context.EvoucherTypes.ToListAsync();
            var stats = new List<EVoucherTypeUsageStats>();

            foreach (var evoucherType in evoucherTypes)
            {
                var evouchers = await _context.Evouchers
                    .Where(e => e.EVoucherTypeID == evoucherType.EVoucherTypeID)
                    .ToListAsync();

                var totalIssued = evouchers.Count;
                var totalUsed = evouchers.Count(e => e.IsUsed);
                var totalUnused = evouchers.Count(e => !e.IsUsed);

                stats.Add(new EVoucherTypeUsageStats
                {
                    EVoucherTypeId = evoucherType.EVoucherTypeID,
                    Name = evoucherType.EVoucherTypeName,
                    TotalIssued = totalIssued,
                    TotalUsed = totalUsed,
                    TotalUnused = totalUnused,
                    RemainingStock = evoucherType.Stock,
                    UsageRate = totalIssued > 0 ? (decimal)totalUsed / totalIssued * 100 : 0,
                    TotalValue = evoucherType.Value * totalIssued
                });
            }

            return stats.OrderByDescending(s => s.TotalValue);
        }
    }
}

