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

Function Split-Playbook
{
    foreach ($Child in Get-ChildItem "$Global:Root\Tql.App\QuickStart\Playbook*.md")
    {
        $Index = 0
        $Content = Get-Content $Child.FullName -Encoding UTF8
        $Part = New-Object System.Collections.Generic.List[string]
        $First = $true

        foreach ($Line in $Content)
        {
            if ($Line -eq "---")
            {
                if ($First)
                {
                    Write-Playbook-Part -Name $Child.Name -Index $Index -Part $Part

                    $First = $false
                    $Part = New-Object System.Collections.Generic.List[string]
                    $Index += 1
                }
                else
                {
                    $First = $true
                }
            }

            $Part.Add($Line)
        }

        Write-Playbook-Part -Name $Child.Name -Index $Index -Part $Part
    }
}

Function Write-Playbook-Part([string]$Name, [int]$Index, $Part)
{
    while ($Part.Count -gt 0 -and $Part[$Part.Count - 1] -eq "")
    {
        $Part.RemoveAt($Part.Count - 1)
    }

    if ($Part.Count -gt 0)
    {
        Set-Content (Get-Playbook-Part-FileName -Name $Name -Index $Index) $Part -Encoding UTF8
    }
}

Function Merge-Playbook
{
    foreach ($Child in Get-ChildItem "$Global:Root\Tql.App\QuickStart\Playbook*.md")
    {
        $Content = @()

        for ($Index = 1; ; $Index += 1)
        {
            $Part = Read-Playbook-Part -Name $Child.Name -Index $Index
            
            if ($Part -eq $null)
            {
                break
            }

            if ($Content.Count -gt 0)
            {
                $Content += ""
            }

            $Content += $Part
        }

        Set-Content $Child.FullName $Content -Encoding UTF8
    }
}

Function Read-Playbook-Part([string]$Name, [int]$Index)
{
    $FileName = Get-Playbook-Part-FileName -Name $Name -Index $Index

    if (-not (Test-Path $FileName))
    {
        return $null
    }

    return Get-Content $FileName -Encoding UTF8
}

Function Get-Playbook-Part-FileName([string]$Name, [int]$Index)
{
    $Name = $Name.Substring(0, $Name.Length - 3)

    return "$Global:Root\Tql.App\QuickStart\Split$Name-$Index.md"
}

################################################################################
# ENTRY POINT
################################################################################

$Global:Root = (Get-Item (Get-Script-Directory)).Parent.FullName

$IsVSCode = ($env:TERM_PROGRAM -eq "vscode")

if ($IsVSCode)
{
    Write-Host "Skipping NPM install because you're running this from VSCode. Start the /runprettier.bat script from outside of VSCode to run NPM install."
}
else
{
    npm install --save-dev prettier @prettier/plugin-xml
}

rm "$Global:Root\Tql.App\QuickStart\SplitPlaybook*.md"

Split-Playbook

npx prettier `
  --plugin=@prettier/plugin-xml `
  --write "$Global:Root\**\*.xaml" "$Global:Root\**\*.md" `
  --end-of-line crlf `
  --bracket-same-line true `
  --single-attribute-per-line true `
  --xml-quote-attributes double `
  --xml-self-closing-space true `
  --xml-whitespace-sensitivity ignore `
  --prose-wrap always

Merge-Playbook

rm "$Global:Root\Tql.App\QuickStart\SplitPlaybook*.md"
