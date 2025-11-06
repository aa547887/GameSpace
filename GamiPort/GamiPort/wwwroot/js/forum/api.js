//// 可換成你的實際 forumId（也可由 Razor 注入到 window.FORUM_ID）
//const BASE = '';

//async function getThreads({ forumId, sort, page, size }) {
//    // 先嘗試打 API；失敗就退回假資料
//    try {
//        const url = `${BASE}/api/forum/threads?forumId=${forumId}&sort=${sort}&page=${page}&size=${size}`;
//        const res = await fetch(url, { credentials: 'same-origin' });
//        if (!res.ok) throw new Error('api fail');
//        const items = await res.json();
//        return { items, total: 9999 }; // 你的 API 之後可以回 total，我先給大數避免卡住
//    } catch {
//        // 假資料 fallback（跟你原本的一樣）
//        const mockData = [
//            {
//                threadId: 101, title: "版規更新：洗版與廣告處置", authorUser: "Moderator",
//                createdAt: "2025-10-10T08:10:00Z", updatedAt: "2025-10-14T03:20:00Z",
//                up: 56, down: 3, replies: 124, isPinned: true, isAnnounce: true, isAdmin: true, isInsight: false
//            },
//            {
//                threadId: 102, title: "玩家留存下滑？三個關鍵洞察屁股", authorUser: "分析師A",
//                createdAt: "2025-10-13T01:00:00Z", updatedAt: "2025-10-14T02:05:00Z",
//                up: 120, down: 12, replies: 88, isPinned: false, isAnnounce: false, isAdmin: false, isInsight: true
//            },
//            {
//                threadId: 103, title: "新手入門小遊戲清單", authorUser: "阿明",
//                createdAt: "2025-10-12T09:00:00Z", updatedAt: "2025-10-13T21:15:00Z",
//                up: 42, down: 1, replies: 33, isPinned: false, isAnnounce: false, isAdmin: false, isInsight: false
//            },
//        ];

//        // 排序/分頁邏輯（與你原本一致）
//        const hot = t => (t.up * 2 + t.replies * 1.5 - t.down);
//        let rows = [...mockData];
//        if (sort === 'created') rows.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
//        else if (sort === 'hot') rows.sort((a, b) => hot(b) - hot(a));
//        else rows.sort((a, b) => new Date(b.updatedAt) - new Date(a.updatedAt));

//        const start = (page - 1) * size;
//        const items = rows.slice(start, start + size);
//        return { items, total: rows.length };
//    }
//}

//export const api = { getThreads };
