### 一. 前置章（最重要）
**連線與讀取**：  
- 先讀取 `C:\Users\n2029\Desktop\GameSpace\GameSpace\schema\資料庫連線與讀取流程_詳細版.md` 並**連線到 SQL 資料庫**，讀取 **SSMS** 的**表、欄位、完整的種子資料**（包含 **minigame area** 和 **使用者權限 manager** 相關）。  
- 專案內對應的 **schema 路徑**為：`GameSpace/GameSpace/schema/`，目前檔案（10 個）：  
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
- **Admin 首頁與樣式為統一樣板（SB Admin）不可更動**。  
- **Admin 後台採用 SB Admin 樣板（禁止修改 vendor 檔）**。  
- **所有後台功能頁共用此樣板**；流程：  
  **登入** → **共用首頁（左側有共用 sidebar）** → 點選 **「小遊戲管理系統」** 標籤按鈕 → **跳轉到 MiniGame area 首頁**（左側有 sidebar）。  
- 進入 MiniGame Area 後，**需細分權限**：以 **Role／Claim／Policy** 控管**模組可見性**與**動作級存取**；左側 **sidebar 僅顯示被授權的第一層標籤**，展開後亦**僅顯示被授權的第二層按鈕**；**未授權的頁面與 API 必須回應 403（專案有全域共用 403 頁面）或導回登入/無權限頁**。  
- **注意**：我們**只要做後台**，而且**只在 `Areas/MiniGame` 作業**（**不能逾越**）；`Program.cs` **只可以新增代碼（註冊器）**。

### 三. MiniGame Area 後台（Admin）樣板與功能規範（細分權限版）
#### A. 樣板與登入流程（強制）
- **Admin 首頁與樣式為統一樣板（SB Admin）不可更動。**  
- **Admin 後台採用 SB Admin 樣板，禁止修改 vendor 檔。**  
- **所有後台功能頁共用此樣板**：  
  **登入** → **共用首頁（左側為共用 sidebar）** → 點選「**小遊戲管理系統**」標籤 → **跳轉至 MiniGame Area 首頁**（左側仍有 sidebar）。  
- 進入 MiniGame Area 後，**需細分權限**：依 **Role／Claim／Policy** 控制**模組可見性**與**動作級存取**；**未授權功能不顯示**，未授權請求回應 **403（我們的專案有全域共用的 403 頁面）／導回登入或無權限頁**。  
- 左側 sidebar 的**第一層／第二層按鈕**，**僅對具備授權者顯示**；每個對應頁之 **Action／View／Model／Controller（或 Server／Filter／Config）** 皆需受 **Policy** 保護。  
- **本期僅實作後台**；僅允許在 **`Areas/MiniGame/`** 內作業（不得逾越）。  
- `Program.cs` **只可新增必要註冊碼**以註冊 MiniGame Area，**不得變更**他人既有註冊與設定。

#### B. 功能最少顯示清單（僅後台 Admin）
**1) 會員錢包系統（Admin）**  
- 查詢會員點數  
- 查詢會員擁有商城優惠券  
- 查詢會員擁有電子禮券  
- 發放會員點數  
- 發放會員之商城優惠券（含發放流程）  
- 調整會員之電子禮券（含發放／增減）  
- 查看會員收支明細（含**會員點數得到／花費**；**商城優惠券得到／使用**；**電子優惠券得到／使用**之**時間／點數或張數／種類**等欄位）

**2) 會員簽到系統（Admin）**  
- 簽到規則設定（如獎勵規則、頻率、條件等）  
- 查看會員簽到紀錄（含簽到時間、獎勵內容等）

**3) 寵物系統（Admin）**  
- 整體寵物系統規則設定  
  - 升級規則  
  - 互動狀態增益規則  
  - 寵物顏色選項  
  - **寵物換色所需點數**  
  - 寵物背景選項  
  - **寵物換背景所需點數**  
- 會員個別寵物設定（手動調整）  
  - 寵物名／膚色／背景  
