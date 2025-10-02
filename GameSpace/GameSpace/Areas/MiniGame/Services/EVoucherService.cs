using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Data;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 電子禮券服務實作
    /// </summary>
    public class EVoucherService : IEVoucherService
    {
        private readonly MiniGameDbContext _context;

        public EVoucherService(MiniGameDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得所有電子禮券
        /// </summary>
        public async Task<IEnumerable<EVoucher>> GetAllEVouchersAsync()
        {
            return await _context.EVouchers
                .Include(ev => ev.EVoucherType)
                .OrderByDescending(ev => ev.AcquiredTime)
                .ToListAsync();
        }

        /// <summary>
        /// 根據ID取得電子禮券
        /// </summary>
        public async Task<EVoucher?> GetEVoucherByIdAsync(int eVoucherId)
        {
            return await _context.EVouchers
                .Include(ev => ev.EVoucherType)
                .FirstOrDefaultAsync(ev => ev.EVoucherID == eVoucherId);
        }

        /// <summary>
        /// 根據使用者ID取得電子禮券清單
        /// </summary>
        public async Task<IEnumerable<EVoucher>> GetEVouchersByUserIdAsync(int userId)
        {
            return await _context.EVouchers
                .Include(ev => ev.EVoucherType)
                .Where(ev => ev.UserID == userId)
                .OrderByDescending(ev => ev.AcquiredTime)
                .ToListAsync();
        }

        /// <summary>
        /// 根據電子禮券代碼取得電子禮券
        /// </summary>
        public async Task<EVoucher?> GetEVoucherByCodeAsync(string eVoucherCode)
        {
            return await _context.EVouchers
                .Include(ev => ev.EVoucherType)
                .FirstOrDefaultAsync(ev => ev.EVoucherCode == eVoucherCode);
        }

        /// <summary>
        /// 建立新電子禮券
        /// </summary>
        public async Task<bool> CreateEVoucherAsync(EVoucher eVoucher)
        {
            try
            {
                _context.EVouchers.Add(eVoucher);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 更新電子禮券
        /// </summary>
        public async Task<bool> UpdateEVoucherAsync(EVoucher eVoucher)
        {
            try
            {
                _context.EVouchers.Update(eVoucher);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 使用電子禮券
        /// </summary>
        public async Task<bool> UseEVoucherAsync(int eVoucherId)
        {
            try
            {
                var eVoucher = await _context.EVouchers.FindAsync(eVoucherId);
                if (eVoucher == null || eVoucher.IsUsed)
                    return false;

                eVoucher.IsUsed = true;
                eVoucher.UsedTime = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 發放電子禮券給使用者
        /// </summary>
        public async Task<bool> GrantEVoucherToUserAsync(int userId, int eVoucherTypeId)
        {
            try
            {
                var eVoucherType = await _context.EVoucherTypes.FindAsync(eVoucherTypeId);
                if (eVoucherType == null)
                    return false;

                var eVoucher = new EVoucher
                {
                    EVoucherCode = GenerateEVoucherCode(eVoucherType.Name),
                    EVoucherTypeID = eVoucherTypeId,
                    UserID = userId,
                    IsUsed = false,
                    AcquiredTime = DateTime.Now
                };

                _context.EVouchers.Add(eVoucher);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 取得所有電子禮券類型
        /// </summary>
        public async Task<IEnumerable<EVoucherType>> GetAllEVoucherTypesAsync()
        {
            return await _context.EVoucherTypes
                .OrderBy(evt => evt.Name)
                .ToListAsync();
        }

        /// <summary>
        /// 根據ID取得電子禮券類型
        /// </summary>
        public async Task<EVoucherType?> GetEVoucherTypeByIdAsync(int eVoucherTypeId)
        {
            return await _context.EVoucherTypes.FindAsync(eVoucherTypeId);
        }

        /// <summary>
        /// 產生電子禮券代碼
        /// </summary>
        private string GenerateEVoucherCode(string typeName)
        {
            var typeCode = GetTypeCode(typeName);
            var random = new Random().Next(1000, 9999).ToString();
            var number = new Random().Next(100000, 999999).ToString();
            return $"EV-{typeCode}-{random}-{number}";
        }

        /// <summary>
        /// 根據類型名稱取得類型代碼
        /// </summary>
        private string GetTypeCode(string typeName)
        {
            if (typeName.Contains("現金")) return "CASH";
            if (typeName.Contains("咖啡")) return "COFFEE";
            if (typeName.Contains("影城")) return "MOVIE";
            if (typeName.Contains("餐廳") || typeName.Contains("食")) return "FOOD";
            if (typeName.Contains("加油")) return "GAS";
            if (typeName.Contains("超商")) return "STORE";
            return "OTHER";
        }
    }
}
