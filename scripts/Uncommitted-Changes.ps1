param(
    [Parameter(Mandatory = $true)][string]$Tool
)

. "$PSScriptRoot\Include.ps1"

################################################################################
# ENTRY POINT
################################################################################

$Changes = git status --porcelain

if ("$Changes".Trim() -ne "")
{
    git --no-pager diff

    Write-Host "::error:: There are uncommitted changes. Run '$Tool' and update your PR with any changes."
    exit 1
}
