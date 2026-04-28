using KLCMC.Pos.Core.Data.Entities;
using KLCMC.Pos.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KLCMC.Pos.Core.Data.Repositories;

public sealed class SaleRepository : ISaleRepository
{
    private readonly IDbContextFactory<PosDbContext> _dbFactory;

    public SaleRepository(IDbContextFactory<PosDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public SaleEntity Record(IEnumerable<CartLine> cartLines, decimal total, IEnumerable<PaymentEntry> payments)
    {
        using var db = _dbFactory.CreateDbContext();
        var sale = new SaleEntity
        {
            CreatedAt = DateTime.UtcNow,
            Total = total,
            Lines = cartLines.Select(line => new SaleLineEntity
            {
                Name = line.Name,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                Remark = line.Remark,
                LineTotal = line.LineTotal
            }).ToList(),
            Payments = payments.Select(p => new SalePaymentEntity
            {
                Method = p.Method.ToString(),
                Amount = p.Amount,
                TenderedAmount = p.TenderedAmount,
                ChangeAmount = p.ChangeAmount
            }).ToList()
        };

        db.Sales.Add(sale);
        db.SaveChanges();
        return sale;
    }

    public IReadOnlyList<SaleSummary> GetForDate(DateOnly localDate)
    {
        using var db = _dbFactory.CreateDbContext();
        var localStart = localDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
        var localEnd = localStart.AddDays(1);
        var utcStart = localStart.ToUniversalTime();
        var utcEnd = localEnd.ToUniversalTime();

        var sales = db.Sales
            .AsNoTracking()
            .Where(s => s.CreatedAt >= utcStart && s.CreatedAt < utcEnd)
            .OrderBy(s => s.CreatedAt)
            .Select(s => new
            {
                s.Id,
                s.CreatedAt,
                s.Total,
                Payments = s.Payments.Select(p => new { p.Method, p.Amount, p.TenderedAmount, p.ChangeAmount }).ToList(),
                Lines = s.Lines.Select(l => new { l.Name, l.Quantity, l.LineTotal }).ToList()
            })
            .ToList();

        return sales.Select(s => new SaleSummary
        {
            Id = s.Id,
            CreatedAtLocal = s.CreatedAt.ToLocalTime(),
            Total = s.Total,
            Payments = s.Payments.Select(p => new PaymentEntry
            {
                Method = p.Method,
                Amount = p.Amount,
                TenderedAmount = p.TenderedAmount,
                ChangeAmount = p.ChangeAmount
            }).ToList(),
            Lines = s.Lines.Select(l => new SaleLineSummary
            {
                Name = l.Name,
                Quantity = l.Quantity,
                LineTotal = l.LineTotal
            }).ToList()
        }).ToList();
    }

    public DailySummary GetDailySummary(DateOnly localDate)
    {
        var transactions = GetForDate(localDate);
        if (transactions.Count == 0)
        {
            return DailySummary.Empty(localDate);
        }

        var byMethod = transactions
            .SelectMany(t => t.Payments)
            .GroupBy(p => p.Method)
            .Select(g => new MethodTotal
            {
                Method = g.Key,
                Count = g.Count(),
                Amount = g.Sum(x => x.Amount)
            })
            .OrderBy(m => m.Method)
            .ToList();

        return new DailySummary
        {
            Date = localDate,
            TransactionCount = transactions.Count,
            GrossTotal = transactions.Sum(t => t.Total),
            ByMethod = byMethod,
            Transactions = transactions
        };
    }

    private static string ParseMethod(string raw) => raw;
}
