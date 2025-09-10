/* Chat 1-to-1 client for GameSpace social_hub (HttpOnly-safe, with unread badges)
   Requires: @microsoft/signalr (global "signalR")
   HTML needs:
     - <ul id="contactList">, <div id="peerTitle">, <div id="messages">
     - <form id="chatForm"> with <input id="messageInput"> and <button id="sendBtn">
     - anti-forgery input: <input name="__RequestVerificationToken" ...>  ← 建議在 #chatForm 內加入
     - meta (optional): <meta name="me-id" content="...">, <meta name="me-name" content="...">
*/

(() => {
    // ====== Config ======
    const CONFIG = {
        hubUrl: "/social_hub/chatHub",
        contactsUrl: "/social_hub/Chat/Contacts",
        historyUrl: "/social_hub/Chat/History",
        markReadUrl: "/social_hub/Chat/MarkRead",
        whoamiUrl: "/social_hub/Chat/WhoAmI",
        unreadUrl: "/social_hub/Chat/UnreadCounts",
        pageSize: 20,
        scrollThrottleMs: 250
    };

    // ====== Utils ======
    const $ = (sel) => document.querySelector(sel);
    function escapeHtml(s) {
        return String(s ?? "").replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    }
    function isNearBottom(el, th = 36) { return (el.scrollTop + el.clientHeight) >= (el.scrollHeight - th); }
    function throttle(fn, ms) {
        let last = 0;
        return (...args) => { const now = Date.now(); if (now - last >= ms) { last = now; fn(...args); } };
    }
    function getCsrfToken() {
        const inp = document.querySelector('input[name="__RequestVerificationToken"]');
        if (inp && inp.value) return inp.value;
        const meta = document.querySelector('meta[name="csrf-token"]');
        return meta ? (meta.getAttribute("content") || "") : "";
    }
    function fmtTime(iso) { try { return new Date(iso).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }); } catch { return ""; } }

    // ====== DOM refs ======
    const contactsUl = $("#contactList");
    const peerTitle = $("#peerTitle");
    const messagesEl = $("#messages");
    const form = $("#chatForm");
    const input = $("#messageInput");
    const sendBtn = $("#sendBtn");
    if (!contactsUl || !peerTitle || !messagesEl || !form || !input || !sendBtn) { console.error("[chat-1to1] Missing DOM nodes."); return; }
    if (!window.signalR) { console.error("[chat-1to1] signalR missing."); return; }

    // ====== State ======
    let meId = 0, meName = "訪客";
    let peerId = 0, peerName = "";
    let earliestIso = null;
    let loadingOld = false;
    const seenIds = new Set();
    const unreadMap = new Map(); // peerId -> count

    // ====== Self detection: meta -> whoami (no cookie access) ======
    async function initSelf() {
        const metaId = document.querySelector('meta[name="me-id"]')?.content;
        const metaNm = document.querySelector('meta[name="me-name"]')?.content;
        if (metaId && parseInt(metaId, 10) > 0) { meId = parseInt(metaId, 10); meName = metaNm || meName; return; }
        try {
            const r = await fetch(CONFIG.whoamiUrl, { headers: { "Accept": "application/json" }, credentials: "same-origin" });
            if (r.ok) { const me = await r.json(); if (me && me.id > 0) { meId = me.id; meName = me.name || meName; } }
        } catch { }
    }

    // ====== SignalR ======
    const conn = new signalR.HubConnectionBuilder().withUrl(CONFIG.hubUrl).build();

    // ReceiveDirect: { messageId, senderId, receiverId, content, sentAtIso }
    conn.on("ReceiveDirect", payload => {
        if (!payload) return;
        const fromId = payload.senderId ?? payload.fromId ?? 0;
        const toId = payload.receiverId ?? payload.toId ?? 0;
        const id = payload.messageId ?? payload.id ?? 0;
        const text = payload.content ?? "";
        const tIso = payload.sentAtIso ?? payload.timeIso ?? "";

        // 若不是目前開啟對話：更新對方 badge 後返回
        if (peerId === 0 || !((fromId === peerId && toId === meId) || (fromId === meId && toId === peerId))) {
            if (toId === meId && fromId !== peerId) {
                unreadMap.set(fromId, (unreadMap.get(fromId) || 0) + 1);
                bumpContactBadge(fromId);
            }
            return;
        }

        if (id && seenIds.has(id)) return;
        if (id) seenIds.add(id);

        const wasAtBottom = isNearBottom(messagesEl);
        renderMsg(id, fromId === meId, text, tIso, /*isRead*/ false);
        if (fromId === peerId) markRead(tIso); // 回報已讀
        if (wasAtBottom) messagesEl.scrollTop = messagesEl.scrollHeight;
    });

    // ReadReceipt({ fromUserId, upToIso })
    conn.on("ReadReceipt", info => {
        if (info && peerId === info.fromUserId) markMineReadUpTo(info.upToIso);
    });

    conn.on("Error", code => {
        if (code === "NOT_LOGGED_IN") alert("請先登入後再使用聊天功能。");
        else if (code === "NO_PEER") alert("尚未選擇對象。");
        else if (code === "RATE_LIMIT") alert("發送過快，請稍後再試。");
        else console.warn("Hub Error:", code);
    });

    async function startConn() {
        try { await conn.start(); await conn.invoke("RegisterUser", meName || ("User " + meId)); }
        catch (e) { console.error("SignalR connect failed:", e); setTimeout(startConn, 2000); }
    }

    // ====== Contacts + Unread ======
    async function loadContacts() {
        try {
            const [resUsers, resCounts] = await Promise.all([
                fetch(CONFIG.contactsUrl, { headers: { "Accept": "application/json" }, credentials: "same-origin" }),
                fetch(CONFIG.unreadUrl, { headers: { "Accept": "application/json" }, credentials: "same-origin" })
            ]);
            if (!resUsers.ok) return;
            const users = await resUsers.json();
            const counts = resCounts.ok ? await resCounts.json() : [];
            unreadMap.clear();
            (counts || []).forEach(x => unreadMap.set(x.peerId, x.count));

            contactsUl.innerHTML = "";
            (users || []).forEach(u => {
                const safe = escapeHtml(u.name || ("User " + u.id));
                const cnt = unreadMap.get(u.id) || 0;
                const badge = cnt ? ` <span class="badge bg-danger float-end" data-badge="${u.id}">${cnt}</span>` : `<span class="float-end" data-badge="${u.id}"></span>`;
                const li = document.createElement("li");
                li.innerHTML = `<button class="btn w-100 text-start" data-id="${u.id}" data-name="${safe}" style="border-radius:0; border-bottom:1px solid #eee;">${safe}${badge}</button>`;
                contactsUl.appendChild(li);
            });
        } catch { }
    }

    function bumpContactBadge(id) {
        const el = contactsUl.querySelector(`[data-badge="${id}"]`);
        if (!el) return;
        const cnt = unreadMap.get(id) || 0;
        el.classList.toggle("badge", cnt > 0);
        el.classList.toggle("bg-danger", cnt > 0);
        el.textContent = cnt > 0 ? String(cnt) : "";
    }

    contactsUl.addEventListener("click", e => {
        const btn = e.target.closest("button[data-id]");
        if (!btn) return;
        const id = parseInt(btn.dataset.id, 10);
        const name = btn.dataset.name;
        if (id === peerId) return;
        selectPeer(id, name);
    });

    async function selectPeer(id, name) {
        peerId = id; peerName = name || ("User " + id);
        peerTitle.textContent = `和 ${peerName} 的對話`;
        messagesEl.innerHTML = "";
        earliestIso = null;
        loadingOld = false;
        seenIds.clear();
        sendBtn.disabled = false;

        // 點開對話視窗時，先把該對象徽章歸零（實際已讀仍以 MarkRead 為準）
        unreadMap.set(id, 0);
        bumpContactBadge(id);

        await loadHistory(false);

        const lastSeenIso = latestFromPartnerIso();
        if (lastSeenIso) markRead(lastSeenIso);

        messagesEl.scrollTop = messagesEl.scrollHeight;
    }

    // ====== History ======
    async function loadHistory(isOlder) {
        if (!peerId) return;
        if (isOlder && loadingOld) return;
        loadingOld = !!isOlder;

        const url = new URL(CONFIG.historyUrl, window.location.origin);
        url.searchParams.set("peerId", String(peerId));
        url.searchParams.set("take", String(CONFIG.pageSize));
        if (isOlder && earliestIso) url.searchParams.set("before", earliestIso);

        try {
            const resp = await fetch(url, { headers: { "Accept": "application/json" }, credentials: "same-origin" });
            if (!resp.ok) { loadingOld = false; return; }
            const list = await resp.json();
            if (!Array.isArray(list) || list.length === 0) { loadingOld = false; return; }

            const prevHeight = messagesEl.scrollHeight;
            const prevTop = messagesEl.scrollTop;
            earliestIso = (list[0].time ?? list[0].Time);

            const usePrepend = !!isOlder;
            for (const m of list) {
                const id = m.messageId ?? m.MessageId ?? 0;
                const mine = m.isMine ?? m.IsMine;
                const text = m.content ?? m.Content ?? "";
                const tIso = m.time ?? m.Time;
                const read = m.isRead ?? m.IsRead;
                if (id && seenIds.has(id)) continue;
                if (id) seenIds.add(id);
                renderMsg(id, mine, text, tIso, read, usePrepend);
            }

            const newHeight = messagesEl.scrollHeight;
            messagesEl.scrollTop = isOlder ? (prevTop + (newHeight - prevHeight)) : messagesEl.scrollHeight;
        } catch { } finally {
            loadingOld = false;
        }
    }

    messagesEl.addEventListener("scroll", throttle(() => {
        if (messagesEl.scrollTop <= 20 && peerId) loadHistory(true);
    }, CONFIG.scrollThrottleMs));

    // ====== Render & helpers ======
    function renderMsg(id, isMine, content, timeIso, isRead, prepend = false) {
        const wrap = document.createElement("div");
        wrap.className = "message " + (isMine ? "mine" : "other");
        if (id) wrap.dataset.id = id;
        if (timeIso) wrap.dataset.timeIso = timeIso;
        wrap.innerHTML = `
      <div class="content">${escapeHtml(content)}</div>
      <div class="meta">
        <span class="time">${fmtTime(timeIso)}</span>
        ${isMine ? `<span class="readmark ${isRead ? 'on' : ''}">✓</span>` : ""}
      </div>`;
        if (prepend) messagesEl.insertAdjacentElement("afterbegin", wrap);
        else messagesEl.appendChild(wrap);
    }

    function markMineReadUpTo(upToIso) {
        const items = messagesEl.querySelectorAll(".message.mine .readmark");
        items.forEach(el => {
            const t = el.closest(".message")?.dataset.timeIso || "";
            if (t && upToIso && t <= upToIso) el.classList.add("on");
        });
    }

    function latestFromPartnerIso() {
        const nodes = Array.from(messagesEl.querySelectorAll(".message.other"));
        if (nodes.length === 0) return null;
        const last = nodes[nodes.length - 1];
        return last.dataset.timeIso || null;
    }

    async function markRead(upToIso) {
        if (!peerId || !upToIso) return;
        const token = getCsrfToken();
        try {
            await fetch(CONFIG.markReadUrl, {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded",
                    "RequestVerificationToken": token
                },
                credentials: "same-origin",
                body: new URLSearchParams({ peerId: String(peerId), upToIso })
            });
        } catch { }
        try { await conn.invoke("NotifyRead", peerId, upToIso); } catch { }
        // 既然我已經把對方最新訊息讀到 upToIso，徽章歸零
        unreadMap.set(peerId, 0);
        bumpContactBadge(peerId);
    }

    // ====== Send ======
    form.addEventListener("submit", async (e) => {
        e.preventDefault();
        const text = input.value.trim();
        if (!text || !peerId) return;
        const wasAtBottom = isNearBottom(messagesEl);
        try {
            await conn.invoke("SendMessageTo", peerId, text);
            input.value = "";
            if (wasAtBottom) messagesEl.scrollTop = messagesEl.scrollHeight;
        } catch (err) {
            console.error(err);
        }
    });

    // ====== Boot ======
    (async () => {
        await initSelf();      // 拿身分（meta / WhoAmI）
        await startConn();     // 連 Hub
        await loadContacts();  // 載聯絡人 + 徽章
        if (!meId) {
            const banner = document.createElement("div");
            banner.className = "alert alert-warning m-2";
            banner.textContent = "尚未登入，無法傳送訊息。請先從右上角登入。";
            document.querySelector(".dialog")?.insertAdjacentElement("afterbegin", banner);
            input.disabled = true;
            sendBtn.disabled = true;
        } else {
            input.disabled = false;
            sendBtn.disabled = (peerId === 0);
        }
    })();
})();
