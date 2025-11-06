using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GamiPort.Areas.OnlineStore.DTO.Store;
using GamiPort.Areas.OnlineStore.Services.store.Abstractions;
using GamiPort.Areas.OnlineStore.ViewModels;

namespace GamiPort.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    [Route("OnlineStore/api/[controller]")]
    [ApiController]
    public class StoreApiController : ControllerBase
    {
        private readonly IStoreService _service;

        public StoreApiController(IStoreService service)
        {
            _service = service;
        }

		/// <summary>
		/// 取得商品清單（支援查詢、篩選、排序與分頁）。
		/// </summary>
		//Get /OnlineStore/api/StoreApi/products?tag=Action&productType=Game&sortBy=price_asc&page=1&pageSize=20
		[HttpGet("products")]
        public async Task<ActionResult<GamiPort.Areas.OnlineStore.DTO.Store.PagedResult<ProductCardDto>>> GetProducts(
            [FromQuery] ProductQuery query,
            [FromQuery] string? tag = null,
            [FromQuery] string? productType = null,
            [FromQuery] string? platform = null,
            [FromQuery] string? genre = null)
        {
            try
            {
                // Debug headers to verify model binding (can be removed later)
                Response.Headers["X-Debug-Query"] = Request?.QueryString.Value ?? string.Empty;
                Response.Headers["X-Debug-Bound"] = $"type={query.type}, platformId={query.platformId}, genreId={query.genreId}, merchTypeId={query.merchTypeId}, supplierId={query.supplierId}, page={query.page}, pageSize={query.pageSize}";
                var result = await _service.GetProducts(query, tag, productType, platform, genre);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(title: "GetProducts failed", detail: ex.Message, statusCode: 500);
            }
        }

        /// <summary>
        /// 以商品代碼取得商品明細。
        /// </summary>
        [HttpGet("products/{code}")]
        public async Task<ActionResult<ProductDetailDto>> GetProductByCode([FromRoute] string code)
        {
            var product = await _service.GetProductByCode(code);
            if (product == null) return NotFound();
            return Ok(product);
        }

        /// <summary>
        /// 取得排行榜（sales/click/favorite）。
        /// </summary>
        [HttpGet("rankings")]
        public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetRankings(string type = "sales", string period = "daily", int take = 12)
        {
            try
            {
                var items = await _service.GetRankings(type, period, take);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return Problem(title: "GetRankings failed", detail: ex.Message, statusCode: 500);
            }
        }

        /// <summary>
        /// 切換商品收藏狀態。
        /// </summary>
        [HttpPost("products/{id}/favorite")]
        public async Task<IActionResult> ToggleFavorite(int id, [FromBody] int userId)
        {
            var isFavorited = await _service.ToggleFavorite(id, userId);
            return Ok(new { isFavorited });
        }

        /// <summary>
        /// 評分/評論商品。
        /// </summary>
        [HttpPost("products/{id}/rate")]
        public async Task<IActionResult> RateProduct(int id, [FromBody] RateRequest req)
        {
            await _service.RateProduct(id, req);
            return Ok();
        }

        /// <summary>
        /// 記錄商品點擊。
        /// </summary>
        [HttpPost("products/{id}/click")]
        public async Task<IActionResult> TrackClick(int id, [FromBody] int? userId)
        {
            await _service.TrackClick(id, userId);
            return Ok();
        }

        /// <summary>
        /// 加入購物車（示範：以固定使用者 Id 1）。
        /// </summary>
        [HttpPost("cart/add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest req)
        {
            // TODO: 替換為實際登入使用者 Id
            var userId = 1;
            try
            {
                await _service.AddToCart(req, userId);
                return Ok(new { message = "Product added to cart." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// 取得最新上架商品。
        /// </summary>
        [HttpGet("latest")]
        public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetLatest([FromQuery] int take = 5)
        {
            var items = await _service.GetLatest(take);
            return Ok(items);
        }

        /// <summary>
        /// 取得官方排行榜（依指標/期間/日期）。
        /// </summary>
        [HttpGet("rankings/official")]
        public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetRankingsFromOfficial(
            [FromQuery] string type = "purchase",
            [FromQuery] string period = "daily",
            [FromQuery] DateTime? date = null,
            [FromQuery] int take = 10)
        {
            var items = await _service.GetRankingsFromOfficial(type, period, date, take);
            return Ok(items);
        }

        /// <summary>
        /// 取得瀏覽頁卡片資料（簡化投影）。
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductCardVM>>> GetBrowseCards()
        {
            var data = await _service.GetBrowseCards();
            return Ok(data);
        }

        /// <summary>
        /// 取得商品詳情（依商品 Id）。
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDetailVM>> GetProductDetail(int id)
        {
            var vm = await _service.GetProductDetailVM(id);
            if (vm == null) return NotFound();
            return Ok(vm);
        }

        /// <summary>
        /// 取得收藏數最高的商品清單。
        /// </summary>
        [HttpGet("top-favorites")]
        public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetTopFavorites(int count = 8)
        {
            var items = await _service.GetTopFavorites(count);
            if (items.Count == 0) return Ok(Array.Empty<ProductCardDto>());
            return Ok(items);
        }
    }
}
