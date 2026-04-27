using KLCMC.Pos.Core.Models;

namespace KLCMC.Pos.Core.Data.Entities;

public sealed class PrinterSettingEntity
{
    public int Id { get; set; } = 1;
    public PrinterConnectionMode Mode { get; set; } = PrinterConnectionMode.Serial;
    public string Endpoint { get; set; } = "COM1";
    public int BaudRate { get; set; } = 115200;
    public int DataBits { get; set; } = 8;
    public int StopBits { get; set; }
    public int Parity { get; set; }
    public int FlowControl { get; set; } = 1;
}
