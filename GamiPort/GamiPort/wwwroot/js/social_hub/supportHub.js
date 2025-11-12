/*!
 * GP.support - SupportHub 前端單例（/hubs/support）
 * 目的：前台/後台共用「同一條」SignalR 連線，避免各頁重複連線。
 *
 * ✅ 功能
 * - 自動重連（withAutomaticReconnect）
 * - 記住已加入的 ticket 群組，重連後自動 re-join（含管理員簽章路線）
 * - 封裝 join/leave/on/off，頁面端好呼叫
 * - 以 Promise 暴露 started，讓頁面 await 連線完成再做事
 * - nudgeTicketAsManager：管理員在寫入 DB 後主動 poke 一次群組，確保另一端立即刷新
 *
 * 🔧 進階
 * - 跨站覆寫：在載入本檔前設定 window.GP_SUPPORT_HUB_URL="https://localhost:7160/hubs/support"
 * - 除非你要靠 Cookie 驗證 Hub，否則無須 withCredentials；管理員走簽章 Join 即可。
 */
(function (global) {
    global.GP = global.GP || {};

    // 允許在 _Layout 事先覆寫 HUB URL（例如後台去連前台）
    const HUB_URL = global.GP_SUPPORT_HUB_URL || "/hubs/support";

    // 建立單一連線（全站共用）
    const connection = new signalR.HubConnectionBuilder()
        .withUrl(HUB_URL) // 跨站時，請在前台 Program.cs 設定適當 CORS
        .withAutomaticReconnect()
        .build();

    // 已加入的 ticket 快取（重連 re-join 用）
    // key: ticketId
    // val: { mode: 'user' | 'manager', managerId?, expires?, sig? }
    const joinedTicketMap = new Map();

    // 事件管理，避免重複註冊
    // eventName -> Set(handler)
    const boundHandlers = new Map();

    // 啟動連線（若尚未或正連線就略過）
    async function startIfNeeded() {
        if (connection.state === "Connected" || connection.state === "Connecting") return;
        try {
            await connection.start();
        //    console.info("[support] started →", HUB_URL);
        } catch (err) {
            console.error("[support] start error:", err);
            // 交給 automatic reconnect 之外，再保險補一個短延遲重試
            setTimeout(startIfNeeded, 1500);
            throw err;
        }
    }

    // 斷線重連後，將所有已加入的 ticket 重新加入群組
    async function rejoinAll() {
        for (const [tid, meta] of joinedTicketMap.entries()) {
            try {
                if (meta.mode === "user") {
                    await connection.invoke("Join", tid);
                } else {
                    await connection.invoke("JoinAsManager", tid, meta.managerId, meta.expires, meta.sig);
                }
            //    console.info("[support] re-joined ticket:", tid);
            } catch (err) {
                console.warn("[support] re-join failed:", tid, err);
            }
        }
    }

    // 自動重連後 re-join
    connection.onreconnected(rejoinAll);

    // 關閉通知（通常 automatic reconnect 會接手）
    connection.onclose(() => {
        console.warn("[support] connection closed.");
    });

    // === 封裝 API（掛到 global.GP.support） ===
    const api = {
        /** 讓頁面 await 這個 Promise，確保連線完成 */
        started: (async () => { await startIfNeeded(); })(),

        /** 使用者加入（伺服端會驗票屬於本人） */
        async joinTicket(ticketId) {
            await startIfNeeded();
            await connection.invoke("Join", ticketId);
            joinedTicketMap.set(ticketId, { mode: "user" });
        //    console.info("[support] joined ticket as user:", ticketId);
        },

        /** 管理員加入（需簽章+有效期；伺服端再做授權判斷） */
        async joinTicketAsManager(ticketId, managerId, expires, sig) {
            await startIfNeeded();
            await connection.invoke("JoinAsManager", ticketId, managerId, expires, sig);
            joinedTicketMap.set(ticketId, { mode: "manager", managerId, expires, sig });
        //    console.info("[support] joined ticket as manager:", ticketId);
        },

        /**
         * 管理員 poke 群組：當後台寫入 DB 成功後，主動廣播一次，
         * 讓前台（或其它視窗）立刻收到 "msg" → 觸發 reload。
         *（伺服端會驗簽章與授權）
         */
        async nudgeTicketAsManager(ticketId, managerId, expires, sig) {
            try {
                await startIfNeeded();
                await connection.invoke("NudgeAsManager", ticketId, managerId, expires, sig);
            //    console.info("[support] nudged ticket:", ticketId);
            } catch (err) {
                console.warn("[support] nudge failed:", err);
            }
        },

        /** 離開群組（並自快取移除） */
        async leaveTicket(ticketId) {
            joinedTicketMap.delete(ticketId);
            try { await connection.invoke("Leave", ticketId); } catch { /* ignore */ }
        //    console.info("[support] left ticket:", ticketId);
        },

        /** 事件訂閱（例："msg"、"ticket.message"、"joined"） */
        on(event, handler) {
            if (!boundHandlers.has(event)) boundHandlers.set(event, new Set());
            const set = boundHandlers.get(event);
            if (!set.has(handler)) {
                set.add(handler);
                connection.on(event, handler);
            }
        },

        /** 事件退訂（不給 handler 則退光該事件全部 handler） */
        off(event, handler) {
            if (!boundHandlers.has(event)) return;
            const set = boundHandlers.get(event);
            if (handler) {
                if (set.has(handler)) {
                    set.delete(handler);
                    connection.off(event, handler);
                }
            } else {
                for (const h of set) connection.off(event, h);
                set.clear();
            }
        },

        /** 方便除錯（非必要） */
        _conn: connection
    };

    global.GP.support = api;
})(window);
