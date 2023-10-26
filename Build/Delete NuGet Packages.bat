@echo off

pushd "%~dp0"

del "..\Tql.Abstractions\bin\Release\Tql.Abstractions.*.nupkg"
del "..\Tql.Utilities\bin\Release\Tql.Utilities.*.nupkg"
del "..\Tql.Plugins.Azure\bin\Release\*.nupkg"
del "..\Tql.Plugins.AzureDevOps\bin\Release\*.nupkg"
del "..\Tql.Plugins.Confluence\bin\Release\*.nupkg"
del "..\Tql.Plugins.Demo\bin\Release\*.nupkg"
del "..\Tql.Plugins.GitHub\bin\Release\*.nupkg"
del "..\Tql.Plugins.Jira\bin\Release\*.nupkg"
del "..\Tql.Plugins.MicrosoftTeams\bin\Release\*.nupkg"

pause

popd
