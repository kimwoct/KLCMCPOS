namespace KLCMC.Pos.Core.Models;

public sealed class MethodTotal
{
    public required string Method { get; init; }
    public required int Count { get; init; }
    public required decimal Amount { get; init; }
}

public sealed class DailySummary
{
    public required DateOnly Date { get; init; }
    public required int TransactionCount { get; init; }
    public required decimal GrossTotal { get; init; }
    public required IReadOnlyList<MethodTotal> ByMethod { get; init; }
    public required IReadOnlyList<SaleSummary> Transactions { get; init; }

    public static DailySummary Empty(DateOnly date) => new()
    {
        Date = date,
        TransactionCount = 0,
        GrossTotal = 0m,
        ByMethod = Array.Empty<MethodTotal>(),
        Transactions = Array.Empty<SaleSummary>()
    };
}
