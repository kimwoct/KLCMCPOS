using KLCMC.Pos.Core.Models;

namespace KLCMC.Pos.Core.Services;

public interface IPrinterService
{
    bool IsOpen { get; }
    string? LastError { get; }

    void Open(PrinterConnectionOptions options);
    void Close();
    void PrintReceipt(IReadOnlyList<ReceiptLine> lines);
}
