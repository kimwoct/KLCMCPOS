namespace KLCMC.Pos.Core.Models;

public sealed class PrinterConnectionOptions
{
    public PrinterConnectionMode Mode { get; set; } = PrinterConnectionMode.Usb;
    public string Endpoint { get; set; } = "POS-80";
    public int BaudRate { get; set; } = 9600;
    public int DataBits { get; set; } = 8;
    public int StopBits { get; set; } = 0;
    public int Parity { get; set; } = 0;
    public int FlowControl { get; set; } = 1;
    public int PaperWidthMm { get; set; } = 80;
    public string CodePage { get; set; } = "UTF-8";
    public PrinterCutMode CutMode { get; set; } = PrinterCutMode.Partial;
    public int DrawerPulseOnMs { get; set; } = 120;
    public int DrawerPulseOffMs { get; set; } = 240;
}
