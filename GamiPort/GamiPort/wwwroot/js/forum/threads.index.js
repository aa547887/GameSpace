// 必須：import 要在最上面（ESM 規則）
import { createApp } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.prod.js'
import { ThreadList } from './components/thread-list.js'

// 1) 先抓 DOM 與 forumId
const root = document.getElementById('thread-app')
const forumId = Number(root?.dataset?.forumId ?? 0)

// 2) 再 debug，這時候 forumId 已經初始化了
console.debug('[threads.index] loaded, forumId =', forumId)

// 3) 最後再 createApp；provide 用「函式」回傳，避免建立物件當下就取值
createApp({
    components: { ThreadList },
    provide() {
        return { forumId }   // 這裡此刻 forumId 已經有值
    },
    template: `<ThreadList />`
}).mount('#thread-app')

