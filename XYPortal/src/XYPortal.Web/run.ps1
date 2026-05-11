#!/usr/bin/env pwsh
#Requires -Version 5.1

<#
.SYNOPSIS
    Cross-platform build and run script for XYPortal.Web

.DESCRIPTION
    This script builds the XYPortal solution dependencies and runs the web application.
    Supports both Windows and Linux platforms.

.NOTES
    Prerequisites:
    - .NET SDK 10.0 or higher
    - PowerShell 5.1 or higher (Windows) / PowerShell 6+ (Linux/macOS)
#>

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

# Get the directory where this script is located
$ScriptRoot = $PSScriptRoot

# Detect platform (using custom names to avoid conflict with PowerShell 6+ built-in variables)
$RunningOnWindows = $PSVersionTable.Platform -eq 'Win32NT' -or $null -eq $PSVersionTable.Platform
$RunningOnLinux = $PSVersionTable.Platform -eq 'Linux'
$RunningOnMacOS = $PSVersionTable.Platform -eq 'Darwin'

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "XYPortal.Web Build and Run Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Platform: $(if ($RunningOnWindows) { 'Windows' } elseif ($RunningOnLinux) { 'Linux' } elseif ($RunningOnMacOS) { 'macOS' } else { 'Unknown' })" -ForegroundColor Yellow
Write-Host ""

# Function to build a project
function Build-Project {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        
        [Parameter(Mandatory = $true)]
        [int]$ExitCode,
        
        [Parameter(Mandatory = $true)]
        [string]$ProjectName
    )
    
    Write-Host "Building $ProjectName..." -ForegroundColor Yellow
    
    try {
        Push-Location $Path
        dotnet build
        if ($LASTEXITCODE -ne 0) {
            Write-Host "ERROR: Failed to build $ProjectName (Exit code: $LASTEXITCODE)" -ForegroundColor Red
            exit $ExitCode
        }
        Write-Host "$ProjectName built successfully" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

# Build sequence
try {
    # 1. Build XYPortal.PasswordBook
    $PasswordBookPath = Join-Path $ScriptRoot "../../../XYPortal.PasswordBook"
    Build-Project -Path $PasswordBookPath -ExitCode 1 -ProjectName "XYPortal.PasswordBook"
    
    # 2. Build XYPortal.LinkBoard
    $LinkBoardPath = Join-Path $ScriptRoot "../../../XYPortal.LinkBoard"
    Build-Project -Path $LinkBoardPath -ExitCode 2 -ProjectName "XYPortal.LinkBoard"
    
    # 3. Build XYPortal.RandomStringProvider
    $RandomStringProviderPath = Join-Path $ScriptRoot "../../../XYPortal.RandomStringProvider"
    Build-Project -Path $RandomStringProviderPath -ExitCode 3 -ProjectName "XYPortal.RandomStringProvider"
    
    # 4. Build and Run XYPortal.Web
    Write-Host ""
    Write-Host "Building and running XYPortal.Web..." -ForegroundColor Yellow
    
    Push-Location $ScriptRoot
    Write-Host "Starting application..." -ForegroundColor Green
    dotnet run
}
catch {
    Write-Host ""
    Write-Host "ERROR: $_" -ForegroundColor Red
    exit 99
}
finally {
    if (Test-Path $ScriptRoot) {
        Pop-Location
    }
}
