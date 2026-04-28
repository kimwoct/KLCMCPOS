using KLCMC.Pos.Core.Models;

namespace KLCMC.Pos.Core.Data.Entities;

public sealed class PrinterSettingEntity
{
    public int Id { get; set; } = 1;
    public PrinterConnectionMode Mode { get; set; } = PrinterConnectionMode.Usb;
    public string Endpoint { get; set; } = "POS-80 11.3.0.1";
    public int BaudRate { get; set; } = 9600;
    public int DataBits { get; set; } = 8;
    public int StopBits { get; set; }
    public int Parity { get; set; }
    public int FlowControl { get; set; } = 1;
    public int PaperWidthMm { get; set; } = 80;
    public string CodePage { get; set; } = "UTF-8";
    public PrinterCutMode CutMode { get; set; } = PrinterCutMode.Partial;
    public int DrawerPulseOnMs { get; set; } = 120;
    public int DrawerPulseOffMs { get; set; } = 240;
}
