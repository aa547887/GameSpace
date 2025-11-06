/**
 * Pet Runner Game - Chrome Dino Style
 * 寵物跑酷遊戲 - Chrome 恐龍風格
 * 特性：可愛風格、障礙物躲避、計分系統
 */

class PetRunnerGame {
    constructor(canvasId, options = {}) {
        this.canvas = document.getElementById(canvasId);
        if (!this.canvas) {
            console.error(`Canvas with id "${canvasId}" not found`);
            return;
        }

        this.ctx = this.canvas.getContext('2d');
        this.options = {
            petColor: options.petColor || '#ff6b6b',
            backgroundColor: options.backgroundColor || '#f0f4f8',
            onGameOver: options.onGameOver || null,
            onScoreUpdate: options.onScoreUpdate || null,
            difficulty: options.difficulty || 'normal', // easy, normal, hard
            ...options
        };

        // 遊戲狀態
        this.gameState = 'ready'; // ready, playing, paused, gameOver
        this.score = 0;
        this.highScore = localStorage.getItem('petRunnerHighScore') || 0;
        this.gameSpeed = 3;
        this.gravity = 0.6;

        // 寵物（玩家）
        this.pet = {
            x: 50,
            y: 0,
            width: 50,
            height: 50,
            velocityY: 0,
            jumping: false,
            grounded: false
        };

        // 地面
        this.groundY = this.canvas.height - 60;
        this.pet.y = this.groundY - this.pet.height;

        // 障礙物
        this.obstacles = [];
        this.obstacleFrequency = 120; // 幀數
        this.obstacleTimer = 0;

        // 雲朵（背景裝飾）
        this.clouds = [];
        this.initClouds();

        // 粒子特效
        this.particles = [];

        // 動畫幀
        this.animationFrame = null;

        // 鍵盤控制
        this.setupControls();

        // 調整 canvas 尺寸
        this.resizeCanvas();
        window.addEventListener('resize', () => this.resizeCanvas());

        // 初始渲染
        this.render();
    }

    /**
     * 調整 Canvas 尺寸
     */
    resizeCanvas() {
        const container = this.canvas.parentElement;
        this.canvas.width = container.clientWidth;
        this.canvas.height = 400;
        this.groundY = this.canvas.height - 60;
        if (!this.pet.jumping) {
            this.pet.y = this.groundY - this.pet.height;
        }
    }

    /**
     * 初始化雲朵
     */
    initClouds() {
        for (let i = 0; i < 5; i++) {
            this.clouds.push({
                x: Math.random() * this.canvas.width,
                y: Math.random() * 100 + 20,
                width: Math.random() * 60 + 40,
                speed: Math.random() * 0.5 + 0.3
            });
        }
    }

    /**
     * 設置鍵盤控制
     */
    setupControls() {
        document.addEventListener('keydown', (e) => {
            if (e.code === 'Space' || e.code === 'ArrowUp') {
                e.preventDefault();
                this.jump();
            }
            if (e.code === 'Enter' && this.gameState === 'ready') {
                this.start();
            }
            if (e.code === 'Enter' && this.gameState === 'gameOver') {
                this.restart();
            }
        });

        // 點擊/觸控跳躍
        this.canvas.addEventListener('click', () => {
            if (this.gameState === 'ready') {
                this.start();
            } else if (this.gameState === 'playing') {
                this.jump();
            } else if (this.gameState === 'gameOver') {
                this.restart();
            }
        });
    }

    /**
     * 開始遊戲
     */
    start() {
        if (this.gameState === 'playing') return;

        this.gameState = 'playing';
        this.score = 0;
        this.gameSpeed = this.getDifficultySpeed();
        this.obstacles = [];
        this.obstacleTimer = 0;
        this.pet.velocityY = 0;
        this.pet.jumping = false;
        this.pet.y = this.groundY - this.pet.height;

        this.gameLoop();
    }

    /**
     * 重新開始
     */
    restart() {
        this.start();
    }

