using ClosedXML.Excel;
using KLCMC.Pos.Core.Models;

namespace KLCMC.Pos.Core.Services;

public static class ExcelExportService
{
    /// <summary>Generates a daily report Excel file and returns the file path.</summary>
    public static string ExportDailyReport(DailySummary summary, string outputFolder)
    {
        var fileName = $"每日報告_{summary.Date:yyyy-MM-dd}.xlsx";
        var filePath = Path.Combine(outputFolder, fileName);

        using var wb = new XLWorkbook();

        BuildTransactionsSheet(wb, summary);
        BuildSummarySheet(wb, summary);

        wb.SaveAs(filePath);
        return filePath;
    }

    private static void BuildTransactionsSheet(XLWorkbook wb, DailySummary summary)
    {
        var ws = wb.Worksheets.Add("交易明細");

        // Title
        var titleCell = ws.Cell(1, 1);
        titleCell.Value = $"每日帳目 — {summary.Date:yyyy-MM-dd}";
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontSize = 14;
        ws.Range(1, 1, 1, 6).Merge();

        // Header row
        int headerRow = 3;
        string[] headers = ["編號", "時間", "產品", "數量", "付款方式", "金額 (HKD)"];
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4CAF50");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
        }

        // Data rows
        int row = headerRow + 1;
        foreach (var t in summary.Transactions)
        {
            var productsText = string.Join("、", t.Lines.Select(l => $"{l.Name} x{l.Quantity}"));
            var paymentsText = string.Join("、", t.Payments.Select(p => p.Method));

            ws.Cell(row, 1).Value = t.Id;
            ws.Cell(row, 2).Value = t.CreatedAtLocal.ToString("HH:mm:ss");
            ws.Cell(row, 3).Value = productsText;
            ws.Cell(row, 4).Value = t.Lines.Sum(l => l.Quantity);
            ws.Cell(row, 5).Value = paymentsText;

            var amtCell = ws.Cell(row, 6);
            amtCell.Value = (double)t.Total;
            amtCell.Style.NumberFormat.Format = "#,##0.00";
            amtCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            if (row % 2 == 0)
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");

            row++;
        }

        // Totals row
        if (summary.Transactions.Count > 0)
        {
            ws.Cell(row, 5).Value = "合計";
            ws.Cell(row, 5).Style.Font.Bold = true;
            ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            var totalCell = ws.Cell(row, 6);
            totalCell.Value = (double)summary.GrossTotal;
            totalCell.Style.NumberFormat.Format = "#,##0.00";
            totalCell.Style.Font.Bold = true;
            totalCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            totalCell.Style.Border.TopBorder = XLBorderStyleValues.Medium;
        }

        ws.Columns().AdjustToContents();
        ws.Column(3).Width = Math.Max(ws.Column(3).Width, 30);
    }

    private static void BuildSummarySheet(XLWorkbook wb, DailySummary summary)
    {
        var ws = wb.Worksheets.Add("付款匯總");

        var titleCell = ws.Cell(1, 1);
        titleCell.Value = $"付款方式匯總 — {summary.Date:yyyy-MM-dd}";
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontSize = 14;
        ws.Range(1, 1, 1, 3).Merge();

        int headerRow = 3;
        string[] headers = ["付款方式", "筆數", "金額 (HKD)"];
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4CAF50");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        int row = headerRow + 1;
        foreach (var m in summary.ByMethod)
        {
            ws.Cell(row, 1).Value = m.Method;
            ws.Cell(row, 2).Value = m.Count;
            var amtCell = ws.Cell(row, 3);
            amtCell.Value = (double)m.Amount;
            amtCell.Style.NumberFormat.Format = "#,##0.00";
            amtCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            row++;
        }

        // Grand total
        ws.Cell(row, 1).Value = "總計";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 2).Value = summary.TransactionCount;
        ws.Cell(row, 2).Style.Font.Bold = true;
        var grandCell = ws.Cell(row, 3);
        grandCell.Value = (double)summary.GrossTotal;
        grandCell.Style.NumberFormat.Format = "#,##0.00";
        grandCell.Style.Font.Bold = true;
        grandCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        grandCell.Style.Border.TopBorder = XLBorderStyleValues.Medium;

        ws.Columns().AdjustToContents();
    }
}
