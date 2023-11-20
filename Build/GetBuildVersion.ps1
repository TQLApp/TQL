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

Function Get-Version-From-Tag([string]$Tag)
{
  if ($tag -match "tags/v(.*)")
  {
    return $Matches[1]
  }
  if ($tag -match "^v(.*)")
  {
    return $Matches[1]
  }

  return $Null
}

################################################################################
# ENTRY POINT
################################################################################

$Global:Root = (Get-Item (Get-Script-Directory)).Parent.FullName

if ($tag -eq $null)
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
  $Version = Get-Version-From-Tag $tag

  if ($Version -eq $null)
  {
    Write-Host "Invalid tag ref"
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

Set-Content -Path "$Global:Root\Build\Version.wxi" -Value $Content

Write-Output "build-version=$Version" >> $env:GITHUB_OUTPUT
