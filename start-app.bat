@echo off
echo ==================================
echo Starting LMS Dashboard Application
echo ==================================
echo.

echo [1/2] Starting Backend API...
start "Backend API" cmd /k "cd src\Lms.Api && dotnet run"
echo Backend API starting on http://localhost:5225
timeout /t 5 /nobreak >nul

echo.
echo [2/2] Starting Frontend...
start "Frontend UI" cmd /k "cd client && npm run dev"
echo Frontend starting on http://localhost:3000

echo.
echo ==================================
echo Application Started Successfully!
echo ==================================
echo.
echo Backend API:  http://localhost:5225
echo Frontend UI:  http://localhost:3000
echo API Docs:     http://localhost:5225/swagger
echo.
echo Wait 10-15 seconds for both servers to fully start,
echo then open http://localhost:3000 in your browser.
echo.
echo To test the API, run: powershell -ExecutionPolicy Bypass -File test-app.ps1
echo.
pause

