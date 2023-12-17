. "$PSScriptRoot\Include.ps1"

################################################################################
# SUPPORT
################################################################################

Function Copy-Output([string]$From, [string]$Target)
{
    Write-Host "Copying $From"

    Copy-Item "$Global:Src\$From\bin\Release\net8.0-windows\win-x64\publish" -Destination $Target -Recurse
    
    Get-ChildItem $Target
}

Function Ensure-WiX
{
    $Url = "https://github.com/wixtoolset/wix3/releases/download/wix3112rtm/wix311-binaries.zip"
    $Target = "$env:TEMP\wix3112rtm"

    if (-not (Test-Path -Path $Target))
    {
        Write-Host "Downloading WiX"

        Invoke-WebRequest -Uri $Url -OutFile "$Target.zip"
        Expand-Archive -Path "$Target.zip" -DestinationPath $Target
    }

    $Global:WiXRoot = $Target
}

Function Run-WiX-Command([string]$File, [string]$ArgumentList, [string]$WorkingDirectory)
{
    Write-Host "Executing $File $ArgumentList"

    $Process = Start-Process `
        -NoNewWindow `
        -Wait `
        -FilePath "$Global:WiXRoot\$File" `
        -ArgumentList $ArgumentList `
        -WorkingDirectory $WorkingDirectory `
        -PassThru
    
    if ($Process.ExitCode -ne 0)
    {
        Write-Host "Command failed with exit code $($Process.ExitCode)"
        exit $Process.ExitCode
    }
}

Function Get-App-Version
{
    $Content = [System.IO.File]::ReadAllText("$Global:Scripts\Version.wxi")

    if (-not ($Content -match "Version=`"(.*?)`""))
    {
        Write-Host "Could not parse Version.wxi"
        exit 1
    }

    return $Matches[1]
}

Function Create-MSI
{
    Write-Host "Collecting source files"

    Run-WiX-Command `
        -File "heat.exe" `
        -ArgumentList "dir . -nologo -v -dr INSTALLFOLDER -srd -cg MyAppComponents -gg -sfrag -scom -sreg -out ..\AppFiles.wxs" `
        -WorkingDirectory "$Global:Distrib\SourceDir"

    Get-Content "$Global:Distrib\AppFiles.wxs"

    Write-Host "Compiling Tql.wxs"

    Run-WiX-Command `
        -File "candle.exe" `
        -ArgumentList "-nologo -v $Global:Scripts\Tql.wxs" `
        -WorkingDirectory $Global:Distrib

    Write-Host "Compiling AppFiles.wxs"

    Run-WiX-Command `
        -File "candle.exe" `
        -ArgumentList "-nologo -v $Global:Distrib\AppFiles.wxs" `
        -WorkingDirectory $Global:Distrib

    $Version = Get-App-Version
    $Output = "TQLApp.msi"

    Write-Host "Linking $Output"

    Run-WiX-Command `
        -File "light.exe" `
        -ArgumentList "-nologo -v Tql.wixobj AppFiles.wixobj -ext WixUIExtension -o $Output -sice:ICE91 -sice:ICE64 -sice:ICE38" `
        -WorkingDirectory $Global:Distrib
}

################################################################################
# ENTRY POINT
################################################################################

$Global:DefaultBuildTarget = "Build"

Prepare-Directory $Global:Distrib

Copy-Output -From "Tql.App" -Target "$Global:Distrib\SourceDir"

Ensure-WiX

Create-MSI
