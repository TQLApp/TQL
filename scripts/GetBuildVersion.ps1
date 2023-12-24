>>>>>CM:GREEN
param($Tag)

>>>>>CM:GREEN
. "$PSScriptRoot\Include.ps1"

>>>>>CM:GREEN
################################################################################
>>>>>CM:GREEN
# SUPPORT
>>>>>CM:GREEN
################################################################################

>>>>>CM:GREEN
Function Get-Version-From-Tag([string]$Tag)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
  if ($Tag -match "tags/v(.*)")
>>>>>CM:GREEN
  {
>>>>>CM:GREEN
    return $Matches[1]
>>>>>CM:GREEN
  }
>>>>>CM:GREEN
  if ($Tag -match "^v(.*)")
>>>>>CM:GREEN
  {
>>>>>CM:GREEN
    return $Matches[1]
>>>>>CM:GREEN
  }

>>>>>CM:GREEN
  return $Null
>>>>>CM:GREEN
}

>>>>>CM:GREEN
################################################################################
>>>>>CM:GREEN
# ENTRY POINT
>>>>>CM:GREEN
################################################################################

>>>>>CM:GREEN
if ($Tag -eq $null)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
  $Tags = git for-each-ref --sort=-creatordate --format '%(refname:short)' refs/tags

>>>>>CM:GREEN
  $Version = $null

>>>>>CM:GREEN
  foreach ($Match in $Tags | Select-String "\S+" -AllMatches)
>>>>>CM:GREEN
  {
>>>>>CM:GREEN
    $Version = Get-Version-From-Tag $Match
>>>>>CM:GREEN
    if ($Version -ne $null)
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
      break
>>>>>CM:GREEN
    }
>>>>>CM:GREEN
  }

>>>>>CM:GREEN
  if ($Version -eq $null)
>>>>>CM:GREEN
  {
>>>>>CM:GREEN
    Write-Host "Could not find tag"
>>>>>CM:GREEN
    exit 2
>>>>>CM:GREEN
  }
>>>>>CM:GREEN
}
>>>>>CM:GREEN
else
>>>>>CM:GREEN
{
>>>>>CM:GREEN
  $Version = Get-Version-From-Tag $Tag

>>>>>CM:GREEN
  if ($Version -eq $null)
>>>>>CM:GREEN
  {
>>>>>CM:GREEN
    Write-Host "Invalid tag ref '$Tag'"
>>>>>CM:GREEN
    exit 1
>>>>>CM:GREEN
  }
>>>>>CM:GREEN
}

>>>>>CM:GREEN
$VersionId = [guid]::NewGuid()

>>>>>CM:GREEN
$Content = @"
>>>>>CM:GREEN
<Include>
>>>>>CM:GREEN
  <?define Version="$Version"?>
>>>>>CM:GREEN
  <!-- This needs to be changed for every new version -->
>>>>>CM:GREEN
  <?define VersionId="$VersionId"?>
>>>>>CM:GREEN
</Include>
>>>>>CM:GREEN
"@

>>>>>CM:GREEN
Set-Content -Path "$Global:Scripts\Version.wxi" -Value $Content

>>>>>CM:GREEN
Write-Output "build-version=$Version" >> $env:GITHUB_OUTPUT
