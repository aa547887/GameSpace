
# MiniGame Admin（先稽核、無誤不動）— 可續跑指令

### 一. 連線與讀取（最重要）

* 先讀取 `C:\Users\n2029\Desktop\GameSpace\GameSpace\schema\資料庫連線與讀取流程_詳細版.md` 並**連線到 SQL 資料庫**，讀取 **SSMS** 的**表、欄位、完整的種子資料**（包含 **minigame area** 和 **使用者權限 manager** 相關）。
* 專案內對應的 **schema 路徑**為：`GameSpace/GameSpace/schema/`，目前檔案（10 個）：

  1. `index.txt`
  2. `MiniGame_Area_完整描述文件.md`
  3. `MiniGame_area功能彙整.txt`
  4. `README_合併版.md`
  5. `專案規格敘述1.txt`
  6. `專案規格敘述2.txt`
  7. `管理者權限相關描述.txt`
  8. `資料庫連線與讀取流程.md`
  9. `資料庫連線與讀取流程_詳細版.md`
  10. `這裡有MinGame Area和管理者權限相關資料庫結構及種子資料.sql`

### 二. 樣板與導覽（細分權限版）

* **Admin 首頁與樣式為統一樣板（SB Admin）不可更動**。
* **Admin 後台採用 SB Admin 樣板（禁止修改 vendor 檔）**。
* **所有後台功能頁共用此樣板**；流程：
  **登入** → **共用首頁（左側有共用 sidebar）** → 點選 **「小遊戲管理系統」** 標籤按鈕 → **跳轉到 MiniGame area 首頁**（左側有 sidebar）。
* 進入 MiniGame Area 後，**需細分權限**：以 **Role／Claim／Policy** 控管**模組可見性**與**動作級存取**；左側 **sidebar 僅顯示被授權的第一層標籤**，展開後亦**僅顯示被授權的第二層按鈕**；**未授權的頁面與 API 必須回應 403（專案有全域共用 403 頁面）或導回登入/無權限頁**。
* **注意**：我們**只要做後台**，而且**只在 `Areas/MiniGame` 作業**（**不能逾越**）；`Program.cs` **只可以新增代碼（註冊器）**。

---

### 三. 先稽核、無誤不動（核心原則｜強制）

* 先**全面稽核現有**的 **View → Model → Controller → Service → Filter → Config** 是否同時滿足：

  1. **符合專案架構與慣例**；
  2. 與 **SSMS 實際資料庫**一致；
  3. 對應功能與資料欄位達到 **100% 覆蓋**。
* **稽核通過（無差異）**：不修改、不新建任何檔案。
* **稽核失敗（有差異／缺漏）**：僅做**最小必要修補或新建**，且**只允許**在 `Areas/MiniGame/` 與 `Program.cs` 新增必要註冊碼；嚴禁改動其他 Area 與 vendor。

### 四. 續跑與狀態檔（可重入｜強制）

* 狀態檔（每次貼上前先讀取、結束後必更新）：`Areas/MiniGame/docs/`

  * `WIP_RUN.md`：時間序記錄「開始／完成／失敗原因／回復方案」。
  * `PROGRESS.json`：以 `todo`、`doing`、`done`、`error` 標記各子任務狀態與附註。
  * `AUDIT_SSMS.md`：逐表記錄 SSMS 結構與種子資料核對結果。
  * `COVERAGE_MATRIX.json`：功能 ×（View／Model／Controller／Service／Filter／Config）× 資料表／欄位 之覆蓋矩陣（目標 100%）。
* **可重入原則**：檔案已存在且內容正確則**跳過**；不正確則**最小修正**；缺失才**補齊**。有阻塞先寫 `WIP_RUN.md` 的「未完成清單／下一步建議」，再繼續可獨立完成項目。

### 五. 稽核步驟（每次皆依序執行）

