// ~/wwwroot/js/forum/me-list.js
import { createApp, reactive } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.prod.js';

/* =========================
   基本設定
   ========================= */
const titleMap = {
    '/api/forum/me/threads': '我的貼文',
    '/api/forum/me/posts': '我的回覆',
    '/api/forum/me/likes/threads': '我的按讚'
};

/* =========================
   小工具：去掉 Markdown 轉成摘要
   ========================= */
function stripMd(s = '') {
    return String(s)
        .replace(/```[\s\S]*?```/g, '')       // 三引號 code block
        .replace(/`[^`]*`/g, '')              // 反引號 inline code
        .replace(/!\[[^\]]*\]\([^)]*\)/g, '') // 圖片
        .replace(/\[[^\]]*\]\([^)]*\)/g, '')  // 超連結
        .replace(/[#>*_~`>-]/g, '')           // Markdown 符號
        .replace(/\s+/g, ' ')
        .trim();
}

/* =========================
   小工具：不同 API 回傳形狀 → 正規化
   - /me/posts 會長得像「回覆」
   - /me/threads、/me/likes/threads 長得像「主題」
   ========================= */
function normalize(x) {
    // 是「回覆」
    if (x.PostId || x.postId) {
        return {
            kind: 'post',
            id: x.PostId ?? x.postId,
            threadId: x.ThreadId ?? x.threadId,
            title: x.ThreadTitle ?? x.threadTitle ?? '(無標題)',
            snippet: stripMd(x.ContentMd ?? x.contentMd ?? ''),
            likeCount: x.LikeCount ?? x.likeCount ?? 0,
            repliesCount: null,
            createdAt: x.CreatedAt ?? x.createdAt
        };
    }
    // 是「主題」
    return {
        kind: 'thread',
        id: x.ThreadId ?? x.threadId,
        threadId: x.ThreadId ?? x.threadId,
        title: x.Title ?? x.title ?? '(無標題)',
        snippet: x.Tldr ?? x.tldr ?? '',
        likeCount: x.LikeCount ?? x.likeCount ?? 0,
        repliesCount: x.RepliesCount ?? x.repliesCount ?? 0,
        createdAt: x.CreatedAt ?? x.createdAt,
        lastReplyAt: x.LastReplyAt ?? x.lastReplyAt
    };
}

/* =========================
   共用：導去登入（帶 ReturnUrl）
   - 預設整頁導向
   - 如果頁面上有 #login-required Modal，就先彈窗
   ========================= */
function goLogin() {
    const loginUrl = `/Login/Login/Login?ReturnUrl=${encodeURIComponent(location.pathname + location.search)}`;

    // 若你有 Bootstrap Modal，可在 View 放一個 id="login-required" 的 Modal
    const modalEl = document.getElementById('login-required');
    if (modalEl && window.bootstrap) {
        const a = modalEl.querySelector('#go-login');
        if (a) a.setAttribute('href', loginUrl);
        new bootstrap.Modal(modalEl).show();
        return;
    }

    // 沒 Modal → 直接導頁
    location.assign(loginUrl);
}

/* =========================
   共用：帶權限偵測的 fetch
   - 一律帶 cookie：credentials: 'include'
   - 只要 401/403/被重導/拿到 HTML，就判定未登入 → goLogin()
   - 成功則回傳 JSON
   ========================= */
async function fetchJsonAuth(url, options = {}) {
    const r = await fetch(url, {
        credentials: 'include',
        headers: { accept: 'application/json', ...(options.headers || {}) },
        ...options
    });

    const ctype = r.headers.get('content-type') || '';

    // 401/403：未登入；redirected：可能被 302 到登入頁；text/html：多半就是登入頁的 HTML
    if (r.status === 401 || r.status === 403 || r.redirected || ctype.includes('text/html')) {
        goLogin();
        throw new Error('AUTH_REQUIRED'); // 中斷後續流程
    }

    if (!r.ok) throw new Error(`HTTP ${r.status}`);
    return r.json();
}

