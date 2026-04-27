namespace KLCMC.Pos.Core.Models;

public sealed class SaleSummary
{
    public required int Id { get; init; }
    public required DateTime CreatedAtLocal { get; init; }
    public required decimal Total { get; init; }
    public required IReadOnlyList<PaymentEntry> Payments { get; init; }
}
