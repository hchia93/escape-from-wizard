Set-Location $PSScriptRoot\..\src\Game\Content
dotnet tool restore
dotnet mgcb /@:"Content.mgcb" /platform:Windows