- 會員個別寵物清單與查詢  
  - 基本資料：**寵物名、目前膚色、目前背景、目前經驗值、目前等級、目前五大狀態值**  
  - **換膚色紀錄查詢**  
  - **換背景紀錄查詢**

**4) 小遊戲系統（Admin）**  
- 遊戲規則設定  
  - 獎勵種類（**會員點數／寵物經驗值／商城優惠券**）  
  - **每日遊戲次數限制（預設一天三次）**  
- 查看會員遊戲紀錄  
  - 時間（`startTime`、`endTime`）  
  - 結果（`win／lose／abort`）  
  - **獲得獎勵**（會員點數／寵物經驗值／商城優惠券）

---

## 一、執行總則（強制）
- **不用建立任何目錄**，目錄已既有存在。  
- 僅在專案範圍內操作：`Areas/MiniGame` 及 `Program.cs` 的**必要註冊碼**；**禁止**修改其他 Area 與 **vendor 檔**。  
- **嚴格對照** schema 與 **實際 SSMS 結構與種子資料**實作；**禁止**使用**假資料或佔位文字**。  
- **實作順序（必遵守）**：  
  **View → Model → Controller → Service → Filter → Config**；**全部建置完成後**才允許處理**編譯錯誤**。  
- 任何外部資源僅能**引用**，不得修改他處程式碼。  
- 產出必須**可用、可測試、可驗證**，並與 **SSMS 真實結果一致**。

## 二、可續跑與斷點規則（強制）
建立並維護下列工作狀態檔與日誌；**每次貼上本指令時，先讀取後續跑**：
- `Areas/MiniGame/docs/WIP_RUN.md`：以時間序紀錄每一步**開始／完成／失敗與原因**。  
- `Areas/MiniGame/docs/PROGRESS.json`：以**狀態機**格式（`todo`、`doing`、`done`、`error`）紀錄各子任務狀態與必要附註。  
- `Areas/MiniGame/docs/AUDIT_SSMS.md`：記錄自 SSMS 讀到之 **Tables／Columns／型別／長度／NOT NULL／DEFAULT／PK／FK／UQ／INDEX／CHECK** 與**種子資料核對結果（逐表）**。

其他要求：
- **可重入**：檔案**已存在且內容正確則跳過**；不正確則**最小修正**；缺失則**補齊**。  
- 若任一步驟發生阻塞：寫入 **WIP_RUN.md** 的「**未完成清單**」與「**下一步建議**」，並**繼續處理可獨立完成**之項目。

## 三、資料來源與驗證（強制）
- **逐表讀取 SSMS**：完整列出 **Tables、Columns、型別、長度、NOT NULL、DEFAULT、PK、FK、UQ、INDEX、CHECK**，並**比對** schema 文件與**實際種子資料**。  
- **所有差異**需在 **AUDIT_SSMS.md** 明確標示，並以「**以 SSMS 實際為準**」原則修正 **Model** 與後端實作。

## 四、產出與結構（強制）
- **View**：使用 **SB Admin** 樣式（vendor 路徑：`wwwroot/lib/sb-admin/`）；**不得更動 vendor**；依 **schema 畫面與欄位**要求呈現；**必須綁定真實後端資料**。  
- **Model**：**完全對應 SSMS 結構與約束**。  
- **Controller**：實作 **CRUD、查詢、分頁、驗證、匯出、權限檢查**；**不得假回傳**；所有 Action **必須套用對應 Policy**（例如 `[Authorize(AuthenticationSchemes="AdminCookie", Policy="...")]` 或等效）。  
- **Service**：**封裝商業邏輯**，與 Controller **解耦**。  
- **Filter**：**權限**與**輸入驗證**；採 **Policy-based Authorization**（以 **Role／Claim** 組合成 **Policy**），並涵蓋**未授權處置**（403／redirect）。  
- **Config**：依 `appsettings.json` 取得**資料庫連線字串**；僅於 `Areas/MiniGame` 範圍內使用。  
- **View／Sidebar 顯示規則（權限）**：全域與 MiniGame 的 `_Sidebar.cshtml` 均須依使用者 **Role／Claim／Policy** 動態**隱藏未授權功能**（第一層/第二層按鈕與頁面）；未授權請求**強制**回應 **403（全域共用 403 頁）或導至無權限頁／登入**。

