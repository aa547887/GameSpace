# 剩餘 4 項功能實現方案（無需新增資料庫欄位）

## 📋 檢查日期：2025-11-06

本文件說明如何使用**現有資料庫欄位**實現剩餘的 4 項商業規則，無需新增任何資料表欄位。

---

## ✅ 可行性分析

### 資料庫現有資源

1. **WalletHistory 表** (11 欄位)：
   - 可用欄位：`ItemCode`, `ChangeType`, `ChangeTime`, `Description`
   - 現有模式：
     - Orders: `ORD-XXXXXX`
     - Coupons: `CPN-YYMM-XXXNNN`
     - EVouchers: `EV-TYPE-XXXX-NNNNNN`
     - Sign-ins: `AUTO-SIGN-XXXXXXXXXX`
     - Initial balance: `INIT-BAL-XXX`
   - **可擴展**：支持新的 ItemCode 模式用於追蹤事件

2. **MiniGame 表** (24 欄位)：
   - 已有欄位：`Level`, `Result`, `StartTime`, `EndTime`
   - **可用於**：計算用戶當前應挑戰的關卡（查詢最後一場完成的遊戲）

3. **Pet 表** (26 欄位)：
   - 已有欄位：飢餓、心情、體力、清潔、健康 (所有 int 0-100)
   - **可用於**：檢查全滿狀態、應用每日衰減

4. **SystemSettings 表** (56 配置項)：
   - 已有配置：
     - `Pet.DailyFullStatsBonus.Experience = 100`
     - `Pet.DailyFullStatsBonus.Points = 0`
     - `Pet.DailyDecay.HungerDecay = 20`
     - `Pet.DailyDecay.MoodDecay = 30`
     - `Pet.DailyDecay.StaminaDecay = 10`
     - `Pet.DailyDecay.CleanlinessDecay = 20`
     - `Pet.DailyDecay.HealthDecay = 0`

---

## 🎯 實現方案

### 1. 每日狀態全滿獎勵 ✅ 可實現

**商業規則**：
> 寵物若於每日首次同時達到飢餓、心情、體力、清潔值皆 100，則額外獲得 100 點寵物經驗值

**實現策略**：

#### 使用 WalletHistory 追蹤事件

**ItemCode 模式**：`PET-FULLSTATS-YYYY-MM-DD`
- 例如：`PET-FULLSTATS-2025-11-06` 表示 2025-11-06 已發放全滿獎勵

**邏輯流程**：

```csharp
// 位置：PetService.cs → InteractWithPetAsync 方法末尾
// 檢查是否達成全滿狀態
if (pet.Hunger == 100 && pet.Mood == 100 &&
    pet.Stamina == 100 && pet.Cleanliness == 100)
{
    pet.Health = 100; // 已實現的全滿回復

    // 檢查今日是否已發放全滿獎勵
    var today = _appClock.Now.Date; // UTC+8
    var todayItemCode = $"PET-FULLSTATS-{today:yyyy-MM-dd}";

    var alreadyGrantedToday = await _context.WalletHistory
        .AnyAsync(w => w.UserID == userId
                    && w.ItemCode == todayItemCode
                    && !w.IsDeleted);

    if (!alreadyGrantedToday)
    {
        // 讀取獎勵配置
        var bonusExp = await _systemSettingsService
            .GetIntSettingAsync("Pet.DailyFullStatsBonus.Experience", 100);
        var bonusPoints = await _systemSettingsService
            .GetIntSettingAsync("Pet.DailyFullStatsBonus.Points", 0);

        // 發放寵物經驗值
        pet.Experience += bonusExp;

        // 檢查升級（使用現有的 CheckForLevelUp 方法）
        var (leveledUp, _) = await CheckForLevelUp(pet);

        // 發放會員點數（如果有配置）
        if (bonusPoints > 0)
        {
            var wallet = await _context.User_Wallet
                .FirstOrDefaultAsync(w => w.UserID == userId && !w.IsDeleted);
            if (wallet != null)
            {
                wallet.UserPoint += bonusPoints;
            }
        }

        // 記錄到 WalletHistory（用於防重複發放）
        var historyRecord = new WalletHistory
        {
            UserID = userId,
            ChangeType = "Point",
            PointsChanged = bonusPoints,
            ItemCode = todayItemCode,
            Description = $"寵物狀態全滿獎勵（經驗值+{bonusExp}）",
            ChangeTime = _appClock.Now
        };
        _context.WalletHistory.Add(historyRecord);

        // 更新回傳訊息
        result.Message += $" | 🎉 首次達成今日狀態全滿！獲得額外 {bonusExp} 經驗值！";
    }
}
```

