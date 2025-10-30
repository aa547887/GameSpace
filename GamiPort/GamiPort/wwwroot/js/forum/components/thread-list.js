// 顯示某看板的主題列表；點擊導到 /Forum/Threads/Detail?threadId=xxx
// 重點：後端可能回 PascalCase（Items/ThreadId/Replies...）
// 這支在前端統一「正規化成 camelCase」給模板吃。
export const ThreadList = {
    inject: ['forumId'],

    template: `
    <section>
      <div class="d-flex align-items-center justify-content-between mb-3">
        <h3 class="m-0">主題列表</h3>
        <div class="btn-group">
          <button class="btn btn-outline-secondary" :class="{active: sort==='newest'}" @click="changeSort('newest')">最新</button>
          <button class="btn btn-outline-secondary" :class="{active: sort==='oldest'}" @click="changeSort('oldest')">最舊</button>
          <button class="btn btn-outline-secondary" :class="{active: sort==='mostLiked'}" @click="changeSort('mostLiked')">最讚</button>
        </div>
      </div>

      <div v-if="error" class="alert alert-danger">{{ error }}</div>

      <ul v-if="!loading" class="list-group mb-3">
        <li v-for="t in items" :key="t.threadId || t.id" class="list-group-item d-flex justify-content-between">
          <a :href="'/Forum/Threads/Detail?threadId=' + (t.threadId ?? t.id)"
             class="link-underline link-underline-opacity-0">
            {{ t.title }}
          </a>
          <small class="text-muted">
            回覆 {{ t.replyCount ?? 0 }} ·
            {{ (t.updatedAt ?? t.createdAt) || '' }}
          </small>
        </li>
        <li v-if="items.length===0" class="list-group-item text-muted">目前沒有主題</li>
      </ul>

      <div class="d-flex align-items-center gap-2">
        <button class="btn btn-outline-secondary" @click="prev" :disabled="loading || page<=1">上一頁</button>
        <span class="small text-muted">第 {{ page }} / {{ pages }} 頁（{{ total }} 筆）</span>
        <button class="btn btn-outline-secondary" @click="next" :disabled="loading || page>=pages">下一頁</button>
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
            error: ''
        };
    },

    computed: {
        pages() { return Math.max(1, Math.ceil(this.total / this.size)); }
    },

    methods: {
        // UI 排序 → 後端需要的排序值
        mapSort(uiSort) {
            switch (uiSort) {
                case 'newest': return 'lastReply'; // 後端的 key（小寫開頭） 
                case 'mostLiked': return 'hot';
                case 'oldest': return 'created';
                default: return 'lastReply';
            }
        },

        // 🔧【關鍵】把後端回傳的欄位統一轉成 camelCase
        // - data.Items  → items   （大小寫對齊）
        // - ThreadId   → threadId
        // - Title      → title
        // - Status     → status
        // - CreatedAt  → createdAt
        // - UpdatedAt  → updatedAt
        // - Replies    → replyCount（我們這邊直接命名為 replyCount）
        normalizeResponse(data) {
            // 1) 先抓清單本體：容忍 Items / items / 直接 array 三種格式
            const rawItems = Array.isArray(data) ? data : (data.items ?? data.Items ?? []);

            // 2) 一筆一筆轉大小寫
            const items = (rawItems ?? []).map(x => ({
                // [大小寫對應] ThreadId / threadId / Id → threadId
                threadId: x.threadId ?? x.ThreadId ?? x.id ?? x.Id,
                // [大小寫對應] Title / title → title
                title: x.title ?? x.Title ?? '',
                // [大小寫對應] Status / status → status
                status: x.status ?? x.Status ?? '',
                // [大小寫對應] CreatedAt / createdAt → createdAt
                createdAt: x.createdAt ?? x.CreatedAt ?? null,
                // [大小寫對應] UpdatedAt / updatedAt → updatedAt
                updatedAt: x.updatedAt ?? x.UpdatedAt ?? null,
                // [大小寫對應] Replies / replies / replyCount → replyCount
                replyCount: x.replyCount ?? x.Replies ?? x.replies ?? 0
            }));

            // 3) 取總筆數：支援 total / Total；如果沒有就用 items.length
            const total = Array.isArray(data)
                ? items.length
                : (data.total ?? data.Total ?? items.length);

            return { items, total };
        },

        async fetchThreads() {
            if (!this.forumId) {
                this.error = '缺少 forumId（沒被 provide）';
                console.warn('[ThreadList] forumId missing');
                return;
            }

            this.loading = true;
            this.error = '';

            try {
                const sortForApi = this.mapSort(this.sort);
                const url = `/api/forums/${this.forumId}/threads?sort=${encodeURIComponent(sortForApi)}&page=${this.page}&size=${this.size}`;
                console.debug('[ThreadList] GET', url);

                const res = await fetch(url, { headers: { 'Accept': 'application/json' } });
                if (!res.ok) throw new Error(`API 失敗：${res.status} ${res.statusText}`);

                const data = await res.json();

                // ✨ 這裡把大小寫全轉成前端習慣的 camelCase
                const normalized = this.normalizeResponse(data);
                this.items = normalized.items;
                this.total = normalized.total;

                console.debug('[ThreadList] items=', this.items.length, 'total=', this.total, 'sample=', this.items[0]);
            } catch (err) {
                this.items = [];
                this.total = 0;
                this.error = err?.message ?? String(err);
                console.error('[ThreadList] error:', this.error);
            } finally {
                this.loading = false;
            }
        },

        changeSort(s) { this.sort = s; this.page = 1; this.fetchThreads(); },
        next() { if (this.page < this.pages) { this.page++; this.fetchThreads(); } },
        prev() { if (this.page > 1) { this.page--; this.fetchThreads(); } }
    },

    mounted() {
        console.debug('[ThreadList] mounted forumId=', this.forumId);
        this.fetchThreads();
    }
};
