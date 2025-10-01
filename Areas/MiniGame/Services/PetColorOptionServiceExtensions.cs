using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物顏色選項服務擴充
    /// </summary>
    public partial class PetColorOptionService
    {
        /// <summary>
        /// 建立顏色選項（表單版本）
        /// </summary>
        public async Task<bool> CreateColorOptionAsync(PetColorOptionFormViewModel model)
        {
            try
            {
                var viewModel = new PetColorOptionViewModel
                {
                    ColorName = model.ColorName,
                    ColorCode = model.ColorCode,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    SortOrder = model.SortOrder
                };

                return await CreateColorOptionAsync(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立顏色選項時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 切換顏色選項啟用狀態
        /// </summary>
        public async Task<bool> ToggleColorOptionStatusAsync(int id)
        {
            try
            {
                var option = _colorOptions.FirstOrDefault(x => x.Id == id);
                if (option == null)
                {
                    return false;
                }

                option.IsActive = !option.IsActive;
                option.UpdatedAt = DateTime.UtcNow;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換顏色選項狀態時發生錯誤，ID: {Id}", id);
                throw;
            }
        }
    }
}
