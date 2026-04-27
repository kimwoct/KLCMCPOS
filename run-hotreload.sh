#!/usr/bin/env bash
# Run the KLCMC POS MAUI app on macOS (Mac Catalyst) with .NET Hot Reload enabled.
# Usage:
#   ./run-hotreload.sh              # normal run (hot reload reuses previous deploy)
#   ./run-hotreload.sh --fresh      # uninstall any previously-deployed test build first
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PID_FILE="$SCRIPT_DIR/.hotreload.pid"

FRESH=0
if [ "${1:-}" = "--fresh" ]; then
  FRESH=1
fi

# Use the Microsoft-provided .NET SDK (has MAUI workload), not Homebrew's dotnet.
DOTNET="${DOTNET:-/usr/local/share/dotnet/dotnet}"

if [ ! -x "$DOTNET" ]; then
  echo "Error: dotnet not found at $DOTNET" >&2
  echo "Install the official .NET 8 SDK from https://dotnet.microsoft.com/download" >&2
  exit 1
fi

if [ -f "$PID_FILE" ]; then
  EXISTING_PID="$(cat "$PID_FILE" 2>/dev/null || true)"
  if [ -n "${EXISTING_PID:-}" ] && kill -0 "$EXISTING_PID" 2>/dev/null; then
    echo "Hot reload is already running (PID $EXISTING_PID)." >&2
    echo "Stop the existing watcher first, then run this script again." >&2
    exit 1
  fi
fi

if [ "$FRESH" -eq 1 ]; then
  echo "--fresh: removing previously-deployed test builds before launch"
  "$SCRIPT_DIR/scripts/uninstall-mac.sh"
fi

echo "$$" > "$PID_FILE"
cleanup() { rm -f "$PID_FILE"; }
trap cleanup EXIT

"$DOTNET" watch \
  --project "$SCRIPT_DIR/KLCMC.Pos.Maui/KLCMC.Pos.Maui.csproj" \
  --framework net8.0-maccatalyst \
  --property:EnableCodeSigning=false \
  run -c Debug
