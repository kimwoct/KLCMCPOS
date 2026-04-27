namespace KLCMC.Pos.Core.Models;

public sealed class PrinterConnectionOptions
{
    public PrinterConnectionMode Mode { get; set; } = PrinterConnectionMode.Usb;
    public string Endpoint { get; set; } = "USB001";
    public int BaudRate { get; set; } = 9600;
    public int DataBits { get; set; } = 8;
    public int StopBits { get; set; } = 0;
    public int Parity { get; set; } = 0;
    public int FlowControl { get; set; } = 1;
}
