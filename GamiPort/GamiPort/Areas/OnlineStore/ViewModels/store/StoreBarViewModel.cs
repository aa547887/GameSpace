using GamiPort.Models;
using System.Collections.Generic;

namespace GamiPort.Areas.OnlineStore.ViewModels.store
{
    public class StoreBarViewModel
    {
        public List<SPlatform> Platforms { get; set; } = new List<SPlatform>();
        public List<SGameGenre> GameGenres { get; set; } = new List<SGameGenre>();
        public List<SMerchType> MerchTypes { get; set; } = new List<SMerchType>();
        public List<SSupplier> Suppliers { get; set; } = new List<SSupplier>();
    }
}
