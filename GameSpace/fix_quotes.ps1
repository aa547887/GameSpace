$file = "GameSpace\Areas\MiniGame\Views\Shared\_AdminLayout.cshtml"
$content = Get-Content $file -Raw -Encoding UTF8
$content = $content -replace '""', '"'
$content = $content -replace '""', '"'
[System.IO.File]::WriteAllText((Resolve-Path $file).Path, $content, [System.Text.Encoding]::UTF8)
Write-Host "Fixed quotes in $file"
