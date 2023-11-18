@echo off

pushd "%~dp0"

docfx docfx.json --serve

popd
