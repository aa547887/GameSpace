// ~/wwwroot/js/forum/home-search.js
(() => {
    const form = document.getElementById("global-search-form");
    const input = document.getElementById("global-search-input");
    const mount = document.getElementById("home-content");
    if (!form || !input || !mount) return;

    let inFlight;

    form.addEventListener("submit", async (e) => {
        e.preventDefault();
        const q = (input.value || "").trim();

        if (!q) {
            history.pushState(null, "", location.pathname);
            mount.innerHTML = originalHomeContent();
            return;
        }

        if (inFlight) inFlight.abort();
        inFlight = new AbortController();

        const url = new URL(location.href);
        url.searchParams.set("q", q);
        history.pushState({ q }, "", url);

        mount.innerHTML = `<div class="text-muted">搜尋中…</div>`;

        try {
            const res = await fetch(`/api/forums/threads/search?q=${encodeURIComponent(q)}&page=1&size=20`,
                { signal: inFlight.signal });
            const raw = await res.json();
            const data = normalizePaged(raw);           // ✅ 這裡也要轉
            render(q, data);
        } catch (err) {
            if (err.name === "AbortError") return;
            mount.innerHTML = `<div class="alert alert-danger">搜尋爆炸了，再一下～</div>`;
        }
    });

    window.addEventListener("popstate", () => {
        const q = new URL(location.href).searchParams.get("q") || "";
        input.value = q;
        if (!q) {
            mount.innerHTML = originalHomeContent();
        } else {
            search(q);
        }
    });

    (async () => {
        const q = new URL(location.href).searchParams.get("q");
        if (!q) return;
        input.value = q;
        search(q);
    })();

    async function search(q) {
        mount.innerHTML = `<div class="text-muted">載入中…</div>`;
        const res = await fetch(`/api/forums/threads/search?q=${encodeURIComponent(q)}&page=1&size=20`);
        const raw = await res.json();
        const data = normalizePaged(raw);
        render(q, data);
    }

    // ✅ 把分頁物件 + 每個 item 都轉成 camelCase
    function normalizePaged(raw) {
        const itemsRaw = raw.items ?? raw.Items ?? [];
        return {
            items: itemsRaw.map(normalizeItem),
            total: raw.total ?? raw.Total ?? 0,
            page: raw.page ?? raw.Page ?? 1,
            size: raw.size ?? raw.Size ?? 20,
        };
    }
    function normalizeItem(it) {
        return {
            threadId: it.threadId ?? it.ThreadId,
            forumId: it.forumId ?? it.ForumId,
            forumName: it.forumName ?? it.ForumName ?? "",
            title: it.title ?? it.Title ?? "",
            authorId: it.authorId ?? it.AuthorId,
            authorName: it.authorName ?? it.AuthorName ?? "",
            replyCount: it.replyCount ?? it.ReplyCount ?? 0,
            likeCount: it.likeCount ?? it.LikeCount ?? 0,
            updatedAt: it.updatedAt ?? it.UpdatedAt ?? null,
        };
    }

    function render(q, data) {
        const list = (data.items || []).map(x => `
      <li class="list-group-item">
        <div class="small text-secondary">${esc(x.forumName || "")}</div>
        <a class="fw-semibold" href="/Forum/Threads/Detail?threadId=${x.threadId}">
          ${esc(x.title)}
        </a>
        <div class="small text-muted">
          by ${esc(x.authorName || "")} · 回覆 ${x.replyCount} · 讚 ${x.likeCount}
        </div>
      </li>`).join("");

        mount.innerHTML = `
      <h5 class="mb-3">搜尋「${esc(q)}」（${data.total}）</h5>
      <ul class="list-group mb-3">
        ${list || `<li class="list-group-item text-muted">查無結果</li>`}
      </ul>`;
    }

    function esc(s) { return (s || "").replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c])) }

    function originalHomeContent() {
        return `
      <div id="forums-app">
        <forum-list></forum-list>
      </div>`;
    }
})();
