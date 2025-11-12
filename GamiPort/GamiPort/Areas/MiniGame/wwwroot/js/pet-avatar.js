/**
 * Pet Avatar SVG Renderer - Enhanced Version
 * 可愛的寵物 SVG 渲染器 - 增強版
 * 特性：眼睛追蹤、呼吸動畫、表情系統、升級特效
 */

class PetAvatar {
    constructor(containerId, options = {}) {
        this.container = document.getElementById(containerId);
        if (!this.container) {
            console.error(`Container with id "${containerId}" not found`);
            return;
        }

        this.options = {
            skinColor: options.skinColor || '#ff6b6b',
            backgroundColor: options.backgroundColor || '#ffffff',
            size: options.size || 200,
            animated: options.animated !== false,
            enableEyeTracking: options.enableEyeTracking !== false,
            enableBreathing: options.enableBreathing !== false,
            // 狀態值 (0-100)
            hunger: options.hunger || 100,
            mood: options.mood || 100,
            stamina: options.stamina || 100,
            cleanliness: options.cleanliness || 100,
            health: options.health || 100,
            level: options.level || 1,
            ...options
        };

        this.mouseX = 0;
        this.mouseY = 0;
        this.currentExpression = this.determineExpression();

        this.render();
        this.setupEventListeners();
        this.addAnimationStyles();
    }

    /**
     * 根據 5 個狀態值決定表情
     */
    determineExpression() {
        const { hunger, mood, stamina, cleanliness, health } = this.options;

        // 健康值最優先
        if (health < 20) return 'sick';

        // 任一屬性為 0 - 狀態不佳
        if (hunger === 0 || mood === 0 || stamina === 0 || cleanliness === 0 || health === 0) {
            return 'critical';
        }

        // 飢餓值低
        if (hunger < 30) return 'hungry';

        // 清潔度低
        if (cleanliness < 30) return 'dirty';

        // 體力低
        if (stamina < 30) return 'sleepy';

        // 心情低
        if (mood < 40) return 'sad';

        // 平均狀態計算
        const avgStat = (hunger + mood + stamina + cleanliness + health) / 5;

        if (avgStat >= 80) return 'happy';
        if (avgStat >= 60) return 'normal';
        if (avgStat >= 40) return 'tired';

        return 'sad';
    }

