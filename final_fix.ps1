# Final Comprehensive Fix for All Syntax Errors
$BasePath = "C:\Users\n2029\Desktop\GameSpace\GameSpace\GameSpace\Areas\MiniGame"

Write-Host "Applying final comprehensive fixes..." -ForegroundColor Green

# Fix SignInService.cs - Restore proper syntax
$file = Join-Path $BasePath "Services\SignInService.cs"
$content = Get-Content $file -Raw -Encoding UTF8

# Fix all the broken field assignments (missing = operators)
$content = $content -replace 'UserId userId,', 'UserId = userId,'
$content = $content -replace 'SignTime DateTime\.UtcNow,', 'SignTime = DateTime.UtcNow,'
$content = $content -replace 'PointsGained reward\.Points,', 'PointsGained = reward.Points,'
$content = $content -replace 'PointsGained reward\.Experience,', 'ExpGained = reward.Experience,'
$content = $content -replace 'PointsGained reward\.CouponCode', 'CouponGained = reward.CouponCode.ToString()'
$content = $content -replace 'UserId = userId', 'UserId == userId'
$content = $content -replace 'w\.UserId= userId', 'w.UserId == userId'
$content = $content -replace 'p\.UserId= userId', 'p.UserId == userId'
$content = $content -replace 'IssueTime =', 'AcquiredTime ='
$content = $content -replace 'ExpiryTime =', '// ExpiryTime property does not exist //'
$content = $content -replace 'ConsecutiveDays = consecutiveDays', '// ConsecutiveDays property does not exist'
$content = $content -replace 'ct\.IsActive', '(ct.ValidFrom <= DateTime.UtcNow && ct.ValidTo >= DateTime.UtcNow)'
$content = $content -replace 'UserId g\.Key', 'UserId = g.Key'
$content = $content -replace 'PointsGained\)', 'ConsecutiveDays)'

[System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($true))

# Fix AdminWalletController.cs - Restore proper syntax
$file = Join-Path $BasePath "Controllers\AdminWalletController.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw -Encoding UTF8
    $content = $content -replace 'TransactionType h\.PointsChanged', 'TransactionType = h.PointsChanged'
    $content = $content -replace 'TransactionType history\.PointsChanged', 'TransactionType = history.PointsChanged'
    $content = $content -replace 'UserName user\.User_Name', 'UserName = user.User_Name'
    $content = $content -replace 'CurrentPoints summary', 'CurrentPoints = summary'
    $content = $content -replace 'TransactionType= "earn"', 'TransactionType == "earn"'
    $content = $content -replace 'ViewBag\.TransactionType changeType', 'ViewBag.TransactionType = changeType'
    $content = $content -replace 'h\.HistoryID', 'h.LogId'
    $content = $content -replace 'h\.UserID', 'h.UserId'
    $content = $content -replace 'TransactionType h\.PointsChanged', 'TransactionType = h.PointsChanged'

    [System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($true))
}

# Fix MiniGameBaseController.cs - Restore proper syntax
$file = Join-Path $BasePath "Controllers\MiniGameBaseController.cs"
$content = Get-Content $file -Raw -Encoding UTF8
$content = $content -replace 'int\? // TargetUserId = targetUserId null', 'int? targetUserId = null'
$content = $content -replace '// TargetUserId = targetUserId targetUserId', '// TargetUserId = targetUserId'
$content = $content -replace 'ManagerId = managerId', 'ManagerId = managerId.Value'

[System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($true))

Write-Host "All syntax fixes applied!" -ForegroundColor Green
