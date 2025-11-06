using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GamiPort.Areas.OnlineStore.DTO.Store;
using GamiPort.Areas.OnlineStore.ViewModels;

namespace GamiPort.Areas.OnlineStore.Services.store.Abstractions
{
    /// <summary>
    /// 線上商店查詢/操作服務介面。
    /// </summary>
    public interface IStoreService
    {
        /// <summary>取得商品清單（分頁/排序/篩選）。</summary>
        Task<GamiPort.Areas.OnlineStore.DTO.Store.PagedResult<ProductCardDto>> GetProducts(ProductQuery q, string? tag, string? productType, string? platform = null, string? genre = null);

        /// <summary>商品完整資料清單（分頁、含排序/篩選）</summary>
        Task<GamiPort.Areas.OnlineStore.DTO.Store.PagedResult<GamiPort.Areas.OnlineStore.DTO.Store.ProductFullDto>> GetProductsFull(ProductQuery q, string? tag, string? productType);

        /// <summary>以商品代碼取得商品明細。</summary>
        Task<ProductDetailDto?> GetProductByCode(string code);

        /// <summary>取得排行榜（sales/click/favorite）。</summary>
        Task<List<ProductCardDto>> GetRankings(string type, string period, int take);

        /// <summary>切換收藏狀態，回傳是否為已收藏。</summary>
        Task<bool> ToggleFavorite(int productId, int userId);

        /// <summary>評分/評論商品。</summary>
        Task RateProduct(int productId, RateRequest req);

        /// <summary>記錄商品點擊。</summary>
        Task TrackClick(int productId, int? userId);

        /// <summary>加入購物車。</summary>
        Task AddToCart(AddToCartRequest req, int userId);

        /// <summary>取得最新上架商品。</summary>
        Task<List<ProductCardDto>> GetLatest(int take);

        /// <summary>取得官方排行榜。</summary>
        Task<List<ProductCardDto>> GetRankingsFromOfficial(string type, string period, DateTime? date, int take);

        /// <summary>取得瀏覽頁卡片資料。</summary>
        Task<List<ProductCardVM>> GetBrowseCards();

        /// <summary>取得商品詳情（依 Id）。</summary>
        Task<ProductDetailVM?> GetProductDetailVM(int id);

        /// <summary>取得收藏數最高的商品清單。</summary>
        Task<List<ProductCardDto>> GetTopFavorites(int count);
    }
}
