>>>>>CM:GREEN
@echo off

>>>>>CM:GREEN
pushd "%~dp0"

>>>>>CM:GREEN
cd scripts

>>>>>CM:GREEN
powershell -executionpolicy bypass -file ./Run-Prettier.ps1

>>>>>CM:GREEN
if "%TERM_PROGRAM%" neq "vscode" (
>>>>>CM:GREEN
    pause
>>>>>CM:GREEN
)

>>>>>CM:GREEN
popd
