using KLCMC.Pos.Core.Models;

namespace KLCMC.Pos.Core.ViewModels;

public sealed class CheckoutPaymentLine
{
    public required string Method { get; init; }
    public required decimal Amount { get; init; }
    public decimal? TenderedAmount { get; init; }
    public decimal? ChangeAmount { get; init; }

    public string DisplayText
    {
        get
        {
            if (Method == "現金" && TenderedAmount.HasValue)
            {
                var change = ChangeAmount ?? 0m;
                return change > 0m
                    ? $"現金 {Amount:F2}（實收 {TenderedAmount:F2}，找贖 {change:F2}）"
                    : $"現金 {Amount:F2}（實收 {TenderedAmount:F2}）";
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
