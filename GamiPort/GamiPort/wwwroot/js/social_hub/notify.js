/* ===================================================================
    前端腳本說明
    1) 監聽整頁 click：凡是點擊到 [data-notify] 的按鈕都會觸發送出
    2) 從按鈕的 dataset 讀 data-* 值 → 組 payload（自動把數字字串轉成 Number）
    3) 檢查至少有一個收件人（toUserId 或 toManagerId）
    4) fetch POST：
         - headers:
             - Content-Type: application/json
             - Accept: application/json（伺服器回 HTML 時可快速分辨）
             - RequestVerificationToken: <token>（若控制器有 [ValidateAntiForgeryToken]）
         - credentials: 'same-origin'（帶上同站 Cookie）
    5) 回應處理：
         - 非 2xx → alert 前 200 字（快速辨識錯誤頁/登入頁等 HTML）
         - Content-Type 非 JSON → alert 提示（多半是被導轉或打到 View）
         - JSON → 解析後讀 data.notificationId（控制器以小駝峰回傳）
    =================================================================== */
(function () {
    const $ = (sel) => document.querySelector(sel);

    /**
     * 將「字串 / 空字串 / 數字字串」轉成合適型別（空值→null；可數字→Number）
     * @param {string|number|null} v - 待轉換的值
     * @returns {string|number|null} 轉換後的值
     */
    function toMaybeNumber(v) {
        if (v == null || v === "") return null;
        return isFinite(v) ? Number(v) : v;
    }

    /**
     * 從 HTML 元素的 dataset 屬性建立送給 API 的 payload 物件。
     * data-aaa-bbb 會被映射為 payload.aaaBbb。
     * @param {DOMStringMap} ds - HTML 元素的 dataset 物件
     * @returns {object} 建立好的 payload 物件
     */
    function buildPayloadFromDataset(ds) {
        const p = {
            sourceId: toMaybeNumber(ds.sourceId),
            actionId: toMaybeNumber(ds.actionId),
            toUserId: toMaybeNumber(ds.toUserId),
            toManagerId: toMaybeNumber(ds.toManagerId),
            groupId: toMaybeNumber(ds.groupId),
            senderUserId: toMaybeNumber(ds.senderUserId),
            senderManagerId: toMaybeNumber(ds.senderManagerId),
            title: ds.title ?? "",
            message: ds.message ?? null
        };
        // 移除 null/空字串鍵，避免把空值送到後端
        Object.keys(p).forEach(k => { if (p[k] === null || p[k] === "") delete p[k]; });
        return p;
    }

    /**
     * 核心發送通知函式。
     * 負責組裝請求、發送 AJAX POST 到通知 API，並處理回應。
     * @param {object} options - 包含通知發送所需參數的物件
     * @param {string} options.url - 通知 API 的 URL
     * @param {object} options.payload - 通知內容的 payload 物件
     * @returns {Promise<object>} 包含發送結果的 Promise，例如 { succeeded: true, notificationId: 123 } 或 { succeeded: false, reason: "錯誤訊息" }
     */
    async function sendNotification(options) {
        const { url, payload } = options;

        // 最小前端驗證：至少要有一個收件人
        if (!('toUserId' in payload) && !('toManagerId' in payload)) {
            alert('請在參數中至少設定一個收件人（toUserId 或 toManagerId）。');
            return { succeeded: false, reason: '缺少收件人' };
        }

        // 顯示狀態訊息
        const statusEl = $('#status');
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
                body: JSON.stringify(payload)
            });

            // 先以純文字拿回（方便在錯誤時印出 HTML 片段）
            const contentType = res.headers.get('content-type') || '';
            const text = await res.text();

            if (!res.ok) {
                // 例如 400/404/405/500，直接把前 200 字顯示方便排錯
                alert(`失敗 (HTTP ${res.status})：${text.slice(0, 200)}`);
                if (statusEl) statusEl.textContent = '';
                return { succeeded: false, reason: `HTTP ${res.status}: ${text.slice(0, 200)}` };
            }

            if (!contentType.includes('application/json')) {
                // 多半是被 302 導去登入頁或打到 View（HTML）
                alert('伺服器回應不是 JSON（可能被轉址到登入頁或打錯路由）。');
                if (statusEl) statusEl.textContent = '';
                return { succeeded: false, reason: '伺服器回應非 JSON' };
            }

            // 解析 JSON；控制器回小駝峰：{ "notificationId": 123 }
            const data = JSON.parse(text);
            const id = data.notificationId;

            alert(`OK！NotificationId=${id}`);
            if (statusEl) statusEl.textContent = '完成';
            return { succeeded: true, notificationId: id };
        } catch (err) {
            alert('例外：' + err.message);
            if (statusEl) statusEl.textContent = '';
            return { succeeded: false, reason: err.message };
        }
    }

    // 將 sendNotification 函式暴露到全域 window 物件，以便其他腳本可以直接呼叫。
    // 例如：window.sendNotification({ url: '/social_hub/notifications/ajax', payload: { toUserId: 1, title: 'Test' } });
    window.sendNotification = sendNotification;

    // 事件代理：監聽文件上的點擊事件。
    // 當點擊的目標或其祖先元素帶有 `data-notify` 屬性時，觸發通知發送。
    document.addEventListener('click', async (e) => {
        const btn = e.target.closest('[data-notify]');
        if (!btn) return; // 如果沒有找到帶有 data-notify 的元素，則不做任何事

        const url = btn.dataset.url; // 從按鈕的 data-url 屬性獲取 API URL
        const payload = buildPayloadFromDataset(btn.dataset); // 從按鈕的 dataset 建立 payload

        // 呼叫核心的 sendNotification 函式來發送通知
        await sendNotification({ url, payload });
    });
})();