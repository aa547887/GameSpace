// 超小 Vue 組件：打你的 /api/forums/{id}/threads，含排序 + 分頁
export const ThreadList = {
    props: { forumId: { type: Number, required: true } },
    data() {
        return {
            sort: 'lastReply', page: 1, size: 10,
            total: 0, items: [], loading: false, err: null
        };
    },
    computed: {
        totalPages() { return Math.max(1, Math.ceil(this.total / this.size)); }
    },
    methods: {
        fmt(s) { return s ? new Date(s).toLocaleString('zh-TW', { hour12: false }) : '-'; },
        async load() {
            this.loading = true; this.err = null;
            try {
                const url = `/api/forums/${this.forumId}/threads?sort=${this.sort}&page=${this.page}&size=${this.size}`;
                const res = await fetch(url, { headers: { 'Accept': 'application/json' } });
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                const data = await res.json(); // { items, page, size, total }
                this.items = data.items ?? [];
                this.page = data.page; this.size = data.size; this.total = data.total ?? 0;
            } catch (e) { this.err = e.message; }
            finally { this.loading = false; }
        },
        changeSort(s) { if (this.sort !== s) { this.sort = s; this.page = 1; this.load(); } },
        prev() { if (this.page > 1) { this.page--; this.load(); } },
        next() { if (this.page < this.totalPages) { this.page++; this.load(); } }
    },
    mounted() { this.load(); },
    template: `
  <div class="thread-list">
    <!-- 排序 Tab -->
    <ul class="nav nav-pills sort-tabs mb-2">
      <li class="nav-item"><a href="#" @click.prevent="changeSort('lastReply')" class="nav-link" :class="{active:sort==='lastReply'}">最新回覆</a></li>
      <li class="nav-item"><a href="#" @click.prevent="changeSort('created')"   class="nav-link" :class="{active:sort==='created'}">發文時間</a></li>
      <li class="nav-item"><a href="#" @click.prevent="changeSort('hot')"       class="nav-link" :class="{active:sort==='hot'}">熱門</a></li>
    </ul>

    <!-- 狀態 -->
    <div v-if="loading" class="text-muted p-3">載入中…</div>
    <div v-else-if="err" class="text-danger p-3">載入失敗：{{ err }}</div>
    <div v-else>
      <div v-if="items.length===0" class="text-muted p-3">沒有資料</div>

      <!-- 列表 -->
      <div v-for="t in items" :key="t.threadId" class="thread-row d-flex justify-content-between align-items-start py-2 border-bottom">
        <div class="thread-main">
          <h3 class="h6 mb-1">
            <a :href="'/t/'+t.threadId">{{ t.title ?? '(無標題)' }}</a>
          </h3>
          <div class="thread-meta small text-muted">
            <span>發文：{{ fmt(t.createdAt) }}</span>
            <span class="ms-3">最後回覆：{{ fmt(t.updatedAt) }}</span>
          </div>
        </div>
        <div class="thread-actions">
          <span class="badge bg-secondary">回覆 {{ t.replies ?? 0 }}</span>
        </div>
      </div>

      <!-- 分頁 -->
      <div class="pager mt-2 d-flex align-items-center gap-2">
        <button class="btn btn-outline-secondary btn-sm" @click="prev" :disabled="page<=1">上一頁</button>
        <span class="text-muted">{{ page }} / {{ totalPages }}</span>
        <button class="btn btn-outline-secondary btn-sm" @click="next" :disabled="page>=totalPages">下一頁</button>
      </div>
    </div>
  </div>
  `
};
