param(
    [Parameter(Mandatory = $true)][string]$Version
)

. "$PSScriptRoot\Include.ps1"

################################################################################
# SUPPORT
################################################################################


Function Prepare-Directory([string]$Path)
{
    if (Test-Path -Path $Path)
    {
        Remove-Item -Recurse -Force $Path
    }

    [void](New-Item -Type directory $Path)
}

Function Get-Latest-Published-Version
{
    $Response = Invoke-RestMethod "https://api.github.com/repos/TQLApp/TQL/releases/latest" -Headers @{ Accept = "application/vnd.github.v3+json" }

    $Version = $Response.tag_name

    if (-not $Version.StartsWith("v"))
    {
        Write-Error "Unexpected version format '$Version'"
        exit 1
    }

    return $Version.Substring(1)
}

Function Download-Release-Files([string]$Version)
{
    $Msi = "$Global:Distrib\VersionCheck\Tql.msi"
    $Target = "$Global:Distrib\VersionCheck\$Version"
    $Url = "https://github.com/TQLApp/TQL/releases/download/v$Version/Tql-$Version.msi"

    Write-Host "Downloading $Url"

    $Global:WebClient.DownloadFile($Url, $Msi)

    Write-Host "Extracting $Msi"

    $Process = Start-Process `
        -NoNewWindow `
        -Wait `
        -FilePath "msiexec" `
        -ArgumentList "/a `"$Msi`" /qn TARGETDIR=`"$Target`"" `
        -PassThru

    if ($Process.ExitCode -ne 0)
    {
        Write-Error "Extracting MSI failed"
        exit 1
    }

    return $Target
}

Function Get-Version([string]$Path)
{
    $VersionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($Path)

    return New-Object System.Version $VersionInfo.FileMajorPart, $VersionInfo.FileMinorPart, $VersionInfo.FileBuildPart, $VersionInfo.FilePrivatePart
}

Function Get-BinPath([string]$Path)
{
    $BinPath = "$Path\TQL"
    if (Test-Path $BinPath)
    {
        return $BinPath
    }

    $BinPath = "$Path\Programs\TQL"
    if (Test-Path $BinPath)
    {
        return $BinPath
    }

    Write-Error "Cannot resolve bin path"
    exit 1
}

Function Compare-Folders([string]$Old, [string]$New)
{
    $HadError = $false

    foreach ($OldItem in Get-ChildItem $Old -Recurse -File)
    {
        $Path = $OldItem.FullName.Substring($Old.Length)

        $NewFile = $New + $Path
        
        if (Test-Path $NewFile)
        {
            $OldVersion = Get-Version -Path $OldItem.FullName
            $NewVersion = Get-Version -Path $NewFile

            $Comparison = $OldVersion.CompareTo($NewVersion)

            if ($Comparison -gt 0)
            {
                Write-Host "::error:: '$Path' '$OldVersion' -> '$NewVersion'"
                $HadError = $true
            }
        }
        else
        {
            Write-Host "::warning:: '$Path' is not in the new version"
        }
    }

    if ($HadError)
    {
        Write-Error "One or more files were downgraded. This will break the users deployment of the MSI because Windows will remove downgraded files instead of installing the old version."
        exit 1
    }
}

Function Sanity-Check([string]$Path, $Version)
{
    Validate-Version -Path $Path -Version $Version -File "Tql.App.dll"
    Validate-Version -Path $Path -Version $Version -File "Tql.App.exe"
    Validate-Version -Path $Path -Version $Version -File "Tql.Abstractions.dll"
    Validate-Version -Path $Path -Version $Version -File "Tql.Utilities.dll"
}

Function Validate-Version([string]$Path, $Version, [string]$File)
{
    $FullName = "$Path\$File"
    
    if (-not (Test-Path $FullName))
    {
        Write-Error "Cannot find '$FullName'"
        exit 1
    }

    $FileVersion = Get-Version -Path $FullName

    if ($Version.CompareTo($FileVersion) -ne 0)
    {
        Write-Error "Expected '$FullName' to be version '$Version', but it is '$FileVersion'"
        exit 1
    }
}

################################################################################
# ENTRY POINT
################################################################################

$Global:DefaultBuildTarget = "Build"
$Global:WebClient = New-Object System.Net.WebClient

Prepare-Directory -Path "$Global:Distrib\VersionCheck"

$OldVersion = Get-Latest-Published-Version
$OldFiles = Download-Release-Files -Version $OldVersion
$NewFiles = "$Global:Src\Tql.App\bin\Release\net8.0-windows\win-x64\publish"

Compare-Folders -Old (Get-BinPath -Path $OldFiles) -New $NewFiles

# System.Version interprets missing components different from components
# that are 0. We pad the incoming version with zeroes to ensure the
# version comparison works correctly.

while ($Version.Split('.').Length -lt 4)
{
    $Version += ".0"
}

$ParsedVersion = New-Object System.Version $Version

Sanity-Check -Path $NewFiles -Version $ParsedVersion
