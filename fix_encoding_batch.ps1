# Fix encoding for all problematic cshtml files
$ErrorActionPreference = 'Continue'
$basePath = "GameSpace\GameSpace\Areas\MiniGame\Views"

# All problematic files from build output
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

Write-Host "Starting encoding fix..." -ForegroundColor Cyan

foreach ($file in $files) {
    $fullPath = Join-Path $PSScriptRoot (Join-Path $basePath $file)

    if (-not (Test-Path $fullPath)) {
        Write-Host "File not found: $file" -ForegroundColor Red
        $failCount++
        continue
    }

    try {
        Write-Host "Processing: $file" -ForegroundColor Yellow

        # Try reading with Big5 (CP950) encoding first
        try {
            $encoding = [System.Text.Encoding]::GetEncoding(950)
            $content = [System.IO.File]::ReadAllText($fullPath, $encoding)
            Write-Host "  Read with Big5 encoding" -ForegroundColor Green
        }
        catch {
            # If Big5 fails, try UTF-8
            Write-Host "  Big5 failed, trying UTF-8..." -ForegroundColor Yellow
            $content = [System.IO.File]::ReadAllText($fullPath, [System.Text.Encoding]::UTF8)
        }

        # Save with UTF-8 BOM
        $utf8BOM = New-Object System.Text.UTF8Encoding($true)
        [System.IO.File]::WriteAllText($fullPath, $content, $utf8BOM)

        Write-Host "  Successfully converted to UTF-8 BOM" -ForegroundColor Green
        $successCount++
    }
    catch {
        Write-Host "  Conversion failed: $_" -ForegroundColor Red
        $failCount++
    }
}

Write-Host "`n========== Complete ==========" -ForegroundColor Cyan
Write-Host "Success: $successCount files" -ForegroundColor Green
if ($failCount -eq 0) {
    Write-Host "Failed: $failCount files" -ForegroundColor Green
} else {
    Write-Host "Failed: $failCount files" -ForegroundColor Red
}