    /**
     * 暫停
     */
    pause() {
        if (this.gameState === 'playing') {
            this.gameState = 'paused';
            cancelAnimationFrame(this.animationFrame);
        }
    }

    /**
     * 恢復
     */
    resume() {
        if (this.gameState === 'paused') {
            this.gameState = 'playing';
            this.gameLoop();
        }
    }

    /**
     * 跳躍
     */
    jump() {
        if (this.gameState !== 'playing') return;
        if (!this.pet.jumping) {
            this.pet.velocityY = -12;
            this.pet.jumping = true;
            this.createJumpParticles();
        }
    }

    /**
     * 遊戲主循環
     */
    gameLoop() {
        if (this.gameState !== 'playing') return;

        this.update();
        this.render();

        this.animationFrame = requestAnimationFrame(() => this.gameLoop());
    }

    /**
     * 更新遊戲狀態
     */
    update() {
        // 更新寵物物理
        this.updatePet();

        // 更新障礙物
        this.updateObstacles();

        // 更新雲朵
        this.updateClouds();

        // 更新粒子
        this.updateParticles();

        // 碰撞檢測
        this.checkCollisions();

        // 更新分數
        this.score += 1;
        if (this.options.onScoreUpdate) {
            this.options.onScoreUpdate(Math.floor(this.score / 10));
        }

        // 逐漸提升難度
        if (this.score % 500 === 0) {
            this.gameSpeed += 0.2;
        }
    }

    /**
     * 更新寵物狀態
     */
    updatePet() {
        // 重力
        this.pet.velocityY += this.gravity;
        this.pet.y += this.pet.velocityY;

        // 地面碰撞
        if (this.pet.y >= this.groundY - this.pet.height) {
            this.pet.y = this.groundY - this.pet.height;
            this.pet.velocityY = 0;
            this.pet.jumping = false;
            this.pet.grounded = true;
        } else {
            this.pet.grounded = false;
        }
    }

    /**
     * 更新障礙物
     */
    updateObstacles() {
        // 生成新障礙物
        this.obstacleTimer++;
        if (this.obstacleTimer > this.obstacleFrequency) {
            this.createObstacle();
            this.obstacleTimer = 0;
            // 隨機調整生成頻率
            this.obstacleFrequency = Math.random() * 60 + 80;
        }

        // 更新障礙物位置
        this.obstacles.forEach((obstacle, index) => {
            obstacle.x -= this.gameSpeed;

            // 移除離開螢幕的障礙物
            if (obstacle.x + obstacle.width < 0) {
                this.obstacles.splice(index, 1);
            }
        });
    }

    /**
     * 創建障礙物
     */
    createObstacle() {
        const types = ['monster1', 'monster2', 'monster3'];
        const type = types[Math.floor(Math.random() * types.length)];

        const obstacle = {
            x: this.canvas.width,
            y: this.groundY,
            width: 40,
            height: 50,
            type: type
        };

        this.obstacles.push(obstacle);
    }

    /**
     * 更新雲朵
     */
    updateClouds() {
        this.clouds.forEach(cloud => {
            cloud.x -= cloud.speed;
            if (cloud.x + cloud.width < 0) {
                cloud.x = this.canvas.width;
                cloud.y = Math.random() * 100 + 20;
            }
        });
    }

    /**
     * 更新粒子
     */
    updateParticles() {
        this.particles.forEach((particle, index) => {
            particle.x += particle.vx;
            particle.y += particle.vy;
            particle.vy += 0.2; // 重力
            particle.life--;

            if (particle.life <= 0) {
                this.particles.splice(index, 1);
            }
        });
    }

    /**
     * 碰撞檢測
     */
    checkCollisions() {
        this.obstacles.forEach(obstacle => {
            if (this.isColliding(this.pet, obstacle)) {
                this.gameOver();
            }
        });
    }

    /**
     * AABB 碰撞檢測
     */
    isColliding(rect1, rect2) {
        // 稍微縮小碰撞箱，讓遊戲更寬容
        const margin = 8;
        return rect1.x + margin < rect2.x + rect2.width &&
               rect1.x + rect1.width - margin > rect2.x &&
               rect1.y + margin < rect2.y + rect2.height &&
               rect1.y + rect1.height - margin > rect2.y;
    }

