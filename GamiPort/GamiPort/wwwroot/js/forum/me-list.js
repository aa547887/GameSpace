import { createApp, reactive } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.prod.js';
const titleMap = {
    '/api/Forum/me/threads': '我的貼文',
    '/api/Forum/me/posts': '我的回覆',
    '/api/Forum/me/likes/threads': '我的按讚'
};

// 簡易去除 Markdown 做摘要
function stripMd(s = '') {
    return String(s)
        .replace(/```[\s\S]*?```/g, '')          // code block
        .replace(/`[^`]*`/g, '')                 // inline code
        .replace(/!\[[^\]]*\]\([^)]*\)/g, '')    // images
        .replace(/\[[^\]]*\]\([^)]*\)/g, '')     // links
        .replace(/[#>*_~`>-]/g, '')              // md symbols
        .replace(/\s+/g, ' ')
        .trim();
}

// 把 API（threads/posts/likes）回來的不同形狀 → 同一形狀
function normalize(x) {
    // 1) 回覆(Post)清單：/me/posts
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
    // 2) 主題(Thread)清單：/me/threads 或 /me/likes/threads
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

export default function MeList(endpoint) {
    document.getElementById('me-page-title').textContent =
        titleMap[endpoint] ?? '我的內容';
    return createApp({
        setup() {
            const st = reactive({
                items: [],
                page: 1,
                size: 20,
                total: 0,
                loading: false,
                error: '',
                sort: 'latest'
            });

            async function load() {
                st.loading = true; st.error = '';
                try {
                    const qs = new URLSearchParams({
                        page: String(st.page),
                        size: String(st.size),
                        sort: st.sort
                    });
                    const res = await fetch(`${endpoint}?${qs.toString()}`, {
                        headers: { 'accept': 'application/json' }
                    });
                    if (res.status === 401) { location.href = '/Login/Login/Login'; return; }
                    if (!res.ok) throw new Error(`HTTP ${res.status}`);
                    const data = await res.json();

                    const raw = data.Items ?? data.items ?? [];
                    st.items = raw.map(normalize);
                    st.total = data.Total ?? data.total ?? raw.length ?? 0;
                } catch (err) {
                    st.error = String(err);
                } finally {
                    st.loading = false;
                }
            }

            function prev() { if (st.page > 1) { st.page--; load(); } }
            function next() { if (st.page * st.size < st.total) { st.page++; load(); } }

            load();
            return { st, load, prev, next };
        },

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
