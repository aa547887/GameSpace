using GamiPort.Models;
using GameSpace.Areas.OnlineStore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamiPort.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    [Route("OnlineStore/api/[controller]")] // ← 這樣 action 不會被 [action] 佔位衝到
    [ApiController]
    public class StoreApiController : ControllerBase
    {
        private readonly GameSpacedatabaseContext _db;
        public StoreApiController(GameSpacedatabaseContext db) => _db = db;

        // ===== Query / DTO =====
        public class ProductQuery
        {
            public string? q { get; set; }
            public string? type { get; set; }
            public int? platformId { get; set; }
            public int? merchTypeId { get; set; }
            public int? excludeMerchTypeId { get; set; }
            public decimal? priceMin { get; set; }
            public decimal? priceMax { get; set; }
            public int page { get; set; } = 1;
            public int pageSize { get; set; } = 12;
            public string? sort { get; set; } // price_asc / price_desc / newest / random
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
            public string? PeripheralTypeName { get; set; }
            public bool IsPreorder { get; set; }

            public string? MerchTypeName { get; set; }  // ← 新增

        }

        public class PagedResult<T>
        {
            public int page { get; set; }
            public int pageSize { get; set; }
            public int totalCount { get; set; }
            public IEnumerable<T> items { get; set; } = new List<T>();
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
            public List<string> Images { get; set; } = new();
            public List<RelatedItem> Related { get; set; } = new();
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

        public class RateRequest { public int UserId { get; set; } public byte Rating { get; set; } public string? Review { get; set; } }

        public class AddToCartRequest { public int ProductId { get; set; } public int Qty { get; set; } = 1; }

        // ===== Helpers =====
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

        // ===== Endpoints =====

        [HttpGet("products")]
        public async Task<ActionResult<PagedResult<ProductCardDto>>> GetProducts([FromQuery] ProductQuery q, [FromQuery] string? tag = null)
        {
            SanitizePaging(q);
            NormalizePriceRange(q);

            var query = _db.SProductInfos.AsNoTracking().Where(p => !p.IsDeleted);

            // 關鍵字（品名 / 產品碼）
            if (!string.IsNullOrWhiteSpace(q.q))
            {
                var keyword = q.q.Trim();
                query = query.Where(p =>
                    p.ProductName.Contains(keyword) ||
                    (p.SProductCode != null && p.SProductCode.ProductCode.Contains(keyword)));
            }

            // 類型：game / notgame
            if (!string.IsNullOrWhiteSpace(q.type))
            {
                var t = q.type.Trim().ToLower();
                if (t == "game" || t == "notgame")
                    query = query.Where(p => p.ProductType.ToLower() == t);
            }

            // 平台
            if (q.platformId.HasValue)
            {
                var pid = q.platformId.Value;
                query = query.Where(p => p.SGameProductDetail != null && p.SGameProductDetail.PlatformId == pid);
            }

            // 周邊類別（包含 / 排除）
            if (q.merchTypeId.HasValue)
            {
                query =
                    from p in query
                    join other in _db.SOtherProductDetails on p.ProductId equals other.ProductId
                    where other.MerchTypeId == q.merchTypeId.Value
                    select p;
            }

            if (q.excludeMerchTypeId.HasValue)
            {
                query =
                    from p in query
                    join other in _db.SOtherProductDetails on p.ProductId equals other.ProductId into details
                    from d in details.DefaultIfEmpty()
                    where d == null || d.MerchTypeId != q.excludeMerchTypeId.Value
                    select p;
            }

            // 價格區間
            if (q.priceMin.HasValue) query = query.Where(p => p.Price >= q.priceMin.Value);
            if (q.priceMax.HasValue) query = query.Where(p => p.Price <= q.priceMax.Value);

            // tag：這裡你原本用 sale => IsPreorderEnabled，可能只是暫代；我保留寫法，但標註提醒
            if (!string.IsNullOrWhiteSpace(tag))
            {
                if (tag == "sale")
                    query = query.Where(p => p.IsPreorderEnabled); // TODO: 之後換成真正「特價中」欄位或條件
            }

            // 排序
            IOrderedQueryable<SProductInfo> orderedQuery;
            if (q.sort == "random")
            {
                orderedQuery = query.OrderBy(p => Guid.NewGuid());
            }
            else
            {
                orderedQuery = q.sort switch
                {
                    "price_asc" => query.OrderBy(p => p.Price),
                    "price_desc" => query.OrderByDescending(p => p.Price),
                    _ => query.OrderByDescending(p => p.CreatedAt) // newest
                };
            }

            var total = await orderedQuery.CountAsync();

            var items = await orderedQuery
                .Skip((q.page - 1) * q.pageSize)
                .Take(q.pageSize)
                .Select(p => new ProductCardDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductType = p.ProductType,        // "game" / "notgame"
                    Price = p.Price,
                    CurrencyCode = p.CurrencyCode,
                    ProductCode = p.SProductCode != null ? p.SProductCode.ProductCode : "",

                    // 主圖（沒有圖就用 nophoto）
                    CoverUrl = p.SProductImages
                        .OrderByDescending(img => img.IsPrimary)
                        .ThenBy(img => img.SortOrder)
                        .Select(img => img.ProductimgUrl)
                        .FirstOrDefault() ?? "/images/onlinestoreNOPic/nophoto.jpg",

                    // 遊戲平台
                    PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null
                        ? p.SGameProductDetail.Platform.PlatformName
                        : null,

                    // 周邊分類（名稱）
                    // 注意：我把欄位名稱改成 PeripheralTypeName，這樣可與 Browse 前端一樣的 key 對上
                    PeripheralTypeName = (
                        from d in _db.SOtherProductDetails
                        join mt in _db.SMerchTypes on d.MerchTypeId equals mt.MerchTypeId
                        where d.ProductId == p.ProductId && !d.IsDeleted
                        select mt.MerchTypeName
                    ).FirstOrDefault(),

                    IsPreorder = p.IsPreorderEnabled
                })
                .ToListAsync();

            return Ok(new PagedResult<ProductCardDto>
            {
                page = q.page,
                pageSize = q.pageSize,
                totalCount = total,
                items = items
            });
        }


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

        [HttpGet("rankings")]
        public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetRankings(string type = "sales", string period = "daily", int take = 12)
        {
            if (take <= 0 || take > 60) take = 12;

            var periodType = period switch
            {
                "daily" => 1,
                "weekly" => 2,
                "monthly" => 3,
                "quarterly" => 4,
                "yearly" => 5,
                _ => 1
            };

            IQueryable<SProductInfo> query;

            if (type == "sales")
            {
                var ranked = _db.SVRankingSales.AsNoTracking()
                    .Where(r => r.PeriodType == periodType)
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
            else if (type == "click")
            {
                var ranked = _db.SVRankingClicks.AsNoTracking()
                    .Where(r => r.PeriodType == periodType)
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
            else if (type == "favorite")
            {
                var ranked = _db.SVRankingRatings.AsNoTracking()
                    .Where(r => r.PeriodType == periodType)
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

        [HttpPost("products/{id}/favorite")]
        public async Task<IActionResult> ToggleFavorite(int id, [FromBody] int userId)
        {
            var existing = await _db.SUserFavorites.FindAsync(userId, id);
            if (existing != null) _db.SUserFavorites.Remove(existing);
            else _db.SUserFavorites.Add(new SUserFavorite { UserId = userId, ProductId = id });
            await _db.SaveChangesAsync();
            return Ok(new { isFavorited = existing == null });
        }

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

        [HttpPost("products/{id}/click")]
        public IActionResult TrackClick(int id, [FromBody] int? userId)
        {
            // ToDo: Implement click logging to the database here.
            return Ok();
        }

        [HttpPost("cart/add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest req)
        {
            // TODO: Replace with actual authenticated user ID
            var userId = 1;

            if (req.Qty <= 0) return BadRequest("Quantity must be positive.");

            var cart = await _db.SoCarts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new SoCart { UserId = userId, CreatedAt = DateTime.UtcNow };
                _db.SoCarts.Add(cart);
                await _db.SaveChangesAsync(); // Save to get CartId
            }

            var cartItem = await _db.SoCartItems.FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.ProductId == req.ProductId);

            if (cartItem != null)
            {
                cartItem.Qty += req.Qty;
            }
            else
            {
                var product = await _db.SProductInfos.FindAsync(req.ProductId);
                if (product == null) return NotFound("Product not found.");

                cartItem = new SoCartItem
                {
                    CartId = cart.CartId,
                    ProductId = req.ProductId,
                    Qty = req.Qty,
                    UnitPrice = product.Price
                };
                _db.SoCartItems.Add(cartItem);
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Product added to cart." });
        }


        [HttpGet("latest")]
        public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetLatest([FromQuery] int take = 5)
        {
            if (take <= 0 || take > 60) take = 5;

            var items = await _db.SProductInfos.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Take(take)
                .Select(p => new ProductCardDto
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
                })
                .ToListAsync();

            return Ok(items);
        }
        [HttpGet("rankings/official")]
        public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetRankingsFromOfficial(
            [FromQuery] string type = "purchase",     // purchase / click / favorite
            [FromQuery] string period = "daily",      // daily / weekly / monthly / quarterly / yearly
            [FromQuery] DateTime? date = null,        // 指定哪一天的榜；不給就抓最新一天
            [FromQuery] int take = 10)

            {
            if (take <= 0 || take > 60) take = 10;

            int periodType = period switch
            {
                "daily" => 1,
                "weekly" => 2,
                "monthly" => 3,
                "quarterly" => 4,
                "yearly" => 5,
                _ => 1
            };
            string metric = type switch
            {
                "purchase" => "purchase",
                "click" => "click",
                "favorite" => "favorite",
                _ => "purchase"
            };

            // ★ 這裡改用 DateOnly
            DateOnly targetDateOnly;
            if (date.HasValue)
            {
                targetDateOnly = DateOnly.FromDateTime(date.Value.Date);
            }
            else
            {
                // 取該 period+metric 最新一天（DateOnly）
                var latest = await _db.SOfficialStoreRankings.AsNoTracking()
                    .Where(r => r.PeriodType == periodType && r.RankingMetric == metric)
                    .OrderByDescending(r => r.RankingDate)
                    .Select(r => r.RankingDate)                // <- DateOnly
                    .FirstOrDefaultAsync();

                targetDateOnly = latest == default ? DateOnly.FromDateTime(DateTime.Today) : latest;
            }

            // 取該天前 N 名
            var ranked = _db.SOfficialStoreRankings.AsNoTracking()
                .Where(r => r.PeriodType == periodType
                         && r.RankingMetric == metric
                         && r.RankingDate == targetDateOnly)   // <- DateOnly 比對
                .OrderBy(r => r.RankingPosition)
                .Take(take)
                .Select(r => new { r.ProductId, r.RankingPosition });

            var query =
                from r in ranked
                join p in _db.SProductInfos.AsNoTracking().Where(p => !p.IsDeleted)
                    on r.ProductId equals p.ProductId
                orderby r.RankingPosition
                select p;

            var items = await query.Select(p => new ProductCardDto
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
                IsPreorder = p.IsPreorderEnabled,

                // 周邊分類名稱（有就帶）
                MerchTypeName = (
                    from d in _db.SOtherProductDetails
                    join mt in _db.SMerchTypes on d.MerchTypeId equals mt.MerchTypeId
                    where d.ProductId == p.ProductId && !d.IsDeleted
                    select mt.MerchTypeName
                ).FirstOrDefault(),
            }).ToListAsync();

            return Ok(items);
        }
        // GET: /OnlineStore/api/StoreApi/GetBrowseCards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductCardVM>>> GetBrowseCards()
        {
            var query =
                from p in _db.SProductInfos
                where p.IsDeleted == false
                orderby p.ProductId descending
                select new ProductCardVM
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductType = p.ProductType,
                    Price = p.Price,
                    CurrencyCode = p.CurrencyCode,
                    CoverUrl = _db.SProductImages
                        .Where(img => img.ProductId == p.ProductId)
                        .OrderByDescending(img => img.IsPrimary) // 先主圖
                        .ThenBy(img => img.SortOrder)
                        .Select(img => img.ProductimgUrl)
                        .FirstOrDefault() ?? "/images/placeholder-cover.png"
                };

            var data = await query.AsNoTracking().ToListAsync();
            return Ok(data);
        }

        // GET: /OnlineStore/api/StoreApi/GetProductDetail/123
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDetailVM>> GetProductDetail(int id)
        {
            var p = await _db.SProductInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductId == id && x.IsDeleted == false);

            if (p == null) return NotFound();

            var imgs = await _db.SProductImages
                .Where(i => i.ProductId == id)
                .OrderByDescending(i => i.IsPrimary)
                .ThenBy(i => i.SortOrder)
                .Select(i => i.ProductimgUrl)
                .ToListAsync();

            // 這裡先簡單把描述來源分流：遊戲類讀 S_GameProductDetails，其它讀 S_OtherProductDetails
            string? desc = await _db.SGameProductDetails
                .Where(d => d.ProductId == id)
                .Select(d => d.ProductDescription)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(desc))
            {
                desc = await _db.SOtherProductDetails
                    .Where(d => d.ProductId == id)
                    .Select(d => d.ProductDescription)
                    .FirstOrDefaultAsync();
            }

            // 評分彙總（可用 view：S_v_ProductRatingStats）
            var rating = await _db.SVProductRatingStats
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ProductId == id);

            var vm = new ProductDetailVM
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductType = p.ProductType,
                Price = p.Price,
                CurrencyCode = p.CurrencyCode,
                ProductDescription = desc,
                IsPreorderEnabled = p.IsPreorderEnabled,
                PublishAt = p.PublishAt,
                CoverUrl = imgs.FirstOrDefault() ?? "/images/placeholder-cover.png",
                Gallery = imgs.ToArray(),
                RatingAvg = rating?.RatingAvg ?? 0,
                RatingCount = rating?.RatingCount ?? 0
            };

            return Ok(vm);
        }
        [HttpGet("top-favorites")]
        public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetTopFavorites(int count = 8)
        {
            count = Math.Clamp(count, 1, 24);

            // 先算出收藏數 Top N（沒有 IsDeleted 的情況）
            var topFavQuery = _db.SUserFavorites
                .GroupBy(f => f.ProductId)
                .Select(g => new { ProductId = g.Key, Cnt = g.Count() })
                .OrderByDescending(x => x.Cnt)
                .ThenBy(x => x.ProductId) // 次序穩定
                .Take(count);

            // 用 join 方式把熱門順序帶進來（避免被二段查詢洗掉順序）
            var query =
                from t in topFavQuery
                join p in _db.SProductInfos.AsNoTracking() on t.ProductId equals p.ProductId
                where !p.IsDeleted
                select new ProductCardDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductType = p.ProductType,
                    Price = p.Price,
                    CurrencyCode = p.CurrencyCode,
                    ProductCode = p.SProductCode != null ? p.SProductCode.ProductCode : "",
                    CoverUrl = p.SProductImages
                                .OrderByDescending(i => i.IsPrimary)
                                .ThenBy(i => i.SortOrder)
                                .Select(i => i.ProductimgUrl)
                                .FirstOrDefault() ?? "/images/onlinestoreNOPic/nophoto.jpg",
                    PlatformName = (p.SGameProductDetail != null && p.SGameProductDetail.Platform != null)
                                    ? p.SGameProductDetail.Platform.PlatformName
                                    : null,
                    PeripheralTypeName =
                        (from d in _db.SOtherProductDetails
                         join mt in _db.SMerchTypes on d.MerchTypeId equals mt.MerchTypeId
                         where d.ProductId == p.ProductId && !d.IsDeleted
                         select mt.MerchTypeName).FirstOrDefault(),
                    IsPreorder = p.IsPreorderEnabled
                };

            var items = await query.ToListAsync();

            // 如果熱門清單為空，直接回傳空集合（200）
            if (items.Count == 0) return Ok(Array.Empty<ProductCardDto>());

            return Ok(items);
        }


        //[HttpGet("top-clicks")]
        //public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetTopClicks(int days = 30, int count = 8)
        //{
        //    days = Math.Clamp(days, 1, 180);
        //    count = Math.Clamp(count, 1, 24);
        //    var from = DateTime.UtcNow.AddDays(-days);

        //    var topIds = await _db.SProductClickLog
        //        .Where(c => c.ClickedAt >= from)     // 若欄位名不同，改這行
        //        .GroupBy(c => c.ProductId)
        //        .Select(g => new { ProductId = g.Key, Cnt = g.Count() })
        //        .OrderByDescending(x => x.Cnt)
        //        .Take(count)
        //        .Select(x => x.ProductId)
        //        .ToListAsync();

        //    var items = await _db.SProductInfos.AsNoTracking()
        //        .Where(p => topIds.Contains(p.ProductId) && !p.IsDeleted)
        //        .OrderByDescending(p => p.CreatedAt)
        //        .Take(count)
        //        .Select(p => new ProductCardDto
        //        {
        //            ProductId = p.ProductId,
        //            ProductName = p.ProductName,
        //            ProductType = p.ProductType,
        //            Price = p.Price,
        //            CurrencyCode = p.CurrencyCode,
        //            ProductCode = p.SProductCode != null ? p.SProductCode.ProductCode : "",
        //            CoverUrl = p.SProductImages.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.SortOrder).Select(i => i.ProductimgUrl).FirstOrDefault() ?? "/images/onlinestoreNOPic/nophoto.jpg",
        //            PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null ? p.SGameProductDetail.Platform.PlatformName : null,
        //            PeripheralTypeName = (from d in _db.SOtherProductDetails
        //                                  join mt in _db.SMerchTypes on d.MerchTypeId equals mt.MerchTypeId
        //                                  where d.ProductId == p.ProductId && !d.IsDeleted
        //                                  select mt.MerchTypeName).FirstOrDefault(),
        //            IsPreorder = p.IsPreorderEnabled
        //        })
        //        .ToListAsync();

        //    return Ok(items);
        //}




    }
}