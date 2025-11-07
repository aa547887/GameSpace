using GamiPort.Areas.OnlineStore.DTO.Store;
using GamiPort.Areas.OnlineStore.Services.store.Abstractions;
using GamiPort.Areas.OnlineStore.ViewModels;
using GamiPort.Models;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.OnlineStore.Services.store.Application
{
	/// <summary>
	/// 線上商店查詢/操作服務實作。
	/// </summary>
	public class StoreService : IStoreService
	{
		private readonly GameSpacedatabaseContext _db;
		public StoreService(GameSpacedatabaseContext db) => _db = db;

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

		/// <inheritdoc />
		public async Task<GamiPort.Areas.OnlineStore.DTO.Store.PagedResult<ProductCardDto>> GetProducts(ProductQuery q, string? tag, string? productType, string? platform = null, string? genre = null)
		{
			SanitizePaging(q);
			NormalizePriceRange(q);
			
			// [OPTIMIZED] Added Include() to prevent N+1 queries for related data.
			var query = _db.SProductInfos
				.AsNoTracking()
				.Include(p => p.SProductCode)
				.Include(p => p.SProductImages)
				.Include(p => p.SGameProductDetail).ThenInclude(d => d.Platform)
				.Where(p => !p.IsDeleted);

			// 如果僅傳入名稱（相容舊連結），轉成對應的 Id
			if (!q.platformId.HasValue && !string.IsNullOrWhiteSpace(platform))
			{
				var pnorm = platform.Trim();
				var pid = await _db.SPlatforms.AsNoTracking()
					.Where(p => p.PlatformName == pnorm || EF.Functions.Like(p.PlatformName, "%" + pnorm + "%"))
					.Select(p => p.PlatformId)
					.FirstOrDefaultAsync();
				if (pid != 0) q.platformId = pid;
			}
			if (!q.genreId.HasValue && !string.IsNullOrWhiteSpace(genre))
			{
				var gnorm = genre.Trim();
				var gid = await _db.SGameGenres.AsNoTracking()
					.Where(g => g.GenreName == gnorm || EF.Functions.Like(g.GenreName, "%" + gnorm + "%"))
					.Select(g => g.GenreId)
					.FirstOrDefaultAsync();
				if (gid != 0) q.genreId = gid;
			}

			// 關鍵字搜尋（商品名）
			if (!string.IsNullOrWhiteSpace(q.q))
			{
				var kw = q.q.Trim();
				query = query.Where(p => EF.Functions.Like(p.ProductName, "%" + kw + "%"));
			}

			// 商品種類：優先用 q.type，其次相容舊的 productType 參數
			string? typeFilter = null;
			if (!string.IsNullOrWhiteSpace(q.type)) typeFilter = q.type!.Trim();
			else if (!string.IsNullOrWhiteSpace(productType)) typeFilter = productType!.Trim();

			if (!string.IsNullOrWhiteSpace(typeFilter))
			{
				var tf = typeFilter!.Trim().ToLower();
				if (tf == "game")
				{
					// 以關聯存在為準（不依賴 ProductType 字串）
					query = query.Where(p => _db.SGameProductDetails.Any(d => d.ProductId == p.ProductId && d.IsDeleted == false));
				}
				else if (tf == "notgame")
				{
					query = query.Where(p => _db.SOtherProductDetails.Any(d => d.ProductId == p.ProductId && d.IsDeleted == false));
				}
			}


			// 平台過濾（僅 game）使用子查詢避免導航屬性翻譯不一致
			if (q.platformId.HasValue)
			{
				int pid = q.platformId.Value;
				query = query.Where(p => _db.SGameProductDetails.Any(d => d.ProductId == p.ProductId && d.PlatformId == pid && d.IsDeleted == false));
			}

			// 類型過濾（僅 game）
			if (q.genreId.HasValue)
			{
				int gid = q.genreId.Value;
				query = query.Where(p => p.Genres.Any(g => g.GenreId == gid));
			}

			// 周邊類型（僅 notgame）：包含或排除
			if (q.merchTypeId.HasValue)
			{
				int mid = q.merchTypeId.Value;
				query = query.Where(p => _db.SOtherProductDetails.Any(d => d.ProductId == p.ProductId && !d.IsDeleted && d.MerchTypeId == mid));
			}
			if (q.excludeMerchTypeId.HasValue)
			{
				int exid = q.excludeMerchTypeId.Value;
				query = query.Where(p => !_db.SOtherProductDetails.Any(d => d.ProductId == p.ProductId && !d.IsDeleted && d.MerchTypeId == exid));
			}

			// Supplier filter (for game and notgame)
			if (q.supplierId.HasValue)
			{
				int sid = q.supplierId.Value;
				query = query.Where(p =>
					_db.SGameProductDetails.Any(d => d.ProductId == p.ProductId && !d.IsDeleted && d.SupplierId == sid)
					|| _db.SOtherProductDetails.Any(d => d.ProductId == p.ProductId && !d.IsDeleted && d.SupplierId == sid)
				);
			}

			// 價格區間
			if (q.priceMin.HasValue) query = query.Where(p => p.Price >= q.priceMin.Value);
			if (q.priceMax.HasValue) query = query.Where(p => p.Price <= q.priceMax.Value);
			// 先計算過濾後的總筆數
			var total = await query.CountAsync();

			// 隨機排序改為隨機分頁，避免 Guid.NewGuid() 在 SQL 不可轉譯
			if (string.Equals(q.sort, "random", StringComparison.OrdinalIgnoreCase) && total > 0)
			{
				var pageSize = Math.Clamp(q.pageSize, 1, 60);
				var maxStart = Math.Max(0, total - pageSize);
				var start = Random.Shared.Next(0, maxStart + 1);

				var randomItems = await query
					.OrderBy(p => p.ProductId)
					.Skip(start)
					.Take(pageSize)
					.Select(p => new ProductCardDto
					{
						ProductId = p.ProductId,
						ProductName = p.ProductName.Trim(),
						ProductType = (p.ProductType ?? "").Trim(),
						Price = p.Price,
						CurrencyCode = (p.CurrencyCode ?? "").Trim(),
						ProductCode = p.SProductCode != null ? (p.SProductCode.ProductCode ?? "").Trim() : "",
						CoverUrl = p.SProductImages
							.OrderByDescending(img => img.IsPrimary)
							.ThenBy(img => img.SortOrder)
							.Select(img => img.ProductimgUrl)
							.FirstOrDefault() ?? "/images/onlinestoreNOPic/nophoto.jpg",
						PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null
							? p.SGameProductDetail.Platform.PlatformName
							: null,
						MerchTypeName = (
							from d in _db.SOtherProductDetails
							join mt in _db.SMerchTypes on d.MerchTypeId equals mt.MerchTypeId
							where d.ProductId == p.ProductId && !d.IsDeleted
							select mt.MerchTypeName
						).FirstOrDefault(),
						IsPreorder = p.IsPreorderEnabled
					})
					.ToListAsync();

				return new GamiPort.Areas.OnlineStore.DTO.Store.PagedResult<ProductCardDto>
				{
					page = 1,
					pageSize = randomItems.Count,
					totalCount = total,
					items = randomItems
				};
			}

			// 其餘排序維持原本邏輯
			IOrderedQueryable<SProductInfo> orderedQuery = q.sort switch
			{
				"price_asc" => query.OrderBy(p => p.Price),
				"price_desc" => query.OrderByDescending(p => p.Price),
				_ => query.OrderByDescending(p => p.CreatedAt)
			};

			var items = await orderedQuery
				.Skip((q.page - 1) * q.pageSize)
				.Take(q.pageSize)
				.Select(p => new ProductCardDto
				{
					ProductId = p.ProductId,
					ProductName = p.ProductName.Trim(),
					ProductType = (p.ProductType ?? "").Trim(),
					Price = p.Price,
					CurrencyCode = (p.CurrencyCode ?? "").Trim(),
					ProductCode = p.SProductCode != null ? (p.SProductCode.ProductCode ?? "").Trim() : "",
					CoverUrl = p.SProductImages
						.OrderByDescending(img => img.IsPrimary)
						.ThenBy(img => img.SortOrder)
						.Select(img => img.ProductimgUrl)
						.FirstOrDefault() ?? "/images/onlinestoreNOPic/nophoto.jpg",
					PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null
						? p.SGameProductDetail.Platform.PlatformName
						: null,
					MerchTypeName = (
						from d in _db.SOtherProductDetails
						join mt in _db.SMerchTypes on d.MerchTypeId equals mt.MerchTypeId
						where d.ProductId == p.ProductId && !d.IsDeleted
						select mt.MerchTypeName
					).FirstOrDefault(),
					IsPreorder = p.IsPreorderEnabled
				})
				.ToListAsync();

			return new GamiPort.Areas.OnlineStore.DTO.Store.PagedResult<ProductCardDto>
			{
				page = q.page,
				pageSize = q.pageSize,
				totalCount = total,
				items = items
			};
		}
		/// <summary>
		/// 取得商品完整資料清單（分頁版）
		/// </summary>
		public async Task<GamiPort.Areas.OnlineStore.DTO.Store.PagedResult<GamiPort.Areas.OnlineStore.DTO.Store.ProductFullDto>> GetProductsFull(ProductQuery q, string? tag, string? productType)
		{
			SanitizePaging(q);
			NormalizePriceRange(q);

			var query = _db.SProductInfos.AsNoTracking().Where(p => !p.IsDeleted);

			if (!string.IsNullOrWhiteSpace(productType))
			{
				var pt = productType.Trim().ToLower();
				if (pt == "game")
				{
					// 以是否存在遊戲明細判斷，避免字串大小寫/空白問題
					query = query.Where(p => _db.SGameProductDetails.Any(d => d.ProductId == p.ProductId && d.IsDeleted == false));
				}
				else if (pt == "notgame")
				{
					query = query.Where(p => _db.SOtherProductDetails.Any(d => d.ProductId == p.ProductId && d.IsDeleted == false));
				}
			}

			// 先計算過濾後的總筆數
			var total = await query.CountAsync();

			// 隨機排序改為隨機分頁
			if (string.Equals(q.sort, "random", StringComparison.OrdinalIgnoreCase) && total > 0)
			{
				var pageSize = Math.Clamp(q.pageSize, 1, 60);
				var maxStart = Math.Max(0, total - pageSize);
				var start = Random.Shared.Next(0, maxStart + 1);

				var randomItems = await query
					.OrderBy(p => p.ProductId)
					.Skip(start)
					.Take(pageSize)
					.Select(p => new GamiPort.Areas.OnlineStore.DTO.Store.ProductFullDto
					{
						ProductId = p.ProductId,
						ProductName = p.ProductName.Trim(),
						ProductType = (p.ProductType ?? "").Trim(),
						Price = p.Price,
						CurrencyCode = (p.CurrencyCode ?? "TWD").Trim().ToUpper(),
						ProductCode = p.SProductCode != null ? (p.SProductCode.ProductCode ?? string.Empty).Trim() : string.Empty,
						IsPreorder = p.IsPreorderEnabled,
						IsPhysical = p.IsPhysical,
						CreatedAt = p.CreatedAt,
						UpdatedAt = p.UpdatedAt,
						PublishAt = p.PublishAt,
						UnpublishAt = p.UnpublishAt,
						SafetyStock = p.SafetyStock,
						PlatformId = p.SGameProductDetail != null ? p.SGameProductDetail.PlatformId : (int?)null,
						PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null
							? p.SGameProductDetail.Platform.PlatformName
							: null,
						PeripheralTypeName = (
							from d in _db.SOtherProductDetails
							join mt in _db.SMerchTypes on d.MerchTypeId equals mt.MerchTypeId
							where d.ProductId == p.ProductId && !d.IsDeleted
							select mt.MerchTypeName
						).FirstOrDefault(),
						ProductDescription = (
							(from gd in _db.SGameProductDetails
							 where gd.ProductId == p.ProductId
							 select gd.ProductDescription).FirstOrDefault()
						) ?? (
							(from od in _db.SOtherProductDetails
							 where od.ProductId == p.ProductId
							 select od.ProductDescription).FirstOrDefault()
						),
						Images = p.SProductImages
							.OrderByDescending(img => img.IsPrimary)
							.ThenBy(img => img.SortOrder)
							.Select(img => img.ProductimgUrl)
							.ToList(),
						Genres = p.Genres.Select(g => g.GenreName).ToList(),
						RatingAvg = _db.SVProductRatingStats
							.Where(r => r.ProductId == p.ProductId)
							.Select(r => r.RatingAvg)
							.FirstOrDefault(),
						RatingCount = _db.SVProductRatingStats
							.Where(r => r.ProductId == p.ProductId)
							.Select(r => r.RatingCount)
							.FirstOrDefault(),
					})
					.ToListAsync();

				return new GamiPort.Areas.OnlineStore.DTO.Store.PagedResult<GamiPort.Areas.OnlineStore.DTO.Store.ProductFullDto>
				{
					page = 1,
					pageSize = randomItems.Count,
					totalCount = total,
					items = randomItems
				};
			}

			IOrderedQueryable<SProductInfo> orderedQuery = q.sort switch
			{
				"price_asc" => query.OrderBy(p => p.Price),
				"price_desc" => query.OrderByDescending(p => p.Price),
				_ => query.OrderByDescending(p => p.CreatedAt)
			};

			var items = await orderedQuery
				.Skip((q.page - 1) * q.pageSize)
				.Take(q.pageSize)
				.Select(p => new GamiPort.Areas.OnlineStore.DTO.Store.ProductFullDto
				{
					ProductId = p.ProductId,
					ProductName = p.ProductName,
					ProductType = p.ProductType,
					Price = p.Price,
					CurrencyCode = p.CurrencyCode,
					ProductCode = p.SProductCode != null ? p.SProductCode.ProductCode : string.Empty,
					IsPreorder = p.IsPreorderEnabled,
					IsPhysical = p.IsPhysical,
					CreatedAt = p.CreatedAt,
					UpdatedAt = p.UpdatedAt,
					PublishAt = p.PublishAt,
					UnpublishAt = p.UnpublishAt,
					SafetyStock = p.SafetyStock,
					PlatformId = p.SGameProductDetail != null ? p.SGameProductDetail.PlatformId : (int?)null,
					PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null
						? p.SGameProductDetail.Platform.PlatformName
						: null,
					PeripheralTypeName = (
						from d in _db.SOtherProductDetails
						join mt in _db.SMerchTypes on d.MerchTypeId equals mt.MerchTypeId
						where d.ProductId == p.ProductId && !d.IsDeleted
						select mt.MerchTypeName
					).FirstOrDefault(),
					ProductDescription = (
						(from gd in _db.SGameProductDetails
						 where gd.ProductId == p.ProductId
						 select gd.ProductDescription).FirstOrDefault()
					) ?? (
						(from od in _db.SOtherProductDetails
						 where od.ProductId == p.ProductId
						 select od.ProductDescription).FirstOrDefault()
					),
					Images = p.SProductImages
						.OrderByDescending(img => img.IsPrimary)
						.ThenBy(img => img.SortOrder)
						.Select(img => img.ProductimgUrl)
						.ToList(),
					Genres = p.Genres.Select(g => g.GenreName).ToList(),
					RatingAvg = _db.SVProductRatingStats
						.Where(r => r.ProductId == p.ProductId)
						.Select(r => r.RatingAvg)
						.FirstOrDefault(),
					RatingCount = _db.SVProductRatingStats
						.Where(r => r.ProductId == p.ProductId)
						.Select(r => r.RatingCount)
						.FirstOrDefault(),
				})
				.ToListAsync();

			return new GamiPort.Areas.OnlineStore.DTO.Store.PagedResult<GamiPort.Areas.OnlineStore.DTO.Store.ProductFullDto>
			{
				page = q.page,
				pageSize = q.pageSize,
				totalCount = total,
				items = items
			};
		}

		public async Task<ProductDetailDto?> GetProductByCode(string code)
		{
			if (string.IsNullOrWhiteSpace(code)) return null;
			code = code.Trim();

			var product = await _db.SProductInfos.AsNoTracking()
				.Where(p => !p.IsDeleted && p.SProductCode != null && p.SProductCode.ProductCode == code)
				.Select(p => new ProductDetailDto
				{
					ProductId = p.ProductId,
					ProductName = p.ProductName.Trim(),
					ProductType = (p.ProductType ?? "").Trim(),
					Price = p.Price,
					CurrencyCode = (p.CurrencyCode ?? "TWD").Trim().ToUpper(),
					ProductCode = (p.SProductCode!.ProductCode ?? "").Trim(),
					IsPreorder = p.IsPreorderEnabled,
					PlatformId = p.SGameProductDetail != null ? p.SGameProductDetail.PlatformId : (int?)null,
					PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null ? p.SGameProductDetail.Platform.PlatformName : null,
					Images = p.SProductImages.OrderByDescending(img => img.IsPrimary).ThenBy(img => img.SortOrder).Select(img => img.ProductimgUrl!).ToList(),
				}).FirstOrDefaultAsync();

			if (product == null) return null;

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
			return product;
		}

		public async Task<List<ProductCardDto>> GetRankings(string type, string period, int take)
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
					ProductName = p.ProductName.Trim(),
					ProductType = (p.ProductType ?? "").Trim(),
					Price = p.Price,
					CurrencyCode = (p.CurrencyCode ?? "TWD").Trim().ToUpper(),
					ProductCode = p.SProductCode != null ? (p.SProductCode.ProductCode ?? "").Trim() : "",
					CoverUrl = p.SProductImages
						.OrderByDescending(img => img.IsPrimary)
						.ThenBy(img => img.SortOrder)
						.Select(img => img.ProductimgUrl)
						.FirstOrDefault() ?? "/images/onlinestoreNOPic/nophoto.jpg",
					IsPreorder = p.IsPreorderEnabled
				})
				.ToListAsync();

			return items;
		}

		/// <inheritdoc />
		public async Task<bool> ToggleFavorite(int productId, int userId)
		{
			var existing = await _db.SUserFavorites.FindAsync(userId, productId);
			if (existing != null)
			{
				_db.SUserFavorites.Remove(existing);
				await _db.SaveChangesAsync();
				return false;
			}
			else
			{
				_db.SUserFavorites.Add(new SUserFavorite { UserId = userId, ProductId = productId });
				await _db.SaveChangesAsync();
				return true;
			}
		}

		/// <inheritdoc />
		public async Task RateProduct(int productId, RateRequest req)
		{
			var rating = await _db.SProductRatings.FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == req.UserId);
			if (rating == null)
			{
				rating = new SProductRating { ProductId = productId, UserId = req.UserId };
				_db.SProductRatings.Add(rating);
			}
			rating.Rating = req.Rating;
			rating.ReviewText = req.Review;
			await _db.SaveChangesAsync();
		}

		/// <inheritdoc />
		public Task TrackClick(int productId, int? userId)
		{
			// ToDo: Implement click logging to the database if required.
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public async Task AddToCart(AddToCartRequest req, int userId)
		{
			if (req.Qty <= 0) throw new ArgumentException("Quantity must be positive.");

			var cart = await _db.SoCarts.FirstOrDefaultAsync(c => c.UserId == userId);
			if (cart == null)
			{
				cart = new SoCart { UserId = userId, CreatedAt = DateTime.UtcNow };
				_db.SoCarts.Add(cart);
				await _db.SaveChangesAsync();
			}

			var cartItem = await _db.SoCartItems.FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.ProductId == req.ProductId);

			if (cartItem != null)
			{
				cartItem.Qty += req.Qty;
			}
			else
			{
				var product = await _db.SProductInfos.FindAsync(req.ProductId);
				if (product == null) throw new InvalidOperationException("Product not found.");

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
		}

		/// <inheritdoc />
		public async Task<List<ProductCardDto>> GetLatest(int take)
		{
			if (take <= 0 || take > 60) take = 5;

			var items = await _db.SProductInfos.AsNoTracking()
				.Where(p => !p.IsDeleted)
								.OrderByDescending(p => p.CreatedAt)
								.Take(take)
								.Select(p => new ProductCardDto
								{
									ProductId = p.ProductId,
									ProductName = p.ProductName.Trim(),
									ProductType = (p.ProductType ?? "").Trim(),
									Price = p.Price,
									CurrencyCode = (p.CurrencyCode ?? "").Trim(),
									ProductCode = p.SProductCode != null ? (p.SProductCode.ProductCode ?? "").Trim() : "",
									CoverUrl = p.SProductImages.OrderByDescending(img => img.IsPrimary).ThenBy(img => img.SortOrder).Select(img => img.ProductimgUrl).FirstOrDefault() ?? "",
									PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null ? p.SGameProductDetail.Platform.PlatformName : null,
									IsPreorder = p.IsPreorderEnabled
								})
				.ToListAsync();

			return items;
		}

		public async Task<List<ProductCardDto>> GetRankingsFromOfficial(string type, string period, DateTime? date, int take)
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

				targetDateOnly = latest == default ? DateOnly.FromDateTime(DateTime.Today) : latest;
			}

			var ranked = _db.SOfficialStoreRankings.AsNoTracking()
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

			var items = await query.Select(p => new ProductCardDto
			{
				ProductId = p.ProductId,
				ProductName = p.ProductName.Trim(),
				ProductType = (p.ProductType ?? "").Trim(),
				Price = p.Price,
				CurrencyCode = (p.CurrencyCode ?? "").Trim(),
				ProductCode = p.SProductCode != null ? (p.SProductCode.ProductCode ?? "").Trim() : "",
				CoverUrl = p.SProductImages
					.OrderByDescending(img => img.IsPrimary)
					.ThenBy(img => img.SortOrder)
					.Select(img => img.ProductimgUrl)
					.FirstOrDefault() ?? "",
				PlatformName = p.SGameProductDetail != null && p.SGameProductDetail.Platform != null
										? p.SGameProductDetail.Platform.PlatformName
										: null,
				PeripheralTypeName =
										(from d in _db.SOtherProductDetails
										 join mt in _db.SMerchTypes on d.MerchTypeId equals mt.MerchTypeId
										 where d.ProductId == p.ProductId && !d.IsDeleted
										 select mt.MerchTypeName).FirstOrDefault(),
				IsPreorder = p.IsPreorderEnabled,
			}).ToListAsync();

			return items;
		}

		public async Task<List<ProductCardVM>> GetBrowseCards()
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
						.OrderByDescending(img => img.IsPrimary)
						.ThenBy(img => img.SortOrder)
						.Select(img => img.ProductimgUrl)
						.FirstOrDefault() ?? "/images/placeholder-cover.png"
				};

			var data = await query.AsNoTracking().ToListAsync();
			return data;
		}

		public async Task<ProductDetailVM?> GetProductDetailVM(string productCode)
		{
			if (string.IsNullOrWhiteSpace(productCode)) return null;

			// [OPTIMIZED] Refactored to use a single, comprehensive query to fetch all data at once.
			var vm = await _db.SProductInfos
				.AsNoTracking()
				.Where(p => p.SProductCode != null && p.SProductCode.ProductCode == productCode && !p.IsDeleted)
				.Select(p => new ProductDetailVM
				{
					ProductId = p.ProductId,
					ProductCode = p.SProductCode.ProductCode,
					ProductName = p.ProductName.Trim(),
					ProductType = p.ProductType.Trim(),
					Price = p.Price,
					CurrencyCode = p.CurrencyCode.Trim(),
					IsPreorderEnabled = p.IsPreorderEnabled,
					PublishAt = p.PublishAt,
					// Combine descriptions from both game and other details
					ProductDescription = p.SGameProductDetail.ProductDescription ?? _db.SOtherProductDetails.Where(o => o.ProductId == p.ProductId).Select(o => o.ProductDescription).FirstOrDefault(),
					// Eagerly load all images and then select the cover
					Gallery = p.SProductImages.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.SortOrder).Select(i => i.ProductimgUrl).ToArray(),
					
					// Game-specific details
					PlatformName = p.SGameProductDetail.Platform.PlatformName,
					DownloadLink = p.SGameProductDetail.DownloadLink,
					GenreName = p.Genres.Select(g => g.GenreName).FirstOrDefault(),
					SupplierName = p.SGameProductDetail.Supplier.SupplierName ?? _db.SOtherProductDetails.Where(o => o.ProductId == p.ProductId).Select(o => o.Supplier.SupplierName).FirstOrDefault(),

					// Other product details
					MerchTypeId = _db.SOtherProductDetails.Where(o => o.ProductId == p.ProductId).Select(o => o.MerchTypeId).FirstOrDefault(),
					PeripheralTypeName = _db.SOtherProductDetails.Where(o => o.ProductId == p.ProductId).Select(o => o.MerchType.MerchTypeName).FirstOrDefault(),
					DigitalCode = _db.SOtherProductDetails.Where(o => o.ProductId == p.ProductId).Select(o => o.DigitalCode).FirstOrDefault(),
					Size = _db.SOtherProductDetails.Where(o => o.ProductId == p.ProductId).Select(o => o.Size).FirstOrDefault(),
					Color = _db.SOtherProductDetails.Where(o => o.ProductId == p.ProductId).Select(o => o.Color).FirstOrDefault(),
					Weight = _db.SOtherProductDetails.Where(o => o.ProductId == p.ProductId).Select(o => o.Weight).FirstOrDefault(),
					Dimensions = _db.SOtherProductDetails.Where(o => o.ProductId == p.ProductId).Select(o => o.Dimensions).FirstOrDefault(),
					Material = _db.SOtherProductDetails.Where(o => o.ProductId == p.ProductId).Select(o => o.Material).FirstOrDefault(),
				})
				.FirstOrDefaultAsync();

			if (vm == null) return null;

			// Set CoverUrl from the already loaded gallery
			vm.CoverUrl = vm.Gallery.FirstOrDefault() ?? "/images/placeholder-cover.png";

			// Post-query for data from views or complex aggregations
			var rating = await _db.SVProductRatingStats
				.AsNoTracking()
				.Where(r => r.ProductId == vm.ProductId)
				.Select(r => new { r.RatingAvg, r.RatingCount })
				.FirstOrDefaultAsync();

			vm.RatingAvg = rating?.RatingAvg ?? 0;
			vm.RatingCount = rating?.RatingCount ?? 0;

			return vm;
		}
		/// <inheritdoc />
		public async Task<List<ProductCardDto>> GetTopFavorites(int count)
		{
			count = Math.Clamp(count, 1, 24);

			var topFavQuery = _db.SUserFavorites.AsNoTracking()
				.GroupBy(f => f.ProductId)
				.Select(g => new { ProductId = g.Key, Cnt = g.Count() })
				.OrderByDescending(x => x.Cnt)
				.ThenBy(x => x.ProductId)
				.Take(count);

			var query =
				from t in topFavQuery
				join p in _db.SProductInfos.AsNoTracking() on t.ProductId equals p.ProductId
				where !p.IsDeleted
				select new ProductCardDto
				{
					ProductId = p.ProductId,
					ProductName = p.ProductName.Trim(),
					ProductType = (p.ProductType ?? "").Trim(),
					Price = p.Price,
					CurrencyCode = (p.CurrencyCode ?? "").Trim(),
					ProductCode = p.SProductCode != null ? (p.SProductCode.ProductCode ?? "").Trim() : "",
					CoverUrl = p.SProductImages
								.OrderByDescending(i => i.IsPrimary)
								.ThenBy(i => i.SortOrder)
								.Select(i => i.ProductimgUrl)
								.FirstOrDefault() ?? "/images/onlinestoreNOPic/nophoto.jpg",
					PlatformName = (p.SGameProductDetail != null && p.SGameProductDetail.Platform != null)
									? p.SGameProductDetail.Platform.PlatformName
									: null,
					MerchTypeName =
											(from d in _db.SOtherProductDetails
											 join mt in _db.SMerchTypes on d.MerchTypeId equals mt.MerchTypeId
											 where d.ProductId == p.ProductId && !d.IsDeleted
											 select mt.MerchTypeName).FirstOrDefault(),
					IsPreorder = p.IsPreorderEnabled
				};

			var items = await query.ToListAsync();
			return items;
		}

		/// <inheritdoc />
		public async Task<GamiPort.Areas.OnlineStore.DTO.Store.PagedResult<ProductCardDto>> GetFavorites(int userId, int page, int pageSize)
		{
			page = Math.Max(1, page);
			pageSize = Math.Clamp(pageSize, 1, 60);

			var favBase = _db.SUserFavorites.AsNoTracking().Where(f => f.UserId == userId);
			var total = await favBase.CountAsync();

			var query =
				from f in favBase
				join p in _db.SProductInfos.AsNoTracking() on f.ProductId equals p.ProductId
				where !p.IsDeleted
				orderby f.CreatedAt descending, p.ProductId
				select new ProductCardDto
				{
					ProductId = p.ProductId,
					ProductName = p.ProductName.Trim(),
					ProductType = (p.ProductType ?? "").Trim(),
					Price = p.Price,
					CurrencyCode = (p.CurrencyCode ?? "").Trim(),
					ProductCode = p.SProductCode != null ? (p.SProductCode.ProductCode ?? "").Trim() : "",
					CoverUrl = p.SProductImages
								.OrderByDescending(i => i.IsPrimary)
								.ThenBy(i => i.SortOrder)
								.Select(i => i.ProductimgUrl)
								.FirstOrDefault() ?? "/images/onlinestoreNOPic/nophoto.jpg",
					PlatformName = (p.SGameProductDetail != null && p.SGameProductDetail.Platform != null)
									? p.SGameProductDetail.Platform.PlatformName
									: null,
					MerchTypeName =
						(from d in _db.SOtherProductDetails
						 join mt in _db.SMerchTypes on d.MerchTypeId equals mt.MerchTypeId
						 where d.ProductId == p.ProductId && !d.IsDeleted
						 select mt.MerchTypeName).FirstOrDefault(),
					IsPreorder = p.IsPreorderEnabled
				};

			var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

			return new GamiPort.Areas.OnlineStore.DTO.Store.PagedResult<ProductCardDto>
			{
				page = page,
				pageSize = pageSize,
				totalCount = total,
				items = items
			};
		}

		/// <inheritdoc />
		public async Task<List<int>> GetFavoriteIds(int userId)
		{
			return await _db.SUserFavorites
				.AsNoTracking()
				.Where(f => f.UserId == userId)
				.Select(f => f.ProductId)
				.ToListAsync();
		}

		/// <inheritdoc />
		public async Task<GamiPort.Areas.OnlineStore.DTO.Store.PagedResult<GamiPort.Areas.OnlineStore.DTO.Store.ReviewDto>> GetProductReviews(int productId, int page, int pageSize)
		{
			page = Math.Max(1, page);
			pageSize = Math.Clamp(pageSize, 1, 60);

			var baseQ = _db.SProductRatings.AsNoTracking().Where(r => r.ProductId == productId);
			var total = await baseQ.CountAsync();
			var items = await baseQ
				.OrderByDescending(r => r.CreatedAt)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(r => new GamiPort.Areas.OnlineStore.DTO.Store.ReviewDto
				{
					RatingId = r.RatingId,
					UserId = r.UserId,
					Rating = r.Rating,
					ReviewText = r.ReviewText,
					CreatedAt = r.CreatedAt
				})
				.ToListAsync();

			return new GamiPort.Areas.OnlineStore.DTO.Store.PagedResult<GamiPort.Areas.OnlineStore.DTO.Store.ReviewDto>
			{
				page = page,
				pageSize = pageSize,
				totalCount = total,
				items = items
			};
		}
	}
}