## 五、進度與驗收（強制）
- 每完成一子模組，更新 **PROGRESS.json** 與 **WIP_RUN.md**，並附上**核對點**：  
  **表結構、索引、約束、種子、一頁列表、詳情、建立、編輯、刪除、分頁、驗證、匯出、權限**。  
- **授權驗收項**：  
  **auth_policies_defined、controller_authorize_applied、sidebar_guarded、views_authorized、unauthorized_behavior_verified（403/redirect）、manager_rules_verified**。  
- 完成全部功能後，提供**最終核對清單**與**差異摘要（以 SSMS 為準）**，確保**零佔位、零假資料、零漏項**。

## 六、原始需求全文（逐字保留，不得增刪改動）

**開發範圍**：  
- 本組目前**只專心開發 Admin 後台**（含 Admin 前端樣式與 Admin 後端 API）。我被分配到 `Areas/MiniGame` 的後端，因此所有開發工作**僅能在 `Areas/MiniGame/` 底下進行**。  
- **嚴格禁止越界**修改其他 Area 或全域檔案；唯一例外為 `Program.cs`，該檔可**僅新增必要註冊碼**以註冊 MiniGame Area（**不得動他人註冊與設定**）。  
- **Admin 首頁與樣式為統一樣板（SB Admin）不可更動**；由首頁點選 **MiniGame** 標籤 → 展開 MiniGame 旗下的各模組子標籤，進入各後台功能頁。  
  - **SB Admin vendor 路徑**：`GameSpace/GameSpace/GameSpace/wwwroot/lib/sb-admin/`；嚴禁修改 vendor 檔案。

**強制實作要求**：  
- **切記一定全部功能都要實作、不可用佔位文字敷衍**（重申多次）。  
- 現在請**仔細閱讀** `schema/` 10 份文件（**line by line**），並配合**實際連線 SSMS**（包含 **minigame area** 與 **使用者權限 manager** 相關）去**完整實作 `Areas/MiniGame` 的所有功能**。

**MiniGame area 登入與權限對接（依實作校正）**：  
- 登入流程由 **Identity** 區域處理；MiniGame 後台採用 **AdminCookie**。  
- MiniGame 控制器（例如：`Areas/MiniGame/Controllers/AdminHomeController.cs`、`MiniGameBaseController*.cs`）採用 `[Area("MiniGame")]` 並以 **Policy** 防護（如 `Policy="AdminOnly"`）。  
- **側邊欄導航**：全域與 MiniGame 皆有 `_Sidebar.cshtml`，實作中可見依 Claims/DB 決定顯示與否的權限邏輯。

**樣板與導覽（細分權限版｜重申）**：  
- **Admin 首頁與樣式為統一樣板（SB Admin）不可更動**。  
- **Admin 後台採用 SB Admin 樣板（禁止修改 vendor 檔）**。  
- **所有後台功能頁共用此樣板**；流程：  
  **登入** → **共用首頁（左側有共用 sidebar）** → 點選 **「小遊戲管理系統」** 標籤按鈕 → **跳轉到 MiniGame area 首頁**（左側有 sidebar）。  
- 進入 MiniGame Area 後，**需細分權限**：以 **Role／Claim／Policy** 控管**模組可見性**與**動作級存取**；左側 **sidebar 僅顯示被授權的第一層標籤**，展開後亦**僅顯示被授權的第二層按鈕**；**未授權的頁面與 API 必須回應 403（全域共用 403 頁）或導回登入/無權限頁**。  
- **注意**：我們**只要做後台**，而且**只在 `Areas/MiniGame` 作業**（**不能逾越**）；`Program.cs` **只可以新增代碼（註冊器）**。

