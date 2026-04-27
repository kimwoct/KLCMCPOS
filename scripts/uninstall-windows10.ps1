# Uninstall any previously-deployed KLCMC POS test builds on Windows 10.
# Stops the running process, removes desktop / start-menu shortcuts, and
# (optionally) clears the project's bin/ and obj/ folders.
#
# Usage (PowerShell, run as the same user that built the app):
#   .\scripts\uninstall-windows10.ps1
#   .\scripts\uninstall-windows10.ps1 -Clean

[CmdletBinding()]
param(
    [switch]$Clean
)

$ErrorActionPreference = 'Stop'

$AppName     = 'KLCMC.Pos.App'
$MauiName    = 'KLCMC.Pos.Maui'
$RepoRoot    = Split-Path -Parent $PSScriptRoot
$WpfProjDir  = Join-Path $RepoRoot 'KLCMC.Pos.App'
$MauiProjDir = Join-Path $RepoRoot 'KLCMC.Pos.Maui'

function Write-Step($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }
function Write-Item($msg) { Write-Host "  $msg" }

Write-Step 'Stopping running instances'
foreach ($name in @($AppName, $MauiName)) {
    Get-Process -Name $name -ErrorAction SilentlyContinue | ForEach-Object {
        Write-Item ("kill {0} (PID {1})" -f $_.ProcessName, $_.Id)
        try { Stop-Process -Id $_.Id -Force -ErrorAction Stop } catch {}
    }
}

Write-Step 'Removing shortcuts'
$shortcutLocations = @(
    [Environment]::GetFolderPath('Desktop'),
    [Environment]::GetFolderPath('CommonDesktopDirectory'),
    [Environment]::GetFolderPath('StartMenu'),
    [Environment]::GetFolderPath('CommonStartMenu'),
    [Environment]::GetFolderPath('Programs'),
    [Environment]::GetFolderPath('CommonPrograms')
) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -Unique

foreach ($loc in $shortcutLocations) {
    Get-ChildItem -Path $loc -Recurse -Filter '*.lnk' -ErrorAction SilentlyContinue |
        Where-Object { $_.BaseName -match 'KLCMC' } |
        ForEach-Object {
            Write-Item ("remove {0}" -f $_.FullName)
            Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue
        }
}

Write-Step 'Removing per-user data and caches'
$userDataTargets = @(
    (Join-Path $env:LOCALAPPDATA $AppName),
    (Join-Path $env:LOCALAPPDATA $MauiName),
    (Join-Path $env:LOCALAPPDATA 'KLCMC'),
    (Join-Path $env:APPDATA      $AppName),
    (Join-Path $env:APPDATA      $MauiName),
    (Join-Path $env:APPDATA      'KLCMC')
)
foreach ($t in $userDataTargets) {
    if (Test-Path $t) {
        Write-Item ("rm {0}" -f $t)
        Remove-Item $t -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# MAUI WinUI packaged installs (if ever produced via MSIX). Best-effort.
Write-Step 'Removing MSIX/AppX package (if installed)'
try {
    $pkgs = Get-AppxPackage -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -match 'klcmc' -or $_.PackageFamilyName -match 'klcmc' }
    foreach ($p in $pkgs) {
        Write-Item ("Remove-AppxPackage {0}" -f $p.PackageFullName)
        Remove-AppxPackage -Package $p.PackageFullName -ErrorAction SilentlyContinue
    }
    if (-not $pkgs) { Write-Item 'no AppX package found' }
} catch {
    Write-Item 'AppX cmdlets not available — skipped'
}

if ($Clean) {
    Write-Step 'Cleaning bin/ and obj/'
    foreach ($dir in @($WpfProjDir, $MauiProjDir)) {
        foreach ($sub in @('bin', 'obj')) {
            $path = Join-Path $dir $sub
            if (Test-Path $path) {
                Write-Item ("rm {0}" -f $path)
                Remove-Item $path -Recurse -Force -ErrorAction SilentlyContinue
            }
        }
    }
}

Write-Host ''
Write-Host '✅ Old KLCMC POS test builds removed. Next run will deploy fresh.' -ForegroundColor Green
