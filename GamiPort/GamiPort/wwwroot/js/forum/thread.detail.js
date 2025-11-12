// thread.detail.js
// 功能：讀主題 + 回覆列表，支援排序 / 分頁 / 主題按讚 / 發回覆
// 注意：後端路徑一律小寫 (/api/forum/threads/...)，避免大小寫踩雷。

import { createApp, reactive } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.prod.js';

// 1) 從 Razor 取 threadId
const root = document.getElementById('thread-detail-app');
const threadId = Number(root?.dataset?.threadId ?? 0);

// 2) 全域狀態（Vue 會追蹤變化 → 自動重繪畫面）
const state = reactive({
    thread: null,                    // 主題資料（已正規化）
    posts: [],                       // 回覆列表（已正規化）
    likeStatus: { isLikedByMe: false, likeCount: 0 }, // 主題按讚狀態（從 thread 帶）
    sort: 'oldest',                  // 'oldest' | 'newest' | 'mostLiked'（UI 值）
    page: 1,
    size: 20,
    total: 0,                        // 回覆總數（分頁用）
    loading: false,
    error: ''
});

// ---------- 小工具：把後端 PascalCase → 前端 camelCase，並補齊必要欄位 ----------
function normalizeThread(x) {
    if (!x) return null;
    const t = {
        threadId: x.threadId ?? x.ThreadId,
        forumId: x.forumId ?? x.ForumId,
        title: x.title ?? x.Title ?? '',
        status: x.status ?? x.Status ?? '',
        authorUserId: x.authorUserId ?? x.AuthorUserId ?? 0,
        createdAt: x.createdAt ?? x.CreatedAt ?? null,
        updatedAt: x.updatedAt ?? x.UpdatedAt ?? null,
        lastReplyAt: x.lastReplyAt ?? x.LastReplyAt ?? null,
        replyCount: x.replyCount ?? x.RepliesCount ?? 0,
        likeCount: x.likeCount ?? x.LikeCount ?? 0,
        isLikedByMe: x.isLikedByMe ?? x.IsLikedByMe ?? false,
        // 內文（你的 DTO 可能是 ContentMd / ContentHtml）
        contentMd: x.contentMd ?? x.ContentMd ?? '',
        contentHtml: x.contentHtml ?? x.ContentHtml ?? null
    };
    return t;
}

function normalizePostsPaged(json, page = state.page, size = state.size) {
    // 後端有時回 {items, total}，也可能 {Items, Total}，甚至直接回陣列 → 這裡一次兼容
    const raw = Array.isArray(json) ? json : (json.items ?? json.Items ?? []);
    const items = raw.map((p, i) => ({
        postId: p.postId ?? p.PostId,
        threadId: p.threadId ?? p.ThreadId ?? threadId,
        parentPostId: p.parentPostId ?? p.ParentPostId ?? null,
        authorId: p.authorId ?? p.AuthorId ?? p.authorUserId ?? p.AuthorUserId ?? 0,
        authorName: p.authorName ?? p.AuthorName ?? '', // 先顯示可用名字；沒給就空字串
        createdAt: p.createdAt ?? p.CreatedAt,
        updatedAt: p.updatedAt ?? p.UpdatedAt ?? null,
        contentMd: p.contentMd ?? p.ContentMd ?? p.bodyMd ?? p.BodyMd ?? '',
        contentHtml: p.contentHtml ?? p.ContentHtml ?? p.bodyHtml ?? p.BodyHtml ?? null,
        likeCount: p.likeCount ?? p.LikeCount ?? 0,
        isLikedByMe: p.isLikedByMe ?? p.IsLikedByMe ?? false,
        canDelete: p.canDelete ?? p.CanDelete ?? false,
        // 前端計算樓層（F）：(頁碼-1)*每頁 + 當頁索引 + 1
        floor: (page - 1) * size + i + 1
    }));

    const total = Array.isArray(json)
        ? items.length
        : (json.total ?? json.Total ?? items.length);

    return { items, total };
}

