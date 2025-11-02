# LMS Dashboard Startup Script
# This script starts both the backend API and frontend development server

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Starting LMS Dashboard Application" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Start Backend API
Write-Host "[1/2] Starting Backend API..." -ForegroundColor Yellow
$backendPath = Join-Path $PSScriptRoot "src\Lms.Api"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$backendPath'; dotnet run"
Write-Host "✓ Backend API starting on http://localhost:5225" -ForegroundColor Green

Start-Sleep -Seconds 5

# Start Frontend
Write-Host "`n[2/2] Starting Frontend..." -ForegroundColor Yellow
$frontendPath = Join-Path $PSScriptRoot "client"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location -Path '$frontendPath'; npm run dev"
Write-Host "✓ Frontend starting on http://localhost:3000" -ForegroundColor Green

Write-Host "`n==================================" -ForegroundColor Cyan
Write-Host "Application Started Successfully!" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Backend API:  http://localhost:5225" -ForegroundColor White
Write-Host "Frontend UI:  http://localhost:3000" -ForegroundColor White
Write-Host "API Docs:     http://localhost:5225/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Wait 10-15 seconds for both servers to fully start," -ForegroundColor Yellow
Write-Host "then open http://localhost:3000 in your browser." -ForegroundColor Yellow
Write-Host ""
Write-Host "To test the API, run: .\test-app.ps1" -ForegroundColor Cyan
Write-Host ""

