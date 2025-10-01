# MiniGame 區域管理腳本
# 此腳本用於管理 MiniGame 區域的各種任務

param(
    [Parameter(Mandatory=False)]
    [ValidateSet("build", "clean", "test", "deploy", "backup", "help")]
    [string] = "help"
)

# 設定顏色
 = "Red"
 = "Green"
 = "Cyan"
 = "Yellow"

# 輸出函數
function Write-ColorOutput {
    param(
        [string],
        [string] = "White"
    )
    Write-Host  -ForegroundColor 
}

# 顯示幫助信息
function Show-Help {
    Write-ColorOutput "MiniGame 區域管理腳本" 
    Write-ColorOutput "=====================" 
    Write-ColorOutput ""
    Write-ColorOutput "用法: .\MiniGame-Manager.ps1 -Action [動作]" 
    Write-ColorOutput ""
    Write-ColorOutput "可用動作:" 
    Write-ColorOutput "  build   - 建置 MiniGame 專案" 
    Write-ColorOutput "  clean   - 清理建置檔案" 
    Write-ColorOutput "  test    - 執行測試" 
    Write-ColorOutput "  deploy  - 部署到目標環境" 
    Write-ColorOutput "  backup  - 備份重要檔案" 
    Write-ColorOutput "  help    - 顯示此幫助信息" 
    Write-ColorOutput ""
}

# 建置專案
function Build-Project {
    Write-ColorOutput "開始建置 MiniGame 專案..." 
    
    try {
        # 檢查是否在正確的目錄
        if (-not (Test-Path "Controllers")) {
            Write-ColorOutput "錯誤: 請在 MiniGame 區域目錄下執行此腳本" 
            return
        }
        
        # 執行建置
        dotnet build --configuration Release --verbosity minimal
        if (1 -eq 0) {
            Write-ColorOutput "建置成功完成!" 
        } else {
            Write-ColorOutput "建置失敗!" 
        }
    }
    catch {
        Write-ColorOutput "建置過程中發生錯誤: " 
    }
}

# 清理建置檔案
function Clean-Project {
    Write-ColorOutput "開始清理建置檔案..." 
    
    try {
        # 清理 bin 和 obj 目錄
         = Get-ChildItem -Path "." -Recurse -Directory -Name "bin" -ErrorAction SilentlyContinue
         = Get-ChildItem -Path "." -Recurse -Directory -Name "obj" -ErrorAction SilentlyContinue
        
        foreach ( in ) {
            Remove-Item -Path  -Recurse -Force -ErrorAction SilentlyContinue
            Write-ColorOutput "已清理: " 
        }
        
        foreach ( in ) {
            Remove-Item -Path  -Recurse -Force -ErrorAction SilentlyContinue
            Write-ColorOutput "已清理: " 
        }
        
        Write-ColorOutput "清理完成!" 
    }
    catch {
        Write-ColorOutput "清理過程中發生錯誤: " 
    }
}

# 執行測試
function Test-Project {
    Write-ColorOutput "開始執行測試..." 
    
    try {
        dotnet test --verbosity minimal
        if (1 -eq 0) {
            Write-ColorOutput "所有測試通過!" 
        } else {
            Write-ColorOutput "測試失敗!" 
        }
    }
    catch {
        Write-ColorOutput "測試過程中發生錯誤: " 
    }
}

# 備份重要檔案
function Backup-Files {
    Write-ColorOutput "開始備份重要檔案..." 
    
    try {
         = ".\Backups\20251001_084338"
        New-Item -Path  -ItemType Directory -Force | Out-Null
        
        # 備份重要目錄
         = @("Controllers", "Models", "Views", "Services")
        foreach ( in ) {
            if (Test-Path ) {
                Copy-Item -Path  -Destination "\" -Recurse -Force
                Write-ColorOutput "已備份: " 
            }
        }
        
        Write-ColorOutput "備份完成! 位置: " 
    }
    catch {
        Write-ColorOutput "備份過程中發生錯誤: " 
    }
}

# 部署到目標環境
function Deploy-Project {
    Write-ColorOutput "開始部署專案..." 
    
    try {
        # 這裡可以添加部署邏輯
        # 例如: 複製檔案到目標目錄、執行部署腳本等
        Write-ColorOutput "部署功能尚未實作" 
        Write-ColorOutput "請手動執行部署流程" 
    }
    catch {
        Write-ColorOutput "部署過程中發生錯誤: " 
    }
}

# 主執行邏輯
Write-ColorOutput "MiniGame 區域管理腳本啟動" 
Write-ColorOutput "執行動作: " 
Write-ColorOutput ""

switch () {
    "build" { Build-Project }
    "clean" { Clean-Project }
    "test" { Test-Project }
    "deploy" { Deploy-Project }
    "backup" { Backup-Files }
    "help" { Show-Help }
    default { Show-Help }
}

Write-ColorOutput ""
Write-ColorOutput "腳本執行完成" 
