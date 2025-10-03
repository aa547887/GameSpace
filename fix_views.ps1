$baseDir = "C:/Users/n2029/Desktop/GameSpace/GameSpace/GameSpace/Areas/MiniGame/Views"

$files = @(
    'AdminEVoucher\CreateType.cshtml',
    'AdminEVoucher\Create.cshtml',
    'AdminMiniGame\Create.cshtml',
    'AdminSignInStats\SignInRules.cshtml',
    'Admin\Index.cshtml',
    'AdminManager\CreateRole.cshtml',
    'AdminManager\Create.cshtml',
    'AdminManager\Edit.cshtml',
    'AdminSignIn\Create.cshtml',
    'AdminUser\Edit.cshtml',
    'AdminUser\Create.cshtml',
    'AdminWallet\ViewHistory.cshtml',
    'AdminWallet\Transaction.cshtml',
    'AdminWallet\QueryHistory.cshtml',
    'AdminWallet\GrantEVouchers.cshtml',
    'AdminWallet\GrantCoupons.cshtml',
    'AdminWallet\AdjustEVouchers.cshtml',
    'AdminWallet\AdjustPoints.cshtml',
    'AdminPet\Create.cshtml',
    'AdminPet\PetRules.cshtml',
    'AdminPet\IndividualSettings.cshtml',
    'AdminPet\ColorChangeHistory.cshtml',
    'AdminPet\ListWithQuery.cshtml',
    'EVouchers\Create.cshtml',
    'EVouchers\Edit.cshtml',
    'PetBackgroundCostSetting\Create.cshtml',
    'PetSkinColorCostSetting\Create.cshtml',
    'PetLevelUpRuleValidation\Index.cshtml'
)

foreach($file in $files) {
    $fullPath = Join-Path $baseDir $file
    if(Test-Path $fullPath) {
        $content = Get-Content $fullPath -Raw
        # Fix ViewModel namespaces
        $content = $content -replace '@model GameSpace\.Areas\.MiniGame\.Models\.([A-Z][a-zA-Z]+ViewModel)', '@model GameSpace.Areas.MiniGame.Models.ViewModels.$1'
        # Fix Model namespaces
        $content = $content -replace '@model GameSpace\.Areas\.MiniGame\.Models\.([A-Z][a-zA-Z]+Model)', '@model GameSpace.Areas.MiniGame.Models.ViewModels.$1'
        # Save
        Set-Content $fullPath -Value $content -NoNewline
        Write-Host "Fixed: $file"
    } else {
        Write-Host "Not found: $file" -ForegroundColor Yellow
    }
}

Write-Host "`nDone!" -ForegroundColor Green
