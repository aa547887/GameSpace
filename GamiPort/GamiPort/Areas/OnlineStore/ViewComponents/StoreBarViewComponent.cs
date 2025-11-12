using GamiPort.Areas.OnlineStore.ViewModels;
using GamiPort.Areas.OnlineStore.ViewModels.store;
using GamiPort.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GamiPort.Areas.OnlineStore.ViewComponents
{
    public class StoreBarViewComponent : ViewComponent
    {
        private readonly GameSpacedatabaseContext _db;

        public StoreBarViewComponent(GameSpacedatabaseContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var viewModel = new StoreBarViewModel
            {
                Platforms = await _db.SPlatforms.AsNoTracking().OrderBy(p => p.PlatformId).ToListAsync(),
                GameGenres = await _db.SGameGenres.AsNoTracking().OrderBy(g => g.GenreId).ToListAsync(),
                MerchTypes = await _db.SMerchTypes.AsNoTracking().OrderBy(m => m.MerchTypeId).ToListAsync(),
                Suppliers = await _db.SSuppliers.AsNoTracking().Where(s => s.IsActive == 1 && s.IsDeleted == false).OrderBy(s => s.SupplierId).ToListAsync()
            };

            return View(viewModel);
        }
    }
}
