# Restore corrupted view files from git history
$ErrorActionPreference = 'Stop'

# Files to restore from commit eece5b1
$files = @(
    "GameSpace/GameSpace/Areas/MiniGame/Views/AdminWallet/QueryHistory.cshtml",
    "GameSpace/GameSpace/Areas/MiniGame/Views/AdminWallet/Transaction.cshtml",
    "GameSpace/GameSpace/Areas/MiniGame/Views/AdminWallet/AdjustPoints.cshtml"
)

# Additional files from other commits
$moreFiles = @(
    "GameSpace/GameSpace/Areas/MiniGame/Views/AdminEVoucher/Create.cshtml",
    "GameSpace/GameSpace/Areas/MiniGame/Views/AdminEVoucher/CreateType.cshtml",
    "GameSpace/GameSpace/Areas/MiniGame/Views/AdminManager/Create.cshtml",
    "GameSpace/GameSpace/Areas/MiniGame/Views/AdminManager/CreateRole.cshtml",
    "GameSpace/GameSpace/Areas/MiniGame/Views/AdminManager/Edit.cshtml",
    "GameSpace/GameSpace/Areas/MiniGame/Views/AdminMiniGame/Create.cshtml",
    "GameSpace/GameSpace/Areas/MiniGame/Views/AdminUser/Create.cshtml",
    "GameSpace/GameSpace/Areas/MiniGame/Views/AdminUser/Edit.cshtml"
)

Write-Host "Restoring view files from git history..." -ForegroundColor Cyan

# Restore from commit eece5b1 (AdminWallet views)
Write-Host "`nRestoring AdminWallet views from commit eece5b1..." -ForegroundColor Yellow
foreach ($file in $files) {
    try {
        Write-Host "  Restoring: $file" -ForegroundColor White
        git checkout eece5b1 -- $file
        if ($LASTEXITCODE -eq 0) {
            Write-Host "    Success" -ForegroundColor Green
        } else {
            Write-Host "    Failed" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "    Error: $_" -ForegroundColor Red
    }
}

# Restore from commit ad68d95 (Other views)
Write-Host "`nRestoring other views from commit ad68d95..." -ForegroundColor Yellow
foreach ($file in $moreFiles) {
    try {
        Write-Host "  Restoring: $file" -ForegroundColor White
        git checkout ad68d95 -- $file
        if ($LASTEXITCODE -eq 0) {
            Write-Host "    Success" -ForegroundColor Green
        } else {
            Write-Host "    Failed" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "    Error: $_" -ForegroundColor Red
    }
}

Write-Host "`n========== Complete ==========" -ForegroundColor Cyan
Write-Host "Files restored from git history" -ForegroundColor Green
Write-Host "Please run 'dotnet build' to verify" -ForegroundColor Yellow