    /**
     * 渲染可愛的貓咪 SVG
     */
    render() {
        const { size, skinColor, backgroundColor } = this.options;
        const svgNamespace = 'http://www.w3.org/2000/svg';

        // 創建 SVG 容器
        const svg = document.createElementNS(svgNamespace, 'svg');
        svg.setAttribute('width', size);
        svg.setAttribute('height', size);
        svg.setAttribute('viewBox', '0 0 200 200');
        svg.style.overflow = 'visible';
        svg.id = 'petSvg';

        // 背景圓形
        const bgCircle = this.createCircle(100, 100, 95, backgroundColor);
        bgCircle.id = 'bgCircle';
        svg.appendChild(bgCircle);

        // 陰影
        const shadow = this.createEllipse(100, 185, 50, 10, 'rgba(0,0,0,0.1)');
        svg.appendChild(shadow);

        // 貓咪身體
        const body = this.createCircle(100, 120, 45, skinColor);
        body.id = 'petBody';
        if (this.options.enableBreathing) {
            body.style.transformOrigin = '100px 120px';
            body.style.animation = 'breathe 3s ease-in-out infinite';
        }
        svg.appendChild(body);

        // 貓咪頭
        const head = this.createCircle(100, 75, 42, skinColor);
        head.id = 'petHead';
        if (this.options.enableBreathing) {
            head.style.transformOrigin = '100px 75px';
            head.style.animation = 'breathe 3s ease-in-out infinite';
        }
        svg.appendChild(head);

        // 左耳
        const leftEar = this.createTriangle(60, 35, 70, 45, 80, 45, skinColor);
        leftEar.id = 'leftEar';
        if (this.options.animated) {
            leftEar.style.transformOrigin = '70px 45px';
            leftEar.style.animation = 'earWiggle 3s ease-in-out infinite';
        }
        svg.appendChild(leftEar);

        // 左耳內側（粉紅色）
        const leftEarInner = this.createTriangle(65, 40, 70, 45, 75, 45, '#ffb3ba');
        leftEarInner.style.transformOrigin = '70px 45px';
        if (this.options.animated) {
            leftEarInner.style.animation = 'earWiggle 3s ease-in-out infinite';
        }
        svg.appendChild(leftEarInner);

        // 右耳
        const rightEar = this.createTriangle(120, 45, 130, 45, 140, 35, skinColor);
        rightEar.id = 'rightEar';
        if (this.options.animated) {
            rightEar.style.transformOrigin = '130px 45px';
            rightEar.style.animation = 'earWiggle 3s ease-in-out infinite 0.3s';
        }
        svg.appendChild(rightEar);

        // 右耳內側
        const rightEarInner = this.createTriangle(125, 45, 130, 45, 135, 40, '#ffb3ba');
        rightEarInner.style.transformOrigin = '130px 45px';
        if (this.options.animated) {
            rightEarInner.style.animation = 'earWiggle 3s ease-in-out infinite 0.3s';
        }
        svg.appendChild(rightEarInner);

        // 臉部特徵容器（用於眼睛追蹤）
        const faceGroup = document.createElementNS(svgNamespace, 'g');
        faceGroup.id = 'faceGroup';
        this.addFaceFeatures(faceGroup);
        svg.appendChild(faceGroup);

        // 尾巴
        const tail = this.createTail(skinColor);
        svg.appendChild(tail);

        // 小手（左）
        const leftPaw = this.createCircle(65, 135, 15, skinColor);
        leftPaw.style.animation = 'pawWave 4s ease-in-out infinite';
        leftPaw.style.transformOrigin = '65px 135px';
        svg.appendChild(leftPaw);

        // 小手（右）
        const rightPaw = this.createCircle(135, 135, 15, skinColor);
        rightPaw.style.animation = 'pawWave 4s ease-in-out infinite 2s';
        rightPaw.style.transformOrigin = '135px 135px';
        svg.appendChild(rightPaw);

        // 腳掌細節
        this.addPawDetails(svg, 65, 135, skinColor);
        this.addPawDetails(svg, 135, 135, skinColor);

        // 清空容器並添加新 SVG
        this.container.innerHTML = '';
        this.container.appendChild(svg);

        // 存儲 SVG 引用
        this.svg = svg;
    }

    /**
     * 添加腳掌細節
     */
    addPawDetails(parent, cx, cy, color) {
        // 三個小肉墊
        for (let i = 0; i < 3; i++) {
            const angle = (i - 1) * 20 * Math.PI / 180;
            const x = cx + Math.sin(angle) * 8;
            const y = cy - 5 + Math.cos(angle) * 5;
            const pad = this.createCircle(x, y, 3, this.darkenColor(color, 20));
            parent.appendChild(pad);
        }
    }

