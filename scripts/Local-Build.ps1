>>>>>CM:GREEN
. "$PSScriptRoot\Include.ps1"

>>>>>CM:GREEN
################################################################################
>>>>>CM:GREEN
# SUPPORT
>>>>>CM:GREEN
################################################################################

>>>>>CM:GREEN
Function Copy-Output([string]$From, [string]$Target)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    Write-Host "Copying $From"

>>>>>CM:GREEN
    [void](New-Item -Type directory $Target)

>>>>>CM:GREEN
    Copy-Item "$Global:Src\$From\bin\Release\net8.0-windows\*" -Destination $Target -Recurse
>>>>>CM:GREEN
}

>>>>>CM:GREEN
Function Copy-Packages([string]$Target)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    Write-Host "Copying NuGet packages"

>>>>>CM:GREEN
    [void](New-Item -Type directory $Target)

>>>>>CM:GREEN
    Get-ChildItem -Path $Global:Src -Recurse -Filter "*.nupkg" | Copy-Item -Destination $Target -Force
>>>>>CM:GREEN
}

>>>>>CM:GREEN
################################################################################
>>>>>CM:GREEN
# ENTRY POINT
>>>>>CM:GREEN
################################################################################

>>>>>CM:GREEN
Prepare-Directory -Path $Global:Distrib

>>>>>CM:GREEN
cd $Global:Root

>>>>>CM:GREEN
dotnet restore

>>>>>CM:GREEN
# Setting version to a high number to prevent automatic updates from
>>>>>CM:GREEN
# overwriting the locally built version.

>>>>>CM:GREEN
dotnet build -c Release -property:Version=999.0

>>>>>CM:GREEN
Copy-Output -From "Tql.App" -Target "$Global:Distrib\App"
>>>>>CM:GREEN
Copy-Output -From "Tql.DebugApp" -Target "$Global:Distrib\DebugApp"

>>>>>CM:GREEN
Copy-Packages -Target "$Global:Distrib\Packages"
