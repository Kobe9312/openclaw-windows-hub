@echo off
setlocal

echo ===================================
echo  Clawdbot Windows Tray - Build
echo ===================================
echo.

:: Detect architecture
if "%PROCESSOR_ARCHITECTURE%"=="ARM64" (
    set RID=win-arm64
) else (
    set RID=win-x64
)

echo Architecture: %RID%
echo.

:: Build
echo [1/3] Building Debug...
dotnet build -c Debug -r %RID%
if errorlevel 1 goto :error

echo.
echo [2/3] Building Release...
dotnet build -c Release -r %RID%
if errorlevel 1 goto :error

echo.
echo [3/3] Publishing self-contained...
dotnet publish -c Release -r %RID% --self-contained -p:PublishSingleFile=true -o publish
if errorlevel 1 goto :error

echo.
echo ===================================
echo  Build complete!
echo  Output: publish\ClawdbotTray.exe
echo  Architecture: %RID%
echo ===================================
goto :end

:error
echo.
echo BUILD FAILED
exit /b 1

:end
endlocal
