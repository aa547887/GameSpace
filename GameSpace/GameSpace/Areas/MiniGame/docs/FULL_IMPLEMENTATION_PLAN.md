# 全面實作計畫 - MiniGame Area

## 執行狀態：進行中
**開始時間**: 2025-10-02 20:15
**預估完成時間**: 2025-10-02 23:00

---

## Phase 1: 重構 Controllers 使用 Service 層

### 已識別的 Controllers (共 23 個，排除 BaseController)

#### 已部分完成 (6個 - 已有 BaseController 但未用 Service)
1. ✅ AdminCouponController - 需建立 ICouponService
2. ✅ AdminEVoucherController - 需建立 IEVoucherService
3. ✅ AdminMiniGameController - 需建立 IMiniGameAdminService
4. ✅ AdminSignInController - 需建立 ISignInService
5. ✅ AdminPetController - 需建立 IPetService
6. ✅ AdminWalletController - 需建立 IWalletService

#### 需全面重構 (17個 - 直接使用 DbContext)
7. ⏳ AdminDiagnosticsController - 需建立 IDiagnosticsService
8. ⏳ AdminHomeController - 需建立 IHomeService
9. ⏳ AdminWalletTypesController - 需建立 IWalletTypesService
10. ⏳ PermissionController - 已有 Service (跳過)
11. ⏳ AdminController - 需建立 IAdminService
12. ⏳ AdminDashboardController - 需建立 IDashboardService
13. ⏳ AdminManagerController - 需建立 IManagerService
14. ⏳ AdminUserController - 需建立 IUserService

#### Pet 設定相關 (11個 - Phase 2 處理繼承)
15. ⏳ PetLevelExperienceSettingController
16. ⏳ PetColorChangeSettingsController
17. ⏳ PetLevelRewardSettingController
18. ⏳ PetLevelUpRuleValidationController
19. ⏳ PetLevelUpRuleController
20. ⏳ PetSkinColorCostSettingController
21. ⏳ PetBackgroundCostSettingController
22. ⏳ DailyGameLimitController

### 實作步驟

#### Step 1.1: 建立 Service Interfaces (8個新介面)
- [ ] ICouponService
- [ ] IEVoucherService
- [ ] IMiniGameAdminService (已存在，需擴充)
- [ ] ISignInService
- [ ] IPetService
- [ ] IWalletService
- [ ] IDiagnosticsService
- [ ] IHomeService
- [ ] IWalletTypesService
- [ ] IAdminService
- [ ] IDashboardService
- [ ] IManagerService
- [ ] IUserService

#### Step 1.2: 實作 Service 類別 (8個新實作)
- [ ] CouponService
- [ ] EVoucherService
- [ ] MiniGameAdminService (擴充)
- [ ] SignInService
- [ ] PetService
- [ ] WalletService
- [ ] DiagnosticsService
- [ ] HomeService
- [ ] WalletTypesService
- [ ] AdminService
- [ ] DashboardService
- [ ] ManagerService
- [ ] UserService

#### Step 1.3: 重構 Controllers
- [ ] 將所有 DbContext 邏輯移到 Service
- [ ] Controllers 只保留 HTTP 處理和呼叫 Service
- [ ] 移除 Controllers 中的 `_context` 欄位

#### Step 1.4: 註冊 Services
- [ ] 在 ServiceExtensions.cs 註冊所有新 Services

---

## Phase 2: Pet 設定 Controllers 繼承 BaseController (11個)

### 需修改的 Controllers
1. ⏳ PetLevelExperienceSettingController
2. ⏳ PetColorChangeSettingsController
3. ⏳ PetLevelRewardSettingController
4. ⏳ PetLevelUpRuleValidationController
5. ⏳ PetLevelUpRuleController
6. ⏳ PetSkinColorCostSettingController
7. ⏳ PetBackgroundCostSettingController
8. ⏳ DailyGameLimitController
9. ⏳ Settings/PointsSettingsController (如果存在)
10. ⏳ Settings/PetBackgroundChangeSettingsController (如果存在)
11. ⏳ Settings/PetColorChangeSettingsController (如果存在)

### 修改內容
- [ ] 改為繼承 `MiniGameBaseController`
- [ ] 加上 `[Authorize(Policy = "AdminOnly")]`
- [ ] 調整建構子使用 `base(context)`
- [ ] 建立對應的 Service (如需要)

---

## Phase 3: P0 功能完整實作

### P0-1: 錢包歷史詳細查詢
**Controller**: AdminWalletController (擴充)
**Service**: IWalletService (新增方法)
**View**: Areas/MiniGame/Views/AdminWallet/History.cshtml (新建)

**實作內容**:
- [ ] Service 新增 GetWalletHistoryDetailsAsync 方法
- [ ] Controller 新增 History Action
- [ ] 建立完整的 History.cshtml View
  - 錢包歷史列表
  - 詳細查詢篩選
  - 分頁功能
  - 匯出功能

### P0-2: 優惠券類型管理 CRUD
**Controller**: AdminCouponTypeController (新建)
**Service**: ICouponTypeService (新建)
**Views**: Areas/MiniGame/Views/AdminCouponType/ (新建資料夾)

**實作內容**:
- [ ] 建立 ICouponTypeService 介面
- [ ] 實作 CouponTypeService 完整 CRUD
- [ ] 建立 AdminCouponTypeController
- [ ] 建立 Views:
  - Index.cshtml (列表)
  - Create.cshtml (新建)
  - Edit.cshtml (編輯)
  - Delete.cshtml (刪除確認)
  - Details.cshtml (詳細資訊)

