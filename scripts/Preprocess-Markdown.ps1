. "$PSScriptRoot\Include.ps1"

################################################################################
# ENTRY POINT
################################################################################

foreach ($Child in Get-ChildItem $Global:Docs -Filter "*.md" -Recurse -File)
{
    $Lines = Get-Content $Child.FullName

    $NewLines = $Lines -replace "^(\s*)(> \[!.*?\]) ", "`$1`$2`r`n`$1> "

    $Content = $Lines -join "`r`n"
    $NewContent = $NewLines -join "`r`n"

    if ($Content -ne $NewContent)
    {
        Write-Host "Updated $($Child.FullName)"

        Set-Content $Child.FullName $NewContent
    }
}
