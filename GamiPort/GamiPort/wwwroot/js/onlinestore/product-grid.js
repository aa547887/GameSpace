// =========================
// Product Grid（Browse 頁）
// 職責：讀取 /api/store/products → 產卡片 + 分頁 + 收藏/購物車（local）
// =========================

// ---- DOM 掛載點與狀態 ----
const el = document.getElementById("product-grid");
if (!el) throw new Error("#product-grid not found");

const state = { page: 1, pageSize: 12, total: 0, items: [], loading: false };

// ---- 小工具：抓篩選值 / localStorage 快捷 ----
function qs(id) { return document.getElementById(id); }
function readQuery() {
    return {
        q: qs("q")?.value?.trim() || "",
        type: qs("type")?.value || "",
        sort: qs("sort")?.value || "",
        page: state.page,
        pageSize: state.pageSize
    };
}
const WL_KEY = "gsp_wishlist";
const CART_KEY = "gsp_cart";
const getWL = () => JSON.parse(localStorage.getItem(WL_KEY) || "[]");
const setWL = (arr) => localStorage.setItem(WL_KEY, JSON.stringify(arr));
const getCart = () => JSON.parse(localStorage.getItem(CART_KEY) || "[]");
const setCart = (arr) => localStorage.setItem(CART_KEY, JSON.stringify(arr));

// ---- 資料存取：呼叫清單 API ----
async function fetchList() {
    state.loading = true; render();
    const p = new URLSearchParams(readQuery()).toString();
    const res = await fetch(`/api/store/products?${p}`);
    if (!res.ok) { el.innerHTML = `<div class="text-danger">讀取失敗</div>`; return; }
    const data = await res.json();
    state.page = data.page; state.pageSize = data.pageSize;
    state.total = data.totalCount; state.items = data.items || [];
    state.loading = false; render();
}

// ---- UI：單張卡片 ----
function renderCard(item) {
    const cover = item.coverUrl || "/images/placeholder-4x3.png";     // ← 無圖用占位
    const platform = item.platformName ? `<span class="badge-platform me-1">${item.platformName}</span>` : "";
    const preorder = item.isPreorder ? `<span class="badge bg-warning text-dark ms-1">預購</span>` : "";
    const isFav = getWL().includes(item.productCode);
    return `
  <div class="col">
    <div class="store-card h-100">
      <a class="text-decoration-none" href="/OnlineStore/Store/Product/${item.productCode}" target="_blank" rel="noopener">
        <div class="ratio ratio-4x3">
          <img src="${cover}" loading="lazy" class="img-fluid rounded-top object-fit-cover" alt="${item.productName}">
        </div>
      </a>
      <div class="p-3">
        <div class="d-flex justify-content-between align-items-start">
          <h6 class="mb-1 text-truncate">${item.productName}</h6>
          <div class="ms-2 fw-bold">${Number(item.price).toLocaleString()} ${item.currencyCode}</div>
        </div>
        <div class="small text-muted mb-2">${platform}${preorder}</div>
        <div class="small text-secondary mb-2">代碼：${item.productCode}</div>

        <!-- 行動列：收藏 / 購物車（localStorage 示意） -->
        <div class="d-flex gap-2">
          <button class="btn btn-sm ${isFav ? 'btn-danger' : 'btn-outline-secondary'} btn-wish" data-code="${item.productCode}">
            ${isFav ? '已追蹤' : '♡ 追蹤'}
          </button>
          <button class="btn btn-sm btn-primary btn-cart" data-code="${item.productCode}">
            加入購物車
          </button>
        </div>
      </div>
    </div>
  </div>`;
}

// ---- UI：分頁列 ----
function renderPagination() {
    const pages = Math.ceil(state.total / state.pageSize);
    if (pages <= 1) return "";
    const btn = (p, text, disabled = false, active = false) =>
        `<li class="page-item ${disabled ? "disabled" : ""} ${active ? "active" : ""}">
       <a class="page-link" href="#" data-page="${p}">${text}</a>
     </li>`;
    let html = `<nav><ul class="pagination justify-content-center">`;
    html += btn(state.page - 1, "«", state.page === 1);
    for (let p = 1; p <= pages && p <= 10; p++) { html += btn(p, p, false, p === state.page); }
    html += btn(state.page + 1, "»", state.page === pages);
    html += `</ul></nav>`;
    return html;
}

// ---- UI：整體渲染 ----
function render() {
    if (state.loading) {
        el.innerHTML = `<div class="row row-cols-2 row-cols-md-3 row-cols-lg-4 g-3">
      ${Array.from({ length: state.pageSize }).map(() => `
        <div class="col"><div class="store-card placeholder-wave" style="height:240px"></div></div>
      `).join("")}
    </div>`;
        return;
    }
    if (!state.items.length) {
        el.innerHTML = `<div class="alert alert-warning">沒有符合的商品。請確認測試資料或放寬篩選。</div>`;
        return;
    }
    const grid = `
    <div class="row row-cols-2 row-cols-md-3 row-cols-lg-4 g-3">
      ${state.items.map(renderCard).join("")}
    </div>
    <div class="mt-3">${renderPagination()}</div>
  `;
    el.innerHTML = grid;

    // 綁定：分頁點擊
    el.querySelectorAll(".page-link").forEach(a => {
        a.addEventListener("click", (e) => {
            e.preventDefault();
            const p = parseInt(a.dataset.page, 10); if (!isNaN(p)) { state.page = p; fetchList(); }
        });
    });

    // 綁定：收藏（localStorage）
    el.querySelectorAll(".btn-wish").forEach(btn => {
        btn.addEventListener("click", () => {
            const code = btn.dataset.code;
            const wl = getWL(); const i = wl.indexOf(code);
            if (i === -1) { wl.push(code); btn.classList.remove("btn-outline-secondary"); btn.classList.add("btn-danger"); btn.textContent = "已追蹤"; }
            else { wl.splice(i, 1); btn.classList.add("btn-outline-secondary"); btn.classList.remove("btn-danger"); btn.textContent = "♡ 追蹤"; }
            setWL(wl);
        });
    });

    // 綁定：加入購物車（localStorage）
    el.querySelectorAll(".btn-cart").forEach(btn => {
        btn.addEventListener("click", () => {
            const code = btn.dataset.code;
            const cart = getCart();
            const i = cart.findIndex(x => x.code === code);
            if (i === -1) cart.push({ code, qty: 1 }); else cart[i].qty += 1;
            setCart(cart);
            btn.textContent = "已加入"; setTimeout(() => btn.textContent = "加入購物車", 1200);
        });
    });
}

// ---- 入口：搜尋按鈕 + 首次載入 ----
document.getElementById("btnSearch")?.addEventListener("click", () => { state.page = 1; fetchList(); });
console.log("[product-grid] loaded");
fetchList();
