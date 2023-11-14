@echo off

pushd "%~dp0"

powershell -executionpolicy bypass -file ./Build.ps1

pause

popd
