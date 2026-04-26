#!/usr/bin/env bash
# Run the KLCMC POS MAUI app on macOS (Mac Catalyst) with .NET Hot Reload enabled.
# Usage: ./run-hotreload.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Use the Microsoft-provided .NET SDK (has MAUI workload), not Homebrew's dotnet.
DOTNET="${DOTNET:-/usr/local/share/dotnet/dotnet}"

if [ ! -x "$DOTNET" ]; then
  echo "Error: dotnet not found at $DOTNET" >&2
  echo "Install the official .NET 8 SDK from https://dotnet.microsoft.com/download" >&2
  exit 1
fi

exec "$DOTNET" watch \
  --project "$SCRIPT_DIR/KLCMC.Pos.Maui/KLCMC.Pos.Maui.csproj" \
  --framework net8.0-maccatalyst \
  run -c Release
