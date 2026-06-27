#!/bin/bash
set -euo pipefail

if [ -z "${NUGET_KEY:-}" ]; then
  echo "Error: NUGET_KEY environment variable is required"
  exit 1
fi

VERSION="${1:-1.0.0}"
echo "Publishing Ivy.Tendril.Plugin.Doom version: $VERSION"

cd "$(dirname "$0")"

rm -rf ./nupkg
mkdir -p ./nupkg

echo "Packing Ivy.Tendril.Plugin.Doom..."
dotnet pack Ivy.Tendril.Plugin.Doom.csproj \
  --configuration Release \
  --output ./nupkg \
  /p:Version=$VERSION

echo ""
echo "Pushing Ivy.Tendril.Plugin.Doom..."
dotnet nuget push "./nupkg/Ivy.Tendril.Plugin.Doom.$VERSION.nupkg" \
  --api-key "$NUGET_KEY" \
  --source https://api.nuget.org/v3/index.json

echo ""
echo "Done! Published Ivy.Tendril.Plugin.Doom $VERSION"
