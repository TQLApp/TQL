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
$Global:Documentation = "$Global:Root\Documentation"

foreach ($Child in Get-ChildItem $Global:Documentation -Filter "*.md" -Recurse -File)
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