    /**
     * 遊戲結束
     */
    gameOver() {
        this.gameState = 'gameOver';
        cancelAnimationFrame(this.animationFrame);

        // 更新最高分
        const finalScore = Math.floor(this.score / 10);
        if (finalScore > this.highScore) {
            this.highScore = finalScore;
            localStorage.setItem('petRunnerHighScore', this.highScore);
        }

        // 觸發回調
        if (this.options.onGameOver) {
            this.options.onGameOver(finalScore);
        }

        this.createGameOverParticles();
    }

    /**
     * 渲染遊戲畫面
     */
    render() {
        const ctx = this.ctx;
        const canvas = this.canvas;

        // 清空畫布
        ctx.fillStyle = this.options.backgroundColor;
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        // 繪製雲朵
        this.renderClouds();

        // 繪製地面
        this.renderGround();

        // 繪製寵物
        this.renderPet();

        // 繪製障礙物
        this.renderObstacles();

        // 繪製粒子
        this.renderParticles();

        // 繪製 UI
        this.renderUI();
    }

    /**
     * 繪製雲朵
     */
    renderClouds() {
        const ctx = this.ctx;
        ctx.fillStyle = 'rgba(255, 255, 255, 0.5)';
        this.clouds.forEach(cloud => {
            ctx.beginPath();
            ctx.arc(cloud.x, cloud.y, cloud.width / 3, 0, Math.PI * 2);
            ctx.arc(cloud.x + cloud.width / 3, cloud.y - 10, cloud.width / 4, 0, Math.PI * 2);
            ctx.arc(cloud.x + cloud.width / 2, cloud.y, cloud.width / 3.5, 0, Math.PI * 2);
            ctx.fill();
        });
    }

    /**
     * 繪製地面
     */
    renderGround() {
        const ctx = this.ctx;
        const canvas = this.canvas;

        // 地面
        ctx.fillStyle = '#95d5b2';
        ctx.fillRect(0, this.groundY, canvas.width, canvas.height - this.groundY);

        // 地面裝飾線
        ctx.strokeStyle = '#74c69d';
        ctx.lineWidth = 3;
        ctx.beginPath();
        ctx.moveTo(0, this.groundY);
        ctx.lineTo(canvas.width, this.groundY);
        ctx.stroke();

        // 草地紋理
        ctx.fillStyle = '#52b788';
        for (let i = 0; i < canvas.width; i += 30) {
            const offset = (this.score * this.gameSpeed) % 30;
            ctx.fillRect(i - offset, this.groundY + 5, 10, 3);
        }
    }

