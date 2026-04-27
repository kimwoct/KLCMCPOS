# Install SQLite CLI on Windows 10 for inspecting the KLCMC POS local database.
# Usage (PowerShell, as Administrator recommended):
#   powershell -ExecutionPolicy Bypass -File .\scripts\install-sqlite-windows10.ps1

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
Write-Host "==> KLCMC POS - SQLite CLI installer (Windows 10)" -ForegroundColor Cyan

function Test-Command($name) {
    return [bool](Get-Command $name -ErrorAction SilentlyContinue)
}

if (Test-Command sqlite3) {
    Write-Host "sqlite3 already installed: $(sqlite3 -version)"
    exit 0
}

# Try winget first
if (Test-Command winget) {
    Write-Host "==> Installing SQLite via winget..."
    try {
        winget install --id SQLite.SQLite -e --accept-package-agreements --accept-source-agreements
    } catch {
        Write-Warning "winget install failed: $_"
    }
}

if (-not (Test-Command sqlite3)) {
    if (Test-Command choco) {
        Write-Host "==> Falling back to Chocolatey..."
        choco install sqlite -y
    } else {
        Write-Host "winget and Chocolatey both unavailable."
        Write-Host "Install one of them, or download SQLite tools manually from:"
        Write-Host "  https://www.sqlite.org/download.html"
        exit 1
    }
}

if (Test-Command sqlite3) {
    Write-Host "==> Installed: $(sqlite3 -version)"
} else {
    Write-Warning "sqlite3 still not on PATH. You may need to restart your shell or add the install dir to PATH."
}

Write-Host ""
Write-Host "App database location (after first run of the MAUI app on Windows):"
Write-Host "  %LOCALAPPDATA%\Packages\<package-id>\LocalState\klcmcpos.db"
Write-Host "  (unpackaged)  %LOCALAPPDATA%\com.klcmc.pos\klcmcpos.db"
Write-Host ""
Write-Host "Inspect with:"
Write-Host '  sqlite3 "<path-to>\klcmcpos.db" ".tables"'
