import { api } from './api.js';

export function ThreadList({ mount, forumId }) {
    const state = { sort: 'lastReply', page: 1, size: 10, total: 0, items: [] };

    const fmt = s => new Date(s).toLocaleString('zh-TW', { hour12: false });
    const $list = document.getElementById(mount);
    const $pageInfo = document.getElementById('pageInfo');
    const $prev = document.getElementById('prevBtn');
    const $next = document.getElementById('nextBtn');

    async function load() {
        $list.innerHTML = '<div class="text-muted p-3">載入中...</div>';
        const { items, total } = await api.getThreads({
            forumId, sort: state.sort, page: state.page, size: state.size
        });
        state.items = items; state.total = total;
        render();
    }

    function render() {
        const html = state.items.map(t => `
      <div class="thread-row">
        <div class="thread-score">
          <span class="badge-score ${((t.up ?? 0) >= (t.down ?? 0)) ? 'badge-up' : 'badge-down'}">
            ${((t.up ?? 0) - (t.down ?? 0)) >= 0 ? '+' : ''}${(t.up ?? 0) - (t.down ?? 0)}
          </span>
        </div>
        <div class="thread-main">
          <div class="thread-tags">
            ${t.isPinned ? '<span class="tag tag-pin">置頂</span>' : ''}
            ${t.isAdmin ? '<span class="tag tag-admin">管理員</span>' : ''}
            ${t.isInsight ? '<span class="tag tag-insight">洞察</span>' : ''}
            ${t.isAnnounce ? '<span class="tag tag-admin">公告</span>' : ''}
          </div>
          <h3 class="title"><a href="/t/${t.threadId}">${t.title}</a></h3>
          <div class="thread-meta">
            <span>發文：${fmt(t.createdAt)}</span>
            <span>最後回覆：${fmt(t.updatedAt || t.lastRepliedAt || t.createdAt)}</span>
          </div>
        </div>
        <div class="thread-actions"><span class="replies">回覆 ${t.replies ?? 0}</span></div>
      </div>
    `).join('') || '<div class="text-muted p-3">沒有資料</div>';

        $list.innerHTML = html;

        const totalPages = Math.max(1, Math.ceil(state.total / state.size));
        $pageInfo.textContent = `${state.page} / ${totalPages}`;
        $prev.disabled = state.page <= 1;
        $next.disabled = state.page >= totalPages;
    }

    function bindSortTabs() {
        document.querySelectorAll('#sortTabs .nav-link').forEach(a => {
            a.addEventListener('click', e => {
                e.preventDefault();
                document.querySelectorAll('#sortTabs .nav-link').forEach(n => n.classList.remove('active'));
                a.classList.add('active');
                if (a.dataset.sort) { state.sort = a.dataset.sort; state.page = 1; }
                // 若之後要支援 filter，可加到 state 再傳給 API
                load();
            });
        });
    }

    function bindPager() {
        $prev.addEventListener('click', () => { state.page = Math.max(1, state.page - 1); load(); });
        $next.addEventListener('click', () => { state.page = state.page + 1; load(); });
    }

    // 公開方法（需要就擴充）
    return {
        init() { bindSortTabs(); bindPager(); load(); },
        reload() { load(); },
        setForumId(id) { forumId = id; state.page = 1; load(); }
    }
}
