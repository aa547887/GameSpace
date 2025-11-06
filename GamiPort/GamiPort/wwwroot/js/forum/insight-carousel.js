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
                .replace(/[#*_>\-\+\[\]\(\)!]/g, ' ') // 移除常見 markdown 標記符號
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

        // 建立 HTML slides
        wrapper.innerHTML = pinned.map(x => {
            const title = esc(x.title ?? x.Title ?? '');
            const excerpt = esc(pickExcerpt(x));
            const id = x.postId ?? x.PostId;
            const date = fmt(x.publishedAt ?? x.PublishedAt);

            return `
        <div class="swiper-slide">
          <article class="news-card" style="cursor:pointer"
                   onclick="location.href='/Forum/Insights/Detail?postId=${id}'">
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

    } catch (err) {
        console.error('Carousel error:', err);
        root?.remove?.(); // 壞就不要顯示
    }
}

// —————— 工具函式 ——————
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
