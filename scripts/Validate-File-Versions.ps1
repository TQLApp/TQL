>>>>>CM:GREEN
param(
>>>>>CM:GREEN
    [Parameter(Mandatory = $true)][string]$Version
>>>>>CM:GREEN
)

>>>>>CM:GREEN
. "$PSScriptRoot\Include.ps1"

>>>>>CM:GREEN
################################################################################
>>>>>CM:GREEN
# SUPPORT
>>>>>CM:GREEN
################################################################################


>>>>>CM:GREEN
Function Prepare-Directory([string]$Path)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    if (Test-Path -Path $Path)
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        Remove-Item -Recurse -Force $Path
>>>>>CM:GREEN
    }

>>>>>CM:GREEN
    [void](New-Item -Type directory $Path)
>>>>>CM:GREEN
}

>>>>>CM:GREEN
Function Get-Latest-Published-Version
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    $Response = Invoke-RestMethod "https://api.github.com/repos/TQLApp/TQL/releases/latest" -Headers @{ Accept = "application/vnd.github.v3+json" }

>>>>>CM:GREEN
    $Version = $Response.tag_name

>>>>>CM:GREEN
    if (-not $Version.StartsWith("v"))
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        Write-Error "Unexpected version format '$Version'"
>>>>>CM:GREEN
        exit 1
>>>>>CM:GREEN
    }

>>>>>CM:GREEN
    return $Version.Substring(1)
>>>>>CM:GREEN
}

>>>>>CM:GREEN
Function Download-Release-Files([string]$Version)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    $Msi = "$Global:Distrib\VersionCheck\TQLApp.msi"
>>>>>CM:GREEN
    $Target = "$Global:Distrib\VersionCheck\$Version"

>>>>>CM:GREEN
    Write-Host "Downloading $Url"

>>>>>CM:GREEN
    try {
>>>>>CM:GREEN
        $Global:WebClient.DownloadFile("https://github.com/TQLApp/TQL/releases/download/v$Version/TQLApp.msi", $Msi)
>>>>>CM:GREEN
    }
>>>>>CM:GREEN
    catch {
>>>>>CM:GREEN
        $Global:WebClient.DownloadFile("https://github.com/TQLApp/TQL/releases/download/v$Version/Tql-$Version.msi", $Msi)
>>>>>CM:GREEN
    }

>>>>>CM:GREEN
    Write-Host "Extracting $Msi"

>>>>>CM:GREEN
    $Process = Start-Process `
>>>>>CM:GREEN
        -NoNewWindow `
>>>>>CM:GREEN
        -Wait `
>>>>>CM:GREEN
        -FilePath "msiexec" `
>>>>>CM:GREEN
        -ArgumentList "/a `"$Msi`" /qn TARGETDIR=`"$Target`"" `
>>>>>CM:GREEN
        -PassThru

>>>>>CM:GREEN
    if ($Process.ExitCode -ne 0)
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        Write-Error "Extracting MSI failed"
>>>>>CM:GREEN
        exit 1
>>>>>CM:GREEN
    }

>>>>>CM:GREEN
    return $Target
>>>>>CM:GREEN
}

>>>>>CM:GREEN
Function Get-Version([string]$Path)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    $VersionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($Path)

>>>>>CM:GREEN
    return New-Object System.Version $VersionInfo.FileMajorPart, $VersionInfo.FileMinorPart, $VersionInfo.FileBuildPart, $VersionInfo.FilePrivatePart
>>>>>CM:GREEN
}

>>>>>CM:GREEN
Function Get-BinPath([string]$Path)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    $BinPath = "$Path\TQL"
>>>>>CM:GREEN
    if (Test-Path $BinPath)
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        return $BinPath
>>>>>CM:GREEN
    }

>>>>>CM:GREEN
    $BinPath = "$Path\Programs\TQL"
>>>>>CM:GREEN
    if (Test-Path $BinPath)
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        return $BinPath
>>>>>CM:GREEN
    }

>>>>>CM:GREEN
    Write-Error "Cannot resolve bin path"
>>>>>CM:GREEN
    exit 1
>>>>>CM:GREEN
}

