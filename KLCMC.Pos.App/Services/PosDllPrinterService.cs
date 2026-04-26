using KLCMC.Pos.App.Models;
using POSDLL;

namespace KLCMC.Pos.App.Services;

public sealed class PosDllPrinterService : IPrinterService
{
    public bool IsOpen => Pos.POS_IsOpen();

    public string? LastError => string.IsNullOrWhiteSpace(Pos.lasterror) ? null : Pos.lasterror;

    public void Open(PrinterConnectionOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            throw new InvalidOperationException("Printer endpoint is required.");
        }

        switch (options.Mode)
        {
            case PrinterConnectionMode.Serial:
                Pos.POS_Open(
                    options.Endpoint,
                    options.BaudRate,
                    options.DataBits,
                    options.StopBits,
                    options.Parity,
                    options.FlowControl);
                break;
            case PrinterConnectionMode.Lan:
                Pos.POS_Open(options.Endpoint, 0, 0, 0, 0, 2);
                break;
            case PrinterConnectionMode.Usb:
                Pos.POS_Open(options.Endpoint, 0, 0, 0, 0, 3);
                break;
            default:
                throw new InvalidOperationException($"Unsupported connection mode: {options.Mode}");
        }

        if (!Pos.POS_IsOpen())
        {
            throw new InvalidOperationException(LastError ?? "Failed to open printer connection.");
        }
    }

    public void Close()
    {
        if (Pos.POS_IsOpen())
        {
            Pos.POS_Close();
        }
    }

    public void PrintReceipt(IReadOnlyList<ReceiptLine> lines)
    {
        if (!Pos.POS_IsOpen())
        {
            throw new InvalidOperationException("Printer is not connected.");
        }

        if (lines.Count == 0)
        {
            throw new InvalidOperationException("Cannot print an empty receipt.");
        }

        Pos.POS_SetMode(0x00);

        foreach (var line in lines)
        {
            Pos.POS_S_TextOut(line.Text + "\n", 0, 0, 0, 0x00, 0x00);
        }

        Pos.POS_FeedLine();
        Pos.POS_FeedLine();
        Pos.POS_CutPaper(0x01, 80);
    }
}
