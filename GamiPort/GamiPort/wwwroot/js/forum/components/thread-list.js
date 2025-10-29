// 顯示某看板的主題列表；點擊導到 /Forum/Threads/Detail?threadId=xxx
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
        <li v-for="t in items" :key="t.threadId || t.id"
            class="list-group-item d-flex justify-content-between">
          <a :href="'/Forum/Threads/Detail?threadId=' + (t.threadId ?? t.id)"
             class="link-underline link-underline-opacity-0">
            {{ t.title }}
          </a>
          <small class="text-muted">
            回覆 {{ t.repliesCount ?? 0 }} · 讚 {{ t.likeCount ?? 0 }} · {{ t.lastReplyAt ?? t.createdAt }}
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
    data() { return { sort: 'newest', page: 1, size: 20, total: 0, items: [], loading: false, error: '' }; },
    computed: { pages() { return Math.max(1, Math.ceil(this.total / this.size)); } },
    methods: {
        async fetchThreads() {
            if (!this.forumId) { this.error = '缺少 forumId'; return; }
            this.loading = true; this.error = '';
            try {
                const url = `/api/forums/${forumId}/threads?sort=${sort}&page=${page}&size=${size}`;
                const res = await fetch(url, { headers: { 'Accept': 'application/json' } });
                if (!res.ok) throw new Error('API 失敗：' + url + ' ' + res.status);
                const data = await res.json();
                this.items = data.items ?? data ?? [];
                this.total = data.total ?? (Array.isArray(data) ? data.length : 0);
            } catch (err) { this.error = err.message ?? String(err); }
            finally { this.loading = false; }
        },
        changeSort(s) { this.sort = s; this.page = 1; this.fetchThreads(); },
        next() { if (this.page < this.pages) { this.page++; this.fetchThreads(); } },
        prev() { if (this.page > 1) { this.page--; this.fetchThreads(); } }
    },
    mounted() { this.fetchThreads(); }
};
