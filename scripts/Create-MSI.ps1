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
    Copy-Item "$Global:Src\$From\bin\Release\net8.0-windows\win-x64\publish" -Destination $Target -Recurse
    
>>>>>CM:GREEN
    Get-ChildItem $Target
>>>>>CM:GREEN
}

>>>>>CM:GREEN
Function Ensure-WiX
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    $Url = "https://github.com/wixtoolset/wix3/releases/download/wix3112rtm/wix311-binaries.zip"
>>>>>CM:GREEN
    $Target = "$env:TEMP\wix3112rtm"

>>>>>CM:GREEN
    if (-not (Test-Path -Path $Target))
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        Write-Host "Downloading WiX"

>>>>>CM:GREEN
        Invoke-WebRequest -Uri $Url -OutFile "$Target.zip"
>>>>>CM:GREEN
        Expand-Archive -Path "$Target.zip" -DestinationPath $Target
>>>>>CM:GREEN
    }

>>>>>CM:GREEN
    $Global:WiXRoot = $Target
>>>>>CM:GREEN
}

>>>>>CM:GREEN
Function Run-WiX-Command([string]$File, [string]$ArgumentList, [string]$WorkingDirectory)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    Write-Host "Executing $File $ArgumentList"

>>>>>CM:GREEN
    $Process = Start-Process `
>>>>>CM:GREEN
        -NoNewWindow `
>>>>>CM:GREEN
        -Wait `
>>>>>CM:GREEN
        -FilePath "$Global:WiXRoot\$File" `
>>>>>CM:GREEN
        -ArgumentList $ArgumentList `
>>>>>CM:GREEN
        -WorkingDirectory $WorkingDirectory `
>>>>>CM:GREEN
        -PassThru
    
>>>>>CM:GREEN
    if ($Process.ExitCode -ne 0)
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        Write-Host "Command failed with exit code $($Process.ExitCode)"
>>>>>CM:GREEN
        exit $Process.ExitCode
>>>>>CM:GREEN
    }
>>>>>CM:GREEN
}

>>>>>CM:GREEN
Function Create-MSI
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    Write-Host "Collecting source files"

>>>>>CM:GREEN
    Run-WiX-Command `
>>>>>CM:GREEN
        -File "heat.exe" `
>>>>>CM:GREEN
        -ArgumentList "dir . -nologo -v -dr INSTALLFOLDER -srd -cg MyAppComponents -gg -sfrag -scom -sreg -out ..\AppFiles.wxs" `
>>>>>CM:GREEN
        -WorkingDirectory "$Global:Distrib\SourceDir"

>>>>>CM:GREEN
    Get-Content "$Global:Distrib\AppFiles.wxs"

>>>>>CM:GREEN
    Write-Host "Compiling Tql.wxs"

>>>>>CM:GREEN
    Run-WiX-Command `
>>>>>CM:GREEN
        -File "candle.exe" `
>>>>>CM:GREEN
        -ArgumentList "-nologo -v $Global:Scripts\Tql.wxs" `
>>>>>CM:GREEN
        -WorkingDirectory $Global:Distrib

>>>>>CM:GREEN
    Write-Host "Compiling AppFiles.wxs"

>>>>>CM:GREEN
    Run-WiX-Command `
>>>>>CM:GREEN
        -File "candle.exe" `
>>>>>CM:GREEN
        -ArgumentList "-nologo -v $Global:Distrib\AppFiles.wxs" `
>>>>>CM:GREEN
        -WorkingDirectory $Global:Distrib

>>>>>CM:GREEN
    $Output = "TQLApp.msi"

>>>>>CM:GREEN
    Write-Host "Linking $Output"

>>>>>CM:GREEN
    Run-WiX-Command `
>>>>>CM:GREEN
        -File "light.exe" `
>>>>>CM:GREEN
        -ArgumentList "-nologo -v Tql.wixobj AppFiles.wixobj -ext WixUIExtension -o $Output -sice:ICE91 -sice:ICE64 -sice:ICE38" `
>>>>>CM:GREEN
        -WorkingDirectory $Global:Distrib
>>>>>CM:GREEN
}

>>>>>CM:GREEN
################################################################################
>>>>>CM:GREEN
# ENTRY POINT
>>>>>CM:GREEN
################################################################################

>>>>>CM:GREEN
$Global:DefaultBuildTarget = "Build"

>>>>>CM:GREEN
Prepare-Directory $Global:Distrib

>>>>>CM:GREEN
Copy-Output -From "Tql.App" -Target "$Global:Distrib\SourceDir"

>>>>>CM:GREEN
Ensure-WiX

>>>>>CM:GREEN
Create-MSI