    /**
     * 繪製寵物
     */
    renderPet() {
        const ctx = this.ctx;
        const pet = this.pet;
        const color = this.options.petColor;

        // 身體
        ctx.fillStyle = color;
        ctx.beginPath();
        ctx.arc(pet.x + 25, pet.y + 30, 20, 0, Math.PI * 2);
        ctx.fill();

        // 頭
        ctx.beginPath();
        ctx.arc(pet.x + 25, pet.y + 15, 18, 0, Math.PI * 2);
        ctx.fill();

        // 耳朵
        ctx.beginPath();
        ctx.moveTo(pet.x + 15, pet.y + 5);
        ctx.lineTo(pet.x + 18, pet.y + 15);
        ctx.lineTo(pet.x + 22, pet.y + 10);
        ctx.fill();

        ctx.beginPath();
        ctx.moveTo(pet.x + 35, pet.y + 5);
        ctx.lineTo(pet.x + 32, pet.y + 15);
        ctx.lineTo(pet.x + 28, pet.y + 10);
        ctx.fill();

        // 眼睛
        ctx.fillStyle = '#2c3e50';
        ctx.beginPath();
        ctx.arc(pet.x + 20, pet.y + 12, 3, 0, Math.PI * 2);
        ctx.fill();
        ctx.beginPath();
        ctx.arc(pet.x + 30, pet.y + 12, 3, 0, Math.PI * 2);
        ctx.fill();

        // 眼睛高光
        ctx.fillStyle = 'white';
        ctx.beginPath();
        ctx.arc(pet.x + 21, pet.y + 11, 1.5, 0, Math.PI * 2);
        ctx.fill();
        ctx.beginPath();
        ctx.arc(pet.x + 31, pet.y + 11, 1.5, 0, Math.PI * 2);
        ctx.fill();

        // 鼻子
        ctx.fillStyle = '#ff6b81';
        ctx.beginPath();
        ctx.arc(pet.x + 25, pet.y + 18, 2, 0, Math.PI * 2);
        ctx.fill();

        // 嘴巴
        ctx.strokeStyle = '#2c3e50';
        ctx.lineWidth = 1.5;
        ctx.beginPath();
        ctx.arc(pet.x + 25, pet.y + 20, 4, 0, Math.PI, false);
        ctx.stroke();

        // 腳（跑步動畫）
        if (pet.grounded) {
            const legOffset = Math.sin(this.score / 5) * 3;
            ctx.fillStyle = color;
            ctx.fillRect(pet.x + 15, pet.y + 45, 8, 5 + legOffset);
            ctx.fillRect(pet.x + 27, pet.y + 45, 8, 5 - legOffset);
        }

        // 尾巴
        ctx.strokeStyle = color;
        ctx.lineWidth = 5;
        ctx.lineCap = 'round';
        ctx.beginPath();
        const tailWag = Math.sin(this.score / 10) * 10;
        ctx.moveTo(pet.x + 35, pet.y + 30);
        ctx.quadraticCurveTo(pet.x + 45, pet.y + 25, pet.x + 50 + tailWag, pet.y + 20);
        ctx.stroke();
    }

    /**
     * 繪製障礙物（怪物）
     */
    renderObstacles() {
        const ctx = this.ctx;

        this.obstacles.forEach(obstacle => {
            this.renderMonster(obstacle);
        });
    }

    /**
     * 繪製怪物
     */
    renderMonster(obstacle) {
        const ctx = this.ctx;
        const x = obstacle.x;
        const y = obstacle.y - obstacle.height;

        // 根據類型選擇顏色
        const colors = {
            monster1: '#9b59b6',
            monster2: '#e74c3c',
            monster3: '#e67e22'
        };
        const color = colors[obstacle.type] || '#9b59b6';

        // 身體
        ctx.fillStyle = color;
        ctx.beginPath();
        ctx.arc(x + 20, y + 30, 18, 0, Math.PI * 2);
        ctx.fill();

        // 眼睛
        ctx.fillStyle = '#fff';
        ctx.beginPath();
        ctx.arc(x + 14, y + 25, 5, 0, Math.PI * 2);
        ctx.arc(x + 26, y + 25, 5, 0, Math.PI * 2);
        ctx.fill();

        ctx.fillStyle = '#2c3e50';
        ctx.beginPath();
        ctx.arc(x + 15, y + 26, 3, 0, Math.PI * 2);
        ctx.arc(x + 25, y + 26, 3, 0, Math.PI * 2);
        ctx.fill();

        // 嘴巴（兇惡）
        ctx.strokeStyle = '#2c3e50';
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.moveTo(x + 12, y + 35);
        ctx.lineTo(x + 28, y + 35);
        ctx.stroke();

        // 牙齒
        ctx.fillStyle = '#fff';
        ctx.fillRect(x + 14, y + 35, 3, 5);
        ctx.fillRect(x + 23, y + 35, 3, 5);

        // 觸角/角
        ctx.strokeStyle = color;
        ctx.lineWidth = 3;
        ctx.beginPath();
        ctx.moveTo(x + 10, y + 15);
        ctx.lineTo(x + 8, y + 5);
        ctx.moveTo(x + 30, y + 15);
        ctx.lineTo(x + 32, y + 5);
        ctx.stroke();

        ctx.fillStyle = color;
        ctx.beginPath();
        ctx.arc(x + 8, y + 5, 3, 0, Math.PI * 2);
        ctx.arc(x + 32, y + 5, 3, 0, Math.PI * 2);
        ctx.fill();
    }

