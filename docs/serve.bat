@echo off

pushd "%~dp0"

dotnet docfx docfx.json --serve

popd
