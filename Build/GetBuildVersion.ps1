param($tag)

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

################################################################################
# ENTRY POINT
################################################################################

$Global:Root = (Get-Item (Get-Script-Directory)).Parent.FullName

if (-not ($tag -match "tags/v(.*)"))
{
  Write-Host "Invalid tag ref"
  exit 1
}

$Version = $Matches[1]
$VersionId = [guid]::NewGuid()

$Content = @"
<Include>
  <?define Version="$Version"?>
  <!-- This needs to be changed for every new version -->
  <?define VersionId="$VersionId"?>
</Include>
"@

Set-Content -Path "$Global:Root\Build\Version.wxi" -Value $Content

Write-Output "build-version=$Version" >> $env:GITHUB_OUTPUT
