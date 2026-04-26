using KLCMC.Pos.Core.Models;

namespace KLCMC.Pos.Core.Services;

public static class ReceiptComposer
{
    public static IReadOnlyList<ReceiptLine> Build(string title, IEnumerable<CartLine> lines, decimal total, DateTime timestamp)
    {
        var receiptLines = new List<ReceiptLine>
        {
            new() { Text = title },
            new() { Text = timestamp.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { Text = "-------------------------------" }
        };

        foreach (var line in lines)
        {
            receiptLines.Add(new ReceiptLine
            {
                Text = $"{line.Name} x{line.Quantity} @ {line.UnitPrice:F2} = {line.LineTotal:F2}"
            });
        }

        receiptLines.Add(new ReceiptLine { Text = "-------------------------------" });
        receiptLines.Add(new ReceiptLine { Text = $"TOTAL: HKD ${total:F2}" });
        receiptLines.Add(new ReceiptLine { Text = "Thank you!" });
        return receiptLines;
    }
}
