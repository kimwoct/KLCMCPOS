using KLCMC.Pos.Core.Models;

namespace KLCMC.Pos.Core.Services;

public static class ReceiptComposer
{
    public static IReadOnlyList<ReceiptLine> Build(
        string title,
        IEnumerable<CartLine> lines,
        decimal total,
        DateTime timestamp,
        IEnumerable<PaymentEntry>? payments = null,
        PrinterConnectionOptions? printerOptions = null)
    {
        var width = printerOptions?.PaperWidthMm == 58 ? 32 : 48;
        var separator = new string('-', width);
        var receiptLines = new List<ReceiptLine>
        {
            new() { Text = title },
            new() { Text = timestamp.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { Text = separator }
        };

        foreach (var line in lines)
        {
            receiptLines.Add(new ReceiptLine
            {
                Text = $"{line.Name} x{line.Quantity} @ {line.UnitPrice:F2} = {line.LineTotal:F2}"
            });
        }

        receiptLines.Add(new ReceiptLine { Text = separator });
        receiptLines.Add(new ReceiptLine { Text = $"TOTAL: HKD ${total:F2}" });

        if (payments is not null)
        {
            var paymentList = payments.ToList();
            if (paymentList.Count > 0)
            {
                receiptLines.Add(new ReceiptLine { Text = "Payments:" });
                decimal totalChange = 0m;
                foreach (var payment in paymentList)
                {
                    if (payment.Method == "現金" && payment.TenderedAmount.HasValue)
                    {
                        receiptLines.Add(new ReceiptLine
                        {
                            Text = $"  {payment.Method} {payment.Amount:F2} (tendered {payment.TenderedAmount:F2})"
                        });
                    }
                    else
                    {
                        receiptLines.Add(new ReceiptLine
                        {
                            Text = $"  {payment.Method} {payment.Amount:F2}"
                        });
                    }

                    if (payment.ChangeAmount.HasValue)
                    {
                        totalChange += payment.ChangeAmount.Value;
                    }
                }

                if (totalChange > 0m)
                {
                    receiptLines.Add(new ReceiptLine { Text = $"CHANGE: HKD ${totalChange:F2}" });
                }
            }
        }

        receiptLines.Add(new ReceiptLine { Text = "Thank you!" });
        return receiptLines;
    }

    public static IReadOnlyList<ReceiptLine> BuildDailyReport(string title, DailySummary summary)
    {
        var lines = new List<ReceiptLine>
        {
            new() { Text = title },
            new() { Text = "DAILY REPORT" },
            new() { Text = summary.Date.ToString("yyyy-MM-dd") },
            new() { Text = "-------------------------------" },
            new() { Text = $"Transactions: {summary.TransactionCount}" },
            new() { Text = $"Gross Total : HKD ${summary.GrossTotal:F2}" },
            new() { Text = "By Method:" }
        };

        if (summary.ByMethod.Count == 0)
        {
            lines.Add(new ReceiptLine { Text = "  (none)" });
        }
        else
        {
            foreach (var m in summary.ByMethod)
            {
                lines.Add(new ReceiptLine
                {
                    Text = $"  {m.Method,-8} x{m.Count,-3} = {m.Amount:F2}"
                });
            }
        }

        lines.Add(new ReceiptLine { Text = "-------------------------------" });
        lines.Add(new ReceiptLine { Text = $"Generated {DateTime.Now:yyyy-MM-dd HH:mm:ss}" });
        return lines;
    }
}
