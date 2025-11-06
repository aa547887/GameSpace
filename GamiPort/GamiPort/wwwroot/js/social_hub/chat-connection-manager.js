/* ===================================================================
   chat-connection-manager.js - SignalR ChatHub 連線管理器
   此腳本負責建立和管理一個單一的 SignalR HubConnection 實例到 /social_hub/chatHub。
   它確保在整個應用程式中，所有需要與 ChatHub 互動的組件都共享同一個連線，
   從而避免重複建立連線，優化資源使用。

   主要功能：
   1. 建立並維護一個單一的 HubConnection 實例到 /social_hub/chatHub。
   2. 提供一個 Promise 接口，讓其他組件可以等待連線準備就緒。
   3. 處理自動重連。
=================================================================== */
(function () {
    let chatHubConnection = null;
    let chatHubConnectionPromise = null;

    /**
     * 獲取或建立 SignalR ChatHub 的單一連線實例。
     * 如果連線尚未建立，則會建立一個新連線並啟動它。
     *
     * @returns {Promise<signalR.HubConnection>} SignalR HubConnection 實例的 Promise。
     */
    window.getChatHubConnection = function () {
        if (chatHubConnectionPromise) {
            return chatHubConnectionPromise;
        }

        chatHubConnectionPromise = (async () => {
            // signalR 庫現在由 _Layout.cshtml 確保載入，無需在此處懶載入
            const SR = window.signalR; // 直接使用全域的 signalR 物件

            if (!chatHubConnection) {
                chatHubConnection = new SR.HubConnectionBuilder()
                    .withUrl("/social_hub/chatHub")
                    .withAutomaticReconnect()
                    .build();

                // 處理連線關閉事件，以便在需要時重新建立連線
                chatHubConnection.onclose(error => {
                    console.warn('[ChatHubConnection] 連線已關閉:', error);
                    // 可以選擇在這裡實現更複雜的重連邏輯，如果 withAutomaticReconnect 不夠用
                });

                // 處理自動重連開始事件
                chatHubConnection.onreconnecting(error => {
                    console.warn('[ChatHubConnection] 正在重連:', error);
                });

                // 處理自動重連完成事件
                chatHubConnection.onreconnected(connectionId => {
                    console.info('[ChatHubConnection] 重連成功，Connection ID:', connectionId);
                });
            }

            if (chatHubConnection.state === SR.HubConnectionState.Disconnected) {
                try {
                    await chatHubConnection.start();
                    console.info('[ChatHubConnection] 連線已啟動。');
                } catch (e) {
                    console.error('[ChatHubConnection] 連線啟動失敗:', e);
                    chatHubConnection = null; // 啟動失敗則重置連線，下次再嘗試
                    throw e;
                }
            }
            return chatHubConnection;
        })();

        return chatHubConnectionPromise;
    };
})();