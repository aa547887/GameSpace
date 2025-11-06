// 顯示所有論壇；點擊導到 /Forum/Threads/Index?forumId=xxx
export const ForumList = {
    template: `
    <section>
      <ul v-if="!loading" class="list-group mb-3">
        <li v-for="f in items" :key="f.ForumId" class="list-group-item d-flex justify-content-between">
          <a :href="'/Forum/Threads/Index?forumId=' + f.ForumId" class="fw-semibold">
            {{ f.Name }}
          </a>
          <small class="text-muted">{{ f.Description || '—' }}</small>
        </li>
        <li v-if="items.length===0" class="list-group-item text-muted">目前沒有論壇</li>
      </ul>
      <div v-else>載入中…</div>
      <div v-if="error" class="alert alert-danger mt-  2">{{ error }}</div>
    </section>
  `,
    data() { return { items: [], loading: false, error: '' }; },
    async mounted() {
        this.loading = true; this.error = '';
        try {
            const res = await fetch('/api/forums', { headers: { Accept: 'application/json' } });
            if (!res.ok) throw new Error('API 失敗：/api/forums ' + res.status);
            const data = await res.json();
            this.items = Array.isArray(data) ? data : (data.items ?? []);
            console.debug('[ForumList] items:', this.items.length);
        } catch (e) { this.error = e.message ?? String(e); }
        finally { this.loading = false; }
    }
};
