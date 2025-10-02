# GameSpace MiniGame Area 修復進度報告

**日期**: 2025-10-03
**當前狀態**: 編譯失敗（294 錯誤，5 警告）

---

## 已完成的修復（Critical 問題）

### ✅ Controllers 層修復（8/8）
1. ✅ AdminPetController - 已加入 GameSpacedatabaseContext 注入並調用 base(context)
2. ✅ AdminManagerController - 已移除重複的 _context 宣告，正確調用 base(context)
3. ✅ AdminMiniGameController - 已移除重複的 _context 宣告，正確調用 base(context)
4. ✅ AdminEVoucherController - 已移除重複的 _context 宣告，正確調用 base(context)
5. ✅ AdminMiniGameController - 已修正 DbSet 名稱從 `MiniGame` 到 `MiniGames`
6. ✅ AdminEVoucherController - 已修正 DbSet 名稱從 `EVoucher` 到 `Evouchers`
7. ✅ AdminCouponController - 已修正所有 DbSet 名稱（23處）：
   - `Coupon` → `Coupons`
   - `CouponType` → `CouponTypes`

### ✅ Services 層修復（4/4）
1. ✅ MiniGameAdminAuthService - 已加入 `: IMiniGameAdminAuthService` 介面實作
2. ✅ UserWalletService - 已修正從 `MiniGameDbContext` 到 `GameSpacedatabaseContext`
3. ✅ PetService - 已修正所有 DbSet 名稱從 `Pet` 到 `Pets`（7處）

---

## 剩餘的主要問題分類

### 🔴 Category 1: Razor Views 語法錯誤（約 200+ 個）

**問題類型**: RZ1031 - Tag helper 'option' must not have C# in the element's attribute declaration area

**影響檔案**:
- AdminCoupon/Index.cshtml (6 個錯誤)
- AdminCoupon/Index_temp.cshtml (7 個錯誤)
- AdminEVoucher/Index.cshtml (4+ 個錯誤)
- AdminHome/Index.cshtml (多個錯誤)
- AdminMiniGame/Index.cshtml (多個錯誤)
- AdminWallet/Index.cshtml (多個錯誤)
- 以及許多其他 Views

**錯誤範例**:
```cshtml
<!-- ❌ 錯誤 -->
<option value="@status">@status</option>

<!-- ✅ 正確 -->
<option value="@status">@status</option>
```

實際上這是 Razor 編譯器對混用 tag helpers 的限制。需要檢查每個 View 的 `<option>` 標籤。

---

### 🔴 Category 2: Controllers 未實作問題（約 30+ 個）

**主要錯誤**:
- `CS0535`: 類別未完整實作介面成員

**影響的 Services**:
1. **SignInStatsService** - 缺少 4 個介面方法：
   - `GetSignInStatsAsync(SignInStatsQueryModel)`
   - `GetSignInStatsSummaryAsync()`
   - `ConfigureSignInRulesAsync(SignInRulesModel)`
   - `GetSignInRulesAsync()`

2. **其他 Services** - 需要逐一檢查是否完整實作介面

---

### 🟠 Category 3: Views 命名空間錯誤（約 20+ 個）

**問題類型**: CS0234 - 命名空間中沒有類型或命名空間名稱

**錯誤範例**:
```cshtml
@* ❌ 錯誤 *@
@model GameSpace.Data.Wallet.UserWalletViewModel

@* ✅ 正確 *@
@model GameSpace.Areas.MiniGame.Models.ViewModels.UserWalletViewModel
```

**影響的 Views**:
- AdminWallet/UserWallet.cshtml - 引用 `GameSpace.Data.Wallet`（不存在）
- AdminManager/Delete.cshtml - 引用 `GameSpace.Data.Manager`（不存在）
- AdminManager/Details.cshtml - 引用 `GameSpace.Data.Manager`（不存在）
- AdminUser/Delete.cshtml - 引用 `GameSpace.Data.Users`（不存在）
- AdminUser/Details.cshtml - 引用 `GameSpace.Data.Users`（不存在）
- PetLevelUpRuleValidation/Index.cshtml - 引用 `GameSpace.Areas.MiniGame.Services.ValidationResult`（應在 Models）

