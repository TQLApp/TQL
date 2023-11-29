################################################################################
# SUPPORT
################################################################################

Function Get-Script-Directory
{
    $Scope = 1
    
    while ($True)
    {
        $Invoction = (Get-Variable MyInvocation -Scope $Scope).Value
        
        if ($Invoction.MyCommand.Path -Ne $Null)
        {
            Return Split-Path $Invoction.MyCommand.Path
        }
        
        $Scope = $Scope + 1
    }
}

Function Prepare-Directory([string]$Path)
{
    if (Test-Path -Path $Path)
    {
        Remove-Item -Recurse -Force $Path
    }

    [void](New-Item -Type directory $Path)
}

Function Copy-Output([string]$From, [string]$Target)
{
    Write-Host "Copying $From"

    [void](New-Item -Type directory $Target)

    Copy-Item ($Global:Root + "\" + $From + "\bin\Release\net8.0-windows\*") -Destination $Target -Recurse
}

################################################################################
# ENTRY POINT
################################################################################

$Global:Root = (Get-Item (Get-Script-Directory)).Parent.FullName
$Global:Distrib = $Global:Root + "\Build\Distrib"

Prepare-Directory -Path $Global:Distrib

Set-Location $Global:Root

dotnet restore

# Setting version to a high number to prevent automatic updates from
# overwriting the locally built version.

dotnet build -c Release -property:Version=999.0

Copy-Output -From "Tql.App" -Target "$Global:Distrib\App"
Copy-Output -From "Tql.DebugApp" -Target "$Global:Distrib\DebugApp"
