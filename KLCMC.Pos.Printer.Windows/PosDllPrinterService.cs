using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using KLCMC.Pos.Core.Models;
using KLCMC.Pos.Core.Services;

namespace KLCMC.Pos.Printer.Windows;

public sealed class PosDllPrinterService : IPrinterService
{
    private bool _isOpen;
    private string? _lastError;
    private string? _printerName;

    public bool IsOpen => _isOpen;

    public string? LastError => _lastError;

    public void Open(PrinterConnectionOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            throw new InvalidOperationException("Printer endpoint is required.");
        }

        var endpoint = options.Endpoint.Trim();
        EnsurePrinterExists(endpoint);

        _printerName = endpoint;
        _isOpen = true;
        _lastError = null;
    }

    public void Close()
    {
        _isOpen = false;
    }

    public void OpenDrawer()
    {
        EnsureConnected();

        // ESC/POS pulse command to open the cash drawer.
        SendRawToPrinter([0x1B, 0x70, 0x00, 0x3C, 0xFF]);
    }

    public void PrintReceipt(IReadOnlyList<ReceiptLine> lines)
    {
        EnsureConnected();

        if (lines.Count == 0)
        {
            throw new InvalidOperationException("Cannot print an empty receipt.");
        }

        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            sb.Append(line.Text);
            sb.Append('\n');
        }

        // Feed and cut at the end of each print.
        sb.Append('\n');
        sb.Append('\n');
        var body = Encoding.UTF8.GetBytes(sb.ToString());
        var cut = new byte[] { 0x1D, 0x56, 0x01 };

        var payload = new byte[body.Length + cut.Length];
        Buffer.BlockCopy(body, 0, payload, 0, body.Length);
        Buffer.BlockCopy(cut, 0, payload, body.Length, cut.Length);

        SendRawToPrinter(payload);
    }

    private void EnsureConnected()
    {
        if (!_isOpen || string.IsNullOrWhiteSpace(_printerName))
        {
            throw new InvalidOperationException("Printer is not connected.");
        }
    }

    private void EnsurePrinterExists(string printerName)
    {
        if (!OpenPrinter(printerName, out var handle, IntPtr.Zero))
        {
            var error = new Win32Exception(Marshal.GetLastWin32Error()).Message;
            _lastError = error;
            throw new InvalidOperationException($"Cannot open printer '{printerName}'. {error}");
        }

        ClosePrinter(handle);
    }

    private void SendRawToPrinter(byte[] payload)
    {
        if (!OpenPrinter(_printerName!, out var printerHandle, IntPtr.Zero))
        {
            FailWithLastError("Failed to open printer");
        }

        var docInfo = new DOCINFO
        {
            pDocName = "KLCMC POS",
            pDataType = "RAW"
        };

        var docStarted = false;
        var pageStarted = false;

        try
        {
            if (StartDocPrinter(printerHandle, 1, docInfo) == 0)
            {
                FailWithLastError("Failed to start print job");
            }
            docStarted = true;

            if (!StartPagePrinter(printerHandle))
            {
                FailWithLastError("Failed to start print page");
            }
            pageStarted = true;

            if (!WritePrinter(printerHandle, payload, payload.Length, out var written) || written != payload.Length)
            {
                FailWithLastError("Failed to send bytes to printer");
            }
        }
        finally
        {
            if (pageStarted)
            {
                EndPagePrinter(printerHandle);
            }

            if (docStarted)
            {
                EndDocPrinter(printerHandle);
            }

            ClosePrinter(printerHandle);
        }

        _lastError = null;
    }

    private void FailWithLastError(string message)
    {
        var error = new Win32Exception(Marshal.GetLastWin32Error()).Message;
        _lastError = error;
        throw new InvalidOperationException($"{message}. {error}");
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DOCINFO
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pDocName;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? pOutputFile;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string pDataType;
    }

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int StartDocPrinter(IntPtr hPrinter, int level, [In] DOCINFO pDocInfo);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool WritePrinter(IntPtr hPrinter, byte[] pBytes, int dwCount, out int dwWritten);
}