**實作順序與要求（務必照此順序）**：  
- **View（配合全域樣式） → Model（完全對應 SSMS） → Controller（完整後端 API 與商業邏輯） → 其他（Service／Filter／Config）**。  
  - **View**：先完成 **Admin 後台頁面**（**必用 SB Admin 樣式**，**不得變更 vendor**）；View 必須依 **schema 畫面與欄位**要求呈現，並能**串接真實後端資料**（**不可假資料／占位文字**）。  
  - **Model**：依據自 SSMS 實際讀到之 **Tables／Columns／資料型別／長度／NOT NULL／DEFAULT／PRIMARY／FOREIGN／UNIQUE／INDEX／CHECK** 等**建立相對應的 Model**。特別包含 **MiniGame Area 必要表**（例：`User_Wallet`、`UserSignInStats`、`Pet`、`MiniGame`、`WalletHistory`、`Coupon`、`EVoucher` 等）及**使用者權限（Manager）相關表**。  
  - **Controller**：必須實作**全部 schema 要求功能**（**CRUD、查詢、分頁、驗證、匯出**等），不得只做介面或回傳假資料；**權限檢查**需參考 **Identity Area** 與專案其他資料夾作法，**對接使用者登入與權限判定**（**不可自行亂改**全域認證流程；只可於 `Areas/MiniGame` 範圍內實作）；**Action 層強制套用 Policy**。  
  - **其他（Service／Filter／Config）**：**Service** 封裝商業邏輯；**Filter** 負責權限與輸入驗證（**Policy-based Authorization**、403/redirect）；**Config** 參考 `appsettings.json`（**SSMS 連線請參考 `appsettings.json`**）取得資料庫連線字串。  
  - 若需引用外部資源（library、共用 service），**只能引用**，不得修改或編輯其他 Area 檔案。

**重要且強制的資料驗證步驟（必做）**：  
1) 務必**實際連線並讀取 SSMS** 的每一張相關表（尤其 **MiniGame Area** 與 **Manager 權限**相關表），逐表確認：**欄位名稱、型別、長度、允許 NULL 與否、預設值、PK／FK／UQ／INDEX、CHECK** 等約束。  
2) 務必**實際讀取種子資料（Seed Data）**：用 `SELECT` 等查詢確認每張表的 seed 是否完整、內容是否合理（**會員錢包、簽到、寵物、小遊戲**以及**管理者權限設定**）。  
3) 任何**未以 SSMS 真實讀取結果為依據**之實作，均視為**不合格**。

**其他限定事項（不可違反）**：  
- 你**只能在 `Areas/MiniGame`** 內實作；可引用專案內 **Identity Area** 與其他資料夾**了解**登入與權限控制方式，但**不可跨區域編輯或更動**他處程式碼（**除了 `Program.cs` 的註冊碼例外**）。  
- **所有後台功能必須真正可用**（不可 placeholder 或假回傳），並必須**對應 schema** 底下所有文件描述，以及 **SSMS** 的表、欄位與種子資料。  
- 開發過程請將 `C:\Users\n2029\Desktop\GameSpace\GameSpace\schema\` 視為 **SSMS 連線的唯一依據**（**SSMS 連線請參考 `appsettings.json`**）。

**重申（強制）與操作提示**：  
- **只需要做 MiniGame Area 的後台功能。**  
- 嚴格遵守：**只能在 `Areas/MiniGame/` 作業、不得修改 vendor／他人 Area、`Program.cs` 只可新增註冊碼不得改他人設定**。  
- **切記一定全部功能都要實作、不可用佔位文字敷衍。**  
- **先別管編譯錯誤**，請先建立正確的 **minigame area：View → Model → Controller → Service → Filter → Config**，**全部建置好後**再處理編譯錯誤。  
- **請用 PowerShell 終端寫檔。**（以上兩句強調語為原文重複，特此完整保留）

## 七、貼上與執行規範（強制）
- **每次只貼上本指令全文**，不加入任何其他內容。  
- 每次貼上前，Cursor 需**先讀取** `WIP_RUN.md` 與 `PROGRESS.json` 以續跑；**完成後更新**兩檔。  
- **不得產生**與本指令無關的修改與輸出；任何偏離需在 **WIP_RUN.md** **記錄原因與回復方案**。