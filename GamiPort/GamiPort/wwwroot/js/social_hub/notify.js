/* ===================================================================
   notify.js - 前端通知發送腳本
   此腳本負責處理前端的通知發送邏輯，包括監聽點擊事件、組裝通知資料、
   發送 AJAX 請求到後端通知 API，並處理後端回應。

   主要功能：
   1. 監聽全域點擊事件：當點擊帶有 `data-notify` 屬性的元素時觸發通知發送流程。
   2. 資料組裝：從觸發元素的 `dataset` 屬性中讀取資料，組合成通知的 payload。
   3. AJAX 請求：使用 `fetch` API 發送 POST 請求到指定的通知 API 端點。
   4. 安全性：自動包含 Anti-Forgery Token（如果存在），以防止 CSRF 攻擊。
   5. 回應處理：根據後端回應的狀態和內容，顯示成功或錯誤訊息。
   6. 全域暴露：將 `sendNotification` 函數暴露到 `window` 物件，供其他腳本直接呼叫。
=================================================================== */
(function () {
    // 簡化 document.querySelector 的使用
    const $ = (sel) => document.querySelector(sel);

    /**
     * toMaybeNumber 函數
     * 說明：將輸入值轉換為數字或 null。主要用於處理從 HTML dataset 讀取到的字串值。
     *      - 如果值為 null 或空字串，則返回 null。
     *      - 如果值是有限數字的字串形式，則轉換為 Number 類型。
     *      - 否則，返回原始值。
     * @param {string|number|null} v - 待轉換的值
     * @returns {string|number|null} 轉換後的值
     */
    function toMaybeNumber(v) {
        if (v == null || v === "") return null;
        return isFinite(v) ? Number(v) : v;
    }

    /**
     * buildPayloadFromDataset 函數
     * 說明：從 HTML 元素的 `dataset` 屬性中提取資料，並構建一個用於發送通知的 payload 物件。
     *      - `data-aaa-bbb` 形式的屬性會被轉換為 `payload.aaaBbb` (小駝峰命名)。
     *      - 會自動將可轉換為數字的字串轉換為數字類型。
     *      - 移除 payload 中值為 null 或空字串的鍵，以避免將空值發送到後端。
     * @param {DOMStringMap} ds - HTML 元素的 dataset 物件 (例如：`element.dataset`)
     * @returns {object} 構建好的 payload 物件，包含通知所需的各項資料。
     */
    function buildPayloadFromDataset(ds) {
        const p = {
            sourceId: toMaybeNumber(ds.sourceId),       // 通知來源的 ID
            actionId: toMaybeNumber(ds.actionId),       // 通知動作的 ID
            toUserId: toMaybeNumber(ds.toUserId),       // 目標用戶的 ID
            toManagerId: toMaybeNumber(ds.toManagerId), // 目標管理員的 ID
            groupId: toMaybeNumber(ds.groupId),         // 相關群組的 ID
            senderUserId: toMaybeNumber(ds.senderUserId), // 發送通知的用戶 ID
            senderManagerId: toMaybeNumber(ds.senderManagerId), // 發送通知的管理員 ID
            title: ds.title ?? "",                      // 通知標題
            message: ds.message ?? null                 // 通知內容訊息
        };
        // 移除 payload 中值為 null 或空字串的鍵，避免把空值送到後端
        Object.keys(p).forEach(k => { if (p[k] === null || p[k] === "") delete p[k]; });
        return p;
    }

    /**
     * sendNotification 函數 (核心發送通知函式)
     * 說明：負責組裝 AJAX 請求、發送 POST 到後端通知 API，並處理伺服器回應。
     *      - 執行最小前端驗證，確保至少有一個收件人 (toUserId 或 toManagerId)。
     *      - 顯示發送中的狀態訊息。
     *      - 自動讀取並包含 Anti-Forgery Token (如果存在於頁面中)。
     *      - 發送 JSON 格式的 POST 請求。
     *      - 處理不同 HTTP 狀態碼的回應 (例如 2xx 成功，非 2xx 錯誤)。
     *      - 解析 JSON 回應，並返回發送結果。
     * @param {object} options - 包含通知發送所需參數的物件
     * @param {string} options.url - 通知 API 的 URL 端點
     * @param {object} options.payload - 通知內容的 payload 物件
     * @returns {Promise<object>} 包含發送結果的 Promise。
     *      - 成功時：`{ succeeded: true, notificationId: 123 }`
     *      - 失敗時：`{ succeeded: false, reason: "錯誤訊息" }`
     */
    async function sendNotification(options) {
        const { url, payload } = options;

        // 最小前端驗證：至少要有一個收件人 (toUserId 或 toManagerId)
        if (!('toUserId' in payload) && !('toManagerId' in payload)) {
            // alert('請在參數中至少設定一個收件人（toUserId 或 toManagerId）。'); // 錯誤提示：缺少收件人
            return { succeeded: false, reason: '缺少收件人' };
        }

        // 顯示狀態訊息 (例如：在頁面上的某個元素中顯示「送出中…」)
        const statusEl = $('#status');
        if (statusEl) statusEl.textContent = '送出中…';

        // 讀取 Anti-Forgery Token (若控制器有 [ValidateAntiForgeryToken] 屬性，則需要此 token)
        const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenEl ? tokenEl.value : null;

        // 組裝請求 Headers，指定內容類型為 JSON，並期望接收 JSON 回應
        const headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };
        // 若後端有啟用 Anti-Forgery，就把 token 放在自訂 Header 中
        if (token) headers['RequestVerificationToken'] = token;

        try {
            // 發送 AJAX POST 請求
            const res = await fetch(url, {
                method: 'POST',
                headers,
                credentials: 'same-origin', // 帶上同站 Cookie，用於身份驗證
                body: JSON.stringify(payload) // 將 payload 物件轉換為 JSON 字串作為請求體
            });

            // 先以純文字形式獲取回應內容 (方便在錯誤時印出 HTML 片段進行排錯)
            const contentType = res.headers.get('content-type') || '';
            const text = await res.text();

            // 檢查 HTTP 回應狀態碼是否為成功 (2xx)
            if (!res.ok) {
                // 例如 400/404/405/500 等錯誤，直接把回應的前 200 字顯示方便排錯
                alert(`失敗 (HTTP ${res.status})：${text.slice(0, 200)}`); // 錯誤提示：HTTP 請求失敗
                if (statusEl) statusEl.textContent = ''; // 清除狀態訊息
                return { succeeded: false, reason: `HTTP ${res.status}: ${text.slice(0, 200)}` };
            }

            // 檢查伺服器回應的 Content-Type 是否為 JSON
            if (!contentType.includes('application/json')) {
                // 如果不是 JSON，多半是被 302 導向到登入頁或打到 View (HTML 頁面)
                alert('伺服器回應不是 JSON（可能被轉址到登入頁或打錯路由）。'); // 錯誤提示：伺服器回應非 JSON
                if (statusEl) statusEl.textContent = ''; // 清除狀態訊息
                return { succeeded: false, reason: '伺服器回應非 JSON' };
            }

            // 解析 JSON 回應；控制器通常會回傳小駝峰命名的屬性，例如：{ "notificationId": 123 }
            const data = JSON.parse(text);
            const id = data.notificationId;

            // alert(`OK！NotificationId=${id}`); // 成功提示：通知已發送，並顯示 ID (此行已被註釋)
            if (statusEl) statusEl.textContent = '完成'; // 顯示完成狀態
            return { succeeded: true, notificationId: id };
        } catch (err) {
            // 捕獲網路請求或 JSON 解析過程中發生的例外
            alert('例外：' + err.message); // 錯誤提示：發生例外
            if (statusEl) statusEl.textContent = ''; // 清除狀態訊息
            return { succeeded: false, reason: err.message };
        }
    }

    // 將 sendNotification 函式暴露到全域 window 物件，以便其他腳本可以直接呼叫。
    // 範例：`window.sendNotification({ url: '/social_hub/notifications/ajax', payload: { toUserId: 1, title: 'Test' } });`
    window.sendNotification = sendNotification;

    // 事件代理：監聽文件上的點擊事件。
    // 當點擊的目標元素或其任何祖先元素帶有 `data-notify` 屬性時，觸發通知發送流程。
    document.addEventListener('click', async (e) => {
        // 尋找最近的帶有 `data-notify` 屬性的祖先元素 (包括自身)
        const btn = e.target.closest('[data-notify]');
        if (!btn) return; // 如果沒有找到符合條件的元素，則不做任何事

        const url = btn.dataset.url; // 從按鈕的 `data-url` 屬性獲取 API URL
        const payload = buildPayloadFromDataset(btn.dataset); // 從按鈕的 `dataset` 建立通知 payload

        // 呼叫核心的 sendNotification 函式來發送通知
        await sendNotification({ url, payload });
    });
})();