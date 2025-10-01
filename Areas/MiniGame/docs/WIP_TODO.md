# MiniGame Area 後台功能修復 - 工作進度清單

## 📋 官方 TODO 清單

### 🎯 Phase 1: 寵物系統功能完善

#### 1.1 寵物換色/換背景所需點數設定
- [x] 新增寵物換色所需點數設定功能 (ID: P1-1.1-01) (ID: P1-1.1-01, Commit: 941bc7e)
- [x] 新增寵物換背景所需點數設定功能 (ID: P1-1.1-02) (ID: P1-1.1-02, Commit: 254a866)
- [x] 建立點數設定管理介面 (ID: P1-1.1-03) (ID: P1-1.1-03, Commit: 01a928a)
- [x] 實作點數設定儲存邏輯 (ID: P1-1.1-04) (ID: P1-1.1-04, Commit: 待提交)

#### 1.2 寵物顏色/背景選項管理
- [ ] 新增寵物顏色選項管理功能 (ID: P1-1.2-01)
- [ ] 新增寵物背景選項管理功能 (ID: P1-1.2-02)
- [ ] 建立選項管理介面 (ID: P1-1.2-03)
- [ ] 實作選項增刪改查邏輯 (ID: P1-1.2-04)

#### 1.3 升級規則詳細設定
- [ ] 完善寵物升級規則設定 (ID: P1-1.3-01)
- [ ] 新增等級對應經驗值設定 (ID: P1-1.3-02)
- [ ] 新增升級獎勵設定 (ID: P1-1.3-03)
- [ ] 實作升級規則驗證邏輯 (ID: P1-1.3-04)

#### 1.4 互動狀態增益規則設定
- [ ] 新增互動狀態增益規則設定 (ID: P1-1.4-01)
- [ ] 新增狀態增益計算邏輯 (ID: P1-1.4-02)
- [ ] 建立狀態增益管理介面 (ID: P1-1.4-03)
- [ ] 實作狀態增益驗證邏輯 (ID: P1-1.4-04)

### 🎯 Phase 2: 小遊戲系統功能完善

#### 2.1 每日遊戲次數限制設定
- [ ] 新增每日遊戲次數限制設定（預設一天三次） (ID: P2-2.1-01)
- [ ] 建立次數限制管理介面 (ID: P2-2.1-02)
- [ ] 實作次數限制驗證邏輯 (ID: P2-2.1-03)
- [ ] 新增次數限制統計功能 (ID: P2-2.1-04)

#### 2.2 獎勵種類詳細設定
- [ ] 完善獎勵種類設定（會員點數／寵物經驗值／商城優惠券） (ID: P2-2.2-01)
- [ ] 新增獎勵比例設定功能 (ID: P2-2.2-02)
- [ ] 建立獎勵管理介面 (ID: P2-2.2-03)
- [ ] 實作獎勵計算邏輯 (ID: P2-2.2-04)

### 🎯 Phase 3: 權限控制優化

#### 3.1 權限控制完善
- [ ] 檢查所有功能權限控制 (ID: P3-3.1-01)
- [ ] 優化 sidebar 權限顯示邏輯 (ID: P3-3.1-02)
- [ ] 新增權限驗證機制 (ID: P3-3.1-03)
- [ ] 實作權限錯誤處理 (ID: P3-3.1-04)

#### 3.2 安全性強化
- [ ] 新增輸入驗證機制 (ID: P3-3.2-01)
- [ ] 實作 SQL 注入防護 (ID: P3-3.2-02)
- [ ] 新增 XSS 防護機制 (ID: P3-3.2-03)
- [ ] 實作 CSRF 防護 (ID: P3-3.2-04)

### 🎯 Phase 4: 資料庫優化

#### 4.1 資料庫結構完善
- [ ] 檢查資料庫表結構 (ID: P4-4.1-01)
- [ ] 新增缺失的欄位 (ID: P4-4.1-02)
- [ ] 優化資料庫索引 (ID: P4-4.1-03)
- [ ] 實作資料庫遷移 (ID: P4-4.1-04)

#### 4.2 資料驗證強化
- [ ] 新增資料完整性檢查 (ID: P4-4.2-01)
- [ ] 實作資料驗證邏輯 (ID: P4-4.2-02)
- [ ] 新增資料備份機制 (ID: P4-4.2-03)
- [ ] 實作資料恢復功能 (ID: P4-4.2-04)

### 🎯 Phase 5: 使用者體驗優化

