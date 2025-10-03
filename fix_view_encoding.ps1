# 修復 MiniGame Area Views 的中文編碼問題
# 問題：檔案以錯誤的編碼儲存，導致 Razor 編譯器無法正確解析中文字元
# 解決方案：先嘗試用 Big5 (cp950) 讀取，再以 UTF-8 BOM 儲存

$ErrorActionPreference = 'Continue'
$viewsPath = "GameSpace\GameSpace\Areas\MiniGame\Views"

# 列出所有需要修復的檔案（根據編譯錯誤）
$filesToFix = @(
    "$viewsPath\AdminEVoucher\Create.cshtml",
    "$viewsPath\AdminEVoucher\CreateType.cshtml",
    "$viewsPath\AdminManager\Create.cshtml",
    "$viewsPath\AdminManager\CreateRole.cshtml",
    "$viewsPath\AdminManager\Edit.cshtml",
    "$viewsPath\AdminMiniGame\Create.cshtml",
    "$viewsPath\AdminUser\Create.cshtml",
    "$viewsPath\AdminUser\Edit.cshtml",
    "$viewsPath\AdminWallet\AdjustPoints.cshtml",
    "$viewsPath\AdminWallet\QueryHistory.cshtml",
    "$viewsPath\AdminWallet\Transaction.cshtml"
)

$successCount = 0
$failCount = 0

foreach ($file in $filesToFix) {
    $fullPath = Join-Path $PSScriptRoot $file

    if (-not (Test-Path $fullPath)) {
        Write-Host "❌ 檔案不存在: $file" -ForegroundColor Red
        $failCount++
        continue
    }

    try {
        Write-Host "🔄 處理: $file" -ForegroundColor Cyan

        # 嘗試用 CP950 (Big5) 讀取
        $content = Get-Content -Path $fullPath -Raw -Encoding ([System.Text.Encoding]::GetEncoding(950))

        # 檢查是否成功讀取（如果還有亂碼可能需要用其他編碼）
        if ($content -match '[\u0000-\u001F\uFFFD]' -and $content -notmatch '[\u4E00-\u9FFF]') {
            Write-Host "  ⚠️  CP950 讀取失敗，嘗試 UTF-8..." -ForegroundColor Yellow
            $content = Get-Content -Path $fullPath -Raw -Encoding UTF8
        }

        # 儲存為 UTF-8 BOM
        $utf8BOM = New-Object System.Text.UTF8Encoding $true
        [System.IO.File]::WriteAllText($fullPath, $content, $utf8BOM)

        Write-Host "  ✅ 成功轉換為 UTF-8 BOM" -ForegroundColor Green
        $successCount++
    }
    catch {
        Write-Host "  ❌ 轉換失敗: $_" -ForegroundColor Red
        $failCount++
    }
}

Write-Host "`n========== 修復完成 ==========" -ForegroundColor Cyan
Write-Host "✅ 成功: $successCount 個檔案" -ForegroundColor Green
Write-Host "❌ 失敗: $failCount 個檔案" -ForegroundColor Red
