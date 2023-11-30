@echo off

pushd "%~dp0"

cd scripts

powershell -executionpolicy bypass -file ./Run-Prettier.ps1

if "%TERM_PROGRAM%" neq "vscode" (
    pause
)

popd
