// 用瀏覽器 ESM 直接從 CDN import（上面 cshtml 已經先載入 vue.esm-browser.prod.js）

import { createApp } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.prod.js';
import { ForumList } from './components/forum-list.js';
console.debug('[main] boot');
createApp({ components: { ForumList } }).mount('#forums-app');