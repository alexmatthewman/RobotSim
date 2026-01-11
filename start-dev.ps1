# RobotSim Development Start Script
# Starts the backend server and frontend client in sequence
# Usage: PowerShell.exe -ExecutionPolicy Bypass -File .\start-dev.ps1

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RobotSim Development Environment Starter" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Paths
$rootDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$serverDir = Join-Path $rootDir "RobotSim.Server"
$clientDir = Join-Path $rootDir "robotsim.client"

# Configuration
$serverPort = 5120
$clientPort = 5173
$serverHost = "127.0.0.1"
$serverUrl = "http://${serverHost}:${serverPort}"
$clientUrl = "http://${serverHost}:${clientPort}"
$maxWaitSeconds = 60

function Test-PortOpen($testHost, $testPort) {
    try {
        $tcp = New-Object System.Net.Sockets.TcpClient
        $iar = $tcp.BeginConnect($testHost, $testPort, $null, $null)
        $success = $iar.AsyncWaitHandle.WaitOne(1000)
        if (-not $success) { return $false }
        $tcp.EndConnect($iar)
        $tcp.Close()
        return $true
    } catch {
        return $false
    }
}

# ============================================
# Start Backend Server (or detect existing)
# ============================================
Write-Host "Starting backend server..." -ForegroundColor Yellow
Write-Host "  Directory: $serverDir" -ForegroundColor Gray
Write-Host "  URL: $serverUrl" -ForegroundColor Gray
Write-Host ""

Push-Location $serverDir

$serverProcess = $null
$serverAlreadyRunning = $false

if (Test-PortOpen -testHost $serverHost -testPort $serverPort) {
    Write-Host "  Port $serverPort is already in use. Assuming backend is already running." -ForegroundColor Yellow
    # Verify health endpoint
    try {
        $resp = Invoke-WebRequest -Uri "$serverUrl/robot/health" -Method Get -UseBasicParsing -TimeoutSec 3 -ErrorAction Stop
        if ($resp.StatusCode -eq 200) {
            Write-Host "  ✓ Detected running backend responding to health probe." -ForegroundColor Green
            $serverAlreadyRunning = $true
        } else {
            Write-Host "  ✗ Port in use but health probe failed (status $($resp.StatusCode))." -ForegroundColor Red
            Pop-Location
            exit 1
        }
    } catch {
        Write-Host "  ✗ Port in use but health probe failed: $($_.Exception.Message)" -ForegroundColor Red
        Pop-Location
        exit 1
    }
} else {
    # Start backend
    $serverProcess = Start-Process -FilePath "dotnet" -ArgumentList "run", "--launch-profile", "http" -PassThru -NoNewWindow
    Write-Host "  Process ID: $($serverProcess.Id)" -ForegroundColor Gray

    # Wait for server to be ready
    Write-Host "  Waiting for server to respond..." -ForegroundColor Gray
    $elapsed = 0
    $serverReady = $false

    while ($elapsed -lt $maxWaitSeconds) {
        try {
            $response = Invoke-WebRequest -Uri "$serverUrl/robot/health" -Method Get -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
            if ($response.StatusCode -eq 200) {
                $serverReady = $true
                Write-Host "  ✓ Server is ready!" -ForegroundColor Green
                break
            }
        } catch {
            # Not ready yet, wait and retry
        }
        Start-Sleep -Seconds 1
        $elapsed += 1
        if ($elapsed % 5 -eq 0) { Write-Host "  Waiting... ($elapsed/$maxWaitSeconds seconds)" -ForegroundColor Gray }
    }

    if (-not $serverReady) {
        Write-Host "  ✗ Server failed to start within $maxWaitSeconds seconds" -ForegroundColor Red
        Write-Host "  Check the server console for errors." -ForegroundColor Red
        Pop-Location
        exit 1
    }
}

Pop-Location
Write-Host ""

# ============================================
# Start Frontend Client
# ============================================
Write-Host "Starting frontend client..." -ForegroundColor Yellow
Write-Host "  Directory: $clientDir" -ForegroundColor Gray
Write-Host "  URL: $clientUrl" -ForegroundColor Gray
Write-Host ""

Push-Location $clientDir

