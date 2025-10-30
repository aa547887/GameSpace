// =========================
// Product Detail（詳情頁）
// 職責：讀取 /api/store/products/{code} → 畫圖庫/資訊/推薦 + 收藏/購物車（local）
// =========================

// ---- DOM 取得 ----
const el = document.getElementById("product-detail");
if (!el) throw new Error("#product-detail not found");
const code = el.dataset.code;
const relatedEl = document.getElementById("related-grid");

// ---- 小工具 ----
function money(v, ccy) { try { return `${Number(v).toLocaleString()} ${ccy || ""}`.trim(); } catch { return `${v} ${ccy || ""}`.trim(); } }

// ---- UI：圖庫 ----
function renderGallery(images) {
    const list = images && images.length ? images : ["/images/placeholder-4x3.png"];
    const main = `<div class="ratio ratio-4x3 mb-3"><img src="${list[0]}" class="img-fluid rounded object-fit-cover" alt=""></div>`;
    const thumbs = list.slice(1, 5).map(u => `<div class="col"><div class="ratio ratio-4x3"><img src="${u}" class="img-fluid rounded object-fit-cover" alt=""></div></div>`).join("");
    return `${main}<div class="row row-cols-4 g-2">${thumbs}</div>`;
}

// ---- UI：推薦清單 ----
function renderRelated(items) {
    if (!items || !items.length) return `<div class="text-muted">暫無推薦</div>`;
    return `
    <div class="row row-cols-2 row-cols-md-4 row-cols-lg-6 g-3">
      ${items.map(x => {
        const cover = x.coverUrl || "/images/placeholder-4x3.png";
        return `
        <div class="col">
          <a class="text-decoration-none" href="/OnlineStore/Store/Product/${x.productCode}" target="_blank" rel="noopener">
            <div class="store-card h-100">
              <div class="ratio ratio-4x3">
                <img src="${cover}" loading="lazy" class="img-fluid rounded-top object-fit-cover" alt="${x.productName}">
              </div>
              <div class="p-2">
                <div class="small text-truncate">${x.productName}</div>
                <div class="small text-secondary">代碼：${x.productCode}</div>
                <div class="fw-bold">${money(x.price, x.currencyCode)}</div>
              </div>
            </div>
          </a>
        </div>`;
    }).join("")}
    </div>`;
}

// ---- 主流程：載入詳情 + 綁定互動 ----
async function load() {
    el.innerHTML = `<div class="placeholder-wave" style="height:220px;border-radius:12px"></div>`;
    try {
        const res = await fetch(`/api/store/products/${encodeURIComponent(code)}`);
        if (!res.ok) { el.innerHTML = `<div class="text-danger">找不到此商品</div>`; return; }
        const d = await res.json();

        const gallery = renderGallery(d.images);
        const platform = d.platformName ? `<span class="badge-platform me-2">${d.platformName}</span>` : "";
        const preorder = d.isPreorder ? `<span class="badge bg-warning text-dark">預購</span>` : "";
        el.innerHTML = `
      <div class="row g-4">
        <div class="col-12 col-lg-6">${gallery}</div>
        <div class="col-12 col-lg-6">
          <h3 class="mb-1">${d.productName}</h3>
          <div class="small text-secondary mb-2">代碼：${d.productCode}</div>
          <div class="mb-2">${platform} ${preorder}</div>
          <div class="fs-4 fw-bold mb-3">${money(d.price, d.currencyCode)}</div>

          <!-- 行動列：收藏 / 購物車（localStorage 示意） -->
          <div class="d-flex gap-2">
            <button class="btn btn-primary" id="btnAddCart">加入購物車（示意）</button>
            <button class="btn btn-outline-secondary" id="btnWish">♡ 追蹤</button>
          </div>
        </div>
      </div>`;

        if (relatedEl) relatedEl.innerHTML = renderRelated(d.related);

        // ---- 收藏 / 購物車（localStorage） ----
        const WL_KEY = "gsp_wishlist", CART_KEY = "gsp_cart";
        const getWL = () => JSON.parse(localStorage.getItem(WL_KEY) || "[]");
        const setWL = (arr) => localStorage.setItem(WL_KEY, JSON.stringify(arr));
        const getCart = () => JSON.parse(localStorage.getItem(CART_KEY) || "[]");
        const setCart = (arr) => localStorage.setItem(CART_KEY, JSON.stringify(arr));

        const btnWish = document.getElementById("btnWish");
        const btnAddCart = document.getElementById("btnAddCart");

        // 收藏初始狀態
        const initWL = () => {
            const has = getWL().includes(d.productCode);
            if (has) { btnWish.classList.add("btn-danger"); btnWish.textContent = "已追蹤"; }
        };
        // 收藏切換
        btnWish?.addEventListener("click", () => {
            const wl = getWL(); const idx = wl.indexOf(d.productCode);
            if (idx === -1) { wl.push(d.productCode); btnWish.classList.add("btn-danger"); btnWish.textContent = "已追蹤"; }
            else { wl.splice(idx, 1); btnWish.classList.remove("btn-danger"); btnWish.textContent = "♡ 追蹤"; }
            setWL(wl);
        });
        // 加入購物車
        btnAddCart?.addEventListener("click", () => {
            const cart = getCart();
            const i = cart.findIndex(x => x.code === d.productCode);
            if (i === -1) cart.push({ code: d.productCode, qty: 1 }); else cart[i].qty += 1;
            setCart(cart);
            btnAddCart.textContent = "已加入"; setTimeout(() => btnAddCart.textContent = "加入購物車（示意）", 1200);
        });
        initWL();

    } catch (e) {
        el.innerHTML = `<div class="text-danger">載入失敗，請稍後再試。</div>`;
        console.error(e);
    }
}
console.log("[product-detail] loaded");
load();
