>>>>>CM:GREEN
param(
>>>>>CM:GREEN
    [Parameter(Mandatory = $true)][string]$Tool
>>>>>CM:GREEN
)

>>>>>CM:GREEN
. "$PSScriptRoot\Include.ps1"

>>>>>CM:GREEN
################################################################################
>>>>>CM:GREEN
# ENTRY POINT
>>>>>CM:GREEN
################################################################################

>>>>>CM:GREEN
$Changes = git status --porcelain

>>>>>CM:GREEN
if ("$Changes".Trim() -ne "")
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    git --no-pager diff

>>>>>CM:GREEN
    Write-Host "::error:: There are uncommitted changes. Run '$Tool' and update your PR with any changes."
>>>>>CM:GREEN
    exit 1
>>>>>CM:GREEN
}
