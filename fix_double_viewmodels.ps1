$baseDir = "C:/Users/n2029/Desktop/GameSpace/GameSpace/GameSpace/Areas/MiniGame/Views"

Get-ChildItem -Path $baseDir -Recurse -Filter "*.cshtml" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw -Encoding UTF8
    if ($content -match 'ViewModels\.ViewModels\.') {
        Write-Host "Fixing double ViewModels in: $($_.FullName)"
        $content = $content -replace 'ViewModels\.ViewModels\.', 'ViewModels.'
        Set-Content $_.FullName -Value $content -NoNewline -Encoding UTF8
    }
}

Write-Host "`nDone fixing double ViewModels!" -ForegroundColor Green
