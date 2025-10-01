MiniGame Area（Admin 後台）實作說明（作文式、強制條款）

本組目前 只專心開發 Admin 後台（含 Admin 前端樣式與 Admin 後端 API）。我被分配到 Areas/MiniGame 的後端，因此所有開發工作 僅能在 Areas/MiniGame/ 底下 進行。嚴格禁止越界修改其他 Area 或全域檔案；唯一例外為 Program.cs，該檔可僅新增必要註冊碼以註冊 MiniGame Area（不得動他人註冊與設定）。Admin 首頁與樣式為統一樣板（SB Admin）不可更動；由首頁點選 MiniGame 標籤 → 展開 MiniGame 旗下的各模組子標籤，進入各後台功能頁。

UI 樣式參考：

Public（前台）：遵照 index.txt（Bootstrap-based）。

Admin（後台）：SB Admin（wwwroot/lib/sb-admin/ 供應；禁止修改 vendor 檔）。

切記一定全部功能都要實作!!!!!! 不可用佔位文字敷衍!!!!!! 切記一定全部功能都要實作!!!!!! 不可用佔位文字敷衍!!!!!! 切記一定全部功能都要實作!!!!!! 不可用佔位文字敷衍!!!!!!

現在請閱讀 schema 下方所有資料，然後配合你實際連線並讀取 SSMS 的表、欄位、種子資料（包含 minigame area 和 使用者權限 manager 相關）（包含 minigame area 和 使用者權限 manager相關）（包含 minigame area 和 使用者權限 manager相關）（包含 minigame area 和 使用者權限 manager相關）（包含 minigame area 和 使用者權限 manager相關）（包含 minigame area 和 使用者權限 manager相關），去完整實作 Areas/MiniGame 的所有功能。

實作順序與要求如下（務必照此順序）：

View（配合全域樣式） →

先完成 Admin 後台頁面（必須使用 SB Admin 樣式，不得變更 vendor 檔）。

View 必須依 schema 中的畫面與欄位要求呈現，並能串接真實後端資料（不可使用假資料或占位文字）。

Model（完全對應 SSMS） →

依據你在 SSMS 中實際讀取到的 表（Tables）、欄位（Columns）、資料型別、長度、NOT NULL、DEFAULT、PRIMARY/FOREIGN/UNIQUE/INDEX、CHECK 等約束，建立相對應的 Model。

特別包含 MiniGame Area 必須的表（例：User_Wallet、UserSignInStats、Pet、MiniGame、WalletHistory、Coupon、EVoucher 等）及使用者權限（Manager）相關表。

Controller（實作完整後端 API 與商業邏輯） →

Controller 必須實作所有 schema 要求的功能（CRUD、查詢、分頁、驗證、匯出等），不能只做介面或回傳假資料。

Controller 中的權限檢查需參考 Identity Area 與專案其他資料夾實作方式，對接使用者登入與權限判定（不可自行亂改全域認證流程，只可參照並於 Areas/MiniGame 範圍內實作）。

其他（Service / Filter / Config） →

Service 層封裝商業邏輯，Filter 層處理權限與輸入驗證，Config 參考 appsettings.json（ssms 連線請參考 appsettings.json）以取得資料庫連線字串。

若需引用外部資源（library、共用 service），只能引用，不得修改或編輯其他 Area 的檔案。

重要且強制的資料驗證步驟（必做）：

務必實際連線並讀取 SSMS 的每一張相關表（尤其是 MiniGame Area 與 Manager 權限相關表），逐表確認欄位名稱、型別、長度、允許 NULL 與否、預設值、PK/FK/UQ/INDEX、CHECK 等約束。

務必實際讀取種子資料（Seed Data）：使用 SELECT 等查詢確認每張表的 seed 是否完整、內容是否合理，尤其是會員錢包、簽到、寵物、小遊戲以及管理者權限設定。

任何未以 SSMS 真實讀取結果為依據之實作，均視為不合格。

其他限定事項（不可違反）：

你只能在 Areas/MiniGame 資料夾底下實作；可以引用專案內的 Identity area 與其他資料夾以了解登入與權限控制方式，但不可跨到別的區域編輯或更動他處程式碼（除了 Program.cs 的註冊碼例外）。

所有後台功能必須真正可用（不可用 placeholder 或假回傳），並必須對應 schema 底下所有文件的描述以及你從 SSMS 讀到的表、欄位與種子資料。

開發過程請將 appsettings.json 視為 SSMS 連線的唯一依據（ssms 連線請參考 appsettings.json）。

最後再次重申（以示強制性）：只需要做 MiniGame Area 的後台功能就好!!!!，本組僅專心 Admin 後台；所有工作範圍與限制（只能在 Areas/MiniGame/ 作業、不得修改 vendor / 他人 Area、Program.cs 可新增註冊碼但不得改他人設定）請嚴格遵守。切記一定全部功能都要實作!!!!!! 不可用佔位文字敷衍!!!!!!               你應該先別管編譯錯誤 應該先建立正確的minigame area view -->  model --> controller ->service -->filter -->config 等  全部建置好後 再來處理編譯錯誤                  請用powershell終端寫檔                                  你應該先別管編譯錯誤 應該先建立正確的minigame area view -->  model --> controller ->service -->filter -->config 等  全部建置好後 再來處理編譯錯誤                  請用powershell終端寫檔                                