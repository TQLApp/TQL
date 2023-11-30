@echo off

pushd "%~dp0"

powershell -executionpolicy bypass -file ./Local-Build.ps1

pause

popd
