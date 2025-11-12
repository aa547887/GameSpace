// =========================
// Home Rankings（首頁用）
// 職責：載入三個區塊（hot/rating/sales）的卡片網格
// =========================
const zones = [
    { el: document.getElementById("rank-hot"), type: "hot", take: 8 },
    { el: document.getElementById("rank-rating"), type: "rating", take: 8 },
    { el: document.getElementById("rank-sales"), type: "sales", take: 8 },
];

// ---- 小工具 ----
function money(v, ccy) {
    try { return `${Number(v).toLocaleString()} ${ccy || ""}`.trim(); }
    catch { return `${v} ${ccy || ""}`.trim(); }
}

// ---- UI：卡片網格 ----
function renderGrid(items) {
    if (!items || !items.length) return `<div class="text-muted">暫無資料</div>`;
    return `
    <div class="row row-cols-2 row-cols-md-4 row-cols-lg-4 g-3">
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

// ---- 主流程：逐區塊載入 ----
async function loadZone(z) {
    if (!z.el) return;
    z.el.innerHTML = `<div class="placeholder-wave" style="height:180px;border-radius:12px"></div>`;
    try {
        const res = await fetch(`/api/store/rankings?type=${encodeURIComponent(z.type)}&take=${z.take}`);
        if (!res.ok) { z.el.innerHTML = `<div class="text-danger">讀取失敗</div>`; return; }
        const data = await res.json();
        z.el.innerHTML = renderGrid(data);
    } catch (e) {
        console.error(e);
        z.el.innerHTML = `<div class="text-danger">載入失敗，請稍後再試。</div>`;
    }
}
zones.forEach(loadZone);
