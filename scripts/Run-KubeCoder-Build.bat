@echo off
setlocal

rem Copies the Windows build a KubeCoder environment publishes (kc project build
rem writes it to build\DebugApp) into %TEMP%\TQL-DebugApp, because Windows won't
rem run it from the network share, and starts it in the Debug environment so it
rem stays apart from an installed TQL. Run this script from the share:
rem
rem   \\<environment>.home\work\TQL\scripts\Run-KubeCoder-Build.bat
rem
rem or pass the build folder as the first argument. A running Tql.DebugApp is
rem killed first, because files it has open can't be replaced.

set "SOURCE=%~1"
if "%SOURCE%"=="" set "SOURCE=%~dp0..\build\DebugApp"
set "TARGET=%TEMP%\TQL-DebugApp"

if not exist "%SOURCE%\Tql.DebugApp.exe" (
    echo No build found in "%SOURCE%".
    echo Run kc project build in the environment, or pass the build folder.
    exit /b 1
)

taskkill /im Tql.DebugApp.exe /f >nul 2>&1

rem /MIR makes the target an exact copy, removing files the build no longer has.
rem The retries give a killed app time to release its files.
robocopy "%SOURCE%" "%TARGET%" /MIR /MT:16 /R:2 /W:1 /NFL /NDL /NJH /NP
if errorlevel 8 (
    echo Copying to "%TARGET%" failed.
    exit /b 1
)

start "" /d "%TARGET%" "%TARGET%\Tql.DebugApp.exe" --env Debug
exit /b 0
