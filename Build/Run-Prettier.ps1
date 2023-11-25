################################################################################
# SUPPORT
################################################################################

Function Split-Playbook
{
    foreach ($Child in Get-ChildItem "Tql.App\QuickStart\Playbook*.md")
    {
        $Index = 0
        $Content = Get-Content $Child.FullName
        $Part = @()
        $First = $true

        foreach ($Line in $Content)
        {
            if ($Line -eq "---")
            {
                if ($First)
                {
                    Write-Playbook-Part -Name $Child.Name -Index $Index -Part $Part

                    $First = $false
                    $Part = @()
                    $Index += 1
                }
                else
                {
                    $First = $true
                }
            }

            $Part += $Line
        }

        Write-Playbook-Part -Name $Child.Name -Index $Index -Part $Part
    }
}

Function Write-Playbook-Part([string]$Name, [int]$Index, [array]$Part)
{
    if ($Part.Count -gt 0)
    {
        Set-Content (Get-Playbook-Part-FileName -Name $Name -Index $Index) $Part
    }
}

Function Merge-Playbook
{
    foreach ($Child in Get-ChildItem "Tql.App\QuickStart\Playbook*.md")
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

        Set-Content $Child.FullName $Content
    }
}

Function Read-Playbook-Part([string]$Name, [int]$Index)
{
    $FileName = Get-Playbook-Part-FileName -Name $Name -Index $Index

    if (-not (Test-Path $FileName))
    {
        return $null
    }

    return Get-Content $FileName
}

Function Get-Playbook-Part-FileName([string]$Name, [int]$Index)
{
    $Name = $Name.Substring(0, $Name.Length - 3)

    return "Tql.App\QuickStart\Split$Name-$Index.md"
}

################################################################################
# ENTRY POINT
################################################################################

npm install --save-dev prettier @prettier/plugin-xml

rm "Tql.App\QuickStart\SplitPlaybook*.md"

Split-Playbook

npx prettier `
  --plugin=@prettier/plugin-xml `
  --write "**/*.xaml" "**/*.md" `
  --end-of-line crlf `
  --bracket-same-line true `
  --single-attribute-per-line true `
  --xml-quote-attributes double `
  --xml-self-closing-space true `
  --xml-whitespace-sensitivity ignore `
  --prose-wrap always

Merge-Playbook

rm "Tql.App\QuickStart\SplitPlaybook*.md"
