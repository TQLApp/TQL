>>>>>CM:GREEN
. "$PSScriptRoot\Include.ps1"

>>>>>CM:GREEN
################################################################################
>>>>>CM:GREEN
# ENTRY POINT
>>>>>CM:GREEN
################################################################################

>>>>>CM:GREEN
foreach ($Child in Get-ChildItem $Global:Docs -Filter "*.md" -Recurse -File)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    $Lines = Get-Content $Child.FullName

>>>>>CM:GREEN
    $NewLines = $Lines -replace "^(\s*)(> \[!.*?\]) ", "`$1`$2`r`n`$1> "

>>>>>CM:GREEN
    $Content = $Lines -join "`r`n"
>>>>>CM:GREEN
    $NewContent = $NewLines -join "`r`n"

>>>>>CM:GREEN
    if ($Content -ne $NewContent)
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        Write-Host "Updated $($Child.FullName)"

>>>>>CM:GREEN
        Set-Content $Child.FullName $NewContent
>>>>>CM:GREEN
    }
>>>>>CM:GREEN
}