**優點**：
- ✅ 無需新增資料表欄位
- ✅ 利用現有 WalletHistory 表的 ItemCode 機制
- ✅ 可追溯（管理員可查詢 WalletHistory 看到所有全滿獎勵記錄）
- ✅ 支持配置化（從 SystemSettings 讀取獎勵數值）

**修改檔案**：
- `Services/PetService.cs` (新增邏輯到 InteractWithPetAsync 方法末尾)

---

### 2. 難度進程機制 ✅ 可實現

**商業規則**：
> 冒險首次從第 1 關開始。若當前關卡勝利，則下次冒險提升至下一關；若失敗則留在同一關卡（最高第 3 關）

**實現策略**：

#### 查詢 MiniGame 表的遊戲歷史

**邏輯流程**：

```csharp
// 位置：GamePlayService.cs → StartGameAsync 方法
// 新增方法：GetUserNextGameLevel

private async Task<int> GetUserNextGameLevelAsync(int userId)
{
    // 查詢該用戶最後一場完成的遊戲
    var lastGame = await _context.MiniGame
        .Where(g => g.UserID == userId
                 && g.EndTime != null  // 已完成
                 && g.Result != null   // 有結果
                 && !g.Aborted)        // 非中斷
        .OrderByDescending(g => g.EndTime)
        .FirstOrDefaultAsync();

    if (lastGame == null)
    {
        // 首次遊戲，從第 1 關開始
        return 1;
    }

    // 根據上次結果決定下次關卡
    int nextLevel;
    if (lastGame.Result == "Win")
    {
        // 勝利：提升至下一關（最高第 3 關）
        nextLevel = Math.Min(lastGame.Level + 1, 3);
    }
    else // "Lose"
    {
        // 失敗：留在同一關
        nextLevel = lastGame.Level;
    }

    return nextLevel;
}

// 修改 StartGameAsync 方法
public async Task<(bool success, string message, string? sessionId)> StartGameAsync(int userId)
{
    // ... 現有的檢查邏輯 ...

    // 確定本次遊戲關卡（不再由前端傳入）
    int level = await GetUserNextGameLevelAsync(userId);

    // ... 後續邏輯使用 level 變數 ...
}
```

**前端調整**：

```javascript
// Pet/Index.cshtml 中的 startAdventure 函數
async function startAdventure(level) {
    // 移除 level 參數，改為由後端自動決定
    const response = await fetch('/MiniGame/Api/GamePlay/Start', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' }
        // body 不再傳 level
    });
    // ...
}
```

**優點**：
- ✅ 無需新增資料表欄位
- ✅ 利用現有 MiniGame 表的 Level 和 Result 欄位
- ✅ 邏輯清晰：贏了晉級，輸了重來
- ✅ 歷史記錄完整（每場遊戲都有記錄）

**修改檔案**：
- `Services/GamePlayService.cs` (新增 GetUserNextGameLevelAsync 方法，修改 StartGameAsync)
- `ApiControllers/GamePlayApiController.cs` (移除 level 參數)
- `Views/Pet/Index.cshtml` (修改 JavaScript 呼叫)

---

### 3. 每日衰減機制 ⚠️ 需跨邊界（背景服務）

**商業規則**：
> 每日 UTC+8 00:00，飢餓值 -20、心情值 -30、體力值 -10、清潔值 -20

