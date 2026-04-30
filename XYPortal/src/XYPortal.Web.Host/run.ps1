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

# Detect platform (handle 'Unix' value on Linux)
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

# Function to check if a port is available (only LISTEN state blocks binding)
function Test-PortAvailable {
    param([int]$Port)
    
    if ($RunningOnWindows) {
        # Only check for LISTEN state - TIME_WAIT does not block port binding
        $listening = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
    }
    else {
        # ss -tlnp only shows LISTEN sockets by default, TIME_WAIT does not appear here
        $ssCmd = "ss -tlnp 2>/dev/null | grep ':$Port'"
        $output = Invoke-Expression $ssCmd
        $listening = $output
    }
    
    if ($listening -and $listening.Count -gt 0) {
        return $false
    }
    return $true
}

# Function to wait for a service to be ready (port is listening)
function Wait-ForServiceReady {
    param(
        [int]$Port,
        [string]$ServiceName,
        [int]$TimeoutSeconds = 60
    )
    
    Write-Host "  Waiting for $ServiceName to be ready on port $Port..." -ForegroundColor Cyan
    
    $elapsed = 0
    $checkInterval = 2
    
    while ($elapsed -lt $TimeoutSeconds) {
        Start-Sleep -Seconds $checkInterval
        $elapsed += $checkInterval
        
        if (-not (Test-PortAvailable -Port $Port)) {
            Write-Host "  [$ServiceName] Ready (port $Port is listening)" -ForegroundColor Green
            return $true
        }
        
        Write-Host "  [$ServiceName] Still starting... (${elapsed}s/${TimeoutSeconds}s)" -ForegroundColor Gray
    }
    
    Write-Host "  [$ServiceName] WARNING: Timeout waiting for port $Port" -ForegroundColor Yellow
    return $false
}

# Function to gracefully stop a process by PID
function Stop-ProcessGracefully {
    param(
        [int]$Pid,
        [string]$Name,
        [int]$WaitSeconds = 5
    )
    
    try {
        $proc = Get-Process -Id $Pid -ErrorAction SilentlyContinue
    }
    catch {
        Write-Host "  [$Name] Process $Pid already gone" -ForegroundColor Gray
        return
    }
    
    if (-not $proc) {
        Write-Host "  [$Name] Process $Pid already gone" -ForegroundColor Gray
        return
    }
    
    if ($RunningOnWindows) {
        # Windows: use GracefulShutdownMethod if available
        try {
            $proc.CloseMainWindow() | Out-Null
            $waited = 0
            while (-not $proc.HasExited -and $waited -lt ($WaitSeconds * 1000)) {
                Start-Sleep -Milliseconds 200
                $waited += 200
            }
            if (-not $proc.HasExited) {
                $proc.Kill()
                Write-Host "  [$Name] Forcefully stopped (PID: $Pid)" -ForegroundColor Gray
            }
            else {
                Write-Host "  [$Name] Gracefully stopped (PID: $Pid)" -ForegroundColor Gray
            }
        }
        catch {
            $proc.Kill() | Out-Null
            Write-Host "  [$Name] Forcefully stopped (PID: $Pid)" -ForegroundColor Gray
        }
    }
    else {
        # Linux/macOS: SIGTERM first, then SIGKILL
        try {
            Kill -Signal SIGTERM $Pid 2>$null
            Write-Host "  [$Name] Sent SIGTERM, waiting ${WaitSeconds}s..." -ForegroundColor Gray
            
            $waited = 0
            $interval = 200  # ms
            while (-not $proc.HasExited -and $waited -lt ($WaitSeconds * 1000)) {
                Start-Sleep -Milliseconds $interval
                $waited += $interval
                # Re-fetch process to check HasExited
                try {
                    $proc = Get-Process -Id $Pid -ErrorAction SilentlyContinue
                    if (-not $proc) { break }
                }
                catch { break }
            }
            
            if ($proc -and -not $proc.HasExited) {
                Kill -Signal SIGKILL $Pid 2>$null
                Write-Host "  [$Name] Forcefully killed with SIGKILL (PID: $Pid)" -ForegroundColor Yellow
            }
            else {
                Write-Host "  [$Name] Gracefully stopped (PID: $Pid)" -ForegroundColor Gray
            }
        }
        catch {
            try {
                Kill -Signal SIGKILL $Pid 2>$null
                Write-Host "  [$Name] Forcefully killed (PID: $Pid)" -ForegroundColor Yellow
            }
            catch {
                Write-Host "  [$Name] Could not stop PID $Pid" -ForegroundColor Yellow
            }
        }
    }
}