    /**
     * 添加臉部特徵
     */
    addFaceFeatures(parent) {
        const expression = this.currentExpression;

        // 眼睛容器（用於追蹤）
        const eyeGroup = document.createElementNS('http://www.w3.org/2000/svg', 'g');
        eyeGroup.id = 'eyeGroup';

        // 左眼白
        const leftEyeWhite = this.createEllipse(85, 70, 10, 12, 'white');
        eyeGroup.appendChild(leftEyeWhite);

        // 右眼白
        const rightEyeWhite = this.createEllipse(115, 70, 10, 12, 'white');
        eyeGroup.appendChild(rightEyeWhite);

        // 左眼珠（可追蹤）
        const leftPupil = this.createCircle(85, 70, 5, '#2c3e50');
        leftPupil.id = 'leftPupil';
        eyeGroup.appendChild(leftPupil);

        // 右眼珠（可追蹤）
        const rightPupil = this.createCircle(115, 70, 5, '#2c3e50');
        rightPupil.id = 'rightPupil';
        eyeGroup.appendChild(rightPupil);

        // 眼睛高光
        if (expression === 'happy' || expression === 'normal') {
            const leftSparkle = this.createCircle(87, 68, 2, 'white');
            const rightSparkle = this.createCircle(117, 68, 2, 'white');
            eyeGroup.appendChild(leftSparkle);
            eyeGroup.appendChild(rightSparkle);
        }

        // 根據表情調整眼睛
        if (expression === 'sleepy') {
            // 瞇眼效果
            leftEyeWhite.setAttribute('ry', '4');
            rightEyeWhite.setAttribute('ry', '4');
        } else if (expression === 'sick' || expression === 'critical') {
            // X_X 眼睛
            const leftX = this.createPath('M 80 67 L 90 73 M 90 67 L 80 73', '#2c3e50', 3);
            const rightX = this.createPath('M 110 67 L 120 73 M 120 67 L 110 73', '#2c3e50', 3);
            eyeGroup.appendChild(leftX);
            eyeGroup.appendChild(rightX);
            leftPupil.style.display = 'none';
            rightPupil.style.display = 'none';
        }

        // 眨眼動畫
        if (this.options.animated && expression !== 'sick' && expression !== 'critical') {
            const blinkAnimation = document.createElementNS('http://www.w3.org/2000/svg', 'animate');
            blinkAnimation.setAttribute('attributeName', 'ry');
            blinkAnimation.setAttribute('values', '12;2;12');
            blinkAnimation.setAttribute('dur', '0.3s');
            blinkAnimation.setAttribute('repeatCount', 'indefinite');
            blinkAnimation.setAttribute('begin', '3s');
            leftEyeWhite.appendChild(blinkAnimation.cloneNode());
            rightEyeWhite.appendChild(blinkAnimation);
        }

        parent.appendChild(eyeGroup);

        // 鼻子
        const nose = this.createTriangle(98, 82, 100, 87, 102, 82, '#ff6b81');
        parent.appendChild(nose);

        // 嘴巴（根據表情）
        const mouth = this.createMouth(expression);
        parent.appendChild(mouth);

        // 鬍鬚
        this.addWhiskers(parent, expression);

        // 腮紅
        if (expression === 'happy' || expression === 'normal') {
            const leftBlush = this.createCircle(70, 85, 8, 'rgba(255, 107, 129, 0.4)');
            const rightBlush = this.createCircle(130, 85, 8, 'rgba(255, 107, 129, 0.4)');
            parent.appendChild(leftBlush);
            parent.appendChild(rightBlush);
        }

        // 特殊表情符號
        if (expression === 'hungry') {
            // 汗滴
            const sweat1 = this.createPath('M 125 60 Q 127 65 125 70', '#4ecdc4', 2);
            const sweat2 = this.createPath('M 130 55 Q 132 60 130 65', '#4ecdc4', 2);
            parent.appendChild(sweat1);
            parent.appendChild(sweat2);
        } else if (expression === 'dirty') {
            // 污漬
            const dirt1 = this.createCircle(125, 80, 4, 'rgba(139, 69, 19, 0.3)');
            const dirt2 = this.createCircle(130, 85, 3, 'rgba(139, 69, 19, 0.3)');
            parent.appendChild(dirt1);
            parent.appendChild(dirt2);
        }
    }

    /**
     * 創建嘴巴路徑
     */
    createMouth(expression) {
        let path;
        switch (expression) {
            case 'happy':
                path = 'M 85 92 Q 100 102 115 92';
                break;
            case 'sad':
            case 'critical':
                path = 'M 85 98 Q 100 92 115 98';
                break;
            case 'hungry':
                path = 'M 85 92 Q 100 100 115 92 M 95 96 Q 100 98 105 96';
                break;
            case 'sleepy':
                path = 'M 85 94 Q 100 96 115 94';
                break;
            case 'sick':
                path = 'M 85 95 L 90 95 M 95 93 L 100 93 M 105 95 L 110 95';
                break;
            default:
                path = 'M 85 94 Q 100 97 115 94';
        }
        return this.createPath(path, '#2c3e50', 2);
    }

