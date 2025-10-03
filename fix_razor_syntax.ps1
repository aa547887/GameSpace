# Fix Razor syntax errors by removing invalid characters
$ErrorActionPreference = 'Continue'
$basePath = "GameSpace\GameSpace\Areas\MiniGame\Views"

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

Write-Host "Fixing Razor syntax errors..." -ForegroundColor Cyan

foreach ($file in $files) {
    $fullPath = Join-Path $PSScriptRoot (Join-Path $basePath $file)

    if (-not (Test-Path $fullPath)) {
        Write-Host "File not found: $file" -ForegroundColor Red
        $failCount++
        continue
    }

    try {
        Write-Host "Processing: $file" -ForegroundColor Yellow

        # Read file as raw bytes
        $bytes = [System.IO.File]::ReadAllBytes($fullPath)

        # Convert from Big5 to UTF-8
        $big5 = [System.Text.Encoding]::GetEncoding(950)
        $content = $big5.GetString($bytes)

        # Count Chinese characters
        $chineseChars = ($content | Select-String -Pattern '[\u4e00-\u9fa5]' -AllMatches).Matches.Count

        if ($chineseChars -gt 10) {
            Write-Host "  Detected $chineseChars Chinese characters - encoding looks correct" -ForegroundColor Green

            # Save with UTF-8 BOM
            $utf8BOM = New-Object System.Text.UTF8Encoding($true)
            [System.IO.File]::WriteAllText($fullPath, $content, $utf8BOM)

            Write-Host "  Successfully converted to UTF-8 BOM" -ForegroundColor Green
            $successCount++
        } else {
            Write-Host "  Warning: Only $chineseChars Chinese characters detected" -ForegroundColor Yellow
            $failCount++
        }
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
