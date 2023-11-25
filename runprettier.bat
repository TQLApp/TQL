@echo off

pushd "%~dp0"

powershell -executionpolicy bypass -file ./Build/Run-Prettier.ps1

pause

popd
