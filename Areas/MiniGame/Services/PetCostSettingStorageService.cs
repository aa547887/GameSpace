using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Data;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物點數設定儲存邏輯服務
    /// </summary>
    public class PetCostSettingStorageService
    {
        private readonly MiniGameDbContext _context;

        public PetCostSettingStorageService(MiniGameDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 儲存寵物換色點數設定
        /// </summary>
        public async Task<bool> SaveColorCostSettingAsync(PetSkinColorCostSetting setting)
        {
            try
            {
                // 檢查是否已存在相同名稱的設定
                var existing = await _context.PetSkinColorCostSettings
                    .FirstOrDefaultAsync(x => x.ColorName == setting.ColorName && x.Id != setting.Id);
                
                if (existing != null)
                {
                    throw new InvalidOperationException($"顏色名稱 '{setting.ColorName}' 已存在");
                }

                // 設定時間戳
                if (setting.Id == 0)
                {
                    setting.CreatedAt = DateTime.UtcNow;
                    _context.PetSkinColorCostSettings.Add(setting);
                }
                else
                {
                    setting.UpdatedAt = DateTime.UtcNow;
                    _context.PetSkinColorCostSettings.Update(setting);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 儲存寵物換背景點數設定
        /// </summary>
        public async Task<bool> SaveBackgroundCostSettingAsync(PetBackgroundCostSetting setting)
        {
            try
            {
                // 檢查是否已存在相同名稱的設定
                var existing = await _context.PetBackgroundCostSettings
                    .FirstOrDefaultAsync(x => x.BackgroundName == setting.BackgroundName && x.Id != setting.Id);
                
                if (existing != null)
                {
                    throw new InvalidOperationException($"背景名稱 '{setting.BackgroundName}' 已存在");
                }

                // 設定時間戳
                if (setting.Id == 0)
                {
                    setting.CreatedAt = DateTime.UtcNow;
                    _context.PetBackgroundCostSettings.Add(setting);
                }
                else
                {
                    setting.UpdatedAt = DateTime.UtcNow;
                    _context.PetBackgroundCostSettings.Update(setting);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 批量儲存寵物換色點數設定
        /// </summary>
        public async Task<bool> SaveColorCostSettingsBatchAsync(IEnumerable<PetSkinColorCostSetting> settings)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                foreach (var setting in settings)
                {
                    if (setting.Id == 0)
                    {
                        setting.CreatedAt = DateTime.UtcNow;
                        _context.PetSkinColorCostSettings.Add(setting);
                    }
                    else
                    {
                        setting.UpdatedAt = DateTime.UtcNow;
                        _context.PetSkinColorCostSettings.Update(setting);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 批量儲存寵物換背景點數設定
        /// </summary>
        public async Task<bool> SaveBackgroundCostSettingsBatchAsync(IEnumerable<PetBackgroundCostSetting> settings)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                foreach (var setting in settings)
                {
                    if (setting.Id == 0)
                    {
                        setting.CreatedAt = DateTime.UtcNow;
                        _context.PetBackgroundCostSettings.Add(setting);
                    }
                    else
                    {
                        setting.UpdatedAt = DateTime.UtcNow;
                        _context.PetBackgroundCostSettings.Update(setting);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 驗證點數設定資料
        /// </summary>
        public async Task<ValidationResult> ValidateCostSettingAsync(object setting)
        {
            var result = new ValidationResult();

            if (setting is PetSkinColorCostSetting colorSetting)
            {
                // 驗證顏色代碼格式
                if (!IsValidColorCode(colorSetting.ColorCode))
                {
                    result.AddError("ColorCode", "顏色代碼格式不正確，應為 #RRGGBB 格式");
                }

                // 驗證點數範圍
                if (colorSetting.RequiredPoints < 0)
                {
                    result.AddError("RequiredPoints", "所需點數不能為負數");
                }

                // 檢查名稱重複
                var existing = await _context.PetSkinColorCostSettings
                    .FirstOrDefaultAsync(x => x.ColorName == colorSetting.ColorName && x.Id != colorSetting.Id);
                if (existing != null)
                {
                    result.AddError("ColorName", "顏色名稱已存在");
                }
            }
            else if (setting is PetBackgroundCostSetting backgroundSetting)
            {
                // 驗證背景代碼格式
                if (!IsValidColorCode(backgroundSetting.BackgroundCode))
                {
                    result.AddError("BackgroundCode", "背景代碼格式不正確，應為 #RRGGBB 格式");
                }

                // 驗證點數範圍
                if (backgroundSetting.RequiredPoints < 0)
                {
                    result.AddError("RequiredPoints", "所需點數不能為負數");
                }

                // 檢查名稱重複
                var existing = await _context.PetBackgroundCostSettings
                    .FirstOrDefaultAsync(x => x.BackgroundName == backgroundSetting.BackgroundName && x.Id != backgroundSetting.Id);
                if (existing != null)
                {
                    result.AddError("BackgroundName", "背景名稱已存在");
                }
            }

            return result;
        }

        /// <summary>
        /// 驗證顏色代碼格式
        /// </summary>
        private bool IsValidColorCode(string colorCode)
        {
            if (string.IsNullOrEmpty(colorCode) || colorCode.Length != 7)
                return false;

            if (!colorCode.StartsWith("#"))
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(colorCode, @"^#[0-9A-Fa-f]{6}$");
        }
    }

    /// <summary>
    /// 驗證結果類別
    /// </summary>
    public class ValidationResult
    {
        public List<ValidationError> Errors { get; } = new List<ValidationError>();
        public bool IsValid => Errors.Count == 0;

        public void AddError(string field, string message)
        {
            Errors.Add(new ValidationError { Field = field, Message = message });
        }
    }

    /// <summary>
    /// 驗證錯誤類別
    /// </summary>
    public class ValidationError
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