/* =========================
   元件主體：MeList
   - endpoint：'/api/forum/me/threads' | '/api/forum/me/posts' | '/api/forum/me/likes/threads'
   ========================= */
export default function MeList(endpoint) {
    // 頁面標題：用小寫比對避免大小寫踩雷
    const titleEl = document.getElementById('me-page-title');
    if (titleEl) titleEl.textContent = titleMap[endpoint?.toLowerCase()] ?? '我的內容';

    return createApp({
        setup() {
            const st = reactive({
                items: [],
                page: 1,
                size: 20,
                total: 0,
                loading: false,
                error: '',
                sort: 'latest' // 你的 API 若支援排序，會帶上去
            });

            // 核心載入流程
            async function load() {
                st.loading = true; st.error = '';
                try {
                    const qs = new URLSearchParams({
                        page: String(st.page),
                        size: String(st.size),
                        sort: st.sort
                    });

                    // ★ 使用共用 fetch：自動處理未登入
                    const data = await fetchJsonAuth(`${endpoint}?${qs.toString()}`);

                    // 支援兩種慣例：{ items, total } 或 { Items, Total }
                    const raw = data.Items ?? data.items ?? [];
                    const totalHeader = Number((data.Total ?? data.total ?? 0) || (await 0)); // 沒 total 就 0
                    st.items = raw.map(normalize);
                    st.total = totalHeader || raw.length;

                } catch (err) {
                    // AUTH_REQUIRED 已在 goLogin 處理，這裡不需要再顯示錯誤
                    if (String(err?.message) !== 'AUTH_REQUIRED') {
                        st.error = String(err);
                    }
                } finally {
                    st.loading = false;
                }
            }

            // 分頁
            function prev() { if (st.page > 1) { st.page--; load(); } }
            function next() { if (st.page * st.size < st.total) { st.page++; load(); } }

            load();
            return { st, load, prev, next, Math };
        },

        // 極簡樣板：可先跑起來再美化
        template: `
      <section>
        <div v-if="st.loading" class="text-muted">載入中…</div>
        <div v-else-if="st.error" class="alert alert-danger">{{ st.error }}</div>

        <div v-else>
          <div v-if="!st.items.length" class="text-muted">目前沒有資料。</div>

          <ul v-else class="list-group mb-3">
            <li v-for="it in st.items" :key="it.kind + '-' + it.id" class="list-group-item">
              <a :href="'/Forum/Threads/Detail?threadId=' + it.threadId" class="fw-semibold text-decoration-none">
                {{ it.title }}
              </a>
              <div v-if="it.snippet" class="text-muted small mt-1 line-clamp-2">{{ it.snippet }}</div>
              <div class="text-muted small mt-1">
                <template v-if="it.repliesCount !== null">回覆：{{ it.repliesCount }}　</template>
                讚：{{ it.likeCount }}
              </div>
            </li>
          </ul>

          <div class="d-flex justify-content-between align-items-center">
            <button class="btn btn-outline-secondary btn-sm" :disabled="st.page<=1" @click="prev">上一頁</button>
            <span class="text-muted small">{{ st.page }} / {{ Math.max(1, Math.ceil(st.total/st.size)) }}</span>
            <button class="btn btn-outline-secondary btn-sm" :disabled="st.page*st.size>=st.total" @click="next">下一頁</button>
          </div>
        </div>
      </section>
    `
    });
}

/* =========================
   可選：在 View 放這段 Modal（若想彈窗而不是直接導頁）
   <div class="modal" id="login-required" tabindex="-1">
     <div class="modal-dialog"><div class="modal-content">
       <div class="modal-header"><h5 class="modal-title">請先登入</h5></div>
       <div class="modal-body">此區需登入才能查看。</div>
       <div class="modal-footer">
         <a class="btn btn-primary" id="go-login">前往登入</a>
       </div>
     </div></div>
   </div>
   ========================= */
