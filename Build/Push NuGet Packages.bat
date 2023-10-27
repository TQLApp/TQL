@echo off

pushd "%~dp0"

dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Abstractions\bin\Release\Tql.Abstractions.*.nupkg"
dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Utilities\bin\Release\Tql.Utilities.*.nupkg"
dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Plugins.Azure\bin\Release\*.nupkg"
dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Plugins.AzureDevOps\bin\Release\*.nupkg"
dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Plugins.Confluence\bin\Release\*.nupkg"
dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Plugins.Demo\bin\Release\*.nupkg"
dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Plugins.GitHub\bin\Release\*.nupkg"
dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Plugins.Jira\bin\Release\*.nupkg"
dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Plugins.MicrosoftTeams\bin\Release\*.nupkg"
dotnet nuget push --source "TQLPlugins" --api-key az  --skip-duplicate "..\Tql.Plugins.Outlook\bin\Release\*.nupkg"

pause

popd
