// 最小可用清單：呼叫 /api/store/products，產卡片＋分頁
const el = document.getElementById("product-grid");
if (!el) throw new Error("#product-grid not found");

const state = {
    page: 1,
    pageSize: 12,
    total: 0,
    items: [],
    loading: false,
    lastQuery: {}
};

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

async function fetchList() {
    state.loading = true; render();
    const p = new URLSearchParams(readQuery()).toString();
    const res = await fetch(`/api/store/products?${p}`);
    if (!res.ok) { el.innerHTML = `<div class="text-danger">讀取失敗</div>`; return; }
    const data = await res.json();
    state.page = data.page;
    state.pageSize = data.pageSize;
    state.total = data.totalCount;
    state.items = data.items || [];
    state.loading = false;
    render();
}

function renderCard(item) {
    const cover = item.coverUrl || "/images/placeholder-4x3.png";
    const platform = item.platformName ? `<span class="badge-platform me-1">${item.platformName}</span>` : "";
    const preorder = item.isPreorder ? `<span class="badge bg-warning text-dark ms-1">預購</span>` : "";
    return `
  <div class="col">
    <div class="store-card h-100">
      <a class="text-decoration-none" href="/OnlineStore/Store/Product/${item.productCode}">
        <div class="ratio ratio-4x3">
          <img src="${cover}" loading="lazy" class="img-fluid rounded-top object-fit-cover" alt="${item.productName}">
        </div>
        <div class="p-3">
          <div class="d-flex justify-content-between align-items-start">
            <h6 class="mb-1 text-truncate">${item.productName}</h6>
            <div class="ms-2 fw-bold">${item.price.toLocaleString()} ${item.currencyCode}</div>
          </div>
          <div class="small text-muted mb-1">
            ${platform}${preorder}
          </div>
          <div class="small text-secondary">代碼：${item.productCode}</div>
        </div>
      </a>
    </div>
  </div>`;
}

function renderPagination() {
    const pages = Math.ceil(state.total / state.pageSize);
    if (pages <= 1) return "";
    const btn = (p, text, disabled = false, active = false) =>
        `<li class="page-item ${disabled ? "disabled" : ""} ${active ? "active" : ""}">
       <a class="page-link" href="#" data-page="${p}">${text}</a>
     </li>`;
    let html = `<nav><ul class="pagination justify-content-center">`;
    html += btn(state.page - 1, "«", state.page === 1);
    for (let p = 1; p <= pages && p <= 10; p++) {
        html += btn(p, p, false, p === state.page);
    }
    html += btn(state.page + 1, "»", state.page === pages);
    html += `</ul></nav>`;
    return html;
}

function render() {
    if (state.loading) {
        el.innerHTML = `<div class="row row-cols-2 row-cols-md-3 row-cols-lg-4 g-3">
      ${Array.from({ length: state.pageSize }).map(() => `
        <div class="col"><div class="store-card placeholder-wave" style="height:240px"></div></div>
      `).join("")}
    </div>`;
        return;
    }
    const grid = `
    <div class="row row-cols-2 row-cols-md-3 row-cols-lg-4 g-3">
      ${state.items.map(renderCard).join("")}
    </div>
    <div class="mt-3">${renderPagination()}</div>
  `;
    el.innerHTML = grid;
    el.querySelectorAll(".page-link").forEach(a => {
        a.addEventListener("click", (e) => {
            e.preventDefault();
            const p = parseInt(a.dataset.page, 10);
            if (!isNaN(p)) { state.page = p; fetchList(); }
        });
    });
}

// 綁定「套用條件」按鈕
document.getElementById("btnSearch")?.addEventListener("click", () => {
    state.page = 1; fetchList();
});

// 初次載入
fetchList();
