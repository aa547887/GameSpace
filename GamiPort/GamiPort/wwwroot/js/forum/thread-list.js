// wwwroot/js/forum/thread-list.js
export const ThreadList = {
    props: {
        forumId: { type: Number, required: true },
        forumName: { type: String, required: false }
    },

    template: `
  <section>
    <!-- 標題 + 排序 + 新增貼文 -->
    <div class="d-flex align-items-center justify-content-between mb-3">
      <h3 class="m-0">🏷️ {{ forumName || '主題列表' }}</h3>
      <div class="d-flex align-items-center gap-2">
        <div class="btn-group me-2">
          <button class="btn btn-outline-secondary" :class="{active: sort==='newest'}" @click="changeSort('newest')">最新</button>
          <button class="btn btn-outline-secondary" :class="{active: sort==='oldest'}"  @click="changeSort('oldest')">最舊</button>
          <button class="btn btn-outline-secondary" :class="{active: sort==='mostLiked'}" @click="changeSort('mostLiked')">最熱</button>
        </div>
        <button class="btn btn-primary" data-bs-toggle="collapse" data-bs-target="#postBox">新增貼文</button> <!-- ⬅︎ 新增 -->
      </div>
    </div>

    <!-- 發文區（collapse） ⬅︎ 新增 -->
    <div id="postBox" class="collapse mb-3">
      <div class="card">
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

    <ul v-if="!loading" class="list-group mb-3">
      <li v-for="t in items" :key="t.threadId" class="list-group-item d-flex justify-content-between align-items-start">
        <a :href="'/Forum/Threads/Detail?threadId=' + t.threadId" class="link-underline link-underline-opacity-0 fw-semibold">
          {{ t.title }}
        </a>
        <small class="text-muted">回覆 {{ t.replyCount }} ・ {{ formatDate(t.updatedAt ?? t.createdAt) }}</small>
      </li>
      <li v-if="items.length===0" class="list-group-item text-muted">目前沒有主題</li>
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
            sort: 'newest',
            page: 1,
            size: 20,
            total: 0,
            items: [],
            loading: false,
            error: '',
            // 發文用 ⬅︎ 新增
            newTitle: '',
            newContent: '',
            creating: false
        };
    },

    computed: {
        pages() { return Math.max(1, Math.ceil(this.total / this.size)); }
    },

    methods: {
        mapSort(ui) {
            switch (ui) {
                case 'newest': return 'lastReply';
                case 'oldest': return 'created';
                case 'mostLiked': return 'hot';
                default: return 'lastReply';
            }
        },

        // 後端 JSON（PascalCase）→ 前端 camelCase
        normalize(data) {
            const raw = Array.isArray(data) ? data : (data.items ?? data.Items ?? []);
            const items = (raw ?? []).map(x => ({
                threadId: x.threadId ?? x.ThreadId ?? x.id ?? x.Id,
                title: x.title ?? x.Title ?? '',
                status: x.status ?? x.Status ?? '',
                createdAt: x.createdAt ?? x.CreatedAt ?? null,
                updatedAt: x.updatedAt ?? x.UpdatedAt ?? null,
                replyCount: x.replyCount ?? x.Replies ?? x.replies ?? 0,
                isOwner: x.isOwner ?? x.IsOwner ?? false,
                canDelete: x.canDelete ?? x.CanDelete ?? false
            }));
            const total = Array.isArray(data) ? items.length : (data.total ?? data.Total ?? items.length);
            return { items, total };
        },

        async fetchThreads() {
            if (!this.forumId) { this.error = 'forumId 缺失'; return; }
            this.loading = true; this.error = '';
            try {
                const url = `/api/forums/${this.forumId}/threads?sort=${this.mapSort(this.sort)}&page=${this.page}&size=${this.size}`;
                const res = await fetch(url, { headers: { 'Accept': 'application/json' }, credentials: 'include' });
                if (!res.ok) throw new Error(`API 失敗：${res.status} ${res.statusText}`);
                const data = await res.json();
                const { items, total } = this.normalize(data);
                this.items = items; this.total = total;
            } catch (e) {
                this.items = []; this.total = 0; this.error = e?.message ?? String(e);
            } finally {
                this.loading = false;
            }
        },

        changeSort(s) { if (this.sort !== s) { this.sort = s; this.page = 1; this.fetchThreads(); } },
        next() { if (this.page < this.pages) { this.page++; this.fetchThreads(); } },
        prev() { if (this.page > 1) { this.page--; this.fetchThreads(); } },

        formatDate(v) {
            if (!v) return '';
            const d = (v instanceof Date) ? v : new Date(v);
            return isNaN(d) ? '' : d.toLocaleString();
        },

        // ⬅︎ 新增：發文
        async createThread() {
            if (!this.newTitle.trim()) { alert('請輸入標題'); return; }
            this.creating = true;
            try {
                const body = { forumId: this.forumId, title: this.newTitle.trim(), contentMd: this.newContent.trim() };
                const r = await fetch('/api/forum/threads', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    credentials: 'include',              // 需要帶 cookie 才會過 [Authorize]
                    body: JSON.stringify(body)
                });

                if (r.status === 401) { alert('請先登入'); return; }
                if (!r.ok) throw new Error(`發文失敗：${r.status}`);

                const data = await r.json().catch(() => ({})); // 後端回 { threadId }
                // 清表單 + 收起發文區 + 重新載入列表
                this.newTitle = ''; this.newContent = '';
                const collapseEl = document.getElementById('postBox');
                if (collapseEl && window.bootstrap) {
                    try { window.bootstrap.Collapse.getOrCreateInstance(collapseEl).hide(); } catch { }
                }
                await this.fetchThreads();

                // 可選：直接導到詳情
                // if (data?.threadId) location.href = '/Forum/Threads/Detail?threadId=' + data.threadId;

            } catch (e) {
                alert(e?.message ?? e);
            } finally {
                this.creating = false;
            }
        }
    },

    mounted() { this.fetchThreads(); }
};
