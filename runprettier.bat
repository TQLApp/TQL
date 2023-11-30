@echo off

pushd "%~dp0"

cd Build

powershell -executionpolicy bypass -file ./Run-Prettier.ps1

if "%TERM_PROGRAM%" neq "vscode" (
    pause
)

popd
