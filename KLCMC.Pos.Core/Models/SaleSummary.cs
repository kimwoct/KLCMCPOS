namespace KLCMC.Pos.Core.Models;

public sealed class SaleLineSummary
{
    public required string Name { get; init; }
    public required int Quantity { get; init; }
    public required decimal LineTotal { get; init; }
}

public sealed class SaleSummary
{
    public required int Id { get; init; }
    public required DateTime CreatedAtLocal { get; init; }
    public required decimal Total { get; init; }
    public required IReadOnlyList<PaymentEntry> Payments { get; init; }
    public required IReadOnlyList<SaleLineSummary> Lines { get; init; }
}
