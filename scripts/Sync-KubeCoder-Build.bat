@echo off
setlocal

rem Copies the Windows build a KubeCoder environment publishes (kc project build
rem writes it to build\DebugApp) into %TEMP%\TQL-DebugApp, because Windows won't
rem run it from the network share. Run this script from the share:
rem
rem   \\<environment>.home\work\TQL\scripts\Sync-KubeCoder-Build.bat
rem
rem or pass the build folder as the first argument. Close Tql.DebugApp first;
rem files it has open can't be replaced.

set "SOURCE=%~1"
if "%SOURCE%"=="" set "SOURCE=%~dp0..\build\DebugApp"
set "TARGET=%TEMP%\TQL-DebugApp"

if not exist "%SOURCE%\Tql.DebugApp.exe" (
    echo No build found in "%SOURCE%".
    echo Run kc project build in the environment, or pass the build folder.
    exit /b 1
)

rem /MIR makes the target an exact copy, removing files the build no longer has.
robocopy "%SOURCE%" "%TARGET%" /MIR /MT:16 /R:2 /W:1 /NFL /NDL /NJH /NP
if errorlevel 8 (
    echo Copying failed. Is Tql.DebugApp still running?
    exit /b 1
)

echo.
echo Synchronized to %TARGET%. Start it with:
echo   "%TARGET%\Tql.DebugApp.exe" --env Debug
exit /b 0