---

### 🟠 Category 4: Controllers 方法錯誤（約 30+ 個）

**主要問題類型**:
1. **CS1503**: 引數類型不匹配
2. **CS0103**: 名稱在目前內容中不存在
3. **CS1061**: 類型未包含定義

**需要檢查的 Controllers**:
- AdminHomeController
- AdminWalletController
- AdminCouponController
- AdminEVoucherController
- AdminSignInController
- AdminManagerController
- AdminUserController
- 等等...

---

### 🟡 Category 5: ViewModel/Model 定義問題（約 10+ 個）

**問題**:
- ViewModels 未定義或定義在錯誤的位置
- Entity 和 ViewModel 混淆
- 缺少必要的屬性

---

## 修復優先順序與計劃

### 階段 1: 修復 Views 命名空間錯誤（預計 30 分鐘）
**優先度**: 🔴 Critical
**數量**: ~20 個檔案

**行動項目**:
1. ✅ 列出所有有命名空間錯誤的 Views
2. ⏳ 修正每個 View 的 `@model` 指令到正確的命名空間
3. ⏳ 確保引用的 ViewModel 確實存在

---

### 階段 2: 修復 Razor Views 語法錯誤（預計 2 小時）
**優先度**: 🔴 Critical
**數量**: ~200+ 個錯誤

**行動項目**:
1. ⏳ 找出所有 RZ1031 錯誤的位置
2. ⏳ 修正 `<option>` 標籤的語法
3. ⏳ 可能需要使用 `@Html.Raw()` 或改變結構

**解決方案選項**:
- Option A: 將 tag helpers 改為傳統 HTML（不使用 asp-* attributes）
- Option B: 使用 `@Html.DropDownListFor()` helper
- Option C: 重構為 Partial View 或 Component

---

### 階段 3: 完成 SignInStatsService 實作（預計 1 小時）
**優先度**: 🔴 Critical
**數量**: 4 個缺少的方法

**行動項目**:
1. ⏳ 檢查 ISignInStatsService 介面定義
2. ⏳ 實作 GetSignInStatsAsync(SignInStatsQueryModel)
3. ⏳ 實作 GetSignInStatsSummaryAsync()
4. ⏳ 實作 ConfigureSignInRulesAsync(SignInRulesModel)
5. ⏳ 實作 GetSignInRulesAsync()

---

### 階段 4: 修復 Controllers 方法錯誤（預計 3 小時）
**優先度**: 🟠 High
**數量**: ~30+ 個錯誤

**行動項目**:
1. ⏳ 逐一檢查每個 Controller 的編譯錯誤
2. ⏳ 修正參數類型不匹配
3. ⏳ 確保所有引用的變數和方法存在
4. ⏳ 修正類型不匹配的問題

---

### 階段 5: 驗證與測試（預計 1 小時）
**優先度**: 🟢 Normal

**行動項目**:
1. ⏳ 再次執行 `dotnet build`
2. ⏳ 確認 0 錯誤
3. ⏳ 執行基本功能測試
4. ⏳ 檢查登入和權限功能
5. ⏳ 測試每個主要頁面是否正常顯示

---

## 預計總時間

- **階段 1**: 30 分鐘
- **階段 2**: 2 小時
- **階段 3**: 1 小時
- **階段 4**: 3 小時
- **階段 5**: 1 小時

**總計**: 約 7.5 小時

---

## 當前進度

**已完成**: Critical 問題修復（Controllers 和 Services 依賴注入、DbSet 名稱）
**進行中**: Views 命名空間錯誤修復
**待完成**: Razor 語法錯誤、SignInStatsService 實作、Controllers 方法錯誤

**完成度**: 約 20%

---

## 建議

鑑於剩餘錯誤數量龐大，建議：

1. **立即處理**: Views 命名空間錯誤（影響最直接）
2. **接著處理**: SignInStatsService 介面實作（阻擋編譯）
3. **批量處理**: Razor Views 語法錯誤（可能需要工具輔助）
4. **逐步修復**: Controllers 方法錯誤（需要仔細檢查每個）

**下一步行動**: 開始修復 Views 命名空間錯誤，這些是最容易解決的問題。
