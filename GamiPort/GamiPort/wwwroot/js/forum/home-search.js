// ~/wwwroot/js/forum/home-search.js
(() => {
    const form = document.getElementById("global-search-form");
    const input = document.getElementById("global-search-input");
    const mount = document.getElementById("home-content");     // 搜尋結果區
    const forums = document.getElementById("forums-app");       // Vue 掛載區（別移除）

    if (!form || !input || !mount || !forums) return;

    let inFlight;

    // -------- helpers --------
    const esc = (s) => (s || "").replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

    function restoreHome() {
        forums.style.display = "";
        mount.innerHTML = "";
        history.pushState(null, "", location.pathname);
    }

    function show(html) {
        forums.style.display = "none";
        mount.innerHTML = html;
    }

    function normalizePaged(raw) {
        const itemsRaw = raw?.items ?? raw?.Items ?? [];
        return {
            items: itemsRaw,
            total: raw?.total ?? raw?.Total ?? itemsRaw.length ?? 0,
            page: raw?.page ?? raw?.Page ?? 1,
            size: raw?.size ?? raw?.Size ?? itemsRaw.length ?? 0
        };
    }

    // -------- rendering --------
    function renderTabs(q, threadsPaged, forumsList) {
        const tItems = threadsPaged.items || [];
        const fItems = forumsList || [];

        const tabs = `
      <div class="d-flex align-items-baseline justify-content-between mb-2">
        <div>
          <span class="text-muted small">搜尋：</span><strong>${esc(q)}</strong>
        </div>
        <div class="btn-group">
          <button id="tabThreads" class="btn btn-sm btn-outline-primary active">主題 (${tItems.length}/${threadsPaged.total ?? tItems.length})</button>
          <button id="tabForums"  class="btn btn-sm btn-outline-secondary">論壇 (${fItems.length})</button>
          <button id="btnBackHome" class="btn btn-sm btn-outline-secondary">返回全部</button>
        </div>
      </div>
    `;

        const threadsHtml = `
      <ul class="list-group mb-3">
        ${tItems.length
                ? tItems.map(x => `
                <li class="list-group-item">
                  <div class="small text-secondary">${esc(x.forumName ?? x.ForumName ?? "")}</div>
                  <a class="fw-semibold" href="/Forum/Threads/Detail?threadId=${x.threadId ?? x.ThreadId}">
                    ${esc(x.title ?? x.Title ?? "")}
                  </a>
                  <div class="small text-muted">
                    by ${esc(x.authorName ?? x.AuthorName ?? "")}
                    · 回覆 ${x.replyCount ?? x.ReplyCount ?? 0}
                    · 讚 ${x.likeCount ?? x.LikeCount ?? 0}
                  </div>
                </li>
              `).join("")
                : `<li class="list-group-item text-muted">這個關鍵字沒有主題</li>`
            }
      </ul>
    `;

        const forumsHtml = `
      <div class="row g-3">
        ${fItems.length
                ? fItems.map(f => `
                <div class="col-12 col-md-6 col-lg-4">
                  <a class="forum-card" href="/Forum/Threads/Index?forumId=${f.ForumId ?? f.forumId}">
                    <img class="forum-cover" src="${esc(f.ImageUrl ?? f.imageUrl ?? "")}" alt="${esc(f.Name ?? f.name ?? "")}" />
                    <div class="forum-info">
                      <h5 class="forum-title">${esc(f.Name ?? f.name ?? "")}</h5>
                      <p class="forum-desc">${esc(f.Description ?? f.description ?? "—")}</p>
                    </div>
                  </a>
                </div>
              `).join("")
                : `<div class="text-muted">這個關鍵字沒有論壇</div>`
            }
      </div>
    `;

        show(`${tabs}
      <div id="panelThreads">${threadsHtml}</div>
      <div id="panelForums" class="d-none">${forumsHtml}</div>
    `);

        // 交互
        const tabThreads = document.getElementById("tabThreads");
        const tabForums = document.getElementById("tabForums");
        const pThreads = document.getElementById("panelThreads");
        const pForums = document.getElementById("panelForums");

        tabThreads.addEventListener("click", () => {
            tabThreads.classList.add("active"); tabThreads.classList.remove("btn-outline-secondary");
            tabForums.classList.remove("active"); tabForums.classList.add("btn-outline-secondary");
            pThreads.classList.remove("d-none");
            pForums.classList.add("d-none");
        });

        tabForums.addEventListener("click", () => {
            tabForums.classList.add("active"); tabForums.classList.remove("btn-outline-secondary");
            tabThreads.classList.remove("active"); tabThreads.classList.add("btn-outline-secondary");
            pForums.classList.remove("d-none");
            pThreads.classList.add("d-none");
        });

        document.getElementById("btnBackHome")?.addEventListener("click", (e) => {
            e.preventDefault();
            input.value = "";
            restoreHome();
        });
    }

    function renderEmpty(q) {
        show(`
      <div class="d-flex align-items-baseline justify-content-between mb-2">
        <div><span class="text-muted small">搜尋：</span><strong>${esc(q)}</strong></div>
        <button class="btn btn-sm btn-outline-secondary" id="btnBackHome">返回全部</button>
      </div>
      <div class="alert alert-warning">找不到相關主題或論壇</div>
    `);
        document.getElementById("btnBackHome")?.addEventListener("click", (e) => {
            e.preventDefault(); input.value = ""; restoreHome();
        });
    }

    // -------- search pipeline --------
    async function doSearch(q) {
        // 中止上一個
        if (inFlight) inFlight.abort();
        inFlight = new AbortController();

        forums.style.display = "none";
        show(`<div class="text-muted">搜尋中…</div>`);

        // 1) 主題搜尋
        const threadsPromise = fetch(`/api/forums/threads/search?q=${encodeURIComponent(q)}&page=1&size=20`, {
            signal: inFlight.signal, headers: { "accept": "application/json" }
        })
            .then(r => r.ok ? r.json() : Promise.reject(new Error(`threads ${r.status}`)))
            .then(normalizePaged)
            .catch(() => ({ items: [], total: 0, page: 1, size: 20 }));

        // 2) 論壇搜尋：優先 /api/forums?query=，失敗時 fallback 抓全部前端過濾
        const forumsPromise = (async () => {
            const ql = q.toLowerCase();
            const pick = (s) => (s || "").toLowerCase().includes(ql);

            // 先嘗試：/api/forums?query=
            try {
                const r = await fetch(`/api/forums?query=${encodeURIComponent(q)}`, {
                    signal: inFlight.signal, headers: { "accept": "application/json" }
                });
                if (r.ok) {
                    const data = await r.json();
                    const arr = Array.isArray(data) ? data : (data.items ?? []);
                    // ✅ 無論後端有沒有真的過濾，前端再過一次 Name/Description
                    return arr.filter(x => pick(x.Name ?? x.name) || pick(x.Description ?? x.description));
                }
            } catch { /* ignore */ }

            // 失敗或不支援 query → 抓全部自己過濾
            try {
                const r2 = await fetch(`/api/forums`, {
                    signal: inFlight.signal, headers: { "accept": "application/json" }
                });
                const all = await r2.json();
                const arr2 = Array.isArray(all) ? all : (all.items ?? []);
                return arr2.filter(x => pick(x.Name ?? x.name) || pick(x.Description ?? x.description));
            } catch {
                return [];
            }
        })();
        const [threadsPaged, forumsList] = await Promise.all([threadsPromise, forumsPromise]);

        if ((threadsPaged.items?.length ?? 0) === 0 && (forumsList.length ?? 0) === 0) {
            return renderEmpty(q);
        }
        renderTabs(q, threadsPaged, forumsList);
    }

    // -------- wire up --------
    form.addEventListener("submit", (e) => {
        e.preventDefault();
        const q = (input.value || "").trim();
        if (!q) return restoreHome();

        const url = new URL(location.href);
        url.searchParams.set("q", q);
        history.pushState({ q }, "", url);

        doSearch(q);
    });

    window.addEventListener("popstate", () => {
        const q = new URL(location.href).searchParams.get("q") || "";
        input.value = q;
        if (!q) return restoreHome();
        doSearch(q);
    });

    // 進頁自動帶 q 搜尋
    const initQ = new URL(location.href).searchParams.get("q");
    if (initQ) {
        input.value = initQ;
        doSearch(initQ);
    }
})();
