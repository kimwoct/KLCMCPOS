using KLCMC.Pos.Core.Models;
using KLCMC.Pos.Core.Services;

namespace KLCMC.Pos.Printer.Windows;

public sealed class PosDllPrinterService : IPrinterService
{
    public bool IsOpen => global::POSDLL.Pos.POS_IsOpen();

    public string? LastError => string.IsNullOrWhiteSpace(global::POSDLL.Pos.lasterror) ? null : global::POSDLL.Pos.lasterror;

    public void Open(PrinterConnectionOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            throw new InvalidOperationException("Printer endpoint is required.");
        }

        switch (options.Mode)
        {
            case PrinterConnectionMode.Serial:
                global::POSDLL.Pos.POS_Open(
                    options.Endpoint,
                    options.BaudRate,
                    options.DataBits,
                    options.StopBits,
                    options.Parity,
                    options.FlowControl);
                break;
            case PrinterConnectionMode.Lan:
                global::POSDLL.Pos.POS_Open(options.Endpoint, 0, 0, 0, 0, 2);
                break;
            case PrinterConnectionMode.Usb:
                global::POSDLL.Pos.POS_Open(options.Endpoint, 0, 0, 0, 0, 3);
                break;
            default:
                throw new InvalidOperationException($"Unsupported connection mode: {options.Mode}");
        }

        if (!global::POSDLL.Pos.POS_IsOpen())
        {
            throw new InvalidOperationException(LastError ?? "Failed to open printer connection.");
        }
    }

    public void Close()
    {
        if (global::POSDLL.Pos.POS_IsOpen())
        {
            global::POSDLL.Pos.POS_Close();
        }
    }

    public void PrintReceipt(IReadOnlyList<ReceiptLine> lines)
    {
        if (!global::POSDLL.Pos.POS_IsOpen())
        {
            throw new InvalidOperationException("Printer is not connected.");
        }

        if (lines.Count == 0)
        {
            throw new InvalidOperationException("Cannot print an empty receipt.");
        }

        global::POSDLL.Pos.POS_SetMode(0x00);
        foreach (var line in lines)
        {
            global::POSDLL.Pos.POS_S_TextOut(line.Text + "\n", 0, 0, 0, 0x00, 0x00);
        }

        global::POSDLL.Pos.POS_FeedLine();
        global::POSDLL.Pos.POS_FeedLine();
        global::POSDLL.Pos.POS_CutPaper(0x01, 80);
    }
}
