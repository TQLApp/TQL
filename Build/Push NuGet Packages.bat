@echo off

pushd "%~dp0"

dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Abstractions\bin\Debug\Tql.Abstractions.*.nupkg"
dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Utilities\bin\Debug\Tql.Utilities.*.nupkg"
dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Plugins.Azure\bin\Debug\*.nupkg"
dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Plugins.AzureDevOps\bin\Debug\*.nupkg"
dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Plugins.Confluence\bin\Debug\*.nupkg"
dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Plugins.Demo\bin\Debug\*.nupkg"
dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Plugins.GitHub\bin\Debug\*.nupkg"
dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Plugins.Jira\bin\Debug\*.nupkg"
dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Plugins.MicrosoftTeams\bin\Debug\*.nupkg"

pause

popd