// UI 排序值 → 後端吃的 query 參數
// 後端常見接法是 'oldest' | 'newest' | 'mostliked'（全小寫！！）
// 這裡把 UI 'mostLiked' 轉成後端 'mostliked'，避免 404/分支沒進。
function mapSort(uiSort) {
    switch ((uiSort || '').toLowerCase()) {
        case 'newest': return 'newest';
        case 'mostliked':  // 使用者可能傳 mostliked（全小寫）
        case 'mostliked':  // 容錯：若你哪天改 UI 值也 OK
        case 'mostliked': return 'mostliked';
        case 'mostliked': return 'mostliked';
        case 'mostliked': return 'mostliked';
        // UI 如果是駝峰 mostLiked → 轉成 mostliked
        default:
            return 'oldest';
    }
}
// 修正：上面多行是保底容錯，其實保留最簡版即可（給你簡版）：
// function mapSort(uiSort) {
//   const s = (uiSort || '').toLowerCase();
//   if (s === 'newest') return 'newest';
//   if (s === 'mostliked' || s === 'mostliked') return 'mostliked';
//   return 'oldest';
// }

// ---------- API 請求區（全部加上錯誤處理與除錯 log） ----------
async function loadThread() {
    const url = `/api/forum/threads/${threadId}`;
    const r = await fetch(url, { headers: { 'Accept': 'application/json' } });
    if (!r.ok) throw new Error(`讀取主題失敗：${r.status}`);
    const json = await r.json();
    state.thread = normalizeThread(json);

    // 主題的 like 狀態直接從 thread 帶（你的 DTO 已含 IsLikedByMe/LikeCount）
    state.likeStatus = {
        isLikedByMe: !!state.thread.isLikedByMe,
        likeCount: Number(state.thread.likeCount ?? 0)
    };
}

async function loadPosts() {
    const sortForApi = mapSort(state.sort);
    const url = `/api/forum/threads/${threadId}/posts?sort=${encodeURIComponent(sortForApi)}&page=${state.page}&size=${state.size}`;
    console.debug('[posts] GET', url);
    const r = await fetch(url, { headers: { 'Accept': 'application/json' } });
    if (!r.ok) throw new Error(`讀取回覆失敗：${r.status}`);
    const json = await r.json();
    const { items, total } = normalizePostsPaged(json, state.page, state.size);
    state.posts = items;
    state.total = total;
    console.debug('[posts] loaded', { count: items.length, total });
}

// 若你的 GetThread 已含 isLikedByMe/likeCount，就不必再多打一支
async function loadLikeStatus() {
    const url = `/api/forum/threads/${threadId}/like/status`;
    const r = await fetch(url, { headers: { 'Accept': 'application/json' } });
    if (!r.ok) return; // 未登入或未實作就跳過
    const dto = await r.json(); // { isLiked: bool, likeCount: n }
    state.likeStatus = {
        isLikedByMe: !!(dto.isLiked ?? dto.IsLiked),
        likeCount: Number(dto.likeCount ?? dto.LikeCount ?? state.likeStatus.likeCount)
    };
}

// ---------- 互動動作 ----------
async function toggleLike() {
    const r = await fetch(`/api/forum/threads/${threadId}/like`, { method: 'POST' });
    if (!r.ok) throw new Error('按讚失敗（需要登入）');
    const { liked } = await r.json(); // { liked: true/false }
    state.likeStatus.isLikedByMe = !!liked;
    state.likeStatus.likeCount += liked ? 1 : -1; // 即時更新
}

async function sendReply(text) {
    const body = { contentMd: text, parentPostId: null };
    const r = await fetch(`/api/forum/threads/${threadId}/posts`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });
    if (!r.ok) throw new Error('送出回覆失敗（需要登入）');
    // 送出成功 → 重新載入回覆 & 同步主題回覆數
    await loadPosts();
    if (state.thread) state.thread.replyCount = Number(state.thread.replyCount ?? 0) + 1;
}

