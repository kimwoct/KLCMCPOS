namespace KLCMC.Pos.Core.Models;

public sealed class PaymentEntry
{
    public required PaymentMethod Method { get; init; }
    public required decimal Amount { get; init; }
    public decimal? TenderedAmount { get; init; }
    public decimal? ChangeAmount { get; init; }
}
