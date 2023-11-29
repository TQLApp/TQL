@echo off

rem See https://help.testspace.com/manual/desktop-preview for instructions
rem on how to setup the Desktop Preview environment. Note that Ruby+DevKit
rem is required to be able to install Jekyll.

pushd "%~dp0"

start http://localhost:4000/

call bundle exec jekyll serve

pause

popd