### P0-3: 電子禮券類型管理 CRUD
**Controller**: AdminEVoucherTypeController (新建)
**Service**: IEVoucherTypeService (新建)
**Views**: Areas/MiniGame/Views/AdminEVoucherType/ (新建資料夾)

**實作內容**:
- [ ] 建立 IEVoucherTypeService 介面
- [ ] 實作 EVoucherTypeService 完整 CRUD
- [ ] 建立 AdminEVoucherTypeController
- [ ] 建立 Views:
  - Index.cshtml (列表)
  - Create.cshtml (新建)
  - Edit.cshtml (編輯)
  - Delete.cshtml (刪除確認)
  - Details.cshtml (詳細資訊)

### P0-4: 簽到統計報表 View
**Controller**: AdminSignInController (擴充)
**Service**: ISignInService (新增方法)
**View**: Areas/MiniGame/Views/AdminSignIn/Statistics.cshtml (新建)

**實作內容**:
- [ ] Service 新增統計方法
- [ ] Controller 新增 Statistics Action
- [ ] 建立完整的 Statistics.cshtml
  - 簽到趨勢圖表
  - 連續簽到統計
  - 獎勵發放統計
  - 使用者排行榜

---

## Phase 4: P1 功能完整實作

### P1-1: 簽到規則設定
**Controller**: AdminSignInRulesController (新建)
**Service**: ISignInRulesService (新建)
**Views**: Areas/MiniGame/Views/AdminSignInRules/ (新建資料夾)

**實作內容**:
- [ ] 建立 ISignInRulesService 介面
- [ ] 實作 SignInRulesService
- [ ] 建立 AdminSignInRulesController
- [ ] 建立 Views:
  - Index.cshtml (規則列表)
  - Edit.cshtml (規則編輯)
  - RewardSettings.cshtml (獎勵設定)

### P1-2: 寵物系統規則設定
**Controller**: AdminPetRulesController (新建)
**Service**: IPetRulesService (新建)
**Views**: Areas/MiniGame/Views/AdminPetRules/ (新建資料夾)

**實作內容**:
- [ ] 建立 IPetRulesService 介面
- [ ] 實作 PetRulesService
- [ ] 建立 AdminPetRulesController
- [ ] 建立 Views:
  - Index.cshtml (規則總覽)
  - LevelUpRules.cshtml (升級規則)
  - ColorChangeRules.cshtml (換色規則)
  - BackgroundRules.cshtml (背景規則)

### P1-3: 遊戲規則設定
**Controller**: AdminGameRulesController (新建)
**Service**: IGameRulesService (新建)
**Views**: Areas/MiniGame/Views/AdminGameRules/ (新建資料夾)

**實作內容**:
- [ ] 建立 IGameRulesService 介面
- [ ] 實作 GameRulesService
- [ ] 建立 AdminGameRulesController
- [ ] 建立 Views:
  - Index.cshtml (規則總覽)
  - DailyLimit.cshtml (每日次數限制)
  - RewardSettings.cshtml (獎勵設定)
  - DifficultySettings.cshtml (難度設定)

### P1-4: 互動獎勵規則管理
**Controller**: AdminInteractionRulesController (新建)
**Service**: IInteractionRulesService (新建)
**Views**: Areas/MiniGame/Views/AdminInteractionRules/ (新建資料夾)

**實作內容**:
- [ ] 建立 IInteractionRulesService 介面
- [ ] 實作 InteractionRulesService
- [ ] 建立 AdminInteractionRulesController
- [ ] 建立 Views:
  - Index.cshtml (規則列表)
  - Create.cshtml (新建規則)
  - Edit.cshtml (編輯規則)
  - Delete.cshtml (刪除確認)

---

## 最終檢查清單

### Controllers 檢查
- [ ] 所有 Controllers 都使用 Service 層
- [ ] 所有 Admin Controllers 都有 [Authorize]
- [ ] 所有 Controllers 都繼承正確的 BaseController
- [ ] 無任何 Controller 直接使用 DbContext

### Services 檢查
- [ ] 所有 Service Interface 都已建立
- [ ] 所有 Service 實作都已完成
- [ ] 所有 Service 都已註冊到 DI
- [ ] 所有商業邏輯都在 Service 層

### Views 檢查
- [ ] 所有 P0 Views 都已建立
- [ ] 所有 P1 Views 都已建立
- [ ] 所有 Views 都有對應的 ViewModel
- [ ] 所有 Views 都可正常運作

### 功能檢查
- [ ] P0-1: 錢包歷史詳細查詢 - 完整可用
- [ ] P0-2: 優惠券類型管理 - 完整 CRUD
- [ ] P0-3: 電子禮券類型管理 - 完整 CRUD
- [ ] P0-4: 簽到統計報表 - 完整圖表
- [ ] P1-1: 簽到規則設定 - 完整可用
- [ ] P1-2: 寵物系統規則設定 - 完整可用
- [ ] P1-3: 遊戲規則設定 - 完整可用
- [ ] P1-4: 互動獎勵規則管理 - 完整可用

---

## 進度追蹤

**Phase 1**: 0% (0/13 Services 完成)
**Phase 2**: 0% (0/11 Controllers 完成)
**Phase 3**: 0% (0/4 P0 功能完成)
**Phase 4**: 0% (0/4 P1 功能完成)

**總體進度**: 0% (0/32 項目完成)

---

**最後更新**: 2025-10-02 20:15
**執行者**: Claude Code
**預估剩餘時間**: 2.5 - 3 小時
