const chatContainer = document.getElementById("chatContainer");
const chatHeader = document.getElementById("chatHeader");
const chatArea = document.getElementById("chatArea");
const messagesDiv = document.getElementById("messages");
const loginArea = document.getElementById("loginArea");
const userDisplay = document.getElementById("userDisplay");

let userName = "";
const originalSize = { width: "400px", height: "500px", right: "20px", bottom: "20px" };

// SignalR
const connection = new signalR.HubConnectionBuilder().withUrl("/social_hub/chatHub").build();
connection.on("ReceiveMessage", (user, message, time) => {
    const cssClass = user === userName ? "mine" : "other";
    const msgHtml = `<div class="message ${cssClass}"><div class="content">${message}</div><div class="time">${time}</div></div>`;
    messagesDiv.innerHTML += msgHtml;
    messagesDiv.scrollTop = messagesDiv.scrollHeight;
});
connection.start().catch(err => console.error(err));

// 登入
document.getElementById("loginBtn").addEventListener("click", () => {
    const name = document.getElementById("userNameInput").value.trim();
    if (!name) return;

    userName = name;
    userDisplay.textContent = `(${userName})`; // 顯示名稱
    connection.invoke("RegisterUser", userName).catch(err => console.error(err));

    loginArea.style.display = "none";
    chatArea.style.display = "flex";
});

// 發送訊息
document.getElementById("chatForm").addEventListener("submit", e => {
    e.preventDefault();
    const msg = document.getElementById("messageInput").value.trim();
    if (!msg) return;
    connection.invoke("SendMessage", msg).catch(err => console.error(err));
    document.getElementById("messageInput").value = "";
});

// 拖曳
let isDragging = false, offsetX, offsetY;
chatHeader.addEventListener("mousedown", e => {
    isDragging = true;
    offsetX = e.clientX - chatContainer.getBoundingClientRect().left;
    offsetY = e.clientY - chatContainer.getBoundingClientRect().top;
    document.body.style.userSelect = "none";
});
document.addEventListener("mousemove", e => {
    if (!isDragging) return;
    chatContainer.style.left = (e.clientX - offsetX) + "px";
    chatContainer.style.top = (e.clientY - offsetY) + "px";
    chatContainer.style.right = "auto";
    chatContainer.style.bottom = "auto";
});
document.addEventListener("mouseup", () => {
    isDragging = false;
    document.body.style.userSelect = "auto";
});

// 控制列
document.querySelector(".minimize").addEventListener("click", () => {
    chatContainer.style.height = "40px";
    chatArea.style.display = "none";
});
document.querySelector(".maximize").addEventListener("click", () => {
    chatContainer.style.width = "100%";
    chatContainer.style.height = "100%";
    chatContainer.style.right = "0";
    chatContainer.style.bottom = "0";
    chatContainer.style.left = "0";
    chatContainer.style.top = "0";
    chatContainer.style.borderRadius = "0";
    chatArea.style.display = "flex";
});
document.querySelector(".close").addEventListener("click", () => {
    chatContainer.style.display = "none";
});
chatHeader.addEventListener("dblclick", () => {
    chatContainer.style.width = originalSize.width;
    chatContainer.style.height = originalSize.height;
    chatContainer.style.right = originalSize.right;
    chatContainer.style.bottom = originalSize.bottom;
    chatContainer.style.left = "auto";
    chatContainer.style.top = "auto";
    chatContainer.style.borderRadius = "12px";
    chatArea.style.display = "flex";
});

// 右下角拖拉
const resizeHandle = document.querySelector(".resize-handle");
let isResizing = false, startX, startY, startWidth, startHeight;

resizeHandle.addEventListener("mousedown", e => {
    isResizing = true;
    startX = e.clientX;
    startY = e.clientY;
    const rect = chatContainer.getBoundingClientRect();
    startWidth = rect.width;
    startHeight = rect.height;
    document.body.style.userSelect = "none";
});

document.addEventListener("mousemove", e => {
    if (!isResizing) return;
    const newWidth = startWidth + (e.clientX - startX);
    const newHeight = startHeight + (e.clientY - startY);
    chatContainer.style.width = Math.max(300, newWidth) + "px";
    chatContainer.style.height = Math.max(200, newHeight) + "px";
});

document.addEventListener("mouseup", () => {
    if (isResizing) {
        isResizing = false;
        document.body.style.userSelect = "auto";
    }
});
