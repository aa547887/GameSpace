// =========================
// Rankings Page（/Rankings）
// 職責：依分頁籤 type 載入 /api/store/rankings → 產卡片
// =========================
const container = document.getElementById("rankings-container");
if (!container) throw new Error("#rankings-container not found");

// ---- 小工具 ----
function money(v, ccy) {
    try { return `${Number(v).toLocaleString()} ${ccy || ""}`.trim(); }
    catch { return `${v} ${ccy || ""}`.trim(); }
}

// ---- UI：卡片網格 ----
function renderGrid(items) {
    if (!items || !items.length) return `<div class="text-muted">暫無資料</div>`;
    return `
    <div class="row row-cols-2 row-cols-md-3 row-cols-lg-4 g-3">
      ${items.map(x => {
        const cover = x.coverUrl || "/images/placeholder-4x3.png";
        const platform = x.platformName ? `<span class="badge-platform me-1">${x.platformName}</span>` : "";
        const preorder = x.isPreorder ? `<span class="badge bg-warning text-dark ms-1">預購</span>` : "";
        return `
          <div class="col">
            <a class="text-decoration-none" href="/OnlineStore/Store/Product/${x.productCode}" target="_blank" rel="noopener">
              <div class="store-card h-100">
                <div class="ratio ratio-4x3">
                  <img src="${cover}" loading="lazy" class="img-fluid rounded-top object-fit-cover" alt="${x.productName}">
                </div>
                <div class="p-3">
                  <div class="d-flex justify-content-between align-items-start">
                    <h6 class="mb-1 text-truncate">${x.productName}</h6>
                    <div class="ms-2 fw-bold">${money(x.price, x.currencyCode)}</div>
                  </div>
                  <div class="small text-muted mb-1">${platform}${preorder}</div>
                  <div class="small text-secondary">代碼：${x.productCode}</div>
                </div>
              </div>
            </a>
          </div>`;
    }).join("")}
    </div>`;
}

// ---- 主流程：依 type 載入 ----
async function load(type = "hot") {
    container.innerHTML = `<div class="placeholder-wave" style="height:200px;border-radius:12px"></div>`;
    try {
        const res = await fetch(`/api/store/rankings?type=${encodeURIComponent(type)}&take=12`);
        if (!res.ok) { container.innerHTML = `<div class="text-danger">讀取失敗</div>`; return; }
        const data = await res.json();
        container.innerHTML = renderGrid(data);
    } catch (e) {
        console.error(e);
        container.innerHTML = `<div class="text-danger">載入失敗，請稍後再試。</div>`;
    }
}

// ---- 綁定分頁籤 + 預設載入 ----
document.querySelectorAll('#rankTabs .nav-link').forEach(btn => {
    btn.addEventListener('click', () => {
        const type = btn.getAttribute('data-type') || 'hot';
        load(type);
    });
});
load("hot");
