#!/usr/bin/env bash
# Install SQLite CLI on macOS for inspecting the KLCMC POS local database.
set -euo pipefail

echo "==> KLCMC POS — SQLite CLI installer (macOS)"

if command -v sqlite3 >/dev/null 2>&1; then
  echo "sqlite3 already installed: $(sqlite3 --version)"
  exit 0
fi

if ! command -v brew >/dev/null 2>&1; then
  echo "Homebrew not found. Installing Homebrew..."
  /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
fi

echo "==> Installing sqlite via Homebrew..."
brew install sqlite

# Homebrew keg-only sqlite — make CLI discoverable.
BREW_PREFIX="$(brew --prefix sqlite)"
if [ -x "$BREW_PREFIX/bin/sqlite3" ]; then
  echo "sqlite3 binary: $BREW_PREFIX/bin/sqlite3"
  echo "Add this to your shell profile if not already on PATH:"
  echo "  export PATH=\"$BREW_PREFIX/bin:\$PATH\""
fi

echo "==> Done."
sqlite3 --version || "$BREW_PREFIX/bin/sqlite3" --version

cat <<'EOF'

App database location (after first run of the MAUI app):
  macOS:   ~/Library/Containers/com.klcmc.pos/Data/Library/Application Support/klcmcpos.db
  (or)     ~/Library/Application Support/com.klcmc.pos/klcmcpos.db

Inspect with:
  sqlite3 "<path-to>/klcmcpos.db" ".tables"
EOF
