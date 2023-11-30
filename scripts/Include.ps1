$Global:Root = [System.IO.Path]::GetFullPath("$PSScriptRoot\..")
$Global:Src = "$Global:Root\src"
$Global:Tests = "$Global:Root\tests"
$Global:Docs = "$Global:Root\docs"
$Global:Scripts = "$Global:Root\scripts"
$Global:Distrib = "$Global:Root\build"

Function Prepare-Directory([string]$Path)
{
    if (Test-Path -Path $Path)
    {
        Remove-Item -Recurse -Force $Path
    }

    [void](New-Item -Type directory $Path)
}