param(
    [Parameter(Mandatory=$false)]
    [string]$a,

    [Parameter(Mandatory=$false)]
    [string]$o
)

# ============================================================
# Vue 项目构建脚本
# 用法:
#   .\build.ps1                                    # 使用 .env 中的现有值
#   .\build.ps1 -a "http://api.example.com"        # 只更新 VITE_API_BASE_URL
#   .\build.ps1 -o "http://auth.example.com"       # 只更新 VITE_AUTH_SERVER_URL
#   .\build.ps1 -a "http://api.example.com" -o "http://auth.example.com"  # 同时更新
# ============================================================

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot
$EnvFile = Join-Path $ProjectRoot ".env"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Vue 项目构建脚本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ---------- 检查 .env 文件 ----------
if (-not (Test-Path $EnvFile)) {
    Write-Host "错误: .env 文件不存在: $EnvFile" -ForegroundColor Red
    exit 1
}

# ---------- 先备份原始 .env ----------
$EnvFileBackup = "$EnvFile.bak"
Copy-Item -Path $EnvFile -Destination $EnvFileBackup -Force

# ---------- 读取并更新 .env ----------
function Update-EnvVar {
    param(
        [string]$FilePath,
        [string]$VarName,
        [string]$VarValue
    )

    if ([string]::IsNullOrWhiteSpace($VarValue)) {
        Write-Host "  [跳过] $VarName 未提供新值，保留现有配置" -ForegroundColor DarkGray
        return
    }

    $content = Get-Content $FilePath -Raw
    $pattern = "^($VarName=.*)$"

    if ($content -match $pattern) {
        $newLine = "$VarName=$VarValue"
        $content = $content -replace $pattern, $newLine
        Write-Host "  [更新] $newLine" -ForegroundColor Yellow
    } else {
        $content = $content.TrimEnd() + "`n$VarName=$VarValue`n"
        Write-Host "  [新增] $VarName=$VarValue" -ForegroundColor Green
    }

    Set-Content -Path $FilePath -Value $content -NoNewline
}

if (-not [string]::IsNullOrWhiteSpace($a)) {
    Write-Host "更新 VITE_API_BASE_URL:" -ForegroundColor White
    Update-EnvVar -FilePath $EnvFile -VarName "VITE_API_BASE_URL" -VarValue $a
}

if (-not [string]::IsNullOrWhiteSpace($o)) {
    Write-Host "更新 VITE_AUTH_SERVER_URL:" -ForegroundColor White
    Update-EnvVar -FilePath $EnvFile -VarName "VITE_AUTH_SERVER_URL" -VarValue $o
}

if ([string]::IsNullOrWhiteSpace($a) -and [string]::IsNullOrWhiteSpace($o)) {
    Write-Host "未提供参数，使用 .env 中现有配置进行构建" -ForegroundColor DarkGray
}

Write-Host ""

# ---------- 执行构建 ----------
Write-Host "开始构建 Vue 项目..." -ForegroundColor Cyan

Push-Location $ProjectRoot

try {
    # 检查 node_modules 是否存在，不存在则先安装依赖
    $nodeModules = Join-Path $ProjectRoot "node_modules"
    if (-not (Test-Path $nodeModules)) {
        Write-Host "node_modules 不存在，正在安装依赖..." -ForegroundColor Yellow
        npm install
        if ($LASTEXITCODE -ne 0) {
            throw "npm install 失败"
        }
    }

    # 执行构建
    Write-Host "执行 npm run build ..." -ForegroundColor White
    npm run build

    if ($LASTEXITCODE -ne 0) {
        throw "npm run build 失败"
    }

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  构建成功!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green

} finally {
    # 恢复原始 .env
    Copy-Item -Path $EnvFileBackup -Destination $EnvFile -Force
    Remove-Item -Path $EnvFileBackup -Force
    Pop-Location
}
