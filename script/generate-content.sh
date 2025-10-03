cd "$(dirname "$0")/../src/Game/Content"
dotnet tool restore
dotnet mgcb /@:"Content.mgcb" /platform:DesktopGL
