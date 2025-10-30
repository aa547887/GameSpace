using GamiPort.Models;                 // ✅ 保持你的命名空間
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GamiPort.Areas.OnlineStore.Controllers
{
    [ApiController]
    [Area("OnlineStore")]
    [Route("api/store")]                // ✅ 保持你原本的路由前綴
    public class StoreApiController : ControllerBase
    {
        private readonly GameSpacedatabaseContext _db;   // ✅ 保持你的 DbContext 類別
        public StoreApiController(GameSpacedatabaseContext db) => _db = db;

        // ===== Query / DTO ===== （定義查詢參數和回傳資料結構）
        public class ProductQuery
        {
            public string? q { get; set; }            // 關鍵字（名稱 / 代碼）
            public string? type { get; set; }         // "game" / "notgame"
            public int? platformId { get; set; }      // 平台（SGameProductDetail.PlatformId）
            public decimal? priceMin { get; set; }
            public decimal? priceMax { get; set; }
            public int page { get; set; } = 1;        // 1-based
            public int pageSize { get; set; } = 12;   // 預設 12
            public string? sort { get; set; }         // price_asc / price_desc / newest
        }

        public class ProductCardDto
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = "";
            public string ProductType { get; set; } = "";
            public decimal Price { get; set; }
            public string CurrencyCode { get; set; } = "TWD";
            public string ProductCode { get; set; } = "";
            public string CoverUrl { get; set; } = "";
            public string? PlatformName { get; set; }
            public bool IsPreorder { get; set; }
        }

        public class PagedResult<T>
        {
            public int page { get; set; }
            public int pageSize { get; set; }
            public int totalCount { get; set; }
            public IEnumerable<T> items { get; set; } = new List<T>();  // 修復：初始化為空List，避免null
        }

        public class ProductDetailDto
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = "";
            public string ProductType { get; set; } = "";
            public decimal Price { get; set; }
            public string CurrencyCode { get; set; } = "TWD";
            public string ProductCode { get; set; } = "";
            public bool IsPreorder { get; set; }
            public string? PlatformName { get; set; }
            public int? PlatformId { get; set; }
            public List<string> Images { get; set; } = new();          // 相簿（主圖優先）
            public List<RelatedItem> Related { get; set; } = new();    // 同類推薦
        }

        public class RelatedItem
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = "";
            public string ProductCode { get; set; } = "";
            public decimal Price { get; set; }
            public string CurrencyCode { get; set; } = "TWD";
            public string CoverUrl { get; set; } = "";
        }

        public class RateRequest { public int UserId { get; set; } public byte Rating { get; set; } public string? Review { get; set; } }  // 修復：Review允許null

        // ===== Helpers ===== （輔助方法：清理分頁、價格範圍）
        private static void SanitizePaging(ProductQuery q)
        {
            const int MaxPageSize = 60;
            if (q.page <= 0) q.page = 1;
            if (q.pageSize <= 0 || q.pageSize > MaxPageSize) q.pageSize = 12;
        }

        private static void NormalizePriceRange(ProductQuery q)
        {
            if (q.priceMin.HasValue && q.priceMax.HasValue && q.priceMin > q.priceMax)
                (q.priceMin, q.priceMax) = (q.priceMax, q.priceMin);
        }

        // ===== Endpoints ===== （API端點）

        // GET /api/store/products （獲取商品列表，支援篩選/排序/分頁，用真實資料庫S_ProductInfo等）
        [HttpGet("products")]
        public async Task<ActionResult<PagedResult<ProductCardDto>>> GetProducts([FromQuery] ProductQuery q, [FromQuery] string? tag = null)  // 修復：tag允許null
        {
            SanitizePaging(q);  // 清理分頁參數
            NormalizePriceRange(q);  // 正規化價格範圍

            var query = _db.SProductInfos.AsNoTracking().Where(p => !p.IsDeleted);  // 基礎查詢：未刪除商品

            // 關鍵字篩選
            if (!string.IsNullOrWhiteSpace(q.q))
            {
                var keyword = q.q.Trim();
                query = query.Where(p => p.ProductName.Contains(keyword) || (p.SProductCode != null && p.SProductCode.ProductCode.Contains(keyword)));
            }

            // 類別篩選
            if (!string.IsNullOrWhiteSpace(q.type))
            {
                var t = q.type.Trim().ToLower();
                if (t == "game" || t == "notgame") query = query.Where(p => p.ProductType.ToLower() == t);
            }

            // 平台篩選
            if (q.platformId.HasValue)
            {
                var pid = q.platformId.Value;
                query = query.Where(p => p.SGameProductDetail != null && p.SGameProductDetail.PlatformId == pid);
            }

            // 價格篩選
            if (q.priceMin.HasValue) query = query.Where(p => p.Price >= q.priceMin.Value);
            if (q.priceMax.HasValue) query = query.Where(p => p.Price <= q.priceMax.Value);

            // tag篩選（e.g., "sale" = 預購）
            if (!string.IsNullOrWhiteSpace(tag))
            {
                if (tag == "sale") query = query.Where(p => p.IsPreorderEnabled);
            }

            // 排序
            IOrderedQueryable<SProductInfo> orderedQuery = q.sort switch  // 修復：宣告為IOrderedQueryable
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.CreatedAt) // newest預設
            };

            var total = await orderedQuery.CountAsync();  // 總數
            var items = await orderedQuery.Skip((q.page - 1) * q.pageSize).Take(q.pageSize).Select(p => new ProductCardDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductType = p.ProductType,
                Price = p.Price,
                CurrencyCode = p.CurrencyCode,
                ProductCode = p.SProductCode != null ? p.SProductCode.ProductCode : "",
                CoverUrl = p.SProductImages.OrderByDescending(img => img.IsPrimary).ThenBy(img => img.SortOrder).Select(img => img.ProductimgUrl).FirstOrDefault() ?? "",
                PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null ? p.SGameProductDetail.Platform.PlatformName : null,
                IsPreorder = p.IsPreorderEnabled
            }).ToListAsync();

            return Ok(new PagedResult<ProductCardDto> { page = q.page, pageSize = q.pageSize, totalCount = total, items = items });
        }

        // GET /api/store/products/{code} （根據代碼獲取商品詳情，用真實資料庫）
        [HttpGet("products/{code}")]
        public async Task<ActionResult<ProductDetailDto>> GetProductByCode([FromRoute] string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return NotFound();
            code = code.Trim();

            var product = await _db.SProductInfos.AsNoTracking().Where(p => !p.IsDeleted && p.SProductCode != null && p.SProductCode.ProductCode == code).Select(p => new ProductDetailDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductType = p.ProductType,
                Price = p.Price,
                CurrencyCode = p.CurrencyCode,
                ProductCode = p.SProductCode!.ProductCode,
                IsPreorder = p.IsPreorderEnabled,
                PlatformId = p.SGameProductDetail != null ? p.SGameProductDetail.PlatformId : (int?)null,
                PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null ? p.SGameProductDetail.Platform.PlatformName : null,
                Images = p.SProductImages.OrderByDescending(img => img.IsPrimary).ThenBy(img => img.SortOrder).Select(img => img.ProductimgUrl!).ToList(),
            }).FirstOrDefaultAsync();

            if (product == null) return NotFound();

            // 同類推薦
            var relatedQuery = _db.SProductInfos.AsNoTracking().Where(x => !x.IsDeleted && x.ProductId != product.ProductId);
            if (!string.IsNullOrWhiteSpace(product.ProductType)) relatedQuery = relatedQuery.Where(x => x.ProductType == product.ProductType);
            if (product.PlatformId.HasValue) relatedQuery = relatedQuery.Where(x => x.SGameProductDetail != null && x.SGameProductDetail.PlatformId == product.PlatformId);

            var related = await relatedQuery.OrderByDescending(x => x.CreatedAt).Take(8).Select(x => new RelatedItem
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                ProductCode = x.SProductCode != null ? x.SProductCode.ProductCode : "",
                Price = x.Price,
                CurrencyCode = x.CurrencyCode,
                CoverUrl = x.SProductImages.OrderByDescending(img => img.IsPrimary).ThenBy(img => img.SortOrder).Select(img => img.ProductimgUrl!).FirstOrDefault() ?? ""
            }).ToListAsync();

            product.Related = related;
            return Ok(product);
        }

        // GET /api/store/rankings （獲取排行榜，用真實視圖SVRankingSales等查詢S_Official_Store_Ranking資料）
        // GET /api/store/rankings （獲取排行榜：sales / rating / hot）
        [HttpGet("rankings")]
        public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetRankings(string type = "sales", int take = 12)
        {
            if (take <= 0 || take > 60) take = 12;

            IQueryable<SProductInfo> query;

            if (type == "sales")
            {
                // 以銷售排行視圖（或表）取得前 N 名的 ProductId，再 join 回商品表
                var ranked = _db.SVRankingSales.AsNoTracking()
                    .OrderBy(r => r.RankingPosition)
                    .Select(r => new { r.ProductId, r.RankingPosition })
                    .Take(take);

                query =
                    from r in ranked
                    join p in _db.SProductInfos.AsNoTracking().Where(p => !p.IsDeleted)
                        on r.ProductId equals p.ProductId
                    orderby r.RankingPosition
                    select p;
            }
            else if (type == "rating")
            {
                // 以評分排行視圖（或表）取得前 N 名的 ProductId，再 join 回商品表
                var ranked = _db.SVRankingRatings.AsNoTracking()
                    .OrderBy(r => r.RankingPosition)
                    .Select(r => new { r.ProductId, r.RankingPosition })
                    .Take(take);

                query =
                    from r in ranked
                    join p in _db.SProductInfos.AsNoTracking().Where(p => !p.IsDeleted)
                        on r.ProductId equals p.ProductId
                    orderby r.RankingPosition
                    select p;
            }
            else
            {
                // 熱門 / 最新：直接取商品
                query = _db.SProductInfos.AsNoTracking()
                    .Where(p => !p.IsDeleted)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(take);
            }

            var items = await query
                .Select(p => new ProductCardDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductType = p.ProductType,
                    Price = p.Price,
                    CurrencyCode = p.CurrencyCode,
                    ProductCode = p.SProductCode != null ? p.SProductCode.ProductCode : "",
                    CoverUrl = p.SProductImages
                        .OrderByDescending(img => img.IsPrimary)
                        .ThenBy(img => img.SortOrder)
                        .Select(img => img.ProductimgUrl)
                        .FirstOrDefault() ?? "",
                    PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null
                        ? p.SGameProductDetail.Platform.PlatformName
                        : null,
                    IsPreorder = p.IsPreorderEnabled
                })
                .ToListAsync();

            return Ok(items);
        }



        // POST /api/store/products/{id}/favorite （切換收藏，用真實S_UserFavorites表）
        [HttpPost("products/{id}/favorite")]
        public async Task<IActionResult> ToggleFavorite(int id, [FromBody] int userId)
        {
            var existing = await _db.SUserFavorites.FindAsync(userId, id);
            if (existing != null) _db.SUserFavorites.Remove(existing);  // 取消收藏
            else _db.SUserFavorites.Add(new SUserFavorite { UserId = userId, ProductId = id });  // 添加收藏
            await _db.SaveChangesAsync();
            return Ok(new { isFavorited = existing == null });
        }

        // POST /api/store/products/{id}/rate （提交評分，用真實S_ProductRatings表）
        [HttpPost("products/{id}/rate")]
        public async Task<IActionResult> RateProduct(int id, [FromBody] RateRequest req)
        {
            var rating = await _db.SProductRatings.FirstOrDefaultAsync(r => r.ProductId == id && r.UserId == req.UserId);
            if (rating == null) rating = new SProductRating { ProductId = id, UserId = req.UserId };
            rating.Rating = req.Rating;
            rating.ReviewText = req.Review;
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}