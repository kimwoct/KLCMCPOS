using KLCMC.Pos.App.Models;

namespace KLCMC.Pos.App.Services;

public interface IPrinterService
{
    bool IsOpen { get; }
    string? LastError { get; }

    void Open(PrinterConnectionOptions options);
    void Close();
    void PrintReceipt(IReadOnlyList<ReceiptLine> lines);
}
