/*!
 * GP.support - SupportHub 前端單例（/hubs/support）
 * 目的：前台/後台共用「同一條」SignalR 連線，避免各頁重複連線。
 *
 * ✅ 功能
 * - 自動重連（withAutomaticReconnect）
 * - 記住已加入的 ticket 群組，重連後自動 re-join（包含管理員簽章路線）
 * - 封裝 join/leave/on/off，頁面端好呼叫
 * - 以 Promise 暴露 started，讓頁面 await 連線完成再做事
 *
 * 🔧 進階
 * - 跨站覆寫：在載入本檔前設定 window.GP_SUPPORT_HUB_URL="https://localhost:7160/hubs/support"
 */
(function (global) {
    global.GP = global.GP || {};
    const HUB_URL = global.GP_SUPPORT_HUB_URL || "/hubs/support";

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(HUB_URL) // 需要跨站時，你的 CORS 已在前台 Program.cs 設定允許後台來源
        .withAutomaticReconnect()
        .build();

    // 已加入的 ticket（用於重連 re-join）
    // key: ticketId, val: { mode: 'user' | 'manager', managerId?, expires?, sig? }
    const joinedTicketMap = new Map();

    // 簡單事件管理（避免重複註冊）
    const boundHandlers = new Map(); // event -> Set(handler)

    async function startIfNeeded() {
        if (connection.state === "Connected" || connection.state === "Connecting")
            return;

        try {
            await connection.start();
            console.info("[support] started →", HUB_URL);
        } catch (err) {
            console.error("[support] start error:", err);
            // 短延遲重試（也可交給 withAutomaticReconnect）
            setTimeout(startIfNeeded, 1500);
            throw err;
        }
    }

    async function rejoinAll() {
        for (const [tid, meta] of joinedTicketMap.entries()) {
            try {
                if (meta.mode === "user") {
                    await connection.invoke("Join", tid);
                } else {
                    await connection.invoke("JoinAsManager", tid, meta.managerId, meta.expires, meta.sig);
                }
                console.info("[support] re-joined ticket:", tid, meta);
            } catch (err) {
                console.warn("[support] re-join failed:", tid, err);
            }
        }
    }

    connection.onreconnected(() => {
        console.info("[support] reconnected, rejoining tickets…");
        rejoinAll();
    });

    connection.onclose(() => {
        console.warn("[support] connection closed.");
    });

    const api = {
        /** 讓頁面 await 連線準備好 */
        started: (async () => { await startIfNeeded(); })(),

        /** 使用者加入：僅工單本人可加入（伺服端會檢查） */
        async joinTicket(ticketId) {
            await startIfNeeded();
            await connection.invoke("Join", ticketId);
            joinedTicketMap.set(ticketId, { mode: "user" });
            console.info("[support] joined ticket as user:", ticketId);
        },

        /** 管理員加入：需帶簽章與有效期（由後台頁面產生） */
        async joinTicketAsManager(ticketId, managerId, expires, sig) {
            await startIfNeeded();
            await connection.invoke("JoinAsManager", ticketId, managerId, expires, sig);
            joinedTicketMap.set(ticketId, { mode: "manager", managerId, expires, sig });
            console.info("[support] joined ticket as manager:", ticketId);
        },

        /** 離開 ticket 群組（也會從快取移除） */
        async leaveTicket(ticketId) {
            joinedTicketMap.delete(ticketId);
            try { await connection.invoke("Leave", ticketId); } catch { /* ignore */ }
            console.info("[support] left ticket:", ticketId);
        },

        /** 事件訂閱（e.g., "msg"） */
        on(event, handler) {
            if (!boundHandlers.has(event)) boundHandlers.set(event, new Set());
            const set = boundHandlers.get(event);
            if (!set.has(handler)) {
                set.add(handler);
                connection.on(event, handler);
            }
        },

        /** 事件退訂（若未提供 handler，則退訂該事件所有處理器） */
        off(event, handler) {
            if (!boundHandlers.has(event)) return;
            const set = boundHandlers.get(event);
            if (handler) {
                if (set.has(handler)) {
                    set.delete(handler);
                    connection.off(event, handler);
                }
            } else {
                // 全退
                for (const h of set) connection.off(event, h);
                set.clear();
            }
        }
    };

    global.GP.support = api;
})(window);