#### 5.1 介面優化
- [ ] 優化響應式設計 (ID: P5-5.1-01)
- [ ] 新增載入動畫 (ID: P5-5.1-02)
- [ ] 優化錯誤訊息顯示 (ID: P5-5.1-03)
- [ ] 新增操作確認機制 (ID: P5-5.1-04)

#### 5.2 功能優化
- [ ] 新增批量操作功能 (ID: P5-5.2-01)
- [ ] 優化搜尋功能 (ID: P5-5.2-02)
- [ ] 新增匯出功能 (ID: P5-5.2-03)
- [ ] 實作資料統計功能 (ID: P5-5.2-04)

### 🎯 Phase 6: 測試與部署

#### 6.1 功能測試
- [ ] 單元測試 (ID: P6-6.1-01)
- [ ] 整合測試 (ID: P6-6.1-02)
- [ ] 效能測試 (ID: P6-6.1-03)
- [ ] 安全性測試 (ID: P6-6.1-04)

#### 6.2 部署準備
- [ ] 環境配置檢查 (ID: P6-6.2-01)
- [ ] 依賴套件檢查 (ID: P6-6.2-02)
- [ ] 部署腳本準備 (ID: P6-6.2-03)
- [ ] 監控設定 (ID: P6-6.2-04)

---

## 📝 完成紀要記錄區

### P1-1.1-01: 新增寵物換色所需點數設定功能
**完成紀要**：
- 建立 PetSkinColorPointSetting Model 和 ViewModel
- 實作 IPetSkinColorPointSettingService 介面和服務
- 建立 PetSkinColorPointSettingController 控制器
- 建立 Index、Create、Edit Views
- 建立 PetSkinColorPointSettings 資料庫表
- 插入測試資料
- 完成完整的 CRUD 功能

**權限名與 Policy 名稱**：PetSkinColorPointSettingManagement
**對應 URL/Route**：/MiniGame/PetSkinColorPointSetting
**涉及的資料表/欄位/索引/檢核**：PetSkinColorPointSettings 表，包含 PetLevel、RequiredPoints、IsEnabled 等欄位
**測試檔案清單**：PetSkinColorPointSettingService.cs, PetSkinColorPointSettingController.cs

### P1-1.1-02: 新增寵物換背景所需點數設定功能
**完成紀要**：
- 建立 PetBackgroundPointSetting Model 和 ViewModel
- 實作 IPetBackgroundPointSettingService 介面和服務
- 建立 PetBackgroundPointSettingController 控制器
- 建立 Index、Create、Edit Views
- 建立 PetBackgroundPointSettings 資料庫表
- 插入測試資料
- 完成完整的 CRUD 功能

**權限名與 Policy 名稱**：PetBackgroundPointSettingManagement
**對應 URL/Route**：/MiniGame/PetBackgroundPointSetting
**涉及的資料表/欄位/索引/檢核**：PetBackgroundPointSettings 表，包含 PetLevel、RequiredPoints、IsEnabled 等欄位
**測試檔案清單**：PetBackgroundPointSettingService.cs, PetBackgroundPointSettingController.cs

### P1-1.1-03: 建立點數設定管理介面
**完成紀要**：
- 建立 PointSettingManagementViewModels 統一管理 ViewModel
- 建立 PointSettingManagementController 統一管理控制器
- 建立 Index、Statistics、BatchOperation Views
- 整合寵物換色和換背景點數設定的統一管理介面
- 提供統計資料、批量操作、搜尋篩選功能
- 完成完整的統一管理功能

**權限名與 Policy 名稱**：PointSettingManagement
**對應 URL/Route**：/MiniGame/PointSettingManagement
**涉及的資料表/欄位/索引/檢核**：整合 PetSkinColorPointSettings 和 PetBackgroundPointSettings 表
**測試檔案清單**：PointSettingManagementController.cs

### P1-1.1-04: 實作點數設定儲存邏輯
**完成紀要**：
- 建立 IPointSettingStorageService 介面和 PointSettingStorageService 實作
- 實作完整的點數設定儲存、驗證和批量操作邏輯
- 新增 PointSettingStorageModel 和相關結果模型
- 完善 PointSettingManagementController 的儲存功能
- 實作資料驗證、錯誤處理和統計功能
- 完成完整的儲存邏輯功能

**權限名與 Policy 名稱**：PointSettingManagement
**對應 URL/Route**：/MiniGame/PointSettingManagement
**涉及的資料表/欄位/索引/檢核**：PetSkinColorPointSettings 和 PetBackgroundPointSettings 表的完整 CRUD 操作
**測試檔案清單**：PointSettingStorageService.cs, PointSettingManagementController.cs
