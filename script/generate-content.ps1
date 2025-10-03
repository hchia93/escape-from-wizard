Set-Location $PSScriptRoot\..\src
dotnet tool restore
dotnet mgcb /@:"Content/Content.mgcb" /platform:Windows
