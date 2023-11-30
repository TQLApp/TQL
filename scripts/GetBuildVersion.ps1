param($Tag)

. "$PSScriptRoot\Include.ps1"

################################################################################
# SUPPORT
################################################################################

Function Get-Version-From-Tag([string]$Tag)
{
  if ($Tag -match "tags/v(.*)")
  {
    return $Matches[1]
  }
  if ($Tag -match "^v(.*)")
  {
    return $Matches[1]
  }

  return $Null
}

################################################################################
# ENTRY POINT
################################################################################

if ($Tag -eq $null)
{
  $Tags = git for-each-ref --sort=-creatordate --format '%(refname:short)' refs/tags

  $Version = $null

  foreach ($Match in $Tags | Select-String "\S+" -AllMatches)
  {
    $Version = Get-Version-From-Tag $Match
    if ($Version -ne $null)
    {
      break
    }
  }

  if ($Version -eq $null)
  {
    Write-Host "Could not find tag"
    exit 2
  }
}
else
{
  $Version = Get-Version-From-Tag $Tag

  if ($Version -eq $null)
  {
    Write-Host "Invalid tag ref '$Tag'"
    exit 1
  }
}

$VersionId = [guid]::NewGuid()

$Content = @"
<Include>
  <?define Version="$Version"?>
  <!-- This needs to be changed for every new version -->
  <?define VersionId="$VersionId"?>
</Include>
"@

Set-Content -Path "$Global:Scripts\Version.wxi" -Value $Content

Write-Output "build-version=$Version" >> $env:GITHUB_OUTPUT
