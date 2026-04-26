namespace KLCMC.Pos.App.Models;

public sealed class PrinterConnectionOptions
{
    public PrinterConnectionMode Mode { get; set; } = PrinterConnectionMode.Serial;
    public string Endpoint { get; set; } = "COM1";
    public int BaudRate { get; set; } = 115200;
    public int DataBits { get; set; } = 8;
    public int StopBits { get; set; } = 0;
    public int Parity { get; set; } = 0;
    public int FlowControl { get; set; } = 1;
}
