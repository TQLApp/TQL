@echo off

pushd "%~dp0"

:run

dotnet docfx docfx.json --serve

goto run

popd