    /**
     * 添加鬍鬚
     */
    addWhiskers(parent, expression) {
        const whiskerColor = '#2c3e50';
        const whiskerWidth = expression === 'sleepy' ? 1 : 1.5;

        const whiskers = [
            'M 55 73 L 75 71',
            'M 55 78 L 75 78',
            'M 55 83 L 75 85',
            'M 125 71 L 145 73',
            'M 125 78 L 145 78',
            'M 125 85 L 145 83'
        ];

        whiskers.forEach((d, index) => {
            const whisker = this.createPath(d, whiskerColor, whiskerWidth);
            if (this.options.animated) {
                whisker.style.animation = `whiskerTwitch 2s ease-in-out infinite ${index * 0.1}s`;
            }
            parent.appendChild(whisker);
        });
    }

    /**
     * 創建尾巴
     */
    createTail(color) {
        const tail = this.createPath(
            'M 140 120 Q 165 110 170 135 Q 173 155 160 165',
            color,
            14
        );
        tail.setAttribute('fill', 'none');
        tail.id = 'petTail';
        if (this.options.animated) {
            tail.style.transformOrigin = '140px 120px';
            tail.style.animation = 'tailWag 2.5s ease-in-out infinite';
        }
        return tail;
    }

    /**
     * 設置事件監聽器
     */
    setupEventListeners() {
        if (this.options.enableEyeTracking) {
            document.addEventListener('mousemove', (e) => this.onMouseMove(e));
        }
    }

    /**
     * 鼠標移動事件 - 眼睛追蹤
     */
    onMouseMove(e) {
        const rect = this.container.getBoundingClientRect();
        const centerX = rect.left + rect.width / 2;
        const centerY = rect.top + rect.height / 2;

        // 計算鼠標相對位置
        const deltaX = e.clientX - centerX;
        const deltaY = e.clientY - centerY;
        const angle = Math.atan2(deltaY, deltaX);
        const distance = Math.min(Math.sqrt(deltaX * deltaX + deltaY * deltaY) / 100, 1);

        // 眼珠移動範圍限制
        const maxMove = 3;
        const moveX = Math.cos(angle) * distance * maxMove;
        const moveY = Math.sin(angle) * distance * maxMove;

        // 更新眼珠位置
        const leftPupil = this.svg?.querySelector('#leftPupil');
        const rightPupil = this.svg?.querySelector('#rightPupil');

        if (leftPupil) {
            leftPupil.setAttribute('cx', 85 + moveX);
            leftPupil.setAttribute('cy', 70 + moveY);
        }
        if (rightPupil) {
            rightPupil.setAttribute('cx', 115 + moveX);
            rightPupil.setAttribute('cy', 70 + moveY);
        }
    }

    /**
     * 添加 CSS 動畫樣式
     */
    addAnimationStyles() {
        const styleId = 'pet-avatar-animations';
        if (document.getElementById(styleId)) return;

        const style = document.createElement('style');
        style.id = styleId;
        style.textContent = `
            @keyframes breathe {
                0%, 100% { transform: scale(1); }
                50% { transform: scale(1.03); }
            }

            @keyframes earWiggle {
                0%, 100% { transform: rotate(0deg); }
                50% { transform: rotate(-8deg); }
            }

            @keyframes tailWag {
                0%, 100% { transform: rotate(0deg); }
                25% { transform: rotate(12deg); }
                75% { transform: rotate(-12deg); }
            }

            @keyframes whiskerTwitch {
                0%, 100% { transform: translateX(0); }
                50% { transform: translateX(2px); }
            }

            @keyframes pawWave {
                0%, 100% { transform: translateY(0) rotate(0deg); }
                25% { transform: translateY(-5px) rotate(-10deg); }
                50% { transform: translateY(0) rotate(0deg); }
            }

            @keyframes heartFloat {
                0% { transform: translateY(0) scale(0); opacity: 1; }
                50% { opacity: 1; }
                100% { transform: translateY(-60px) scale(1.2); opacity: 0; }
            }

            @keyframes levelUpBadge {
                0% { transform: translateY(0) scale(0) rotate(-180deg); opacity: 0; }
                50% { transform: translateY(-30px) scale(1.3) rotate(0deg); opacity: 1; }
                100% { transform: translateY(-50px) scale(1) rotate(0deg); opacity: 1; }
            }

            @keyframes levelUpGlow {
                0%, 100% { filter: drop-shadow(0 0 5px #ffd700); }
                50% { filter: drop-shadow(0 0 20px #ffd700); }
            }

            @keyframes starBurst {
                0% { transform: scale(0) rotate(0deg); opacity: 1; }
                100% { transform: scale(2) rotate(180deg); opacity: 0; }
            }
        `;
        document.head.appendChild(style);
    }

