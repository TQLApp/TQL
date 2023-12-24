>>>>>CM:GREEN
@echo off

>>>>>CM:GREEN
pushd "%~dp0"

>>>>>CM:GREEN
powershell -executionpolicy bypass -file ./Local-Build.ps1

>>>>>CM:GREEN
pause

>>>>>CM:GREEN
popd
