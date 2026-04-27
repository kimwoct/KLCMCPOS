# Install SQLite CLI on Windows 10 for inspecting the KLCMC POS local database.
# Usage (PowerShell, as Administrator recommended):
#   powershell -ExecutionPolicy Bypass -File .\scripts\install-sqlite-windows10.ps1
#   powershell -ExecutionPolicy Bypass -File .\scripts\install-sqlite-windows10.ps1 -MauiTarget windows10
#   powershell -ExecutionPolicy Bypass -File .\scripts\install-sqlite-windows10.ps1 -MauiTarget macos

[CmdletBinding()]
param(
    [ValidateSet('windows10', 'macos')]
    [string]$MauiTarget = 'windows10'
)

$ErrorActionPreference = "Stop"
Write-Host "==> KLCMC POS - SQLite CLI installer (Windows 10)" -ForegroundColor Cyan

function Test-Command($name) {
    return [bool](Get-Command $name -ErrorAction SilentlyContinue)
}

function Set-MauiTargetConfiguration {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('windows10', 'macos')]
        [string]$Target
    )

    $repoRoot = Split-Path -Parent $PSScriptRoot
    $csprojPath = Join-Path $repoRoot 'KLCMC.Pos.Maui\KLCMC.Pos.Maui.csproj'
    if (-not (Test-Path $csprojPath)) {
        Write-Warning "KLCMC.Pos.Maui.csproj not found at: $csprojPath"
        return
    }

    Write-Host "==> Applying MAUI target configuration: $Target"
    $content = Get-Content -Path $csprojPath -Raw

    if ($Target -eq 'windows10') {
        $windowsTfms = '<TargetFrameworks Condition="$([MSBuild]::IsOSPlatform(''windows''))">net8.0-windows10.0.19041.0</TargetFrameworks>'
    } else {
        $windowsTfms = '<TargetFrameworks Condition="$([MSBuild]::IsOSPlatform(''windows''))">net8.0-maccatalyst;net8.0-windows10.0.19041.0</TargetFrameworks>'
    }

    $nonWindowsTfms = '<TargetFrameworks Condition="!$([MSBuild]::IsOSPlatform(''windows''))">net8.0-maccatalyst</TargetFrameworks>'
    $maccatRids = '<RuntimeIdentifiers Condition="$([MSBuild]::GetTargetPlatformIdentifier(''$(TargetFramework)'')) == ''maccatalyst''">maccatalyst-arm64;maccatalyst-x64</RuntimeIdentifiers>'

    $content = [regex]::Replace(
        $content,
        '<TargetFrameworks Condition="\$\(\[MSBuild\]::IsOSPlatform\(''windows''\)\)">.*?</TargetFrameworks>',
        $windowsTfms)

    $content = [regex]::Replace(
        $content,
        '<TargetFrameworks Condition="!\$\(\[MSBuild\]::IsOSPlatform\(''windows''\)\)">.*?</TargetFrameworks>',
        $nonWindowsTfms)

    if ($content -match '<RuntimeIdentifiers[^>]*>') {
        $content = [regex]::Replace(
            $content,
            '<RuntimeIdentifiers[^>]*>.*?</RuntimeIdentifiers>',
            $maccatRids)
    } else {
        $content = $content -replace '(?s)(<SingleProject>true</SingleProject>\s*)', "`$1    $maccatRids`r`n"
    }

    Set-Content -Path $csprojPath -Value $content -Encoding UTF8
    Write-Host "==> Updated: $csprojPath"
}

Set-MauiTargetConfiguration -Target $MauiTarget

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
