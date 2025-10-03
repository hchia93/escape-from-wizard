Set-Location $PSScriptRoot\..\src
dotnet tool restore
dotnet mgcb /@:"Game/Content/Content.mgcb" /platform:Windows
