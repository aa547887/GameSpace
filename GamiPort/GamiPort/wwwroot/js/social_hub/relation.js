/* ===================================================================
    前端腳本說明
    1) 提供 sendRelationCommand 函式，用於發送交友關係指令。
    2) 處理 Anti-Forgery Token。
    3) 處理後端回應，並顯示結果。
    =================================================================== */
(function () {
    const $ = (s) => document.querySelector(s);

    /**
     * 將「字串 / 空字串 / 數字字串」轉成合適型別（空值→null；可數字→Number）
     * @param {string|number|null} v - 待轉換的值
     * @returns {string|number|null} 轉換後的值
     */
    const toInt = (v) => (v == null || v === "" ? null : (isFinite(v) ? Number(v) : null));

    /**
     * 核心發送交友關係指令函式。
     * 負責組裝請求、發送 AJAX POST 到交友 API，並處理回應。
     * param {object} options - 包含指令發送所需參數的物件
     * param {string} options.url - 交友 API 的 URL (例如 /social_hub/relations/exec)
     * param {object} options.command - 交友指令的 payload 物件
     * param {number} options.command.actorUserId - 執行操作的使用者 ID
     * param {number} options.command.targetUserId - 目標使用者 ID
     * param {string} options.command.actionCode - 操作代碼 (e.g., "friend_request", "accept", "block")
     * param {string} [options.command.nickname] - 暱稱 (僅用於 "set_nickname")
     * returns {Promise<object>} 包含發送結果的 Promise，例如 { succeeded: true, noOp: false, newStatusCode: "ACCEPTED" } 或 { succeeded: false, reason: "錯誤訊息" }
     */
    async function sendRelationCommand(options) {
        const { url, command } = options;

        // 最小前端驗證：檢查必填欄位
        if (!command.actorUserId || !command.targetUserId || !command.actionCode) {
            alert('actorUserId / targetUserId / actionCode 為必填。');
            return { succeeded: false, reason: '缺少必填參數' };
        }

        // 顯示狀態訊息
        const statusEl = $('#rel-status');
        if (statusEl) statusEl.textContent = '送出中…';

        // 讀取 Anti-Forgery Token（若控制器有 [ValidateAntiForgeryToken]）
        const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenEl ? tokenEl.value : null;

        // 組裝請求 Headers
        const headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };
        // 若後端有啟用 Anti-Forgery，就把 token 放在自訂 Header
        if (token) headers['RequestVerificationToken'] = token;

        try {
            const res = await fetch(url, {
                method: 'POST',
                headers,
                credentials: 'same-origin', // 帶上同站 Cookie
                body: JSON.stringify(command)
            });

            const ct = res.headers.get('content-type') || '';
            const txt = await res.text();

            if (!res.ok) {
                alert(`失敗 (HTTP ${res.status})：${txt.slice(0, 200)}`);
                if (statusEl) statusEl.textContent = '';
                return { succeeded: false, reason: `HTTP ${res.status}: ${txt.slice(0, 200)}` };
            }
            if (!ct.includes('application/json')) {
                alert('伺服器回的不是 JSON（可能被轉址或打錯路由）。');
                if (statusEl) statusEl.textContent = '';
                return { succeeded: false, reason: '伺服器回應非 JSON' };
            }

            const data = JSON.parse(txt); // { noOp, relationId, newStatusCode, reason }
            const info = [
                `NoOp=${data.noOp}`,
                `relationId=${data.relationId ?? '—'}`,
                `status=${data.newStatusCode ?? '—'}`,
                data.reason ? `reason=${data.reason}` : null
            ].filter(Boolean).join(' | ');

            alert('OK！' + info);
            if (statusEl) statusEl.textContent = info;
            return { succeeded: true, ...data };
        } catch (err) {
            alert('例外：' + err.message);
            if (statusEl) statusEl.textContent = '';
            return { succeeded: false, reason: err.message };
        }
    }

    // 將 sendRelationCommand 函式暴露到全域 window 物件，以便其他腳本可以直接呼叫。
    window.sendRelationCommand = sendRelationCommand;
})();