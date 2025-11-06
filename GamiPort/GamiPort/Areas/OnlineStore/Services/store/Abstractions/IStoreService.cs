using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GamiPort.Areas.OnlineStore.DTO.Store;
using GamiPort.Areas.OnlineStore.ViewModels;

namespace GamiPort.Areas.OnlineStore.Services.store.Abstractions
{
    public interface IStoreService
    {
        Task<GamiPort.Areas.OnlineStore.DTO.Store.PagedResult<ProductCardDto>> GetProducts(ProductQuery q, string? tag, string? productType, string? platform = null, string? genre = null);
        Task<GamiPort.Areas.OnlineStore.DTO.Store.PagedResult<GamiPort.Areas.OnlineStore.DTO.Store.ProductFullDto>> GetProductsFull(ProductQuery q, string? tag, string? productType);
        Task<ProductDetailDto?> GetProductByCode(string code);
        Task<List<ProductCardDto>> GetRankings(string type, string period, int take);
        Task<bool> ToggleFavorite(int productId, int userId);
        Task RateProduct(int productId, RateRequest req);
        Task TrackClick(int productId, int? userId);
        Task AddToCart(AddToCartRequest req, int userId);
        Task<List<ProductCardDto>> GetLatest(int take);
        Task<List<ProductCardDto>> GetRankingsFromOfficial(string type, string period, DateTime? date, int take);
        Task<List<ProductCardVM>> GetBrowseCards();
        Task<ProductDetailVM?> GetProductDetailVM(int id);
        Task<List<ProductCardDto>> GetTopFavorites(int count);
        Task<GamiPort.Areas.OnlineStore.DTO.Store.PagedResult<ProductCardDto>> GetFavorites(int userId, int page, int pageSize);
        Task<List<int>> GetFavoriteIds(int userId);
        Task<GamiPort.Areas.OnlineStore.DTO.Store.PagedResult<GamiPort.Areas.OnlineStore.DTO.Store.ReviewDto>> GetProductReviews(int productId, int page, int pageSize);
    }
}


