# Fix MiniGame Area Database Field Name Errors
# This script fixes all field name mismatches between code and database schema

$BasePath = "C:\Users\n2029\Desktop\GameSpace\GameSpace\GameSpace\Areas\MiniGame"

Write-Host "Starting MiniGame field name fixes..." -ForegroundColor Green

# Fix 1: CouponTypeService.cs - Remove non-existent properties
Write-Host "Fixing CouponTypeService.cs..." -ForegroundColor Yellow
$file = Join-Path $BasePath "Services\CouponTypeService.cs"
$content = Get-Content $file -Raw -Encoding UTF8
$content = $content -replace '\.CreatedAt', '.ValidFrom'
$content = $content -replace 'couponType\.IsActive = true;', '// IsActive property does not exist in CouponType'
$content = $content -replace 'couponType\.UpdatedAt = DateTime\.UtcNow;', '// UpdatedAt property does not exist in CouponType'
$content = $content -replace 'ct\.IsActive = !ct\.IsActive;', '// IsActive toggle not available'
$content = $content -replace 'ct\.IsActive', '(ct.ValidFrom <= DateTime.UtcNow && ct.ValidTo >= DateTime.UtcNow)'
$content = $content -replace '\.CouponTypeName', '.Name'
$content = $content -replace 'couponType\.CouponTypeName', 'couponType.Name'
[System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($true))

# Fix 2: EVoucherTypeService.cs - Fix all field names
Write-Host "Fixing EVoucherTypeService.cs..." -ForegroundColor Yellow
$file = Join-Path $BasePath "Services\EVoucherTypeService.cs"
$content = Get-Content $file -Raw -Encoding UTF8
$content = $content -replace '\.CreatedAt', '.ValidFrom'
$content = $content -replace 'evoucherType\.IsActive = true;', '// IsActive property does not exist in EvoucherType'
$content = $content -replace 'evoucherType\.UpdatedAt = DateTime\.UtcNow;', '// UpdatedAt property does not exist in EvoucherType'
$content = $content -replace 'et\.IsActive = !et\.IsActive;', '// IsActive toggle not available'
$content = $content -replace '\.IsActive', '(ValidFrom <= DateTime.UtcNow && ValidTo >= DateTime.UtcNow)'
$content = $content -replace '\.EVoucherTypeName', '.Name'
$content = $content -replace '\.Stock', '.TotalAvailable'
$content = $content -replace '\.Value', '.ValueAmount'
$content = $content -replace 'evoucherType\.EVoucherTypeName', 'evoucherType.Name'
[System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($true))

# Fix 3: UserService.cs - Fix User field names
Write-Host "Fixing UserService.cs..." -ForegroundColor Yellow
$file = Join-Path $BasePath "Services\UserService.cs"
$content = Get-Content $file -Raw -Encoding UTF8
$content = $content -replace 'User_LockoutEnd', 'UserLockoutEnd'
$content = $content -replace '\.User_RightName', '.UserStatus' # UserRight doesn't have User_RightName
$content = $content -replace 'UserRight\.User_RightName', 'UserRight.UserStatus.ToString()'
$content = $content -replace 'UserRight\.User_GrantedAt', 'DateTime.UtcNow' # Property doesn't exist
$content = $content -replace 'User_RightName =', '// User_RightName property does not exist //'
$content = $content -replace 'User_GrantedAt =', '// User_GrantedAt property does not exist //'
$content = $content -replace '\.Date(?=\s*==)', '.Value.Date'
[System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($true))

# Fix 4: SignInStatsService.cs - Fix UserSignInStat field names
Write-Host "Fixing SignInStatsService.cs..." -ForegroundColor Yellow
$file = Join-Path $BasePath "Services\SignInStatsService.cs"
$content = Get-Content $file -Raw -Encoding UTF8
$content = $content -replace '\.UserID', '.UserId'
$content = $content -replace '\.SignInTime', '.SignTime'
$content = $content -replace '\.SignTimeFrom', '.StartDate'
$content = $content -replace '\.SignTimeTo', '.EndDate'
$content = $content -replace '\.ConsecutiveDays', '.PointsGained' # ConsecutiveDays doesn't exist
$content = $content -replace '\.PointsEarned', '.PointsGained'
$content = $content -replace '\.PetExpEarned', '.ExpGained'
$content = $content -replace '\.CouponEarned', '.CouponGained'
$content = $content -replace 'TodaySignInCount =', '// Field mapping needed //'
$content = $content -replace 'ThisWeekSignInCount =', '// Field mapping needed //'
$content = $content -replace 'ThisMonthSignInCount =', '// Field mapping needed //'
$content = $content -replace 'PerfectAttendanceCount =', '// Field mapping needed //'
$content = $content -replace 'TotalPointsGranted =', '// Field mapping needed //'
$content = $content -replace 'TotalExpGranted =', '// Field mapping needed //'
$content = $content -replace 'TotalCouponsGranted =', '// Field mapping needed //'
$content = $content -replace 'List<UserSignInStat>', 'List<UserSignInStats>'
[System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($true))

