
using GamiPort.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamiPort.Areas.OnlineStore.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;

namespace GamiPort.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    [ApiController]
    [Route("OnlineStore/api/[controller]")] // 前端用 /OnlineStore/api/StoreApi/products
    
    
    public class StoreApiController : ControllerBase

    {
        private readonly GameSpacedatabaseContext _db;
        private readonly IMemoryCache _cache;
        public StoreApiController(GameSpacedatabaseContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        // ===== Query / DTO（延用你檔案的小寫欄位） =====
        public class ProductQuery
        {
            public string? q { get; set; }
            public string? type { get; set; }              // game / notgame
            public int? platformId { get; set; }           // 平台
            public int? merchTypeId { get; set; }          // 周邊類別包含
            public int? excludeMerchTypeId { get; set; }   // 周邊類別排除
            public decimal? priceMin { get; set; }
            public decimal? priceMax { get; set; }
            public int page { get; set; } = 1;
            public int pageSize { get; set; } = 12;
            public string? sort { get; set; }              // price_asc / price_desc / newest / random
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
            public string? MerchTypeName { get; set; }
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
        // 分頁參數防呆：確保 page 與 pageSize 在合理範圍
        // 被叫用處：GetProducts() 一開始就會呼叫 SanitizePaging(q)
        //小抄：這個方法會在 GetProducts() 的第一行被呼叫，所以任何前端傳來的 page/pageSize 都會被這裡「清洗」過
        // ===== Helpers =====
        private static void SanitizePaging(ProductQuery q)
        {
            const int MaxPageSize = 80; // ← 你原本有提到已從 60 改 80
            if (q.page <= 0) q.page = 1;
            if (q.pageSize <= 0 || q.pageSize > MaxPageSize) q.pageSize = 40;
        }
        private static void NormalizePriceRange(ProductQuery q)
        {
            if (q.priceMin.HasValue && q.priceMax.HasValue && q.priceMin > q.priceMax)
                (q.priceMin, q.priceMax) = (q.priceMax, q.priceMin);
        }


        //API 商品資訊讀取 ->商城首頁 Store/Index + 商品列表 Products/Details
        [HttpGet("products")]
        public async Task<ActionResult<PagedResult<ProductCardDto>>> GetProducts([FromQuery] ProductQuery q, [FromQuery] string? tag = null)
        {
            SanitizePaging(q);
            NormalizePriceRange(q);

            var query = _db.SProductInfos
                .AsNoTracking()
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(q.q))
            {
                var keyword = q.q.Trim();
                query = query.Where(p =>
                    p.ProductName.Contains(keyword) ||
                    (p.SProductCode != null && p.SProductCode.ProductCode.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(q.type))
            {
                var t = q.type.Trim().ToLower();
                if (t == "game" || t == "notgame")
                    query = query.Where(p => p.ProductType.ToLower() == t);
            }

            if (q.platformId.HasValue)
            {
                var pid = q.platformId.Value;
                query = query.Where(p => p.SGameProductDetail != null && p.SGameProductDetail.PlatformId == pid);
            }

            if (q.merchTypeId.HasValue)
            {
                query =
                    (from p in query
                     join other in _db.SOtherProductDetails on p.ProductId equals other.ProductId
                     where other.MerchTypeId == q.merchTypeId.Value
                     select p).Distinct();
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

            if (q.priceMin.HasValue) query = query.Where(p => p.Price >= q.priceMin.Value);
            if (q.priceMax.HasValue) query = query.Where(p => p.Price <= q.priceMax.Value);

            if (!string.IsNullOrWhiteSpace(tag))
            {
                // 先用預留的 tag=sale 對應預購旗標；之後你要換成折扣價條件也可
                if (tag == "sale")
                    query = query.Where(p => p.IsPreorderEnabled);
            }

            var total = await query.CountAsync();

            IOrderedQueryable<SProductInfo> orderedQuery;
            if (q.sort == "random")
            {
                var seed = DateOnly.FromDateTime(DateTime.UtcNow).DayNumber;
                orderedQuery = query.OrderBy(p => (p.ProductId ^ seed));
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

            var items = await orderedQuery
                .Skip((q.page - 1) * q.pageSize)
                .Take(q.pageSize)
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
                        .FirstOrDefault() ?? "/images/onlinestoreNOPic/nophoto.jpg",
                    PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null
                        ? p.SGameProductDetail.Platform.PlatformName
                        : null,
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


        // ============================================================================
        // 【商城首頁專用】取得隨機商品清單（輕量版）
        // 說明：
        //  - 用於商城首頁（Store/Index），顯示隨機 N 筆商品。
        //  - 不需要分頁，不需要篩選，只取最基本欄位（給前台卡片展示）。
        //  - 可用於「你可能喜歡」「隨機推薦」「最新上架區」等區塊。
        //  - 預設亂數演算法使用 XOR 方式（依每日日期不同），以避免 Guid.NewGuid() 效能問題。
        // ============================================================================

        [HttpGet("home-random")]
        public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetHomeRandom([FromQuery] int count = 8)
        {
            if (count <= 0) count = 8; if (count > 60) count = 60;
            var seed = DateOnly.FromDateTime(DateTime.UtcNow).DayNumber;

            var items = await _db.SProductInfos.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .OrderBy(p => (p.ProductId ^ seed))
                .Take(count)
                .Select(p => new ProductCardDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductType = p.ProductType,
                    Price = p.Price,
                    CurrencyCode = p.CurrencyCode,
                    ProductCode = p.SProductCode != null ? p.SProductCode.ProductCode : "",
                    CoverUrl = p.SProductImages.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.SortOrder).Select(i => i.ProductimgUrl).FirstOrDefault() ?? "/images/onlinestoreNOPic/nophoto.jpg",
                    PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null ? p.SGameProductDetail.Platform.PlatformName : null,
                    PeripheralTypeName = (
                        from d in _db.SOtherProductDetails
                        join mt in _db.SMerchTypes on d.MerchTypeId equals mt.MerchTypeId
                        where d.ProductId == p.ProductId && !d.IsDeleted
                        select mt.MerchTypeName
                    ).FirstOrDefault(),
                    IsPreorder = p.IsPreorderEnabled
                })
                .ToListAsync();

            return Ok(items);
        }



        // /OnlineStore/api/StoreApi/products/{code}


        //   1) Product
        //   2)Related/ ProductType/ PlatformId/
        // roductDetailDto
        // Purpose: Product detail by code, includes images and related items.
        // Used by: wwwroot/js/onlinestore/product-detail.js (via /api/store/products/{code})
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
                CoverUrl = x.SProductImages.OrderByDescending(img => img.IsPrimary).ThenBy(img => img.SortOrder).Select(img => img.ProductimgUrl!).FirstOrDefault() ?? "/images/onlinestoreNOPic/nophoto.jpg"
            }).ToListAsync();

            product.Related = related;
            return Ok(product);
        }

      
        //   - type: sales / click / favorite
        //   - period: daily / weekly / monthly / quarterly / yearly

        // Enumerable<ProductCardDto>
        // Purpose: Top-N rankings by sales/click/favorite and period.
        // Used by: Views/Store/Index.cshtml, wwwroot/js/onlinestore/rankings*.js (via /api/store/rankings)
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
                        .FirstOrDefault() ?? "/images/onlinestoreNOPic/nophoto.jpg",
                    PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null
                        ? p.SGameProductDetail.Platform.PlatformName
                        : null,
                    IsPreorder = p.IsPreorderEnabled
                })
                .ToListAsync();

            return Ok(items);
        }

       
        // Purpose: Upsert rating for a product (user-provided userId).
        // Used by: (no direct references found)
        [HttpPost("products/{id}/rate")]
        public async Task<IActionResult> RateProduct(int id, [FromBody] RateRequest req)
        {
            // 1) 
            if (req == null) return BadRequest("Empty payload.");
            if (req.UserId <= 0) return BadRequest("Invalid user.");
            if (req.Rating < 1 || req.Rating > 5) return BadRequest("Rating must be 1~5.");

            // 2)
            var rating = await _db.SProductRatings
                .FirstOrDefaultAsync(r => r.ProductId == id && r.UserId == req.UserId);

            if (rating == null)
            {
               
                rating = new SProductRating
                {
                    ProductId = id,
                    UserId = req.UserId,
                    Rating = req.Rating,
                    ReviewText = req.Review
                };
                _db.SProductRatings.Add(rating);
            }
            else
            {
             
                rating.Rating = req.Rating;
                rating.ReviewText = req.Review;
            }

            await _db.SaveChangesAsync();
            return Ok(new { success = true });
        }


        // Purpose: Track product click (in-memory throttle/counter for now).
        // Used by: Views/Store/Index.cshtml (sendBeacon/fetch)
        [HttpPost("products/{id}/click")]
        public IActionResult TrackClick(int id, [FromBody] int? userId)
        {
            // In-memory click logging with simple dedupe window
            var ip = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "-";
            var who = userId?.ToString() ?? ip;
            var bucket = DateTime.UtcNow.ToString("yyyyMMddHHmm"); // 1-min bucket
            var key = $"click:{id}:{who}:{bucket}";

            if (!_cache.TryGetValue(key, out _))
            {
                _cache.Set(key, 1, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2) });
                var totalKey = $"click-total:{id}";
                var total = _cache.GetOrCreate(totalKey, e => { e.SlidingExpiration = TimeSpan.FromHours(1); return 0; });
                _cache.Set(totalKey, (int)total + 1);
            }
            return Ok();
        }


        // Purpose: Add product to current user's cart.
        // Used by: (no direct references found)
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



        // ===== 商城首頁：最新上架 =====
        [HttpGet("latest")]
        public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetLatest([FromQuery] int take = 12)
        {
            if (take <= 0 || take > 60) take = 12;

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
                    CoverUrl = p.SProductImages.OrderByDescending(img => img.IsPrimary).ThenBy(img => img.SortOrder).Select(img => img.ProductimgUrl).FirstOrDefault() ?? "/images/onlinestoreNOPic/nophoto.jpg",
                    PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null ? p.SGameProductDetail.Platform.PlatformName : null,
                    IsPreorder = p.IsPreorderEnabled
                })
                .ToListAsync();

            return Ok(items);
        }

        // GET: /OnlineStore/api/StoreApi/rankings/official
        // 依據官方彙總表 S_OfficialStoreRankings 取榜單（可選 period 與 date）
        // Query 範例：?type=purchase&period=daily&date=2025-11-01&take=10
        // 備註：目前支援 type = purchase / click / favorite；period = daily/weekly/monthly/quarterly/yearly
        [HttpGet("rankings/official")]
        public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetRankingsFromOfficial(
            [FromQuery] string type = "purchase",
            [FromQuery] string period = "daily",
            [FromQuery] DateTime? date = null,
            [FromQuery] int take = 10)
        {
            // --- 區間/數量防呆 ---
            if (take <= 0 || take > 60) take = 10;

            // --- 快取鍵（避免重複查詢）---
            var cacheKey = $"official:{type}:{period}:{(date?.Date.ToString("yyyyMMdd") ?? "latest")}:{take}";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<ProductCardDto> cached))
                return Ok(cached);

            // --- 將 period / type 正規化到內部欄位 ---
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

            // --- 目標日期（date 未給時，取該 period+metric 的最新一筆日期）---
            DateOnly targetDateOnly;
            if (date.HasValue)
            {
                targetDateOnly = DateOnly.FromDateTime(date.Value.Date);
            }
            else
            {
                var latest = await _db.SOfficialStoreRankings.AsNoTracking()
                    .Where(r => r.PeriodType == periodType && r.RankingMetric == metric)
                    .OrderByDescending(r => r.RankingDate)
                    .Select(r => r.RankingDate)
                    .FirstOrDefaultAsync();

                // 沒資料則用今日（避免 null）
                targetDateOnly = latest == default
                    ? DateOnly.FromDateTime(DateTime.Today)
                    : latest;
            }

            // --- 取榜單（Top N ProductId + 排名），再 Join 商品主檔 ---
            var ranked =
                _db.SOfficialStoreRankings.AsNoTracking()
                    .Where(r => r.PeriodType == periodType
                                && r.RankingMetric == metric
                                && r.RankingDate == targetDateOnly)
                    .OrderBy(r => r.RankingPosition)
                    .Take(take)
                    .Select(r => new { r.ProductId, r.RankingPosition });

            var query =
                from r in ranked
                join p in _db.SProductInfos.AsNoTracking().Where(p => !p.IsDeleted)
                    on r.ProductId equals p.ProductId
                orderby r.RankingPosition
                select p;

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
                        .FirstOrDefault() ?? "/images/onlinestoreNOPic/nophoto.jpg",
                    PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null
                        ? p.SGameProductDetail.Platform.PlatformName
                        : null,
                    IsPreorder = p.IsPreorderEnabled,
                    // 周邊類別名稱（若有）
                    PeripheralTypeName = (
                        from d in _db.SOtherProductDetails
                        join mt in _db.SMerchTypes on d.MerchTypeId equals mt.MerchTypeId
                        where d.ProductId == p.ProductId && !d.IsDeleted
                        select mt.MerchTypeName
                    ).FirstOrDefault(),
                })
                .ToListAsync();

            // --- 寫入快取 ---
            _cache.Set(cacheKey, items, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // demo: 5 分鐘
            });

            // --- 重要：回傳 OK，並結束方法（避免 CS0161 / CS1513）---
            return Ok(items);
        }




        // GET: /OnlineStore/api/StoreApi/GetBrowseCards
        // ??鋆? template嚗???賢?批?冽頝臬?
        // 隤芣?嚗汗?典???殷?蝪∠? VM嚗?靘????汗??
        // 頝舐嚗ET /OnlineStore/api/StoreApi/GetBrowseCards
        // Purpose: Simple browse cards for home/browse page (VM list).
        // Used by: Areas/OnlineStore/Views/Store/Browse.cshtml
        [HttpGet("GetBrowseCards")]
            public async Task<ActionResult<IEnumerable<ProductCardVM>>> GetBrowseCards()
            {
                // ?芸??芸?文???
                var query =
                    from p in _db.SProductInfos
                    where !p.IsDeleted
                    orderby p.ProductId descending
                    select new ProductCardVM
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        ProductType = p.ProductType,
                        Price = p.Price,
                        CurrencyCode = p.CurrencyCode,
                        // ?蜓?????嗆活摨?嚗?策 placeholder
                        CoverUrl = _db.SProductImages
                            .Where(img => img.ProductId == p.ProductId)
                            .OrderByDescending(img => img.IsPrimary)
                            .ThenBy(img => img.SortOrder)
                            .Select(img => img.ProductimgUrl)
                            .FirstOrDefault() ?? "/images/placeholder-cover.png"
                    };

                var data = await query.AsNoTracking().ToListAsync();
                return Ok(data);
            }


                  // Purpose: Product detail by numeric id (VM with desc/gallery/rating stats).
            // Used by: (view calls '/OnlineStore/api/StoreApi/product-detail/{id}' — route mismatch; consider alias)
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

                // ?ㄐ?陛?格??膩靘???嚗??脤?霈 S_GameProductDetails嚗摰? S_OtherProductDetails
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

                // 閰?敶蜇嚗??view嚗_v_ProductRatingStats嚗?
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

            //??璁?憭??憟踝?靽??梢???嚗?

            // 隤芣?嚗??嗉? Top N嚗誑 SUserFavorites 蝯梯?嚗?
            // 頝舐嚗ET /OnlineStore/api/StoreApi/top-favorites?count=8
            // Purpose: Top favorites (most favorited products).
            // Used by: (no direct references found)
            [HttpGet("top-favorites")]
            public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetTopFavorites(int count = 8)
            {
                count = Math.Clamp(count, 1, 24);

                // ???箸? Top N嚗???IsDeleted ??瘜?
                var topFavQuery = _db.SUserFavorites
                    .GroupBy(f => f.ProductId)
                    .Select(g => new { ProductId = g.Key, Cnt = g.Count() })
                    .OrderByDescending(x => x.Cnt)
                    .ThenBy(x => x.ProductId) // 甈∪?蝛拙?
                    .Take(count);

                // ??join ?孵?????撣園脖?嚗?◤鈭挾?亥岷瘣???嚗?
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

                // 憒??梢?皜?箇征嚗?亙??喟征??嚗?00嚗?
                if (items.Count == 0) return Ok(Array.Empty<ProductCardDto>());

                return Ok(items);
            }


            //?犖?冽?末

            // ?芸??交??撌脣??典停?湔??200嚗????嚗?
            // 隤芣?嚗??交???芰?嚗歇摮?? duplicated=true嚗?
            // 頝舐嚗OST /OnlineStore/api/StoreApi/favorites/{productId}
            // ?批捆嚗ody = userId嚗?蝬??餃頨怠?嚗?皜祈岫?券?
            // Purpose: Add to favorites (idempotent); body = userId.
            // Used by: (no direct references found)
            [HttpPost("favorites/{productId:int}")]
            public async Task<IActionResult> AddFavorite(int productId, [FromBody] int userId)
            {
                if (userId <= 0) return BadRequest("userId 敹?憭扳 0");

                var exists = await _db.SUserFavorites.FindAsync(userId, productId); // 銴?銝駁 (UserId, ProductId)
                if (exists != null)
                {
                    return Ok(new { success = true, duplicated = true });
                }

                _db.SUserFavorites.Add(new SUserFavorite
                {
                    UserId = userId,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow // 銋銝神嚗? DB default ?芸楛憛?
                });

                await _db.SaveChangesAsync();
                return Ok(new { success = true });
            }

            // 靘?user_id ????????殷????撠 DTO嚗?
            // 隤芣?嚗?敺蝙?刻????
            // 頝舐嚗ET /OnlineStore/api/StoreApi/favorites?userId=xxx
            // Purpose: Get user's favorite list by userId query.
            // Used by: Areas/OnlineStore/Views/Member/Favorites.cshtml, Areas/OnlineStore/Views/Browse/Index.cshtml
            [HttpGet("favorites")]
            public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetMyFavorites([FromQuery] int userId)
            {
                if (userId <= 0) return BadRequest("userId 敹?憭扳 0");

                var favIds = await _db.SUserFavorites
                    .Where(f => f.UserId == userId)
                    .Select(f => f.ProductId)
                    .ToListAsync();

                if (favIds.Count == 0) return Ok(Array.Empty<ProductCardDto>());

                var items = await _db.SProductInfos
                    .AsNoTracking()
                    .Where(p => favIds.Contains(p.ProductId) && !p.IsDeleted)
                    .Select(p => new ProductCardDto
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
                    })
                    .ToListAsync();

                return Ok(items);
            }
        /// <summary>
        /// 取得全部商品（不分頁）— 請審慎使用，預設最多 1000 筆（可用 ?max= 調整，硬上限 5000）
        /// GET /OnlineStore/api/StoreApi/products/all?max=1000&sort=newest
        /// sort: newest(預設) / price_asc / price_desc
        /// </summary>
        [HttpGet("products/all")]
        [ProducesResponseType(typeof(IEnumerable<ProductCardDto>), 200)]
        public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetAllProducts(
            [FromQuery] int max = 1000,
            [FromQuery] string? sort = "newest",
            CancellationToken ct = default)
        {
            // 上限防呆（避免一次撈爆記憶體）
            if (max <= 0) max = 1000;
            if (max > 5000) max = 5000;

            var baseQuery = _db.SProductInfos
                .AsNoTracking()
                .Where(p => !p.IsDeleted);

            IOrderedQueryable<SProductInfo> ordered = sort switch
            {
                "price_asc" => baseQuery.OrderBy(p => p.Price),
                "price_desc" => baseQuery.OrderByDescending(p => p.Price),
                _ => baseQuery.OrderByDescending(p => p.CreatedAt) // newest
            };

            var items = await ordered
                .Take(max)
                .Select(p => new ProductCardDto
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
                    PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null
                                    ? p.SGameProductDetail.Platform.PlatformName
                                    : null,
                    PeripheralTypeName = (
                        from d in _db.SOtherProductDetails
                        join mt in _db.SMerchTypes on d.MerchTypeId equals mt.MerchTypeId
                        where d.ProductId == p.ProductId && !d.IsDeleted
                        select mt.MerchTypeName
                    ).FirstOrDefault(),
                    IsPreorder = p.IsPreorderEnabled
                })
                .ToListAsync(ct);

            return Ok(items);
        }


        //[HttpGet("top-clicks")]
        //public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetTopClicks(int days = 30, int count = 8)
        //{
        //    days = Math.Clamp(days, 1, 180);
        //    count = Math.Clamp(count, 1, 24);
        //    var from = DateTime.UtcNow.AddDays(-days);

        //    var topIds = await _db.SProductClickLog
        //        .Where(c => c.ClickedAt >= from)     // ?交?雿?銝?嚗??
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



