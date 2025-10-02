using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 電子禮券服務介面
    /// </summary>
    public interface IEVoucherService
    {
        /// <summary>
        /// 取得所有電子禮券
        /// </summary>
        /// <returns>電子禮券清單</returns>
        Task<IEnumerable<EVoucher>> GetAllEVouchersAsync();

        /// <summary>
        /// 根據ID取得電子禮券
        /// </summary>
        /// <param name="eVoucherId">電子禮券ID</param>
        /// <returns>電子禮券資料</returns>
        Task<EVoucher?> GetEVoucherByIdAsync(int eVoucherId);

        /// <summary>
        /// 根據使用者ID取得電子禮券清單
        /// </summary>
        /// <param name="userId">使用者ID</param>
        /// <returns>使用者的電子禮券清單</returns>
        Task<IEnumerable<EVoucher>> GetEVouchersByUserIdAsync(int userId);

        /// <summary>
        /// 根據電子禮券代碼取得電子禮券
        /// </summary>
        /// <param name="eVoucherCode">電子禮券代碼</param>
        /// <returns>電子禮券資料</returns>
        Task<EVoucher?> GetEVoucherByCodeAsync(string eVoucherCode);

        /// <summary>
        /// 建立新電子禮券
        /// </summary>
        /// <param name="eVoucher">電子禮券資料</param>
        /// <returns>建立結果</returns>
        Task<bool> CreateEVoucherAsync(EVoucher eVoucher);

        /// <summary>
        /// 更新電子禮券
        /// </summary>
        /// <param name="eVoucher">電子禮券資料</param>
        /// <returns>更新結果</returns>
        Task<bool> UpdateEVoucherAsync(EVoucher eVoucher);

        /// <summary>
        /// 使用電子禮券
        /// </summary>
        /// <param name="eVoucherId">電子禮券ID</param>
        /// <returns>使用結果</returns>
        Task<bool> UseEVoucherAsync(int eVoucherId);

        /// <summary>
        /// 發放電子禮券給使用者
        /// </summary>
        /// <param name="userId">使用者ID</param>
        /// <param name="eVoucherTypeId">電子禮券類型ID</param>
        /// <returns>發放結果</returns>
        Task<bool> GrantEVoucherToUserAsync(int userId, int eVoucherTypeId);

        /// <summary>
        /// 取得所有電子禮券類型
        /// </summary>
        /// <returns>電子禮券類型清單</returns>
        Task<IEnumerable<EVoucherType>> GetAllEVoucherTypesAsync();

        /// <summary>
        /// 根據ID取得電子禮券類型
        /// </summary>
        /// <param name="eVoucherTypeId">電子禮券類型ID</param>
        /// <returns>電子禮券類型資料</returns>
        Task<EVoucherType?> GetEVoucherTypeByIdAsync(int eVoucherTypeId);
    }
}
