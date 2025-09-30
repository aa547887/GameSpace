// MINIGAME AREA JavaScript
// 小遊戲區域的 JavaScript 功能

// 遊戲狀態管理
let gameState = {
    isPlaying: false,
    score: 0,
    level: 1,
    timeLeft: 60
};

// 初始化遊戲
function initGame() {
    console.log('初始化小遊戲...');
    gameState.isPlaying = false;
    gameState.score = 0;
    gameState.level = 1;
    gameState.timeLeft = 60;
    
    // 更新 UI
    updateScore();
    updateLevel();
    updateTimer();
}

// 開始遊戲
function startGame() {
    if (gameState.isPlaying) return;
    
    gameState.isPlaying = true;
    console.log('遊戲開始！');
    
    // 開始計時器
    startTimer();
    
    // 隱藏開始按鈕，顯示遊戲控制
    const startBtn = document.getElementById('startBtn');
    const gameControls = document.getElementById('gameControls');
    
    if (startBtn) startBtn.style.display = 'none';
    if (gameControls) gameControls.style.display = 'block';
}

// 停止遊戲
function stopGame() {
    gameState.isPlaying = false;
    console.log('遊戲結束！最終分數:', gameState.score);
    
    // 顯示結果
    showGameResult();
    
    // 重置 UI
    const startBtn = document.getElementById('startBtn');
    const gameControls = document.getElementById('gameControls');
    
    if (startBtn) startBtn.style.display = 'block';
    if (gameControls) gameControls.style.display = 'none';
}

// 計分
function addScore(points) {
    if (!gameState.isPlaying) return;
    
    gameState.score += points;
    updateScore();
    
    // 檢查是否升級
    checkLevelUp();
}

// 更新分數顯示
function updateScore() {
    const scoreElement = document.getElementById('score');
    if (scoreElement) {
        scoreElement.textContent = gameState.score;
    }
}

// 檢查升級
function checkLevelUp() {
    const newLevel = Math.floor(gameState.score / 100) + 1;
    if (newLevel > gameState.level) {
        gameState.level = newLevel;
        updateLevel();
        console.log('升級到第', gameState.level, '關！');
    }
}

// 更新等級顯示
function updateLevel() {
    const levelElement = document.getElementById('level');
    if (levelElement) {
        levelElement.textContent = gameState.level;
    }
}

// 計時器
let gameTimer = null;

function startTimer() {
    gameTimer = setInterval(() => {
        if (gameState.timeLeft > 0) {
            gameState.timeLeft--;
            updateTimer();
        } else {
            stopGame();
        }
    }, 1000);
}

function updateTimer() {
    const timerElement = document.getElementById('timer');
    if (timerElement) {
        timerElement.textContent = gameState.timeLeft;
    }
}

// 顯示遊戲結果
function showGameResult() {
    const resultElement = document.getElementById('gameResult');
    if (resultElement) {
        resultElement.innerHTML = `
            <h3>遊戲結束！</h3>
            <p>最終分數: ${gameState.score}</p>
            <p>達到等級: ${gameState.level}</p>
            <button onclick="initGame()" class="btn btn-primary">再玩一次</button>
        `;
        resultElement.style.display = 'block';
    }
}

// 遊戲控制按鈕事件
document.addEventListener('DOMContentLoaded', function() {
    // 開始按鈕
    const startBtn = document.getElementById('startBtn');
    if (startBtn) {
        startBtn.addEventListener('click', startGame);
    }
    
    // 停止按鈕
    const stopBtn = document.getElementById('stopBtn');
    if (stopBtn) {
        stopBtn.addEventListener('click', stopGame);
    }
    
    // 重置按鈕
    const resetBtn = document.getElementById('resetBtn');
    if (resetBtn) {
        resetBtn.addEventListener('click', initGame);
    }
    
    // 初始化遊戲
    initGame();
});

// 鍵盤事件處理
document.addEventListener('keydown', function(event) {
    if (!gameState.isPlaying) return;
    
    switch(event.key) {
        case ' ':
            // 空白鍵 - 增加分數
            addScore(10);
            break;
        case 'Escape':
            // ESC 鍵 - 停止遊戲
            stopGame();
            break;
    }
});

// 導出函數供其他模組使用
window.MiniGame = {
    initGame,
    startGame,
    stopGame,
    addScore,
    gameState
};
