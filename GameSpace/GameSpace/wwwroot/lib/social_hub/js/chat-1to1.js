/* [Chat 1-to-1 client for GameSpace social_hub]
   [雙模] 有 window.signalR → 即時；否則 → HTTP 輪詢
   [需要的 HTML 元素]
     - <div id="chatBox">                     [訊息容器]
     - <form id="chatSendForm">               [送訊息表單]
         ├─ <input id="messageInput" ...>     [輸入框]
         ├─ <input type="hidden" id="otherId" name="otherId" value="對方ID">
         ├─ <input type="hidden" id="isManagerDm" name="isManagerDm" value="true|false">
         └─ <input name="__RequestVerificationToken" ...>  [Anti-Forgery 必填]
   [可選] 若版型有放：<meta name="me-id" content="..."> / <meta name="me-name" content="...">
*/

(function () {
    // ====== [設定與 URL] ======
    const CONFIG = {
        hubUrl: "/social_hub/chatHub",            // [URL] Hub 端點
        sendUrl: "/social_hub/Chat/Send",         // [URL] POST 送訊息
        markReadUrl: "/social_hub/Chat/MarkRead", // [URL] POST 標記已讀
        historyUrl: "/social_hub/Chat/History",   // [URL] GET 取歷史/增量（otherId&after）
        pollMs: 2500                              // [輪詢間隔] 無 SignalR 時使用
    };

    // ====== [工具] ======
    const $ = sel => document.querySelector(sel);
    const esc = s => String(s ?? "").replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    const fmt = iso => { try { return new Date(iso).toLocaleString(); } catch { return ""; } };
    const isNearBottom = (el, th = 36) => (el.scrollTop + el.clientHeight) >= (el.scrollHeight - th);

    // ====== [DOM 需求] ======
    const chatBox = $("#chatBox");
    const form = $("#chatSendForm");
    const input = $("#messageInput");
    const otherId = parseInt($("#otherId")?.value || "0", 10) || 0;
    const isManagerDm = (String($("#isManagerDm")?.value || "false").toLowerCase() === "true");
    const csrf = document.querySelector('input[name="__RequestVerificationToken"]')?.value || "";

    if (!chatBox || !form || !input || !otherId || !csrf) {
        console.warn("[chat-1to1] 缺少必要 DOM 或 Anti-Forgery/otherId。此腳本不會啟動。");
        return;
    }

    // ====== [身分（可選）] ======
    // [說明] 僅用來在即時模式判斷「是否我方訊息」。若沒有 meta，仍可用：用 senderId 與 otherId 比較判斷。
    const meId = parseInt(document.querySelector('meta[name="me-id"]')?.content || "0", 10) || 0;
    const meName = document.querySelector('meta[name="me-name"]')?.content || "";

    // ====== [狀態] ======
    let lastIso = null;       // [游標] 已載入訊息的最後時間（拿增量）
    let conn = null;          // [SignalR 連線]
    let pollTimer = 0;        // [輪詢計時器]
    const seen = new Set();   // [去重] 防止重複插入

    // ====== [畫面插入訊息] ======
    function appendMessage({ id, mine, text, iso, isRead }) {
        if (id && seen.has(id)) return;
        if (id) seen.add(id);

        const wasBottom = isNearBottom(chatBox);
        const div = document.createElement("div");
        div.className = "d-flex justify-content-" + (mine ? "end" : "start") + " mb-2";
        if (id) div.setAttribute("data-id", String(id));
        if (iso) div.setAttribute("data-at", iso);

        div.innerHTML = `
      <div class="p-2 rounded ${mine ? "bg-primary text-white" : "bg-light"}" style="max-width:70%;">
        <div class="small text-break">${esc(text)}</div>
        <div class="text-end mt-1" style="font-size:.75rem;">
          <span class="text-muted">${fmt(iso)}</span>
          ${mine ? `<span class="ms-2 ${isRead ? "text-success" : "text-muted"}" data-read="badge">[${isRead ? "已讀" : "未讀"}]</span>` : ""}
        </div>
      </div>`;

        chatBox.appendChild(div);
        if (wasBottom) chatBox.scrollTop = chatBox.scrollHeight;

        // [看到對方訊息 → 回報已讀]
        if (!mine && iso) markRead(upToIso(iso));

        // [更新游標]
        if (!lastIso || (iso && iso > lastIso)) lastIso = iso;
    }

    // [把 ISO 正規化成完整 RFC3339（保守處理）]
    function upToIso(iso) {
        try {
            const d = new Date(iso);
            return isNaN(d.getTime()) ? null : d.toISOString();
        } catch { return null; }
    }

    // ====== [HTTP：取得增量] ======
    async function fetchDelta() {
        try {
            const url = new URL(CONFIG.historyUrl, window.location.origin); // [URL] /social_hub/Chat/History
            url.searchParams.set("otherId", String(otherId));
            url.searchParams.set("isManagerDm", String(isManagerDm));
            if (lastIso) url.searchParams.set("after", lastIso);

            const r = await fetch(url, { method: "GET", headers: { "Accept": "application/json" }, credentials: "same-origin" });
            if (!r.ok) return;
            const arr = await r.json();
            if (!Array.isArray(arr)) return;

            for (const m of arr) {
                appendMessage({
                    id: m.messageId ?? m.MessageId ?? 0,
                    mine: (m.isMine ?? m.IsMine) === true,
                    text: m.content ?? m.text ?? m.Content ?? m.Text ?? "",
                    iso: m.time ?? m.at ?? m.Time ?? m.At ?? null,
                    isRead: (m.isRead ?? m.IsRead) === true
                });
            }
        } catch { /* ignore */ }
    }

    // ====== [HTTP：標記已讀] ======
    async function markRead(iso) {
        if (!iso) return;
        try {
            await fetch(CONFIG.markReadUrl, { // [URL] /social_hub/Chat/MarkRead
                method: "POST",
                credentials: "same-origin",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded",
                    "RequestVerificationToken": csrf
                },
                body: new URLSearchParams({
                    otherId: String(otherId),
                    isManagerDm: String(isManagerDm),
                    upToIso: iso
                })
            });
        } catch { /* ignore */ }

        // [即時模式] 若 Hub 可用，順便發送回執給對方
        if (conn && conn.invoke) {
            try { await conn.invoke("NotifyRead", otherId, iso); } catch { /* ignore */ }
        }
    }

    // ====== [送出：Hub 優先，失敗回退 HTTP] ======
    form.addEventListener("submit", async (e) => {
        e.preventDefault();
        const txt = (input.value || "").trim();
        if (!txt) return;

        if (conn && conn.invoke) {
            try {
                await conn.invoke("SendMessageTo", otherId, txt); // [Hub 方法] SendMessageTo
                input.value = "";
                return;
            } catch (err) {
                console.warn("[Hub 送出失敗，改用 HTTP]", err);
            }
        }

        try {
            // [URL] /social_hub/Chat/Send
            const fd = new FormData(form);
            const r = await fetch(CONFIG.sendUrl, { method: "POST", body: fd, credentials: "same-origin" });
            if (r.ok) input.value = "";
        } catch (err) {
            console.error("[HTTP 送出失敗]", err);
        }
    });

    // ====== [即時模式：連線與事件] ======
    function setupSignalR() {
        if (!window.signalR) return false;

        conn = new signalR.HubConnectionBuilder()
            .withUrl(CONFIG.hubUrl)                // [URL] /social_hub/chatHub
            .withAutomaticReconnect()
            .build();

        // [事件] 有新訊息（我方/對方）
        // [payload] { messageId, senderId, receiverId, content, sentAtIso }
        conn.on("ReceiveDirect", payload => {
            if (!payload) return;
            const from = payload.senderId ?? 0;
            const to = payload.receiverId ?? 0;
            const isThisConversation = (from === otherId && to !== 0) || (to === otherId && from !== 0);
            if (!isThisConversation) return;

            // [判斷是否我方] 若 meId 可得，直接比對；否則以 senderId != otherId 視為我方
            const mine = meId > 0 ? (from === meId) : (from !== otherId);
            appendMessage({
                id: payload.messageId ?? 0,
                mine,
                text: payload.content ?? "",
                iso: payload.sentAtIso ?? null,
                isRead: mine ? false : true // [說明] 對方的訊息一顯示就會觸發 markRead；這裡先給 true/false 不影響
            });
        });

        // [事件] 對方回報已讀
        // [payload] { fromUserId, upToIso }
        conn.on("ReadReceipt", info => {
            if (!info || info.fromUserId !== otherId) return;
            const upTo = info.upToIso ? new Date(info.upToIso) : null;
            if (!upTo) return;
            chatBox.querySelectorAll('[data-read="badge"]').forEach(badge => {
                const atIso = badge.closest("[data-at]")?.getAttribute("data-at");
                if (!atIso) return;
                const t = new Date(atIso);
                if (t <= upTo) {
                    badge.textContent = "[已讀]";
                    badge.classList.remove("text-muted");
                    badge.classList.add("text-success");
                }
            });
        });

        conn.on("Error", code => {
            // [Hub 端錯誤提示]
            if (code === "NOT_LOGGED_IN") alert("請先登入後再使用聊天功能。");
            else if (code === "NO_PEER") alert("尚未選擇對象。");
            else if (code === "RATE_LIMIT") alert("發送過快，請稍後再試。");
            else console.warn("[Hub Error]", code);
        });

        conn.start().then(async () => {
            // [可選] 報上顯示名稱
            if (meName) { try { await conn.invoke("RegisterUser", meName); } catch { } }
            // [補漏] 連上後再拉一次增量
            await fetchDelta();
        }).catch(err => {
            console.error("[SignalR 連線失敗，退回輪詢]", err);
            // [退回輪詢]
            pollTimer = window.setInterval(fetchDelta, CONFIG.pollMs);
        });

        // [視窗可見 → 回報已讀]
        document.addEventListener("visibilitychange", () => {
            if (document.visibilityState === "visible" && lastIso) markRead(lastIso);
        });

        return true;
    }

    // ====== [啟動] ======
    (async () => {
        // [先拉一次增量，避免空白]
        await fetchDelta();

        // [有 signalR 就用即時；否則啟動輪詢]
        const ok = setupSignalR();
        if (!ok) pollTimer = window.setInterval(fetchDelta, CONFIG.pollMs);
    })();
})();
