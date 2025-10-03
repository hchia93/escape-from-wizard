#!/bin/bash

cd "$(dirname "$0")/.."

rm -f src/escape-from-wizard.sln

dotnet new sln -n escape-from-wizard -o src
dotnet sln src/escape-from-wizard.sln add src/escape-from-wizard.csproj

echo "Solution file generated at src/escape-from-wizard.sln"
