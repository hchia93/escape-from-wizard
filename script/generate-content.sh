#!/bin/bash
cd "$(dirname "$0")/../src"
dotnet tool restore
dotnet mgcb /@:"Game/Content/Content.mgcb" /platform:DesktopGL
