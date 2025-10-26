export const ForumList = {
    data() {
        return { items: [], loading: false, err: null };
    },
    async mounted() {
        this.loading = true;
        try {
            const res = await fetch('/api/forums');
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            this.items = await res.json();   // ForumListItemDto[]
        } catch (e) {
            this.err = e.message;
        } finally {
            this.loading = false;
        }
    },
    template: `
    <div class="forum-list">
      <div v-if="loading" class="text-muted p-3">載入中...</div>
      <div v-else-if="err" class="text-danger p-3">錯誤：{{ err }}</div>
      <div v-else>
        <div v-if="items.length === 0" class="text-muted p-3">目前沒有論壇資料</div>

        <div v-for="f in items" :key="f.forumId" class="forum-row border-bottom py-2">
          <h5 class="mb-1">
            <a :href="'/Forum/Boards/Index/' + f.forumId" class="text-decoration-none">
              {{ f.displayName || f.DisplayName || f.name || f.Name || '(未命名)' }}
            </a>
          </h5>
          <small class="text-muted">
            <span v-if="f.gameName">遊戲：{{ f.gameName }}</span>
            <span v-if="f.threadCount" class="ms-2">主題數：{{ f.threadCount }}</span>
          </small>
        </div>
      </div>
    </div>
  `
};
