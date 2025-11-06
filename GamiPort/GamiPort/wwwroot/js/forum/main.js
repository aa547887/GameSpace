//// 用瀏覽器 ESM
//import { createApp } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.prod.js';
//// ❗用「絕對路徑」，避免相對路徑在不同頁面失效
//import { ForumList } from '/js/forum/components/forum-list.js';

//console.debug('[main] boot');
//createApp({})
//    .component('forum-list', ForumList)   // ❗用 kebab-case 註冊，對應 <forum-list>
//    .mount('#forums-app');

//console.debug('[main] mounted #forums-app ✅');

import { createApp } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.prod.js';
// ☆ 重點：用「絕對路徑」(以 / 開頭)
import { ForumList } from '/js/forum/components/forum-list.js';

console.log('[main] boot');
createApp({})
    .component('forum-list', ForumList) // 跟 <forum-list> 對上
    .mount('#forums-app');
console.log('[main] mounted #forums-app ✅');
