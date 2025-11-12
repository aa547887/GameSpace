// 用 ESM 版 Swiper，不用管 <script> 載入順序
import Swiper from 'https://unpkg.com/swiper@9/swiper-bundle.esm.browser.min.js';

// 初始化輪播
export async function InsightCarousel() {
    const root = document.getElementById('insight-carousel-root');
    const wrapper = document.getElementById('insight-swiper-wrapper');

    try {
        // 抓前台文章資料（先用 /api/posts，之後你會做 pinned endpoint）
        const res = await fetch('/api/posts?page=1&size=10&type=insight', {
            headers: { accept: 'application/json' }
        });
        if (!res.ok) throw new Error('fetch failed');

        const raw = await res.json();

        // API 可能是 array 或返回 {Items: [...]}
        const list = Array.isArray(raw) ? raw : (raw.items ?? raw.Items ?? []);

        // 只要置頂文章
        const pinned = list.filter(x => (x.pinned ?? x.Pinned) === true);

        // 如果沒有置頂文章 → 直接不顯示輪播
        if (!pinned.length) {
            root?.remove?.();
            return;
        }

        // ——————————————————————
        // 🔥 摘要字串優先順序：
        // BodyPreview > tldr > BodyMd（純文字＋清除 Markdown）
        // ——————————————————————
        const stripMarkdown = (s = '') =>
            String(s)
                .replace(/```[\s\S]*?```/g, '')   // 移除 code block
                .replace(/`([^`]+)`/g, '$1')      // 移除 inline code `text`
                .replace(/[#*_>\-\+\[\]\(\)!]/g, ' ') // 移除常見 markdown 符號
                .replace(/\s+/g, ' ')             // 壓縮多餘空白
                .trim();

        const pickExcerpt = (x) => {
            const raw =
                x.bodyPreview ?? x.BodyPreview ??
                x.tldr ?? x.Tldr ??
                x.bodyMd ?? x.BodyMd ?? '';
            const clean = stripMarkdown(raw);
            return clean.length > 180 ? clean.slice(0, 180) + '…' : clean;
        };

        const getId = x => x.postId ?? x.PostId;
        const getTitle = x => x.title ?? x.Title ?? '';
        const getPublishedAt = x => x.publishedAt ?? x.PublishedAt ?? '';

        // 先把 pinned 存著，點擊時若沒有詳細 API 就用這份列表當 fallback
        const pinnedById = new Map(pinned.map(x => [String(getId(x)), x]));

        // 建立 HTML slides（改：不用 inline onclick，改用 data-*）
        wrapper.innerHTML = pinned.map(x => {
            const title = esc(getTitle(x));
            const excerpt = esc(pickExcerpt(x));
            const id = getId(x);
            const date = fmt(getPublishedAt(x));

            return `
        <div class="swiper-slide">
          <article class="news-card" style="cursor:pointer" data-post-id="${id}">
            <div class="news-tags">
              <span class="tag tag-insight">洞察</span>
              <span class="tag tag-pin">置頂</span>
            </div>
            <h3 class="news-title">${title}</h3>
            <p class="news-excerpt">${excerpt}</p>
            <div class="news-meta">${date} · Admin</div>
          </article>
        </div>`;
        }).join('');

        // 顯示輪播容器
        root.classList.remove('d-none');

        // 啟動 Swiper
        new Swiper('.hero-swiper', {
            loop: true,
            centeredSlides: true,
            slidesPerView: 'auto',
            spaceBetween: 16,
            autoplay: { delay: 4800, disableOnInteraction: false },
            pagination: { el: '.swiper-pagination', clickable: true },
            navigation: { nextEl: '.swiper-button-next', prevEl: '.swiper-button-prev' },
            speed: 550
        });

        // 事件委派：點卡片開 Modal
        wrapper.addEventListener('click', async (e) => {
            const card = e.target.closest('.news-card');
            if (!card) return;
            const postId = card.getAttribute('data-post-id');
            await openInsightModal(postId, pinnedById);
        });

    } catch (err) {
        console.error('Carousel error:', err);
        root?.remove?.(); // 壞就不要顯示
    }
}

// —————— Modal 開啟邏輯 ——————
async function openInsightModal(postId, pinnedById) {
    const $title = document.getElementById('insightModalTitle');
    const $meta = document.getElementById('insightModalMeta');
    const $body = document.getElementById('insightModalBody');
    if (!$title || !$meta || !$body) return;

    // 先清空
    $title.textContent = '讀取中…';
    $meta.textContent = '';
    $body.textContent = '載入中…';

    // 嘗試打「單筆 API」，沒有就用 pinned 列表當後備
    let item = null;
    try {
        // 你若有 /api/posts/{id} 就會成功；沒有會丟錯，下面會 fallback
        const res = await fetch(`/api/posts/${postId}`, { headers: { accept: 'application/json' } });
        if (!res.ok) throw new Error('detail not found');
        item = await res.json();
    } catch {
        // 後備：用一開始的 pinned 資料
        item = pinnedById.get(String(postId)) ?? null;
    }
    if (!item) {
        $title.textContent = '找不到這篇文章';
        $body.textContent = '可能已被移除或權限不足。';
        showModal();
        return;
    }

    // 取欄位（大小寫雙軌）
    const title = item.title ?? item.Title ?? '';
    const publishedAt = item.publishedAt ?? item.PublishedAt ?? '';
    const author = item.authorName ?? item.AuthorName ?? 'Admin';
    const bodyMd = item.bodyMd ?? item.BodyMd ?? item.body ?? item.Body ?? '';

    // 超保守的 Markdown → HTML（不引第三方，避免 XSS）
    const safeHtml = mdToSafeHtml(bodyMd);

    // 填入
    $title.textContent = title;
    $meta.textContent = `${fmt(publishedAt)} · ${author}`;
    $body.innerHTML = safeHtml;

    // 開
    showModal();
}

function showModal() {
    // 需要 Bootstrap 5 的 JS：<script src=".../bootstrap.bundle.min.js"></script>
    const modalEl = document.getElementById('insightModal');
    if (!modalEl) return;
    // eslint-disable-next-line no-undef
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    modal.show();
}

// —————— 小工具：極簡 Markdown → 安全 HTML（保留段落與連結） ——————
function mdToSafeHtml(src = '') {
    // 逃逸所有 HTML，避免 XSS
    let s = esc(String(src));

    // 換行 → <br>
    s = s.replace(/\n{2,}/g, '</p><p>')
        .replace(/\n/g, '<br>');

    // 極簡網址偵測 [text](url) → <a>
    s = s.replace(/\[([^\]]+)\]\((https?:\/\/[^\s)]+)\)/g, '<a href="$2" target="_blank" rel="noopener">$1</a>');

    // 包成段落
    return `<p>${s}</p>`;
}

// —————— 其他工具 ——————
function esc(s = '') {
    return s.replace(/[&<>"']/g, m => (
        { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": "&#39;" }[m]
    ));
}
function fmt(d) {
    return d
        ? new Date(d).toLocaleDateString('zh-TW', { year: 'numeric', month: '2-digit', day: '2-digit' })
        : '';
}
