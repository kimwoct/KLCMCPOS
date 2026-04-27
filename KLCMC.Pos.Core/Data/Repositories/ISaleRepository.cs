using KLCMC.Pos.Core.Data.Entities;
using KLCMC.Pos.Core.Models;

namespace KLCMC.Pos.Core.Data.Repositories;

public interface ISaleRepository
{
    SaleEntity Record(IEnumerable<CartLine> cartLines, decimal total, IEnumerable<PaymentEntry> payments);
    IReadOnlyList<SaleSummary> GetForDate(DateOnly localDate);
    DailySummary GetDailySummary(DateOnly localDate);
}
