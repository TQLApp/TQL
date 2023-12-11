. "$PSScriptRoot\Include.ps1"

################################################################################
# SUPPORT
################################################################################

Function Copy-Output([string]$From, [string]$Target)
{
    Write-Host "Copying $From"

    [void](New-Item -Type directory $Target)

    Copy-Item "$Global:Src\$From\bin\Release\net8.0-windows\*" -Destination $Target -Recurse
}

Function Copy-Packages([string]$Target)
{
    Write-Host "Copying NuGet packages"

    [void](New-Item -Type directory $Target)

    Get-ChildItem -Path $Global:Src -Recurse -Filter "*.nupkg" | Copy-Item -Destination $Target -Force
}

################################################################################
# ENTRY POINT
################################################################################

Prepare-Directory -Path $Global:Distrib

cd $Global:Root

dotnet restore

# Setting version to a high number to prevent automatic updates from
# overwriting the locally built version.

dotnet build -c Release -property:Version=999.0

Copy-Output -From "Tql.App" -Target "$Global:Distrib\App"
Copy-Output -From "Tql.DebugApp" -Target "$Global:Distrib\DebugApp"

Copy-Packages -Target "$Global:Distrib\Packages"
