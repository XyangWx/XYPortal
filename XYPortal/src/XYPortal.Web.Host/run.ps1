#!/usr/bin/env pwsh
#Requires -Version 5.1

<#
.SYNOPSIS
    Build and run script for XYPortal AuthServer, HttpApi.Host and Web.Host

.DESCRIPTION
    Builds XYPortal.LinkBoard, XYPortal.RandomStringProvider, XYPortal.PasswordBook,
    then runs AuthServer, HttpApi.Host and Web.Host simultaneously.
    Supports Windows and Linux/macOS.
    
    Type /q and press Enter to stop all services and exit.
#>

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$ScriptRoot = $PSScriptRoot

# Detect platform
$RunningOnWindows = $PSVersionTable.Platform -eq 'Win32NT' -or $null -eq $PSVersionTable.Platform
$RunningOnLinux = $PSVersionTable.Platform -eq 'Linux'
$RunningOnMacOS = $PSVersionTable.Platform -eq 'Darwin'

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "XYPortal Build and Run Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Platform: $(if ($RunningOnWindows) { 'Windows' } elseif ($RunningOnLinux) { 'Linux' } elseif ($RunningOnMacOS) { 'macOS' } else { 'Unknown' })" -ForegroundColor Yellow
Write-Host ""

# XYPortal root is 3 levels up from script root
# Script: src/XYPortal.Web.Host/run.ps1
# Root:   /data/Repositories/XYPortal/
$XYPortalRoot = Split-Path (Split-Path (Split-Path $ScriptRoot -Parent) -Parent) -Parent

function Test-ProjectExists {
    param([string]$Path, [string]$ProjectName)
    if (-not (Test-Path $Path)) {
        Write-Host ""
        Write-Host "ERROR: Cannot find path '$Path'" -ForegroundColor Red
        Write-Host "       for project '$ProjectName'" -ForegroundColor Red
        exit 1
    }
    Write-Host "  [OK] $ProjectName" -ForegroundColor Gray
}

function Build-Project {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        
        [Parameter(Mandatory = $true)]
        [string]$ProjectName
    )
    
    Write-Host ""
    Write-Host "[$ProjectName] Building..." -ForegroundColor Yellow
    
    if (-not (Test-Path $Path)) {
        Write-Host ""
        Write-Host "ERROR: Cannot find path '$Path' for project '$ProjectName'" -ForegroundColor Red
        exit 1
    }
    
    try {
        Push-Location $Path
        $output = dotnet build 2>&1
        $exitCode = $LASTEXITCODE
        
        if ($exitCode -ne 0) {
            Write-Host ""
            Write-Host "========================================" -ForegroundColor Red
            Write-Host "BUILD FAILED: $ProjectName" -ForegroundColor Red
            Write-Host "========================================" -ForegroundColor Red
            Write-Host "Path: $Path" -ForegroundColor Red
            Write-Host "Exit Code: $exitCode" -ForegroundColor Red
            Write-Host ""
            Write-Host "Build Output:" -ForegroundColor Red
            Write-Host $output -ForegroundColor Red
            Pop-Location
            exit 1
        }
        
        Write-Host "[$ProjectName] Build succeeded" -ForegroundColor Green
    }
    catch {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Red
        Write-Host "BUILD ERROR: $ProjectName" -ForegroundColor Red
        Write-Host "========================================" -ForegroundColor Red
        Write-Host "Path: $Path" -ForegroundColor Red
        Write-Host "Exception: $_" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    finally {
        if (Test-Path $Path) {
            Pop-Location
        }
    }
}

# =============================================
# Phase 1: Build dependency projects
# =============================================
Write-Host "Phase 1: Building dependency projects..." -ForegroundColor Cyan

# Build XYPortal.LinkBoard
$LinkBoardPath = Join-Path $XYPortalRoot "XYPortal.LinkBoard"
Test-ProjectExists -Path $LinkBoardPath -ProjectName "XYPortal.LinkBoard"
Build-Project -Path $LinkBoardPath -ProjectName "XYPortal.LinkBoard"

# Build XYPortal.RandomStringProvider
$RandomStringProviderPath = Join-Path $XYPortalRoot "XYPortal.RandomStringProvider"
Test-ProjectExists -Path $RandomStringProviderPath -ProjectName "XYPortal.RandomStringProvider"
Build-Project -Path $RandomStringProviderPath -ProjectName "XYPortal.RandomStringProvider"

# Build XYPortal.PasswordBook
$PasswordBookPath = Join-Path $XYPortalRoot "XYPortal.PasswordBook"
Test-ProjectExists -Path $PasswordBookPath -ProjectName "XYPortal.PasswordBook"
Build-Project -Path $PasswordBookPath -ProjectName "XYPortal.PasswordBook"

Write-Host ""
Write-Host "All dependency projects built successfully." -ForegroundColor Green

# =============================================
# Phase 2: Run AuthServer, HttpApi.Host, Web.Host
# =============================================
Write-Host ""
Write-Host "Phase 2: Starting services..." -ForegroundColor Cyan

$AuthServerPath = Join-Path $ScriptRoot "../XYPortal.AuthServer"
$HttpApiHostPath = Join-Path $ScriptRoot "../XYPortal.HttpApi.Host"
$WebHostPath = $ScriptRoot

Test-ProjectExists -Path $AuthServerPath -ProjectName "XYPortal.AuthServer"
Test-ProjectExists -Path $HttpApiHostPath -ProjectName "XYPortal.HttpApi.Host"
Test-ProjectExists -Path $WebHostPath -ProjectName "XYPortal.Web.Host"

Write-Host ""
Write-Host "Starting AuthServer, HttpApi.Host, and Web.Host..." -ForegroundColor Green
Write-Host "Type /q and press Enter to stop all services and exit." -ForegroundColor Yellow
Write-Host ""

