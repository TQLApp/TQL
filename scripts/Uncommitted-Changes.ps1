param($tool)

$Changes = git status --porcelain

if ("$Changes".Trim() -ne "")
{
    git --no-pager diff

    Write-Host "::error:: There are uncommitted changes. Run '$tool' and update your PR with any changes."
    exit 1
}
