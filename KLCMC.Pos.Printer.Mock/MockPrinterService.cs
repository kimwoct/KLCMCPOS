using System.Text;
using KLCMC.Pos.Core.Models;
using KLCMC.Pos.Core.Services;

namespace KLCMC.Pos.Printer.Mock;

public sealed class MockPrinterService : IPrinterService
{
    private readonly string _outputDirectory;
    private bool _isOpen;
    private string? _lastError;

    public MockPrinterService(string? outputDirectory = null)
    {
        _outputDirectory = outputDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "KLCMCPOS");
    }

    public bool IsOpen => _isOpen;
    public string? LastError => _lastError;

    public void Open(PrinterConnectionOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            _lastError = "Printer endpoint is required.";
            throw new InvalidOperationException(_lastError);
        }

        Directory.CreateDirectory(_outputDirectory);
        _isOpen = true;
        _lastError = null;
    }

    public void Close()
    {
        _isOpen = false;
    }

    public void OpenDrawer()
    {
        if (!_isOpen)
        {
            _lastError = "Printer is not connected.";
            throw new InvalidOperationException(_lastError);
        }

        var fileName = $"drawer-open-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
        var outputPath = Path.Combine(_outputDirectory, fileName);
        File.WriteAllText(outputPath, "Drawer open triggered.", Encoding.UTF8);
        _lastError = null;
    }

    public void PrintReceipt(IReadOnlyList<ReceiptLine> lines)
    {
        if (!_isOpen)
        {
            _lastError = "Printer is not connected.";
            throw new InvalidOperationException(_lastError);
        }

        if (lines.Count == 0)
        {
            _lastError = "Cannot print an empty receipt.";
            throw new InvalidOperationException(_lastError);
        }

        var fileName = $"receipt-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
        var outputPath = Path.Combine(_outputDirectory, fileName);
        var content = string.Join(Environment.NewLine, lines.Select(line => line.Text));
        File.WriteAllText(outputPath, content, Encoding.UTF8);
        _lastError = null;
    }

    public IReadOnlyList<string> GetInstalledPrinters()
    {
        return ["Mock Printer"];
    }

    public void OpenPrinterProperties(string printerName)
    {
        if (string.IsNullOrWhiteSpace(printerName))
        {
            throw new InvalidOperationException("Printer endpoint is required.");
        }
    }

    public PrinterCapabilities ProbePrinter(string printerName)
    {
        if (string.IsNullOrWhiteSpace(printerName))
        {
            throw new InvalidOperationException("Printer endpoint is required.");
        }

        return new PrinterCapabilities
        {
            PrinterName = printerName.Trim(),
            DriverName = "Mock Driver",
            PortName = "MOCK",
            IsDefault = true,
            StatusText = "Ready",
            DefaultPaper = "80mm",
            PaperSizes = ["58mm", "80mm"]
        };
    }
}
