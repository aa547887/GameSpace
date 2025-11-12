// wwwroot/js/forum/thread-list.js
console.log('[ThreadList] file loaded');

export const ThreadList = {
    props: {
        forumId: { type: Number, required: true },
        forumName: { type: String, required: false }
    },

    template: `
  <section class="gp-threadlist">
    <div class="d-flex align-items-center justify-content-between mb-3">
      <h3 class="m-0">🏷️ {{ forumName || '主題列表' }}</h3>

      <div class="d-flex align-items-center gap-2">
        <div class="me-2">
          <select class="form-select form-select-sm" v-model.number="size" @change="changePageSize" style="width:auto">
            <option :value="10">每頁 10 筆</option>
            <option :value="20">每頁 20 筆</option>
            <option :value="30">每頁 30 筆</option>
          </select>
        </div>

        <div class="btn-group me-2">
          <button class="btn btn-outline-secondary" :class="{active: sort==='newest'}"    @click="changeSort('newest')">最新</button>
          <button class="btn btn-outline-secondary" :class="{active: sort==='oldest'}"    @click="changeSort('oldest')">最舊</button>
          <button class="btn btn-outline-secondary" :class="{active: sort==='mostLiked'}" @click="changeSort('mostLiked')">最熱</button>
        </div>

        <button class="btn btn-primary" data-bs-toggle="collapse" data-bs-target="#postBox">新增貼文</button>
      </div>
    </div>

    <!-- 發文區 -->
    <div id="postBox" class="collapse mb-3">
      <div class="card shadow-sm border-0 rounded-3">
        <div class="card-body">
          <div class="mb-2">
            <input v-model.trim="newTitle" class="form-control" placeholder="標題（必填）">
          </div>
          <div class="mb-2">
            <textarea v-model.trim="newContent" class="form-control" rows="4" placeholder="內容（Markdown 可留空）"></textarea>
          </div>
          <div class="d-flex gap-2">
            <button class="btn btn-success" :disabled="creating" @click="createThread">送出</button>
            <button class="btn btn-outline-secondary" data-bs-toggle="collapse" data-bs-target="#postBox">取消</button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="error" class="alert alert-danger">{{ error }}</div>
    <div v-if="loading" class="text-muted">載入中…</div>

    <ul v-if="!loading" class="list-group mb-3 border-0">
      <li v-for="t in items" :key="t.threadId" class="list-group-item thread-item shadow-sm border-0 rounded-3 mb-2">
        <div class="d-flex align-items-start justify-content-between">
          <div class="me-3 flex-grow-1">
            <a :href="'/Forum/Threads/Detail?threadId=' + t.threadId" class="thread-title stretched-link">
              {{ t.title }}
            </a>
            <div class="thread-meta mt-1">
              <span class="pill">👍 {{ t.likeCount ?? 0 }}</span>
              <span class="pill">💬 {{ t.replyCount ?? 0 }}</span>
              <span class="pill muted">{{ sortLabel }}：{{ formatDate(displayDate(t)) }}</span>
            </div>
          </div>
          <div v-if="t.pinned" class="badge bg-warning-subtle text-warning-emphasis align-self-start">置頂</div>
        </div>
      </li>
      <li v-if="items.length===0" class="list-group-item text-muted border-0">目前沒有主題</li>
    </ul>

    <div class="d-flex align-items-center justify-content-between">
      <div class="small text-muted">共 {{ total }} 筆</div>
      <div class="btn-group">
        <button class="btn btn-outline-secondary btn-sm" @click="prev" :disabled="loading || page<=1">上一頁</button>
        <span class="btn btn-outline-secondary btn-sm disabled">第 {{ page }} / {{ pages }} 頁</span>
        <button class="btn btn-outline-secondary btn-sm" @click="next" :disabled="loading || page>=pages">下一頁</button>
      </div>
    </div>
  </section>
  `,

    data() {
        return {
            sort: 'newest',   // newest | oldest | mostLiked
            page: 1,
            size: 20,
            total: 0,
            items: [],
            loading: false,
            error: '',
            newTitle: '',
            newContent: '',
            creating: false,
            inFlight: null    // AbortController
        };
    },

    computed: {
        pages() {
            return Math.max(1, Math.ceil(this.total / this.size));
        },
        sortLabel() {       // 顯示文字避免誤解
            return this.sort === 'oldest' ? '建立'
                : this.sort === 'newest' ? '活躍'
                    : '活躍';
        }
    },

    methods: {
        // 對應後端 sort（不傳 dir）
        mapSort(ui) {
            if (ui === 'newest') return 'lastReply';
            if (ui === 'oldest') return 'created';
            if (ui === 'mostLiked') return 'hot';
            return 'lastReply';
        },

        // 「最舊」顯示 CreatedAt，其餘顯示活躍時間
        displayDate(t) {
            if (this.sort === 'oldest') {
                return t.createdAt ?? t.updatedAt ?? null;
            }
            return t.updatedAt ?? t.createdAt ?? null;
        },

        normalize(data) {
            const raw = Array.isArray(data) ? data : (data.items ?? data.Items ?? []);
            const items = (raw ?? []).map(x => ({
                threadId: x.threadId ?? x.ThreadId ?? x.id ?? x.Id,
                title: x.title ?? x.Title ?? '',
                status: x.status ?? x.Status ?? '',
                createdAt: x.createdAt ?? x.CreatedAt ?? null,
                updatedAt: x.updatedAt ?? x.UpdatedAt ?? null,
                replyCount: x.replyCount ?? x.Replies ?? x.replies ?? 0,
                likeCount: x.likeCount ?? x.LikeCount ?? 0,
                hotScore: x.hotScore ?? x.HotScore ?? null,
                pinned: x.pinned ?? x.Pinned ?? false,
                isOwner: x.isOwner ?? x.IsOwner ?? false,
                canDelete: x.canDelete ?? x.CanDelete ?? false
            }));
            const total = Array.isArray(data) ? items.length : (data.total ?? data.Total ?? items.length);
            return { items, total };
        },

        computeHotScore(t) {
            if (t.hotScore != null && !Number.isNaN(Number(t.hotScore))) return Number(t.hotScore);
            const likes = Number(t.likeCount ?? 0);
            const replies = Number(t.replyCount ?? 0);
            const ts = new Date(t.updatedAt ?? t.createdAt ?? 0).getTime();
            const hours = Math.max(0, (Date.now() - ts) / 3600000);
            const engagement = replies * 2 + likes;
            return engagement / Math.log10(hours + 10);
        },

        maybeClientSort(items) {
            const backendSupportsHot = items.length > 0 && (items[0].hotScore != null);
            if (this.sort === 'mostLiked' && !backendSupportsHot) {
                return [...items].sort((a, b) => {
                    const ha = this.computeHotScore(a);
                    const hb = this.computeHotScore(b);
                    if (hb !== ha) return hb - ha;
                    const ta = new Date(a.updatedAt ?? a.createdAt ?? 0).getTime();
                    const tb = new Date(b.updatedAt ?? b.createdAt ?? 0).getTime();
                    return tb - ta;
                });
            }
            return items;
        },

        _verifyOrder(items) {
            try {
                if (this.sort === 'oldest') {
                    for (let i = 1; i < items.length; i++) {
                        const prev = new Date(items[i - 1].createdAt ?? 0).getTime();
                        const curr = new Date(items[i].createdAt ?? 0).getTime();
                        if (prev > curr) { console.warn('[oldest] not ascending', { prev: items[i - 1], curr: items[i] }); break; }
                    }
                } else if (this.sort === 'newest') {
                    for (let i = 1; i < items.length; i++) {
                        const prev = new Date(items[i - 1].updatedAt ?? items[i - 1].createdAt ?? 0).getTime();
                        const curr = new Date(items[i].updatedAt ?? items[i].createdAt ?? 0).getTime();
                        if (prev < curr) { console.warn('[newest] not descending', { prev: items[i - 1], curr: items[i] }); break; }
                    }
                }
            } catch { }
        },

        async fetchThreads() {
            if (!this.forumId) { this.error = 'forumId 缺失'; return; }
            this.loading = true; this.error = '';

            if (this.inFlight) this.inFlight.abort();
            this.inFlight = new AbortController();

            try {
                const sort = this.mapSort(this.sort);
                const url = new URL(`/api/forums/${this.forumId}/threads`, location.origin);
                url.searchParams.set('sort', sort);
                url.searchParams.set('page', String(this.page));
                url.searchParams.set('size', String(this.size));

                const res = await fetch(url.toString(), {
                    headers: { 'Accept': 'application/json' },
                    credentials: 'include',
                    signal: this.inFlight.signal
                });
                if (!res.ok) throw new Error(`API 失敗：${res.status} ${res.statusText}`);

                const data = await res.json();
                const { items, total } = this.normalize(data);

                this.items = this.maybeClientSort(items);
                this.total = total;
                this._verifyOrder(this.items);

            } catch (e) {
                if (e?.name === 'AbortError') return;
                this.items = [];
                this.total = 0;
                this.error = e?.message ?? String(e);
            } finally {
                this.loading = false;
                this.inFlight = null;
            }
        },

        changeSort(s) {
            if (this.sort !== s) {
                this.sort = s;
                this.page = 1;
                this.fetchThreads();
            }
        },

        changePageSize() {
            this.page = 1;
            this.fetchThreads();
        },

        next() { if (this.page < this.pages) { this.page++; this.fetchThreads(); } },
        prev() { if (this.page > 1) { this.page--; this.fetchThreads(); } },

        formatDate(v) {
            if (!v) return '';
            const d = (v instanceof Date) ? v : new Date(v);
            return isNaN(d) ? '' : d.toLocaleString();
        },

        async createThread() {
            if (!this.newTitle.trim()) { alert('請輸入標題'); return; }
            this.creating = true;
            try {
                const body = { forumId: this.forumId, title: this.newTitle.trim(), contentMd: this.newContent.trim() };
                const r = await fetch('/api/forum/threads', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    credentials: 'include',
                    body: JSON.stringify(body)
                });

                if (r.status === 401) { alert('請先登入'); return; }
                if (!r.ok) throw new Error(`發文失敗：${r.status}`);

                this.newTitle = ''; this.newContent = '';
                const el = document.getElementById('postBox');
                if (el && window.bootstrap) {
                    try { window.bootstrap.Collapse.getOrCreateInstance(el).hide(); } catch { }
                }
                await this.fetchThreads();
            } catch (e) {
                alert(e?.message ?? e);
            } finally {
                this.creating = false;
            }
        },

        ensureStyles() {
            if (document.getElementById('gp-threadlist-style')) return;
            const css = `
      .gp-threadlist .list-group { display:flex; flex-direction:column; gap:.5rem; }
      .gp-threadlist .list-group .list-group-item { margin:0; }
      .gp-threadlist .thread-item { transition: transform .08s ease, box-shadow .12s ease; }
      .gp-threadlist .thread-item:hover { transform: translateY(-1px); box-shadow: 0 .375rem .8rem rgba(0,0,0,.06); }
      .gp-threadlist .thread-title { font-weight: 600; color: var(--bs-body-color); text-decoration: none; display: -webkit-box; -webkit-line-clamp: 1; -webkit-box-orient: vertical; overflow: hidden; }
      .gp-threadlist .thread-title:hover { text-decoration: underline; }
      .gp-threadlist .thread-meta .pill { display:inline-block; font-size: .8rem; padding: .15rem .5rem; border-radius: 999px; background: var(--bs-light); margin-right:.35rem; }
      .gp-threadlist .thread-meta .pill.muted { background: transparent; color: var(--bs-secondary-color); }
      `;
            const el = document.createElement('style');
            el.id = 'gp-threadlist-style';
            el.textContent = css;
            document.head.appendChild(el);
        }
    },

    mounted() {
        this.ensureStyles();
        this.fetchThreads();
    }
};
