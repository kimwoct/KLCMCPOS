# KLCMCPOS

KLCMCPOS is a .NET 8 WPF POS starter app with phase-1 receipt printing through `POSDLL.dll`.

## Project layout

- `KLCMCPOS.sln` - solution file
- `KLCMC.Pos.App/` - WPF application
- `KLCMC.Pos.App/libs/POSDLL.dll` - vendor SDK dependency copied from sample SDK

## Build requirements

1. .NET 8 SDK
2. Windows-targeting build support (`EnableWindowsTargeting=true` is set in project)
3. `POSDLL.dll` compatible with x64 runtime
4. Printer driver installed for USB/driver mode

## Run

1. Open `KLCMCPOS.sln` in Visual Studio or Rider on Windows.
2. Build `KLCMC.Pos.App` (x64).
3. Start app and configure printer connection:
   - Serial: endpoint like `COM1`
   - LAN: endpoint as printer IP
   - USB: endpoint as installed printer name
4. Add preset items, optionally override quantity/price in cart, then print receipt.

## POSDLL phase-1 API usage

- Open/close/status: `POS_Open`, `POS_IsOpen`, `POS_Close`
- Print flow: `POS_SetMode(0x00)`, `POS_S_TextOut`, `POS_FeedLine`, `POS_CutPaper`

## Smoke checklist

1. Connect with valid endpoint and confirm status shows `Connected`.
2. Add at least one item and edit quantity/price in cart.
3. Print receipt and verify lines, total, feed, and cut behavior.
4. Disconnect and confirm status shows `Disconnected`.