    /**
     * 更新狀態值並重新渲染
     */
    updateStats(stats) {
        this.options = { ...this.options, ...stats };
        this.currentExpression = this.determineExpression();
        this.render();
    }

    /**
     * 更新膚色
     */
    updateSkinColor(newColor) {
        this.options.skinColor = newColor;
        this.render();
    }

    /**
     * 更新背景色
     */
    updateBackgroundColor(newColor) {
        this.options.backgroundColor = newColor;
        // 只更新背景，不重新渲染整個寵物
        const bgCircle = this.svg?.querySelector('#bgCircle');
        if (bgCircle) {
            bgCircle.setAttribute('fill', newColor);
        }
    }

    /**
     * 播放升級特效
     */
    playLevelUpEffect(newLevel, pointsGained) {
        // 1. 全身發光
        const petBody = this.svg?.querySelector('#petBody');
        const petHead = this.svg?.querySelector('#petHead');
        if (petBody && petHead) {
            petBody.style.animation = 'levelUpGlow 1.5s ease-in-out 3';
            petHead.style.animation = 'levelUpGlow 1.5s ease-in-out 3';
        }

        // 2. Level Up 標籤
        this.showLevelUpBadge(newLevel);

        // 3. 星星爆發特效
        this.showStarBurst();

        // 4. 點數獲得提示
        this.showPointsGained(pointsGained);

        // 5. 暫時切換到 excited 表情
        const originalExpression = this.currentExpression;
        setTimeout(() => {
            this.currentExpression = 'happy';
            this.render();
            setTimeout(() => {
                this.currentExpression = originalExpression;
                this.render();
            }, 3000);
        }, 100);
    }

    /**
     * 顯示 Level Up 標籤
     */
    showLevelUpBadge(level) {
        const badge = document.createElement('div');
        badge.innerHTML = `<div style="font-weight: bold; color: #ffd700;">LEVEL ${level}</div>`;
        badge.style.position = 'absolute';
        badge.style.left = '50%';
        badge.style.top = '-20px';
        badge.style.transform = 'translateX(-50%)';
        badge.style.fontSize = '24px';
        badge.style.textShadow = '0 0 10px #ffd700, 0 0 20px #ffa500';
        badge.style.animation = 'levelUpBadge 0.8s ease-out forwards';
        badge.style.zIndex = '1000';
        badge.style.pointerEvents = 'none';

        this.container.style.position = 'relative';
        this.container.appendChild(badge);

        setTimeout(() => badge.remove(), 3000);
    }

    /**
     * 星星爆發特效
     */
    showStarBurst() {
        for (let i = 0; i < 8; i++) {
            setTimeout(() => {
                const star = document.createElement('div');
                star.textContent = '★';
                const angle = (i / 8) * 2 * Math.PI;
                star.style.position = 'absolute';
                star.style.left = '50%';
                star.style.top = '50%';
                star.style.fontSize = '30px';
                star.style.color = '#ffd700';
                star.style.animation = 'starBurst 1s ease-out forwards';
                star.style.transformOrigin = 'center';
                star.style.zIndex = '999';
                star.style.pointerEvents = 'none';

                this.container.appendChild(star);
                setTimeout(() => star.remove(), 1000);
            }, i * 50);
        }
    }

    /**
     * 顯示獲得點數提示
     */
    showPointsGained(points) {
        const pointsBadge = document.createElement('div');
        pointsBadge.innerHTML = `+${points} 點數`;
        pointsBadge.style.position = 'absolute';
        pointsBadge.style.left = '50%';
        pointsBadge.style.top = '20px';
        pointsBadge.style.transform = 'translateX(-50%)';
        pointsBadge.style.fontSize = '18px';
        pointsBadge.style.fontWeight = 'bold';
        pointsBadge.style.color = '#28a745';
        pointsBadge.style.animation = 'heartFloat 2s ease-out forwards';
        pointsBadge.style.zIndex = '1001';
        pointsBadge.style.pointerEvents = 'none';

        this.container.appendChild(pointsBadge);
        setTimeout(() => pointsBadge.remove(), 2000);
    }