    /**
     * 繪製粒子
     */
    renderParticles() {
        const ctx = this.ctx;

        this.particles.forEach(particle => {
            ctx.fillStyle = particle.color;
            ctx.globalAlpha = particle.life / particle.maxLife;
            ctx.beginPath();
            ctx.arc(particle.x, particle.y, particle.size, 0, Math.PI * 2);
            ctx.fill();
        });

        ctx.globalAlpha = 1;
    }

    /**
     * 繪製 UI
     */
    renderUI() {
        const ctx = this.ctx;
        const canvas = this.canvas;

        // 分數
        ctx.fillStyle = '#2c3e50';
        ctx.font = 'bold 24px Arial';
        ctx.textAlign = 'right';
        ctx.fillText(`分數: ${Math.floor(this.score / 10)}`, canvas.width - 20, 40);

        // 最高分
        ctx.font = '16px Arial';
        ctx.fillText(`最高: ${this.highScore}`, canvas.width - 20, 65);

        // 遊戲狀態提示
        if (this.gameState === 'ready') {
            this.renderCenterText('按 SPACE 或點擊開始', 30);
            this.renderCenterText('躲避怪物！', 60, '20px');
        } else if (this.gameState === 'gameOver') {
            this.renderCenterText('遊戲結束！', 30);
            this.renderCenterText(`得分: ${Math.floor(this.score / 10)}`, 60, '24px');
            this.renderCenterText('按 ENTER 或點擊重新開始', 95, '18px');
        } else if (this.gameState === 'paused') {
            this.renderCenterText('暫停', 30);
        }
    }

    /**
     * 渲染居中文字
     */
    renderCenterText(text, yOffset = 0, fontSize = '30px') {
        const ctx = this.ctx;
        const canvas = this.canvas;

        ctx.save();
        ctx.fillStyle = 'rgba(255, 255, 255, 0.9)';
        ctx.font = `bold ${fontSize} Arial`;
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';

        // 文字陰影
        ctx.shadowColor = 'rgba(0, 0, 0, 0.3)';
        ctx.shadowBlur = 10;

        ctx.fillText(text, canvas.width / 2, canvas.height / 2 + yOffset);
        ctx.restore();
    }

    /**
     * 創建跳躍粒子
     */
    createJumpParticles() {
        for (let i = 0; i < 5; i++) {
            this.particles.push({
                x: this.pet.x + 25,
                y: this.pet.y + this.pet.height,
                vx: (Math.random() - 0.5) * 3,
                vy: Math.random() * -2,
                size: Math.random() * 3 + 2,
                color: '#95d5b2',
                life: 30,
                maxLife: 30
            });
        }
    }

    /**
     * 創建遊戲結束粒子
     */
    createGameOverParticles() {
        for (let i = 0; i < 20; i++) {
            this.particles.push({
                x: this.pet.x + 25,
                y: this.pet.y + 25,
                vx: (Math.random() - 0.5) * 8,
                vy: (Math.random() - 0.5) * 8,
                size: Math.random() * 5 + 3,
                color: this.options.petColor,
                life: 60,
                maxLife: 60
            });
        }
    }

    /**
     * 根據難度獲取初始速度
     */
    getDifficultySpeed() {
        const speeds = {
            easy: 2.5,
            normal: 3.5,
            hard: 5
        };
        return speeds[this.options.difficulty] || 3.5;
    }

    /**
     * 獲取當前分數
     */
    getScore() {
        return Math.floor(this.score / 10);
    }

    /**
     * 銷毀遊戲
     */
    destroy() {
        cancelAnimationFrame(this.animationFrame);
        window.removeEventListener('resize', this.resizeCanvas);
    }
}

// 全局暴露
window.PetRunnerGame = PetRunnerGame;
