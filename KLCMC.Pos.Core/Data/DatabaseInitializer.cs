using KLCMC.Pos.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KLCMC.Pos.Core.Data;

public static class DatabaseInitializer
{
    private static readonly (string Name, decimal Price)[] DefaultProducts =
    {
        ("Americano", 6.50m),
        ("Latte", 8.90m),
        ("Cappuccino", 8.90m),
        ("Sandwich", 12.00m),
        ("Muffin", 5.00m),
        ("Mineral Water", 2.00m)
    };

    public static void EnsureReady(PosDbContext db)
    {
        db.Database.EnsureCreated();
        EnsurePrinterSettingsSchema(db);

        if (!db.Products.Any())
        {
            db.Products.AddRange(DefaultProducts.Select(p => new ProductEntity
            {
                Name = p.Name,
                DefaultPrice = p.Price
            }));
            db.SaveChanges();
        }

        if (!db.PrinterSettings.Any())
        {
            db.PrinterSettings.Add(new PrinterSettingEntity
            {
                Id = 1,
                Mode = KLCMC.Pos.Core.Models.PrinterConnectionMode.Usb,
                Endpoint = "POS-80",
                BaudRate = 9600,
                DataBits = 8,
                StopBits = 0,
                Parity = 0,
                FlowControl = 1,
                PaperWidthMm = 80,
                CodePage = "UTF-8",
                CutMode = KLCMC.Pos.Core.Models.PrinterCutMode.Partial,
                DrawerPulseOnMs = 120,
                DrawerPulseOffMs = 240
            });
            db.SaveChanges();
        }
    }

    private static void EnsurePrinterSettingsSchema(PosDbContext db)
    {
        using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = "PRAGMA table_info('PrinterSettings');";
        db.Database.OpenConnection();
        using var reader = command.ExecuteReader();
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (reader.Read())
        {
            columns.Add(reader.GetString(1));
        }

        AddColumnIfMissing(db, columns, "PaperWidthMm");
        AddColumnIfMissing(db, columns, "CodePage");
        AddColumnIfMissing(db, columns, "CutMode");
        AddColumnIfMissing(db, columns, "DrawerPulseOnMs");
        AddColumnIfMissing(db, columns, "DrawerPulseOffMs");
        db.Database.CloseConnection();
    }

    private static void AddColumnIfMissing(PosDbContext db, ISet<string> columns, string columnName)
    {
        if (columns.Contains(columnName))
        {
            return;
        }

        var sql = columnName switch
        {
            "PaperWidthMm" => "ALTER TABLE PrinterSettings ADD COLUMN PaperWidthMm INTEGER NOT NULL DEFAULT 80;",
            "CodePage" => "ALTER TABLE PrinterSettings ADD COLUMN CodePage TEXT NOT NULL DEFAULT 'UTF-8';",
            "CutMode" => "ALTER TABLE PrinterSettings ADD COLUMN CutMode INTEGER NOT NULL DEFAULT 1;",
            "DrawerPulseOnMs" => "ALTER TABLE PrinterSettings ADD COLUMN DrawerPulseOnMs INTEGER NOT NULL DEFAULT 120;",
            "DrawerPulseOffMs" => "ALTER TABLE PrinterSettings ADD COLUMN DrawerPulseOffMs INTEGER NOT NULL DEFAULT 240;",
            _ => throw new InvalidOperationException($"Unsupported column '{columnName}'.")
        };

        db.Database.ExecuteSqlRaw(sql);
        columns.Add(columnName);
    }
}