>>>>>CM:GREEN
Function Compare-Folders([string]$Old, [string]$New)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    $HadError = $false

>>>>>CM:GREEN
    foreach ($OldItem in Get-ChildItem $Old -Recurse -File)
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        $Path = $OldItem.FullName.Substring($Old.Length)

>>>>>CM:GREEN
        $NewFile = $New + $Path
        
>>>>>CM:GREEN
        if (Test-Path $NewFile)
>>>>>CM:GREEN
        {
>>>>>CM:GREEN
            $OldVersion = Get-Version -Path $OldItem.FullName
>>>>>CM:GREEN
            $NewVersion = Get-Version -Path $NewFile

>>>>>CM:GREEN
            $Comparison = $OldVersion.CompareTo($NewVersion)

>>>>>CM:GREEN
            if ($Comparison -gt 0)
>>>>>CM:GREEN
            {
>>>>>CM:GREEN
                Write-Host "::error:: '$Path' '$OldVersion' -> '$NewVersion'"
>>>>>CM:GREEN
                $HadError = $true
>>>>>CM:GREEN
            }
>>>>>CM:GREEN
        }
>>>>>CM:GREEN
        else
>>>>>CM:GREEN
        {
>>>>>CM:GREEN
            Write-Host "::warning:: '$Path' is not in the new version"
>>>>>CM:GREEN
        }
>>>>>CM:GREEN
    }

>>>>>CM:GREEN
    if ($HadError)
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        Write-Error "One or more files were downgraded. This will break the users deployment of the MSI because Windows will remove downgraded files instead of installing the old version."
>>>>>CM:GREEN
        exit 1
>>>>>CM:GREEN
    }
>>>>>CM:GREEN
}

>>>>>CM:GREEN
Function Sanity-Check([string]$Path, $Version)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    Validate-Version -Path $Path -Version $Version -File "Tql.App.dll"
>>>>>CM:GREEN
    Validate-Version -Path $Path -Version $Version -File "Tql.App.exe"
>>>>>CM:GREEN
    Validate-Version -Path $Path -Version $Version -File "Tql.Abstractions.dll"
>>>>>CM:GREEN
    Validate-Version -Path $Path -Version $Version -File "Tql.Utilities.dll"
>>>>>CM:GREEN
}

>>>>>CM:GREEN
Function Validate-Version([string]$Path, $Version, [string]$File)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    $FullName = "$Path\$File"
    
>>>>>CM:GREEN
    if (-not (Test-Path $FullName))
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        Write-Error "Cannot find '$FullName'"
>>>>>CM:GREEN
        exit 1
>>>>>CM:GREEN
    }

>>>>>CM:GREEN
    $FileVersion = Get-Version -Path $FullName

>>>>>CM:GREEN
    if ($Version.CompareTo($FileVersion) -ne 0)
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        Write-Error "Expected '$FullName' to be version '$Version', but it is '$FileVersion'"
>>>>>CM:GREEN
        exit 1
>>>>>CM:GREEN
    }
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
$Global:WebClient = New-Object System.Net.WebClient

>>>>>CM:GREEN
Prepare-Directory -Path "$Global:Distrib\VersionCheck"

>>>>>CM:GREEN
$OldVersion = Get-Latest-Published-Version
>>>>>CM:GREEN
$OldFiles = Download-Release-Files -Version $OldVersion
>>>>>CM:GREEN
$NewFiles = "$Global:Src\Tql.App\bin\Release\net8.0-windows\win-x64\publish"

>>>>>CM:GREEN
Compare-Folders -Old (Get-BinPath -Path $OldFiles) -New $NewFiles

>>>>>CM:GREEN
# System.Version interprets missing components different from components
>>>>>CM:GREEN
# that are 0. We pad the incoming version with zeroes to ensure the
>>>>>CM:GREEN
# version comparison works correctly.

>>>>>CM:GREEN
while ($Version.Split('.').Length -lt 4)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    $Version += ".0"
>>>>>CM:GREEN
}

>>>>>CM:GREEN
$ParsedVersion = New-Object System.Version $Version

>>>>>CM:GREEN
Sanity-Check -Path $NewFiles -Version $ParsedVersion
