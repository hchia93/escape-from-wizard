Set-Location $PSScriptRoot/..

if (Test-Path "src/escape-from-wizard.sln") {
    Remove-Item "src/escape-from-wizard.sln"
}

dotnet new sln -n escape-from-wizard -o src
dotnet sln src/escape-from-wizard.sln add src/escape-from-wizard.csproj

Write-Host "Solution file generated at src/escape-from-wizard.sln"