# Start all three services as background jobs
$jobs = @()

$authJob = Start-Job -ScriptBlock {
    param($Path, $Name)
    try {
        Push-Location $Path
        $process = Start-Process -FilePath "dotnet" -ArgumentList "run" -NoNewWindow -PassThru -Wait
        return @{ Name = $Name; ExitCode = $process.ExitCode; State = "Completed" }
    }
    catch {
        return @{ Name = $Name; Error = $_.Exception.Message; State = "Failed" }
    }
    finally {
        Pop-Location
    }
} -ArgumentList $AuthServerPath, "AuthServer"
$jobs += @{ Name = "AuthServer"; Job = $authJob }
Write-Host "[AuthServer] Started (Job ID: $($authJob.Id))" -ForegroundColor Yellow

$httpApiJob = Start-Job -ScriptBlock {
    param($Path, $Name)
    try {
        Push-Location $Path
        $process = Start-Process -FilePath "dotnet" -ArgumentList "run" -NoNewWindow -PassThru -Wait
        return @{ Name = $Name; ExitCode = $process.ExitCode; State = "Completed" }
    }
    catch {
        return @{ Name = $Name; Error = $_.Exception.Message; State = "Failed" }
    }
    finally {
        Pop-Location
    }
} -ArgumentList $HttpApiHostPath, "HttpApi.Host"
$jobs += @{ Name = "HttpApi.Host"; Job = $httpApiJob }
Write-Host "[HttpApi.Host] Started (Job ID: $($httpApiJob.Id))" -ForegroundColor Yellow

$webHostJob = Start-Job -ScriptBlock {
    param($Path, $Name)
    try {
        Push-Location $Path
        $process = Start-Process -FilePath "dotnet" -ArgumentList "run" -NoNewWindow -PassThru -Wait
        return @{ Name = $Name; ExitCode = $process.ExitCode; State = "Completed" }
    }
    catch {
        return @{ Name = $Name; Error = $_.Exception.Message; State = "Failed" }
    }
    finally {
        Pop-Location
    }
} -ArgumentList $WebHostPath, "Web.Host"
$jobs += @{ Name = "Web.Host"; Job = $webHostJob }
Write-Host "[Web.Host] Started (Job ID: $($webHostJob.Id))" -ForegroundColor Yellow

Write-Host ""
Write-Host "All three services are running." -ForegroundColor Green

# =============================================
# Phase 3: Monitor services and wait for /q
# =============================================
$quit = $false
$checkInterval = 2  # seconds

while (-not $quit) {
    Start-Sleep -Seconds $checkInterval
    
    # Check if any job has ended unexpectedly
    foreach ($jobInfo in $jobs) {
        $job = $jobInfo.Job
        $name = $jobInfo.Name
        
        # Get any output/error
        if ($job.HasMoreData) {
            $output = Receive-Job -Job $job
            if ($output) {
                foreach ($item in $output) {
                    if ($item.State -eq "Failed") {
                        Write-Host ""
                        Write-Host "========================================" -ForegroundColor Red
                        Write-Host "SERVICE FAILED: $($item.Name)" -ForegroundColor Red
                        Write-Host "========================================" -ForegroundColor Red
                        if ($item.Error) {
                            Write-Host "Error: $($item.Error)" -ForegroundColor Red
                        }
                        
                        # Stop all other jobs
                        Write-Host ""
                        Write-Host "Stopping all remaining services..." -ForegroundColor Yellow
                        foreach ($ji in $jobs) {
                            Stop-Job -Job $ji.Job -ErrorAction SilentlyContinue
                            Remove-Job -Job $ji.Job -ErrorAction SilentlyContinue
                        }
                        
                        Write-Host "Exiting due to service failure." -ForegroundColor Red
                        exit 1
                    }
                }
            }
        }
        
        # Check job state
        if ($job.State -eq 'Failed') {
            Write-Host ""
            Write-Host "========================================" -ForegroundColor Red
            Write-Host "SERVICE STOPPED: $name" -ForegroundColor Red
            Write-Host "========================================" -ForegroundColor Red
            
            $output = Receive-Job -Job $job
            if ($output) {
                Write-Host ($output | Out-String) -ForegroundColor Red
            }
            
            # Stop all other jobs
            Write-Host ""
            Write-Host "Stopping all remaining services..." -ForegroundColor Yellow
            foreach ($ji in $jobs) {
                Stop-Job -Job $ji.Job -ErrorAction SilentlyContinue
                Remove-Job -Job $ji.Job -ErrorAction SilentlyContinue
            }
            
            Write-Host "Exiting due to service failure." -ForegroundColor Red
            exit 1
        }
    }
    
    # Check for /q input (non-blocking)
    try {
        if ($Host.UI.RawUI.KeyAvailable) {
            $key = $Host.UI.RawUI.ReadKey('NoEcho, IncludeKeyDown')
            if ($key.Character -eq 'q' -or $key.Character -eq 'Q') {
                $quit = $true
            }
        }
    }
    catch {
        # Key reading not available, continue polling
    }
}

# =============================================
# Exit: User pressed /q or all services stopped
# =============================================
Write-Host ""
Write-Host "/q received. Stopping all services..." -ForegroundColor Yellow

foreach ($jobInfo in $jobs) {
    try {
        Stop-Job -Job $jobInfo.Job -ErrorAction SilentlyContinue
        Remove-Job -Job $jobInfo.Job -ErrorAction SilentlyContinue
        Write-Host "  [$($jobInfo.Name)] stopped" -ForegroundColor Gray
    }
    catch {
        Write-Host "  [$($jobInfo.Name)] could not be stopped cleanly" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "All services stopped. Exiting." -ForegroundColor Green
exit 0
