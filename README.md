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

## Smoke checklist

1. Build `KLCMC.Pos.Core` and `KLCMC.Pos.Printer.Mock`.
2. Run UI on target platform:
   - MAUI (target architecture path)
   - WPF (temporary Windows fallback/reference)
3. Verify cart operations and total calculation.
4. On macOS, verify mock receipt file output.
5. On Windows 10, verify `POSDLL` connect/print/feed/cut flow with hardware.
