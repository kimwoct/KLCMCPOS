using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
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
    private PrinterConnectionOptions _options = new();

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
        _options = Clone(options);
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

        var pulseOn = ToDrawerTimingByte(_options.DrawerPulseOnMs);
        var pulseOff = ToDrawerTimingByte(_options.DrawerPulseOffMs);
        SendRawToPrinter([0x1B, 0x70, 0x00, pulseOn, pulseOff]);
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

        sb.Append('\n');
        sb.Append('\n');
        var body = ResolveEncoding(_options.CodePage).GetBytes(sb.ToString());
        var cut = ResolveCutBytes(_options.CutMode);

        if (cut.Length == 0)
        {
            SendRawToPrinter(body);
            return;
        }

        var payload = new byte[body.Length + cut.Length];
        Buffer.BlockCopy(body, 0, payload, 0, body.Length);
        Buffer.BlockCopy(cut, 0, payload, body.Length, cut.Length);

        SendRawToPrinter(payload);
    }

    public IReadOnlyList<string> GetInstalledPrinters()
    {
        return PrinterSettings.InstalledPrinters
            .Cast<string>()
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public void OpenPrinterProperties(string printerName)
    {
        if (string.IsNullOrWhiteSpace(printerName))
        {
            throw new InvalidOperationException("Printer endpoint is required.");
        }

        EnsurePrinterExists(printerName);
        var escaped = printerName.Replace("\"", "\"\"");
        var psi = new ProcessStartInfo
        {
            FileName = "rundll32.exe",
            Arguments = $"printui.dll,PrintUIEntry /p /n \"{escaped}\"",
            UseShellExecute = true
        };

        var process = Process.Start(psi);
        if (process is null)
        {
            throw new InvalidOperationException("Failed to open Windows printer properties.");
        }
    }

    public PrinterCapabilities ProbePrinter(string printerName)
    {
        if (string.IsNullOrWhiteSpace(printerName))
        {
            throw new InvalidOperationException("Printer endpoint is required.");
        }

        EnsurePrinterExists(printerName);

        var settings = new PrinterSettings { PrinterName = printerName };
        if (!settings.IsValid)
        {
            throw new InvalidOperationException($"Printer '{printerName}' is not installed.");
        }

        var defaultPrinter = new PrinterSettings().PrinterName;
        var info = GetPrinterInfo2(printerName);
        var papers = settings.PaperSizes.Cast<PaperSize>()
            .Select(p => p.PaperName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new PrinterCapabilities
        {
            PrinterName = printerName,
            DriverName = info.pDriverName ?? string.Empty,
            PortName = info.pPortName ?? string.Empty,
            IsDefault = string.Equals(defaultPrinter, printerName, StringComparison.OrdinalIgnoreCase),
            StatusText = ToPrinterStatusText(info.Status),
            DefaultPaper = settings.DefaultPageSettings?.PaperSize?.PaperName ?? string.Empty,
            PaperSizes = papers
        };
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

    private static PrinterConnectionOptions Clone(PrinterConnectionOptions options)
    {
        return new PrinterConnectionOptions
        {
            Mode = options.Mode,
            Endpoint = options.Endpoint,
            BaudRate = options.BaudRate,
            DataBits = options.DataBits,
            StopBits = options.StopBits,
            Parity = options.Parity,
            FlowControl = options.FlowControl,
            PaperWidthMm = options.PaperWidthMm,
            CodePage = options.CodePage,
            CutMode = options.CutMode,
            DrawerPulseOnMs = options.DrawerPulseOnMs,
            DrawerPulseOffMs = options.DrawerPulseOffMs
        };
    }

    private static byte[] ResolveCutBytes(PrinterCutMode cutMode)
    {
        return cutMode switch
        {
            PrinterCutMode.None => [],
            PrinterCutMode.Full => [0x1D, 0x56, 0x00],
            _ => [0x1D, 0x56, 0x01]
        };
    }

    private static Encoding ResolveEncoding(string? codePage)
    {
        if (string.IsNullOrWhiteSpace(codePage))
        {
            return Encoding.UTF8;
        }

        if (codePage.Equals("UTF-8", StringComparison.OrdinalIgnoreCase))
        {
            return Encoding.UTF8;
        }

        if (codePage.Equals("ASCII", StringComparison.OrdinalIgnoreCase))
        {
            return Encoding.ASCII;
        }

        if (codePage.Equals("Windows-1252", StringComparison.OrdinalIgnoreCase))
        {
            return Encoding.GetEncoding(1252);
        }

        if (codePage.Equals("Big5", StringComparison.OrdinalIgnoreCase))
        {
            return Encoding.GetEncoding(950);
        }

        if (codePage.Equals("GB18030", StringComparison.OrdinalIgnoreCase))
        {
            return Encoding.GetEncoding(54936);
        }

        if (codePage.Equals("Shift_JIS", StringComparison.OrdinalIgnoreCase))
        {
            return Encoding.GetEncoding(932);
        }

        return Encoding.GetEncoding(codePage);
    }

    private static byte ToDrawerTimingByte(int milliseconds)
    {
        var value = Math.Clamp(milliseconds / 2, 1, 255);
        return (byte)value;
    }

    private static string ToPrinterStatusText(uint status)
    {
        if (status == 0)
        {
            return "Ready";
        }

        var states = new List<string>();
        if ((status & PRINTER_STATUS_OFFLINE) != 0) states.Add("Offline");
        if ((status & PRINTER_STATUS_PAPER_OUT) != 0) states.Add("Paper Out");
        if ((status & PRINTER_STATUS_BUSY) != 0) states.Add("Busy");
        if ((status & PRINTER_STATUS_ERROR) != 0) states.Add("Error");
        if ((status & PRINTER_STATUS_DOOR_OPEN) != 0) states.Add("Door Open");
        if ((status & PRINTER_STATUS_NOT_AVAILABLE) != 0) states.Add("Not Available");
        if ((status & PRINTER_STATUS_TONER_LOW) != 0) states.Add("Low Toner");
        if ((status & PRINTER_STATUS_OUTPUT_BIN_FULL) != 0) states.Add("Output Bin Full");

        return states.Count == 0 ? $"Status: 0x{status:X}" : string.Join(", ", states);
    }

    private static PRINTER_INFO_2 GetPrinterInfo2(string printerName)
    {
        if (!OpenPrinter(printerName, out var handle, IntPtr.Zero))
        {
            var error = new Win32Exception(Marshal.GetLastWin32Error()).Message;
            throw new InvalidOperationException($"Cannot open printer '{printerName}'. {error}");
        }

        try
        {
            _ = GetPrinter(handle, 2, IntPtr.Zero, 0, out var needed);
            if (needed == 0)
            {
                return new PRINTER_INFO_2();
            }

            var buffer = Marshal.AllocHGlobal((int)needed);
            try
            {
                if (!GetPrinter(handle, 2, buffer, needed, out _))
                {
                    var error = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                    throw new InvalidOperationException($"Cannot read printer capabilities for '{printerName}'. {error}");
                }

                return Marshal.PtrToStructure<PRINTER_INFO_2>(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            ClosePrinter(handle);
        }
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct PRINTER_INFO_2
    {
        public string? pServerName;
        public string? pPrinterName;
        public string? pShareName;
        public string? pPortName;
        public string? pDriverName;
        public string? pComment;
        public string? pLocation;
        public IntPtr pDevMode;
        public string? pSepFile;
        public string? pPrintProcessor;
        public string? pDatatype;
        public string? pParameters;
        public IntPtr pSecurityDescriptor;
        public uint Attributes;
        public uint Priority;
        public uint DefaultPriority;
        public uint StartTime;
        public uint UntilTime;
        public uint Status;
        public uint cJobs;
        public uint AveragePPM;
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

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool GetPrinter(IntPtr hPrinter, int level, IntPtr pPrinter, uint cbBuf, out uint pcbNeeded);

    private const uint PRINTER_STATUS_BUSY = 0x00000200;
    private const uint PRINTER_STATUS_DOOR_OPEN = 0x00400000;
    private const uint PRINTER_STATUS_ERROR = 0x00000002;
    private const uint PRINTER_STATUS_NOT_AVAILABLE = 0x00001000;
    private const uint PRINTER_STATUS_OFFLINE = 0x00000080;
    private const uint PRINTER_STATUS_OUTPUT_BIN_FULL = 0x00000800;
    private const uint PRINTER_STATUS_PAPER_OUT = 0x00000010;
    private const uint PRINTER_STATUS_TONER_LOW = 0x00020000;
}
