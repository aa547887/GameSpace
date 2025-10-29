import { createApp, reactive } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.prod.js';

const root = document.getElementById('thread-detail-app');
const threadId = Number(root?.dataset?.threadId ?? 0);

const state = reactive({
    thread: null,
    posts: [],
    likeStatus: { isLikedByMe: false, likeCount: 0 },
    sort: 'oldest',
    loading: false,
    error: ''
});

async function loadThread() {
    const r = await fetch(`/api/Forum/threads/${threadId}`);
    if (!r.ok) throw new Error('讀取主題失敗');
    state.thread = await r.json();
}
async function loadPosts() {
    const r = await fetch(`/api/Forum/threads/${threadId}/posts?sort=${sort}`);
    if (!r.ok) throw new Error('讀取貼文失敗');
    state.posts = await r.json();
}
async function loadLikeStatus() {
    const r = await fetch(`/api/Forum/threads/${threadId}/like/status`);
    if (!r.ok) return;
    state.likeStatus = await r.json();
}

createApp({
    async mounted() {
        try {
            state.loading = true;
            await loadThread();
            await loadPosts();
            await loadLikeStatus();
        } catch (e) { state.error = e.message ?? String(e); }
        finally { state.loading = false; }
    },
    methods: {
        async changeSort(s) { state.sort = s; await loadPosts(); },
        // 這兩顆之後綁登入再開 POST
        async toggleLike() { /* POST /Forum/api/forum/threads/{id}/like */ },
        async sendReply() { /* POST /Forum/api/forum/threads/{id}/posts */ }
    },
    data() { return { state, replyText: '' }; },
    template: `
    <section>
      <div v-if="state.error" class="alert alert-danger">{{ state.error }}</div>

      <div v-if="state.thread" class="mb-3">
        <h3 class="mb-1">{{ state.thread.title }}</h3>
        <div class="text-muted small">
          作者 {{ state.thread.authorName }} · {{ state.thread.createdAt }} · 讚 {{ state.likeStatus.likeCount ?? 0 }}
        </div>
        <div class="btn-group mt-2">
          <button class="btn btn-outline-secondary" :class="{active: state.sort==='oldest'}" @click="changeSort('oldest')">由舊到新</button>
          <button class="btn btn-outline-secondary" :class="{active: state.sort==='newest'}" @click="changeSort('newest')">由新到舊</button>
          <button class="btn btn-outline-secondary" :class="{active: state.sort==='mostLiked'}" @click="changeSort('mostLiked')">最讚</button>
        </div>
        <button class="btn btn-outline-primary ms-2" disabled>按讚（未登入）</button>
      </div>

      <div v-if="state.loading" class="text-muted">載入中...</div>

      <div v-for="p in state.posts" :key="p.postId" class="card mb-2">
        <div class="card-body">
          <div class="d-flex justify-content-between">
            <div class="fw-semibold">{{ p.authorName }}</div>
            <small class="text-muted">{{ p.createdAt }}</small>
          </div>
          <div class="mt-2" v-html="p.bodyHtml ?? p.body"></div>
        </div>
      </div>

      <div class="mt-3">
        <textarea class="form-control" rows="3" v-model="replyText" placeholder="登入後可回覆..." disabled></textarea>
        <div class="mt-2">
          <button class="btn btn-primary" disabled>送出回覆（未登入）</button>
        </div>
      </div>
    </section>
  `
}).mount('#thread-detail-app');