**實現策略**：

#### 創建背景服務（需要放在 Infrastructure/ 或 Program.cs）

**挑戰**：
- ❌ **跨越 Areas\MiniGame 邊界**：背景服務需要註冊在 `Program.cs`
- ⚠️ **需要用戶批准**：是否允許在 `Infrastructure/` 創建 `PetDailyDecayService.cs`

**如果允許跨邊界，實現方案**：

```csharp
// 位置：Infrastructure/BackgroundServices/PetDailyDecayService.cs (新檔案)
public class PetDailyDecayService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PetDailyDecayService> _logger;

    public PetDailyDecayService(
        IServiceScopeFactory scopeFactory,
        ILogger<PetDailyDecayService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZones.Taipei);

            // 計算下次執行時間（明天 00:00）
            var tomorrow = now.Date.AddDays(1);
            var delay = tomorrow - now;

            _logger.LogInformation(
                "寵物每日衰減服務將在 {NextRun} 執行（{Delay} 後）",
                tomorrow, delay);

            await Task.Delay(delay, stoppingToken);

            // 執行每日衰減
            await ApplyDailyDecay();
        }
    }

    private async Task ApplyDailyDecay()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<GameSpacedatabaseContext>();
        var settingsService = scope.ServiceProvider
            .GetRequiredService<ISystemSettingsService>();

        // 讀取衰減配置
        var hungerDecay = await settingsService
            .GetIntSettingAsync("Pet.DailyDecay.HungerDecay", 20);
        var moodDecay = await settingsService
            .GetIntSettingAsync("Pet.DailyDecay.MoodDecay", 30);
        var staminaDecay = await settingsService
            .GetIntSettingAsync("Pet.DailyDecay.StaminaDecay", 10);
        var cleanlinessDecay = await settingsService
            .GetIntSettingAsync("Pet.DailyDecay.CleanlinessDecay", 20);

        // 查詢所有未刪除的寵物
        var pets = await context.Pets
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        foreach (var pet in pets)
        {
            // 應用衰減（使用鉗位確保不低於 0）
            pet.Hunger = Math.Max(0, pet.Hunger - hungerDecay);
            pet.Mood = Math.Max(0, pet.Mood - moodDecay);
            pet.Stamina = Math.Max(0, pet.Stamina - staminaDecay);
            pet.Cleanliness = Math.Max(0, pet.Cleanliness - cleanlinessDecay);
        }

        await context.SaveChangesAsync();

        _logger.LogInformation(
            "每日衰減完成：已更新 {Count} 隻寵物", pets.Count);
    }
}

// 位置：Program.cs
// 在 builder.Services 區塊中新增
builder.Services.AddHostedService<PetDailyDecayService>();
```

**替代方案（不跨邊界）**：
- 在用戶每次訪問時，檢查上次訪問日期（使用 Pet 表現有欄位，如 `LevelUpTime`？）
- 如果距離上次訪問超過 1 天，應用 N 天的衰減
- **缺點**：不是真正的「每日 00:00」執行，而是「下次訪問時」執行

**需要用戶決策**：
1. 是否允許創建 `Infrastructure/BackgroundServices/PetDailyDecayService.cs`？
2. 是否允許修改 `Program.cs` 註冊背景服務？

**如不允許跨邊界**：
- 建議調整商業規則為「訪問時檢查並應用衰減」

---

### 4. 狀態描述邏輯 ✅ 可實現（純前端）

**商業規則**：
> 飢餓、心情、體力、清潔、健康值 < 20，分別代表寵物處於飢餓、難過、很累、身體很臭、生病的狀態，否則為飽食、開心、充分休息、身體乾淨、很健康的狀態

**實現策略**：

#### 前端 JavaScript 實現