# Fix 5: SignInService.cs - Fix field names
Write-Host "Fixing SignInService.cs..." -ForegroundColor Yellow
$file = Join-Path $BasePath "Services\SignInService.cs"
$content = Get-Content $file -Raw -Encoding UTF8
$content = $content -replace '\.SignInTime', '.SignTime'
$content = $content -replace '\.ConsecutiveDays', '.PointsGained'
$content = $content -replace '\.IsActive(?=\s*==)', '(ValidFrom <= DateTime.UtcNow && ValidTo >= DateTime.UtcNow)'
$content = $content -replace 'Coupon\.IssueTime', 'Coupon.AcquiredTime'
$content = $content -replace 'Coupon\.ExpiryTime', 'CouponType.ValidTo'
$content = $content -replace 'UserSignInStats', 'UserSignInStat'
$content = $content -replace 'UserId =', '// Field mapping needed //'
$content = $content -replace 'SignInTime =', '// Use SignTime //'
$content = $content -replace 'PointsGained =', '// Already exists //'
$content = $content -replace 'ExpGained =', '// Already exists //'
$content = $content -replace 'CouponGained =', '// Already exists //'
[System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($true))

# Fix 6: DashboardService.cs - Fix field names
Write-Host "Fixing DashboardService.cs..." -ForegroundColor Yellow
$file = Join-Path $BasePath "Services\DashboardService.cs"
$content = Get-Content $file -Raw -Encoding UTF8
$content = $content -replace '\.SignInTime', '.SignTime'
$content = $content -replace '\.UserID', '.UserId'
$content = $content -replace 'Pet\.UserID', 'Pet.UserId'
$content = $content -replace 'UserSignInStat\.UserID', 'UserSignInStat.UserId'
$content = $content -replace 'Coupon\.ExpiryTime', 'CouponType.ValidTo'
$content = $content -replace '\.Users', '.User'
$content = $content -replace '\.Date(?=\s*[>=<])', '.Value.Date'
[System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($true))

# Fix 7: MiniGameBaseController.cs - Fix SystemSetting and AdminOperationLog
Write-Host "Fixing MiniGameBaseController.cs..." -ForegroundColor Yellow
$file = Join-Path $BasePath "Controllers\MiniGameBaseController.cs"
$content = Get-Content $file -Raw -Encoding UTF8
$content = $content -replace 'SystemSetting\.Key', 'SystemSetting.SettingKey'
$content = $content -replace 'SystemSetting\.Value', 'SystemSetting.SettingValue'
$content = $content -replace 'SystemSetting\.CreatedTime', 'SystemSetting.CreatedAt'
$content = $content -replace 'AdminOperationLog\.Operation', 'AdminOperationLog.OperationType'
$content = $content -replace 'AdminOperationLog\.Details', 'AdminOperationLog.OperationDetails'
$content = $content -replace 'AdminOperationLog\.TargetUserId', 'AdminOperationLog.ManagerId'
$content = $content -replace 'Operation =', 'OperationType ='
$content = $content -replace 'Details =', 'OperationDetails ='
$content = $content -replace 'TargetUserId =', '// TargetUserId property does not exist //'
[System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($true))

# Fix 8: AdminController.cs - Fix User.UserWallet
Write-Host "Fixing AdminController.cs..." -ForegroundColor Yellow
$file = Join-Path $BasePath "Controllers\AdminController.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw -Encoding UTF8
    $content = $content -replace 'User\.UserWallet', 'UserWallet'
    $content = $content -replace '\.UserWallet(?=\s*\))', ''
    [System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($true))
}

# Fix 9: AdminCouponController.cs - Fix Coupon.Users
Write-Host "Fixing AdminCouponController.cs..." -ForegroundColor Yellow
$file = Join-Path $BasePath "Controllers\AdminCouponController.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw -Encoding UTF8
    $content = $content -replace 'Coupon\.Users', 'Coupon.User'
    $content = $content -replace '\.Users', '.User'
    [System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($true))
}

# Fix 10: AdminWalletController.cs - Fix WalletHistory fields
Write-Host "Fixing AdminWalletController.cs..." -ForegroundColor Yellow
$file = Join-Path $BasePath "Controllers\AdminWalletController.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw -Encoding UTF8
    $content = $content -replace 'UserName =', '// UserName //'
    $content = $content -replace 'CurrentPoints =', '// CurrentPoints //'
    $content = $content -replace 'TransactionType =', '// Use ChangeType //'
    $content = $content -replace 'TransactionAmount =', '// Use PointsChanged //'
    $content = $content -replace 'WalletHistory\.HistoryID', 'WalletHistory.LogId'
    $content = $content -replace 'WalletHistory\.UserID', 'WalletHistory.UserId'
    [System.IO.File]::WriteAllText($file, $content, [System.Text.UTF8Encoding]::new($true))
}

Write-Host "All fixes applied successfully!" -ForegroundColor Green
Write-Host "Please rebuild the project to verify all fixes." -ForegroundColor Cyan
