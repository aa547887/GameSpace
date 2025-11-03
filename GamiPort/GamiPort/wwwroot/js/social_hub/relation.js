/* ===================================================================
   relation.js - 前端關係操作輔助腳本
   此腳本提供前端處理用戶關係操作 (如加好友、接受/拒絕好友請求、封鎖/解除封鎖等) 的功能。
   它暴露了一個全域函數 `window.sendRelationCommand`，供其他腳本呼叫以執行關係相關的 AJAX 請求。

   主要功能：
   1. 發送關係命令：構建並發送 AJAX POST 請求到後端關係 API。
   2. 參數驗證：在發送請求前，驗證必要的參數 (actorUserId, targetUserId, actionCode) 是否存在。
   3. 安全性：自動包含 Anti-Forgery Token (如果存在)，以防止 CSRF 攻擊。
   4. 狀態更新：在頁面上顯示請求的狀態 (例如「送出中...」)。
   5. 回應處理：處理後端 API 的回應，包括成功、錯誤和非 JSON 格式的回應。
   6. 全域暴露：將 `sendRelationCommand` 函數暴露到 `window` 物件，供其他腳本 (如 `Index.cshtml` 中的內聯腳本) 呼叫。
=================================================================== */
(function () {
    /**
     * sendRelationCommand 函數
     * 說明：此為核心函數，用於向後端發送關係操作命令。
     *      - 接收一個包含 URL 和命令物件的 options 參數。
     *      - 驗證命令物件中是否包含 actorUserId (執行操作的用戶ID)、targetUserId (目標用戶ID) 和 actionCode (操作代碼)。
     *      - 顯示操作的狀態訊息。
     *      - 讀取並包含 Anti-Forgery Token (如果存在)。
     *      - 發送 JSON 格式的 POST 請求到指定的 URL。
     *      - 處理伺服器回應，包括 HTTP 錯誤、非 JSON 回應和 JSON 解析錯誤。
     *      - 返回一個 Promise，包含操作的成功狀態和相關資料 (如新的關係狀態碼)。
     * @param {{url: string, command: {actorUserId:number, targetUserId:number, actionCode:string, nickname?:string}}} options - 關係命令的選項物件。
     *      - `url`: 關係 API 的 URL 端點。
     *      - `command`: 包含關係操作詳細資訊的物件。
     *          - `actorUserId`: 執行關係操作的用戶 ID。
     *          - `targetUserId`: 關係操作的目標用戶 ID。
     *          - `actionCode`: 關係操作的代碼 (例如 'friend_request', 'accept', 'block')。
     *          - `nickname`?: 可選，用戶暱稱。
     * @returns {Promise<object>} 包含發送結果的 Promise。
     *      - 成功時範例：`{ succeeded: true, noOp: false, newStatusCode: 'ACCEPTED', relationId: 123 }`
     *      - 失敗時範例：`{ succeeded:false, reason:'錯誤訊息' }`
     */
    async function sendRelationCommand(options) {
        const { url, command } = options || {};
        // 驗證必要的命令參數
        if (!command || !command.actorUserId || !command.targetUserId || !command.actionCode) {
            alert('actorUserId / targetUserId / actionCode 為必填'); // 錯誤提示：缺少必要參數
            return { succeeded: false, reason: 'MISSING_PARAMS' };
        }

        // 獲取並更新頁面上的關係狀態顯示元素
        const statusEl = document.getElementById('rel-status');
        if (statusEl) statusEl.textContent = '送出中...'; // 顯示「送出中...」狀態

        // 讀取 Anti-Forgery Token (用於防止 CSRF 攻擊)
        const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenEl ? tokenEl.value : null;

        // 組裝請求 Headers，指定內容類型為 JSON，並期望接收 JSON 回應
        const headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };
        // 如果存在 Anti-Forgery Token，則將其添加到請求頭中
        if (token) headers['RequestVerificationToken'] = token;

        try {
            // 發送 AJAX POST 請求
            const res = await fetch(url, {
                method: 'POST',
                headers,
                credentials: 'same-origin', // 帶上同站 Cookie，用於身份驗證
                body: JSON.stringify(command) // 將命令物件轉換為 JSON 字串作為請求體
            });

            // 先以純文字形式獲取回應內容 (方便在錯誤時印出 HTML 片段進行排錯)
            const ct = res.headers.get('content-type') || '';
            const txt = await res.text();

            // 檢查 HTTP 回應狀態碼是否為成功 (2xx)
            if (!res.ok) {
                alert(`失敗 (HTTP ${res.status})：${txt.slice(0, 200)}`); // 錯誤提示：HTTP 請求失敗
                if (statusEl) statusEl.textContent = ''; // 清除狀態訊息
                return { succeeded: false, reason: `HTTP ${res.status}: ${txt.slice(0, 200)}` };
            }
            // 檢查伺服器回應的 Content-Type 是否為 JSON
            if (!ct.includes('application/json')) {
                alert('伺服器未回傳 JSON（可能被轉向或出現 HTML）'); // 錯誤提示：伺服器回應非 JSON
                if (statusEl) statusEl.textContent = ''; // 清除狀態訊息
                return { succeeded: false, reason: 'NON_JSON' };
            }

            let data;
            try {
                data = JSON.parse(txt);
            } catch {
                // JSON 解析失敗
                if (statusEl) statusEl.textContent = ''; // 清除狀態訊息
                return { succeeded: false, reason: 'PARSE_ERROR' };
            }

            // 構建關係資訊字串，用於顯示在狀態區域
            const info = [
                `NoOp=${data.noOp}`,
                `relationId=${data.relationId ?? ''}`,
                `status=${data.newStatusCode ?? ''}`,
                data.reason ? `reason=${data.reason}` : null
            ].filter(Boolean).join(' | ');

            // alert('OK：' + info); // 成功提示：顯示關係操作結果 (此行已被註釋)
            if (statusEl) statusEl.textContent = info; // 在狀態區域顯示關係資訊
            return { succeeded: true, ...data }; // 返回成功狀態和後端返回的資料
        } catch (err) {
            // 捕獲網路請求或 JSON 解析過程中發生的例外
            alert('例外：' + err.message); // 錯誤提示：發生例外
            if (statusEl) statusEl.textContent = ''; // 清除狀態訊息
            return { succeeded: false, reason: err.message };
        }
    }

    // 將 sendRelationCommand 函式暴露到全域 window 物件，以便其他腳本可以直接呼叫。
    // 範例：`window.sendRelationCommand({ url: '/social_hub/relations/exec', command: { actorUserId: 1, targetUserId: 2, actionCode: 'friend_request' } });`
    window.sendRelationCommand = sendRelationCommand;
})();