```javascript
// 位置：Pet/Index.cshtml 中的 Vue 組件或純 JS

function getStatusDescription(attrName, attrValue) {
    const statusMap = {
        hunger: { low: '飢餓', high: '飽食' },
        mood: { low: '難過', high: '開心' },
        stamina: { low: '很累', high: '充分休息' },
        cleanliness: { low: '身體很臭', high: '身體乾淨' },
        health: { low: '生病', high: '很健康' }
    };

    const threshold = 20;
    const status = statusMap[attrName];

    if (!status) return '';

    return attrValue < threshold ? status.low : status.high;
}

// 在顯示寵物狀態時使用
function updatePetStatusDisplay(pet) {
    document.getElementById('hungerStatus').textContent =
        getStatusDescription('hunger', pet.hunger);
    document.getElementById('moodStatus').textContent =
        getStatusDescription('mood', pet.mood);
    document.getElementById('staminaStatus').textContent =
        getStatusDescription('stamina', pet.stamina);
    document.getElementById('cleanlinessStatus').textContent =
        getStatusDescription('cleanliness', pet.cleanliness);
    document.getElementById('healthStatus').textContent =
        getStatusDescription('health', pet.health);
}
```

**HTML 調整**：

```html
<!-- Pet/Index.cshtml -->
<div class="status-item">
    <span class="status-label">飢餓值</span>
    <span class="status-value">@Model.Pet.Hunger</span>
    <span class="status-description" id="hungerStatus">
        @(Model.Pet.Hunger < 20 ? "飢餓" : "飽食")
    </span>
</div>
<!-- 其他屬性類似 -->
```

**優點**：
- ✅ 純前端實現，無需後端修改
- ✅ 無需資料庫變更
- ✅ 即時更新（互動後立即顯示新狀態）

**修改檔案**：
- `Views/Pet/Index.cshtml` (新增 HTML + JavaScript)

---

## 📊 實現可行性總結

| 功能 | 可行性 | 需要新欄位 | 需要跨邊界 | 優先級 |
|------|--------|------------|------------|--------|
| 1. 每日狀態全滿獎勵 | ✅ 可實現 | ❌ 否（用 WalletHistory） | ❌ 否 | P1 |
| 2. 難度進程機制 | ✅ 可實現 | ❌ 否（查詢 MiniGame） | ❌ 否 | P1 |
| 3. 每日衰減機制 | ⚠️ 需批准 | ❌ 否 | ⚠️ **是** (Program.cs + Infrastructure) | P2 |
| 4. 狀態描述邏輯 | ✅ 可實現 | ❌ 否（純前端） | ❌ 否 | P2 |

---

## 🎯 建議執行順序

### 階段 1：立即可執行（無需跨邊界）
1. ✅ 難度進程機制（修改 GamePlayService.cs）
2. ✅ 每日狀態全滿獎勵（修改 PetService.cs）
3. ✅ 狀態描述邏輯（修改 Pet/Index.cshtml）

### 階段 2：需用戶批准
4. ⚠️ 每日衰減機制（需要在 Infrastructure/ 創建背景服務 + Program.cs 註冊）

---

## 💡 關於每日衰減的決策問題

**問題**：商業規則要求「每日 UTC+8 00:00 自動執行」，但這需要背景服務，會跨越 Areas\MiniGame 邊界。

**選項 A**：批准跨邊界（推薦）
- ✅ 完全符合商業規則
- ✅ 自動執行，用戶無感
- ⚠️ 需要修改 Program.cs 和 Infrastructure/

**選項 B**：調整為「訪問時檢查」模式
- ✅ 不跨邊界（僅修改 PetService.cs）
- ⚠️ 不完全符合商業規則（不是真正的「每日 00:00」）
- ⚠️ 如果用戶長期不訪問，衰減會累積（例如 3 天未訪問，一次性扣除 3 天的衰減）

**請用戶決策**：
- 是否允許創建 `Infrastructure/BackgroundServices/PetDailyDecayService.cs`？
- 是否允許修改 `Program.cs` 註冊 `AddHostedService<PetDailyDecayService>()`？

---

*文件生成時間：2025-11-06*
