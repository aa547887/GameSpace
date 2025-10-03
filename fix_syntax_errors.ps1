# Fix Syntax Errors from Previous Script
$BasePath = "C:\Users\n2029\Desktop\GameSpace\GameSpace\GameSpace\Areas\MiniGame"

Write-Host "Fixing syntax errors..." -ForegroundColor Green

# Fix SignInService.cs - Remove broken comment syntax
$file = Join-Path $BasePath "Services\SignInService.cs"
$content = Get-Content $file -Raw -Encoding UTF8
# Fix the broken field assignments
$content = $content -replace 's\.// Field mapping needed //=', 's.UserId ='
$content = $content -replace '// Use SignTime //', 'SignTime'
$content = $content -replace '// Already exists //', 'PointsGained'
$content = $content -replace '// Field mapping needed //', 'UserId'
[System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($true))

# Fix AdminWalletController.cs - Remove broken comment syntax
$file = Join-Path $BasePath "Controllers\AdminWalletController.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw -Encoding UTF8
    $content = $content -replace '// UserName //', 'UserName'
    $content = $content -replace '// CurrentPoints //', 'CurrentPoints'
    $content = $content -replace '// Use ChangeType //', 'TransactionType'
    $content = $content -replace '// Use PointsChanged //', 'TransactionAmount'
    [System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($true))
}

# Fix MiniGameBaseController.cs - Remove broken comment syntax
$file = Join-Path $BasePath "Controllers\MiniGameBaseController.cs"
$content = Get-Content $file -Raw -Encoding UTF8
$content = $content -replace '// TargetUserId property does not exist //', '// TargetUserId = targetUserId'
[System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($true))

# Fix CouponTypeService.cs - Remove the remaining IsActive reference
$file = Join-Path $BasePath "Services\CouponTypeService.cs"
$content = Get-Content $file -Raw -Encoding UTF8
$content = $content -replace 'couponType\.IsActive = false;', '// IsActive property does not exist'
[System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($true))

Write-Host "Syntax errors fixed!" -ForegroundColor Green