    /**
     * 播放互動動畫
     */
    playInteraction(type) {
        const effects = {
            feed: () => this.showParticles('🍖', '#ff6b81', 6),
            bath: () => this.showParticles('💧', '#4ecdc4', 10),
            play: () => this.showParticles('⚽', '#ffd93d', 8),
            sleep: () => this.showParticles('💤', '#9b59b6', 5)
        };

        if (effects[type]) {
            effects[type]();
        }
    }

    /**
     * 顯示粒子特效
     */
    showParticles(emoji, color, count) {
        const rect = this.container.getBoundingClientRect();

        for (let i = 0; i < count; i++) {
            setTimeout(() => {
                const particle = document.createElement('div');
                particle.textContent = emoji;
                particle.style.position = 'absolute';
                particle.style.fontSize = '28px';
                particle.style.left = `${50 + (Math.random() - 0.5) * 80}%`;
                particle.style.top = '50%';
                particle.style.animation = 'heartFloat 2s ease-out forwards';
                particle.style.zIndex = '1000';
                particle.style.pointerEvents = 'none';

                this.container.style.position = 'relative';
                this.container.appendChild(particle);

                setTimeout(() => particle.remove(), 2000);
            }, i * 150);
        }
    }

    /**
     * 工具：創建圓形
     */
    createCircle(cx, cy, r, fill) {
        const circle = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
        circle.setAttribute('cx', cx);
        circle.setAttribute('cy', cy);
        circle.setAttribute('r', r);
        circle.setAttribute('fill', fill);
        return circle;
    }

    /**
     * 工具：創建橢圓
     */
    createEllipse(cx, cy, rx, ry, fill) {
        const ellipse = document.createElementNS('http://www.w3.org/2000/svg', 'ellipse');
        ellipse.setAttribute('cx', cx);
        ellipse.setAttribute('cy', cy);
        ellipse.setAttribute('rx', rx);
        ellipse.setAttribute('ry', ry);
        ellipse.setAttribute('fill', fill);
        return ellipse;
    }

    /**
     * 工具：創建三角形
     */
    createTriangle(x1, y1, x2, y2, x3, y3, fill) {
        const polygon = document.createElementNS('http://www.w3.org/2000/svg', 'polygon');
        polygon.setAttribute('points', `${x1},${y1} ${x2},${y2} ${x3},${y3}`);
        polygon.setAttribute('fill', fill);
        return polygon;
    }

    /**
     * 工具：創建路徑
     */
    createPath(d, stroke, strokeWidth = 2, fill = 'none') {
        const path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
        path.setAttribute('d', d);
        path.setAttribute('stroke', stroke);
        path.setAttribute('stroke-width', strokeWidth);
        path.setAttribute('fill', fill);
        path.setAttribute('stroke-linecap', 'round');
        return path;
    }

    /**
     * 工具：顏色變暗
     */
    darkenColor(color, percent) {
        const num = parseInt(color.replace('#', ''), 16);
        const amt = Math.round(2.55 * percent);
        const R = (num >> 16) - amt;
        const G = ((num >> 8) & 0x00FF) - amt;
        const B = (num & 0x0000FF) - amt;
        return '#' + (0x1000000 + (R < 255 ? R < 1 ? 0 : R : 255) * 0x10000 +
            (G < 255 ? G < 1 ? 0 : G : 255) * 0x100 +
            (B < 255 ? B < 1 ? 0 : B : 255))
            .toString(16).slice(1);
    }

    /**
     * 銷毀實例
     */
    destroy() {
        if (this.container) {
            this.container.innerHTML = '';
        }
        document.removeEventListener('mousemove', this.onMouseMove);
    }
}

// 全局暴露
window.PetAvatar = PetAvatar;