// ---------- Vue app ----------
createApp({
    data() {
        return { state, replyText: '' };
    },
    async mounted() {
        try {
            state.loading = true;
            await loadThread();
            await loadPosts();
            // 若需要可再啟用
            // await loadLikeStatus();
        } catch (e) {
            state.error = e?.message ?? String(e);
            console.error(e);
        } finally {
            state.loading = false;
        }
    },
    methods: {
        async changeSort(s) {
            state.sort = s;
            state.page = 1;
            await loadPosts();
        },

        async onToggleLike() {
            try { await toggleLike(); }
            catch (e) { alert(e.message ?? e); }
        },

        async onSendReply() {
            const text = this.replyText?.trim();
            if (!text) return;
            try {
                await sendReply(text);
                this.replyText = '';
            } catch (e) {
                alert(e.message ?? e);
            }
        }, // 👈 這裡一定要有逗號！！！

        formatDateTime(isoString) {
            if (!isoString) return '';
            const dateKey = GPTime.dateKey(isoString);
            const hm = GPTime.hm(isoString);
            const prettyDate = GPTime.prettyKey(dateKey);
            return `${prettyDate} ${hm}`;
        },

        async onTogglePostLike(p) {
            try {
                const r = await fetch(`/api/forum/posts/${p.postId}/like`, { method: 'POST' });
                if (!r.ok) throw new Error('請先登入或無權限');
                const { liked } = await r.json();
                p.isLikedByMe = !!liked;
                p.likeCount += liked ? 1 : -1;
            } catch (e) {
                alert(e.message ?? e);
            }
        },

        async onDeletePost(p) {
            if (!confirm("確定刪除這則回覆？")) return;
            try {
                const r = await fetch(`/api/forum/posts/${p.postId}`, { method: 'DELETE' });
                if (!r.ok) throw new Error('刪除失敗（需要登入或無權限）');
                state.posts = state.posts.filter(x => x.postId !== p.postId);
                state.total = Math.max(0, state.total - 1);
                state.posts.forEach((x, i) => x.floor = (state.page - 1) * state.size + i + 1);
            } catch (e) {
                alert(e.message ?? e);
            }
        }
    },


    // 注意：v-html 會渲染 HTML，請確保 contentHtml 來自你信任的來源（否則有 XSS 風險）
    template: `
  <section>
    <div v-if="state.error" class="alert alert-danger">{{ state.error }}</div>

    <div v-if="state.thread" class="mb-3">
      <h3 class="mb-1">{{ state.thread.title }}</h3>
      <div class="text-muted small">
        建立 {{ formatDateTime(state.thread.createdAt) }}
        · 最後回覆 {{ formatDateTime(state.thread.lastReplyAt || state.thread.updatedAt || state.thread.createdAt) }}
        · 回覆 {{ state.thread.replyCount }}
        · 讚 {{ state.likeStatus.likeCount }}
      </div>

      <div class="mt-2 d-flex align-items-center gap-2">
        <div class="btn-group">
          <button class="btn btn-outline-secondary" :class="{active: state.sort==='oldest'}" @click="changeSort('oldest')">由舊到新</button>
          <button class="btn btn-outline-secondary" :class="{active: state.sort==='newest'}" @click="changeSort('newest')">由新到舊</button>
          <button class="btn btn-outline-secondary" :class="{active: state.sort==='mostLiked'}" @click="changeSort('mostLiked')">最讚</button>
        </div>

        <button class="btn"
                :class="state.likeStatus.isLikedByMe ? 'btn-primary' : 'btn-outline-primary'"
                @click.prevent="onToggleLike">
          👍 {{ state.likeStatus.isLikedByMe ? '已讚' : '按讚' }}
        </button>
      </div>

      <div class="mt-3" v-html="state.thread.contentHtml || state.thread.contentMd"></div>
    </div>

    <div v-if="state.loading" class="text-muted">載入中...</div>

    <div v-for="p in state.posts" :key="p.postId" class="card mb-2">
      <div class="card-body">
        <div class="d-flex justify-content-between align-items-center">
          <div class="fw-semibold">
            <span class="badge text-bg-secondary me-2">{{ p.floor }}F</span>
            <a class="link-underline" :href="\`/MemberManagement/MyHome/User/\${p.authorId}\`">
              {{ p.authorName || ('user_' + p.authorId) }}
            </a>
          </div>
          <small class="text-muted">{{ formatDateTime(p.createdAt) }}</small>
        </div>

        <div class="mt-2" v-html="p.contentHtml || p.contentMd"></div>

        <div class="mt-2 d-flex gap-2 align-items-center">
          <!-- ✅ 回覆按讚 -->
          <button class="btn btn-sm"
                  :class="p.isLikedByMe ? 'btn-primary' : 'btn-outline-primary'"
                  @click.prevent="onTogglePostLike(p)">
            👍 {{ p.isLikedByMe ? '已讚' : '按讚' }}（{{ p.likeCount }}）
          </button>

          <!-- ✅ 刪除 -->
          <button v-if="p.canDelete"
                  class="btn btn-sm btn-outline-danger"
                  @click.prevent="onDeletePost(p)">
            刪除
          </button>
        </div>
      </div>
    </div>

    <div class="mt-3">
      <textarea class="form-control" rows="3" v-model="replyText" placeholder="登入後可回覆..."></textarea>
      <div class="mt-2">
        <button class="btn btn-primary" @click="onSendReply">送出回覆</button>
      </div>
    </div>

    <div class="mt-2 text-muted small" v-if="state.total > state.posts.length">
      共 {{ state.total }} 則回覆
    </div>
  </section>
  `
}).mount('#thread-detail-app');