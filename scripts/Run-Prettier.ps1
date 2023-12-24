>>>>>CM:GREEN
. "$PSScriptRoot\Include.ps1"

>>>>>CM:GREEN
################################################################################
>>>>>CM:GREEN
# SUPPORT
>>>>>CM:GREEN
################################################################################

>>>>>CM:GREEN
Function Split-Playbook
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    foreach ($Child in Get-ChildItem "$Global:Src\Tql.App\QuickStart\Playbook*.md")
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        $Index = 0
>>>>>CM:GREEN
        $Content = Get-Content $Child.FullName -Encoding UTF8
>>>>>CM:GREEN
        $Part = New-Object System.Collections.Generic.List[string]
>>>>>CM:GREEN
        $First = $true

>>>>>CM:GREEN
        foreach ($Line in $Content)
>>>>>CM:GREEN
        {
>>>>>CM:GREEN
            if ($Line -eq "---")
>>>>>CM:GREEN
            {
>>>>>CM:GREEN
                if ($First)
>>>>>CM:GREEN
                {
>>>>>CM:GREEN
                    Write-Playbook-Part -Name $Child.Name -Index $Index -Part $Part

>>>>>CM:GREEN
                    $First = $false
>>>>>CM:GREEN
                    $Part = New-Object System.Collections.Generic.List[string]
>>>>>CM:GREEN
                    $Index += 1
>>>>>CM:GREEN
                }
>>>>>CM:GREEN
                else
>>>>>CM:GREEN
                {
>>>>>CM:GREEN
                    $First = $true
>>>>>CM:GREEN
                }
>>>>>CM:GREEN
            }

>>>>>CM:GREEN
            $Part.Add($Line)
>>>>>CM:GREEN
        }

>>>>>CM:GREEN
        Write-Playbook-Part -Name $Child.Name -Index $Index -Part $Part
>>>>>CM:GREEN
    }
>>>>>CM:GREEN
}

>>>>>CM:GREEN
Function Write-Playbook-Part([string]$Name, [int]$Index, $Part)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    while ($Part.Count -gt 0 -and $Part[$Part.Count - 1] -eq "")
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        $Part.RemoveAt($Part.Count - 1)
>>>>>CM:GREEN
    }

>>>>>CM:GREEN
    if ($Part.Count -gt 0)
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        Set-Content (Get-Playbook-Part-FileName -Name $Name -Index $Index) $Part -Encoding UTF8
>>>>>CM:GREEN
    }
>>>>>CM:GREEN
}

>>>>>CM:GREEN
Function Merge-Playbook
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    foreach ($Child in Get-ChildItem "$Global:Src\Tql.App\QuickStart\Playbook*.md")
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        $Content = @()

>>>>>CM:GREEN
        for ($Index = 1; ; $Index += 1)
>>>>>CM:GREEN
        {
>>>>>CM:GREEN
            $Part = Read-Playbook-Part -Name $Child.Name -Index $Index
            
>>>>>CM:GREEN
            if ($Part -eq $null)
>>>>>CM:GREEN
            {
>>>>>CM:GREEN
                break
>>>>>CM:GREEN
            }

>>>>>CM:GREEN
            if ($Content.Count -gt 0)
>>>>>CM:GREEN
            {
>>>>>CM:GREEN
                $Content += ""
>>>>>CM:GREEN
            }

>>>>>CM:GREEN
            $Content += $Part
>>>>>CM:GREEN
        }

>>>>>CM:GREEN
        Set-Content $Child.FullName $Content -Encoding UTF8
>>>>>CM:GREEN
    }
>>>>>CM:GREEN
}

>>>>>CM:GREEN
Function Read-Playbook-Part([string]$Name, [int]$Index)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    $FileName = Get-Playbook-Part-FileName -Name $Name -Index $Index

>>>>>CM:GREEN
    if (-not (Test-Path $FileName))
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        return $null
>>>>>CM:GREEN
    }

>>>>>CM:GREEN
    return Get-Content $FileName -Encoding UTF8
>>>>>CM:GREEN
}

>>>>>CM:GREEN
Function Get-Playbook-Part-FileName([string]$Name, [int]$Index)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    $Name = $Name.Substring(0, $Name.Length - 3)

>>>>>CM:GREEN
    return "$Global:Src\Tql.App\QuickStart\Split$Name-$Index.md"
>>>>>CM:GREEN
}

>>>>>CM:GREEN
################################################################################
>>>>>CM:GREEN
# ENTRY POINT
>>>>>CM:GREEN
################################################################################

>>>>>CM:GREEN
$IsVSCode = ($env:TERM_PROGRAM -eq "vscode")

>>>>>CM:GREEN
if ($IsVSCode)
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    Write-Host "Skipping NPM install because you're running this from VSCode. Start the /runprettier.bat script from outside of VSCode to run NPM install."
>>>>>CM:GREEN
}
>>>>>CM:GREEN
else
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    npm install --save-dev prettier @prettier/plugin-xml
>>>>>CM:GREEN
}

>>>>>CM:GREEN
rm "$Global:Src\Tql.App\QuickStart\SplitPlaybook*.md"

>>>>>CM:GREEN
Split-Playbook

>>>>>CM:GREEN
npx prettier `
>>>>>CM:GREEN
  --plugin=@prettier/plugin-xml `
>>>>>CM:GREEN
  --write "$Global:Root\**\*.xaml" "$Global:Root\**\*.md" `
>>>>>CM:GREEN
  --end-of-line crlf `
>>>>>CM:GREEN
  --bracket-same-line true `
>>>>>CM:GREEN
  --single-attribute-per-line true `
>>>>>CM:GREEN
  --xml-quote-attributes double `
>>>>>CM:GREEN
  --xml-self-closing-space true `
>>>>>CM:GREEN
  --xml-whitespace-sensitivity ignore `
>>>>>CM:GREEN
  --prose-wrap always

>>>>>CM:GREEN
Merge-Playbook

>>>>>CM:GREEN
rm "$Global:Src\Tql.App\QuickStart\SplitPlaybook*.md"
