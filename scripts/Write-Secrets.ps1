>>>>>CM:GREEN
$ErrorActionPreference = "Stop"

>>>>>CM:GREEN
. "$PSScriptRoot\Include.ps1"

>>>>>CM:GREEN
################################################################################
>>>>>CM:GREEN
# ENTRY POINT
>>>>>CM:GREEN
################################################################################

>>>>>CM:GREEN
$ReplacementScriptBlock = {
>>>>>CM:GREEN
    param($Match)

>>>>>CM:GREEN
    $SecretName = $Match.Groups[1].Value

>>>>>CM:GREEN
    Write-Host "Replacing secret $SecretName"

>>>>>CM:GREEN
    $SecretValue = $null

>>>>>CM:GREEN
    switch ($SecretName) {
>>>>>CM:GREEN
        "GOOGLE_DRIVE_API_SECRET" { $SecretValue = $env:GOOGLE_DRIVE_API_SECRET }
>>>>>CM:GREEN
        "GITHUB_OAUTH_SECRET" { $SecretValue = $env:GITHUB_OAUTH_SECRET }
>>>>>CM:GREEN
        default { throw "Invalid secret name $SecretName" }
>>>>>CM:GREEN
    }

>>>>>CM:GREEN
    if ($SecretValue -eq $null -or $SecretValue -eq "") {
>>>>>CM:GREEN
        throw "No value found for secret $SecretName"
>>>>>CM:GREEN
    }

>>>>>CM:GREEN
    # Base 64 encoding to obfuscate the secret. This is only to not
>>>>>CM:GREEN
    # have the secret show up in a `strings` command or whatnot. It
>>>>>CM:GREEN
    # won't really hide it.

>>>>>CM:GREEN
    $Bytes = [System.Text.Encoding]::UTF8.GetBytes($SecretValue)
>>>>>CM:GREEN
    return [System.Convert]::ToBase64String($Bytes)
>>>>>CM:GREEN
}

>>>>>CM:GREEN
Get-ChildItem -Path $Global:Src -Filter "*.cs" -Recurse -File | ForEach-Object {
>>>>>CM:GREEN
    $FilePath = $_.FullName
>>>>>CM:GREEN
    $Content = Get-Content $FilePath -Raw
>>>>>CM:GREEN
    $ReplacedContent = [regex]::Replace($Content, "<!\[SECRET\[(.*?)\]\]>", $ReplacementScriptBlock)
>>>>>CM:GREEN
    if ($Content -ne $ReplacedContent) {
>>>>>CM:GREEN
        Set-Content $FilePath $ReplacedContent
>>>>>CM:GREEN
    }
>>>>>CM:GREEN
}
