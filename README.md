# KLCMCPOS

KLCMCPOS is being migrated to a cross-platform architecture for **Windows 10 + macOS** on .NET 8.

## Projects

- `KLCMC.Pos.Core` (`net8.0`)
  - Shared POS domain models
  - Cart and receipt composition logic
  - Shared `IPrinterService` and `MainViewModel`
- `KLCMC.Pos.Printer.Windows` (`net8.0-windows`)
  - Real printer integration through `POSDLL.dll`
- `KLCMC.Pos.Printer.Mock` (`net8.0`)
  - Mock/file printer for macOS and non-hardware testing
- `KLCMC.Pos.Maui` (`net8.0-maccatalyst;net8.0-windows10.0.19041.0`)
  - Cross-platform UI shell (migration target)
- `KLCMC.Pos.App` (`net8.0-windows`, WPF)
  - Existing Windows UI retained temporarily as migration reference

## Platform behavior

- **Windows 10**: real `POSDLL.dll` hardware printing path is supported.
- **macOS**: app flow uses mock printer output; real POSDLL hardware printing is not available on macOS.

## Printer API usage (Windows)

- Open/close/status: `POS_Open`, `POS_IsOpen`, `POS_Close`
- Print flow: `POS_SetMode(0x00)`, `POS_S_TextOut`, `POS_FeedLine`, `POS_CutPaper`

## Build notes

1. `KLCMC.Pos.Core`, `KLCMC.Pos.Printer.Mock`, and `KLCMC.Pos.Printer.Windows` build with .NET 8 SDK.
2. `KLCMC.Pos.Maui` requires MAUI workloads/tooling support in your local .NET installation.
3. `KLCMC.Pos.App` (WPF) requires Windows desktop build support.

### VS Code / C# Dev Kit on macOS

If `ms-dotnettools.csdevkit.Projects.log` shows errors like:
- `The target platform identifier maccatalyst was not recognized`
- `Could not resolve SDK "Microsoft.NET.Sdk.WindowsDesktop"`

then VS Code is usually resolving to Homebrew `dotnet` instead of the SDK at
`/usr/local/share/dotnet` (which has the MAUI workloads used by this repo).

This repository pins the SDK in three layers:

1. `global.json` requires SDK `9.0.100` (`rollForward: latestMajor`). This makes
   the .NET host skip Homebrew `dotnet@8` (8.0.125) and resolve to
   `/usr/local/share/dotnet`, which ships the MAUI workloads and the
   `Microsoft.NET.Sdk.WindowsDesktop` targets used by the WPF host.
2. `.vscode/settings.json` pins C# / C# Dev Kit / .NET runtime acquisition to
   `/usr/local/share/dotnet/dotnet` and forces the terminal to use the same
   `DOTNET_ROOT` and `PATH`.
3. `~/.zshrc` exports `DOTNET_ROOT=/usr/local/share/dotnet` and prefixes
   `PATH` so CLI builds outside VS Code use the same SDK.

If VS Code is launched from the Dock/Spotlight (not from a terminal), GUI apps
do not read `~/.zshrc`. Run this once so launchd-spawned VS Code inherits the
correct env, then quit and relaunch VS Code:

```sh
launchctl setenv DOTNET_ROOT /usr/local/share/dotnet
```

After any of these changes, run **Developer: Reload Window** in VS Code to
make the C# Dev Kit re-evaluate projects.

## Smoke checklist

1. Build `KLCMC.Pos.Core` and `KLCMC.Pos.Printer.Mock`.
2. Run UI on target platform:
   - MAUI (target architecture path)
   - WPF (temporary Windows fallback/reference)
3. Verify cart operations and total calculation.
4. On macOS, verify mock receipt file output.
5. On Windows 10, verify `POSDLL` connect/print/feed/cut flow with hardware.

## Local database (SQLite)

Persistent state is stored in a local SQLite database via EF Core
(`Microsoft.EntityFrameworkCore.Sqlite 8.0.x`).

Persisted tables:
- `Products` — product catalog (seeded on first run with the default items)
- `Sales` / `SaleLines` — every confirmed checkout is recorded
- `SalePayments` — payment lines per sale (method, amount, tendered, change)
- `PrinterSettings` — single-row printer connection options

DB file location:
- macOS (MAUI): `<MAUI AppDataDirectory>/klcmcpos.db`
- Windows (MAUI / WPF): `%LOCALAPPDATA%\com.klcmc.pos\klcmcpos.db`

The schema is created via `EnsureCreated` at startup — no migrations are
shipped yet. If you change an entity, delete the `.db` file during dev or
introduce migrations.

> ⚠️ The Multi-Payment Checkout feature added a new `SalePayments` table
> and removed `Sales.PaymentMethod`. Dev databases created before this
> change must be deleted before first run (delete the `klcmcpos.db` file
> at the path above).

## Checkout & Daily Account

Tap **Checkout** on the dashboard to open the multi-payment popup:

- Total / Paid / Outstanding update live as you add payment lines.
- Pick a method (Cash, Card, Octopus, FPS, Other) and enter the amount on
  the on-screen money pad. Quick buttons for 20/50/100/500 and **Exact**
  (auto-fills the outstanding amount) make split tenders fast.
- For **Cash** the input is treated as money received: the system applies
  only the outstanding portion and records the rest as change. The change
  preview updates as you type.
- Add as many payment lines as needed (e.g. cash + card). **Confirm &
  Print** is enabled once Paid ≥ Total. The receipt prints with the
  payment breakdown and total change, the sale is persisted, and the
  cart clears.

Tap **Daily Account** in the header (MAUI) or footer (WPF) to open the
day-end view:

- Date picker plus Prev / Today / Next buttons (uses local calendar date).
- Summary cards: transaction count and gross total.
- "By Payment Method" tiles totalling each method for the day.
- Transactions list with timestamp, total, and per-payment chips.
- **Print Daily Report** prints a Z-style summary to the connected
  printer.

### Install the SQLite CLI (optional, for inspection)

- macOS: `./scripts/install-sqlite-mac.sh`
- Windows 10: `powershell -ExecutionPolicy Bypass -File .\scripts\install-sqlite-windows10.ps1`
  - Optional MAUI target mode:
    - `-MauiTarget windows10` (default; sets Windows-only MAUI target on Windows)
    - `-MauiTarget macos` (restores dual target: macOS + Windows)

Inspect with:

```
sqlite3 "<path-to>/klcmcpos.db" ".tables"
```
