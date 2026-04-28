namespace KLCMC.Pos.Core.Models;

public sealed class PrinterCapabilities
{
    public string PrinterName { get; init; } = string.Empty;
    public string DriverName { get; init; } = string.Empty;
    public string PortName { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public string StatusText { get; init; } = string.Empty;
    public string DefaultPaper { get; init; } = string.Empty;
    public IReadOnlyList<string> PaperSizes { get; init; } = [];
}