# Locate npm executable
$npmCmd = $null
try { $cmd = Get-Command npm -ErrorAction Stop; $npmCmd = $cmd.Source } catch {}

# Check if node_modules exists, if not run npm install
if (-not (Test-Path "node_modules")) {
    Write-Host "  Installing npm dependencies..." -ForegroundColor Yellow
    try {
        if ($npmCmd) { & "$npmCmd" install } else { Start-Process -FilePath $env:ComSpec -ArgumentList "/c npm install" -WorkingDirectory $clientDir -Wait -NoNewWindow }
        if ($LASTEXITCODE -ne 0) { Write-Host "  ✗ npm install failed" -ForegroundColor Red; Pop-Location; exit 1 }
        Write-Host "  ✓ Dependencies installed" -ForegroundColor Green
    } catch { Write-Host "  ✗ npm install failed: $($_.Exception.Message)" -ForegroundColor Red; Pop-Location; exit 1 }
}

# Start the dev server.
try {
    $comSpec = $env:ComSpec
    $clientProcess = Start-Process -FilePath $comSpec -ArgumentList "/c npm run dev" -WorkingDirectory $clientDir -PassThru
    Write-Host "  Process ID: $($clientProcess.Id)" -ForegroundColor Gray
} catch { Write-Host "  ✗ Failed to start npm dev server: $($_.Exception.Message)" -ForegroundColor Red; Pop-Location; exit 1 }

# Wait for client to be ready
Write-Host "  Waiting for client to respond..." -ForegroundColor Gray
$elapsed = 0
$clientReady = $false

while ($elapsed -lt $maxWaitSeconds) {
    try {
        $response = Invoke-WebRequest -Uri "$clientUrl/" -Method Get -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            $clientReady = $true
            Write-Host "  ✓ Client is ready!" -ForegroundColor Green
            break
        }
    } catch { }
    Start-Sleep -Seconds 1
    $elapsed += 1
    if ($elapsed % 5 -eq 0) { Write-Host "  Waiting... ($elapsed/$maxWaitSeconds seconds)" -ForegroundColor Gray }
}

if (-not $clientReady) { Write-Host "  ✗ Client failed to start within $maxWaitSeconds seconds" -ForegroundColor Red; Write-Host "  Check the client console for errors." -ForegroundColor Red; Pop-Location; exit 1 }

Pop-Location
Write-Host ""

# Open browser to client URL
try { Start-Process $clientUrl } catch { Write-Host "Could not open browser automatically. Please open $clientUrl manually." -ForegroundColor Yellow }

# ============================================
# Success
# ============================================
Write-Host "========================================" -ForegroundColor Green
Write-Host "✓ Both services are ready!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Backend:  $serverUrl" -ForegroundColor Cyan
Write-Host "Frontend: $clientUrl" -ForegroundColor Cyan
Write-Host ""
Write-Host "Open your browser to: $clientUrl" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C in either console to stop the services." -ForegroundColor Yellow
Write-Host ""

# Keep script running until user interrupts
try {
    while ($true) {
        # Check if processes are still running
        $serverRunning = $null
        if ($serverProcess) { $serverRunning = Get-Process -Id $serverProcess.Id -ErrorAction SilentlyContinue }
        else { $serverRunning = Test-PortOpen -testHost $serverHost -testPort $serverPort }
        $clientRunning = Get-Process -Id $clientProcess.Id -ErrorAction SilentlyContinue
        
        if (-not $serverRunning -or -not $clientRunning) {
            if (-not $serverRunning) { Write-Host "⚠ Server process ended unexpectedly" -ForegroundColor Red }
            if (-not $clientRunning) { Write-Host "⚠ Client process ended unexpectedly" -ForegroundColor Red }
            break
        }
        Start-Sleep -Seconds 2
    }
} catch { Write-Host "Shutting down..." -ForegroundColor Yellow }

# Cleanup
try {
    if ($serverProcess) { Stop-Process -Id $serverProcess.Id -ErrorAction SilentlyContinue }
    Stop-Process -Id $clientProcess.Id -ErrorAction SilentlyContinue
} catch {}

Write-Host "Services stopped." -ForegroundColor Yellow
