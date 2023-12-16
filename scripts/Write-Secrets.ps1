$ErrorActionPreference = "Stop"

. "$PSScriptRoot\Include.ps1"

################################################################################
# ENTRY POINT
################################################################################

$ReplacementScriptBlock = {
    param($Match)

    $SecretName = $Match.Groups[1].Value

    Write-Host "Replacing secret $SecretName"

    $SecretValue = $null

    switch ($SecretName) {
        "GOOGLE_DRIVE_API_SECRET" { $SecretValue = $env:GOOGLE_DRIVE_API_SECRET }
        "GITHUB_OAUTH_SECRET" { $SecretValue = $env:GITHUB_OAUTH_SECRET }
        default { throw "Invalid secret name $SecretName" }
    }

    if ($SecretValue -eq $null -or $SecretValue -eq "") {
        throw "No value found for secret $SecretName"
    }

    # Base 64 encoding to obfuscate the secret. This is only to not
    # have the secret show up in a `strings` command or whatnot. It
    # won't really hide it.

    $Bytes = [System.Text.Encoding]::UTF8.GetBytes($SecretValue)
    return [System.Convert]::ToBase64String($Bytes)
}

Get-ChildItem -Path $Global:Src -Filter "*.cs" -Recurse -File | ForEach-Object {
    $FilePath = $_.FullName
    $Content = Get-Content $FilePath -Raw
    $ReplacedContent = [regex]::Replace($Content, "<!\[SECRET\[(.*?)\]\]>", $ReplacementScriptBlock)
    if ($Content -ne $ReplacedContent) {
        Set-Content $FilePath $ReplacedContent
    }
}
