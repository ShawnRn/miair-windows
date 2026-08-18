# MiAir for Windows 一键编译与打包脚本 (PowerShell)
param (
    [string]$Configuration = "Release",
    [string]$Platform = "Auto" # "Auto", "x64", "ARM64"
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  MiAir for Windows 构建脚本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 1. 自动探测或确认目标架构
if ($Platform -eq "Auto") {
    $arch = [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString()
    if ($arch -eq "Arm64") {
        $Platform = "ARM64"
    } else {
        $Platform = "x64"
    }
}

Write-Host "-> 目标架构: $Platform" -ForegroundColor Yellow
Write-Host "-> 构建配置: $Configuration" -ForegroundColor Yellow

# 2. 检查 Go 核心二进制
$coreBin = "$ScriptDir\core\bin\win-$($Platform.ToLower())\miair-core.exe"
if (-not (Test-Path $coreBin)) {
    Write-Host "-> 未发现预编译 Go 核心，正在调用 Go 编译..." -ForegroundColor Yellow
    Push-Location "$ScriptDir\core"
    $goArch = if ($Platform -eq "ARM64") { "arm64" } else { "amd64" }
    $env:CGO_ENABLED = "0"
    $env:GOOS = "windows"
    $env:GOARCH = $goArch
    New-Item -ItemType Directory -Force -Path "$ScriptDir\core\bin\win-$($Platform.ToLower())" | Out-Null
    go build -trimpath -ldflags="-s -w" -o $coreBin .
    Pop-Location
}

if (-not (Test-Path $coreBin)) {
    Write-Error "找不到 Go 核心可执行文件: $coreBin"
}
Write-Host "-> Go 核心已就绪: $coreBin" -ForegroundColor Green

# 3. 执行 .NET WinUI 3 构建
Write-Host "-> 正在编译 WinUI 3 桌面端..." -ForegroundColor Yellow
$rid = if ($Platform -eq "ARM64") { "win-arm64" } else { "win-x64" }

dotnet publish "$ScriptDir\src\MiAir.WinUI\MiAir.WinUI.csproj" `
    -c $Configuration `
    -r $rid `
    --self-contained true `
    -p:Platform=$Platform `
    -p:PublishSingleFile=false `
    -p:WindowsPackageType=None

$outputDir = "$ScriptDir\src\MiAir.WinUI\bin\$Platform\$Configuration\net8.0-windows10.0.19041.0\$rid\publish"

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  构建完成！可直接运行:" -ForegroundColor Green
Write-Host "  $outputDir\MiAir.WinUI.exe" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Green
