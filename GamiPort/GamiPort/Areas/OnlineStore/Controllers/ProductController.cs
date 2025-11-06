
﻿﻿using GamiPort.Areas.OnlineStore.Services.store.Abstractions;
﻿﻿using GamiPort.Models;
﻿﻿using Microsoft.AspNetCore.Mvc;
﻿﻿using Microsoft.EntityFrameworkCore;
﻿﻿using System.Linq;
﻿﻿using System.Threading.Tasks;
﻿﻿
﻿﻿namespace GamiPort.Areas.OnlineStore.Controllers
﻿﻿{
﻿﻿    [Area("OnlineStore")]
﻿﻿    public class ProductController : Controller
﻿﻿    {
﻿﻿        private readonly GameSpacedatabaseContext _db;
﻿﻿        private readonly IStoreService _storeService;
﻿﻿
﻿﻿        public ProductController(GameSpacedatabaseContext db, IStoreService storeService)
﻿﻿        {
﻿﻿            _db = db;
﻿﻿            _storeService = storeService;
﻿﻿        }
﻿﻿
﻿﻿        [HttpGet("/OnlineStore/Product/Detail/{productCode}")]
﻿﻿        public async Task<IActionResult> Detail(string productCode)
﻿﻿        {
﻿﻿            var product = await _db.SProductCodes
﻿﻿                                   .AsNoTracking()
﻿﻿                                   .FirstOrDefaultAsync(p => p.ProductCode == productCode);
﻿﻿
﻿﻿            if (product == null)
﻿﻿            {
﻿﻿                return NotFound();
﻿﻿            }
﻿﻿
﻿﻿            var viewModel = await _storeService.GetProductDetailVM(product.ProductId);
﻿﻿
﻿﻿            if (viewModel == null)
﻿﻿            {
﻿﻿                return NotFound();
﻿﻿            }
﻿﻿
﻿﻿            return View(viewModel);
﻿﻿        }
﻿﻿    }
﻿﻿}
