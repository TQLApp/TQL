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

Function Console-Update-Status([string]$Status, [string]$ForegroundColor)
{
    $WindowWidth = (Get-Host).UI.RawUI.WindowSize.Width
    
    $Position = (Get-Host).UI.RawUI.CursorPosition
    
    $Position.X = $WindowWidth - ($Status.Length + 1)
    $Position.Y = $Position.Y - 1
    
    if ($Position.X -ge 0 -and $Position.Y -ge 0)
    {
        (Get-Host).UI.RawUI.CursorPosition = $Position
    }
    
    Write-Host $Status -ForegroundColor $ForegroundColor
}

Function Prepare-Directory([string]$Path)
{
    if (Test-Path -Path $Path)
    {
        Remove-Item -Recurse -Force $Path
    }

    [void](New-Item -Type directory $Path)
}

Function Get-MSBuild
{
    Write-Host "Finding MSBuild"

    if (Get-Module VSSetup -ListAvailable)
    {
        Import-Module VSSetup
    }
    else
    {
        Install-Module VSSetup -Scope CurrentUser
    }

    $vsInstances = Get-VSSetupInstance
    $vs = @($vsInstances | Select-VSSetupInstance -Require Microsoft.Component.MSBuild)
    if ($vs)
    {
        $MSBuild = Join-Path ($vs[0].InstallationPath) MSBuild\Current\Bin\MSBuild.exe
    }
    else
    {
        $vs = @($vsInstances | Select-VSSetupInstance -Product Microsoft.VisualStudio.Product.BuildTools)
        if ($vs)
        {
            $MSBuild = Join-Path ($vs[0].InstallationPath) MSBuild\Current\Bin\MSBuild.exe
        }
        else
        {
            throw "Cannot find MSBuild";
        }
    }

    Console-Update-Status "[OK]" -ForegroundColor Green

    return $MSBuild
}

Function Build-Solution([string]$Solution, [string]$Target = $Null, [string]$Configuration = "Release", [string]$Platform = "Any CPU", [bool]$Skip = $False, [string]$ExtraProperties = $Null, [string]$SolutionDisplay = $Null)
{
    if ($Target -eq $Null -or $Target -eq "")
    {
        $Target = $Global:DefaultBuildTarget
    }
    if ($SolutionDisplay -eq "")
    {
        $SolutionDisplay = [System.IO.Path]::GetFileName($Solution)
    }
    
    $Status = "Building " + $SolutionDisplay + " ($Configuration"
    
    if ($Target -ne $DefaultBuildTarget)
    {
        $Status = $Status + ", " + $Target
    }
    
    $Status = $Status + ")"

    Write-Host $Status
    
    if ($Skip)
    {
        Console-Update-Status "[SKIPPED]" -ForegroundColor Yellow
    }
    else
    {
        $Properties = "Platform=`"$Platform`";Configuration=`"$Configuration`";NoWarn=`"1591,1587,1573,0436`""
        
        if ($ExtraProperties -ne $Null -and $ExtraProperties -ne "")
        {
            $Properties = $Properties + ";" + $ExtraProperties
        }
        
        # Perform the build. Note that warnings CS1591, CS1587 and CS1573 are the warnings for missing
        # XML documentation. Warning CS0436 is because we need to override a framework
        # class.
        
        $Arguments = "/nr:false /m /verbosity:quiet /nologo `"/t:$Target`" /p:$Properties `"$Solution`""
        
        $Process = Start-Process -NoNewWindow -Wait -FilePath $Global:MSBuild -ArgumentList $Arguments -PassThru
        
        if ($Process.ExitCode -eq 0)
        {
            Console-Update-Status "[OK]" -ForegroundColor Green
        }
        else
        {
            Console-Update-Status "[FAILED]" -ForegroundColor Red
            
            # If we get an error from a build, the chances that we'll be able
            # to continue are very dim. Because of this, we bail immediately.

            Exit 2
        }
    }
}

Function Copy-Output([string]$From, [string]$Target)
{
    Write-Host "Copying $From"

    Copy-Item ($Global:Root + "\" + $From + "\bin\Release\net48") -Destination $Target -Recurse

    Console-Update-Status "[OK]" -ForegroundColor Green
}

################################################################################
# ENTRY POINT
################################################################################

$Global:Root = (Get-Item (Get-Script-Directory)).Parent.FullName
$Global:DefaultBuildTarget = "Build"
$Global:Distrib = $Global:Root + "\Build\Distrib"
$Global:MSBuild = Get-MSBuild

Prepare-Directory -Path $Global:Distrib

Build-Solution -Solution ($Global:Root + "\Tql.sln")

Copy-Output -From "Tql.App" -Target $Global:Distrib
