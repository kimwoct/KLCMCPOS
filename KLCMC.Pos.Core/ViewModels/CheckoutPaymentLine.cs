using KLCMC.Pos.Core.Models;

namespace KLCMC.Pos.Core.ViewModels;

public sealed class CheckoutPaymentLine
{
    public required PaymentMethod Method { get; init; }
    public required decimal Amount { get; init; }
    public decimal? TenderedAmount { get; init; }
    public decimal? ChangeAmount { get; init; }

    public string DisplayText
    {
        get
        {
            if (Method == PaymentMethod.Cash && TenderedAmount.HasValue)
            {
                var change = ChangeAmount ?? 0m;
                return change > 0m
                    ? $"Cash {Amount:F2} (tendered {TenderedAmount:F2}, change {change:F2})"
                    : $"Cash {Amount:F2} (tendered {TenderedAmount:F2})";
            }

            return $"{Method} {Amount:F2}";
        }
    }

    public PaymentEntry ToEntry() => new()
    {
        Method = Method,
        Amount = Amount,
        TenderedAmount = TenderedAmount,
        ChangeAmount = ChangeAmount
    };
}
