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

# Detect platform (improved: handle 'Unix' value on Linux)
$RunningOnWindows = $PSVersionTable.Platform -eq 'Win32NT' -or $null -eq $PSVersionTable.Platform
$RunningOnLinux = $PSVersionTable.Platform -eq 'Linux' -or $PSVersionTable.Platform -eq 'Unix'
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

# Function to check if a port is available
function Test-PortAvailable {
    param([int]$Port)
    
    $endpoint = "127.0.0.1:$Port"
    
    if ($RunningOnWindows) {
        $connections = Get-NetTCPConnection -LocalAddress $endpoint -ErrorAction SilentlyContinue
    }
    else {
        # Linux/macOS: use ss or netstat
        $ssCmd = "ss -tlnp 2>/dev/null | grep ':$Port'"
        $output = Invoke-Expression $ssCmd
        $connections = $output
    }
    
    if ($connections -and $connections.Count -gt 0) {
        return $false
    }
    return $true
}

# Function to kill process on a specific port
function Stop-ProcessOnPort {
    param([int]$Port)
    
    Write-Host "  Checking port $Port..." -ForegroundColor Gray
    
    if ($RunningOnWindows) {
        $pids = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
        foreach ($pid in $pids) {
            try {
                Stop-Process -Id $pid -Force -ErrorAction Stop
                Write-Host "  [Killed] PID $pid on port $Port" -ForegroundColor Yellow
            }
            catch {
                Write-Host "  [Warn] Could not kill PID $pid on port $Port (may need elevated privileges)" -ForegroundColor Yellow
            }
        }
    }
    else {
        # Linux/macOS: parse ss output to get PIDs
        $ssCmd = "ss -tlnp 2>/dev/null | grep ':$Port'"
        $output = Invoke-Expression $ssCmd
        
        if ($output -match 'pid=(\d+)') {
            $pids = [regex]::Matches($output, 'pid=(\d+)') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
            foreach ($pid in $pids) {
                try {
                    # Try graceful kill first
                    Kill -Signal SIGTERM $pid 2>$null
                    Start-Sleep -Milliseconds 500
                    # Force kill if still running
                    Kill -Signal SIGKILL $pid 2>$null
                    Write-Host "  [Killed] PID $pid on port $Port" -ForegroundColor Yellow
                }
                catch {
                    Write-Host "  [Warn] Could not kill PID $pid on port $Port (may need sudo)" -ForegroundColor Yellow
                }
            }
        }
    }
}

# Function to cleanup all service processes
function Stop-AllServices {
    param([hashtable]$Processes)
    
    Write-Host ""
    Write-Host "Stopping all services..." -ForegroundColor Yellow
    
    foreach ($entry in $Processes.GetEnumerator()) {
        $name = $entry.Key
        $proc = $entry.Value
        
        if ($proc -and -not $proc.HasExited) {
            try {
                # Kill the process tree
                if (-not $RunningOnWindows) {
                    # On Linux/macOS, try to kill process group
                    Kill -Signal SIGTERM -Id $proc.Id 2>$null
                    Start-Sleep -Milliseconds 500
                    if (-not $proc.HasExited) {
                        Kill -Signal SIGKILL -Id $proc.Id 2>$null
                    }
                }
                else {
                    $proc.Kill()
                }
                Write-Host "  [$name] stopped (PID: $($proc.Id))" -ForegroundColor Gray
            }
            catch {
                Write-Host "  [$name] could not be stopped cleanly" -ForegroundColor Gray
            }
        }
    }
    
    # Also try to kill any stray dotnet processes for these projects
    $projectNames = @("XYPortal.AuthServer", "XYPortal.HttpApi.Host", "XYPortal.Web.Host")
    foreach ($projName in $projectNames) {
        if ($RunningOnWindows) {
            $strayProcs = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -like "*$projName*" }
        }
        else {
            $strayProcs = Invoke-Expression "pgrep -f 'dotnet.*$projName' 2>/dev/null" | ForEach-Object { Get-Process -Id $_ -ErrorAction SilentlyContinue }
        }
        
        foreach ($sp in $strayProcs) {
            try {
                if ($sp -and -not $sp.HasExited) {
                    Kill -Signal SIGKILL $sp.Id 2>$null
                    Write-Host "  [Cleanup] Killed stray $projName (PID: $($sp.Id))" -ForegroundColor Gray
                }
            }
            catch {
                # Ignore
            }
        }
    }
}

