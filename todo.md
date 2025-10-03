# TODO（Claude Code 工作待辦）
> 用來保存每次對話前的需求整理與任務拆解，避免上下文遺失。

## 本次任務（2025-10-03 當前時間）

### 主要目標：修復 GameSpace MiniGame Area 所有編譯錯誤，達成 0 錯誤可運行狀態

#### 階段 1：建立缺少的 ViewModel 類型 (當前執行中)
- [ ] 創建 `SignInRuleReadModel` - 簽到規則讀取模型
- [ ] 創建 `SignInRecordReadModel` - 簽到記錄讀取模型
- [ ] 創建 `SignInStatsQueryModel` - 簽到統計查詢模型
- [ ] 創建 `SignInStatsSummary` - 簽到統計摘要
- [ ] 創建 `SignInRulesModel` - 簽到規則模型
- [ ] 創建 `PetRuleReadModel` - 寵物規則讀取模型
- [ ] 創建 `PetSummary` - 寵物摘要模型
- [ ] 創建 `PetSkinColorChangeLog` - 寵物膚色變更記錄
- [ ] 創建 `PetBackgroundColorChangeLog` - 寵物背景變更記錄
- [ ] 創建 `PetBackgroundOption` - 寵物背景選項
- [ ] 創建 `UserCouponReadModel` - 用戶優惠券讀取模型
- [ ] 創建 `WalletQueryModel` - 錢包查詢模型
- [ ] 修復 `UserWallet` 類型引用問題

#### 階段 2：創建缺少的 Service 介面和實作
- [ ] 創建 `IPetLevelUpRuleService` 介面和實作
- [ ] 創建 `IPetBackgroundChangeSettingsService` 介面和實作
- [ ] 創建 `IPointsSettingsStatisticsService` 介面和實作
- [ ] 在 `ServiceExtensions.cs` 註冊所有新服務

#### 階段 3：修復 Controller 問題
- [ ] 修復 `PermissionController` 中的 ViewModels 命名空間問題
- [ ] 修復 `PetLevelUpRuleController` 的依賴注入
- [ ] 修復 `Settings/PetBackgroundChangeSettingsController` 的依賴注入
- [ ] 修復 `Settings/PointsSettingsController` 的依賴注入
- [ ] 修復所有 Controller 的警告（CS0108 隱藏成員）

#### 階段 4：最終驗證
- [ ] 執行完整編譯測試 (dotnet build)
- [ ] 確認錯誤數 = 0
- [ ] 確認警告已處理
- [ ] Git commit 所有變更
- [ ] Git push 到遠端

### 執行記錄
- 已完成 Agent 初步稽核和修復（修復 122 個錯誤）
- 當前錯誤數：402 個（因依賴問題增加）
- 開始系統性創建缺少的 ViewModel 類型...

## 歷史紀錄

### 2025-10-03 初步稽核和修復（已完成）
- [x] 閱讀所有必要文件並連線資料庫
- [x] 執行完整專案稽核並生成詳細報告
  - 檔案：`FINAL_AUDIT_REPORT_2025.md`
  - 檔案：`MODIFICATION_SUMMARY.md`
- [x] 修復 Razor Views Tag Helper 錯誤 (72個)
  - 修改 8 個 View 檔案
- [x] 修復 Views 命名空間錯誤 (12個)
  - 修改 6 個 View 檔案
- [x] 創建缺少的 ViewModels 和 Settings (20個)
  - 新建/修改 7 個 Model 檔案
- [x] 修復服務介面命名空間衝突 (18個)
  - 修改 3 個 Service 介面檔案

**成果**：從 294 錯誤降至 223 錯誤（修復率 24.1%）


補充:
根據終端記錄，當前 Claude Code 剩餘的工作如下：

## 當前狀態總結

### 已完成的工作
1. ✅ 閱讀所有必要文件並連線資料庫
2. ✅ 執行完整專案稽核並生成詳細報告  
3. ✅ 修復 Razor Views Tag Helper 錯誤 (72個)
4. ✅ 修復 Views 命名空間錯誤 (12個)
5. ✅ 創建缺少的 ViewModels 和 Settings (20個)
6. ✅ 修復服務介面命名空間衝突 (18個)

### 剩餘工作 (223個編譯錯誤待修復)

#### 1. 主要錯誤類型分析
從最後的編譯輸出可以看到以下主要錯誤：

**A. 缺少類型定義錯誤 (CS0246)**
- `SystemStatsModel` 類型找不到
- `ChartData` 類型找不到  
- 多個 ViewModels 類型缺失

**B. 命名空間衝突錯誤 (CS0101)**
- 重複的類別定義 (SignInRuleReadModel, SignInRecordReadModel, PetRuleReadModel, PetSummary)
- 已刪除 `MissingViewModels.cs` 但可能還有其他重複定義

**C. 服務實作問題**
- `ManagerService.cs` 在修復過程中出現命名混亂
- 需要重新建立正確的 ManagerService 實作
- 多個服務缺少 `using GameSpace.Models;` 語句

#### 2. 具體待修復項目

**A. 創建缺少的 ViewModels**
- `SystemStatsModel` (AdminController.cs 需要)
- `ChartData` (AdminDashboardController.cs 需要)
- 其他控制器需要的 ViewModels

**B. 修復 ManagerService.cs**
- 重新建立正確的 ManagerService 實作
- 使用正確的 ManagerDatum 實體和屬性名稱
- 確保與 IManagerService 介面一致

**C. 修復服務命名空間**
- 為所有服務檔案添加 `using GameSpace.Models;`
- 修復 ViewModels 命名空間引用

**D. 清理重複定義**
- 檢查並移除重複的類別定義
- 確保每個 ViewModel 只定義一次

#### 3. 建議的修復順序

1. **優先修復 ManagerService.cs** - 這是核心服務，影響多個控制器
2. **創建缺少的 ViewModels** - 解決 CS0246 錯誤
3. **修復命名空間引用** - 添加缺少的 using 語句
4. **清理重複定義** - 解決 CS0101 錯誤
5. **執行完整建置測試** - 驗證所有錯誤已修復

#### 4. 關鍵檔案位置

- **服務檔案**: `Areas/MiniGame/Services/`
- **ViewModels**: `Areas/MiniGame/Models/ViewModels/`
- **Settings**: `Areas/MiniGame/Models/Settings/`
- **服務註冊**: `Areas/MiniGame/config/ServiceExtensions.cs`

#### 5. 注意事項

- 所有修改必須在 `Areas/MiniGame/` 範圍內
- 使用正確的資料庫實體名稱 (ManagerDatum, ManagerData 等)
- 保持繁體中文註解和 UI 文字
- 確保所有服務都在 ServiceExtensions.cs 中註冊

**下一步**: 建議先執行 `dotnet build` 獲取最新的完整錯誤列表，然後按優先級逐一修復。
