>>>>>CM:GREEN
$Global:Root = [System.IO.Path]::GetFullPath("$PSScriptRoot\..")
>>>>>CM:GREEN
$Global:Src = "$Global:Root\src"
>>>>>CM:GREEN
$Global:Tests = "$Global:Root\tests"
>>>>>CM:GREEN
$Global:Docs = "$Global:Root\docs"
>>>>>CM:GREEN
$Global:Scripts = "$Global:Root\scripts"
>>>>>CM:GREEN
$Global:Distrib = "$Global:Root\build"

>>>>>CM:GREEN
Function Prepare-Directory([string]$Path)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    if (Test-Path -Path $Path)
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        Remove-Item -Recurse -Force $Path
>>>>>CM:GREEN
    }

>>>>>CM:GREEN
    [void](New-Item -Type directory $Path)
>>>>>CM:GREEN
}