# =============================================
# Phase 0: Pre-flight check - cleanup stale processes
# =============================================
Write-Host "Phase 0: Pre-flight check..." -ForegroundColor Cyan

$ports = @{ 44367 = "AuthServer"; 44373 = "HttpApi.Host"; 44331 = "Web.Host" }

foreach ($port in $ports.Keys) {
    if (-not (Test-PortAvailable -Port $port)) {
        Write-Host "  Port $port ($($ports[$port])) is in use, attempting to free it..." -ForegroundColor Yellow
        Stop-ProcessOnPort -Port $port
        Start-Sleep -Milliseconds 500
    }
    else {
        Write-Host "  Port $port ($($ports[$port])) is available" -ForegroundColor Gray
    }
}

Write-Host ""

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

# Track running processes (PID-based, not Job-based)
$runningProcesses = @{}

try {
    # Start AuthServer
    Write-Host "[AuthServer] Starting dotnet run..." -ForegroundColor Yellow
    $authProc = Start-Process -FilePath "dotnet" -ArgumentList "run" -NoNewWindow -PassThru -WorkingDirectory $AuthServerPath
    $runningProcesses["AuthServer"] = $authProc
    Write-Host "[AuthServer] Started (PID: $($authProc.Id))" -ForegroundColor Green
    
    # Start HttpApi.Host
    Write-Host "[HttpApi.Host] Starting dotnet run..." -ForegroundColor Yellow
    $httpApiProc = Start-Process -FilePath "dotnet" -ArgumentList "run" -NoNewWindow -PassThru -WorkingDirectory $HttpApiHostPath
    $runningProcesses["HttpApi.Host"] = $httpApiProc
    Write-Host "[HttpApi.Host] Started (PID: $($httpApiProc.Id))" -ForegroundColor Green
    
    # Start Web.Host
    Write-Host "[Web.Host] Starting dotnet run..." -ForegroundColor Yellow
    $webHostProc = Start-Process -FilePath "dotnet" -ArgumentList "run" -NoNewWindow -PassThru -WorkingDirectory $WebHostPath
    $runningProcesses["Web.Host"] = $webHostProc
    Write-Host "[Web.Host] Started (PID: $($webHostProc.Id))" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "All three services are running." -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "ERROR: Failed to start services: $_" -ForegroundColor Red
    Stop-AllServices -Processes $runningProcesses
    exit 1
}

# =============================================
# Phase 3: Monitor services and wait for /q
# =============================================
$quit = $false
$checkInterval = 2  # seconds
$anyFailure = $false
$failureMessage = ""

while (-not $quit -and -not $anyFailure) {
    Start-Sleep -Seconds $checkInterval
    
    # Check if any process has exited unexpectedly
    foreach ($entry in $runningProcesses.GetEnumerator()) {
        $name = $entry.Key
        $proc = $entry.Value
        
        if ($proc -and $proc.HasExited) {
            $exitCode = $proc.ExitCode
            Write-Host ""
            Write-Host "========================================" -ForegroundColor Red
            Write-Host "SERVICE STOPPED: $name" -ForegroundColor Red
            Write-Host "========================================" -ForegroundColor Red
            Write-Host "PID: $($proc.Id)" -ForegroundColor Red
            Write-Host "Exit Code: $exitCode" -ForegroundColor Red
            
            if ($exitCode -ne 0) {
                $anyFailure = $true
                $failureMessage = "$name exited with code $exitCode"
            }
            
            # Remove from tracking
            $runningProcesses.Remove($name)
        }
    }
    
    # If all processes have exited
    if ($runningProcesses.Count -eq 0 -and -not $quit) {
        Write-Host ""
        Write-Host "All services have stopped." -ForegroundColor Yellow
        $anyFailure = $true
        $failureMessage = "All services exited unexpectedly"
    }
    
    # Check for /q input (non-blocking)
    if (-not $anyFailure) {
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
}

# =============================================
# Exit: User pressed /q or all services stopped
# =============================================
if ($quit) {
    Write-Host ""
    Write-Host "/q received. Stopping all services..." -ForegroundColor Yellow
}

Stop-AllServices -Processes $runningProcesses

if ($anyFailure) {
    Write-Host ""
    Write-Host "Exiting due to: $failureMessage" -ForegroundColor Red
    exit 1
}
else {
    Write-Host ""
    Write-Host "All services stopped. Exiting." -ForegroundColor Green
    exit 0
}
