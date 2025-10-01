# 新增功能說明

## 已完成的檔案

### 1. 寵物換色/換背景所需點數設定
- **檔案**: `ColorBackgroundCostSettings.cshtml`
- **功能**: 管理寵物換色和換背景所需的點數設定
- **特色**: 
  - 分別設定換色和換背景的點數需求
  - 即時儲存設定
  - 載入現有設定值

### 2. 寵物顏色/背景選項管理
- **檔案**: `ColorBackgroundOptions.cshtml`
- **功能**: 管理寵物可選的顏色和背景選項
- **特色**:
  - 新增/刪除顏色選項
  - 新增/刪除背景選項
  - 啟用/停用選項
  - 設定顯示順序
  - 即時預覽顏色值

### 3. 小遊戲系統設定
- **檔案**: `GameSystemSettings.cshtml`
- **功能**: 管理小遊戲系統的各種設定
- **特色**:
  - 每日遊戲次數限制設定
  - 遊戲獎勵比例設定（點數、經驗值、優惠券）
  - 獎勵類型開關控制

## 需要新增的資料庫模型

### 在 DatabaseModels.cs 中需要新增的模型：

1. **PetColorChangeCost** - 寵物換色所需點數設定
2. **PetBackgroundChangeCost** - 寵物換背景所需點數設定
3. **PetColorOption** - 寵物顏色選項
4. **PetBackgroundOption** - 寵物背景選項
5. **PetLevelUpRule** - 寵物升級規則
6. **PetInteractionBonusRule** - 寵物互動狀態增益規則
7. **DailyGameLimit** - 每日遊戲次數限制設定
8. **GameRewardSettings** - 遊戲獎勵設定

## 需要新增的 Controller 方法

### 在 AdminPetController 中需要新增的方法：

1. **寵物換色/換背景所需點數設定**:
   - `GetColorCost()` - 取得換色所需點數
   - `UpdateColorCost(int pointsRequired)` - 更新換色所需點數
   - `GetBackgroundCost()` - 取得換背景所需點數
   - `UpdateBackgroundCost(int pointsRequired)` - 更新換背景所需點數

2. **寵物顏色/背景選項管理**:
   - `GetColorOptions()` - 取得所有顏色選項
   - `AddColorOption()` - 新增顏色選項
   - `ToggleColorStatus(int id)` - 切換顏色選項狀態
   - `DeleteColorOption(int id)` - 刪除顏色選項
   - `GetBackgroundOptions()` - 取得所有背景選項
   - `AddBackgroundOption()` - 新增背景選項
   - `ToggleBackgroundStatus(int id)` - 切換背景選項狀態
   - `DeleteBackgroundOption(int id)` - 刪除背景選項

### 在 AdminMiniGameController 中需要新增的方法：

1. **小遊戲系統設定**:
   - `GetDailyGameLimit()` - 取得每日遊戲次數限制
   - `UpdateDailyGameLimit(int dailyLimit)` - 更新每日遊戲次數限制
   - `GetGameRewardSettings()` - 取得遊戲獎勵設定
   - `UpdateGameRewardSettings()` - 更新遊戲獎勵設定

## 需要更新的資料庫上下文

### 在 MiniGameDbContext.cs 中需要新增的 DbSet：

```csharp
// 寵物系統設定相關表
public DbSet<PetColorChangeCost> PetColorChangeCosts { get; set; } = null!;
public DbSet<PetBackgroundChangeCost> PetBackgroundChangeCosts { get; set; } = null!;
public DbSet<PetColorOption> PetColorOptions { get; set; } = null!;
public DbSet<PetBackgroundOption> PetBackgroundOptions { get; set; } = null!;
public DbSet<PetLevelUpRule> PetLevelUpRules { get; set; } = null!;
public DbSet<PetInteractionBonusRule> PetInteractionBonusRules { get; set; } = null!;

// 小遊戲系統設定相關表
public DbSet<DailyGameLimit> DailyGameLimits { get; set; } = null!;
public DbSet<GameRewardSettings> GameRewardSettings { get; set; } = null!;
```

## 下一步

1. 在 DatabaseModels.cs 中新增上述資料庫模型
2. 在 MiniGameDbContext.cs 中新增對應的 DbSet 和配置
3. 在 AdminPetController.cs 中新增對應的 Action 方法
4. 在 AdminMiniGameController.cs 中新增對應的 Action 方法
5. 執行資料庫遷移來建立新的資料表
6. 測試所有功能是否正常運作

## 注意事項

- 所有新增的 View 都使用 Bootstrap 4 樣式
- 使用 jQuery 進行 AJAX 請求
- 包含完整的錯誤處理和成功提示
- 所有表單都有適當的驗證
- 使用 UTF-8 編碼確保中文顯示正常
