#!/bin/bash
cd "$(dirname "$0")/../src"
dotnet tool restore
dotnet mgcb /@:"Content/Content.mgcb" /platform:DesktopGL
