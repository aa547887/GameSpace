// /wwwroot/js/forum/components/forum-list.js
console.log('[ForumList] file loaded v1003');

export const ForumList = {
    template: `
  <section class="rank-list">
    <!-- 🔘 排序工具列 -->
    <div class="rank-toolbar d-flex justify-content-end gap-2 mb-3">
      <button class="btn btn-sm btn-outline-secondary" @click="sortBy('id')">默認排序</button>
      <button class="btn btn-sm btn-outline-secondary" @click="sortBy('name')">名稱筆畫</button>
      <button class="btn btn-sm btn-outline-secondary" @click="sortBy('pop')">熱度排序</button>
      <button class="btn btn-sm btn-outline-secondary" @click="sortBy('random')">隨機</button>
    </div>

    <!-- 一列一張卡 -->
    <article class="rank-item" v-for="(f,i) in items" :key="f.ForumId"
             @click="go(f.ForumId)" role="button" tabindex="0">
      <div class="rank-num" :class="{ top1: i===0, top2: i===1, top3: i===2 }">{{ i+1 }}</div>

      <img class="rank-cover"
           :src="f.ImageUrl || '/images/placeholder/cover-320x120.jpg'"
           :alt="f.Name" loading="lazy"
           @error="onImgErr($event)" />

      <div class="rank-body">
        <h5 class="rank-title">
          <a :href="'/Forum/Threads/Index?forumId=' + f.ForumId" @click.stop>{{ f.Name }}</a>
        </h5>
        <div class="rank-meta">
          <span class="dot">人氣 {{ f.FakeViews.toLocaleString() }}</span>
          <span>主題 {{ f.FakeThreads }}</span>
        </div>
        <p class="rank-desc">{{ f.Description || '—' }}</p>
      </div>

      <div class="rank-watermark" aria-hidden="true"></div>
    </article>

    <div v-if="loading" class="text-muted mt-3">載入中…</div>
    <div v-if="error" class="alert alert-danger mt-3">{{ error }}</div>
  </section>
  `,

    data() {
        return {
            items: [],
            loading: false,
            error: ''
        };
    },

    async mounted() {
        this.loading = true;
        try {
            const res = await fetch('/api/forums', { headers: { Accept: 'application/json' } });
            if (!res.ok) throw new Error('API 失敗：/api/forums ' + res.status);

            const data = await res.json();
            const arr = Array.isArray(data) ? data : (data.items ?? []);

            // ✅ 一次性生成穩定的假數據（基於 ForumId 的 seed → 排序時不會跳來跳去）
            this.items = arr.map(x => {
                const id = Number(x.ForumId ?? 0);
                const seed = this.hashSeed(String(id || x.Name || Math.random()));
                const FakeViews = this.randRange(seed, 3_000, 500_000);  // 3k ~ 500k
                const FakeThreads = this.randRange(seed * 7, 30, 800);   // 30 ~ 800
                return { ...x, FakeViews, FakeThreads };
            });

            // 預設先來個熱度排序，看起來比較「像真的」
            this.sortBy('pop');
            console.debug('[ForumList] items loaded:', this.items.length);
        } catch (e) {
            console.error('[ForumList] error:', e);
            this.error = e.message ?? String(e);
        } finally {
            this.loading = false;
        }
    },

    methods: {
        go(id) {
            location.href = `/Forum/Threads/Index?forumId=${id}`;
        },
        onImgErr(e) {
            e.target.src = '/images/placeholder/cover-320x120.jpg';
        },
        // 🔢 穩定 seed（字串→數字）
        hashSeed(s) {
            let h = 2166136261 >>> 0;
            for (let i = 0; i < s.length; i++) {
                h ^= s.charCodeAt(i);
                h = Math.imul(h, 16777619);
            }
            return (h >>> 1) || 1;
        },
        // 🎲 範圍亂數，但用 seed 產生「穩定隨機」
        randRange(seed, min, max) {
            // xorshift32
            let x = seed | 0;
            x ^= x << 13; x ^= x >>> 17; x ^= x << 5;
            const u = ((x >>> 0) / 0xFFFFFFFF);
            return Math.floor(min + u * (max - min + 1));
        },

        // 🔁 排序
        sortBy(type) {
            if (type === 'id') {
                this.items.sort((a, b) => (a.ForumId ?? 0) - (b.ForumId ?? 0));
            } else if (type === 'name') {
                const collator = new Intl.Collator('zh-Hant-u-co-stroke'); // 以筆畫排
                this.items.sort((a, b) => collator.compare(a.Name || '', b.Name || ''));
            } else if (type === 'pop') {
                // 熱度參考：80% 看人氣 + 20% 看主題數
                this.items.sort((a, b) => (b.FakeViews * 0.8 + b.FakeThreads * 100 * 0.2)
                    - (a.FakeViews * 0.8 + a.FakeThreads * 100 * 0.2));
            } else if (type === 'random') {
                // 真的亂序（這個才會每次都不同）
                this.items = this.items.slice().sort(() => Math.random() - 0.5);
            }
        }
    }
};
