using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IEVoucherTypeService
    {
        // EVoucherType 基本 CRUD
        Task<IEnumerable<EVoucherType>> GetAllEVoucherTypesAsync();
        Task<EVoucherType?> GetEVoucherTypeByIdAsync(int evoucherTypeId);
        Task<bool> CreateEVoucherTypeAsync(EVoucherType evoucherType);
        Task<bool> UpdateEVoucherTypeAsync(EVoucherType evoucherType);
        Task<bool> DeleteEVoucherTypeAsync(int evoucherTypeId);

        // EVoucherType 狀態管理
        Task<bool> ActivateEVoucherTypeAsync(int evoucherTypeId);
        Task<bool> DeactivateEVoucherTypeAsync(int evoucherTypeId);

        // EVoucherType 查詢
        Task<IEnumerable<EVoucherType>> GetActiveEVoucherTypesAsync();
        Task<IEnumerable<EVoucherType>> GetEVoucherTypesByValueRangeAsync(decimal minValue, decimal maxValue);
        Task<IEnumerable<EVoucherType>> GetAvailableEVoucherTypesAsync();

        // EVoucherType 庫存管理
        Task<bool> IncreaseStockAsync(int evoucherTypeId, int amount);
        Task<bool> DecreaseStockAsync(int evoucherTypeId, int amount);
        Task<int> GetRemainingStockAsync(int evoucherTypeId);
        Task<bool> IsStockAvailableAsync(int evoucherTypeId);

        // EVoucherType 統計
        Task<int> GetTotalEVoucherTypesCountAsync();
        Task<int> GetActiveEVoucherTypesCountAsync();
        Task<Dictionary<string, int>> GetEVoucherTypesDistributionAsync();
        Task<IEnumerable<EVoucherTypeUsageStats>> GetEVoucherTypeUsageStatsAsync();
    }

    public class EVoucherTypeUsageStats
    {
        public int EVoucherTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TotalIssued { get; set; }
        public int TotalUsed { get; set; }
        public int TotalUnused { get; set; }
        public int RemainingStock { get; set; }
        public decimal UsageRate { get; set; }
        public decimal TotalValue { get; set; }
    }
}
