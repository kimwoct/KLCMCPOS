#!/usr/bin/env bash
# Uninstall any previously-deployed KLCMC POS Mac Catalyst test builds and
# wipe local caches so the next `dotnet run` / `run-hotreload.sh` deploys clean.
#
# Safe to run repeatedly. Does NOT touch source code or scripts/.
#
# Usage:
#   ./scripts/uninstall-mac.sh           # remove app + caches (keep build output)
#   ./scripts/uninstall-mac.sh --clean   # also delete bin/ and obj/ from MAUI project

set -euo pipefail

BUNDLE_ID="com.klcmc.pos"
APP_NAME="KLCMC.Pos.Maui.app"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
MAUI_PROJ_DIR="$REPO_ROOT/KLCMC.Pos.Maui"

CLEAN_BUILD=0
if [ "${1:-}" = "--clean" ]; then
  CLEAN_BUILD=1
fi

log() { printf "  %s\n" "$*"; }

echo "==> Stopping any running KLCMC POS instances"
PIDS=$(pgrep -f "$APP_NAME" 2>/dev/null || true)
if [ -n "$PIDS" ]; then
  # shellcheck disable=SC2086
  for pid in $PIDS; do
    log "kill $pid"
    kill "$pid" 2>/dev/null || true
  done
  sleep 1
  PIDS=$(pgrep -f "$APP_NAME" 2>/dev/null || true)
  if [ -n "$PIDS" ]; then
    for pid in $PIDS; do
      log "kill -9 $pid"
      kill -9 "$pid" 2>/dev/null || true
    done
  fi
else
  log "no running instance"
fi

# Stop hot-reload watcher if its pid file is around.
HR_PID_FILE="$REPO_ROOT/.hotreload.pid"
if [ -f "$HR_PID_FILE" ]; then
  HR_PID=$(cat "$HR_PID_FILE" 2>/dev/null || true)
  if [ -n "${HR_PID:-}" ] && kill -0 "$HR_PID" 2>/dev/null; then
    log "stop hot-reload watcher PID $HR_PID"
    kill "$HR_PID" 2>/dev/null || true
  fi
  rm -f "$HR_PID_FILE"
fi

echo "==> Removing installed .app bundles"
CANDIDATE_DIRS=(
  "/Applications"
  "$HOME/Applications"
  "$HOME/Library/Developer/Xcode/DerivedData"
)
for base in "${CANDIDATE_DIRS[@]}"; do
  [ -d "$base" ] || continue
  while IFS= read -r app; do
    [ -z "$app" ] && continue
    log "rm -rf $app"
    rm -rf "$app"
  done < <(find "$base" -maxdepth 4 -name "$APP_NAME" -type d 2>/dev/null)
done

# Inside the project's bin/ folders (created by dotnet build/run).
if [ -d "$MAUI_PROJ_DIR/bin" ]; then
  while IFS= read -r app; do
    [ -z "$app" ] && continue
    log "rm -rf $app"
    rm -rf "$app"
  done < <(find "$MAUI_PROJ_DIR/bin" -maxdepth 6 -name "$APP_NAME" -type d 2>/dev/null)
fi

echo "==> Removing app sandbox container, preferences, and caches"
TARGETS=(
  "$HOME/Library/Containers/$BUNDLE_ID"
  "$HOME/Library/Application Scripts/$BUNDLE_ID"
  "$HOME/Library/Preferences/$BUNDLE_ID.plist"
  "$HOME/Library/Saved Application State/$BUNDLE_ID.savedState"
  "$HOME/Library/Caches/$BUNDLE_ID"
  "$HOME/Library/HTTPStorages/$BUNDLE_ID"
  "$HOME/Library/HTTPStorages/$BUNDLE_ID.binarycookies"
  "$HOME/Library/WebKit/$BUNDLE_ID"
)
for t in "${TARGETS[@]}"; do
  if [ -e "$t" ]; then
    log "rm -rf $t"
    rm -rf "$t"
  fi
done

echo "==> Pruning Launch Services duplicate registrations"
LSREG="/System/Library/Frameworks/CoreServices.framework/Versions/A/Frameworks/LaunchServices.framework/Versions/A/Support/lsregister"
if [ -x "$LSREG" ]; then
  "$LSREG" -kill -r -domain local -domain system -domain user >/dev/null 2>&1 || true
  log "Launch Services database refreshed"
else
  log "lsregister not found — skipped"
fi

if [ "$CLEAN_BUILD" -eq 1 ]; then
  echo "==> Cleaning bin/ and obj/ in $MAUI_PROJ_DIR"
  rm -rf "$MAUI_PROJ_DIR/bin" "$MAUI_PROJ_DIR/obj"
  log "done"
fi

echo
echo "✅ Old KLCMC POS test builds removed (bundle: $BUNDLE_ID)."
echo "   Next run will deploy a fresh copy."
