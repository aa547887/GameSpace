// wwwroot/js/forum/leaderboard-widget.js
export default {
    props: { limit: { type: Number, default: 10 }, date: { type: String, default: null } },
    data() {
        return { loading: true, error: '', items: [], dateStr: '', prevDateStr: '' };
    },
    async mounted() {
        try {
            const q = new URLSearchParams();
            if (this.limit) q.set('limit', String(this.limit));
            if (this.date) q.set('date', this.date);
            const url = `${window.location.origin}/api/forum/leaderboard/daily-with-delta${q.toString() ? `?${q.toString()}` : ''}`;

            const res = await fetch(url, { headers: { accept: 'application/json' } });
            if (!res.ok) { const body = await res.text().catch(() => ''); throw new Error(`HTTP ${res.status} ${res.statusText} :: ${body.slice(0, 200)}`); }

            const data = await res.json();
            this.dateStr = data?.date ?? data?.Date ?? '';
            this.prevDateStr = data?.prevDate ?? data?.PrevDate ?? '';
            const raw = Array.isArray(data?.items ?? data?.Items) ? (data.items ?? data.Items) : [];
            this.items = raw.map(x => ({
                rank: x.rank ?? x.Rank,
                gameId: x.gameId ?? x.GameId,
                name: x.name ?? x.Name,
                index: x.index ?? x.Index,
                delta: x.delta ?? x.Delta,
                trend: x.trend ?? x.Trend
            }));
        } catch (err) { console.error('[leaderboard]', err); this.error = '排行榜暫時讀不到資料。'; this.items = []; }
        finally { this.loading = false; }
    },
    methods: {
        // Top3 給金/銀/銅，其餘灰
        rankClass(n) {
            return n === 1 ? 'lb-rank gold' : n === 2 ? 'lb-rank silver' : n === 3 ? 'lb-rank bronze' : 'lb-rank';
        },
        trendIcon(t) { return t === 'up' ? '▲' : t === 'down' ? '▼' : t === 'new' ? '★' : '—'; },
        trendClass(t) { return t === 'up' ? 'pill up' : t === 'down' ? 'pill down' : t === 'new' ? 'pill new' : 'pill same'; },
        indexWidth(v) { const x = Math.max(10, Math.min(100, Number(v || 0))); return `${x}%`; },
        formatIndex(n) { return Number.isInteger(n) ? String(n) : Number(n).toFixed(1); },
        deltaText(d) { return d == null ? '' : (d > 0 ? `+${d}` : `${d}`); },
    },
    template: `
  <div class="lb-card">
    <div v-if="loading">
      <div class="placeholder-glow mb-2" v-for="i in 5" :key="i"><span class="placeholder col-12"></span></div>
    </div>

    <template v-else>
      <div v-if="error" class="alert alert-warning py-2">{{ error }}</div>

      <ul v-else class="lb-list">
        <li v-for="x in items" :key="x.gameId" class="lb-item">
          <div :class="rankClass(x.rank)">{{ x.rank }}</div>

          <div class="lb-main">
            <div class="lb-title">
              <span class="lb-name">{{ x.name }}</span>
              <span :class="trendClass(x.trend)">
                <span class="me-1">{{ trendIcon(x.trend) }}</span>{{ x.delta!=null ? deltaText(x.delta) : (x.trend==='new'?'NEW':'') }}
              </span>
            </div>
            <div class="lb-meter">
              <div class="lb-bar" :style="{ width: indexWidth(x.index) }"></div>
              <div class="lb-index">指數 {{ formatIndex(x.index) }}</div>
            </div>
          </div>
        </li>
      </ul>

      <div class="lb-foot">
        <small class="text-muted">日期：{{ dateStr }} <span v-if="prevDateStr">（對比 {{ prevDateStr }}）</span></small>
      </div>
    </template>
  </div>
  `
};
