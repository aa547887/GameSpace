using GameSpace.Areas.MiniGame.Data;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物背景選項服務擴充
    /// </summary>
    public partial class PetBackgroundOptionService
    {
        /// <summary>
        /// 切換背景選項啟用狀態
        /// </summary>
        public async Task<bool> ToggleBackgroundOptionStatusAsync(int id)
        {
            try
            {
                var backgroundOption = await _context.PetBackgroundOptions.FindAsync(id);
                if (backgroundOption == null)
                    return false;

                backgroundOption.IsActive = !backgroundOption.IsActive;
                backgroundOption.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
