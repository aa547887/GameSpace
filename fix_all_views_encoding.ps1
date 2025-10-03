# 強制修復所有 MiniGame Views 的編碼問題
# 使用 Big5 讀取並以 UTF-8 BOM 儲存

$ErrorActionPreference = 'Continue'
$basePath = "GameSpace\GameSpace\Areas\MiniGame\Views"

# 所有有問題的檔案
$files = @(
    "AdminEVoucher\Create.cshtml",
    "AdminEVoucher\CreateType.cshtml",
    "AdminManager\Create.cshtml",
    "AdminManager\CreateRole.cshtml",
    "AdminManager\Edit.cshtml",
    "AdminMiniGame\Create.cshtml",
    "AdminUser\Create.cshtml",
    "AdminUser\Edit.cshtml",
    "AdminWallet\AdjustPoints.cshtml",
    "AdminWallet\QueryHistory.cshtml",
    "AdminWallet\Transaction.cshtml"
)

$successCount = 0
$failCount = 0

Write-Host "開始修復 View 編碼問題..." -ForegroundColor Cyan

foreach ($file in $files) {
    $fullPath = Join-Path $PSScriptRoot (Join-Path $basePath $file)

    if (-not (Test-Path $fullPath)) {
        Write-Host "❌ 檔案不存在: $file" -ForegroundColor Red
        $failCount++
        continue
    }

    try {
        Write-Host "處理: $file" -ForegroundColor Yellow

        # 先嘗試用 Big5 (CP950) 讀取
        try {
            $encoding = [System.Text.Encoding]::GetEncoding(950)
            $content = [System.IO.File]::ReadAllText($fullPath, $encoding)
            Write-Host "  使用 Big5 編碼讀取成功" -ForegroundColor Green
        }
        catch {
            # 如果 Big5 失敗，嘗試 UTF-8
            Write-Host "  Big5 失敗，嘗試 UTF-8..." -ForegroundColor Yellow
            $content = [System.IO.File]::ReadAllText($fullPath, [System.Text.Encoding]::UTF8)
        }

        # 以 UTF-8 BOM 儲存
        $utf8BOM = New-Object System.Text.UTF8Encoding($true)
        [System.IO.File]::WriteAllText($fullPath, $content, $utf8BOM)

        Write-Host "  ✓ 成功轉換為 UTF-8 BOM" -ForegroundColor Green
        $successCount++
    }
    catch {
        Write-Host "  ✗ 轉換失敗: $_" -ForegroundColor Red
        $failCount++
    }
}

Write-Host "`n========== 完成 ==========" -ForegroundColor Cyan
Write-Host "成功: $successCount 個檔案" -ForegroundColor Green
Write-Host "失敗: $failCount 個檔案" -ForegroundColor $(if ($failCount -eq 0) { "Green" } else { "Red" })