1. 依「一. 連線與讀取」內容，**連線 SSMS**（以 `appsettings.json` 連線字串為準），並逐表擷取：**Tables、Columns、型別、長度、NULL、DEFAULT、PK、FK、UQ、INDEX、CHECK**；抽樣核對 **種子資料**（含 MiniGame 與 Manager 權限相關表）。
2. 全面掃描 `Areas/MiniGame/` 既有檔案，歸檔分層為 **View／Model／Controller／Service／Filter／Config**，審查**檔名、命名空間、路徑、相依**是否符合 ASP.NET Core MVC 與專案慣例。
3. 建立／更新 `COVERAGE_MATRIX.json`：以**功能為列**，**六層構件與資料表／欄位為欄**，標示 `covered`／`missing`／`mismatch`，目標 **100% covered**。
4. 在 `AUDIT_SSMS.md` 條列**所有差異**（以 SSMS 為準）：欄位型別、長度、約束、索引、種子差異等。
5. 若 `COVERAGE_MATRIX.json` 全 `covered` 且 `AUDIT_SSMS.md` 無差異：於 `WIP_RUN.md` 記錄「本輪稽核通過，無變更」，**立即結束**。
6. 若有差異：於 `WIP_RUN.md` 寫入差異摘要與**最小修補計畫**，再進入第六節「變更規則」。

### 六. 變更規則（僅在稽核失敗時啟用）

* **範圍限制**：只允許 `Areas/MiniGame/` 調整與新增；`Program.cs` 僅可新增**必要註冊碼**。嚴禁修改其他 Area 與 **vendor**。
* **最小變更**：保留現有結構；不得大規模重構無關區塊；**只補缺與糾錯**。
* **真資料與真流程**：禁止佔位文字／假回傳；View 必須綁定實際後端資料；Controller Action 必須套用正確 **Policy**（`AdminCookie` + 對應 Policy）。
* **權限定義與樣板**：

  * 使用 **SB Admin** 樣式（`wwwroot/lib/sb-admin/`，**vendor 禁改**）。
  * Sidebar 依 **Role／Claim／Policy** 動態顯示第一層／第二層；未授權請求強制 **403（全域共用 403 頁）／導向無權限或登入**。
* **產出同步**：

  * 更新 `COVERAGE_MATRIX.json` 至 **100%**；
  * 更新 `AUDIT_SSMS.md`（將差異標記為「已修正／暫掛原因」）；
  * 更新 `PROGRESS.json` 與 `WIP_RUN.md`（含時間戳與本輪摘要）。

### 七. 驗收清單（每輪結束必檢）

* **結構對應**：六層構件完整，路由與檔案位置／命名空間正確。
* **資料一致**：Model 與 SSMS 欄位／型別／約束一致；必要索引存在；種子可查得且頁面能正確讀取。
* **功能可用**：CRUD、查詢、分頁、驗證、匯出皆可運作；嚴禁假資料。
* **權限就緒**：`auth_policies_defined`、`controller_authorize_applied`、`sidebar_guarded`、`views_authorized`、`unauthorized_behavior_verified(403/redirect)`、`manager_rules_verified` 全數通過。
* **覆蓋率**：`COVERAGE_MATRIX.json` 達 **100% covered**。
* **日誌完備**：`WIP_RUN.md`、`PROGRESS.json`、`AUDIT_SSMS.md` 均已更新。

### 八. 原則重申（強制）

* 我們**只實作後台**，且**只在 `Areas/MiniGame/` 作業**；`Program.cs` 僅新增**必要註冊碼**。
* **SB Admin vendor 禁止改**；外部共用程式只能引用、不可修改。
* **先完成 View → Model → Controller → Service → Filter → Config 全鏈**後，才處理編譯錯誤。
* 任何偏離與阻塞，先記錄於 `WIP_RUN.md`，再提出回復方案；不得無紀錄更動。

### 九. 貼上與執行規範（每次都一樣）

* **每次只貼上本指令全文**，不加入任何其他內容。
* 貼上後先讀取 `Areas/MiniGame/docs/` 狀態檔續跑：`WIP_RUN.md`、`PROGRESS.json`、`AUDIT_SSMS.md`、`COVERAGE_MATRIX.json`。
* 完成後務必更新上述四檔並在 `WIP_RUN.md` 留下本輪摘要；確保 `COVERAGE_MATRIX.json` 維持 **100%**。

### 十. 參照路徑（固定）

* 方案根：`C:\Users\n2029\Desktop\GameSpace\GameSpace\GameSpace\`
* MiniGame 區：`C:\Users\n2029\Desktop\GameSpace\GameSpace\GameSpace\Areas\MiniGame\`
* SB Admin vendor（**禁止修改**）：`C:\Users\n2029\Desktop\GameSpace\GameSpace\GameSpace\wwwroot\lib\sb-admin\`
* schema 目錄：`C:\Users\n2029\Desktop\GameSpace\GameSpace\schema\`（以〈一. 連線與讀取〉為唯一連線依據）

---

> 流程總結：**先連線讀取→全面稽核→無誤不動→有差才小補→全程可續跑與留痕**。