# Function to kill process on a specific port
function Stop-ProcessOnPort {
    param([int]$Port, [string]$ServiceName)
    
    Write-Host "  Port $Port ($ServiceName) is in use, attempting to free it..." -ForegroundColor Yellow
    
    if ($RunningOnWindows) {
        $pids = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
        foreach ($pid in $pids) {
            Stop-ProcessGracefully -Pid $pid -Name "$ServiceName (port $Port)"
        }
    }
    else {
        $ssCmd = "ss -tlnp 2>/dev/null | grep ':$Port'"
        $output = Invoke-Expression $ssCmd
        
        if ($output -match 'pid=(\d+)') {
            $pids = [regex]::Matches($output, 'pid=(\d+)') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
            foreach ($pid in $pids) {
                Stop-ProcessGracefully -Pid $pid -Name "$ServiceName (port $Port)"
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
        
        if ($proc) {
            Stop-ProcessGracefully -Pid $proc.Id -Name $name
        }
    }
    
    # Cleanup stray dotnet processes for AuthServer, HttpApi.Host, and Web.Host
    $dotnetProjects = @("XYPortal.AuthServer", "XYPortal.HttpApi.Host", "XYPortal.Web.Host")
    foreach ($projName in $dotnetProjects) {
        if ($RunningOnWindows) {
            $strayPids = @()
            try {
                $strayProcs = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -like "*$projName*" }
                $strayPids = $strayProcs | Select-Object -ExpandProperty Id
            }
            catch { }
        }
        else {
            $strayPids = Invoke-Expression "pgrep -f 'dotnet.*$projName' 2>/dev/null"
            if ($strayPids -is [string]) { $strayPids = @($strayPids) }
        }
        
        foreach ($pid in $strayPids) {
            if ($pid) {
                Stop-ProcessGracefully -Pid $pid -Name "stray $projName"
            }
        }
    }
}

# =============================================
# Phase 0: Pre-flight check - cleanup stale processes
# =============================================
Write-Host "Phase 0: Pre-flight check..." -ForegroundColor Cyan

$ports = @{
    44367 = "AuthServer"
    44373 = "HttpApi.Host"
    44331 = "Web.Host"
}

foreach ($port in $ports.Keys) {
    if (-not (Test-PortAvailable -Port $port)) {
        Stop-ProcessOnPort -Port $port -ServiceName $ports[$port]
        Start-Sleep -Milliseconds 1000
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
# Phase 2: Run AuthServer, HttpApi.Host, then Web.Host
# =============================================
Write-Host ""
Write-Host "Phase 2: Starting services..." -ForegroundColor Cyan

$AuthServerPath = Join-Path $ScriptRoot "../XYPortal.AuthServer"
$HttpApiHostPath = Join-Path $ScriptRoot "../XYPortal.HttpApi.Host"
$WebHostPath = $ScriptRoot

Test-ProjectExists -Path $AuthServerPath -ProjectName "XYPortal.AuthServer"
Test-ProjectExists -Path $HttpApiHostPath -ProjectName "XYPortal.HttpApi.Host"
Test-ProjectExists -Path $WebHostPath -ProjectName "XYPortal.Web.Host"

$runningProcesses = @{}

try {
    # Start AuthServer
    Write-Host ""
    Write-Host "[AuthServer] Starting dotnet run..." -ForegroundColor Yellow
    $authProc = Start-Process -FilePath "dotnet" -ArgumentList "run" -NoNewWindow -PassThru -WorkingDirectory $AuthServerPath
    $runningProcesses["AuthServer"] = $authProc
    Write-Host "[AuthServer] Started (PID: $($authProc.Id))" -ForegroundColor Green
    
    # Wait for AuthServer to be ready (port 44367)
    $authReady = Wait-ForServiceReady -Port 44367 -ServiceName "AuthServer" -TimeoutSeconds 60
    if (-not $authReady) {
        Write-Host ""
        Write-Host "ERROR: AuthServer failed to start within timeout" -ForegroundColor Red
        Stop-AllServices -Processes $runningProcesses
        exit 1
    }
    
    # Start HttpApi.Host
    Write-Host ""
    Write-Host "[HttpApi.Host] Starting dotnet run..." -ForegroundColor Yellow
    $httpApiProc = Start-Process -FilePath "dotnet" -ArgumentList "run" -NoNewWindow -PassThru -WorkingDirectory $HttpApiHostPath
    $runningProcesses["HttpApi.Host"] = $httpApiProc
    Write-Host "[HttpApi.Host] Started (PID: $($httpApiProc.Id))" -ForegroundColor Green
    
    # Wait for HttpApi.Host to be ready (port 44373)
    $httpApiReady = Wait-ForServiceReady -Port 44373 -ServiceName "HttpApi.Host" -TimeoutSeconds 60
    if (-not $httpApiReady) {
        Write-Host ""
        Write-Host "ERROR: HttpApi.Host failed to start within timeout" -ForegroundColor Red
        Stop-AllServices -Processes $runningProcesses
        exit 1
    }
    
    # Only after both AuthServer and HttpApi.Host are ready, start Web.Host
    Write-Host ""
    Write-Host "[Web.Host] Starting dotnet run..." -ForegroundColor Yellow
    $webHostProc = Start-Process -FilePath "dotnet" -ArgumentList "run" -NoNewWindow -PassThru -WorkingDirectory $WebHostPath
    $runningProcesses["Web.Host"] = $webHostProc
    Write-Host "[Web.Host] Started (PID: $($webHostProc.Id))" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "All three services are running." -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Type /q and press Enter to stop all services and exit." -ForegroundColor Yellow
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
$checkInterval = 2
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
            
            if ($exitCode -ne 0 -and $exitCode -ne $null) {
                $anyFailure = $true
                $failureMessage = "$name exited with code $exitCode"
            }
            
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
