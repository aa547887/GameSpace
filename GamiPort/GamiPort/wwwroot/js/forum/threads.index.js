import { createApp } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.prod.js';
import { ThreadList } from './components/forum-list.js';

// 從 Razor 取 forumId（存在容器的 data-* 裡）
const root = document.getElementById('thread-app');
const forumId = Number(root?.dataset?.forumId ?? 0);

// 掛上「主題清單」
createApp({
    components: { ThreadList },
    provide: { forumId } // 用 provide/inject 傳給子元件
}).mount('#thread-app');
